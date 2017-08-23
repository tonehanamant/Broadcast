using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces
{
    [DataContract]
    [Serializable]
    public class ReleaseLockResponse
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Error { get; set; }
    }
}
