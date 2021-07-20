using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BroadcastJobScheduler.JobQueueMonitors
{
    /// <summary>
    /// A service to monitor job queues.
    /// </summary>
    public interface IJobQueueMonitorService
    {
        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// Provides base support to job queue monitor service classes.
    /// </summary>
    /// <seealso cref="IJobQueueMonitorService" />
    public abstract class JobQueueMonitorServiceBase : BroadcastJobSchedulerBaseClass, IJobQueueMonitorService
    {
        private readonly List<BackgroundJobServer> _hangfireBackgroundJobServers = new List<BackgroundJobServer>();

        /// <summary>
        /// Gets the service and queue details to be monitored.
        /// </summary>
        /// <param name="queueMonitorServiceNameBase">The queue monitor service name base.</param>
        /// <returns></returns>
        protected abstract List<QueueMonitorServiceDetail> _GetQueueServiceDetails(string queueMonitorServiceNameBase);

        /// <inheritdoc />
        public void Start()
        {
            _AddJobQueueMonitors();
        }

        /// <inheritdoc />
        public void Stop()
        {
            foreach (var backgroundJobService in _hangfireBackgroundJobServers)
            {
                backgroundJobService.Dispose();
            }
        }

        private void _AddJobQueueMonitors()
        {
            var queueServiceDetails = _GetQueueServiceDetails(_GetQueueMonitorServiceNameBase());
            foreach (var queueService in queueServiceDetails)
            {
                _AddService(queueService.QueueMonitorServiceName, queueService.QueuesToMonitor, queueService.WorkerCountPerProcessor);
            }
        }

        private void _AddService(string name, IEnumerable<string> queuesToManage, double workerCountPerProcessor)
        {
            var workerCount = (int)Math.Round(Environment.ProcessorCount * workerCountPerProcessor, MidpointRounding.AwayFromZero);
            if (workerCount < 1)
            {
                workerCount = 1;
            }

            // the name must fit
            const int nameMaxChars = 50;
            var serverName = name;
            if (name.Length > nameMaxChars)
            {
                serverName = name.Substring(0, nameMaxChars);
            }

            var options = new BackgroundJobServerOptions
            {
                ServerName = serverName,
                Queues = queuesToManage.ToArray(),
                WorkerCount = workerCount,
                CancellationCheckInterval = TimeSpan.FromSeconds(AppSettingHelper.GetConfigSetting("HangfireCancellationCheckIntervalSeconds", 1.0))
            };

            _LogInfo($"Worker options: {options.ToJson()}");

            _hangfireBackgroundJobServers.Add(new BackgroundJobServer(options));
        }

        private string _GetQueueMonitorServiceNameBase()
        {
            var applicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            var serviceNameBase = $"{Environment.MachineName}.{applicationName}";
            return serviceNameBase;
        }
    }
}