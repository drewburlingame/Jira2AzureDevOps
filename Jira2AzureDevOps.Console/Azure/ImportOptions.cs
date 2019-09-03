using CommandDotNet;
using System.IO;

namespace Jira2AzureDevOps.Console.Azure
{
    public class ImportOptions : IArgumentModel
    {
        [Option(ShortName = "f", LongName = "force",
            Description = "if the item has already been imported, it will be deleted and reimported.")]
        public bool Force { get; set; }

        [Option(ShortName = "t", LongName = "issue-type-mappings",
            Description = "CSV file containing mappings from Jira issue types to Azure DevOps issue types")]
        public FileInfo IssueTypeMappingFile { get; set; }

        [Option(ShortName = "s", LongName = "status-mappings",
            Description = "CSV file containing mappings from Jira statuses to Azure DevOps statuses")]
        public FileInfo StatusMappingFile { get; set; }

        [Option(Description = "The ids of all issues that fail to import will be written to this " +
                              "file which can be used as @response-file input for the import-by-id command")]
        public FileInfo FailFile { get; set; }
    }
}