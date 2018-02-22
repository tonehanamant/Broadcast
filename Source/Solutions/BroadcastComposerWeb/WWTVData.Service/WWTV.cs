using System;
using System.Timers;
using Services.Broadcast.Services;

namespace WWTVData.Service
{
    public interface IWWTV
    {
        void CheckWWTVFiles(DateTime timeSignaled);
    }

    public class WWTV : ServiceBase , IWWTV
    {
        public const int MILSEC_BETWEEN_CHECKS = 1000 * 5;
        private DateTime _timeLastRun;

        public int SecondsBetweenRuns { get; set; }

        readonly Timer _Timer;
        public WWTV(string serviceName) : base(serviceName)
        {
            SecondsBetweenRuns = 10 ;
            _Timer = new Timer(MILSEC_BETWEEN_CHECKS) { AutoReset = true }; // once an hour
            _Timer.Elapsed += _Timer_check_for_WWT_files;
            _timeLastRun = DateTime.Now;
        }

        protected override string GetServiceName()
        {
            return GetServiceNameStaticPlaceholder();
        }

        protected void _Timer_check_for_WWT_files(object sender, ElapsedEventArgs e)
        {
            _Timer.Stop();

            var signalTime = e.SignalTime;

            if (signalTime.AddSeconds(-SecondsBetweenRuns) >= _timeLastRun)
            {

                CheckWWTVFiles(_timeLastRun);

                _timeLastRun = signalTime;
            }
            _Timer.Start();
        }
        public override void Start() { _Timer.Start(); }
        public override void Stop() { _Timer.Stop(); }

        public void CheckWWTVFiles(DateTime timeSignaled)
        {
            Console.WriteLine("Not Implemented at time " + timeSignaled.ToString("s"));
            LogServiceError("abc","oopsie",new Exception());
        }
    }
}