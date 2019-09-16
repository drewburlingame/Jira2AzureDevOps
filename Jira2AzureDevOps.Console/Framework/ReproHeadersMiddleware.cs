using CommandDotNet;
using CommandDotNet.Builders;
using CommandDotNet.Execution;
using CommandDotNet.Parsing;
using Jira2AzureDevOps.Logic.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Console.Framework
{
    public static class ReproHeadersMiddleware
    {
        public static AppRunner UseReproHeaders(this AppRunner appRunner,
            Action<string> writer = null,
            Predicate<CommandContext> skipCommand = null,
            Predicate<IArgument> skipArgument = null,
            Predicate<IArgument> obscureArgument = null,
            bool includeToolVersion = false,
            bool includeDotNetVersion = false,
            bool includeOsVersion = false,
            bool includeMachine = false,
            bool includeUsername = false,
            Func<CommandContext, IEnumerable<(string name, string text)>> additionalHeadersCallback = null)
        {
            return appRunner.Configure(c =>
            {
                c.Services.Add(new LogHeaderConfig(
                    writer, skipCommand, skipArgument, obscureArgument,
                    includeToolVersion, includeDotNetVersion, includeOsVersion,
                    includeMachine, includeUsername,
                    additionalHeadersCallback));
                c.UseMiddleware(LogHeader, MiddlewareStages.PostBindValuesPreInvoke);
            });
        }

        private class LogHeaderConfig
        {
            public Action<string> Writer { get; }
            public Predicate<CommandContext> SkipCommand { get; }
            public Predicate<IArgument> SkipArgument { get; }
            public Predicate<IArgument> ObscureArgument { get; }
            public bool IncludeToolVersion { get; }
            public bool IncludeDotNetVersion { get; }
            public bool IncludeOsVersion { get; }
            public bool IncludeMachine { get; }
            public bool IncludeUsername { get; }
            public Func<CommandContext, IEnumerable<(string name, string text)>> AdditionalHeadersCallback { get; }

            public LogHeaderConfig(Action<string> writer,
                Predicate<CommandContext> skipCommand,
                Predicate<IArgument> skipArgument,
                Predicate<IArgument> obscureArgument,
                bool includeToolVersion,
                bool includeDotNetVersion,
                bool includeOsVersion,
                bool includeMachine,
                bool includeUsername,
                Func<CommandContext, IEnumerable<(string name, string text)>> additionalHeadersCallback)
            {
                Writer = writer;
                SkipCommand = skipCommand;
                SkipArgument = skipArgument;
                ObscureArgument = obscureArgument;
                IncludeToolVersion = includeToolVersion;
                IncludeDotNetVersion = includeDotNetVersion;
                IncludeOsVersion = includeOsVersion;
                IncludeMachine = includeMachine;
                IncludeUsername = includeUsername;
                AdditionalHeadersCallback = additionalHeadersCallback;
            }
        }

        private static Task<int> LogHeader(CommandContext commandContext, ExecutionDelegate next)
        {
            var config = commandContext.AppConfig.Services.Get<LogHeaderConfig>();
            if (config.SkipCommand?.Invoke(commandContext) ?? false)
            {
                return next(commandContext);
            }

            PrintHeader(commandContext, config);

            return next(commandContext);
        }

        private static void PrintHeader(CommandContext commandContext, LogHeaderConfig config)
        {
            var parseResult = commandContext.ParseResult;
            var targetCommand = parseResult.TargetCommand;

            var sb = new StringBuilder(Environment.NewLine);

            sb.AppendLine("***************************************");
            sb.AppendLine($" Command: {targetCommand.GetParentCommands(true).Reverse().Skip(1).Select(c => c.Name).ToCsv(" ")}");
            sb.LogArguments("Arguments", targetCommand.Operands, config, parseResult);
            sb.LogArguments("Options", IncludeInherited(targetCommand, c => c.Options).Where(o => !o.IsMiddlewareOption), config, parseResult);

            sb.AppendLine();
            sb.AppendLine(" Original input:");
            sb.AppendLine($"   {commandContext.Original.Args.ToCsv(" ")}");

            var otherConfigEntries = GetOtherConfigInfo(commandContext, config).ToList();
            if (!otherConfigEntries.IsNullOrEmpty())
            {
                sb.AppendLine();
                var maxName = otherConfigEntries.Max(e => e.name.Length);
                foreach (var entry in otherConfigEntries)
                {
                    sb.AppendFormat($"  {{0, -{maxName + 1}}} = {{1}}", entry.name, entry.text);
                    sb.AppendLine();
                }
            }

            sb.AppendLine("***************************************");

            var writer = config.Writer;
            if (writer == null)
                commandContext.Console.Out.WriteLine(sb.ToString());
            else
                writer(sb.ToString());
        }

        private static IEnumerable<(string name, string text)> GetOtherConfigInfo(CommandContext commandContext, LogHeaderConfig config)
        {
            if (config.IncludeToolVersion)
            {
                var versionInfo = VersionInfo.GetVersionInfo(commandContext);
                yield return ("Tool version", $"{versionInfo.Filename} {versionInfo.Version}");
            }

            if (config.IncludeDotNetVersion)
            {
                yield return (".Net version", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Trim());
            }

            if (config.IncludeOsVersion)
            {
                yield return ("OS version", System.Runtime.InteropServices.RuntimeInformation.OSDescription.Trim());
            }

            if (config.IncludeMachine)
            {
                yield return ("Machine", Environment.MachineName);
            }

            if (config.IncludeUsername)
            {
                yield return ("Username", $"{Environment.UserDomainName}\\{Environment.UserName}");
            }

            if (config.AdditionalHeadersCallback != null)
            {
                foreach (var header in config.AdditionalHeadersCallback(commandContext))
                {
                    yield return header;
                }
            }
        }

        private static IEnumerable<T> IncludeInherited<T>(Command c, Func<Command, IEnumerable<T>> getArgs) where T : IArgument
        {
            var args = getArgs(c);
            if (c.Parent != null)
            {
                args = args.Union(getArgs(c.Parent));
            }
            return args;
        }

        private static void LogArguments(this StringBuilder sb,
            string type, IEnumerable<IArgument> arguments, LogHeaderConfig config, ParseResult parseResult)
        {
            var argValues = SuppliedValues(config, parseResult, arguments);
            if (argValues.Any())
            {
                sb.AppendLine($"  {type}:");
                var maxName = argValues.Max(v => v.name.Length);

                foreach (var argValue in argValues)
                {
                    sb.AppendFormat($"    {{0, -{maxName + 1}}} = {{1}}", argValue.name, argValue.value);
                    sb.AppendLine();
                }
            }
        }

        private static List<(string name, string value)> SuppliedValues(
            LogHeaderConfig config, ParseResult parseResult, IEnumerable<IArgument> arguments)
        {
            return arguments
                .Where(arg => !config.SkipArgument?.Invoke(arg) ?? true)
                .Select(arg =>
                {
                    var specified = parseResult.ArgumentValues.TryGetValues(arg, out var values);
                    return new { arg, specified, values };
                })
                .Where(a => a.values != null || a.arg.DefaultValue != null)
                .Select(a =>
                {
                    var value = config.ObscureArgument?.Invoke(a.arg) ?? false
                        ? "***"
                        : a.values?.ToCsv();

                    if (value == null && a.arg.DefaultValue != null)
                    {
                        value = $"{a.arg.DefaultValue} (default)";
                    }

                    return (a.arg.Name, value);
                })
                .ToList();
        }
    }
}