using Jira2AzureDevOps.Console.Jira;
using Jira2AzureDevOps.Logic.Jira;
using Jira2AzureDevOps.Logic.Jira.JiraApi;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Jira2AzureDevOps.Console.Framework
{
    internal static class ConsoleEnumerator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static void EnumerateOperation<T>(this IEnumerable<T> items, int totalCount, Action<T> action)
        {
            var etaCalculator = new EtaCalculator(5, maximumDuration: TimeSpan.FromMinutes(2).Ticks, totalCount);
            var totalTime = Stopwatch.StartNew();
            var count = 0;

            foreach (var item in items.TakeWhile(i => Cancellation.IsNotRequested))
            {
                count++;
                action(item);
                etaCalculator.Increment();
                if (count % 10 == 0 && etaCalculator.TryGetEta(out var etr, out var eta))
                {
                    Logger.Info(new { count, of = totalCount, elapsed = totalTime.Elapsed, etr, eta });
                }
            }
        }

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