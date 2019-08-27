using Jira2AzureDevOps.Framework.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Jira.Model
{
    public partial class StatusesByType
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subtask")]
        public bool Subtask { get; set; }

        [JsonProperty("statuses")]
        public List<Status> Statuses { get; set; }
    }
}