using Jira2AzureDevOps.Framework;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Jira.JiraApi
{
    public class JiraRestApiIssueIdEnumerator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationToken _cancellationToken;
        private readonly Atlassian.Jira.Jira _jiraClient;

        public JiraRestApiIssueIdEnumerator(Atlassian.Jira.Jira jiraClient, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _jiraClient = jiraClient;
        }

        public async Task<int> GetTotalIssueCount(ICollection<string> projectIds, IssueId resumeAfterId = null)
        {
            var jql = BuildSearchJql(projectIds, resumeAfterId);
            var page = await _jiraClient.Issues.GetIssuesFromJqlAsync(jql, startAt: 0, maxIssues: 1, token: _cancellationToken);
            return page.TotalItems;
        }

        public IEnumerable<IssueId> GetIssueIdsByProject(string projectId,
            IssueId resumeAfterId = null,
            int? batchSize = null)
        {
            Logger.Debug("Begin JiraRestApi.GetIssueIds {params}", new { projectIds = projectId.ToOrderedCsv(), resumeAfterId, batchSize });

            batchSize = batchSize ?? 100;

            var jql = BuildSearchJql(new[] { projectId }, resumeAfterId);

            var currentStart = 0;
            var pageCount = 0;
            List<IssueId> ids;

            do
            {
                var page = _jiraClient.Issues.GetIssuesFromJqlAsync(jql, startAt: currentStart, maxIssues: batchSize, token: _cancellationToken).Result;
                ids = page.Select(p => new IssueId(p.Key.Value)).ToList();
                ids.Sort();

                pageCount++;
                var totalPages = (page.TotalItems / batchSize) + 1; // plus 1 because it's unlikely the remainder is 0

                Logger.Debug($"Fetched page {pageCount} of {totalPages}");
                Logger.Trace("Returned ids: {ids}", ids);

                foreach (var id in ids)
                {
                    yield return id;
                }

                currentStart += batchSize.Value;
            } while (ids.Count >= batchSize && !_cancellationToken.IsCancellationRequested);
        }

        private static string BuildSearchJql(ICollection<string> projectIds, IssueId lastSavedId)
        {
            var jql = $"project in ({projectIds.ToCsv()})";

            if (lastSavedId != null)
            {
                jql += $" AND Id > {lastSavedId}";
            }

            jql += " ORDER BY id ASC";

            return jql;
        }
    }
}