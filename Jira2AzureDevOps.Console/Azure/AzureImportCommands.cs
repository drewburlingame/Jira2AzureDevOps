using CommandDotNet;
using CommandDotNet.Rendering;
using Jira2AzureDevOps.Console.Framework;
using Jira2AzureDevOps.Console.Jira;
using Jira2AzureDevOps.Logic.Azure;
using Jira2AzureDevOps.Logic.Framework;
using Jira2AzureDevOps.Logic.Jira;
using Jira2AzureDevOps.Logic.Migrations;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jira2AzureDevOps.Console.Azure
{
    [Command(Name = "import", Description = "Commands to import issues. Options listed are for all import subcommands.")]
    public class AzureImportCommands
    {
        private readonly IConsole _console;
        private readonly CancellationToken _cancellationToken;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private MigrationRepository _migrationRepository;
        private JiraContext _jiraContext;
        private AdoContext _adoContext;
        private MigrationMetaDataService _migrationMetaDataService;

        public AzureImportCommands(IConsole console, CancellationToken cancellationToken)
        {
            _console = console;
            _cancellationToken = cancellationToken;
        }

        public Task<int> Intercept(
            CommandContext commandContext, Func<CommandContext, Task<int>> next,
            AdoApiSettings adoApiSettings, WorkspaceSettings workspaceSettings, JiraApiSettings jiraApiSettings)
        {
            _adoContext = new AdoContext(adoApiSettings, commandContext.AppConfig.CancellationToken);
            if (!_adoContext.TryConnect())
            {
                _console.Out.WriteLine("Unable to connect to TFS");
                return Task.FromResult(1);
            }
            _jiraContext = new JiraContext(jiraApiSettings, workspaceSettings, commandContext.AppConfig.CancellationToken);
            _migrationRepository = new MigrationRepository(_jiraContext.LocalDirs);
            _migrationMetaDataService = new MigrationMetaDataService(_jiraContext);

            return next(commandContext);
        }

        [Command(Description = "Resets the import status for the given issue(s)")]
        public void Reset(
            [Option(ShortName = "d", LongName = "delete-from-azure", Description = "Removes the work item(s) from Azure DevOps")]
            bool deleteFromAzure,
            [Operand(Description = "The Jira issue id(s) of the migrations to reset (space delimited)")]
            List<IssueId> issueIds)
        {
            issueIds.EnumerateOperation(issueIds.Count, "Reset Migration", _cancellationToken, issueId =>
            {
                var migration = _migrationRepository.Get(issueId);
                if (deleteFromAzure)
                {
                    _adoContext.Api.Delete(migration);
                }
                _migrationRepository.Reset(migration);
            });
        }

        [Command(Description = "Imports the issues for the given Jira project(s) to Azure DevOps",
            ExtendedHelpText = "Import includes comments & attachments for each issue. " +
                               "Only the first page of comments are currently imported, appended to the end of the description.")]
        public void IssuesByProject(
            ImportOptions importOptions,
            ProjectFilter projectFilter,
            [Option(Description = "Resumes import after this issue")]
            IssueId resumeAfter)
        {
            var allMigrations = _jiraContext.Api.GetIssueIds(projectFilter, out int totalCount, resumeAfter)
                .Select(_migrationMetaDataService.Get);

            ImportMigrations(importOptions, totalCount, allMigrations);
        }

        [Command(Description = "Imports the given issue(s) to Azure DevOps.",
            ExtendedHelpText = "Import includes comments & attachments for each issue. " +
                               "Only the first page of comments are currently imported, appended to the end of the description.")]
        public void IssuesById(
            ImportOptions importOptions,
            [Operand(Description = "The Jira issue id(s) to import (space delimited).  Alternatively, specify a @fail-file-path to import issues that failed in a previous import.")]
            List<IssueId> issueIds)
        {
            if (issueIds.IsNullOrEmpty())
            {
                _console.Out.WriteLine("no issue ids provided");
                return;
            }

            // distinct because failure files could have issues appended more than once when reused
            var issueMigrations = issueIds.Distinct().Select(_migrationMetaDataService.Get).ToList();

            var pendingMigrations = issueMigrations.Where(m => !m.ImportComplete).ToList();
            if (!importOptions.Force && pendingMigrations.Count < issueMigrations.Count)
            {
                var migratedIssueIds = issueMigrations.Where(m => m.ImportComplete).ToOrderedCsv();

                _console.Out.WriteLine($"Migrations already imported {migratedIssueIds}");
                _console.Out.WriteLine("continue without them   : y");
                _console.Out.WriteLine("delete and reimport them: f");
                var input = _console.In.ReadLine();
                if (!"f".Equals(input, StringComparison.OrdinalIgnoreCase))
                {
                    importOptions.Force = true;
                }
                else if (!"y".Equals(input, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            ImportMigrations(importOptions, issueMigrations.Count, issueMigrations);
        }

        private void ImportMigrations(ImportOptions importOptions, int totalCount, IEnumerable<IssueMigration> issueMigrations)
        {
            var issueTypeMappingFile = importOptions.IssueTypeMappingFile;
            var statusMappingFile = importOptions.StatusMappingFile;
            if (!issueTypeMappingFile.Exists)
            {
                _console.Out.WriteLine("issue type mapping file not found:" + issueTypeMappingFile.FullName);
                return;
            }
            if (!statusMappingFile.Exists)
            {
                _console.Out.WriteLine("status mapping file not found:" + statusMappingFile.FullName);
                return;
            }

            var issueTypeCsvMapper = new IssueTypeCsvMapper(issueTypeMappingFile);
            issueTypeCsvMapper.LoadMap();
            var statusMapper = new StatusCsvMapper(statusMappingFile);
            statusMapper.LoadMap();

            ImportMigrations(
                importOptions.Force,
                importOptions.FailFile,
                issueTypeCsvMapper,
                statusMapper,
                totalCount,
                issueMigrations);
        }

        private void ImportMigrations(
            bool force,
            FileInfo failFile,
            IssueTypeCsvMapper issueTypeCsvMapper,
            StatusCsvMapper statusMapper,
            int totalCount,
            IEnumerable<IssueMigration> issueMigrations)
        {
            var importer = new WorkItemImporter(force, _migrationRepository,
                _adoContext, _jiraContext,
                statusMapper, issueTypeCsvMapper);

            int imported = 0;

            var state = new ConsoleEnumerator.State(totalCount);
            issueMigrations.EnumerateOperation(
                state,
                "Import Issue",
                migration => migration.IssueId,
                failFile,
                _cancellationToken,
                migration =>
                {

                    if (migration.ImportComplete)
                    {
                        if (force)
                        {
                            Logger.Debug("Forcing import for already imported migration {issueId}", migration.IssueId);
                        }
                        else
                        {
                            return;
                        }
                    }

                    if (importer.TryImport(migration))
                    {
                        imported++;
                    }
                });

            Logger.Info(new { imported });
        }
    }
}
