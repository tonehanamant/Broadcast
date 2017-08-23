using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces
{
    [DataContract]
    [Serializable]
    public class LockResponse
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public int LockTimeoutInSeconds { get; set; }

        [DataMember]
        public string LockedUserId { get; set; }

        [DataMember]
        public string LockedUserName { get; set; }

        [DataMember]
        public string Error { get; set; }
    }
}
