using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Jira.Model
{
    public class IssueFields
    {
        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }
        
        [JsonProperty("updated")]
        public DateTimeOffset Updated { get; set; }

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
        public CommentCollection Comment { get; set; }

        [JsonProperty("issuelinks")]
        public List<IssueLink> IssueLinks { get; set; }
    }
}