using Jira2AzureDevOps.Logic.Jira.JiraApi;
using NLog;
using System.Threading;

namespace Jira2AzureDevOps.Logic.Jira
{
    public class JiraContext
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IJiraApiSettings ApiSettings { get; }
        public IJiraApi Api { get; }
        public LocalDirs LocalDirs { get; }
        public LocalDirJiraApi LocalJiraApi { get; }

        public JiraContext(IJiraApiSettings apiSettings, IWorkspaceSettings workspaceSettings, CancellationToken cancellationToken)
        {
            ApiSettings = apiSettings;
            LocalDirs = new LocalDirs(workspaceSettings.WorkspaceDir);
            LocalJiraApi = new LocalDirJiraApi(LocalDirs);

            if (apiSettings.JiraOffline)
            {
                Logger.Debug("Using local cache only");
                Api = LocalJiraApi;
            }
            else
            {
                Logger.Debug("Using jira api at {url}", apiSettings.JiraUrl);
                var restJiraApi = new RestJiraApi(apiSettings, LocalDirs, cancellationToken);
                Api = new CacheJiraApi(apiSettings, restJiraApi, LocalJiraApi);
            }
        }
    }
}