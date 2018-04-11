using System;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.Services
{
    public abstract class ScheduledServiceMethod 
    {
        protected DateTime? _LastRun;

        public WindowsServiceBase BaseWindowsService { get; set; }

        
        public ScheduledServiceMethod(WindowsServiceBase windowsServiceBase)
        {
            BaseWindowsService = windowsServiceBase;
        }

        private bool _RunWhenChecked = false;
        private DateTime? _RunWhen = null;
        /// <summary>
        /// When RunWeeklyWhen is null, SecondsBetweenRuns is used
        /// </summary>
        public virtual int SecondsBetweenRuns
        {
            get
            {
                return BroadcastServiceSystemParameter.WWTV_SecondsBetweenRuns;
            }
        }

        private BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        public BroadcastApplicationServiceFactory ApplicationServiceFactory
        {
            get
            {
                if (_ApplicationServiceFactory == null)
                {
                    _ApplicationServiceFactory = new BroadcastApplicationServiceFactory();
                }

                return _ApplicationServiceFactory;
            }
        }

        
        public abstract string ServiceName { get; }

        /// <summary>
        /// Calls RunService when ready
        /// It is ready first call.
        /// Then ready is calculated using:
        ///     If RunWeeklyWhen is null, SecondsBetweenRuns is used to check is ready
        ///     otherwise RunWeeklyWhen is used 
        /// </summary>
        /// <param name="timeSignaled"></param>
        public bool RunWhenReady(DateTime timeSignaled)
        {
            if (_LastRun == null)
            {   // first time run
                return RunService(timeSignaled);
            }

            bool ret = false;
            if (RunWeeklyWhen == null)
            {
                if (_LastRun.Value.AddSeconds(SecondsBetweenRuns) < timeSignaled)
                    ret = RunService(timeSignaled);
            }
            else
            if (DateTime.Now.DayOfWeek == RunWeeklyWhen.Value.DayOfWeek
                && DateTime.Now.TimeOfDay > RunWeeklyWhen.Value.TimeOfDay)
            {
                ret = RunService(timeSignaled);
            }
            return ret;
        }

        /// <summary>
        /// Use when you want day/time of week to run.
        /// </summary>
        protected abstract DateTime? RunWeeklyWhen
        {
            get;
        }


        public abstract bool RunService(DateTime timeSignaled);
    }
}