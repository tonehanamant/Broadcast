using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Data.Entities
{
    [DataContract]
    [Serializable]
    public class LockInfo
    {
        [DataMember]
        public IBusinessEntity Entity;
        [DataMember]
        public string Identifier;
        [DataMember]
        public int SecondsToSpart;
        [DataMember]
        public string UserName;
        [DataMember]
        public string DomainSId;

        public LockInfo(IBusinessEntity pEntity, string pIdentifier, int pSecondsToSpart)
        {
            this.Entity = pEntity;
            this.Identifier = pIdentifier;
            this.SecondsToSpart = pSecondsToSpart;
            this.UserName = "";
            this.DomainSId = "";
        }
        public LockInfo(IBusinessEntity pEntity, string pIdentifier, int pSecondsToSpart, string pUserName, string pDomainSId)
        {
            this.Entity = pEntity;
            this.Identifier = pIdentifier;
            this.SecondsToSpart = pSecondsToSpart;
            this.UserName = pUserName;
            this.DomainSId = pDomainSId;
        }
    }
}
