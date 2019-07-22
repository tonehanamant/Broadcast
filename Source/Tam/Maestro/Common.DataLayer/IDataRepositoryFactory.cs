using Microsoft.Practices.Unity;
using Tam.Maestro.Services.Clients;

namespace Common.Services.Repositories
{
    public interface IDataRepositoryFactory
    {
        T GetDataRepository<T>() where T : class, IDataRepository;
        void RebindSms(ISMSClient smsClient);
        UnityContainer GetUnityContainer();
    }
}