using System;
using System.Collections.Generic;

namespace Jira2AzureDevOps.Framework
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TKey, TValue> createValue)
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = createValue(key);
                dictionary.Add(key, value);
            }

            return value;
        }
    }
}