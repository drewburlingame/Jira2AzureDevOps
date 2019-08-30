using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Jira2AzureDevOps.Framework;
using NLog;

namespace Jira2AzureDevOps
{
    internal static class ConsoleEnumerator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static void EnumerateOperation<T>(this IEnumerable<T> items, int totalCount, Action<T> action)
        {
            var etaCalculator = new EtaCalculator(5, maximumDuration: TimeSpan.FromMinutes(2).Ticks, totalCount);
            var totalTime = Stopwatch.StartNew();
            var count = 0;

            foreach (var item in items.TakeWhile(i => Cancellation.IsNotRequested))
            {
                count++;
                action(item);
                etaCalculator.Increment();
                if (count % 10 == 0 && etaCalculator.TryGetEta(out var etr, out var eta))
                {
                    Logger.Info(new { count, of= totalCount, elapsed=totalTime.Elapsed, etr, eta });
                }
            }
        }
    }
}