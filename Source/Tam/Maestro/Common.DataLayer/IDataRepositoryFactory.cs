using Microsoft.Practices.Unity;

namespace Common.Services.Repositories
{
    public interface IDataRepositoryFactory
    {
        T GetDataRepository<T>() where T : class, IDataRepository;
        UnityContainer GetUnityContainer();
    }
}