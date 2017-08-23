using System;
using System.Runtime.Serialization;

namespace Tam.Maestro.Data.Entities
{
    [DataContract]
    [Serializable]
    public enum EmployeeColumns
    {
        [EnumMember]
        Id = 0,
        [EnumMember]
        Username = 1,
        [EnumMember]
        Accountdomainsid = 2,
        [EnumMember]
        Firstname = 3,
        [EnumMember]
        Lastname = 4,
        [EnumMember]
        Mi = 5,
        [EnumMember]
        Email = 6,
        [EnumMember]
        Phone = 7,
        [EnumMember]
        InternalExtension = 8,
        [EnumMember]
        Status = 9,
        [EnumMember]
        Datecreated = 10,
        [EnumMember]
        Datelastlogin = 11,
        [EnumMember]
        Datelastmodified = 12,
        [EnumMember]
        Hitcount = 13
    }

    [DataContract]
    [Serializable]
    public partial class Employee : IBusinessEntity, ICloneable
    {
        /// <summary>
        /// employees.id
        /// </summary>
        [DataMember]
        private int _Id;
        /// <summary>
        /// employees.username
        /// </summary>
        [DataMember]
        private string _Username;
        /// <summary>
        /// employees.accountdomainsid
        /// </summary>
        [DataMember]
        private string _Accountdomainsid;
        /// <summary>
        /// employees.firstname
        /// </summary>
        [DataMember]
        private string _Firstname;
        /// <summary>
        /// employees.lastname
        /// </summary>
        [DataMember]
        private string _Lastname;
        /// <summary>
        /// employees.mi
        /// </summary>
        [DataMember]
        private string _Mi;
        /// <summary>
        /// employees.email
        /// </summary>
        [DataMember]
        private string _Email;
        /// <summary>
        /// employees.phone
        /// </summary>
        [DataMember]
        private string _Phone;
        /// <summary>
        /// employees.internal_extension
        /// </summary>
        [DataMember]
        private string _InternalExtension;
        /// <summary>
        /// employees.status
        /// </summary>
        [DataMember]
        private byte _Status;
        /// <summary>
        /// employees.datecreated
        /// </summary>
        [DataMember]
        private DateTime _Datecreated;
        /// <summary>
        /// employees.datelastlogin
        /// </summary>
        [DataMember]
        private DateTime _Datelastlogin;
        /// <summary>
        /// employees.datelastmodified
        /// </summary>
        [DataMember]
        private DateTime _Datelastmodified;
        /// <summary>
        /// employees.hitcount
        /// </summary>
        [DataMember]
        private int _Hitcount;

        /// <summary>
        /// Gets and Sets the Id Property.
        /// </summary>
        public int Id
        {
            get { return (this._Id); }
            set { this._Id = value; }
        }
        /// <summary>
        /// Gets and Sets the Username Property.
        /// </summary>
        public string Username
        {
            get { return (this._Username); }
            set { this._Username = value; }
        }
        /// <summary>
        /// Gets and Sets the Accountdomainsid Property.
        /// </summary>
        public string Accountdomainsid
        {
            get { return (this._Accountdomainsid); }
            set { this._Accountdomainsid = value; }
        }
        /// <summary>
        /// Gets and Sets the Firstname Property.
        /// </summary>
        public string Firstname
        {
            get { return (this._Firstname); }
            set { this._Firstname = value; }
        }
        /// <summary>
        /// Gets and Sets the Lastname Property.
        /// </summary>
        public string Lastname
        {
            get { return (this._Lastname); }
            set { this._Lastname = value; }
        }
        /// <summary>
        /// Gets and Sets the Mi Property.
        /// </summary>
        public string Mi
        {
            get { return (this._Mi); }
            set { this._Mi = value; }
        }
        /// <summary>
        /// Gets and Sets the Email Property.
        /// </summary>
        public string Email
        {
            get { return (this._Email); }
            set { this._Email = value; }
        }
        /// <summary>
        /// Gets and Sets the Phone Property.
        /// </summary>
        public string Phone
        {
            get { return (this._Phone); }
            set { this._Phone = value; }
        }
        /// <summary>
        /// Gets and Sets the InternalExtension Property.
        /// </summary>
        public string InternalExtension
        {
            get { return (this._InternalExtension); }
            set { this._InternalExtension = value; }
        }
        /// <summary>
        /// Gets and Sets the Status Property.
        /// </summary>
        public byte Status
        {
            get { return (this._Status); }
            set { this._Status = value; }
        }
        /// <summary>
        /// Gets and Sets the Datecreated Property.
        /// </summary>
        public DateTime Datecreated
        {
            get { return (this._Datecreated); }
            set { this._Datecreated = value; }
        }
        /// <summary>
        /// Gets and Sets the Datelastlogin Property.
        /// </summary>
        public DateTime Datelastlogin
        {
            get { return (this._Datelastlogin); }
            set { this._Datelastlogin = value; }
        }
        /// <summary>
        /// Gets and Sets the Datelastmodified Property.
        /// </summary>
        public DateTime Datelastmodified
        {
            get { return (this._Datelastmodified); }
            set { this._Datelastmodified = value; }
        }
        /// <summary>
        /// Gets and Sets the Hitcount Property.
        /// </summary>
        public int Hitcount
        {
            get { return (this._Hitcount); }
            set { this._Hitcount = value; }
        }

