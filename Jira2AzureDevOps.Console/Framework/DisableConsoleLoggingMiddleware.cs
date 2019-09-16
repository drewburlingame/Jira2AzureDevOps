using CommandDotNet;
using CommandDotNet.Execution;
using NLog;
using System;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Console.Framework
{
    public static class DisableConsoleLoggingMiddleware
    {
        public static AppRunner CommandsCanDisableConsoleLogging(this AppRunner appRunner)
        {
            return appRunner.Configure(c => c.UseMiddleware(DisableConsoleLogging, MiddlewareStages.PostBindValuesPreInvoke));
        }

        private static Task<int> DisableConsoleLogging(CommandContext context, ExecutionDelegate next)
        {
            if (context.ParseResult.TargetCommand.CustomAttributes.IsDefined(typeof(DisableConsoleLoggingAttribute),
                false))
            {
                LogManager.Configuration.RemoveTarget("logconsole");
                LogManager.ReconfigExistingLoggers();
            }

            return next(context);
        }

    }
}