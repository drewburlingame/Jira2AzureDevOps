using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Jira.Model
{
    public class IssueFields
    {
        [JsonProperty("issuetype")]
        public IssueType IssueType { get; set; }
        
        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("attachment")]
        public List<Attachment> Attachments { get; set; }

        [JsonProperty("comment")]
        public ChangeLog Comment { get; set; }

        [JsonProperty("issuelinks")]
        public List<IssueLink> IssueLinks { get; set; }
    }
}