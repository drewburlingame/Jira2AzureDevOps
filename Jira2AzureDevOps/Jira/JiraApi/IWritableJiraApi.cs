using Newtonsoft.Json.Linq;

namespace Jira2AzureDevOps.Jira.JiraApi
{
    public interface IWritableJiraApi : IJiraApi
    {
        void SaveIssue(IssueId issueId, JObject issue);
        void SaveAttachmentMetadata(string attachmentId, JObject metadata);
        void SaveIssueFields(JArray issueFields);
        void SaveIssueLinkTypes(JObject linkTypes);
        void SaveIssuePriorities(JArray priorities);
        void SaveIssueResolutions(JArray resolutions);
        void SaveIssueTypes(JArray issueTypes);
        void SaveLabels(string[] labels);
        void SaveStatusesByProject(JArray statuses);
        void SaveProjects(JArray projects);
    }
}