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

    public class WWTVErrorFiles :  ScheduledServiceMethod
    {

        public WWTVErrorFiles() : base(null)
        {
        }


        public override string ServiceName
        {
            get { return "WWTV Error File Retiever";  }
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
                if (!_RunWhenChecked)
                {
                    if (SMSClient.Handler.TamEnvironment != TAMEnvironment.PROD.ToString())
                        _RunWhen = null;
                    else
                    {
                        DateTime d;
                        if (DateTime.TryParse(BroadcastServiceSystemParameter.WWTV_WhenToCheckErrorFiles, out d))
                            _RunWhen = d;
                    }
                    _RunWhenChecked = true;
                }

                return _RunWhen;
            }
        }

        public override bool RunService(DateTime timeSignaled)
        {
            _LastRun = DateTime.Now;
            //BaseWindowsService.LogServiceEvent("Checking Error Files . . .");
            try
            {
                var service = ApplicationServiceFactory.GetApplicationService<IAffidavitPreprocessingService>();
                service.ProcessErrorFiles();
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