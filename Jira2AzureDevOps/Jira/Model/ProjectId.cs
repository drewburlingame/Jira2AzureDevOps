using Jira2AzureDevOps.Framework.Json;
using Newtonsoft.Json;

namespace Jira2AzureDevOps.Jira.Model
{
    public partial class ProjectId
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }
    }
}