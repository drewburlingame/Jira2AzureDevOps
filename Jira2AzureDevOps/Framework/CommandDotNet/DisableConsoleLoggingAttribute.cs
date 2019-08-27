using System;

namespace Jira2AzureDevOps.Framework.CommandDotNet
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class DisableConsoleLoggingAttribute : Attribute { }
}