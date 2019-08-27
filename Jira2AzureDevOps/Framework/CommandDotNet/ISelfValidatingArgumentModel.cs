using CommandDotNet;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Framework.CommandDotNet
{
    public interface ISelfValidatingArgumentModel : IArgumentModel
    {
        IEnumerable<string> GetValidationErrors();
    }
}