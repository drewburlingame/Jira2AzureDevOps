using CommandDotNet;
using System.IO;

namespace Jira2AzureDevOps
{
    public class WorkspaceSettings : IArgumentModel
    {
        [Option(ShortName = "W", LongName = "workspace", Description = "Where api responses and file downloads are stored.", Inherited = true)]
        public string WorkspaceDir { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "jira-cache");
    }
}