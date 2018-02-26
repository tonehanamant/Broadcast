using System;
using System.IO;
using System.Timers;
using Services.Broadcast.Services;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace WWTVData.Service
{
    public interface IWWTV
    {
        void CheckWWTVFiles(DateTime timeSignaled);
    }

    public class WWTV : ServiceBase , IWWTV
    {
        public const int MILSEC_BETWEEN_CHECKS = 1000 * 5;
        private DateTime? _timeLastRun;

        public string DropFileFolder
        {
            get
            {
                return "R:\\MBH_Stuff\\WWTVDropFolder";
                //return BroadcastServiceSystemParameter.WWTV_DropFileFolder;
            }
        }

        public int SecondsBetweenRuns
        {
            get
            {
                return 15 ;// BroadcastServiceSystemParameter.WWTV_SecondsBetweenRuns;
            }
        }

        readonly Timer _Timer;
        public WWTV(string serviceName) : base(serviceName)
        {
            _Timer = new Timer(MILSEC_BETWEEN_CHECKS) { AutoReset = true }; // once an hour
            _Timer.Elapsed += _Timer_check_for_WWT_files;
        }

        protected override string GetServiceName()
        {
            return GetServiceNameStaticPlaceholder();
        }

        protected void _Timer_check_for_WWT_files(object sender, ElapsedEventArgs e)
        {
            _Timer.Stop();

            var signalTime = e.SignalTime;
            if (_timeLastRun == null)
            {
                _timeLastRun = DateTime.Now;
                CheckWWTVFiles(_timeLastRun.Value);
            }
            else
            if (signalTime.AddSeconds(-SecondsBetweenRuns) >= _timeLastRun)
            {

                CheckWWTVFiles(_timeLastRun.Value);
                _timeLastRun = signalTime;
            }
            _Timer.Start();
        }
        public override void Start() { _Timer.Start(); }
        public override void Stop() { _Timer.Stop(); }

        public void CheckWWTVFiles(DateTime timeSignaled)
        {
            Console.WriteLine(timeSignaled.ToString("s") + "::Checking WWTV files Started");
            string[] filesFound;

            try
            {
                filesFound = Directory.GetFiles(DropFileFolder);
            }
            catch (Exception e)
            {
                throw new Exception("Error reading from drop folder file list.", e);
            }

            int filesProcessed = 0;
            int filesFailed = 0;

            var message = string.Format("\r\nFound {0} file; Process {1}; Failed {2}",filesFound.Length,filesProcessed,filesFailed);
            Console.WriteLine(DateTime.Now + "::Checking WWTV files Finished");
            Console.WriteLine(message);
            LogServiceEvent(GetServiceName(),message);
        }
    }
}