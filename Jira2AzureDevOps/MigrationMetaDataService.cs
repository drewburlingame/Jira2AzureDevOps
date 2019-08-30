using System;
using System.Collections.Generic;
using System.Linq;
using Jira2AzureDevOps.Framework;
using Jira2AzureDevOps.Framework.Json;
using Jira2AzureDevOps.Jira;
using Jira2AzureDevOps.Jira.Model;
using Newtonsoft.Json.Linq;
using NLog;

namespace Jira2AzureDevOps
{
    public class MigrationMetaDataService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly JiraContext _jiraContext;
        private readonly MigrationRepository _migrationRepository;

        public MigrationMetaDataService(JiraContext jiraContext)
        {
            _jiraContext = jiraContext;
            _migrationRepository = new MigrationRepository(jiraContext.LocalDirs);
        }

        public void UpdateMigrationMetaData(IssueMigration migration)
        {
            IssueId issueId = migration.IssueId;
            try
            {
                // waiting on results prevents overwhelming Jira API resulting in 503's
                var issueData = _jiraContext.Api.GetIssue(issueId).Result;

                if (issueData == null)
                {
                    Logger.Error($"Failed to retrieve json data for {issueId}", issueId);
                    return;
                }

                var issue = issueData.ToObject<Issue>();
                migration.IssueType = issue.Fields.IssueType.Name;
                migration.Status = issue.Fields.Status.Name;
                migration.StatusCategory = issue.Fields.Status.StatusCategory.Name;
                var attachments = issue.Fields.Attachments;

                if (!issue.ChangeLog.Histories.Any())
                {
                    Logger.Warn("History missing for {issueId}", issueId);
                }

                UpdateAttachmentMigrationMetaData(migration, attachments);

                migration.ExportCompleted = true;
                _migrationRepository.Save(migration);

                AlertIfPartialPagedData(issueId, issueData);
            }
            catch (Exception e)
            {
                e.Data["IssueId"] = issueId.ToString();
                throw;
            }
        }

        private void UpdateAttachmentMigrationMetaData(IssueMigration migration, List<Attachment> attachments)
        {
            var attachmentsById =
                migration.Attachments.ToDictionary(a =>
                    Int64.Parse(_jiraContext.LocalDirs.GetAttachmentIdFromPath(a.File)));
            foreach (var attachment in attachments)
            {
                var attachmentMigration = attachmentsById.GetOrAdd(attachment.Id, key => new AttachmentMigration());
                if (!attachmentMigration.Imported || !attachmentMigration.File.EndsWith(attachment.Filename))
                {
                    // if the item wasn't imported OR if a different file name is used
                    // ... I don't think can happen, but... just in case
                    attachmentMigration.Imported = false;
                    var attachmentFile = _jiraContext.Api.GetAttachment(attachment).Result;
                    var relativePath = _jiraContext.LocalDirs.GetRelativePath(attachmentFile);
                    attachmentMigration.File = relativePath;
                }
            }
        }

        private void AlertIfPartialPagedData(IssueId issueId, JToken issueData)
        {
            issueData.WalkNode(o =>
            {
                if (o.TryGetJiraPageCounts(out int maxResults, out int total))
                {
                    if (o.Path == "changelog" || o.Path.EndsWith(".worklog", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: when we import history, log as Error
                        Logger.Debug("Pages are missing for {issueId} {page}", issueId,
                            new {o.Path, maxResults, total});
                    }
                    else
                    {
                        Logger.Error("Pages are missing for {issueId} {page}", issueId,
                            new {o.Path, maxResults, total});
                    }
                }
            });
        }

        private IEnumerable<Attachment> GetRemovedAttachments(Issue issue)
        {
            return issue.ChangeLog.Histories.SelectMany(h =>
                    h.Items
                        .Where(i => i.Field == "Attachment" && i.RemovedId != null)
                        .Select(i => i.RemovedId))
                .Select(id => _jiraContext.Api.GetAttachmentMetadata(id).Result)
                .Select(j => j.ToObject<Attachment>())
                .ToList();
        }

    }
}