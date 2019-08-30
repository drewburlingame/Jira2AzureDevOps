using System;
using Newtonsoft.Json.Linq;

namespace Jira2AzureDevOps.Framework.Json
{
    internal static class JTokenExtensions
    {
        internal static void WalkNode(this JToken node, Action<JObject> action)
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

        internal static bool TryGetJiraPageCounts(this JObject jObject, out int maxResults, out int total)
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