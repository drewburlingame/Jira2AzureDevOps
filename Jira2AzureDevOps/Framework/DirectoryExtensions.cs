using System.Collections.Generic;
using System.IO;

namespace Jira2AzureDevOps.Framework
{
    public static class DirectoryExtensions
    {
        public static DirectoryInfo EnsureExists(this DirectoryInfo directory)
        {
            directory.FullName.EnsureDirectoryExists();
            return directory;
        }

        public static void EnsureDirectoryExists(this string directoryPath)
        {
            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
            {
                var parentDir = Path.GetDirectoryName(directoryPath);
                EnsureDirectoryExists(parentDir);
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}