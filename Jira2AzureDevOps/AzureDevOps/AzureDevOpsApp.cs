using CommandDotNet;
using Jira2AzureDevOps.Framework;
using Jira2AzureDevOps.Jira;
using Jira2AzureDevOps.Jira.JiraApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace Jira2AzureDevOps.AzureDevOps
{
    [Command(Name = "azure", Description = "Azure DevOps commands")]
    class AzureDevOpsApp
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private MigrationRepository _migrationRepository;
        private JiraContext _jiraContext;
        private AdoContext _adoContext;

        public Task<int> Intercept(
            CommandContext commandContext, Func<CommandContext, Task<int>> next,
            AdoApiSettings adoApiSettings, WorkspaceSettings workspaceSettings, JiraApiSettings jiraApiSettings)
        {
            _adoContext = new AdoContext(adoApiSettings);
            if (!_adoContext.TryConnect())
            {
                Console.Out.WriteLine("Unable to connect to TFS");
                return Task.FromResult(1);
            }
            _jiraContext = new JiraContext(jiraApiSettings, workspaceSettings);
            _migrationRepository = new MigrationRepository(_jiraContext.LocalDirs);

            return next(commandContext);
        }

        [Command(Description = "Imports the issue(s) for the given id(s)")]
        public void ImportById(
            [Option(ShortName = "f", LongName = "force", Description = "if the item has already been imported, it will be deleted and reimported.")] 
            bool force,
            FileInfo issueTypeMappingFile,
            FileInfo statusMappingFile,
            List<IssueId> issueIds)
        {
            if (!issueTypeMappingFile.Exists)
            {
                Console.Out.WriteLine("issue type mapping file not found:" + issueTypeMappingFile.FullName);
            }
            if (!statusMappingFile.Exists)
            {
                Console.Out.WriteLine("status mapping file not found:" + statusMappingFile.FullName);
            }
            if (issueIds.IsNullOrEmpty())
            {
                Console.Out.WriteLine("no issue ids provided");
                return;
            }

            var issueMigrations = issueIds.Select(id => _migrationRepository.Get(id)).Where(m => m != null).ToList();
            if (issueMigrations.Count < issueIds.Count)
            {
                var missingIssueIds = issueIds.Except(issueMigrations.Select(m => m.IssueId)).ToOrderedCsv();

                Console.Out.WriteLine($"Migrations were not found for {missingIssueIds}");
                Console.Out.WriteLine("continue without them: y");
                var intput = Console.In.ReadLine();
                if (!"y".Equals(intput, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            var pendingMigrations = issueMigrations.Where(m => !m.ImportComplete).ToList();
            if(!force && pendingMigrations.Count < issueMigrations.Count)
            {
                var migratedIssueIds = issueMigrations.Where(m => m.ImportComplete).ToOrderedCsv();

                Console.Out.WriteLine($"Migrations already imported {migratedIssueIds}");
                Console.Out.WriteLine("continue without them   : y");
                Console.Out.WriteLine("delete and reimport them: f");
                var intput = Console.In.ReadLine();
                if (!"f".Equals(intput, StringComparison.OrdinalIgnoreCase))
                {
                    force = true;
                }
                else if (!"y".Equals(intput, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            var statusMapper = new StatusCsvMapper(statusMappingFile);
            statusMapper.LoadMap();
            var issueTypeCsvMapper = new IssueTypeCsvMapper(issueTypeMappingFile);
            issueTypeCsvMapper.LoadMap();

            var importer = new WorkItemImporter(force, _migrationRepository, _adoContext, _jiraContext, statusMapper, issueTypeCsvMapper);

            int imported = 0;
            foreach (var migration in issueMigrations.Where(m => force || !m.ImportComplete))
            {
                if (importer.TryImport(migration))
                {
                    imported++;
                }
            }
        }
    }
}
