using System;
using System.Collections.Generic;

namespace BroadcastJobScheduler.JobQueueMonitors
{
    /// <summary>
    /// Jobs Queue Monitor for the Inventory Aggregation Queue.
    /// </summary>
    public interface IInventoryAggregationQueueMonitor : IJobQueueMonitorService
    {
    }

    /// <summary>
    /// Jobs Queue Monitor for the Inventory Aggregation Queue.
    /// </summary>
    public class InventoryAggregationQueueMonitor : JobQueueMonitorServiceBase, IInventoryAggregationQueueMonitor
    {
        protected override List<QueueMonitorServiceDetail> _GetQueueServiceDetails(string queueMonitorServiceNameBase)
        {
            var serviceDetails = new List<QueueMonitorServiceDetail>();

            var inventoryAggregationDetails = new QueueMonitorServiceDetail
            {
                QueueMonitorServiceName = $"{queueMonitorServiceNameBase}.InventoryAggregation",
                QueuesToMonitor = _GetQueueNames_InventoryAggregation(),
                WorkerCountPerProcessor = _GetInventoryAggregationWorkerCountPerProcessor()
            };
            serviceDetails.Add(inventoryAggregationDetails);

            return serviceDetails;
        }

        private string[] _GetQueueNames_InventoryAggregation()
        {
            return new[] { "inventorysummaryaggregation" };
        }

        private double _GetInventoryAggregationWorkerCountPerProcessor()
        {
            const double toEnforceOneWorkerPerProcessor = 1.0;
            var result = (toEnforceOneWorkerPerProcessor / Environment.ProcessorCount);
            return result;
        }
    }
}