using System;
using System.Collections.Generic;
using System.Timers;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Services
{
    public class ScheduledWindowsServiceMethodRunner : WindowsServiceBase
    {
        protected List<ScheduledServiceMethod> _ServicesToRun;

        public const int MILSEC_BETWEEN_CHECKS = 1000 * 5;
        private DateTime? _timeLastRun;

        readonly Timer _Timer;
        public ScheduledWindowsServiceMethodRunner(string serviceName, List<ScheduledServiceMethod> servicesToRun) : base(serviceName)
        {
            _Timer = new Timer(MILSEC_BETWEEN_CHECKS) { AutoReset = true }; // once an hour
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
                    LogServiceError("Error Running Service: ", s.ServiceName, ex);
                }
            });

            _Timer.Start();
        }
        public override void Start() { _Timer.Start(); }
        public override void Stop() { _Timer.Stop(); }

    }

}