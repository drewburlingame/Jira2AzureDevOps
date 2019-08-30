﻿using Jira2AzureDevOps.AzureDevOps;
using Jira2AzureDevOps.Jira.JiraApi;
using NLog;

namespace Jira2AzureDevOps.Jira
{
    public class JiraContext
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public JiraApiSettings ApiSettings { get; }
        public IJiraApi Api { get; }
        public LocalDirs LocalDirs { get; }
        public LocalDirJiraApi LocalJiraApi { get; }

        public JiraContext(JiraApiSettings apiSettings, WorkspaceSettings workspaceSettings)
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
                var restJiraApi = new RestJiraApi(apiSettings, LocalDirs, Cancellation.Token);
                Api = new CacheJiraApi(apiSettings, restJiraApi, LocalJiraApi);
            }
        }
    }
}