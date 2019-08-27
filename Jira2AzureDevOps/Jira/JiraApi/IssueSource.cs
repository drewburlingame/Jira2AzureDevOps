using System;

namespace Jira2AzureDevOps.Jira.JiraApi
{
    [Flags]
    public enum IssueSource
    {
        Jira,
        Cache,
        Both = Jira | Cache
    }
}