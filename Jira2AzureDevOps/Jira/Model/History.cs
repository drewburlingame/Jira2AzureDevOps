using Jira2AzureDevOps.Framework.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Jira.Model
{
    public class History
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("author")]
        public Creator Author { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }
}