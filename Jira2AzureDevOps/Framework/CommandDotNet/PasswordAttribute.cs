using System;

namespace Jira2AzureDevOps.Framework.CommandDotNet
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public class PasswordAttribute : Attribute { }
}