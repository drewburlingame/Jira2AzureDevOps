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
    }
}