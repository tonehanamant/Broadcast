using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Data.Entities
{
    [DataContract]
    [Serializable]
    public enum ServiceStatus
    {
        [EnumMember]
        Open = 0,
        [EnumMember]
        Working = 1,
        [EnumMember]
        Idle = 2,
        [EnumMember]
        Closed = 3
    }

    [DataContract]
    [Serializable]
    public enum EntityAction
    {
        [EnumMember]
        Insert = 0,
        [EnumMember]
        Update,
        [EnumMember]
        None,
        [EnumMember]
        Delete,
        [EnumMember]
        BeforeTimeExpired,
        [EnumMember]
        TimeExpired
    }

    [DataContract]
    [Serializable]
    public enum AccessMode
    {
        [EnumMember]
        OnebyOne = 0,
        [EnumMember]
        Bulk
    }

    [DataContract]
    [Serializable]
    public enum EInsertMethod
    {
        /// <summary>
        /// If the table being inserted into has an identity then that identity will be returned, otherwise if
        /// the table does not have an identity then the insert will be optimized using bulk insert.
        /// </summary>
        [EnumMember]
        Dynamic = 0,
        /// <summary>
        /// Whether or not the table being inserted into has an identity or not the insert will be optimized using bulk insert.
        /// Identities will NOT be returned.
        /// </summary>
        [EnumMember]
        Bulk = 1
    }

    public interface IBusinessEntity
    {
        string UniqueIdentifier
        {
            get;
        }
    }
}
