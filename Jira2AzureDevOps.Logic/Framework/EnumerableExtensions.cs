using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Jira2AzureDevOps.Logic.Framework
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items) => items == null || !items.Any();

        public static IEnumerable<T> ToEnumerable<T>(this T instance)
        {
            yield return instance;
        }

        public static ICollection<T> ToCollection<T>(this IEnumerable<T> items) => items as ICollection<T> ?? items.ToList();

        public static string ToCsv(this IEnumerable<string> items, string separator = ",") => string.Join(separator, items);

        public static string ToCsv<T>(this IEnumerable<T> items, string separator = ",") => items.Select(i => i?.ToString()).ToCsv(separator);

        public static string ToCsv(this IEnumerable items, string separator = ",") => items.Cast<object>().ToCsv(separator);

        /// <summary>Joins the string into a sorted delimited string. Useful for logging.</summary>
        public static string ToOrderedCsv(this IEnumerable<string> items, string separator = ",") => items.OrderBy(i => i).ToCsv(separator);

        /// <summary>Joins the object.ToString() into a sorted delimited string. Useful for logging.</summary>
        public static string ToOrderedCsv<T>(this IEnumerable<T> items, string separator = ",") => items.Select(i => i?.ToString()).ToOrderedCsv(separator);

        public static string ToOrderedCsv(this IEnumerable items, string separator = ",") => items.Cast<object>().ToOrderedCsv(separator);
    }
}