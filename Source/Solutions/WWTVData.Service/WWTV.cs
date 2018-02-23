using System;
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

        public int SecondsBetweenRuns
        {
            get
            {
                return 86000;
                //return BroadcastServiceSystemParameter.WWTV_SecondsBetweenRuns;
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
            LogServiceEvent(GetServiceName(), "checking . . . .");
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
            Console.WriteLine("Not Implemented at time " + timeSignaled.ToString("s"));
            LogServiceEvent("abc","oopsie");
        }
    }
}