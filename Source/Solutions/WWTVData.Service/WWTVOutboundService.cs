using System;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Helpers;
using Services.Broadcast.Services;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace WWTVData.Service
{
    public class WWTVOutboundService : ScheduledServiceMethod
    {
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private readonly Lazy<bool> _IsPipelineVariablesEnabled;
        public WWTVOutboundService() : base(null)
        {
        }
        
        public override string ServiceName
        {
            get { return "WWTV File Retriever"; }
        }
        
        private DateTime? _RunWhen = null;

        /// <summary>
        /// Use when you want day/time of week to run.
        /// </summary>
        protected override DateTime? RunWeeklyWhen
        {
            get
            {
                _RunWhen = _EnsureRunWeeklyWhen(_IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.WWTV_WHENTOCHECKDATAFILES_KEY, "") : BroadcastServiceSystemParameter.WWTV_WhenToCheckDataFiles);

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