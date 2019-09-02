using Jira2AzureDevOps.Logic.Framework;
using Jira2AzureDevOps.Logic.Jira.Model;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Logic.Jira.JiraApi
{
    public class LocalDirJiraApi : IWritableJiraApi
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly LocalDirs _localDirs;
        private readonly ConcurrentDictionary<string, HashSet<IssueId>> _issuesByProject;

        public LocalDirJiraApi(LocalDirs localDirs)
        {
            _localDirs = localDirs ?? throw new ArgumentNullException(nameof(localDirs));
            _issuesByProject = LoadedIssuesByProject(localDirs);
        }

        public IEnumerable<(string Key, int Count)> ExportedProjectKeys()
        {
            return _issuesByProject.Select(kvp => (kvp.Key, kvp.Value.Count));
        }

        public Task<int> GetTotalIssueCount(ICollection<string> projectIds, IssueId resumeAfterId = null)
        {
            var filteredProjects = _issuesByProject.Where(kvp => projectIds.Contains(kvp.Key));

            int totalIssueCount = 0;
            if (resumeAfterId == null)
            {
                totalIssueCount = filteredProjects.Sum(kvp => kvp.Value.Count);
            }
            else
            {
                foreach (var project in filteredProjects)
                {
                    var comparison = string.Compare(project.Key, resumeAfterId.Project, StringComparison.Ordinal);
                    if (comparison == 0)
                    {
                        totalIssueCount += project.Value.Count(id => id > resumeAfterId);
                    }
                    else if (comparison > 0)
                    {
                        totalIssueCount += project.Value.Count;
                    }
                }
            }

            return Task.FromResult(totalIssueCount);
        }

        public IEnumerable<IssueId> GetIssueIdsByProject(string projectId,
            IssueId resumeAfterId = null)
        {
            Logger.Debug("Begin LocalDirJiraApi.GetIssueIds {params}", new { projectIds = projectId.ToOrderedCsv(), resumeAfterId });

            return _issuesByProject.GetOrAdd(projectId, new HashSet<IssueId>())
                .OrderBy(id => id)
                .SkipWhile(id => resumeAfterId != null && id <= resumeAfterId);
        }

        public Task<JObject> GetIssue(IssueId issueId)
        {
            var file = _localDirs.GetIssueJsonFile(issueId);
            Logger.Trace("Get issue from file: {file}", file.FullName);
            return LoadJsonFromFile<JObject>(file);
        }

        public void SaveIssue(IssueId issueId, JObject issue)
        {
            var file = _localDirs.GetIssueJsonFile(issueId);
            Logger.Trace("Save issue to file: {file}", file.FullName);
            File.WriteAllText(file.FullName, issue.ToString());
        }

        public Task<JObject> GetAttachmentMetadata(string attachmentId)
        {
            var file = _localDirs.GetAttachmentMetadataFile(attachmentId);
            Logger.Trace("Get attachment-metadata from file: {file}", file.FullName);
            return LoadJsonFromFile<JObject>(file);
        }

        public void SaveAttachmentMetadata(string attachmentId, JObject metadata)
        {
            var file = _localDirs.GetAttachmentMetadataFile(attachmentId);
            Logger.Trace("Save attachment-metadata to file: {file}", file.FullName);
            file.WriteAllText(metadata.ToString());
        }

        public Task<FileInfo> GetAttachment(Attachment attachment)
        {
            var file = _localDirs.GetAttachmentFile(attachment);
            if (file.Exists)
            {
                Logger.Debug("Downloaded attachment found: {file}", file.FullName);
                return Task.FromResult(file);
            }

            Logger.Debug("Downloaded attachment not found: {file}", file.FullName);
            return Task.FromResult((FileInfo)null);
        }

        public Task<JArray> GetIssueFields()
        {
            var file = _localDirs.GetIssueFieldsFile();
            Logger.Trace("Get issue fields from file: {file}", file.FullName);
            return LoadJsonFromFile<JArray>(file);
        }

        public void SaveIssueFields(JArray issueFields)
        {
            var file = _localDirs.GetIssueFieldsFile();
            Logger.Trace("Save issue fields to file: {file}", file.FullName);
            file.WriteAllText(issueFields.ToString());
        }

        public Task<JObject> GetIssueLinkTypes()
        {
            var file = _localDirs.GetIssueLinkTypesFile();
            Logger.Trace("Get issue link types from file: {file}", file.FullName);
            return LoadJsonFromFile<JObject>(file);
        }

        public void SaveIssueLinkTypes(JObject linkTypes)
        {
            var file = _localDirs.GetIssueLinkTypesFile();
            Logger.Trace("Save issue link types to file: {file}", file.FullName);
            file.WriteAllText(linkTypes.ToString());
        }

        public Task<JArray> GetIssuePriorities()
        {
            var file = _localDirs.GetIssuePrioritiesFile();
            Logger.Trace("Get issue priorities from file: {file}", file.FullName);
            return LoadJsonFromFile<JArray>(file);
        }

        public void SaveIssuePriorities(JArray priorities)
        {
            var file = _localDirs.GetIssuePrioritiesFile();
            Logger.Trace("Save issue priorities to file: {file}", file.FullName);
            file.WriteAllText(priorities.ToString());
        }

        public Task<JArray> GetIssueResolutions()
        {
            var file = _localDirs.GetIssueResolutionsFile();
            Logger.Trace("Get issue resolutions from file: {file}", file.FullName);
            return LoadJsonFromFile<JArray>(file);
        }

        public void SaveIssueResolutions(JArray resolutions)
        {
            var file = _localDirs.GetIssueResolutionsFile();
            Logger.Trace("Save issue resolutions to file: {file}", file.FullName);
            file.WriteAllText(resolutions.ToString());
        }

        public Task<JArray> GetIssueTypes()
        {
            var file = _localDirs.GetIssueTypesFile();
            Logger.Trace("Get issue types from file: {file}", file.FullName);
            return LoadJsonFromFile<JArray>(file);
        }

        public void SaveIssueTypes(JArray issueTypes)
        {
            var file = _localDirs.GetIssueTypesFile();
            Logger.Trace("Save issue types to file: {file}", file.FullName);
            file.WriteAllText(issueTypes.ToString());
        }

        public Task<string[]> GetLabels()
        {
            var file = _localDirs.GetLabelsFile();
            Logger.Trace("Get labels from file: {file}", file.FullName);
            return LoadLinesFromFile(file);
        }

        public void SaveLabels(string[] labels)
        {
            var file = _localDirs.GetLabelsFile();
            Logger.Trace("Save labels to file: {file}", file.FullName);
            file.WriteAllLines(labels);
        }

        public Task<JArray> GetStatusesByProject()
        {
            var file = _localDirs.GetStatusesFile();
            Logger.Trace("Get statuses from file: {file}", file.FullName);
            return LoadJsonFromFile<JArray>(file);
        }

        public void SaveStatusesByProject(JArray statuses)
        {
            var file = _localDirs.GetStatusesFile();
            Logger.Trace("Save statuses to file: {file}", file.FullName);
            file.WriteAllText(statuses.ToString());
        }

        public Task<JArray> GetProjects()
        {
            var file = _localDirs.GetProjectsFile();
            Logger.Trace("Get projects from file: {file}", file.FullName);
            return LoadJsonFromFile<JArray>(file);
        }

        public void SaveProjects(JArray projects)
        {
            var file = _localDirs.GetProjectsFile();
            Logger.Trace("Save projects to file: {file}", file.FullName);
            file.WriteAllText(projects.ToString());
        }

        private static async Task<T> LoadJsonFromFile<T>(FileInfo file) where T : JToken
        {
            var json = LoadTextFromFile(file);
            return json == null ? null : (T)JToken.Parse(json);
        }

        private static string LoadTextFromFile(FileInfo file)
        {
            if (!file.Exists)
            {
                Logger.Trace("File not found: {file}", file.FullName);
                return null;
            }

            return file.ReadAllText();
        }

        private static async Task<string[]> LoadLinesFromFile(FileInfo file)
        {
            if (!file.Exists)
            {
                Logger.Trace("File not found: {file}", file.FullName);
                return null;
            }

            return file.ReadAllLines();
        }

        private static ConcurrentDictionary<string, HashSet<IssueId>> LoadedIssuesByProject(LocalDirs localDirs)
        {
            var issuesByProject = localDirs.Issues.GetDirectories()
                .Select(dir => new IssueId(dir.Name))
                .ToLookup(id => id.Project)
                .ToDictionary(g => g.Key, g => g.ToHashSet());

            Logger.Debug("Loaded {issueCount} cached issues", issuesByProject.Sum(kvp => kvp.Value.Count));

            return new ConcurrentDictionary<string, HashSet<IssueId>>(issuesByProject);
        }
    }
}