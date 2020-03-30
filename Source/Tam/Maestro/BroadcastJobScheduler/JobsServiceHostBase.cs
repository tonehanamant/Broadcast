using Hangfire;
using Tam.Maestro.Common.Utilities.Logging;

namespace BroadcastJobScheduler
{
    /// <summary>
    /// A host for background job services.
    /// </summary>
    public interface IJobsServiceHost
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
    /// A host for background job services.
    /// </summary>
    public abstract class JobsServiceHostBase : IJobsServiceHost
    {
        protected const string RECURRING_JOBS_USERNAME = "RecurringJobsUser";

        protected  readonly IRecurringJobManager _RecurringJobManager;

        protected JobsServiceHostBase(IRecurringJobManager recurringJobManager)
        {
            _RecurringJobManager = recurringJobManager;
        }

        protected abstract void OnStart();
        protected abstract void OnStop();

        /// <inheritdoc />
        public void Start()
        {
            LogHelper.Logger.Info("JobsServiceHost is starting.");

            OnStart();
        }

        public void Stop()
        {
            LogHelper.Logger.Info("JobsServiceHost is Stopping.");
            OnStop();
        }
    }
}