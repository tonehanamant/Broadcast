using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces
{
    [DataContract]
    [Serializable]
    public class ServiceManagerServiceEventArgs
    {
        [DataMember]
        public string OldUri;
        [DataMember]
        public string NewUri;
        [DataMember]
        public TAMEnvironment EnvironmentChanged;
        [DataMember]
        public TAMService ServiceChanged;

        public ServiceManagerServiceEventArgs(string pOldUri, string pNewUri, TAMEnvironment pE, TAMService pS)
        {
            this.OldUri = pOldUri;
            this.NewUri = pNewUri;
            this.EnvironmentChanged = pE;
            this.ServiceChanged = pS;
        }
    }
}