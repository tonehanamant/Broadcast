using System;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Tam.Maestro.Services.Clients
{
    public interface IBOMSClient
    {
        MaestroServiceRuntime GetMaestroServiceRuntime();
        void Subscribe<gEntity>(ISubscriber<gEntity> pSusbcriber, Delegate pTarget, EntityAction pEntityAction) where gEntity : IBusinessEntity;
        void UnSubscribe<gEntity>(ISubscriber<gEntity> pSusbcriber, Delegate pTarget, EntityAction pEntityAction) where gEntity : IBusinessEntity;
        TAMResult CreateTransaction(string pTransactionName);
        TAMResult CommitTransaction(string pTransactionName);
        TAMResult CancelTransaction(string pTransactionName);
        TAMResult KillTransaction(TransactionInfo pTransactionInfo);
        TAMResult Insert(IBusinessEntity[] pEntities, AccessMode pAccessMode);
        TAMResult Insert(IBusinessEntity[] pEntities, AccessMode pAccessMode, EInsertMethod pInsertMethod);
        TAMResult TransactionalInsert(IBusinessEntity[] pEntities, AccessMode pAccessMode, string pTransactionName);
        TAMResult TransactionalInsert(IBusinessEntity[] pEntities, AccessMode pAccessMode, EInsertMethod pInsertMethod, string pTransactionName);
        TAMResult Update(IBusinessEntity[] pEntities, AccessMode pAccessMode);
        TAMResult TransactionalUpdate(IBusinessEntity[] pEntities, AccessMode pAccessMode, string pTransactionName);
        TAMResult Delete(IBusinessEntity[] pEntities, AccessMode pAccessMode);
        TAMResult TransactionalDelete(IBusinessEntity[] pEntities, AccessMode pAccessMode, string pTransactionName);
        void ReleaseEntity(IBusinessEntity[] pEntities);
        void ForciblyReleaseEntity(IBusinessEntity[] pEntities);
        TAMResult LockEntity(IBusinessEntity[] pEntities);
        TAMResult2<bool, LockInfo> LockEntity2(IBusinessEntity[] pEntities);
        TAMResult IsLocked(IBusinessEntity[] pEntities);
        TAMResult RequestMoreTime(IBusinessEntity[] pEntities);
        TAMResult2<LockInfo[]> GetAllLockedEntities();
        TAMResult2<LockInfo[]> IsLockedByUser(IBusinessEntity[] pEntities);
        TAMResult2<TransactionInfo[]> GetActiveTransactions();
        void OnInsert(byte[] pEntities);
        void OnUpdate(byte[] pOldEntities, byte[] pNewEntities);
        void OnDelete(byte[] pEntities);
        void OnBeforeExpiredTimeLockedEntity(byte[] pEntities);
        void OnExpiredTimeLockedEntity(byte[] pEntities);
        void Dispose();
        ServiceStatus GetStatus();
        LockResponse LockObject(string key, string userId);
        ReleaseLockResponse ReleaseObject(string key, string userId);
        bool IsObjectLocked(string key, string userId);
    }
}