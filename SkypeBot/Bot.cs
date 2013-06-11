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
using SKYPE4COMLib;
using SkypeBot.Common;
using System.IO;
using SkypeBot.Common.Commands;

namespace SkypeBot
{
    public class Bot: IDisposable
    {
        private bool _disposed;
        private Skype _skype;
        private const string Trigger = "@bot";
        private AggregateCatalog _catalog;
        private CompositionContainer _container;

        [ImportMany]
#pragma warning disable 649
        IEnumerable<Lazy<ISkypeBotCommand, ISkypeBotCommandData>> _plugins;
#pragma warning restore 649

        public Bot()
        {
            LoadPlugins();
        }

        public void Listen()
        {
            _skype = new Skype();
            _skype.Attach(7, false);
            _skype.MessageStatus += (message, status) =>
            {
                if (message.Body.IndexOf(Trigger, StringComparison.Ordinal) == 0)//&& status == TChatMessageStatus.cmsReceived)
                {
                    IChat ichat = _skype.Chat[message.Chat.Name];
                    var msg = message.Body.ToLowerInvariant();
                    ichat.SendMessage(string.Format("Bot finished:{0} {1}", Environment.NewLine, HandleCommand(msg)));
                }
            };
        }

        public string HandleCommand(string command)
        {
            return ProcessCommand(command);
        }

        private string ProcessCommand(string commandString)
        {
            var command = commandString.Replace(Trigger, string.Empty);
            var processResults = new StringBuilder();

            var raw = command.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            if (raw.Length != 0)
            {
                command = raw[0];
                var args = raw.Skip(1).ToArray();

                var skypeBotCommands =
                    _plugins.Where(s => s.Metadata.Command.Equals(command, StringComparison.OrdinalIgnoreCase));
                
                foreach (var plugin in skypeBotCommands)
                {
                    processResults.AppendLine(plugin.Value.Process(args));
                }
            }
            if (processResults.Length == 0)
            {
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
            catch (DirectoryNotFoundException) { }
            
            _catalog.Catalogs.Add(new AssemblyCatalog(typeof(SkypeBotEmptyCommand).Assembly));
            _container = new CompositionContainer(_catalog);

            try
            {
                _container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
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

            _skype = null;
            _disposed = true;
        }
    }
}
