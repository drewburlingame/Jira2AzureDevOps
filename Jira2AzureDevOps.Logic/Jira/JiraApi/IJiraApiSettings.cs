using Jira2AzureDevOps.Logic.Framework;

namespace Jira2AzureDevOps.Logic.Jira.JiraApi
{
    public interface IJiraApiSettings
    {
        string JiraUsername { get; set; }
        Password JiraToken { get; set; }
        string JiraUrl { get; set; }
        int JiraBatchSize { get; set; }
        bool JiraOffline { get; set; }
        bool ForceRefresh { get; set; }
    }
}