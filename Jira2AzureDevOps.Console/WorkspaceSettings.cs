using CommandDotNet;
using Jira2AzureDevOps.Logic;
using System.IO;

namespace Jira2AzureDevOps.Console
{
    public class WorkspaceSettings : IArgumentModel, IWorkspaceSettings
    {
        [Option(ShortName = "W", LongName = "workspace", Description = "Where api responses and file downloads are stored.", Inherited = true)]
        public string WorkspaceDir { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "jira-cache");
    }
}