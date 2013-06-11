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
using System.ComponentModel.Composition;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SkypeBot.Common.Commands
{
    [SkypeBotExport]
    [ExportMetadata("Command", CommandParameters.ScriptCs)]
    public class SkypeBotScriptCsCommand : ISkypeBotCommand
    {
        private readonly string _scriptcs;
        private readonly string _csxFolder;

        public SkypeBotScriptCsCommand()
        {
            _scriptcs = LocateScriptCsExe();
            _csxFolder = Path.Combine(Environment.CurrentDirectory, ConfigurationManager.AppSettings["SkypeBotScriptcsCommand:ScriptCsScriptsPath"]);
        }

        public string Process(params string[] args)
        {   
            if (string.IsNullOrWhiteSpace(_scriptcs))
                return "Sorry couldn't find the scriptcs.exe";

            var file = args[0].EndsWith(".csx") ? args[0] : args[0] + ".csx";
            var scriptcsFile = Path.Combine(_csxFolder, file);

            if(!File.Exists(scriptcsFile))
                return string.Format("Sorry couldn't find the scriptcs file {0}.", file);

            var arguments = string.Format("{0} {1}", scriptcsFile, string.Join(" ", args.Skip(1)));

            var results = new StringBuilder();
            var proc = new Process
                {
                    StartInfo =
                        new ProcessStartInfo(_scriptcs)
                            {
                                Arguments = arguments,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true
                            }
                };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                results.AppendLine(proc.StandardOutput.ReadLine());
            }

            return results.ToString();
        }

        private static string LocateScriptCsExe()
        {
            const string filename = "scriptcs.exe";
            var path = Environment.GetEnvironmentVariable("path");
            if (path != null)
            {
                var folders = path.Split(';');
                var scriptcsFilePath = folders.Select(folder => Path.Combine(folder, filename)).FirstOrDefault(File.Exists);
                if(!string.IsNullOrWhiteSpace(scriptcsFilePath))
                    return scriptcsFilePath;
            }

            return ConfigurationManager.AppSettings["SkypeBotScriptcsCommand:ScriptCsExePath"] ?? string.Empty;
        }
    }
}