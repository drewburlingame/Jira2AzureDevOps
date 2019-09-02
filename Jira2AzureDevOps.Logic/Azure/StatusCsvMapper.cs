using NLog;
using System;
using System.IO;
using System.Linq;

namespace Jira2AzureDevOps.Logic.Azure
{
    public class StatusCsvMapper : CsvMapperBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public StatusCsvMapper(FileInfo csvMapFile) : base(csvMapFile)
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
                    case MappingConstants.FileHeaders.StatusCategory:
                        var i3 = i;
                        KeyBuildersFromRow.Add(l => l[i3]);
                        KeyBuildersFromMigration.Add(m => m.StatusCategory);
                        break;
                    case MappingConstants.FileHeaders.Status:
                        var i4 = i;
                        KeyBuildersFromRow.Add(l => l[i4]);
                        KeyBuildersFromMigration.Add(m => m.Status);
                        break;
                    case MappingConstants.FileHeaders.WorkItemStatus:
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