using log4net;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities;

namespace Tam.Maestro.Common
{
    internal struct VoidTypeStruct { }  // See Footnote #1

    public static class TaskExtensions
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static Task<ServiceStatus> TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            // tcs.Task will be returned as a proxy to the caller
            TaskCompletionSource<ServiceStatus> tcs =
                new TaskCompletionSource<ServiceStatus>();

            // Set up a timer to complete after the specified timeout period
            Timer timer = new Timer(_ =>
            {
                // Fault our proxy Task with a TimeoutException
                tcs.TrySetException(new TimeoutException());
            }, null, millisecondsTimeout, Timeout.Infinite);

            // Wire up the logic for what happens when source task completes
            task.ContinueWith(antecedent =>
            {
                timer.Dispose(); // Cancel the timer
                MarshalTaskResults(antecedent, tcs); // Marshal results to proxy
            }, CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            return tcs.Task;
        }

        internal static void MarshalTaskResults<TResult>(Task source, TaskCompletionSource<TResult> proxy)
        {
            switch (source.Status)
            {
                case TaskStatus.Faulted:
                    proxy.TrySetException(source.Exception);
                    break;
                case TaskStatus.Canceled:
                    proxy.TrySetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    Task<TResult> castedSource = source as Task<TResult>;
                    proxy.TrySetResult(
                        castedSource == null ? default(TResult) : // source is a Task
                            castedSource.Result); // source is a Task<TResult>
                    break;
            }
        }

        public static void LogExceptions(this Task task)
        {
            task.ContinueWith(t =>
            {
                var aggException = t.Exception.Flatten();
                foreach (var exception in aggException.InnerExceptions)
                    Log.Error(exception);
            },
            TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
