using Jira2AzureDevOps.Framework;
using Jira2AzureDevOps.Jira.Model;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Jira.JiraApi
{
    public class CacheJiraApi : IJiraApi
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly JiraApiSettings _jiraApiSettings;
        private readonly IJiraApi _sourceApi;
        private readonly IWritableJiraApi _cacheApi;

        public IssueSource IssueListSource { get; set; } = IssueSource.Both;

        public CacheJiraApi(JiraApiSettings jiraApiSettings, IJiraApi sourceApi, IWritableJiraApi cacheApi)
        {
            _jiraApiSettings = jiraApiSettings ?? throw new ArgumentNullException(nameof(jiraApiSettings));
            _sourceApi = sourceApi ?? throw new ArgumentNullException(nameof(sourceApi));
            _cacheApi = cacheApi ?? throw new ArgumentNullException(nameof(cacheApi));
        }

        public Task<int> GetTotalIssueCount(ICollection<string> projectIds, IssueId resumeAfterId = null)
        {
            return _jiraApiSettings.JiraOffline
            ? _cacheApi.GetTotalIssueCount(projectIds, resumeAfterId)
            : _sourceApi.GetTotalIssueCount(projectIds, resumeAfterId);
        }

        public IEnumerable<IssueId> GetIssueIdsByProject(string projectId, IssueId resumeAfterId = null)
        {
            Logger.Debug("Begin JiraCacheApi.GetIssueIds {params}", new { projectIds = projectId.ToOrderedCsv(), resumeAfterId });

            if (IssueListSource.HasFlag(IssueSource.Cache))
            {
                foreach (var issueId in GetIssueIdsFromSource("cache-by-project", _cacheApi.GetIssueIdsByProject(projectId, resumeAfterId)))
                {
                    if (resumeAfterId == null || resumeAfterId < issueId)
                        resumeAfterId = issueId;

                    yield return issueId;
                }
            }

            if (IssueListSource.HasFlag(IssueSource.Jira))
            {
                foreach (var issueId in GetIssueIdsFromSource("jira-by-project", _sourceApi.GetIssueIdsByProject(projectId, resumeAfterId)))
                {
                    yield return issueId;
                }
            }
        }

        private static IEnumerable<IssueId> GetIssueIdsFromSource(string sourceName, IEnumerable<IssueId> source)
        {
            IssueId leastIssueId = null;
            IssueId maxIssueId = null;
            int issueIdCount = 0;

            foreach (var issueId in source)
            {
                if (leastIssueId == null || leastIssueId > issueId)
                    leastIssueId = issueId;

                if (maxIssueId == null || maxIssueId < issueId)
                    maxIssueId = issueId;

                issueIdCount++;
                yield return issueId;
            }

            Logger.Debug($"issue ids from {sourceName}: {{cachedIds}}",
                new { min = leastIssueId, max = maxIssueId, count = issueIdCount });
        }

        public async Task<JObject> GetIssue(IssueId issueId)
        {
            return await CallAndCache(
                "issue",
                issueId.ToString(),
                api => api.GetIssue(issueId),
                onSetCache: result => _cacheApi.SaveIssue(issueId, result));
        }

        public async Task<JObject> GetAttachmentMetadata(string attachmentId)
        {
            return await CallAndCache(
                "attachment-metadata",
                attachmentId,
                api => api.GetAttachmentMetadata(attachmentId),
                result => _cacheApi.SaveAttachmentMetadata(attachmentId, result));
        }

        public async Task<FileInfo> GetAttachment(Attachment attachment)
        {
            return await CallAndCache(
                "attachment",
                attachment.Id.ToString(),
                api => api.GetAttachment(attachment));
        }

        public async Task<JArray> GetIssueFields()
        {
            return await CallAndCache(
                "issue fields",
                null,
                api => api.GetIssueFields(),
                onSetCache: result => _cacheApi.SaveIssueFields(result));
        }

        public async Task<JObject> GetIssueLinkTypes()
        {
            return await CallAndCache(
                "issue link types",
                null,
                api => api.GetIssueLinkTypes(),
                onSetCache: result => _cacheApi.SaveIssueLinkTypes(result));
        }

        public async Task<JArray> GetIssuePriorities()
        {
            return await CallAndCache(
                "issue priorities",
                null,
                api => api.GetIssuePriorities(),
                onSetCache: result => _cacheApi.SaveIssuePriorities(result));
        }

        public async Task<JArray> GetIssueResolutions()
        {
            return await CallAndCache(
                "issue resolutions",
                null,
                api => api.GetIssueResolutions(),
                onSetCache: result => _cacheApi.SaveIssueResolutions(result));
        }

        public async Task<JArray> GetIssueTypes()
        {
            return await CallAndCache(
                "issue types",
                null,
                api => api.GetIssueTypes(),
                onSetCache: result => _cacheApi.SaveIssueTypes(result));
        }

        public async Task<string[]> GetLabels()
        {
            return await CallAndCache(
                "labels",
                null,
                api => api.GetLabels(),
                onSetCache: result => _cacheApi.SaveLabels(result));
        }

        public async Task<JArray> GetStatusesByProject()
        {
            return await CallAndCache(
                "statuses",
                null,
                api => api.GetStatusesByProject(),
                onSetCache: result => _cacheApi.SaveStatusesByProject(result));
        }

        public async Task<JArray> GetProjects()
        {
            return await CallAndCache(
                "projects",
                null,
                api => api.GetProjects(),
                onSetCache: result => _cacheApi.SaveProjects(result));
        }

        private async Task<T> CallAndCache<T>(string resourceName, string resourceId,
            Func<IJiraApi, Task<T>> getFromApi, Action<T> onSetCache = null) where T : class
        {
            T resource;

            if (!_jiraApiSettings.ForceRefresh)
            {
                resource = await getFromApi(_cacheApi);
                if (resource != null)
                {
                    Logger.Trace(new { cache = resourceName, action = "hit", key = resourceId });
                    return resource;
                }
                Logger.Trace(new { cache = resourceName, action = "miss", key = resourceId });
            }

            resource = await getFromApi(_sourceApi);

            if (onSetCache != null)
            {
                Logger.Trace(new { cache = resourceName, action = "set", key = resourceId });
                onSetCache(resource);
            }

            return resource;
        }
    }
}