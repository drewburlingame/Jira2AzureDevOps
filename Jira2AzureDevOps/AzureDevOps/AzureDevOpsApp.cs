using CommandDotNet;
using Jira2AzureDevOps.Framework;
using Jira2AzureDevOps.Jira;
using Jira2AzureDevOps.Jira.JiraApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jira2AzureDevOps.Jira.Model;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Internals;
using NLog;
using Project = Microsoft.TeamFoundation.WorkItemTracking.Client.Project;

namespace Jira2AzureDevOps.AzureDevOps
{
    [Command(Name = "azure", Description = "Azure DevOps commands")]
    class AzureDevOpsApp
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private AdoApiSettings _adoApiSettings;
        private RestAdoApi _adoApi;
        private LocalDirJiraApi _jiraApi;
        private MigrationRepository _migrationRepository;
        private JiraContext _jiraContext;
        private Project _adoProject;

        public Task<int> Intercept(
            CommandContext commandContext, Func<CommandContext, Task<int>> next,
            AdoApiSettings adoApiSettings, WorkspaceSettings workspaceSettings, JiraApiSettings jiraApiSettings)
        {
            _adoApiSettings = adoApiSettings;
            _adoApi = new RestAdoApi(adoApiSettings);
            if (!_adoApi.TryConnectToTfs())
            {
                Console.Out.WriteLine("Unable to connect to TFS");
                return Task.FromResult(1);
            }
            _adoProject = _adoApi.GetProject(adoApiSettings.AdoProject);

            _jiraContext = new JiraContext(jiraApiSettings, workspaceSettings);
            _migrationRepository = new MigrationRepository(_jiraContext.LocalDirs);
            return next(commandContext);
        }

        [Command(Description = "Imports the issue(s) for the given id(s)")]
        public void ImportById(List<IssueId> issueIds)
        {
            if (issueIds.IsNullOrEmpty())
            {
                Console.Out.WriteLine("no issue ids provided");
                return;
            }
            foreach (var migration in issueIds.Select(id => _migrationRepository.GetOrCreate(id)).Where(m => !m.ImportComplete))
            {
                ImportWorkItem(migration);
            }
        }

        private void ImportWorkItem(IssueMigration migration)
        {
            var issueId = migration.IssueId;
            var existingWorkItem = _adoApi.GetWorkItem(issueId);
            var issue = _jiraApi.GetIssue(issueId).Result.ToObject<Issue>();

            if (existingWorkItem != null)
            {
                Logger.Info("deleteing pre-existing workitem with originalId {originalId}", issueId);
                _adoApi.Delete(existingWorkItem);
            }

            WorkItem workItem = CreateWorkItem(issue);
            if (workItem == null)
            {
                return;
            }
        }

        private WorkItem CreateWorkItem(Issue issue)
        {
            var workItemTypeKey = GetWorkItemTypeKey(issue);

            WorkItemType type = null;
            try
            {
                type = _adoProject.WorkItemTypes[workItemTypeKey];
            }
            catch (WorkItemTypeDeniedOrNotExistException) { }//ignore the exception will be logged  

            if (type == null)
            {
                Logger.Error("Unable to find work item type {WorkItemDestinationType}", workItemTypeKey);
                return null;
            }

            WorkItem workItem = new WorkItem(type);
            Logger.Info("created Work Item for type {workItemType} related to original id {originalId}", workItem.Type.Name, issue.Id);

            //now start creating basic value that we need, like the original id 
            workItem[_adoApiSettings.JiraIdField] = issue.Id;

            workItem.UploadAttachment(new AttachmentInfo());
            return workItem;
        }

        private string GetWorkItemTypeKey(Issue issue)
        {
            return "TODO";
        }
    }
}
