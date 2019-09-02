using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Logic.Jira.Model
{
    public partial class ChangeLog : IPagedCollection
    {
        [JsonProperty("startAt")]
        public long StartAt { get; set; }

        [JsonProperty("maxResults")]
        public long MaxResults { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("histories", NullValueHandling = NullValueHandling.Ignore)]
        public List<History> Histories { get; set; }
    }
}