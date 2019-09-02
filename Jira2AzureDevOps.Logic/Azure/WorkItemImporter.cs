using Jira2AzureDevOps.Logic.Jira;
using Jira2AzureDevOps.Logic.Jira.JiraApi;
using Jira2AzureDevOps.Logic.Jira.Model;
using Jira2AzureDevOps.Logic.Migrations;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using NLog;
using System;
using System.Linq;
using System.Text;
using WiAttachment = Microsoft.TeamFoundation.WorkItemTracking.Client.Attachment;

namespace Jira2AzureDevOps.Logic.Azure
{
    public class WorkItemImporter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly bool _force;
        private readonly MigrationRepository _migrationRepository;
        private readonly AdoContext _adoContext;
        private readonly JiraContext _jiraContext;
        private readonly StatusCsvMapper _statusMapper;
        private readonly IssueTypeCsvMapper _issueTypeCsvMapper;
        private readonly IJiraApi _jiraApi;
        private readonly AdoApi _adoApi;

        public WorkItemImporter(bool force, MigrationRepository migrationRepository, AdoContext adoContext,
            JiraContext jiraContext, StatusCsvMapper statusMapper, IssueTypeCsvMapper issueTypeCsvMapper)
        {
            _force = force;
            _migrationRepository = migrationRepository ?? throw new ArgumentNullException(nameof(migrationRepository));
            _adoContext = adoContext ?? throw new ArgumentNullException(nameof(adoContext));
            _jiraContext = jiraContext ?? throw new ArgumentNullException(nameof(jiraContext));
            _statusMapper = statusMapper ?? throw new ArgumentNullException(nameof(statusMapper));
            _issueTypeCsvMapper = issueTypeCsvMapper ?? throw new ArgumentNullException(nameof(issueTypeCsvMapper));

            _adoApi = _adoContext.Api;
            _jiraApi = _jiraContext.Api;
        }

        public bool TryImport(IssueMigration migration)
        {
            if (migration.ImportComplete)
            {
                if (_force)
                {
                    _migrationRepository.Reset(migration);
                }
                else
                {
                    Logger.Debug("Skipping already imported issue {issue}", migration.IssueId);
                    return false;
                }
            }

            if (!_issueTypeCsvMapper.TryGetMappedValue(migration, out var workItemTypeKey))
            {
                Logger.Debug("Skipping issue {issue} for unknown type {issueType}", migration.IssueId, migration.IssueType);
                return false;
            }

            if (!TryGetWorkItemType(workItemTypeKey, out var workItemType))
            {
                throw new Exception($"Unable to find work item type {workItemTypeKey} in {_adoApi.TfsProject.Name}");
            }

            var existingWorkItem = _adoApi.GetWorkItem(migration);
            if (existingWorkItem != null)
            {
                if (_force)
                {
                    Logger.Info("deleteing pre-existing workitem with originalId {originalId}", migration.IssueId);
                    _adoApi.Delete(migration);
                }
                else
                {
                    // TODO: resume migration from last stopping point
                    throw new Exception($"Skipping issue {migration.IssueId} for existing {migration.IssueType}.  TODO: resume migration");
                }
            }

            WorkItem workItem = workItemType.NewWorkItem();
            migration.TempWorkItemId = workItem.TemporaryId;
            _migrationRepository.Save(migration);
            var wiLog = new { Type = workItem.Type.Name, migration.IssueId, TempId = migration.TempWorkItemId };
            Logger.Debug("created Work Item for {wi}", wiLog);

            var issue = _jiraApi.GetIssue(migration.IssueId).Result.ToObject<Issue>();

            MapWorkItem(migration, issue, workItem);
            Logger.Debug("mapped Work Item for {wi}", wiLog);

            workItem.Save();
            migration.WorkItemId = workItem.Id;
            migration.IssueImported = true;
            _migrationRepository.Save(migration);
            Logger.Info("Imported issue {issue} as work item {workItem}",
                new { migration.IssueId, migration.IssueType, migration.Status },
                new { workItem.Id, workItem.Type.Name, workItem.State, TempId = migration.TempWorkItemId });

            return true;
        }

        private void MapWorkItem(
            IssueMigration migration, Issue issue, WorkItem workItem)
        {
            workItem[_adoContext.ApiSettings.JiraIdField] = issue.Key;

            workItem.Title = issue.Fields.Summary;
            workItem.Description = issue.Fields.Description;

            workItem.State = _statusMapper.GetMappedValueOrThrow(migration);

            workItem.Fields["System.CreatedDate"].Value = issue.Fields.Created.UtcDateTime;
            workItem.Fields["System.ChangedDate"].Value = issue.Fields.Updated.UtcDateTime;

            var comments = issue.Fields.Comment.Comments;
            if (comments.Any())
            {
                var sb = new StringBuilder(workItem.Description);
                var headerBar = new string('-', 50);
                var smallHeaderBar = new string('-', 20);
                sb.AppendLine(
                    $"{Environment.NewLine}{headerBar}{Environment.NewLine}  COMMENTS{Environment.NewLine}{headerBar}{Environment.NewLine}");
                foreach (var comment in comments)
                {
                    sb.AppendLine($"[{comment.Author.DisplayName} @ {comment.Created}]");
                    sb.AppendLine(comment.Body);
                    sb.AppendLine($"{smallHeaderBar}");
                }

                workItem.Description = sb.ToString();
            }


            foreach (var attachmentMigration in migration.Attachments)
            {
                var fullPath = _jiraContext.LocalDirs.GetFullPath(attachmentMigration.File);
                workItem.Attachments.Add(new WiAttachment(fullPath));
                Logger.Debug("Added attachment to {issueId} {file}", issue.Key, fullPath);
                attachmentMigration.Imported = true;
                //_migrationRepository.Save(migration);
            }

            // TODO: map other fields

        }

        private bool TryGetWorkItemType(string workItemTypeKey, out WorkItemType workItemType)
        {
            try
            {
                workItemType = _adoApi.TfsProject.WorkItemTypes[workItemTypeKey];
            }
            catch (WorkItemTypeDeniedOrNotExistException)
            {
                //ignore the exception will be logged below
                workItemType = null;
            }

            return workItemType != null;
        }
    }
}