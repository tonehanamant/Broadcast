using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces
{
    [DataContract]
    [Serializable]
    public enum TAMEnvironment
    {
        [EnumMember]
        [Description("Development (DEV)")]
        DEV = 0,
        [EnumMember]
        [Description("Quality Assurance (QA)")]
        QA = 1,
        [EnumMember]
        [Description("User Acceptance Testing (UAT)")]
        UAT = 2,
        [EnumMember]
        [Description("Production (PROD)")]
        PROD = 3,
        [EnumMember]
        [Description("Local (LOCAL)")]
        LOCAL = 4
    };
}