using Jira2AzureDevOps.Framework.Json;
using Newtonsoft.Json;
using System;

namespace Jira2AzureDevOps.Jira.Model
{
    public class Issue
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("changelog")]
        public ChangeLog ChangeLog { get; set; }

        [JsonProperty("fields")]
        public IssueFields Fields { get; set; }
    }
}