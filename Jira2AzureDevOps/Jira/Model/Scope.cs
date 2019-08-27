using Newtonsoft.Json;

namespace Jira2AzureDevOps.Jira.Model
{
    public partial class Scope
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("project")]
        public ProjectId ProjectId { get; set; }
    }
}