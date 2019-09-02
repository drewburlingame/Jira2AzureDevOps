using System;

namespace Jira2AzureDevOps.Logic.Framework
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string text) => string.IsNullOrWhiteSpace(text);

        public static string[] Split(this string text, string separator,
            StringSplitOptions options = StringSplitOptions.None) =>
            text?.Split(new[] { separator }, options);
    }
}