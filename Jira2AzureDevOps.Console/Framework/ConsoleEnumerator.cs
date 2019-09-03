using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Jira2AzureDevOps.Logic.Framework;

namespace Jira2AzureDevOps.Console.Framework
{
    internal static class ConsoleEnumerator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static void EnumerateOperation<T>(this IEnumerable<T> items, 
            int totalCount, 
            string operationName,
            Action<T> action)
        {
            items.EnumerateOperation(new State(totalCount), operationName, null, null, action);
        }

        internal static void EnumerateOperation<T>(this IEnumerable<T> items,
            int totalCount,
            string operationName,
            Func<T, object> getId,
            Action<T> action)
        {
            items.EnumerateOperation(new State(totalCount), operationName, getId, null, action);
        }

        internal static void EnumerateOperation<T>(this IEnumerable<T> items,
            int totalCount,
            string operationName,
            FileInfo failFile,
            Action<T> action)
        {
            items.EnumerateOperation(new State(totalCount), operationName, null, failFile, action);
        }

        internal static void EnumerateOperation<T>(this IEnumerable<T> items, 
            State state, 
            string operationName,
            Func<T, object> getId,
            FileInfo failFile,
            Action<T> action)
        {
            getId = getId ?? (item => item);

            var etaCalculator = new EtaCalculator(5, maximumDuration: TimeSpan.FromMinutes(2).Ticks, state.TotalCount);
            var totalTime = Stopwatch.StartNew();

            Logger.Info("begin {operationName} for {count} items", operationName, state.TotalCount);

            foreach (var item in items.TakeWhile(i => !state.ShouldQuit && Cancellation.IsNotRequested))
            {
                var id = getId(item);
                try
                {
                    state.Processed++;
                    etaCalculator.Increment();
                    action(item);
                    Logger.Debug("processed {operationName} for {id}", operationName, id);
                }
                catch (Exception e)
                {
                    state.Errored++;
                    Logger.Error(e, "errored {operationName} for {id}", operationName, id);
                    failFile?.AppendAllLines(id.ToString().ToEnumerable());
                }
                finally
                {
                    if (state.ShouldReport())
                    {
                        if (etaCalculator.TryGetEta(out var etr, out var eta))
                        {
                            Logger.Info(new { state, elapsed = totalTime.Elapsed, etr, eta });
                        }
                        else
                        {
                            Logger.Info(new { state, elapsed = totalTime.Elapsed });
                        }
                    }
                }
            }

            Logger.Info(
                state.ShouldQuit ? "quit {operationName}" : "completed {operationName}", 
                operationName);
            if (state.Errored == 0)
            {
                failFile = null;
            }
            Logger.Info(new { state, elapsed = totalTime.Elapsed, failFile });
        }

        internal class State
        {
            private readonly int _minReportCount;
            private readonly TimeSpan _minReportTimeSpan;
            private int _nextReportCount;
            private DateTime _nextReportTime;

            public int TotalCount { get; }

            public int Processed { get; set; }
            public int Errored { get; set; }
            public int Succeeded => Processed - Errored;

            public bool ShouldQuit { get; private set; }

            public State(int totalCount, int minReportCount = 10, TimeSpan? minReportTimeSpan = null)
            {
                TotalCount = totalCount;
                _minReportCount = minReportCount;
                _minReportTimeSpan = minReportTimeSpan ?? TimeSpan.FromMinutes(1);
            }

            public void Quit() => ShouldQuit = true;

            internal bool ShouldReport()
            {
                if (Processed >= _nextReportCount || DateTime.Now >= _nextReportTime)
                {
                    Reported();
                    return true;
                }

                return false;
            }

            private void Reported()
            {
                _nextReportCount = Processed + _minReportCount;
                _nextReportTime = DateTime.Now.Add(_minReportTimeSpan);
            }
        }
    }
}