using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Linq;
using Jira2AzureDevOps.Jira;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Jira2AzureDevOps.AzureDevOps
{
    public class RestAdoApi
    {
        private readonly AdoApiSettings _adoApiSettings;
        private VssConnection _vssConnection;
        private TfsTeamProjectCollection _tfsCollection;
        private WorkItemStore _workItemStore;

        public RestAdoApi(AdoApiSettings adoApiSettings)
        {
            _adoApiSettings = adoApiSettings;
        }

        public bool TryConnectToTfs()
        {
            var creds = new VssBasicCredential("", _adoApiSettings.AdoToken);
            var accountUri = new Uri(_adoApiSettings.AdoUrl);
            _vssConnection = new VssConnection(accountUri, creds);

            _tfsCollection = new TfsTeamProjectCollection(accountUri, creds);
            _tfsCollection.Authenticate();

            //we need to bypass rules if we want to load data in the past.
            _workItemStore = new WorkItemStore(_tfsCollection, WorkItemStoreFlags.BypassRules);
            return true;
        }

        internal Project GetProject(string name)
        {
            return _workItemStore.Projects[name];
        }

        public WorkItem GetWorkItem(IssueId originalId)
        {
            var wiql = $@"select * from  workitems where {_adoApiSettings.JiraIdField} = '" + originalId + "'";
            return _workItemStore.Query(wiql).OfType<WorkItem>().FirstOrDefault();
        }

        public void Delete(params WorkItem[] workItems)
        {
            _workItemStore.DestroyWorkItems(workItems.Select(i => i.Id));
        }
    }
}