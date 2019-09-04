using CommandDotNet;
using Jira2AzureDevOps.Logic.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Jira2AzureDevOps.Console.Jira
{
    public class ProjectFilter : IArgumentModel
    {
        [Option(ShortName = "P", LongName = "projects",
            Description = "If provided, the operation is applied to only these projects")]
        public List<string> Projects { get; set; }

        private HashSet<string> _projects;

        public bool IncludesProject(string projectKey)
        {
            if (_projects == null)
            {
                _projects = Projects.IsNullOrEmpty()
                    ? new HashSet<string>()
                    : Projects.SelectMany(i => StringExtensions.Split(i, ",")).ToHashSet();
            }
            return Projects.IsNullOrEmpty() || Projects.Contains(projectKey);
        }
    }
}