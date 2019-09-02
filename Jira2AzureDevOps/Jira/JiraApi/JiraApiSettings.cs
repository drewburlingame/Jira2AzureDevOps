using CommandDotNet;
using Jira2AzureDevOps.Framework;
using Jira2AzureDevOps.Framework.CommandDotNet;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Jira.JiraApi
{
    public class JiraApiSettings : ISelfValidatingArgumentModel
    {
        [Option(Description = "Jira username")]
        public string JiraUsername { get; set; }

        [Option(Description = "Jira API Token")]
        public Password JiraToken { get; set; }

        [Option(Description = "Jira URL")]
        public string JiraUrl { get; set; }

        [Option(Description = "Batch size used in search api")]
        public int JiraBatchSize { get; set; } = 100;

        [Option(Description = "Uses only local cache and does not query Jira.")]
        public bool JiraOffline { get; set; }

        [Option(LongName = "jira-force", Description = "Always fetched data from the API and overwrite local cache")]
        public bool ForceRefresh { get; set; }

        public IEnumerable<string> GetValidationErrors()
        {
            if (JiraOffline)
            {
                if (ForceRefresh) yield return "force and offline cannot be specified together";
            }
            else
            {
                if (JiraUsername.IsNullOrWhiteSpace()) yield return "jira-username is required";
                if (JiraToken == null || JiraToken.Value.IsNullOrWhiteSpace()) yield return "jira-token is required";
                if (JiraUrl.IsNullOrWhiteSpace()) yield return "ado-url is required";
            }
        }
    }
}