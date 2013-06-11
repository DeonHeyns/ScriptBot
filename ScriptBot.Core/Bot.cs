﻿//    Copyright 2013 Deon Heyns
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.IO;
using Common.Logging;
using ScriptBot.Core.Contracts;
using ScriptBot.Core.Messages;

namespace ScriptBot.Core
{
    public class Bot: IDisposable
    {
        private readonly ILog _logger;
        private Type[] _assemblies;
        private bool _disposed;
        private AggregateCatalog _catalog;
        private CompositionContainer _container;
        private readonly string _trigger;

        [ImportMany]
        #pragma warning disable 649
        IEnumerable<Lazy<IScriptBotCommand, IScriptBotCommandData>> _plugins;
        #pragma warning restore 649

        public Bot(ITransport transport, ILog logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
            _trigger = transport.Trigger;
            _logger.Info(string.Format("Set Trigger for Bot to: {0}", _trigger));
            transport.Listen(message => new ReplyMessage {Message = HandleCommand(message.Message)});
        }

        public void Initialize(params Type[] assemblies)
        {
            _logger.Info(string.Format("Initializing Bot with the following assemblies: {0}",
                                        string.Join(", ", assemblies.AsEnumerable())));
            _assemblies = assemblies;
            LoadPlugins();
        }

        public string HandleCommand(string command)
        {
            _logger.Info(string.Format("Handling command text: {0}", command));
            return ProcessCommand(command);
        }

        private string ProcessCommand(string commandString)
        {
            var command = commandString.Replace(_trigger, string.Empty);
            var processResults = new StringBuilder();

            var raw = command.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            if (raw.Length != 0)
            {
                command = raw[0];
                var args = raw.Skip(1).ToArray();

                var scriptBotCommands =
                    _plugins.Where(s => s.Metadata.Command.Equals(command, StringComparison.OrdinalIgnoreCase)).ToList();

                _logger.Info(string.Format("Plugins loaded to handle: {0} are {1}", commandString, string.Join(", ", scriptBotCommands)));

                foreach (var plugin in scriptBotCommands)
                {
                    processResults.AppendLine(plugin.Value.Process(args));
                }
            }
            if (processResults.Length == 0)
            {
                _logger.Info(string.Format("No plugins were found to handle: {0}", commandString));
                processResults.AppendLine("couldn't find any related commands!");
            }
            return processResults.ToString();
        }

        private void LoadPlugins()
        {
            _catalog = new AggregateCatalog();
            try
            {
                _catalog.Catalogs.Add(new DirectoryCatalog(Environment.CurrentDirectory + @"\plugins\"));
            }
            catch (DirectoryNotFoundException)
            {
                _logger.Warn(string.Format("Directory for plugins {0} does not exist", Environment.CurrentDirectory + @"\plugins\"));
            }
            
            foreach (var assembly in _assemblies)
            {
                _catalog.Catalogs.Add(new AssemblyCatalog(assembly.Assembly));
            }

            _container = new CompositionContainer(_catalog);

            try
            {
                _container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                _logger.Error(compositionException);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _catalog.Dispose();
                _container.Dispose();
            }

            _disposed = true;
        }
    }
}
