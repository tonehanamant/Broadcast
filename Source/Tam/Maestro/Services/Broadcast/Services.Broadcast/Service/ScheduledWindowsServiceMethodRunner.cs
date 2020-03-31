using System;
using System.Collections.Generic;
using System.Timers;

namespace Services.Broadcast.Services
{
    public class ScheduledWindowsServiceMethodRunner : WindowsServiceBase
    {
        public override string ServiceName
        {
            get { return "_WWTVData.Service"; }
        }

        protected List<ScheduledServiceMethod> _ServicesToRun;

        public const int MILSEC_BETWEEN_CHECKS = 1000 * 5;

        readonly Timer _Timer;
        public ScheduledWindowsServiceMethodRunner(List<ScheduledServiceMethod> servicesToRun) 
        {
            _Timer = new Timer(MILSEC_BETWEEN_CHECKS) { AutoReset = true }; 
            _Timer.Elapsed += _Timer_check_for_WWT_files;
            _ServicesToRun = servicesToRun;
            _ServicesToRun.ForEach(s => s.BaseWindowsService = this);
        }
        
        protected void _Timer_check_for_WWT_files(object sender, ElapsedEventArgs args)
        {
            _Timer.Stop();

            _ServicesToRun.ForEach(s =>
            {
                var signalTime = args.SignalTime;
                try
                {
                    LogServiceEvent("Running service: " + s.ServiceName);
                    s.RunWhenReady(signalTime); //TODO this is blocking and can ruin everything
                }
                catch (Exception ex)
                {
                    LogServiceError("Error Running Service: ", ex);
                }
            });

            _Timer.Start();
        }

        public override void Start()
        {
            _Timer.Start();
        }

        public override void Stop()
        {
            _Timer.Stop();
        }
    }
}