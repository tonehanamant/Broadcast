using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Tam.Maestro.Services.Clients;

namespace Common.Services.Repositories
{
    public abstract class CommonRepositoryFactory : IDataRepositoryFactory
    {
        private static volatile UnityContainer _instance = null;
        private static readonly object SyncLock = new object();

        protected CommonRepositoryFactory()
        {
            RegisterTypes(Instance);
        }

        public static UnityContainer Instance
        {
            get
            {
                lock (SyncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new UnityContainer();
                    }
                }
                return _instance;
            }
        }

        public abstract void RegisterTypes(UnityContainer instance);

        public void RebindSms(ISMSClient smsClient)
        {
            Instance.RegisterInstance<ISMSClient>(smsClient);
        }

        public UnityContainer GetUnityContainer()
        {
            return Instance;
        }

        public T GetDataRepository<T>() where T : class, IDataRepository
        {
            return Intercept.ThroughProxy(
                Instance.Resolve<T>(),
                new InterfaceInterceptor(),
                new IInterceptionBehavior[] { new ValidationExceptionInterceptionBehavior() });
        }
    }
}
