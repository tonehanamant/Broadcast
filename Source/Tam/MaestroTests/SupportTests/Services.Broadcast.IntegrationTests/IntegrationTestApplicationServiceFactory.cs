using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using ConfigurationService.Client;
using Hangfire;
using Moq;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;
using Unity;
using Unity.Interception;
using Unity.Interception.InterceptionBehaviors;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using Unity.Lifetime;

namespace Services.Broadcast.IntegrationTests
{
    public class IntegrationTestApplicationServiceFactory
    {
        private static volatile UnityContainer _instance;
        private static readonly object SyncLock = new object();

        public static BroadcastDataDataRepositoryFactory BroadcastDataRepositoryFactory;
        public static IMediaMonthAndWeekAggregateCache MediaMonthAndWeekAggregateCache;

        public static  IBackgroundJobClient BackgroundJobClient;

        static IntegrationTestApplicationServiceFactory()
        {
            lock (SyncLock)
            {
                if (_instance == null)
                {
                    IntegrationTestJsonSerializationControl.FollowIgnoreRules = true;
                    IntegrationTestHelper.Ignorer = new AttributeIgnorer();
                    RepositoryOptions.CodeExecutingForIntegegrationTest = true;

                    _instance = new UnityContainer();

                    var launchDarklyClientStub = new LaunchDarklyClientStub();
                    // populate the flags
                    _SetupGlobalFeatureToggles(launchDarklyClientStub);
                    // register our stub instance so it is used to instantiate the service
                    _instance.RegisterInstance<ILaunchDarklyClient>(launchDarklyClientStub);
                    
                    BroadcastDataRepositoryFactory = new BroadcastDataDataRepositoryFactory();
                    BroadcastDataRepositoryFactory.GetUnityContainer().RegisterInstance<ILaunchDarklyClient>(launchDarklyClientStub);
                    _instance.RegisterInstance<IDataRepositoryFactory>(BroadcastDataRepositoryFactory);

                    CommonRepositoryFactory.Instance.RegisterInstance<IConfigurationWebApiClient>(new StubbedConfigurationWebApiClient());

                    var stubbedSmsClient = new StubbedSMSClient();
                    var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();

                    
                    
                    _instance.RegisterInstance<IConfigurationWebApiClient>(stubbedConfigurationClient);
                    

                    SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);
                    
                    _instance.RegisterType<IBroadcastLockingManagerApplicationService, BroadcastLockingManagerApplicationService>(new ContainerControlledLifetimeManager());
                    _instance.RegisterType<IBroadcastLockingService, BroadcastLockingService>(new ContainerControlledLifetimeManager());
                    _instance.RegisterInstance<ISMSClient>(stubbedSmsClient);
                    BroadcastApplicationServiceFactory.RegisterApplicationServices(_instance);

                    _SetupBackgroundJobClient();

                    MediaMonthAndWeekAggregateCache = _instance.Resolve<IMediaMonthAndWeekAggregateCache>();

                    _instance.RegisterType<ICampaignAggregator, CampaignAggregator>();

                    _instance.RegisterType<IAgencyAdvertiserBrandApiClient, AgencyAdvertiserBrandApiClientStub>();


                    _instance.RegisterType<IAsyncTaskHelper, AsyncTaskHelperStub>();

                }
            }
        }

        private static void _SetupBackgroundJobClient()
        {
            var configSettingsHelper = _instance.Resolve<IConfigurationSettingsHelper>();
            var connectionStringRaw = configSettingsHelper.GetConfigValue<string>(ConnectionStringConfigKeys.CONNECTIONSTRINGS_BROADCAST);
            var connectionString = ConnectionStringHelper.BuildConnectionString(connectionStringRaw, System.Diagnostics.Process.GetCurrentProcess().ProcessName);

            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);
            BackgroundJobClient = new BackgroundJobClient(JobStorage.Current);
            _instance.RegisterInstance<IBackgroundJobClient>(BackgroundJobClient);
        }

        public static UnityContainer Instance
        {
            get { return _instance; }
        }

        public static T GetApplicationService<T>() where T : class, IApplicationService
        {
            return Intercept.ThroughProxy(
                Instance.Resolve<T>(),
                new InterfaceInterceptor(),
                new IInterceptionBehavior[] { });
        }

        private static void _SetupGlobalFeatureToggles(LaunchDarklyClientStub launchDarklyClientStub)
        {
            /*** These are set as they are in production. ***/
            launchDarklyClientStub.FeatureToggles.Add(FeatureToggles.ENABLE_PRICING_EFFICIENCY_MODEL, true);
            launchDarklyClientStub.FeatureToggles.Add(FeatureToggles.ENABLE_PIPELINE_VARIABLES, true);
            launchDarklyClientStub.FeatureToggles[FeatureToggles.PRICING_MODEL_OPEN_MARKET_INVENTORY] = true;
            launchDarklyClientStub.FeatureToggles[FeatureToggles.EMAIL_NOTIFICATIONS] = true;

            // TODO: Affected tests should be reworked for these to be false, as they are in production
            launchDarklyClientStub.FeatureToggles[FeatureToggles.PRICING_MODEL_BARTER_INVENTORY] = true;
            launchDarklyClientStub.FeatureToggles[FeatureToggles.PRICING_MODEL_PROPRIETARY_O_AND_O_INVENTORY] = true;

            // this is only enabled for test
            launchDarklyClientStub.FeatureToggles[FeatureToggles.ENABLE_SAVE_INGESTED_INVENTORY_FILE] = true;
        }
    }

    public class RepositoryMock<T> : IDisposable where T : class, IDataRepository
    {
        private readonly T _Old;

        public RepositoryMock(IMock<T> mock)
        {
            _Old = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<T>();
            IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetUnityContainer().RegisterInstance(mock.Object);
        }


        public void Dispose()
        {
            IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetUnityContainer().RegisterInstance(_Old);
        }
    }

    public class ApplicationServiceMock<T> : IDisposable where T : class, IApplicationService
    {
        private readonly T _Old;

        public ApplicationServiceMock(IMock<T> mock)
        {
            _Old = IntegrationTestApplicationServiceFactory.GetApplicationService<T>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance(mock.Object);
        }


        public void Dispose()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance(_Old);
        }
    }
}
