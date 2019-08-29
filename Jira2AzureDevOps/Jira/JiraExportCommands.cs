using CommandDotNet;
using Jira2AzureDevOps.Framework;
using Jira2AzureDevOps.Jira.JiraApi;
using Jira2AzureDevOps.Jira.Model;
using MoreLinq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jira2AzureDevOps.Framework.Json;
using Jira2AzureDevOps.Jira.ArgumentModels;
using Newtonsoft.Json.Linq;

namespace Jira2AzureDevOps.Jira
{
    [Command(Name = "export", Description = "Commands to export issues and metadata")]
    class JiraExportCommands
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private JiraContext _jiraContext;

        private IJiraApi _jiraApi;
        private JiraApiSettings _jiraApiSettings;

        private int _totalIssueCount;
        private EtaCalculator _etaCalculator;
        private MigrationRepository _migrationRepository;

        public Task<int> Interceptor(
            CommandContext commandContext, Func<CommandContext, Task<int>> next,
            JiraApiSettings jiraApiSettings, WorkspaceSettings workspaceSettings)
        {
            _jiraContext = new JiraContext(jiraApiSettings, workspaceSettings);
            _jiraApi = _jiraContext.Api;
            _jiraApiSettings = _jiraContext.ApiSettings;
            _migrationRepository = new MigrationRepository(_jiraContext.LocalDirs);
            return next(commandContext);
        }

        [Command(Description = "exports a subset of Jira metadata")]
        public void Metadata()
        {
            if (_jiraApiSettings.JiraOffline)
            {
                Console.Out.WriteLine("Cannot export from Jira in --offline mode.");
                return;
            }

            Logger.Info("exporting projects");
            _jiraApi.GetProjects().Wait();

            Logger.Info("exporting issue fields");
            _jiraApi.GetIssueFields().Wait();

            Logger.Info("exporting issue link types");
            _jiraApi.GetIssueLinkTypes().Wait();

            Logger.Info("exporting issue priorities");
            _jiraApi.GetIssuePriorities().Wait();

            Logger.Info("exporting issue resolutions");
            _jiraApi.GetIssueResolutions().Wait();

            Logger.Info("exporting issue types");
            _jiraApi.GetIssueTypes().Wait();

            Logger.Info("exporting labels");
            _jiraApi.GetLabels().Wait();

            Logger.Info("exporting statuses");
            _jiraApi.GetStatusesByProject().Wait();
        }

        [Command(Description = "Exports issues for the given id(s)")]
        public void IssuesById(List<IssueId> issueIds)
        {
            _totalIssueCount = issueIds.Count;

            _etaCalculator = new EtaCalculator(5, maximumDuration: TimeSpan.FromMinutes(5).Ticks, _totalIssueCount);

            issueIds
                .TakeWhile(i => Cancellation.IsNotRequested)
                .ForEach(ExportIssue);
        }

        [Command(Description = "Exports issues for the given project(s)")]
        public int IssuesByProject(
            ProjectFilter projectFilter,
            [Option(Description = "Resumes export after this issue")]
            IssueId resumeAfter,
            [Option(Description = "Specify if the issue list should come from Jira, Cache or Both. " +
                                  "Use Jira when there are unknown updates. " +
                                  "Use Cache for speed when you only need to updates.")]
            IssueSource? issueListSource = null)
        {
            if (_jiraApiSettings.JiraOffline && issueListSource.HasValue && issueListSource.Value.HasFlag(IssueSource.Jira))
            {
                throw new ArgumentException($"--{nameof(issueListSource)} cannot include {nameof(IssueSource.Jira)} when --{nameof(_jiraApiSettings.JiraOffline)} is specified.");
            }

            if (!_jiraApiSettings.JiraOffline)
            {
                ((CacheJiraApi)_jiraApi).IssueListSource = issueListSource.GetValueOrDefault(IssueSource.Both);
            }

            var projects = projectFilter.Projects;
            projects.Sort();

            _totalIssueCount = _jiraApi.GetTotalIssueCount(projects, resumeAfter).Result;
            Logger.Info("Total issue count {totalIssueCount} for {projects}", _totalIssueCount, projects.ToCsv());

            _etaCalculator = new EtaCalculator(10, maximumDuration: TimeSpan.FromMinutes(10).Ticks, _totalIssueCount);

            projects.SelectMany(p => _jiraApi.GetIssueIdsByProject(p, resumeAfter))
                .TakeWhile(i => Cancellation.IsNotRequested)
                .ForEach(ExportIssueAndLog);

            return 0;
        }

        private void ExportIssueAndLog(IssueId issueId, int index)
        {
            ExportIssue(issueId, index);
            _etaCalculator.Increment();
            if (index % 10 == 0 && _etaCalculator.TryGetEta(out var etr, out var eta))
            {
                Logger.Info(new { etr, eta });
            }
        }

        private void ExportIssue(IssueId issueId, int index)
        {
            // waiting on results prevents overwhelming Jira API resulting in 503's

            var issueData = _jiraApi.GetIssue(issueId).Result;
            var migration = new IssueMigration {IssueId = issueId};

            try
            {
                var issue = issueData.ToObject<Issue>();
                migration.IssueType = issue.Fields.IssueType.Name;
                migration.Status = issue.Fields.Status.Name;
                migration.StatusCategory = issue.Fields.Status.StatusCategory.Name;
                var attachments = issue.Fields.Attachments;

                if (!issue.ChangeLog.Histories.Any())
                {
                    Logger.Warn("History missing for {issueId}", issueId);
                }

                foreach (var attachment in attachments)
                {
                    var attachmentFile = _jiraApi.GetAttachment(attachment).Result;
                    migration.Attachments.Add(new AttachmentMigration
                    {
                        File = _jiraContext.LocalDirs.GetRelativePath(attachmentFile)
                    });
                }

                migration.ExportCompleted = true;
                _migrationRepository.Save(migration);

                AlertIfPartialPagedData(issueId, issueData);

                Logger.Info($"Exported issue {issueId} ({index + 1}/{_totalIssueCount})");
            }
            catch (Exception e)
            {
                e.Data["IssueId"] = issueId.ToString();
                throw;
            }
        }

        private void AlertIfPartialPagedData(IssueId issueId, JToken issueData)
        {
            issueData.WalkNode(o =>
            {
                if (TryGetPageData(o, out int maxResults, out int total))
                {
                    if (o.Path == "changelog" || o.Path.EndsWith(".worklog", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: when we import history, log as Error
                        Logger.Debug("Pages are missing for {issueId} {page}", issueId, new { o.Path, maxResults, total });
                    }
                    else
                    {
                        Logger.Error("Pages are missing for {issueId} {page}", issueId, new {o.Path, maxResults, total});
                    }
                }
            });
        }

        private static bool TryGetPageData(JObject jObject, out int maxResults, out int total)
        {
            if (jObject.ContainsKey("maxResults") && jObject.ContainsKey("total"))
            {
                maxResults = jObject.Value<int>("maxResults");
                total = jObject.Value<int>("total");
                return total > maxResults;
            }

            maxResults = 0;
            total = 0;
            return false;
        }

        private IEnumerable<Attachment> GetRemovedAttachments(Issue issue)
        {
            return issue.ChangeLog.Histories.SelectMany(h =>
                    h.Items
                        .Where(i => i.Field == "Attachment" && i.RemovedId != null)
                        .Select(i => i.RemovedId))
                .Select(id => _jiraApi.GetAttachmentMetadata(id).Result)
                .Select(j => j.ToObject<Attachment>())
                .ToList();
        }
    }
}