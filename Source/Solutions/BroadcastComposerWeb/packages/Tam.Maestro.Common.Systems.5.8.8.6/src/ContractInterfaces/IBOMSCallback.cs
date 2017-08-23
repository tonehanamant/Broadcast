using System.ServiceModel;

namespace Tam.Maestro.Services.ContractInterfaces
{
    public interface IBOMSCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnBeforeExpiredTimeLockedEntity(byte[] pEntities);

        [OperationContract(IsOneWay = true)]
        void OnInsert(byte[] pEntities);

        [OperationContract(IsOneWay = true)]
        void OnUpdate(byte[] pOldEntities, byte[] pNewEntities);

        [OperationContract(IsOneWay = true)]
        void OnDelete(byte[] pEntities);

        [OperationContract(IsOneWay = true)]
        void OnExpiredTimeLockedEntity(byte[] pEntities);
    }
}
