using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;

namespace Tam.Maestro.Data.Entities
{
    [DataContract]
    [Serializable]
    public enum MediaWeekColumns
    {
        [EnumMember]
        Id = 0,
        [EnumMember]
        MediaMonthId = 1,
        [EnumMember]
        WeekNumber = 2,
        [EnumMember]
        StartDate = 3,
        [EnumMember]
        EndDate = 4
    }

    [DataContract]
    [Serializable]
    [ProtoBuf.ProtoContract]
    public partial class MediaWeek : IBusinessEntity, ICloneable, INotifyPropertyChanged
    {
        public const short TOTAL_FIELDS = 5;
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
        /// media_weeks.id
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(1)]
        private int _Id;
        /// <summary>
        /// media_weeks.media_month_id
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(2)]
        private int _MediaMonthId;
        /// <summary>
        /// media_weeks.week_number
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(3)]
        private int _WeekNumber;
        /// <summary>
        /// media_weeks.start_date
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(4)]
        private DateTime _StartDate;
        /// <summary>
        /// media_weeks.end_date
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(5)]
        private DateTime _EndDate;

        /// <summary>
        /// Determines if a field in the object has been modified after it's been initialized by one if its constructors.
        /// </summary>
        [DataMember]
        [ProtoBuf.ProtoMember(6)]
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
        /// Gets and Sets the MediaMonthId Property.
        /// A reference to the media month this media week belongs to.
        /// </summary>
        public int MediaMonthId
        {
            get { return (this._MediaMonthId); }
            set
            {
                if (this._MediaMonthId != value)
                {
                    this._MediaMonthId = value;
                    this.IsDirty = true;
                    OnPropertyChanged("MediaMonthId");
                }
                else
                    this._MediaMonthId = value;
            }
        }
        /// <summary>
        /// Gets and Sets the WeekNumber Property.
        /// The number of the week in the media month.
        /// </summary>
        /// <remarks>
        /// Sorting by this lowest to highest for a given media month will give you the proper order of the weeks.
        /// </remarks>
        public int WeekNumber
        {
            get { return (this._WeekNumber); }
            set
            {
                if (this._WeekNumber != value)
                {
                    this._WeekNumber = value;
                    this.IsDirty = true;
                    OnPropertyChanged("WeekNumber");
                }
                else
                    this._WeekNumber = value;
            }
        }
        /// <summary>
        /// Gets and Sets the StartDate Property.
        /// Start date of the media week.
        /// </summary>
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
        /// End date of the media week.
        /// </summary>
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
        public MediaWeek()
        {
            this.IsDirty = false;
        }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <remarks>
        /// Convenient for use with a DataRow.ItemArray property.
        /// </remarks>
        public MediaWeek(object[] pItems)
            : this(pItems, 0) { }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <remarks>
        /// Convenient for use with a DataRow.ItemArray property offset by a specified number of columns.
        /// </remarks>
        public MediaWeek(object[] pItems, int pOffset)
        {
            this.Id = (int)pItems[0 + pOffset];
            this.MediaMonthId = (int)pItems[1 + pOffset];
            this.WeekNumber = (int)pItems[2 + pOffset];
            this.StartDate = (DateTime)pItems[3 + pOffset];
            this.EndDate = (DateTime)pItems[4 + pOffset];
            this.IsDirty = false;
        }
        /// <summary>
        /// Constructor used for pulling the entity from the database.
        /// </summary>
        /// <param name="pId">Unique Identifier</param>
        /// <param name="pMediaMonthId">A reference to the media month this media week belongs to.</param>
        /// <param name="pWeekNumber">The number of the week in the media month.</param>
        /// <param name="pStartDate">Start date of the media week.</param>
        /// <param name="pEndDate">End date of the media week.</param>
        public MediaWeek(int pId, int pMediaMonthId, int pWeekNumber, DateTime pStartDate, DateTime pEndDate)
        {
            this._Id = pId;
            this._MediaMonthId = pMediaMonthId;
            this._WeekNumber = pWeekNumber;
            this._StartDate = pStartDate;
            this._EndDate = pEndDate;
            this.IsDirty = false;
        }
        /// <summary>
        /// Constructor used for copying from another entity.
        /// </summary>
        /// <param name="pMediaWeek">The MediaWeek object to copy from.</param>
        public MediaWeek(MediaWeek pMediaWeek)
        {
            this._Id = pMediaWeek.Id;
            this._MediaMonthId = pMediaWeek.MediaMonthId;
            this._WeekNumber = pMediaWeek.WeekNumber;
            this._StartDate = pMediaWeek.StartDate;
            this._EndDate = pMediaWeek.EndDate;
            this.IsDirty = false;
        }

        public override int GetHashCode()
        {
            return ((this.Id.ToString()).GetHashCode());
        }
        public override bool Equals(object obj)
        {
            if (!(obj is MediaWeek))
                return (false);
            return (this.GetHashCode() == obj.GetHashCode());
        }
        public override string ToString()
        {
            return ("MediaWeek [Id = " + this.Id.ToString() + "]");
        }
        public static bool operator ==(MediaWeek a, MediaWeek b)
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
                a.MediaMonthId == b.MediaMonthId &&
                a.WeekNumber == b.WeekNumber &&
                a.StartDate == b.StartDate &&
                a.EndDate == b.EndDate);
        }
        public static bool operator !=(MediaWeek a, MediaWeek b)
        {
            return !(a == b);
        }
        public object Clone()
        {
            return (
                new MediaWeek(
                    this._Id,
                    this._MediaMonthId,
                    this._WeekNumber,
                    this._StartDate,
                    this._EndDate));
        }
    }
}
