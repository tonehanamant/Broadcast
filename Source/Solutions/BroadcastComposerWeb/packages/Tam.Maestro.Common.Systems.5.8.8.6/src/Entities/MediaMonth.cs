using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;

namespace Tam.Maestro.Data.Entities
{
    [DataContract]
    [Serializable]
    public enum MediaMonthColumns
    {
        [EnumMember]
        Id = 0,
        [EnumMember]
        Year = 1,
        [EnumMember]
        Month = 2,
        [EnumMember]
        MediaMonthX = 3,
        [EnumMember]
        StartDate = 4,
        [EnumMember]
        EndDate = 5
    }

    [DataContract]
    [Serializable]
    [ProtoBuf.ProtoContract]
    public partial class MediaMonth : IBusinessEntity, ICloneable, INotifyPropertyChanged
    {
        public const short TOTAL_FIELDS = 6;
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
        /// media_months.id
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(1)]
        private int _Id;
        /// <summary>
        /// media_months.year
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(2)]
        private int _Year;
        /// <summary>
        /// media_months.month
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(3)]
        private int _Month;
        /// <summary>
        /// media_months.media_month
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(4)]
        private string _MediaMonthX;
        /// <summary>
        /// media_months.start_date
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(5)]
        private DateTime _StartDate;
        /// <summary>
        /// media_months.end_date
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(6)]
        private DateTime _EndDate;

        /// <summary>
        /// Determines if a field in the object has been modified after it's been initialized by one if its constructors.
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(7)]
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
        /// Unique Identifier
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
        /// Gets and Sets the Year Property.
        /// The year the media month belongs to.
        /// </summary>
        public int Year
        {
            get { return (this._Year); }
            set
            {
                if (this._Year != value)
                {
                    this._Year = value;
                    this.IsDirty = true;
                    OnPropertyChanged("Year");
                }
                else
                    this._Year = value;
            }
        }
        /// <summary>
        /// Gets and Sets the Month Property.
        /// The month the media month belongs to.
        /// </summary>
        public int Month
        {
            get { return (this._Month); }
            set
            {
                if (this._Month != value)
                {
                    this._Month = value;
                    this.IsDirty = true;
                    OnPropertyChanged("Month");
                }
                else
                    this._Month = value;
            }
        }
        /// <summary>
        /// Gets and Sets the MediaMonthX Property.
        /// The textual representation of the media month.
        /// </summary>
        public string MediaMonthX
        {
            get { return (this._MediaMonthX); }
            set
            {
                if (this._MediaMonthX != value)
                {
                    this._MediaMonthX = value;
                    this.IsDirty = true;
                    OnPropertyChanged("MediaMonthX");
                }
                else
                    this._MediaMonthX = value;
            }
        }
        /// <summary>
        /// Gets and Sets the StartDate Property.
        /// The start date of the media month.
        /// </summary>
        /// <remarks>
        /// This should correspond with the start date of the first week in the media month.
        /// </remarks>
        public DateTime StartDate
        {
            get { return (this._StartDate); }
            set
            {
                if (this._StartDate != value)
                {
                    this._StartDate = value;
                    this.IsDirty = true;
                    OnPropertyChanged("StartDate");
                }
                else
                    this._StartDate = value;
            }
        }
        /// <summary>
        /// Gets and Sets the EndDate Property.
        /// The end date of the media month.
        /// </summary>
        /// <remarks>
        /// This should correspond with the end date of the last week in the media month.
        /// </remarks>
        public DateTime EndDate
        {
            get { return (this._EndDate); }
            set
            {
                if (this._EndDate != value)
                {
                    this._EndDate = value;
                    this.IsDirty = true;
                    OnPropertyChanged("EndDate");
                }
                else
                    this._EndDate = value;
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
        public MediaMonth()
        {
            this.IsDirty = false;
        }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <remarks>
        /// Convenient for use with a DataRow.ItemArray property.
        /// </remarks>
        public MediaMonth(object[] pItems)
            : this(pItems, 0) { }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <remarks>
        /// Convenient for use with a DataRow.ItemArray property offset by a specified number of columns.
        /// </remarks>
        public MediaMonth(object[] pItems, int pOffset)
        {
            this.Id = (int)pItems[0 + pOffset];
            this.Year = (int)pItems[1 + pOffset];
            this.Month = (int)pItems[2 + pOffset];
            this.MediaMonthX = (string)pItems[3 + pOffset];
            this.StartDate = (DateTime)pItems[4 + pOffset];
            this.EndDate = (DateTime)pItems[5 + pOffset];
            this.IsDirty = false;
        }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <param name="pId">Unique Identifier</param>
        /// <param name="pYear">The year the media month belongs to.</param>
        /// <param name="pMonth">The month the media month belongs to.</param>
        /// <param name="pMediaMonth">The textual representation of the media month.</param>
        /// <param name="pStartDate">The start date of the media month.</param>
        /// <param name="pEndDate">The end date of the media month.</param>
        public MediaMonth(int pId, int pYear, int pMonth, string pMediaMonthX, DateTime pStartDate, DateTime pEndDate)
        {
            this._Id = pId;
            this._Year = pYear;
            this._Month = pMonth;
            this._MediaMonthX = pMediaMonthX;
            this._StartDate = pStartDate;
            this._EndDate = pEndDate;
            this.IsDirty = false;
        }
        /// <summary>
        /// Constructor used for copying from another entity.
        /// </summary>
        /// <param name="pMediaMonth">The MediaMonth object to copy from.</param>
        public MediaMonth(MediaMonth pMediaMonth)
        {
            this._Id = pMediaMonth.Id;
            this._Year = pMediaMonth.Year;
            this._Month = pMediaMonth.Month;
            this._MediaMonthX = pMediaMonth.MediaMonthX;
            this._StartDate = pMediaMonth.StartDate;
            this._EndDate = pMediaMonth.EndDate;
            this.IsDirty = false;
        }

        public override int GetHashCode()
        {
            return ((this.Id.ToString()).GetHashCode());
        }
        public override bool Equals(object obj)
        {
            if (!(obj is MediaMonth))
                return (false);
            return (this.GetHashCode() == obj.GetHashCode());
        }
        public override string ToString()
        {
            return ("MediaMonth [Id = " + this.Id.ToString() + "]");
        }
        public static bool operator ==(MediaMonth a, MediaMonth b)
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
                a.Year == b.Year &&
                a.Month == b.Month &&
                a.MediaMonthX == b.MediaMonthX &&
                a.StartDate == b.StartDate &&
                a.EndDate == b.EndDate);
        }
        public static bool operator !=(MediaMonth a, MediaMonth b)
        {
            return !(a == b);
        }
        public object Clone()
        {
            return (
                new MediaMonth(
                    this._Id,
                    this._Year,
                    this._Month,
                    this._MediaMonthX,
                    this._StartDate,
                    this._EndDate));
        }
    }
}
