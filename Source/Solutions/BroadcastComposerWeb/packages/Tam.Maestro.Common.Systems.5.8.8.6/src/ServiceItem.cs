using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces
{
    [DataContract]
    [Serializable]
    public class ServiceItem
    {
        [DataMember]
        public int Port;
        [DataMember]
        public string Name;
        [DataMember]
        public string Host;


        public ServiceItem(int lPort, string lName, string lHost)
        {
            this.Port = lPort;
            this.Name = lName;
            this.Host = lHost;
        }

        public void Copy(ServiceItem pDest)
        {
            pDest.Name = this.Name;
            pDest.Host = this.Host;
            pDest.Port = this.Port;
        }
    }
}