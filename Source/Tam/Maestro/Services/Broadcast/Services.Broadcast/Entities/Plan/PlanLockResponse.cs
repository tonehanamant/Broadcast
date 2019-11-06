using System.Runtime.Serialization;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.Entities.Plan
{
    [DataContract]
    public class PlanLockResponse : LockResponse
    {
        [DataMember]
        public string PlanName { get; set; }
    }
}
