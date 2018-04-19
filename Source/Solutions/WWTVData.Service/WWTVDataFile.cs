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

    public class WWTVDataFile :  ScheduledServiceMethod
    {

        public WWTVDataFile() : base(null)
        {
        }


        public override string ServiceName
        {
            get { return "WWTV File Retriever";  }
        }

        public string SharedFolder
        {
            get
            {
                return BroadcastServiceSystemParameter.WWTV_SharedFolder;
            }
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
                        if (DateTime.TryParse(BroadcastServiceSystemParameter.WWTV_WhenToCheckDataFiles, out d))
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
                return BroadcastServiceSystemParameter.WWTV_SecondsBetweenRuns;
            }
        }

        public override bool RunService(DateTime timeSignaled)
        {
            _LastRun = DateTime.Now;
            //BaseWindowsService.LogServiceEvent("Checking WWTV OutPost files. . .");

            string[] filesFound;
            int filesProcessed = 0;
            int filesFailed = 0;
            
            try
            {
                try
                {
                    filesFound = Directory.GetFiles(SharedFolder);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Could not find WWTV_SharedFolder.  Please check it is created.",e);
                }

                var service = ApplicationServiceFactory.GetApplicationService<IAffidavitPreprocessingService>();
                var response = service.ProcessFiles(filesFound.ToList(), ServiceName);
                response.ForEach(r =>
                {
                    if (r.Status == AffidaviteFileProcessingStatus.Valid)
                    {
                        File.Delete(r.FilePath);
                        filesProcessed++;
                    } else if (r.Status == AffidaviteFileProcessingStatus.Valid)
                        filesFailed++;
                });
            }
            catch (Exception e)
            {
                BaseWindowsService.LogServiceError("Error reading from Drop folder.", e);
                return false;
            }


            //BaseWindowsService.LogServiceEvent(". . . Done Checking WWTV OutPost files\n");
            var message = string.Format("Found {0} file; Process {1}; Failed {2}", filesFound.Length, filesProcessed, filesFailed);
            //BaseWindowsService.LogServiceEvent(message);
            return true;
        }
    }
}