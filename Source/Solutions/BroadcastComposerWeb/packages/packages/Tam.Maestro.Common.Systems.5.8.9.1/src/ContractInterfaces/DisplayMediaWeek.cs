using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Tam.Maestro.Services.ContractInterfaces.Common
{
    [DataContract]
    [Serializable]
    public class DisplayMediaWeek
    {
        [DataMember]
        public int Id;
        [DataMember]
        public int Week;
        [DataMember]
        public int MediaMonthId;
        [DataMember]
        public int Year;
        [DataMember]
        public int Month;
        [DataMember]
        public DateTime WeekStartDate;
        [DataMember]
        public DateTime WeekEndDate;
        [DataMember]
        public DateTime MonthStartDate;
        [DataMember]
        public DateTime MonthEndDate;

        public bool IsHiatus;

        public string MediaMonth
        {
            get
            {
                return (Month.ToString("00") + Year.ToString(CultureInfo.InvariantCulture).Substring(2, 2));
            }
        }

        public DisplayMediaWeek(object[] pItemArray)
        {
            Id = (int)pItemArray[0];
            Week = (int)pItemArray[1];
            MediaMonthId = (int)pItemArray[2];
            Year = (int)pItemArray[3];
            Month = (int)pItemArray[4];
            WeekStartDate = (DateTime)pItemArray[5];
            WeekEndDate = (DateTime)pItemArray[6];
            MonthStartDate = (DateTime)pItemArray[7];
            MonthEndDate = (DateTime)pItemArray[8];

            IsHiatus = false;
        }
        public DisplayMediaWeek(int pId, int pWeek, int pMediaMonthId, int pYear, int pMonth, DateTime pWeekStartDate, DateTime pWeekEndDate, DateTime pMonthStartDate, DateTime pMonthEndDate)
        {
            Id = pId;
            Week = pWeek;
            MediaMonthId = pMediaMonthId;
            Year = pYear;
            Month = pMonth;
            WeekStartDate = pWeekStartDate;
            WeekEndDate = pWeekEndDate;
            MonthStartDate = pMonthStartDate;
            MonthEndDate = pMonthEndDate;

            IsHiatus = false;
        }

        public DisplayMediaWeek()
        {
        }
    }
}
