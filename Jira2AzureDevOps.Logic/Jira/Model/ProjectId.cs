using Jira2AzureDevOps.Logic.Framework.Json;
using Newtonsoft.Json;

namespace Jira2AzureDevOps.Logic.Jira.Model
{
    public partial class ProjectId
    {
        [JsonProperty("id")]
        [JsonConverter(typeof(LongConverter))]
        public long Id { get; set; }
    }
}