using System.Collections.Generic;

namespace BroadcastJobScheduler.JobQueueMonitors
{
    /// <summary>
    /// A queue monitor for the quick running background jobs.
    /// </summary>
    public interface IQuickRunQueueMonitors : IJobQueueMonitorService
    {
    }

    /// <summary>
    /// A queue monitor for the quick running background jobs.
    /// </summary>
    public class QuickRunQueueMonitors : JobQueueMonitorServiceBase, IQuickRunQueueMonitors
    {
        /// <inheritdoc />
        protected override List<QueueMonitorServiceDetail> _GetQueueServiceDetails(string queueMonitorServiceNameBase)
        {
            var serviceDetails = new List<QueueMonitorServiceDetail>();

            var details = new QueueMonitorServiceDetail
            {
                QueueMonitorServiceName = $"{queueMonitorServiceNameBase}.QuickRun",
                QueuesToMonitor = _GetQuickRunQueueNames(),
                WorkerCountPerProcessor = _GetWorkerCountPerProcessor()
            };

            serviceDetails.Add(details);
            return serviceDetails;
        }

        private string[] _GetQuickRunQueueNames()
        {
            // these are in priority order 
            // all lower case.
            return new[]
            {
                "campaignaggregation",
                "planstatustransition"
            };
        }

        protected virtual double _GetWorkerCountPerProcessor()
        {
            var result = ConfigurationSettingHelper.GetConfigSetting("HangfireQuickWorkerCountPerProcessor", 1.0);
            return result;
        }
    }
}