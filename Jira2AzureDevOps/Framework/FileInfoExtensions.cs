using System.Collections.Generic;
using System.IO;

namespace Jira2AzureDevOps.Framework
{
    public static class FileInfoExtensions
    {
        public static void AppendAllLines(this FileInfo file, IEnumerable<string> lines) => File.AppendAllLines(file.FullName, lines);

        public static string[] ReadAllLines(this FileInfo file) => File.ReadAllLines(file.FullName);
        public static string ReadAllText(this FileInfo file) => File.ReadAllText(file.FullName);
        public static void WriteAllLines(this FileInfo file, string[] contents) => File.WriteAllLines(file.FullName, contents);
        public static void WriteAllText(this FileInfo file, string contents) => File.WriteAllText(file.FullName, contents);
    }
}