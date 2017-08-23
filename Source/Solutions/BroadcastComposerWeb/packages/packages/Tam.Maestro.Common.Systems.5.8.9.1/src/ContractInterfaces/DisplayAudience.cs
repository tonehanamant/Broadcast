using ProtoBuf;
using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects
{
    [DataContract]
    [Serializable]
    [ProtoContract]
    public class DisplayAudience
    {
        [DataMember]
        [ProtoMember(1)]
        public int Id;
        [DataMember]
        [ProtoMember(2)]
        public string AudienceString;

        public DisplayAudience()
        {
        }

        public DisplayAudience(int pId, string pAudienceString)
        {
            this.Id = pId;
            this.AudienceString = pAudienceString;
        }
    }
}
