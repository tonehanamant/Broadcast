﻿using System;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Services;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace WWTVData.Service
{

    public class WWTVInboundService :  ScheduledServiceMethod
    {  
        public WWTVInboundService () : base(null)
        {
            
        }
        
        public override string ServiceName
        {
            get { return "WWTV Download from WWTV";  }
        }

        private DateTime? _RunWhen = null;
        /// <summary>
        /// Use when you want day/time of week to run.
        /// </summary>
        protected override DateTime? RunWeeklyWhen
        {
            get
            {
                _RunWhen = _EnsureRunWeeklyWhen("");

                return _RunWhen;
            }
        }

        public override int SecondsBetweenRuns
        {
            get
            {
                return  0;
            }
        }
        
        public override bool RunService(DateTime timeSignaled)
        {
            _LastRun = DateTime.Now;
            BaseWindowsService.LogServiceEvent("Checking files to download . . .");

            try
            {
                var now = DateTime.Now;
                ApplicationServiceFactory.GetApplicationService<IAffidavitPostProcessingService>().DownloadAndProcessWWTVFiles("WWTV Service", now);
                ApplicationServiceFactory.GetApplicationService<IPostLogPostProcessingService>().DownloadAndProcessWWTVFiles("WWTV Service", now);
                _LastRun = DateTime.Now;
            }
            catch (Exception e)
            {
                BaseWindowsService.LogServiceError("Error reading from WWTV FTP.\r\n" + e.ToString());
                return false;
            }
            BaseWindowsService.LogServiceEvent(". . . Done Checking files to download\n");
            return true;
        }
    }
}