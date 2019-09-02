using CommandDotNet;
using Jira2AzureDevOps.Framework.CommandDotNet;
using Jira2AzureDevOps.Framework.NLog;
using NLog;
using System;
using System.Threading.Tasks;

namespace Jira2AzureDevOps
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            DemystifyExceptionLayoutRenderer.Register();
            
            Console.CancelKeyPress += Console_CancelKeyPress;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            new AppRunner<App>(new AppSettings
            {
                EnableDirectives = true,
                Case = Case.KebabCase,
            })
                .UseDebugDirective()
                .UseParseDirective()
                .UseResponseFiles()
                .UseSelfValidatingArgumentModels()
                .CommandsCanDisableConsoleLogging()
                .UseBeginCommandHeaders(
                    Logger.Info,
                    skipCommand: ctx => ctx.ParseResult.TargetCommand.CustomAttributes.IsDefined(typeof(NoHeadersAttribute), false),
                    includeToolVersion: true,
                    includeDotNetVersion: true,
                    includeOsVersion: true,
                    includeMachine: true,
                    includeUsername: true)
                .Run(args);
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.Debug("Exiting program");
            Cancellation.Shutdown();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;

            if (ex is TaskCanceledException && Cancellation.IsRequested)
            {
                // already handled by Console_CancelKeyPress
                return;
            }

            if (e.IsTerminating)
            {
                Logger.Fatal(ex, "unhandled exception. stopping program.");
                Cancellation.Shutdown();
            }
            else
            {
                Logger.Error(ex, "unhandled exception");
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Logger.Info("Shutting down program by user request");
            Cancellation.Shutdown();
            e.Cancel = true;
        }
    }
}
