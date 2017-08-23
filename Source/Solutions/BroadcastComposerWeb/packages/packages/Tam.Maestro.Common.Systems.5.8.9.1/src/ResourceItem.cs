using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces
{
    [DataContract]
    [Serializable]
    public class ResourceItem
    {
        [DataMember]
        public string ResourceString;

        public ResourceItem(string lResourceString)
        {
            this.ResourceString = lResourceString;
        }

        public void Copy(ResourceItem pDest)
        {
            pDest.ResourceString = this.ResourceString;
        }

    }
}