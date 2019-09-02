using CommandDotNet;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Console.Framework
{
    public interface ISelfValidatingArgumentModel : IArgumentModel
    {
        IEnumerable<string> GetValidationErrors();
    }
}