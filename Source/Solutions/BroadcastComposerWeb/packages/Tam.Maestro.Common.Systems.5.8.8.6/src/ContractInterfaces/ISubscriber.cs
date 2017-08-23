using Tam.Maestro.Data.Entities;

namespace Tam.Maestro.Services.ContractInterfaces
{
    public interface ISubscriber<gEntity>
        where gEntity : IBusinessEntity
    {
        void OnInsert<lEntity>(IBusinessEntity[] pEntities) where lEntity : gEntity;
        void OnUpdate<lEntity>(IBusinessEntity[] pOldEntities, IBusinessEntity[] pNewEntities) where lEntity : gEntity;
        void OnDelete<lEntity>(IBusinessEntity[] pEntities) where lEntity : gEntity;
        void OnTimeExpired<lEntity>(IBusinessEntity[] pEntities) where lEntity : gEntity;
        void OnBeforeTimeExpired<lEntity>(IBusinessEntity[] pEntities) where lEntity : gEntity;
    }
}
