using Atlassian.Jira.Remote;
using Jira2AzureDevOps.Logic.Jira.Model;
using Newtonsoft.Json.Linq;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Logic.Jira.JiraApi
{
    public class RestJiraApi : IJiraApi
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IJiraApiSettings _jiraApiSettings;
        private readonly LocalDirs _localDirs;
        private readonly CancellationToken _cancellationToken;
        private readonly string _apiUrl;
        private readonly Atlassian.Jira.Jira _jiraClient;
        private readonly IJiraRestClient _restClient;

        public RestJiraApi(IJiraApiSettings jiraApiSettings, LocalDirs localDirs, CancellationToken cancellationToken)
        {
            _jiraApiSettings = jiraApiSettings ?? throw new ArgumentNullException(nameof(jiraApiSettings));
            _localDirs = localDirs ?? throw new ArgumentNullException(nameof(localDirs));
            _cancellationToken = cancellationToken;
            _apiUrl = "/rest/api/2/";
            _jiraClient = Atlassian.Jira.Jira.CreateRestClient(jiraApiSettings.JiraUrl, jiraApiSettings.JiraUsername, jiraApiSettings.JiraToken.Value);
            _restClient = _jiraClient.RestClient;
        }

        public Task<int> GetTotalIssueCount(ICollection<string> projectIds, IssueId resumeAfterId = null)
        {
            return new JiraRestApiIssueIdEnumerator(_jiraClient, _cancellationToken).GetTotalIssueCount(projectIds, resumeAfterId);
        }

        public IEnumerable<IssueId> GetIssueIdsByProject(string projectId, IssueId resumeAfterId = null)
        {
            return new JiraRestApiIssueIdEnumerator(_jiraClient, _cancellationToken).GetIssueIdsByProject(projectId, resumeAfterId, _jiraApiSettings.JiraBatchSize);
        }

        public async Task<JObject> GetIssue(IssueId issueId)
        {
            Logger.Debug("Fetching issue {issueId}", issueId);
            var url = $"{_apiUrl}/issue/{issueId}?fields=*all&expand=changelog,renderedFields";
            return (JObject)await _restClient.ExecuteRequestAsync(Method.GET, url, token: _cancellationToken).ConfigureAwait(false);
        }

        public async Task<JObject> GetAttachmentMetadata(string attachmentId)
        {
            Logger.Debug("Fetching attachment metadata {attachmentId}", attachmentId);
            var url = $"{_apiUrl}/attachment/{attachmentId}";
            return (JObject)await _restClient.ExecuteRequestAsync(Method.GET, url, token: _cancellationToken).ConfigureAwait(false);
        }

        public async Task<JArray> GetIssueFields()
        {
            Logger.Debug("Fetching issue fields");
            var url = $"{_apiUrl}/field";
            return (JArray)await _restClient.ExecuteRequestAsync(Method.GET, url, token: _cancellationToken).ConfigureAwait(false);
        }

        public async Task<JObject> GetIssueLinkTypes()
        {
            Logger.Debug("Fetching issue link types");
            var url = $"{_apiUrl}/issueLinkType";
            return (JObject)await _restClient.ExecuteRequestAsync(Method.GET, url, token: _cancellationToken).ConfigureAwait(false);
        }

        public async Task<JArray> GetIssuePriorities()
        {
            Logger.Debug("Fetching issue priorities");
            var url = $"{_apiUrl}/priority";
            return (JArray)await _restClient.ExecuteRequestAsync(Method.GET, url, token: _cancellationToken).ConfigureAwait(false);
        }

        public async Task<JArray> GetIssueResolutions()
        {
            Logger.Debug("Fetching issue resolutions");
            var url = $"{_apiUrl}/resolution";
            return (JArray)await _restClient.ExecuteRequestAsync(Method.GET, url, token: _cancellationToken).ConfigureAwait(false);
        }

        public async Task<JArray> GetIssueTypes()
        {
            Logger.Debug("Fetching issue types");
            var url = $"{_apiUrl}/issuetype";
            return (JArray)await _restClient.ExecuteRequestAsync(Method.GET, url, token: _cancellationToken).ConfigureAwait(false);
        }

        public async Task<string[]> GetLabels()
        {
            var labels = new List<string>();
            Logger.Debug("Fetching labels");
            var url = $"{_apiUrl}/label";
            bool isLast = false;

            while (!isLast)
            {
                var page = (JObject)await _restClient.ExecuteRequestAsync(Method.GET, url, token: _cancellationToken)
                    .ConfigureAwait(false);

                isLast = page.Value<bool>("isLast");
                var values = page.GetValue("values").Values<string>();
                labels.AddRange(values.Where(v => !string.IsNullOrWhiteSpace(v)));
            }

            return labels.ToArray();
        }

        public async Task<JArray> GetStatusesByProject()
        {
            var jArray = new JArray();
            var projects = await _jiraClient.Projects.GetProjectsAsync(_cancellationToken).ConfigureAwait(false);

            foreach (var project in projects)
            {
                var url = $"{_apiUrl}/project/{project.Key}/statuses";
                var statuses = (JArray)await _restClient.ExecuteRequestAsync(Method.GET, url, token: _cancellationToken).ConfigureAwait(false);

                var jObject = new JObject
                {
                    {"project", new JValue(project.Key)},
                    {"statusesByType", statuses }
                };
                jArray.Add(jObject);
            }

            return jArray;
        }

        public async Task<JArray> GetProjects()
        {
            Logger.Debug("Fetching projects");
            var url = $"{_apiUrl}/project?expand=description,lead,issueTypes,url,projectKeys";
            return (JArray)await _restClient.ExecuteRequestAsync(Method.GET, url, token: _cancellationToken).ConfigureAwait(false);
        }

        public async Task<FileInfo> GetAttachment(Attachment attachment)
        {
            Logger.Debug("Downloading attachment {attachmentId}", attachment.Id);

            var zipFile = _localDirs.GetAttachmentFile(attachment);

            using (var wc = new WebClient())
            {
                string encodedUserNameAndPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(_jiraApiSettings.JiraUsername + ":" + _jiraApiSettings.JiraToken));

                wc.Headers.Remove(HttpRequestHeader.Authorization);
                wc.Headers.Add(HttpRequestHeader.Authorization, "Basic " + encodedUserNameAndPassword);
                await wc.DownloadFileTaskAsync(attachment.Content, zipFile.FullName);
            }

            return zipFile;
        }
    }
}