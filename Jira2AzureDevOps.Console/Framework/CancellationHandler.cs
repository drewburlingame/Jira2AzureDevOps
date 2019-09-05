using System;
using System.Threading;
using System.Threading.Tasks;
using CommandDotNet;
using NLog;

namespace Jira2AzureDevOps.Console.Framework
{
    public static class CancellationHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly CancellationTokenSource TokenSource = new CancellationTokenSource();
        
        public static AppRunner UseCancellationHandler(this AppRunner appRunner)
        {
            System.Console.CancelKeyPress += Console_CancelKeyPress;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            return appRunner.Configure(c =>
            {
                c.CancellationToken = TokenSource.Token;
            });
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.Debug("Exiting program");
            Shutdown();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;

            if (ex is TaskCanceledException && TokenSource.IsCancellationRequested)
            {
                // already handled by Console_CancelKeyPress
                return;
            }

            if (e.IsTerminating)
            {
                Logger.Fatal(ex, "unhandled exception. stopping program.");
                Shutdown();
            }
            else
            {
                Logger.Error(ex, "unhandled exception");
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Logger.Info("Shutting down program by user request");
            Shutdown();
            e.Cancel = true;
        }

        private static void Shutdown()
        {
            if (!TokenSource.IsCancellationRequested)
                TokenSource.Cancel();
        }
    }
}