using System;
using System.IO;
using System.Linq;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Services;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace WWTVData.Service
{

    public class WWTVDownloadFromWWTV :  ScheduledServiceMethod
    {
        public WWTVDownloadFromWWTV () : base(null)
        {
        }


        public override string ServiceName
        {
            get { return "WWTV Download from WWTV";  }
        }

        private bool _RunWhenChecked = false;
        private DateTime? _RunWhen = null;
        /// <summary>
        /// Use when you want day/time of week to run.
        /// </summary>
        protected override DateTime? RunWhen
        {
            get
            {
                if (!_RunWhenChecked)
                {
                    if (SMSClient.Handler.TamEnvironment != TAMEnvironment.PROD.ToString())
                        _RunWhen = null;
                    else
                    {
                        DateTime d;
                        if (DateTime.TryParse(BroadcastServiceSystemParameter.WWTV_WhenToCheckWWTV, out d))
                            _RunWhen = d;
                    }
                    _RunWhenChecked = true;
                }

                return _RunWhen;
            }
        }

        public override int SecondsBetweenRuns
        {
            get
            {
                return  BroadcastServiceSystemParameter.WWTV_SecondsBetweenRuns;
            }
        }


        public override bool RunWhenReady(DateTime timeSignaled)
        {
            if (_LastRun == null)
            {   // first time run
                return RunService(timeSignaled);
            }

            bool ret = false;
            if (RunWhen == null)
            {
                if (_LastRun.Value.AddSeconds(SecondsBetweenRuns) < timeSignaled)
                    ret = RunService(timeSignaled);
            }
            else
            if (DateTime.Now.DayOfWeek == RunWhen.Value.DayOfWeek 
                    && DateTime.Now.TimeOfDay > RunWhen.Value.TimeOfDay)
            {
                ret = RunService(timeSignaled);
            }
            return ret;
        }

        public override bool RunService(DateTime timeSignaled)
        {
            _LastRun = DateTime.Now;
            try
            {
                var service = ApplicationServiceFactory.GetApplicationService<IAffidavitPostProcessingService>();
                service.DownloadAndProcessWWTVFiles();
                _LastRun = DateTime.Now;
            }
            catch (Exception e)
            {
                BaseWindowsService.LogServiceError("Error reading from WWTV FTP.", e);
                return false;
            }
            BaseWindowsService.LogServiceEvent(". . . Done Checking files to download\n");
            return true;
        }
    }
}