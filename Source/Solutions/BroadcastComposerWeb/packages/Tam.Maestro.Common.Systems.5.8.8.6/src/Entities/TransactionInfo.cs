using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Data.Entities
{
    [DataContract]
    [Serializable]
    public class TransactionInfo
    {
        [DataMember]
        public uint CallbackMappingNumber;
        [DataMember]
        public string TransactionName;
        [DataMember]
        public string[] PendingCommitts;

        public TransactionInfo(uint pCallbackMappingNumber, string pTransactionName)
        {
            this.CallbackMappingNumber = pCallbackMappingNumber;
            this.TransactionName = pTransactionName;
        }
    }
}
