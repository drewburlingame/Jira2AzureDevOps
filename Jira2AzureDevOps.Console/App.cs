using CommandDotNet;
using Jira2AzureDevOps.Console.Azure;
using Jira2AzureDevOps.Console.Jira;

namespace Jira2AzureDevOps.Console
{
    public class App
    {
        [SubCommand]
        public JiraApp JiraApp { get; set; }

        [SubCommand]
        public AzureImportCommands AzureImportCommands { get; set; }
    }
}