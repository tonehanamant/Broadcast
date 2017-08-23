using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces
{
    [DataContract]
    [Serializable]
    public class ServiceManagerResourceEventArgs
    {
        [DataMember]
        public string OldValue;
        [DataMember]
        public string NewValue;
        [DataMember]
        public TAMEnvironment EnvironmentChanged;
        [DataMember]
        public TAMResource ResourceChanged;

        public ServiceManagerResourceEventArgs(string pOldValue, string pNewValue, TAMEnvironment pE, TAMResource pR)
        {
            this.OldValue = pOldValue;
            this.NewValue = pNewValue;
            this.EnvironmentChanged = pE;
            this.ResourceChanged = pR;
        }
    }
}