using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Jira.Model
{
    public partial class StatusesByProject
    {
        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("statusesByType")]
        public List<StatusesByType> StatusesByType { get; set; }
    }
}