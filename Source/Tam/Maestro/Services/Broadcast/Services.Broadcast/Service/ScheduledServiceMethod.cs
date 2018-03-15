using System;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.Services
{
    public abstract class ScheduledServiceMethod 
    {
        public ServiceBase BaseService { get; set; }

        
        public ScheduledServiceMethod(ServiceBase serviceBase)
        {
            BaseService = serviceBase;
        }

        private bool _RunWhenChecked = false;
        private DateTime? _RunWhen = null;
        /// <summary>
        /// Use when you want day/time of week to run.
        /// </summary>
        protected abstract DateTime? RunWhen
        {
            get;
        }

        /// <summary>
        /// Use when you want task to run periodically.
        /// </summary>
        public abstract int SecondsBetweenRuns { get; }

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

        public abstract bool RunWhenReady(DateTime timeSignaled);

        public abstract bool RunService(DateTime timeSignaled);
    }

}