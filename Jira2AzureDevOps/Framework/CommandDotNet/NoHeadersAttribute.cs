using System;

namespace Jira2AzureDevOps.Framework.CommandDotNet
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NoHeadersAttribute : Attribute { }
}