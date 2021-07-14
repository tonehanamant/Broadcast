﻿using System;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Helpers;
using Services.Broadcast.Services;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace WWTVData.Service
{

    public class WWTVInboundErrorService :  ScheduledServiceMethod
    {
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly Lazy<bool> _IsPipelineVariablesEnabled;
        public WWTVInboundErrorService() : base(null)
        {
        }
        
        public override string ServiceName
        {
            get { return "WWTV Error File Retiever";  }
        }

        private DateTime? _RunWhen = null;
        /// <summary>
        /// Use when you want day/time of week to run.
        /// </summary>
        protected override DateTime? RunWeeklyWhen
        {
            get
            {
                _RunWhen = _EnsureRunWeeklyWhen(_IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.WWTV_WhenTocheckErrorFiles, "") : BroadcastServiceSystemParameter.WWTV_WhenToCheckErrorFiles);

                return _RunWhen;
            }
        }

        public override bool RunService(DateTime timeSignaled)
        {
            _LastRun = DateTime.Now;
            //BaseWindowsService.LogServiceEvent("Checking Error Files . . .");
            try
            {
                ApplicationServiceFactory.GetApplicationService<IAffidavitPostProcessingService>().ProcessErrorFiles();
                ApplicationServiceFactory.GetApplicationService<IPostLogPostProcessingService>().ProcessErrorFiles();
            }
            catch (Exception e)
            {
                BaseWindowsService.LogServiceError("Error reading FTP Error files", e);
                return false;
            }
            //BaseWindowsService.LogServiceEvent(". . . Done Checking Error Files\n");
            return true;
        }
    }
}