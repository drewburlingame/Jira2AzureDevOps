using Jira2AzureDevOps.Jira.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Jira.JiraApi
{
    public interface IJiraApi
    {
        Task<int> GetTotalIssueCount(ICollection<string> projectIds, IssueId resumeAfterId = null);
        IEnumerable<IssueId> GetIssueIdsByProject(string projectId, IssueId resumeAfterId = null);
        Task<JObject> GetIssue(IssueId issueId);
        Task<JObject> GetAttachmentMetadata(string attachmentId);
        Task<FileInfo> GetAttachment(Attachment attachment);
        Task<JArray> GetIssueFields();
        Task<JObject> GetIssueLinkTypes();
        Task<JArray> GetIssuePriorities();
        Task<JArray> GetIssueResolutions();
        Task<JArray> GetIssueTypes();
        Task<string[]> GetLabels();
        Task<JArray> GetStatusesByProject();
        Task<JArray> GetProjects();
    }
}