using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Moq;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Repositories;
using System;
using Common.Services;
using IntegrationTests.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.IntegrationTests
{
    public class IntegrationTestApplicationServiceFactory
    {
        private static volatile UnityContainer _instance;
        private static readonly object SyncLock = new object();

        public static readonly BroadcastDataDataRepositoryFactory BroadcastDataRepositoryFactory;
        public static IMediaMonthAndWeekAggregateCache MediaMonthAndWeekAggregateCache;

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

                    var stubbedSmsClient = new StubbedSMSClient();
                    BroadcastDataRepositoryFactory.RebindSms(stubbedSmsClient);
                    _instance.RegisterInstance<IDataRepositoryFactory>(BroadcastDataRepositoryFactory);

                    SystemComponentHelper.SetSmsClient(stubbedSmsClient);
                    
                    _instance.RegisterType<ILockingManagerApplicationService, LockingManagerApplicationService>();
                    _instance.RegisterInstance<ISMSClient>(stubbedSmsClient);
                    BroadcastApplicationServiceFactory.RegisterApplicationServices(_instance);
                    MediaMonthAndWeekAggregateCache = _instance.Resolve<IMediaMonthAndWeekAggregateCache>();
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
