using CommandDotNet;
using System.Collections.Generic;
using System.Linq;
using Jira2AzureDevOps.Framework;

namespace Jira2AzureDevOps.Jira.ArgumentModels
{
    public class ProjectFilter : IArgumentModel
    {
        [Option(ShortName = "P", LongName = "projects",
            Description = "If provided, the operation is applied to only these projects")]
        public List<string> Projects { get; set; } = new List<string>
            {"APP", "ARCH", "BI", "BILL", "DEVOPS", "GN", "MS", "QA"};

        private HashSet<string> _projects;

        public bool IncludesProject(string projectKey)
        {
            if (_projects == null)
            {
                if (Projects.IsNullOrEmpty())
                {
                    _projects = new HashSet<string>();
                }
                else
                {
                    // CommandDotNet doesn't auto split comma separated values yet.
                    _projects = Projects.SelectMany(i => i.Split(",")).ToHashSet();
                }
            }
            return Projects.IsNullOrEmpty() || Projects.Contains(projectKey);
        }
    }
}