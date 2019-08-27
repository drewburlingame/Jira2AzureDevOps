using System;

namespace Jira2AzureDevOps.Framework
{
    internal static class StringExtensions
    {
        internal static bool IsNullOrWhiteSpace(this string text) => string.IsNullOrWhiteSpace(text);

        internal static string[] Split(this string text, string separator,
            StringSplitOptions options = StringSplitOptions.None) =>
            text?.Split(new[] { separator }, options);
    }
}