using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Jira2AzureDevOps.Framework
{
    internal static class EnumerableExtensions
    {
        internal static bool IsNullOrEmpty<T>(this IEnumerable<T> items) => items == null || !items.Any();

        internal static IEnumerable<T> ToEnumerable<T>(this T instance)
        {
            yield return instance;
        }

        public static string ToCsv(this IEnumerable<string> items, string separator = ",") => string.Join(separator, items);

        public static string ToCsv<T>(this IEnumerable<T> items, string separator = ",") => items.Select(i => i?.ToString()).ToCsv(separator);

        internal static string ToCsv(this IEnumerable items, string separator = ",") => items.Cast<object>().ToCsv(separator);

        /// <summary>Joins the string into a sorted delimited string. Useful for logging.</summary>
        public static string ToOrderedCsv(this IEnumerable<string> items, string separator = ",") => items.OrderBy(i => i).ToCsv(separator);

        /// <summary>Joins the object.ToString() into a sorted delimited string. Useful for logging.</summary>
        public static string ToOrderedCsv<T>(this IEnumerable<T> items, string separator = ",") => items.Select(i => i?.ToString()).ToOrderedCsv(separator);

        internal static string ToOrderedCsv(this IEnumerable items, string separator = ",") => items.Cast<object>().ToOrderedCsv(separator);
    }
}