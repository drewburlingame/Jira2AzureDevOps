using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProgressItem = System.Collections.Generic.KeyValuePair<long, float>;

namespace Jira2AzureDevOps.Framework
{
    // copied and altered from https://github.com/scottrippey/Progression/blob/master/Progression/Extras/ETACalculator.cs


    /// <summary> Calculates the "Estimated Time of Arrival"
    /// (or more accurately, "Estimated Time of Completion"),
    /// based on a "rolling average" of progress over time.
    /// </summary>
    public class EtaCalculator
    {
        /// <summary>
        /// </summary>
        /// <param name="minimumData">
        /// The minimum number of data points required before ETA can be calculated.
        /// </param>
        /// <param name="maximumDuration">
        /// Determines how many seconds of data will be used to calculate the ETA.
        /// </param>
        /// <param name="totalExpectedCount"></param>
        public EtaCalculator(int minimumData, double maximumDuration, int totalExpectedCount)
        {
            _minimumData = minimumData;
            _totalExpectedCount = totalExpectedCount;
            _maximumTicks = (long)(maximumDuration * Stopwatch.Frequency);
            _queue = new Queue<ProgressItem>(minimumData * 2);
            _timer = Stopwatch.StartNew();
        }

        private readonly int _minimumData;
        private readonly int _totalExpectedCount;
        private readonly long _maximumTicks;
        private readonly Stopwatch _timer;
        private readonly Queue<ProgressItem> _queue;
        private int _incremented;

        private ProgressItem _current;
        private ProgressItem _oldest;

        public void Reset()
        {
            _queue.Clear();

            _timer.Reset();
            _timer.Start();
        }

        private void ClearExpired()
        {
            var expired = _timer.ElapsedTicks - _maximumTicks;
            while (_queue.Count > _minimumData && _queue.Peek().Key < expired)
            {
                _oldest = _queue.Dequeue();
            }
        }

        public void Increment()
        {
            _incremented++;
            var progress = (float)_incremented / _totalExpectedCount;

            // Clear space for this item:
            ClearExpired();

            // Queue this item:
            long currentTicks = _timer.ElapsedTicks;
            _current = new ProgressItem(currentTicks, progress);
            _queue.Enqueue(_current);

            // See if its the first item:
            if (_queue.Count == 1)
            {
                _oldest = _current;
            }
        }

        public bool TryGetEta(out TimeSpan etr, out DateTime eta)
        {
            if (!ETAIsAvailable)
            {
                etr = TimeSpan.MaxValue;
                eta = DateTime.MaxValue;
                return false;
            }

            // Create local copies of the oldest & current,
            // so that another thread can update them without locking:
            var oldest = _oldest;
            var current = _current;

            // Calculate the estimated finished time:
            double finishedInTicks = (1.0d - current.Value) * (current.Key - oldest.Key) / (current.Value - oldest.Value);

            etr = TimeSpan.FromSeconds(finishedInTicks / Stopwatch.Frequency);
            eta = DateTime.Now.Add(etr);
            return true;
        }

        /// <summary> Returns True when there is enough data to calculate the ETA.
        /// Returns False if the ETA is still calculating.
        /// </summary>
        private bool ETAIsAvailable => _queue.Count >= _minimumData && _oldest.Value != _current.Value;
    }
}