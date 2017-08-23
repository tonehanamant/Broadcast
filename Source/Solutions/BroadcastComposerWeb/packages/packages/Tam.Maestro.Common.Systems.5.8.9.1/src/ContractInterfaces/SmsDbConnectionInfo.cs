using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces.Common
{
    [DataContract]
    [Serializable]
    public class SmsDbConnectionInfo
    {
        [DataMember]
        public TAMResource TamResource { get; set; }
        [DataMember]
        public string Server { get; set; }
        [DataMember]
        public string Database { get; set; }

        public SmsDbConnectionInfo(TAMResource pTamResource, string pServer, string pDatabase)
        {
            this.TamResource = pTamResource;
            this.Server = pServer;
            this.Database = pDatabase;
        }

        public override string ToString()
        {
            return this.Server + (!string.IsNullOrEmpty(this.Server) ? "." : "") + this.Database;
        }
    }
}
