using System;
using System.Diagnostics;
using Common.Logging;
using Common.Logging.Simple;
using ScriptBot.Core;
using ScriptBot.Core.Contracts;
using Topshelf;

namespace ScriptBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new TraceLogger(true, "ScriptBot logging", LogLevel.All, true, true, true, @"yyyy-MM-dd hh:mm:ss");

            ITransport transport = new Skype.SkypeTransport("@bot", logger);
            HostFactory.Run(configurator => configurator.Service<Bot>(b =>
                {
                    b.ConstructUsing(t => new Bot(transport, logger));
                    b.WhenStarted(t=> t.Initialize(typeof(Plugins.ScriptCs.ScriptBotScriptCsCommand)));
                    b.WhenStopped(t => t.Dispose());
                }));
            if (Debugger.IsAttached)
                Console.ReadLine();
        }
    }
}
