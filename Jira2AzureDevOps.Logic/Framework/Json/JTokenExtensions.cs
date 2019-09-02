using Newtonsoft.Json.Linq;
using System;

namespace Jira2AzureDevOps.Logic.Framework.Json
{
    public static class JTokenExtensions
    {
        public static void WalkNode(this JToken node, Action<JObject> action)
        {
            if (node.Type == JTokenType.Object)
            {
                action((JObject)node);

                foreach (JProperty child in node.Children<JProperty>())
                {
                    WalkNode(child.Value, action);
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                foreach (JToken child in node.Children())
                {
                    WalkNode(child, action);
                }
            }
        }

        public static bool TryGetJiraPageCounts(this JObject jObject, out int maxResults, out int total)
        {
            if (jObject.ContainsKey("maxResults") && jObject.ContainsKey("total"))
            {
                maxResults = jObject.Value<int>("maxResults");
                total = jObject.Value<int>("total");
                return total > maxResults;
            }

            maxResults = 0;
            total = 0;
            return false;
        }
    }
}