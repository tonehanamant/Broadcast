using System.Collections.Generic;

namespace BroadcastJobScheduler.JobQueueMonitors
{
    /// <summary>
    /// A queue monitor for the long running background jobs.
    /// </summary>
    public interface ILongRunQueueMonitors : IJobQueueMonitorService
    {
    }

    /// <summary>
    /// A queue monitor for the long running background jobs.
    /// </summary>
    public class LongRunQueueMonitors : JobQueueMonitorServiceBase, ILongRunQueueMonitors
    {
        /// <inheritdoc />
        protected override List<QueueMonitorServiceDetail> _GetQueueServiceDetails(string queueMonitorServiceNameBase)
        {
            var serviceDetails = new List<QueueMonitorServiceDetail>();

            var longRunDetails = new QueueMonitorServiceDetail
            {
                QueueMonitorServiceName = $"{queueMonitorServiceNameBase}.LongRun",
                QueuesToMonitor = _GetQueueNames_LongRunningBackgroundTasks(),
                WorkerCountPerProcessor = _GetLongRunWorkerCountPerProcessor()
            };
            serviceDetails.Add(longRunDetails);

            return serviceDetails;
        }

        private string[] _GetQueueNames_LongRunningBackgroundTasks()
        {
            // these are in priority order 
            // all lower case.
            return new[]
            {
                "inventoryrating",
                "planpricing",
                "inventoryprogramsprocessing",
                "processprogramenrichedinventoryfiles",
                "scxfilegeneration",
                "stationsupdate",
                "inventorysummaryaggregation",
                "aggregateinventoryproprietarysummary",
                "programmappings",
                "savepricingrequest",
                "planbuying",
                "savebuyingrequest",
                "reelisciingest",
                "planstatustransitionv2",
                "spotexceptionssyncingest",
                "spotexceptioningestrun",
                "default"
            };
        }

        private double _GetLongRunWorkerCountPerProcessor()
        {
            var result = AppSettingHelper.GetConfigSetting("HangfireLongWorkerCountPerProcessor", 1.0);
            return result;
        }
    }
}