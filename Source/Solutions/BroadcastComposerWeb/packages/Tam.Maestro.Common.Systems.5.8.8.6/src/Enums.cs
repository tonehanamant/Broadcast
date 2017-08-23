using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Data.Entities
{
    [DataContract]
    [Serializable]
    public enum CMWImageEnums
    {
        [EnumMember]
        CMW_CADENT_LOGO = 1,
        [EnumMember]
        CMW_CADENT_HISPANIC_LOGO = 2,
        [EnumMember]
        CMW_CADENT_BROADCAST_LOGO = 3,
        [EnumMember]
        CMW_APEX_LOGO = 4,
        [EnumMember]
        CMW_CROSSAGENCY_LOGO = 5,
        [EnumMember]
        CMW_WIZEBUYS_LOGO = 6,
        [EnumMember]
        CMW_HMW_LOGO = 9,
        [EnumMember]
        CMW_IMW_LOGO = 10,
    }
}