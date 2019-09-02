using System;

namespace Jira2AzureDevOps.Logic.Jira.JiraApi
{
    [Flags]
    public enum IssueSource
    {
        Jira,
        Cache,
        Both = Jira | Cache
    }
}