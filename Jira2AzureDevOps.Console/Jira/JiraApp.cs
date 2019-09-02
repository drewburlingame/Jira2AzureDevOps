using CommandDotNet;

namespace Jira2AzureDevOps.Console.Jira
{
    [Command(Name = "jira", Description = "Jira commands")]
    public class JiraApp
    {
        [SubCommand]
        public JiraExportCommands JiraExportCommands { get; set; }

        [SubCommand]
        public JiraReportCommands JiraReportCommands { get; set; }
    }
}