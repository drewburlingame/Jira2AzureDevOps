using CommandDotNet;
using CommandDotNet.Execution;
using Jira2AzureDevOps.Logic.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Console.Framework
{
    public static class SelfValidatingArgumentsMiddleware
    {
        public static AppRunner UseSelfValidatingArgumentModels(this AppRunner appRunner)
        {
            return appRunner.Configure(c => c.UseMiddleware(ValidateModels, MiddlewareStages.PostBindValuesPreInvoke));
        }

        private static Task<int> ValidateModels(CommandContext context, ExecutionDelegate next)
        {
            var paramValues = context.InvocationContexts.All.SelectMany(ic => ic.Invocation.ParameterValues);
            var errors = paramValues
                .OfType<ISelfValidatingArgumentModel>()
                .SelectMany(m => m.GetValidationErrors())
                .ToCsv(Environment.NewLine);

            if (errors.IsNullOrWhiteSpace())
            {
                return next(context);
            }

            context.Console.Out.WriteLine(errors);
            return Task.FromResult(2);
        }
    }
}