using System;
using System.Configuration;
using Microsoft.Owin.Hosting;
using Owin;
using Hangfire;

namespace Broadcast.Worker
{
    public class BroadcastWorkerService
    {
        private IDisposable _host;

        /// <summary>
        /// Configure OWIN hosting options for Hangfire
        /// </summary>
        public void Start()
        {
            var options = new StartOptions { Port = Int32.Parse(ConfigurationManager.AppSettings["HostPort"]) };
            _host = WebApp.Start<Startup>(options);
            Console.WriteLine();
            Console.WriteLine("HangFire has started");
            Console.WriteLine();
        }

        public void Stop()
        {
            _host.Dispose();
        }
    }
}
