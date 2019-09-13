using System;
using System.Configuration;
using Broadcast.Worker.Filters;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Practices.Unity;
using Owin;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Clients;

namespace Broadcast.Worker
{
    public partial class Startup
    {
        public void ConfigureHangfire(IAppBuilder app)
        {
            //Set Hangfire to use SQL Server for job persistence
            var connectionString = GetConnectionString();
            GlobalConfiguration.Configuration.UseSqlServerStorage(@connectionString, new SqlServerStorageOptions
            {
                QueuePollInterval = TimeSpan.FromMilliseconds(double.Parse(ConfigurationManager.AppSettings["HangfirePollingInterval"]))
            });

            //Sets Hangfire to use the Ioc container
            GlobalConfiguration.Configuration.UseActivator(new HangfireJobActivator(BroadcastApplicationServiceFactory.Instance));

            //Configure Hangfire Dashboard and Dashboard Authorization
            app.UseHangfireDashboard("/jobs", new DashboardOptions()
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                ServerName = $"{Environment.MachineName}.{Guid.NewGuid().ToString()}",
                Queues = Enum.GetNames(typeof(QueueEnum)),
                WorkerCount = Environment.ProcessorCount * 5
            });
        }

        public class HangfireJobActivator : JobActivator
        {
            private readonly IUnityContainer _container;

            public HangfireJobActivator(IUnityContainer container)
            {
                _container = container;
            }

            public override object ActivateJob(Type type)
            {
                return _container.Resolve(type);
            }
        }

        private string GetConnectionString()
        {
            var resource = TAMResource.BroadcastConnectionString.ToString();
            var connectionString = ConfigurationClientSwitch.Handler.GetResource(resource);
            return ConnectionStringHelper.BuildConnectionString(connectionString, ApplicationName);
        }

        private static volatile string _ApplicationName = null;
        public static string ApplicationName
        {
            get
            {
                if (string.IsNullOrEmpty(_ApplicationName))
                    _ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                return _ApplicationName;
            }
        }
    }
}
