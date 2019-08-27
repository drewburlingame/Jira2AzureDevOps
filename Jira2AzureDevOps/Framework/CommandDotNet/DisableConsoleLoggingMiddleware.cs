using System;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Execution;
using NLog;

namespace Jira2AzureDevOps.Framework.CommandDotNet
{
    public static class DisableConsoleLoggingMiddleware
    {
        public static AppRunner CommandsCanDisableConsoleLogging(this AppRunner appRunner)
        {
            return appRunner.Configure(c => c.UseMiddleware(DisableConsoleLogging, MiddlewareStages.PostBindValuesPreInvoke));
        }

        public static Task<int> DisableConsoleLogging(CommandContext context, Func<CommandContext, Task<int>> next)
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