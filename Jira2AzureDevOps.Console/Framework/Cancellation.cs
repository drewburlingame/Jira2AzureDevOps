using System;
using System.Threading;

namespace Jira2AzureDevOps.Console.Framework
{
    public static class Cancellation
    {
        private static readonly CancellationTokenSource TokenSource = new CancellationTokenSource();

        public static CancellationToken Token => TokenSource.Token;

        public static bool IsNotRequested => !TokenSource.IsCancellationRequested;
        public static bool IsRequested => TokenSource.IsCancellationRequested;

        public static void Shutdown()
        {
            if (!TokenSource.IsCancellationRequested)
                TokenSource.Cancel();
        }

        public static void SleepUnlessAppDomainIsCancelled(this ManualResetEventSlim wait, TimeSpan timeout)
        {
            if (timeout == TimeSpan.Zero || IsRequested)
            {
                return;
            }
            try
            {
                wait.Wait(timeout, Token);
            }
            catch (OperationCanceledException)
            {
                //expected.  nothing to do here except return
            }
        }
    }
}