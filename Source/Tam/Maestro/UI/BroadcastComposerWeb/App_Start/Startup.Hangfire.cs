using BroadcastComposerWeb.Filters;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Practices.Unity;
using Owin;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Enums;
using System;
using System.Configuration;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Services.Clients;

namespace BroadcastComposerWeb
{
    public partial class Startup
    {
        public void ConfigureHangfire(IAppBuilder app)
        {
            var applicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            //Set Hangfire to use SQL Server for job persistence
            var connectionString = GetConnectionString(applicationName);
            var sqlStorageOptions = _GetSqlServerStorageOptions();
            GlobalConfiguration.Configuration.UseSqlServerStorage(@connectionString, sqlStorageOptions);

            //Sets Hangfire to use the Ioc container
            GlobalConfiguration.Configuration.UseActivator(new HangfireJobActivator(BroadcastApplicationServiceFactory.Instance));

            // Define the services and their queues.
            var serviceNameBase = $"{Environment.MachineName}.{applicationName}";
            var quickRunWorkerCount = double.Parse(ConfigurationManager.AppSettings["HangfireQuickWorkerCountPerProcessor"]);
            var longRunWorkerCount = double.Parse(ConfigurationManager.AppSettings["HangfireLongWorkerCountPerProcessor"]); ;
            _AddService($"{serviceNameBase}.QuickRun", GetQuickRunQueueNames(), quickRunWorkerCount, app);
            _AddService($"{serviceNameBase}.LongRun", GetLongRunQueueNames(), longRunWorkerCount, app);

            // set the default retry count.  
            // override on a queue item with the AutomaticRetry attribute
            var defaultRetryCount = int.Parse(ConfigurationManager.AppSettings["HangfireDefaultRetryCount"]);
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = defaultRetryCount });

            var recurringJobsFactory = new RecurringJobsFactory(
                BroadcastApplicationServiceFactory.Instance.Resolve<IRecurringJobManager>(),
                BroadcastApplicationServiceFactory.Instance.Resolve<IPlanService>());
            recurringJobsFactory.AddOrUpdateRecurringJobs();

            //Configure Hangfire Dashboard and Dashboard Authorization
            app.UseHangfireDashboard("/jobs", new DashboardOptions()
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
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

        private string GetConnectionString(string applicationName)
        {
            var resource = TAMResource.BroadcastConnectionString.ToString();
            var connectionString = ConfigurationClientSwitch.Handler.GetResource(resource);
            
            return ConnectionStringHelper.BuildConnectionString(connectionString, applicationName);
        }

        private SqlServerStorageOptions _GetSqlServerStorageOptions()
        {
            // these are recommended settings for Hangfire from version 1.7.0 on.
            var options = new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromMilliseconds(double.Parse(ConfigurationManager.AppSettings["HangfirePollingInterval"])),
                UseRecommendedIsolationLevel = true,
                UsePageLocksOnDequeue = true,
                DisableGlobalLocks = true
            };

            return options;
        }

        private string[] GetLongRunQueueNames()
        {
            // these are in priority order 
            return new[]
            {
                Enum.GetName(typeof(QueueEnum), QueueEnum.InventoryRating),
                Enum.GetName(typeof(QueueEnum), QueueEnum.PlanPricing),
                Enum.GetName(typeof(QueueEnum), QueueEnum.InventorySummaryAggregation),
                Enum.GetName(typeof(QueueEnum), QueueEnum.InventoryProgramEnrichment),
                Enum.GetName(typeof(QueueEnum), QueueEnum.ScxFileGeneration),
                Enum.GetName(typeof(QueueEnum), QueueEnum.Default),
            };
        }

        private string[] GetQuickRunQueueNames()
        {
            var longs = GetLongRunQueueNames();
            return Enum.GetNames(typeof(QueueEnum)).Where(n => longs.Contains(n) == false).ToArray();
        }

        private void _AddService(string name, string[] queuesToManage, double workerCountPerProcessor, IAppBuilder app)
        {
            var workerCount = (int)Math.Round(Environment.ProcessorCount * workerCountPerProcessor, MidpointRounding.AwayFromZero);
            if (workerCount < 1)
            {
                workerCount = 1;
            }

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                ServerName = name,
                Queues = queuesToManage,
                WorkerCount = workerCount
            });
        }
    }
}