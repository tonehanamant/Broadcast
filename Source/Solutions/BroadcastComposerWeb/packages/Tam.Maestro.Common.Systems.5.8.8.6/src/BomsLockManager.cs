using System;
using System.Linq;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;

namespace Common.Services
{
    public class BomsLockManager : IDisposable
    {
        private readonly ISMSClient _SmsClient;
        private readonly IBusinessEntity _BusinessEntity;

        public BomsLockManager(ISMSClient smsClient, IBusinessEntity businessEntity)
        {
            _SmsClient = smsClient;
            _BusinessEntity = businessEntity;

            var isLocked_response = _SmsClient.IsLocked(new IBusinessEntity[] { _BusinessEntity });
            if ((bool)isLocked_response.MultipleResult.First() == true)
            {
                throw new Exception("Cannot Perform the Operation on Traffic Order: " + _BusinessEntity.UniqueIdentifier + ". An Operation is already in Progress.");
            }
            var lock_response = _SmsClient.LockEntity(new IBusinessEntity[] { _BusinessEntity });
            if ((bool)lock_response.SingleResult != true)
            {
                throw new Exception("Could not lock the object.");
            }
        }

        public void Dispose()
        {
            _SmsClient.ReleaseEntity(new IBusinessEntity[] { _BusinessEntity });
        }
    }
}