        /// <summary>
        /// Guarenteed to return a unique string to identify this object.
        /// </summary>
        /// <remarks>This string is based solely on the primary key of the object.</remarks>
        public string UniqueIdentifier
        {
            get { return (this.Id.ToString().ToLower()); }
        }

        /// <summary>
        /// Default constructor used for creating a new entity.
        /// </summary>
        public Employee() { }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <remarks>
        /// Convenient for use with a DataRow.ItemArray property.
        /// </remarks>
        public Employee(object[] pItems)
        {
            this._Id = (int)pItems[0];
            this._Username = (string)pItems[1];
            this._Accountdomainsid = (string)pItems[2];
            this._Firstname = (string)pItems[3];
            this._Lastname = (string)pItems[4];
            this._Mi = (string)pItems[5];
            this._Email = (string)pItems[6];
            this._Phone = pItems[7] is DBNull ? null : (string)pItems[7];
            this._InternalExtension = pItems[8] is DBNull ? null : (string)pItems[8];
            this._Status = (byte)pItems[9];
            this._Datecreated = (DateTime)pItems[10];
            this._Datelastlogin = (DateTime)pItems[11];
            this._Datelastmodified = (DateTime)pItems[12];
            this._Hitcount = (int)pItems[13];
        }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <remarks>
        /// Convenient for use with a DataRow.ItemArray property offset by a specified number of columns.
        /// </remarks>
        public Employee(object[] pItems, int pOffset)
        {
            this._Id = (int)pItems[0 + pOffset];
            this._Username = (string)pItems[1 + pOffset];
            this._Accountdomainsid = (string)pItems[2 + pOffset];
            this._Firstname = (string)pItems[3 + pOffset];
            this._Lastname = (string)pItems[4 + pOffset];
            this._Mi = (string)pItems[5 + pOffset];
            this._Email = (string)pItems[6 + pOffset];
            this._Phone = pItems[7 + pOffset] is DBNull ? null : (string)pItems[7 + pOffset];
            this._InternalExtension = pItems[8 + pOffset] is DBNull ? null : (string)pItems[8 + pOffset];
            this._Status = (byte)pItems[9 + pOffset];
            this._Datecreated = (DateTime)pItems[10 + pOffset];
            this._Datelastlogin = (DateTime)pItems[11 + pOffset];
            this._Datelastmodified = (DateTime)pItems[12 + pOffset];
            this._Hitcount = (int)pItems[13 + pOffset];
        }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pUsername"></param>
        /// <param name="pAccountdomainsid"></param>
        /// <param name="pFirstname"></param>
        /// <param name="pLastname"></param>
        /// <param name="pMi"></param>
        /// <param name="pEmail"></param>
        /// <param name="pPhone"></param>
        /// <param name="pInternalExtension"></param>
        /// <param name="pStatus"></param>
        /// <param name="pDatecreated"></param>
        /// <param name="pDatelastlogin"></param>
        /// <param name="pDatelastmodified"></param>
        /// <param name="pHitcount"></param>
        public Employee(int pId, string pUsername, string pAccountdomainsid, string pFirstname, string pLastname, string pMi, string pEmail, string pPhone, string pInternalExtension, byte pStatus, DateTime pDatecreated, DateTime pDatelastlogin, DateTime pDatelastmodified, int pHitcount)
        {
            this._Id = pId;
            this._Username = pUsername;
            this._Accountdomainsid = pAccountdomainsid;
            this._Firstname = pFirstname;
            this._Lastname = pLastname;
            this._Mi = pMi;
            this._Email = pEmail;
            this._Phone = pPhone;
            this._InternalExtension = pInternalExtension;
            this._Status = pStatus;
            this._Datecreated = pDatecreated;
            this._Datelastlogin = pDatelastlogin;
            this._Datelastmodified = pDatelastmodified;
            this._Hitcount = pHitcount;
        }

        public override int GetHashCode()
        {
            return ((this.Id.ToString()).GetHashCode());
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Employee))
                return (false);
            return (this.GetHashCode() == obj.GetHashCode());
        }
        public override string ToString()
        {
            return ("Employee [Id = " + this.Id.ToString() + "]");
        }
        public static bool operator ==(Employee a, Employee b)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
                return false;

            // Return true if the fields match:
            return (
                a.Id == b.Id &&
                a.Username == b.Username &&
                a.Accountdomainsid == b.Accountdomainsid &&
                a.Firstname == b.Firstname &&
                a.Lastname == b.Lastname &&
                a.Mi == b.Mi &&
                a.Email == b.Email &&
                a.Phone == b.Phone &&
                a.InternalExtension == b.InternalExtension &&
                a.Status == b.Status &&
                a.Datecreated == b.Datecreated &&
                a.Datelastlogin == b.Datelastlogin &&
                a.Datelastmodified == b.Datelastmodified &&
                a.Hitcount == b.Hitcount);
        }
        public static bool operator !=(Employee a, Employee b)
        {
            return !(a == b);
        }
        public object Clone()
        {
            return (
                new Employee(
                    this._Id,
                    this._Username,
                    this._Accountdomainsid,
                    this._Firstname,
                    this._Lastname,
                    this._Mi,
                    this._Email,
                    this._Phone,
                    this._InternalExtension,
                    this._Status,
                    this._Datecreated,
                    this._Datelastlogin,
                    this._Datelastmodified,
                    this._Hitcount));
        }
    }
}
