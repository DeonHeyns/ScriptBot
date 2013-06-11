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
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using CL = Common.Logging;
using CLS = Common.Logging.Simple;
using NuGet;
using ScriptCs;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Package;
using ScriptCs.Package.InstallationProvider;

namespace SkypeBot.Common.Commands
{
    [SkypeBotExport]
    [ExportMetadata("Command", CommandParameters.ScriptCs)]
    public class SkypeBotScriptCsCommand : ISkypeBotCommand
    {
        private readonly string _csxFolder;
        private readonly ContainerBuilder _builder;
        private readonly IContainer _container;

        public SkypeBotScriptCsCommand()
        {
            _csxFolder = Path.Combine(Environment.CurrentDirectory,
                                      ConfigurationManager.AppSettings["SkypeBotScriptcsCommand:ScriptCsScriptsPath"]);

            _builder = new ContainerBuilder();
            _builder.RegisterModule(new ScriptModule());
            _container = _builder.Build();
        }

        public string Process(params string[] args)
        {
            var file = args[0].EndsWith(".csx") ? args[0] : args[0] + ".csx";
            var scriptcsFile = Path.Combine(_csxFolder, file);

            if (!File.Exists(scriptcsFile))
                return string.Format("Sorry couldn't find the scriptcs file {0}.", file);


            var results = new StringBuilder();

            using (var scope = _container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<CL.ILog>();
                var executeScriptCs = scope.Resolve<ExecuteScriptCs>();

                try
                {
                    executeScriptCs.Run(scriptcsFile, string.Join(" ", args.Skip(1)));
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    throw;
                }
            }

            return results.ToString();
        }
    }

    internal class ExecuteScriptCs
    {
        // dependencies
        private readonly CL.ILog _logger;
        private readonly ScriptCs.IFileSystem _fileSystem;
        private readonly IPackageAssemblyResolver _packageAssemblyResolver;
        private readonly IPackageInstaller _packageInstaller;
        private readonly IScriptPackResolver _scriptPackResolver;
        private readonly IScriptExecutor _scriptExecutor;

        public ExecuteScriptCs(CL.ILog logger, ScriptCs.IFileSystem fileSystem,
                                IPackageAssemblyResolver packageAssemblyResolver, IPackageInstaller packageInstaller,
                                IScriptPackResolver scriptPackResolver, IScriptExecutor scriptExecutor)
        {
            this._logger = logger;
            this._fileSystem = fileSystem;
            this._packageAssemblyResolver = packageAssemblyResolver;
            this._packageInstaller = packageInstaller;
            this._scriptPackResolver = scriptPackResolver;
            this._scriptExecutor = scriptExecutor;
        }

        public void Run(string scriptPath, params string[] args)
        {
            // preserve current directory
            var previousCurrentDirectory = Environment.CurrentDirectory;

            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(scriptPath);

                // prepare NuGet dependencies, download them if required
//                var nuGetReferences = PreparePackages(
//                                                scriptPath,
//                                                _fileSystem, _packageAssemblyResolver,
//                                                _packageInstaller, _logger.Info);

                // get script packs: not fully tested yet        
                var scriptPacks = _scriptPackResolver.GetPacks();

                _scriptExecutor.Initialize(new[] { scriptPath }, scriptPacks);
                // execute script from file
                var result = _scriptExecutor.Execute(scriptPath, args);
                
            }
            finally
            {
                // restore current directory
                Environment.CurrentDirectory = previousCurrentDirectory;
            }
        }

        private static IEnumerable<string> PreparePackages(
                                string scriptPath,
                                ScriptCs.IFileSystem fileSystem, IPackageAssemblyResolver packageAssemblyResolver,
                                IPackageInstaller packageInstaller, Action<string> outputCallback = null)
        {
            var workingDirectory = Path.GetDirectoryName(scriptPath);
            var binDirectory = Path.Combine(workingDirectory, ScriptCs.Constants.BinFolder);

            var packages = packageAssemblyResolver.GetPackages(workingDirectory);

            packageInstaller.InstallPackages(
                                packages,
                                allowPreRelease: true, packageInstalled: outputCallback);

            // current implementeation of RoslynCTP required dependencies to be in 'bin' folder
            if (!fileSystem.DirectoryExists(binDirectory))
            {
                fileSystem.CreateDirectory(binDirectory);
            }

            // copy dependencies one by one from 'packages' to 'bin'
            foreach (var assemblyName
                        in packageAssemblyResolver.GetAssemblyNames(workingDirectory, outputCallback))
            {
                var assemblyFileName = Path.GetFileName(assemblyName);
                var destFile = Path.Combine(binDirectory, assemblyFileName);

                var sourceFileLastWriteTime = fileSystem.GetLastWriteTime(assemblyName);
                var destFileLastWriteTime = fileSystem.GetLastWriteTime(destFile);

                if (sourceFileLastWriteTime == destFileLastWriteTime)
                {
                    outputCallback(string.Format("Skipped: '{0}' because it is already exists", assemblyName));
                }
                else
                {
                    fileSystem.Copy(assemblyName, destFile, overwrite: true);

                    if (outputCallback != null)
                    {
                        outputCallback(string.Format("Copy: '{0}' to '{1}'", assemblyName, destFile));
                    }
                }

                yield return destFile;
            }
        }
    }

    internal class ScriptModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ScriptCs.FileSystem>()
                .As<ScriptCs.IFileSystem>()
                .SingleInstance();

            builder
                .RegisterType<CLS.ConsoleOutLogger>()
                .As<CL.ILog>()
                .SingleInstance()
                .WithParameter("logName", @"Custom ScriptCs from C#")
                .WithParameter("logLevel", CL.LogLevel.All)
                .WithParameter("showLevel", true)
                .WithParameter("showDateTime", true)
                .WithParameter("showLogName", true)
                .WithParameter("dateTimeFormat", @"yyyy-mm-dd hh:mm:ss");

            builder
                .RegisterType<FilePreProcessor>()
                .As<IFilePreProcessor>()
                .SingleInstance();

            builder
                .RegisterType<ScriptHostFactory>()
                .As<IScriptHostFactory>()
                .SingleInstance();

            builder
                .RegisterType<RoslynScriptEngine>()
                .As<IScriptEngine>();

            builder
                .RegisterType<ScriptExecutor>()
                .As<IScriptExecutor>();

            builder
                .RegisterType<NugetInstallationProvider>()
                .As<IInstallationProvider>()
                .SingleInstance();

            builder
                .RegisterType<PackageAssemblyResolver>()
                .As<IPackageAssemblyResolver>()
                .SingleInstance();

            builder
                .RegisterType<PackageContainer>()
                .As<IPackageContainer>()
                .SingleInstance();

            builder
                .RegisterType<PackageInstaller>()
                .As<IPackageInstaller>()
                .SingleInstance();

            builder
                .RegisterType<PackageManager>()
                .As<IPackageManager>()
                .SingleInstance();

            builder
                .RegisterType<ScriptPackResolver>()
                .As<IScriptPackResolver>()
                .SingleInstance();

            builder
                .RegisterType<ExecuteScriptCs>();
        }
    }
}