﻿using System;
using System.IO;
using System.Linq;
using NLog;

namespace Jira2AzureDevOps.AzureDevOps
{
    public class IssueTypeCsvMapper : CsvMapperBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IssueTypeCsvMapper(FileInfo csvMapFile) : base(csvMapFile)
        {
        }

        protected override void ParseHeaders(out Func<string[], string> getValueCallback)
        {
            getValueCallback = null;
            var headerRow = Rows[0];

            for (int i = 0; i < headerRow.Length; i++)
            {
                switch (headerRow[i])
                {
                    case MappingConstants.FileHeaders.Project:
                        var i1 = i;
                        KeyBuildersFromRow.Add(l => l[i1]);
                        KeyBuildersFromMigration.Add(m => m.IssueId.Project);
                        break;
                    case MappingConstants.FileHeaders.IssueType:
                        var i2 = i;
                        KeyBuildersFromRow.Add(l => l[i2]);
                        KeyBuildersFromMigration.Add(m => m.IssueType);
                        break;
                    case MappingConstants.FileHeaders.WorkItemType:
                        var i5 = i;
                        getValueCallback = row => row[i5];
                        break;
                    case MappingConstants.FileHeaders.IssueCount:
                        break;
                    default:
                        Logger.Warn($"Unknown column header '{headerRow[i]}' in {CsvMapFile}");
                        break;
                }
            }

            if (!KeyBuildersFromRow.Any())
            {
                throw new Exception(
                    $"No source columns found in {CsvMapFile}");
            }

            if (getValueCallback == null)
            {
                throw new Exception(
                    $"Missing '{MappingConstants.FileHeaders.WorkItemStatus}' column in {CsvMapFile}");
            }
        }
    }
}