using CommandDotNet;

namespace Jira2AzureDevOps.Jira
{
    [Command(Name = "jira", Description = "Jira commands")]
    class JiraApp
    {
        [SubCommand]
        public JiraExportCommands JiraExportCommands { get; set; }

        [SubCommand]
        public JiraReportCommands JiraReportCommands { get; set; }
    }
}