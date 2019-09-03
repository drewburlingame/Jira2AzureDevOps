using CommandDotNet;
using CommandDotNet.Rendering;
using Jira2AzureDevOps.Console.Framework;
using Jira2AzureDevOps.Logic.Jira;
using Jira2AzureDevOps.Logic.Jira.JiraApi;
using Jira2AzureDevOps.Logic.Migrations;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Console.Jira
{
    [Command(Name = "export", Description = "Commands to export issues and metadata")]
    public class JiraExportCommands
    {
        private readonly IConsole _console;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private JiraContext _jiraContext;

        private IJiraApi _jiraApi;
        private IJiraApiSettings _jiraApiSettings;
        private MigrationMetaDataService _migrationMetaDataService;

        public JiraExportCommands(IConsole console)
        {
            _console = console;
        }

        public Task<int> Interceptor(
            CommandContext commandContext, Func<CommandContext, Task<int>> next,
            JiraApiSettings jiraApiSettings, WorkspaceSettings workspaceSettings)
        {
            _jiraContext = new JiraContext(jiraApiSettings, workspaceSettings, commandContext.AppConfig.CancellationToken);
            _jiraApi = _jiraContext.Api;
            _jiraApiSettings = _jiraContext.ApiSettings;
            _migrationMetaDataService = new MigrationMetaDataService(_jiraContext);

            return next(commandContext);
        }

        [Command(Description = "exports a subset of Jira metadata")]
        public void Metadata()
        {
            if (_jiraApiSettings.JiraOffline)
            {
                _console.Out.WriteLine("Cannot export from Jira in --offline mode.");
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
        public void IssuesById(ExportOptions exportOptions, List<IssueId> issueIds)
        {
            issueIds.EnumerateOperation(issueIds.Count, "Export Issue", exportOptions.FailFile, ExportIssue);
        }

        [Command(Description = "Exports issues for the given project(s)")]
        public void IssuesByProject(
            ExportOptions exportOptions,
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

            var issueIds = _jiraContext.Api.GetIssueIds(projectFilter, out int totalCount, resumeAfter);
            issueIds.EnumerateOperation(totalCount, "Export Issue", exportOptions.FailFile, ExportIssue);
        }

        private void ExportIssue(IssueId issueId)
        {
            _migrationMetaDataService.UpdateMigrationMetaData(new IssueMigration { IssueId = issueId });
        }
    }
}