using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using ConfigurationService.Client;
using Hangfire;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Moq;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.UnitTests;
using Services.Broadcast.Repositories;
using System;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.IntegrationTests
{
    public class IntegrationTestApplicationServiceFactory
    {
        private static volatile UnityContainer _instance;
        private static readonly object SyncLock = new object();

        public static readonly BroadcastDataDataRepositoryFactory BroadcastDataRepositoryFactory;
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

                    BroadcastDataRepositoryFactory = new BroadcastDataDataRepositoryFactory();
                    CommonRepositoryFactory.Instance.RegisterInstance<IConfigurationWebApiClient>(new StubbedConfigurationWebApiClient());

                    var stubbedSmsClient = new StubbedSMSClient();
                    var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();
                    var connectionString = ConnectionStringHelper.BuildConnectionString(
                        stubbedConfigurationClient.GetResource(TAMResource.BroadcastConnectionString.ToString()), 
                        System.Diagnostics.Process.GetCurrentProcess().ProcessName);
                    GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);
                    BackgroundJobClient = new BackgroundJobClient(JobStorage.Current);

                    _instance.RegisterInstance<IDataRepositoryFactory>(BroadcastDataRepositoryFactory);
                    _instance.RegisterInstance<IConfigurationWebApiClient>(stubbedConfigurationClient);
                    _instance.RegisterInstance<IBackgroundJobClient>(BackgroundJobClient);

                    SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);
                    
                    _instance.RegisterType<ILockingManagerApplicationService, BroadcastLockingManagerApplicationService>();
                    _instance.RegisterInstance<ISMSClient>(stubbedSmsClient);
                    BroadcastApplicationServiceFactory.RegisterApplicationServices(_instance);
                    MediaMonthAndWeekAggregateCache = _instance.Resolve<IMediaMonthAndWeekAggregateCache>();

                    _instance.RegisterType<ICampaignAggregator, CampaignAggregator>();
                    _instance.RegisterType<ITrafficApiClient, TrafficApiClientStub>();
                    _instance.RegisterType<ITrafficApiCache, TrafficApiCache>();

                    _instance.RegisterType<IProgramGuideApiClient, ProgramGuideApiClient>();
                    _instance.RegisterType<IProgramGuideService, ProgramGuideService>();
                    _instance.RegisterType<IProgramGuideApiClientSimulator, ProgramGuideApiClientSimulator>();
                }
            }
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
