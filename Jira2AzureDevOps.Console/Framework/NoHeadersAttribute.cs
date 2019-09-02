using System;

namespace Jira2AzureDevOps.Console.Framework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NoHeadersAttribute : Attribute { }
}