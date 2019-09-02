using Jira2AzureDevOps.Logic.Jira.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Logic.Jira.JiraApi
{
    public class NullJiraApi : IJiraApi
    {
        private static readonly Task<FileInfo> NullFileInfoTask = Task.FromResult((FileInfo)null);
        private static readonly Task<JObject> NullJObjectTask = Task.FromResult((JObject)null);
        private static readonly Task<JArray> NullJArrayTask = Task.FromResult((JArray)null);

        public Task<int> GetTotalIssueCount(ICollection<string> projectIds, IssueId resumeAfterId = null)
        {
            return Task.FromResult(0);
        }

        public IEnumerable<IssueId> GetIssueIdsByProject(string projectId, IssueId resumeAfterId = null)
        {
            return Enumerable.Empty<IssueId>();
        }

        public Task<JObject> GetIssue(IssueId issueId)
        {
            return NullJObjectTask;
        }

        public Task<JObject> GetAttachmentMetadata(string attachmentId)
        {
            return NullJObjectTask;
        }

        public Task<FileInfo> GetAttachment(Attachment attachment)
        {
            return NullFileInfoTask;
        }

        public Task<JArray> GetIssueFields()
        {
            return NullJArrayTask;
        }

        public Task<JObject> GetIssueLinkTypes()
        {
            return NullJObjectTask;
        }

        public Task<JArray> GetIssuePriorities()
        {
            return NullJArrayTask;
        }

        public Task<JArray> GetIssueResolutions()
        {
            return NullJArrayTask;
        }

        public Task<JArray> GetIssueTypes()
        {
            return NullJArrayTask;
        }

        public Task<string[]> GetLabels()
        {
            return Task.FromResult((string[])null);
        }

        public Task<JArray> GetStatusesByProject()
        {
            return NullJArrayTask;
        }

        public Task<JArray> GetProjects()
        {
            return NullJArrayTask;
        }
    }
}