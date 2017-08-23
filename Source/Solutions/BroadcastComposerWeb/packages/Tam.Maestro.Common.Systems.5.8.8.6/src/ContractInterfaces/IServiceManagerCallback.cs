using System.ServiceModel;

namespace Tam.Maestro.Services.ContractInterfaces
{

    public interface IServiceManagerCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnUriChanged(ServiceManagerServiceEventArgs p1);

        [OperationContract(IsOneWay = true)]
        void OnResourceChanged(ServiceManagerResourceEventArgs p1);
    }
}
