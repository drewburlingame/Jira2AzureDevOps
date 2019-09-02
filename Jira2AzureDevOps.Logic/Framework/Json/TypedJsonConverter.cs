using Newtonsoft.Json;
using System;

namespace Jira2AzureDevOps.Logic.Framework.Json
{
    public abstract class TypedJsonConverter<T> : JsonConverter<T>
    {
        protected abstract T Parse(string value);

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return default;
            var value = serializer.Deserialize<string>(reader);
            return Parse(value);
        }

        public override void WriteJson(JsonWriter writer, T untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
            }
            else
            {
                serializer.Serialize(writer, ((T)untypedValue).ToString());
            }
        }
    }
}