using CommandDotNet;
using CommandDotNet.Rendering;
using Jira2AzureDevOps.Console.Framework;
using Jira2AzureDevOps.Logic;
using Jira2AzureDevOps.Logic.Jira;
using Jira2AzureDevOps.Logic.Jira.Model;
using Jira2AzureDevOps.Logic.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Console.Jira
{
    [Command(Name = "report", Description = "Commands to summarize Jira data to aid in creating mapping files. Options listed are for all report subcommands.")]
    public class JiraReportCommands
    {
        private readonly IConsole _console;

        private JiraContext _jiraContext;
        private MigrationRepository _migrationRepository;

        public JiraReportCommands(IConsole console)
        {
            _console = console;
        }

        public Task<int> Interceptor(
            CommandContext commandContext, Func<CommandContext, Task<int>> next,
            JiraApiSettings jiraApiSettings, WorkspaceSettings workspaceSettings)
        {
            _jiraContext = new JiraContext(jiraApiSettings, workspaceSettings, commandContext.AppConfig.CancellationToken);
            _migrationRepository = new MigrationRepository(_jiraContext.LocalDirs);
            return next(commandContext);
        }

        [DisableConsoleLogging]
        [Command(Description = "prints all exported project keys and the count of exported issues for each")]
        public void ExportedProjects()
        {
            var projects = _jiraContext.LocalJiraApi.ExportedProjectKeys().ToList();

            var maxName = projects.Max(e => e.Key.Length);
            foreach (var project in projects.OrderBy(p => p.Key))
            {
                _console.Out.WriteLine(string.Format($"{{0, -{maxName + 1}}} = {{1}}", project.Key, project.Count));
            }
        }

        [DisableConsoleLogging]
        [Command(Description = "generates mapping records in csv format to kick start the mapping process. " +
                               "run with '> {filename}.csv' to pipe the output to a mapping file")]
        public void StubStatusMapping(
            ProjectFilter projectFilter,
            [Option(ShortName = "H", LongName = "print-headers")]
            bool printHeaders = false,
            [Option(ShortName = "p", LongName = "print-project")]
            bool printProject = false,
            [Option(ShortName = "t", LongName = "print-issue-type")]
            bool printIssueType = false,
            [Option(ShortName = "c", LongName = "print-category")]
            bool printCategory = false,
            [Option(ShortName = "s", LongName = "print-status")]
            bool printStatus = false,
            [Option(ShortName = "i", LongName = "print-issue-count")]
            bool printIssueCount = false,
            [Option(ShortName = "x", LongName = "exportedOnly", Description = "When specified, only statuses existing for exported items will be printed")]
            bool exportedOnly = false)
        {
            var somethingIsPrinting = new[] { printProject, printIssueType, printCategory, printStatus }.Any(p => p);
            if (!somethingIsPrinting)
            {
                _console.Out.WriteLine("Select an element to print: -ptcs");
                return;
            }

            string Row(string project, string issueType, string category, string status)
            {
                var row = $"{(printProject ? $"{project}," : null)}" +
                          $"{(printIssueType ? $"{issueType}," : null)}" +
                          $"{(printCategory ? $"{category}," : null)}" +
                          $"{(printStatus ? $"{status}," : null)}";
                return row.Substring(0, row.Length - 1);
            }
            if (printHeaders)
            {
                _console.Out.Write(Row(
                    MappingConstants.FileHeaders.Project,
                    MappingConstants.FileHeaders.IssueType,
                    MappingConstants.FileHeaders.StatusCategory,
                    MappingConstants.FileHeaders.Status));
                _console.Out.Write(printIssueCount ? $",{MappingConstants.FileHeaders.IssueCount}," : ",");
                _console.Out.WriteLine(MappingConstants.FileHeaders.WorkItemStatus);
            }

            var statuses = exportedOnly
            ? GetStatusesFromExported(projectFilter)
            : GetStatusesFromMetadata(projectFilter);

            var rows = statuses
                .Select(s => Row(s.project, s.issueType, s.category, s.status))
                .ToLookup(r => r)
                .OrderBy(r => r.Key);
            foreach (var row in rows)
            {
                _console.Out.Write(row.Key);
                _console.Out.WriteLine(printIssueCount ? $",{row.Count()}," : null);
            }
        }

        [DisableConsoleLogging]
        [Command(Description = "generates mapping records in csv format to kick start the mapping process. " +
                               "run with '> {filename}.csv' to pipe the output to a mapping file")]
        public void StubIssueTypesMapping(
            ProjectFilter projectFilter,
            [Option(ShortName = "H", LongName = "print-headers")]
            bool printHeaders = false,
            [Option(ShortName = "p", LongName = "print-project")]
            bool printProject = false,
            [Option(ShortName = "t", LongName = "print-issue-type")]
            bool printIssueType = false,
            [Option(ShortName = "d", LongName = "print-description")]
            bool printDescription = false,
            [Option(ShortName = "i", LongName = "print-issue-count")]
            bool printIssueCount = false,
            [Option(ShortName = "x", LongName = "exportedOnly", Description = "When specified, only issue types existing for exported items will be printed")]
            bool exportedOnly = false)
        {
            var somethingIsPrinting = new[] { printProject, printIssueType, printDescription }.Any(p => p);
            if (!somethingIsPrinting)
            {
                _console.Out.WriteLine("Select an element to print: -ptd");
                return;
            }

            string Row(string project, string issueType, string description)
            {
                var row = $"{(printProject ? $"{project}," : null)}" +
                          $"{(printIssueType ? $"{issueType}," : null)}" +
                          $"{(printDescription ? $"{description}," : null)}";
                return row.Substring(0, row.Length - 1);
            }
            if (printHeaders)
            {
                _console.Out.Write(Row(MappingConstants.FileHeaders.Project, MappingConstants.FileHeaders.IssueType, MappingConstants.FileHeaders.IssueTypeDescription));
                _console.Out.Write(printIssueCount ? $",{MappingConstants.FileHeaders.IssueCount}," : ",");
                _console.Out.WriteLine(MappingConstants.FileHeaders.WorkItemType);
            }

            var issueTypes = exportedOnly
                ? GetIssueTypesFromExported(projectFilter)
                : GetIssueTypesFromMetadata(projectFilter);

            var rows = issueTypes
                .Select(s => Row(s.project, s.issueType, s.description))
                .ToLookup(r => r)
                .OrderBy(r => r.Key);
            foreach (var row in rows)
            {
                _console.Out.Write(row.Key);
                _console.Out.WriteLine(printIssueCount ? $",{row.Count()}," : null);
            }
        }

        private IEnumerable<(string project, string issueType, string description)> GetIssueTypesFromMetadata(ProjectFilter projectFilter)
        {
            var projects = _jiraContext.Api.GetProjects().Result
                .ToObject<List<Project>>()
                .Where(p => projectFilter.IncludesProject(p.Key));

            foreach (var project in projects)
            {
                foreach (var type in project.IssueTypes)
                {
                    yield return (project.Key, type.Name, type.Description);
                }
            }
        }

        private IEnumerable<(string project, string issueType, string description)> GetIssueTypesFromExported(ProjectFilter projectFilter)
        {
            return _jiraContext.LocalJiraApi
                .GetIssueIds(projectFilter, out _)
                .Select(_migrationRepository.Get)
                .Where(m => m != null)
                .Select(m => (m.IssueId.Project, m.IssueType, "description not available in exported view"));
        }

        private IEnumerable<(string project, string issueType, string category, string status)>
            GetStatusesFromMetadata(ProjectFilter projectFilter)
        {
            var json = _jiraContext.Api.GetStatusesByProject().Result;
            var statusesByProjects = json
                .ToObject<List<StatusesByProject>>()
                .Where(s => projectFilter.IncludesProject(s.Project));

            foreach (var project in statusesByProjects)
            {
                foreach (var type in project.StatusesByType)
                {
                    var categories = type.Statuses
                        .Select(s => (name: s.Name, cat: s.StatusCategory.Name))
                        .ToLookup(s => s.cat, s => s.name);

                    foreach (var category in categories)
                    {
                        foreach (var status in category)
                        {
                            yield return (project.Project, type.Name, category.Key, status);
                        }
                    }
                }
            }
        }

        private IEnumerable<(string project, string issueType, string category, string status)>
            GetStatusesFromExported(ProjectFilter projectFilter)
        {
            return _jiraContext.LocalJiraApi
                .GetIssueIds(projectFilter, out _)
                .Select(_migrationRepository.Get)
                .Where(m => m != null)
                .Select(m => (m.IssueId.Project, m.IssueType, m.StatusCategory, m.Status));
        }
    }
}