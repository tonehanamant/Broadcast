namespace BroadcastJobScheduler.JobQueueMonitors
{
    /// <summary>
    /// Describes a queue to be monitored by a service.
    /// </summary>
    public class QueueMonitorServiceDetail
    {
        /// <summary>
        /// The name for the queue monitor service.
        /// </summary>
        public string QueueMonitorServiceName { get; set; }

        /// <summary>
        /// The list of queues to be monitored by this service.
        /// </summary>
        public string[] QueuesToMonitor { get; set; }

        /// <summary>
        /// The queue worker count per processor.
        /// </summary>
        public double WorkerCountPerProcessor { get; set; } = 1.0;
    }
}