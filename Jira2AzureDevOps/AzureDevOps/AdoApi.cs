using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Jira2AzureDevOps.Framework;
using Jira2AzureDevOps.Jira;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using NLog;
using Project = Microsoft.TeamFoundation.WorkItemTracking.Client.Project;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;

namespace Jira2AzureDevOps.AzureDevOps
{
    public class AdoApi
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly AdoApiSettings _adoApiSettings;
        private TfsTeamProjectCollection _tfsCollection;
        private WorkItemStore _workItemStore;
        private bool _isConnected;
        
        public Project TfsProject { get; private set; }


        public AdoApi(AdoApiSettings adoApiSettings)
        {
            _adoApiSettings = adoApiSettings;
        }

        public bool TryConnectToTfs()
        {
            if (_isConnected)
            {
                return true;
            }

            var accountUri = new Uri(_adoApiSettings.AdoUrl);
            _tfsCollection = new TfsTeamProjectCollection(
                accountUri, 
                new VssBasicCredential("", _adoApiSettings.AdoToken.Value));

            Logger.Debug("Try connect to TFS {url}", accountUri);

            _tfsCollection.Authenticate();

            //we need to bypass rules if we want to load data in the past.
            _workItemStore = new WorkItemStore(_tfsCollection, WorkItemStoreFlags.BypassRules);
            TfsProject = _workItemStore.Projects[_adoApiSettings.AdoProject];

            Logger.Debug("Loaded project {projectName} {url}", TfsProject.Name, TfsProject.Uri);

            return _isConnected = true;
        }

        public WorkItem GetWorkItem(IssueMigration issueMigration)
        {
            Logger.Debug("Get work item {migration}", (jiraId:issueMigration.IssueId.ToString(), wiId:issueMigration.WorkItemId));
            return issueMigration.WorkItemId == default
                ? GetWorkItem(issueMigration.IssueId)
                : _workItemStore.GetWorkItem(issueMigration.WorkItemId);
        }

        public WorkItem GetWorkItem(IssueId jiraId)
        {
            Logger.Debug("Get work item by jira id {jiraId}", jiraId);
            var wiql = $@"select * from  workitems where {_adoApiSettings.JiraIdField} = '{jiraId}'";
            return _workItemStore.Query(wiql).OfType<WorkItem>().FirstOrDefault();
        }

        public IEnumerable<WorkItem> GetWorkItems(IEnumerable<IssueId> jiraIds)
        {
            var ids = jiraIds.ToCollection();
            Logger.Debug("Get work items by jira ids {jiraIds}", ids);
            var wiql = $@"select * from  workitems where {_adoApiSettings.JiraIdField} IN ({ids.Select(i => $"'{i}'").ToCsv()})";
            return _workItemStore.Query(wiql).OfType<WorkItem>();
        }

        public void Delete(IssueMigration migration)
        {
            Delete(migration.ToEnumerable());
        }

        public void Delete(IEnumerable<IssueMigration> issueMigrations)
        {
            var migrations = issueMigrations.ToCollection();
            var idsToDelete = migrations.Where(m => m.WorkItemId != default).Select(m => (jiraId:m.IssueId.ToString(), wiId: m.WorkItemId));
            var missingWiId = migrations.Where(m => m.WorkItemId == default).Select(m => m.IssueId).ToList();
            if (missingWiId.Any())
            {
                var foundIds = GetWorkItems(missingWiId).Select(wi => (jiraId: wi[_adoApiSettings.JiraIdField].ToString(), wiId:wi.Id));
                idsToDelete = idsToDelete.Concat(foundIds);
            }
            Delete(idsToDelete.ToArray());
        }

        private void Delete((string jiraId, int wiId)[] ids)
        {
            if (Cancellation.IsRequested) return;

            Logger.Debug("Deleting work items {ids}", ids);
            _workItemStore.DestroyWorkItems(ids.Select(i => i.wiId));
        }
    }
}