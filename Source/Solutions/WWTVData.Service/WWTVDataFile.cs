using System;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Services;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace WWTVData.Service
{
    public class WWTVDataFile : ScheduledServiceMethod
    {
        public WWTVDataFile() : base(null)
        {
        }


        public override string ServiceName
        {
            get { return "WWTV File Retriever"; }
        }


        private bool _RunWhenChecked = false;
        private DateTime? _RunWhen = null;

        /// <summary>
        /// Use when you want day/time of week to run.
        /// </summary>
        protected override DateTime? RunWeeklyWhen
        {
            get
            {
                _RunWhen = _EnsureRunWeeklyWhen(BroadcastServiceSystemParameter.WWTV_WhenToCheckDataFiles);

                return _RunWhen;
            }
        }


        public override int SecondsBetweenRuns
        {
            get { return BroadcastServiceSystemParameter.WWTV_SecondsBetweenRuns; }
        }

        public override bool RunService(DateTime timeSignaled)
        {
            _LastRun = DateTime.Now;
            //BaseWindowsService.LogServiceEvent("Checking WWTV OutPost files. . .");

            try
            {
                ApplicationServiceFactory.GetApplicationService<IAffidavitPreprocessingService>().ProcessFiles(ServiceName);  // Affidavit Files
                ApplicationServiceFactory.GetApplicationService<IPostLogPreprocessingService>().ProcessFiles(ServiceName);  // Sigma and KeepingTrac files   
            }
            catch (Exception e)
            {
                BaseWindowsService.LogServiceError("Error reading from Drop folder.", e);
                return false;
            }

            return true;
        }
    }
}