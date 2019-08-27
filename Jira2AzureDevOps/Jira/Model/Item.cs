using Newtonsoft.Json;

namespace Jira2AzureDevOps.Jira.Model
{
    public class Item
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("fieldtype")]
        public string Fieldtype { get; set; }

        [JsonProperty("fieldId", NullValueHandling = NullValueHandling.Ignore)]
        public string FieldId { get; set; }

        [JsonProperty("from")]
        public string RemovedId { get; set; }

        [JsonProperty("fromString")]
        public string RemovedName { get; set; }

        [JsonProperty("to")]
        public string AddedId { get; set; }

        [JsonProperty("toString")]
        public string AddedName { get; set; }
    }
}