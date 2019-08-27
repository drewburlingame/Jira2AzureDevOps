using Jira2AzureDevOps.Framework.Json;
using Newtonsoft.Json;
using System;

namespace Jira2AzureDevOps.Jira.Model
{
    public class IssueLink
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("type")]
        public LinkType Type { get; set; }

        [JsonProperty("outwardIssue", NullValueHandling = NullValueHandling.Ignore)]
        public LinkedIssue OutwardIssue { get; set; }

        [JsonProperty("inwardIssue", NullValueHandling = NullValueHandling.Ignore)]
        public LinkedIssue InwardIssue { get; set; }
    }
}