﻿using Jira2AzureDevOps.Logic.Jira;
using System.Collections.Generic;
using System.Linq;

namespace Jira2AzureDevOps.Logic.Migrations
{
    public class IssueMigration
    {
        public IssueId IssueId { get; set; }
        public string IssueType { get; set; }
        public string Status { get; set; }
        public string StatusCategory { get; set; }
        public List<AttachmentMigration> Attachments { get; set; } = new List<AttachmentMigration>();

        public bool ExportCompleted { get; set; }

        public bool ImportComplete => IssueImported && (Attachments?.All(a => a.Imported) ?? true);
        public bool IssueImported { get; set; }
        public int WorkItemId { get; set; }
        public int TempWorkItemId { get; set; }
    }
}