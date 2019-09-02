using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Execution;

namespace Jira2AzureDevOps.Framework.CommandDotNet
{
    public static class SetDefaultsFromConfigMiddleware
    {
        public static AppRunner UseAppSettingsForOptionDefaults(this AppRunner appRunner,
            NameValueCollection nameValue = null)
        {
            // run before help command so help will display the updated defaults
            return appRunner.Configure(c =>
            {
                var config = new Config
                {
                    AppSettings = nameValue ?? ConfigurationManager.AppSettings
                };
                c.Services.Set(config);

                // run before help so the default values can be displayed in the help text 
                c.UseMiddleware(SetDefaultsFromAppSettings, MiddlewareStages.PostParseInputPreBindValues, -1);
            });
        }

        private static Task<int> SetDefaultsFromAppSettings(CommandContext context, Func<CommandContext, Task<int>> next)
        {
            if (context.ParseResult.ParseError != null)
            {
                return next(context);
            }

            var config = context.AppConfig.Services.Get<Config>();
            var command = context.ParseResult.TargetCommand;
            var options = command.Options.Union(command.Parent?.Options ?? Enumerable.Empty<Option>());
            
            string GetLongNameSetting(Option option) => 
                !option.LongName.IsNullOrWhiteSpace() ? config.AppSettings[$"--{option.LongName}"] : null;

            string GetShortNameSetting(Option option) =>
                option.ShortName.HasValue ? config.AppSettings[$"-{option.ShortName}"] : null;

            foreach (var option in options)
            {
                string value = GetLongNameSetting(option) ?? GetShortNameSetting(option);

                if (value != null)
                {
                    if (option.Arity.AllowsZeroOrMore())
                    {
                        option.DefaultValue = value
                            .Split(",")
                            .Select(v => ConvertToDefault(option, v))
                            .ToList();
                    }
                    else
                    {
                        option.DefaultValue = ConvertToDefault(option, value);
                    }
                }
            }

            return next(context);
        }

        private static object ConvertToDefault(Option option, string value) =>
            option.TypeInfo.UnderlyingType.IsAssignableFrom(typeof(Password))
                ? (object)new Password(value)
                : value;

        private class Config
        {
            public NameValueCollection AppSettings { get; set; }
        }
    }
}