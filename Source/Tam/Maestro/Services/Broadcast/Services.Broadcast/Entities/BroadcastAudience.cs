using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;

namespace Services.Broadcast.Entities
{
    public enum EBroadcastAudienceCategoryCode : byte
    {
        [EnumMember]
        Sex = 0,
        [EnumMember]
        Income = 1,
        [EnumMember]
        Ethnicity = 2,
        [EnumMember]
        Other = 3
    }

    [DataContract]
    [Serializable]
    public enum AudienceColumns
    {
        [EnumMember]
        Id = 0,
        [EnumMember]
        CategoryCode = 1,
        [EnumMember]
        SubCategoryCode = 2,
        [EnumMember]
        RangeStart = 3,
        [EnumMember]
        RangeEnd = 4,
        [EnumMember]
        Custom = 5,
        [EnumMember]
        Code = 6,
        [EnumMember]
        Name = 7
    }

    [DataContract]
    [Serializable]
    public partial class BroadcastAudience : ICloneable, INotifyPropertyChanged
    {
        public const short TOTAL_FIELDS = 8;
        #region OnPropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Triggers the PropertyChanged event.
        /// </summary>
        public virtual void OnPropertyChanged(string propertyName)
        {
            // Copy a reference to the delegate field into a temporary field for thread safety
            PropertyChangedEventHandler deleg = Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (deleg != null)
                deleg(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        /// <summary>
        /// audiences.id
        /// </summary>
        [DataMember]
        private int _Id;
        /// <summary>
        /// audiences.category_code
        /// </summary>
        [DataMember]
        private EBroadcastAudienceCategoryCode _CategoryCode;
        /// <summary>
        /// audiences.sub_category_code
        /// </summary>
        [DataMember]
        private string _SubCategoryCode;
        /// <summary>
        /// audiences.range_start
        /// </summary>
        [DataMember]
        private int? _RangeStart;
        /// <summary>
        /// audiences.range_end
        /// </summary>
        [DataMember]
        private int? _RangeEnd;
        /// <summary>
        /// audiences.custom
        /// </summary>
        [DataMember]
        private bool _Custom;
        /// <summary>
        /// audiences.code
        /// </summary>
        [DataMember]
        private string _Code;
        /// <summary>
        /// audiences.name
        /// </summary>
        [DataMember]
        private string _Name;

        /// <summary>
        /// Determines if a field in the object has been modified after it's been initialized by one if its constructors.
        /// </summary>
        [DataMember]
        public bool IsDirty { get; private set; }
        /// <summary>
        /// Let's you know if this object is new or not based on it's identity value.
        /// </summary>
        public bool IsNew
        {
            get { return (this._Id <= 0); }
        }

        /// <summary>
        /// Gets and Sets the Id Property.
        /// </summary>
        public int Id
        {
            get { return (this._Id); }
            set
            {
                if (this._Id != value)
                {
                    this._Id = value;
                    this.IsDirty = true;
                    OnPropertyChanged("Id");
                }
                else
                    this._Id = value;
            }
        }
        /// <summary>
        /// Gets and Sets the CategoryCode Property.
        /// </summary>
        public EBroadcastAudienceCategoryCode CategoryCode
        {
            get { return (this._CategoryCode); }
            set
            {
                if (this._CategoryCode != value)
                {
                    this._CategoryCode = value;
                    this.IsDirty = true;
                    OnPropertyChanged("CategoryCode");
                }
                else
                    this._CategoryCode = value;
            }
        }
        /// <summary>
        /// Gets and Sets the SubCategoryCode Property.
        /// </summary>
        public string SubCategoryCode
        {
            get { return (this._SubCategoryCode); }
            set
            {
                if (this._SubCategoryCode != value)
                {
                    this._SubCategoryCode = value;
                    this.IsDirty = true;
                    OnPropertyChanged("SubCategoryCode");
                }
                else
                    this._SubCategoryCode = value;
            }
        }
        /// <summary>
        /// Gets and Sets the RangeStart Property.
        /// </summary>
        public int? RangeStart
        {
            get { return (this._RangeStart); }
            set
            {
                if (this._RangeStart != value)
                {
                    this._RangeStart = value;
                    this.IsDirty = true;
                    OnPropertyChanged("RangeStart");
                }
                else
                    this._RangeStart = value;
            }
        }
        /// <summary>
        /// Gets and Sets the RangeEnd Property.
        /// </summary>
        public int? RangeEnd
        {
            get { return (this._RangeEnd); }
            set
            {
                if (this._RangeEnd != value)
                {
                    this._RangeEnd = value;
                    this.IsDirty = true;
                    OnPropertyChanged("RangeEnd");
                }
                else
                    this._RangeEnd = value;
            }
        }
        /// <summary>
        /// Gets and Sets the Custom Property.
        /// </summary>
        public bool Custom
        {
            get { return (this._Custom); }
            set
            {
                if (this._Custom != value)
                {
                    this._Custom = value;
                    this.IsDirty = true;
                    OnPropertyChanged("Custom");
                }
                else
                    this._Custom = value;
            }
        }
        /// <summary>
        /// Gets and Sets the Code Property.
        /// </summary>
        public string Code
        {
            get { return (this._Code); }
            set
            {
                if (this._Code != value)
                {
                    this._Code = value;
                    this.IsDirty = true;
                    OnPropertyChanged("Code");
                }
                else
                    this._Code = value;
            }
        }
        /// <summary>
        /// Gets and Sets the Name Property.
        /// </summary>
        public string Name
        {
            get { return (this._Name); }
            set
            {
                if (this._Name != value)
                {
                    this._Name = value;
                    this.IsDirty = true;
                    OnPropertyChanged("Name");
                }
                else
                    this._Name = value;
            }
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
        public BroadcastAudience()
        {
            this.IsDirty = false;
        }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <remarks>
        /// Convenient for use with a DataRow.ItemArray property.
        /// </remarks>
        public BroadcastAudience(object[] pItems)
            : this(pItems, 0) { }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <remarks>
        /// Convenient for use with a DataRow.ItemArray property offset by a specified number of columns.
        /// </remarks>
        public BroadcastAudience(object[] pItems, int pOffset)
        {
            this.Id = (int)pItems[0 + pOffset];
            this.CategoryCode = (EBroadcastAudienceCategoryCode)(byte)pItems[1 + pOffset];
            this.SubCategoryCode = (string)pItems[2 + pOffset];
            this.RangeStart = pItems[3 + pOffset] is DBNull ? null : (int?)pItems[3 + pOffset];
            this.RangeEnd = pItems[4 + pOffset] is DBNull ? null : (int?)pItems[4 + pOffset];
            this.Custom = (bool)pItems[5 + pOffset];
            this.Code = (string)pItems[6 + pOffset];
            this.Name = (string)pItems[7 + pOffset];
            this.IsDirty = false;
        }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="pCategoryCode"></param>
        /// <param name="pSubCategoryCode"></param>
        /// <param name="pRangeStart"></param>
        /// <param name="pRangeEnd"></param>
        /// <param name="pCustom"></param>
        /// <param name="pCode"></param>
        /// <param name="pName"></param>
        public BroadcastAudience(int pId, EBroadcastAudienceCategoryCode pCategoryCode, string pSubCategoryCode, int? pRangeStart, int? pRangeEnd, bool pCustom, string pCode, string pName)
        {
            this._Id = pId;
            this._CategoryCode = pCategoryCode;
            this._SubCategoryCode = pSubCategoryCode;
            this._RangeStart = pRangeStart;
            this._RangeEnd = pRangeEnd;
            this._Custom = pCustom;
            this._Code = pCode;
            this._Name = pName;
            this.IsDirty = false;
        }
        /// <summary>
        /// Constructor used for copying from another entity.
        /// </summary>
        /// <param name="pBroadcastAudience">The Audience object to copy from.</param>
        public BroadcastAudience(BroadcastAudience pBroadcastAudience)
        {
            this._Id = pBroadcastAudience.Id;
            this._CategoryCode = pBroadcastAudience.CategoryCode;
            this._SubCategoryCode = pBroadcastAudience.SubCategoryCode;
            this._RangeStart = pBroadcastAudience.RangeStart;
            this._RangeEnd = pBroadcastAudience.RangeEnd;
            this._Custom = pBroadcastAudience.Custom;
            this._Code = pBroadcastAudience.Code;
            this._Name = pBroadcastAudience.Name;
            this.IsDirty = false;
        }

        public override int GetHashCode()
        {
            return ((this.Id.ToString()).GetHashCode());
        }
        public override bool Equals(object obj)
        {
            if (!(obj is BroadcastAudience))
                return (false);
            return (this.GetHashCode() == obj.GetHashCode());
        }
        public override string ToString()
        {
            return ("Audience [Id = " + this.Id.ToString() + "]");
        }
        public static bool operator ==(BroadcastAudience a, BroadcastAudience b)
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
                a.CategoryCode == b.CategoryCode &&
                a.SubCategoryCode == b.SubCategoryCode &&
                a.RangeStart == b.RangeStart &&
                a.RangeEnd == b.RangeEnd &&
                a.Custom == b.Custom &&
                a.Code == b.Code &&
                a.Name == b.Name);
        }
        public static bool operator !=(BroadcastAudience a, BroadcastAudience b)
        {
            return !(a == b);
        }
        public object Clone()
        {
            return (
                new BroadcastAudience(
                    this._Id,
                    this._CategoryCode,
                    this._SubCategoryCode,
                    this._RangeStart,
                    this._RangeEnd,
                    this._Custom,
                    this._Code,
                    this._Name));
        }
    }
}
