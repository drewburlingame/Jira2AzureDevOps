using CommandDotNet;
using CommandDotNet.Execution;
using System;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Jira2AzureDevOps.Framework.CommandDotNet
{
    public static class SelfValidatingArgumentsMiddleware
    {
        public static AppRunner UseSelfValidatingArgumentModels(this AppRunner appRunner)
        {
            return appRunner.Configure(c => c.UseMiddleware(ValidateModels, MiddlewareStages.PostBindValuesPreInvoke));
        }

        private static Task<int> ValidateModels(CommandContext context, Func<CommandContext, Task<int>> next)
        {
            var commandParams = context.InvocationContext.CommandInvocation.ParameterValues;
            var interceptorParams = context.InvocationContext.InterceptorInvocation.ParameterValues;
            var errors = commandParams.Union(interceptorParams ?? Enumerable.Empty<object>())
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