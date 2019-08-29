using Jira2AzureDevOps.Framework;
using Jira2AzureDevOps.Jira;
using Jira2AzureDevOps.Jira.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jira2AzureDevOps
{
    public class LocalDirs
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DirectoryInfo Root { get; }
        public DirectoryInfo Issues { get; }
        public DirectoryInfo Attachments { get; }
        public DirectoryInfo Meta { get; }

        public LocalDirs(string rootDir)
        {
            if (rootDir.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(rootDir));
            }

            Logger.Info("root directory {rootDir}", rootDir);
            Root = new DirectoryInfo(rootDir);
            Issues = new DirectoryInfo(Path.Combine(rootDir, "issues"));
            Attachments = new DirectoryInfo(Path.Combine(rootDir, "attachments"));
            Meta = new DirectoryInfo(Path.Combine(rootDir, "meta"));

            Root.FullName.EnsureDirectoryExists();
            Issues.FullName.EnsureDirectoryExists();
            Attachments.FullName.EnsureDirectoryExists();
            Meta.FullName.EnsureDirectoryExists();
        }

        public string GetFullPath(string relativePath) =>
            Path.Combine(Root.FullName, relativePath.StartsWith(@"\") ? relativePath.Substring(1) : relativePath);

        public string GetRelativePath(FileInfo fileInfo) => 
            fileInfo.FullName.Replace(Root.FullName, null);

        public FileInfo GetFileFromRelativePath(string relativePath) => 
            new FileInfo(GetFullPath(relativePath));

        public DirectoryInfo GetIssueDir(IssueId issueId) =>
            new DirectoryInfo(Path.Combine(Issues.FullName, issueId.ToString()))
                .EnsureExists();

        public FileInfo GetIssueJsonFile(IssueId issueId) =>
            new FileInfo(Path.Combine(GetIssueDir(issueId).FullName, "issue.json"));

        public FileInfo GetIssueMigrationStatusFile(IssueId issueId) =>
            new FileInfo(Path.Combine(GetIssueDir(issueId).FullName, "migration-status.json"));

        public IEnumerable<FileInfo> GetAllIssueMigrationStatusFiles() => Directory
            .GetFiles(Issues.FullName, "migration-status.json", SearchOption.AllDirectories)
            .Select(p => new FileInfo(p));

        public DirectoryInfo GetAttachmentsDir(string attachmentId) =>
            new DirectoryInfo(Path.Combine(Attachments.FullName, attachmentId))
                .EnsureExists();

        public FileInfo GetAttachmentMetadataFile(string attachmentId) =>
            new FileInfo(Path.Combine(GetAttachmentsDir(attachmentId).FullName, "attachment.json"));

        public FileInfo GetAttachmentFile(Attachment attachment) =>
            new FileInfo(Path.Combine(GetAttachmentsDir(attachment.Id.ToString()).FullName, attachment.Filename));

        public FileInfo GetIssueFieldsFile() =>
        new FileInfo(Path.Combine(Meta.FullName, "IssueFields.json"));

        public FileInfo GetIssueLinkTypesFile() =>
            new FileInfo(Path.Combine(Meta.FullName, "IssueLinkTypes.json"));

        public FileInfo GetIssuePrioritiesFile() =>
            new FileInfo(Path.Combine(Meta.FullName, "IssuePriorities.json"));

        public FileInfo GetIssueResolutionsFile() =>
            new FileInfo(Path.Combine(Meta.FullName, "IssueResolutions.json"));

        public FileInfo GetIssueTypesFile() =>
            new FileInfo(Path.Combine(Meta.FullName, "IssueTypes.json"));

        public FileInfo GetLabelsFile() =>
            new FileInfo(Path.Combine(Meta.FullName, "IssueLabels.txt"));

        public FileInfo GetProjectsFile() =>
            new FileInfo(Path.Combine(Meta.FullName, "Projects.json"));

        public FileInfo GetStatusesFile() =>
            new FileInfo(Path.Combine(Meta.FullName, "Statuses.json"));
    }
}