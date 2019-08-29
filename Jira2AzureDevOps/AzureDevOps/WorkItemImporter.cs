using System;
using Jira2AzureDevOps.Jira;
using Jira2AzureDevOps.Jira.JiraApi;
using Jira2AzureDevOps.Jira.Model;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using NLog;

namespace Jira2AzureDevOps.AzureDevOps
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
            if(migration.ImportComplete)
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
                Logger.Error("Unable to find work item type {WorkItemDestinationType} in {Project}", workItemTypeKey, _adoApi.TfsProject.Name);
                return false;
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
                    Logger.Error("Skipping issue {issue} for existing {issueType}.  TODO: resume migration", migration.IssueId, migration.IssueType);
                    return false;
                }
            }

            WorkItem workItem = workItemType.NewWorkItem();
            Logger.Debug("created Work Item for type {workItemType} for jira id {jiraId}", workItem.Type.Name, migration.IssueId);

            var issue = _jiraApi.GetIssue(migration.IssueId).Result.ToObject<Issue>();

            MapWorkItem(migration, issue, workItem);
            Logger.Debug("mapped Work Item for type {workItemType} for jira id {jiraId}", workItem.Type.Name, migration.IssueId);

            workItem.Save();
            migration.WorkItemId = workItem.Id;
            migration.IssueImported = true;
            _migrationRepository.Save(migration);
            Logger.Info("Imported issue {issue} as work item {workItem}",
                new {migration.IssueId, migration.IssueType, migration.Status},
                new {workItem.Id, workItem.Type.Name, workItem.State});

            return true;
        }

        private void MapWorkItem(
            IssueMigration migration, Issue issue, WorkItem workItem)
        {
            workItem[_adoContext.ApiSettings.JiraIdField] = issue.Key;

            workItem.Fields["System.CreatedDate"].Value = DateTime.Now.AddDays(-1);
            workItem.Fields["System.ChangedDate"].Value = DateTime.Now.AddDays(-1);

            workItem.Title = issue.Fields.Summary;
            workItem.Description = issue.Fields.Description;

            workItem.State = _statusMapper.GetMappedValueOrThrow(migration);

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