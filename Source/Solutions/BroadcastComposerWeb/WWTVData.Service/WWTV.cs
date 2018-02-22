using System;
using System.Timers;

namespace WWTVData.Service
{
    public interface IWWTV
    {
        void Start();
        void Stop();
        void CheckWWTVFiles(DateTime timeSignaled);
    }
    public class WWTV : IWWTV
    {
        public const int MILSEC_BETWEEN_CHECKS = 1000 * 5;
        private DateTime _timeLastRun;

        public int SecondsBetweenRuns { get; set; }

        readonly Timer _Timer;
        public WWTV()
        {
            SecondsBetweenRuns = 10 ;
            _Timer = new Timer(MILSEC_BETWEEN_CHECKS) { AutoReset = true }; // once an hour
            _Timer.Elapsed += _Timer_check_for_WWT_files;
            _timeLastRun = DateTime.Now;
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
        public void Start() { _Timer.Start(); }
        public void Stop() { _Timer.Stop(); }

        public void CheckWWTVFiles(DateTime timeSignaled)
        {
            Console.WriteLine("Not Implemented at time " + timeSignaled.ToString("s"));
        }
    }
}