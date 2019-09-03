using System.Collections.Generic;
using System.Linq;
using Jira2AzureDevOps.Logic.Jira;
using Jira2AzureDevOps.Logic.Jira.JiraApi;

namespace Jira2AzureDevOps.Console.Jira
{
    public static class JiraApiExtensions
    {
        internal static IEnumerable<IssueId> GetIssueIds(this IJiraApi jiraApi,
            ProjectFilter projectFilter, out int totalCount, IssueId resumeAfter = null)
        {
            var projects = projectFilter.Projects;
            projects.Sort();
            totalCount = jiraApi.GetTotalIssueCount(projects, resumeAfter).Result;

            return projects.SelectMany(p => jiraApi.GetIssueIdsByProject(p, resumeAfter));
        }
    }
}