using Jira2AzureDevOps.Logic.Framework.Json;
using Newtonsoft.Json;
using System;

namespace Jira2AzureDevOps.Logic.Jira.Model
{
    public class LinkType
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(LongConverter))]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("inward")]
        public string Inward { get; set; }

        [JsonProperty("outward")]
        public string Outward { get; set; }

        [JsonProperty("self")]
        public Uri Self { get; set; }
    }
}