﻿using CommandDotNet;
using Jira2AzureDevOps.Framework;
using Jira2AzureDevOps.Jira;
using Jira2AzureDevOps.Jira.JiraApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jira2AzureDevOps.Jira.ArgumentModels;
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
        private MigrationMetaDataService _migrationMetaDataService;

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
            _migrationMetaDataService = new MigrationMetaDataService(_jiraContext);

            return next(commandContext);
        }

        [Command(Description = "Resets the migration status for the given issue(s)")]
        public void ResetMigrations(
            [Option(ShortName = "d", LongName = "delete-from-azure", Description = "Removes the work item(s) from Azure DevOps")]
            bool deleteFromAzure,
            List<IssueId> issueIds)
        {
            issueIds.EnumerateOperation(issueIds.Count, issueId => 
            {
                try
                {
                    var migration = _migrationRepository.Get(issueId);
                    if (deleteFromAzure)
                    {
                        _adoContext.Api.Delete(migration);
                    }
                    _migrationRepository.Reset(migration);
                    Logger.Info("Reset migration for {issueId}", issueId);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Failed to reset {issueId}", issueId);
                }
            });
        }

        [Command(Description = "Imports the issues for the given Jira project(s) to Azure DevOps")]
        public void ImportAll(ProjectFilter projectFilter,
            [Option(ShortName = "f", LongName = "force", Description = "if the item has already been imported, it will be deleted and reimported.")]
            bool force,
            FileInfo issueTypeMappingFile,
            FileInfo statusMappingFile)
        {
            var allMigrations = _jiraContext.Api
                .GetIssueIds(projectFilter, out int totalCount)
                .Select(_migrationMetaDataService.Get);

            ImportMigrations(force, issueTypeMappingFile, statusMappingFile, totalCount, allMigrations);
        }

        [Command(Description = "Imports the given issue(s) to Azure DevOps")]
        public void ImportById(
            [Option(ShortName = "f", LongName = "force", Description = "if the item has already been imported, it will be deleted and reimported.")] 
            bool force,
            FileInfo issueTypeMappingFile,
            FileInfo statusMappingFile,
            List<IssueId> issueIds)
        {
            if (issueIds.IsNullOrEmpty())
            {
                Console.Out.WriteLine("no issue ids provided");
                return;
            }

            var issueMigrations = issueIds.Select(_migrationMetaDataService.Get).ToList();

            var pendingMigrations = issueMigrations.Where(m => !m.ImportComplete).ToList();
            if (!force && pendingMigrations.Count < issueMigrations.Count)
            {
                var migratedIssueIds = issueMigrations.Where(m => m.ImportComplete).ToOrderedCsv();

                Console.Out.WriteLine($"Migrations already imported {migratedIssueIds}");
                Console.Out.WriteLine("continue without them   : y");
                Console.Out.WriteLine("delete and reimport them: f");
                var input = Console.In.ReadLine();
                if (!"f".Equals(input, StringComparison.OrdinalIgnoreCase))
                {
                    force = true;
                }
                else if (!"y".Equals(input, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            ImportMigrations(force, issueTypeMappingFile, statusMappingFile, issueMigrations.Count, issueMigrations);
        }

        private void ImportMigrations(
            bool force, 
            FileInfo issueTypeMappingFile, 
            FileInfo statusMappingFile,
            int totalCount,
            IEnumerable<IssueMigration> issueMigrations)
        {
            if (!issueTypeMappingFile.Exists)
            {
                Console.Out.WriteLine("issue type mapping file not found:" + issueTypeMappingFile.FullName);
                return;
            }
            if (!statusMappingFile.Exists)
            {
                Console.Out.WriteLine("status mapping file not found:" + statusMappingFile.FullName);
                return;
            }

            var statusMapper = new StatusCsvMapper(statusMappingFile);
            statusMapper.LoadMap();
            var issueTypeCsvMapper = new IssueTypeCsvMapper(issueTypeMappingFile);
            issueTypeCsvMapper.LoadMap();

            var importer = new WorkItemImporter(force, _migrationRepository, 
                _adoContext, _jiraContext, 
                statusMapper, issueTypeCsvMapper);

            int imported = 0;
            int errored = 0;

            issueMigrations.EnumerateOperation(totalCount, migration =>
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
                
                try
                {
                    if (importer.TryImport(migration))
                    {
                        imported++;
                    }
                }
                catch (Exception e)
                {
                    errored++;
                    Logger.Error(e, "Failed to import {issueId}", migration.IssueId);
                }
            });

            Logger.Info(new {imported, errored});
        }
    }
}
