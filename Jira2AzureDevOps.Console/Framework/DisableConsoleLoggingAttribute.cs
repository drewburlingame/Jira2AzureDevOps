using System;

namespace Jira2AzureDevOps.Console.Framework
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class DisableConsoleLoggingAttribute : Attribute { }
}