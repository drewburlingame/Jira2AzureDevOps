using CommandDotNet;
using Jira2AzureDevOps.Console;
using Jira2AzureDevOps.Console.Framework;
using Jira2AzureDevOps.Logic.Framework.NLog;
using NLog;
using System;
using System.Threading.Tasks;
using CommandDotNet.Execution;

namespace Jira2AzureDevOps
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "generate-docs")
            {
                new AppRunner<App>(new AppSettings
                    {
                        Case = Case.KebabCase
                    })
                    .Configure(c => c.CustomHelpProvider = new AllCommandsMarkDownHelpProvider(true, true))
                    .Run("-h");
                return;
            }


            DemystifyExceptionLayoutRenderer.Register();


            new AppRunner<App>(new AppSettings
            {
                EnableDirectives = true,
                Case = Case.KebabCase,
            })
                .UseCancellationHandler()
                .UseDebugDirective()
                .UseParseDirective()
                .UseResponseFiles()
                .UseAppSettingsForOptionDefaults()
                .UseSelfValidatingArgumentModels()
                .CommandsCanDisableConsoleLogging()
                .UseReproHeaders(
                    Logger.Info,
                    skipCommand: ctx => ctx.ParseResult.TargetCommand.CustomAttributes.IsDefined(typeof(NoReproHeadersAttribute), false),
                    includeToolVersion: true,
                    includeDotNetVersion: true,
                    includeOsVersion: true,
                    includeMachine: true,
                    includeUsername: true)
                .Run(args);
        }
    }
}
