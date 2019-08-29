using System.Collections.Generic;
using Newtonsoft.Json;

namespace Jira2AzureDevOps.Jira.Model
{
    public partial class CommentCollection : IPagedCollection
    {
        [JsonProperty("startAt")]
        public long StartAt { get; set; }

        [JsonProperty("maxResults")]
        public long MaxResults { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("comments", NullValueHandling = NullValueHandling.Ignore)]
        public List<Comment> Comments { get; set; }
    }
}