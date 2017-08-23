using System.ServiceModel;

namespace Tam.Maestro.Services.ContractInterfaces
{
    [ServiceContract()]
    public interface ITAMService
    {
        [OperationContract]
        Tam.Maestro.Data.Entities.ServiceStatus GetStatus();
        [OperationContract]
        string GetVersion();
        [OperationContract]
        Common.MaestroServiceRuntime GetMaestroServiceRuntime();
    }
}
