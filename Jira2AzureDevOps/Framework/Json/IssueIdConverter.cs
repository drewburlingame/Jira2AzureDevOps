using System;
using Jira2AzureDevOps.Jira;
using Newtonsoft.Json;

namespace Jira2AzureDevOps.Framework.Json
{
    internal class IssueIdConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(IssueId);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            return new IssueId(value);
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (IssueId)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }
    }
}