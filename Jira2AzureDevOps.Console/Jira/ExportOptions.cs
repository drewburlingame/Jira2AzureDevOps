using System.IO;
using CommandDotNet;

namespace Jira2AzureDevOps.Console.Jira
{
    public class ExportOptions : IArgumentModel
    {
        [Option(Description = "The ids of all issues that fail to export will be written to this " +
                              "file which can be used as @response-file input for the export commands")]
        public FileInfo FailFile { get; set; }
    }
}