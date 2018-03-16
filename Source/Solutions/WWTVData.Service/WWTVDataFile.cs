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
        protected DateTime? _LastRun;

        public WWTVDataFile() : base(null)
        {
        }


        public override string ServiceName
        {
            get { return "WWTV File Retriever";  }
        }

        private string _SharedFolder;
        public string SharedFolder
        {
            get
            {
                if (_SharedFolder == null)
                    _SharedFolder = BroadcastServiceSystemParameter.WWTV_SharedFolder;
                return _SharedFolder;
            }
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
                        if (DateTime.TryParse(BroadcastServiceSystemParameter.WWTV_WhenToCheckDataFiles, out d))
                            _RunWhen = d;
                    }
                    _RunWhenChecked = true;
                }

                return _RunWhen;
            }
        }

        private int? _SecondsBetweenRuns;
        public override int SecondsBetweenRuns
        {
            get
            {
                if (_SecondsBetweenRuns == null)
                    _SecondsBetweenRuns = BroadcastServiceSystemParameter.WWTV_SecondsBetweenRuns;
                return _SecondsBetweenRuns.Value;

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
            Console.WriteLine(timeSignaled.ToString("s") + "::Checking WWTV files Started");
            string[] filesFound;

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
                    if (r.Status == (int)AffidaviteFileProcessingStatus.Valid)
                        File.Delete(r.FilePath);
                });
                _LastRun = DateTime.Now;
            }
            catch (Exception e)
            {
                BaseWindowsService.LogServiceError("Error reading from Drop folder.", e);
                return false;
            }

            int filesProcessed = 0;
            int filesFailed = 0;

            var message = string.Format("\r\nFound {0} file; Process {1}; Failed {2}", filesFound.Length, filesProcessed, filesFailed);
            Console.WriteLine(DateTime.Now + "::Checking WWTV files Finished");
            Console.WriteLine(message);
            BaseWindowsService.LogServiceEvent(message);
            return true;
        }
    }
}