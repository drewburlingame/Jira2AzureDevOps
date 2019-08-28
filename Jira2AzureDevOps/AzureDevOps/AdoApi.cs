using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Linq;
using Jira2AzureDevOps.Jira;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using NLog;
using Project = Microsoft.TeamFoundation.WorkItemTracking.Client.Project;

namespace Jira2AzureDevOps.AzureDevOps
{
    public class AdoApi
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly AdoApiSettings _adoApiSettings;
        private VssConnection _vssConnection;
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

            var creds = new VssBasicCredential("", _adoApiSettings.AdoToken);
            var accountUri = new Uri(_adoApiSettings.AdoUrl);
            _vssConnection = new VssConnection(accountUri, creds);

            Logger.Debug("Try connect to TFS {url}", accountUri);

            _tfsCollection = new TfsTeamProjectCollection(accountUri, creds);
            _tfsCollection.Authenticate();

            //we need to bypass rules if we want to load data in the past.
            _workItemStore = new WorkItemStore(_tfsCollection, WorkItemStoreFlags.BypassRules);
            TfsProject = _workItemStore.Projects[_adoApiSettings.AdoProject];

            Logger.Debug("Loaded project {projectName} {url}", TfsProject.Name, TfsProject.Uri);

            return _isConnected = true;
        }

        internal WorkItem CreateWorkItem(string type)
        {
            return TfsProject.WorkItemTypes[type].NewWorkItem();
        }

        public WorkItem GetWorkITem(int wiId)
        {
            Logger.Debug("Get work item by work item id {wiId}", wiId);
            return _workItemStore.GetWorkItem(wiId);
        }

        public WorkItem GetWorkItem(IssueId jiraId)
        {
            Logger.Debug("Get work item by jira id {jiraId}", jiraId);
            var wiql = $@"select * from  workitems where {_adoApiSettings.JiraIdField} = '" + jiraId + "'";
            return _workItemStore.Query(wiql).OfType<WorkItem>().FirstOrDefault();
        }

        public void Delete(params WorkItem[] workItems)
        {
            Logger.Debug("(DISABLED) Deleting work items {ids}", workItems.Select(wi => $"{wi.Id} ({wi[_adoApiSettings.JiraIdField]})").ToArray());
            //_workItemStore.DestroyWorkItems(workItems.Select(i => i.Id));
        }
    }
}