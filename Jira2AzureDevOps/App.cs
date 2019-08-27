using CommandDotNet;
using Jira2AzureDevOps.AzureDevOps;
using Jira2AzureDevOps.Jira;

namespace Jira2AzureDevOps
{
    class App
    {
        [SubCommand]
        public JiraApp JiraApp { get; set; }

        [SubCommand]
        public AzureDevOpsApp AzureDevOpsApp { get; set; }
    }
}