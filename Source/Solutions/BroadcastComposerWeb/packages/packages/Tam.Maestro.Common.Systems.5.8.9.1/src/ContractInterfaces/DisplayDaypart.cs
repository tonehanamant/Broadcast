using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tam.Maestro.Common;

namespace Tam.Maestro.Services.ContractInterfaces.Common
{
    public enum SpotSlot
    {
        Morning = 1,
        Afternoon = 2,
        Prime = 3,
        Overnight = 4,
        None = 5
    }

    [DataContract]
    [Serializable]
    [ProtoBuf.ProtoContract]
    public class DisplayDaypart : ICloneable, INotifyPropertyChanged, IComparable
    {
        [DataMember]
        [ProtoBuf.ProtoMember(1)]
        private int _Id;
        public int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(2)]
        private string _Code;
        public string Code
        {
            get { return _Code; }
            set { _Code = value; }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(3)]
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(4)]
        private int _StartTime;
        public int StartTime
        {
            get { return _StartTime; }
            set { _StartTime = value; OnPropertyChanged("StartTime"); }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(5)]
        private int _EndTime;
        public int EndTime
        {
            get { return _EndTime; }
            set { _EndTime = value; OnPropertyChanged("EndTime"); }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(6)]
        private bool _Monday;
        public bool Monday
        {
            get { return _Monday; }
            set { _Monday = value; OnPropertyChanged("Preview"); }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(7)]
        private bool _Tuesday;
        public bool Tuesday
        {
            get { return _Tuesday; }
            set { _Tuesday = value; OnPropertyChanged("Preview"); }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(8)]
        private bool _Wednesday;
        public bool Wednesday
        {
            get { return _Wednesday; }
            set { _Wednesday = value; OnPropertyChanged("Preview"); }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(9)]
        private bool _Thursday;
        public bool Thursday
        {
            get { return _Thursday; }
            set { _Thursday = value; OnPropertyChanged("Preview"); }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(10)]
        private bool _Friday;
        public bool Friday
        {
            get { return _Friday; }
            set { _Friday = value; OnPropertyChanged("Preview"); }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(11)]
        private bool _Saturday;
        public bool Saturday
        {
            get { return _Saturday; }
            set { _Saturday = value; OnPropertyChanged("Preview"); }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(12)]
        private bool _Sunday;
        public bool Sunday
        {
            get { return _Sunday; }
            set { _Sunday = value; OnPropertyChanged("Preview"); }
        }
        [DataMember]
        [ProtoBuf.ProtoMember(13)]
        private bool _Is24Hour = false;
        [DataMember]
        [ProtoBuf.ProtoMember(14)]
        private int _LastStartTime;
        [DataMember]
        [ProtoBuf.ProtoMember(15)]
        private int _LastEndTime;

        #region WPF Helper properties
        //Below needed as helpers for WPF

        // Create the OnPropertyChanged method to raise the event
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public string StartHourFullPercision
        {
            get
            {
                var lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));
                return lStartTime.ToString("HH");
            }
        }

        public string StartHour
        {
            get
            {
                DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));
                string lTemp = lStartTime.ToString("hh");
                if (lTemp.Length > 1 && lTemp.Substring(0, 1) == "0")
                    return lTemp.Substring(1, 1);
                else
                    return lTemp;
            }
            set
            {
                int ltemphour = System.Convert.ToInt32(value);
                if (StartAMPM == "PM" && ltemphour != 12)
                {
                    ltemphour += 12;
                }
                else if (StartAMPM == "AM" && ltemphour == 12)
                {
                    //Special Case for 12AM
                    ltemphour = 0;
                }

                DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));

                int lCurrentMinutes = System.Convert.ToInt32(lStartTime.ToString("mm"));
                lCurrentMinutes = lCurrentMinutes * 60;
                //Now we need to redo the starttime
                StartTime = (ltemphour * 3600) + lCurrentMinutes;

                if (StartTime == 0 && EndTime == 86399)
                    Is24Hours = true;
                System.Diagnostics.Debug.WriteLine("Change in Start Hour");
                OnPropertyChanged("StartHour");
                OnPropertyChanged("Preview");
                //Need to account for minutes too!!
            }
        }

        public string EndHourFullPercision
        {
            get
            {
                DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime + 1, 0));
                return lEndTime.ToString("HH");
            }
        }

        public string EndHour
        {
            get
            {
                DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime + 1, 0));
                string lTemp = lEndTime.ToString("hh");
                if (lTemp.Length > 1 && lTemp.Substring(0, 1) == "0")
                    return lTemp.Substring(1, 1);
                else
                    return lTemp;
            }
            set
            {
                int ltemphour = System.Convert.ToInt32(value);
                if (EndAMPM == "PM" && ltemphour != 12)
                {
                    ltemphour += 12;
                }
                else if (EndAMPM == "AM" && ltemphour == 12)
                {
                    //Special Case for 12AM
                    ltemphour += 12;
                }

                DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime + 1, 0));

                int lCurrentMinutes = System.Convert.ToInt32(lEndTime.ToString("mm"));
                lCurrentMinutes = lCurrentMinutes * 60;

                //Now we need to redo the starttime
                if (ltemphour == 24 && lCurrentMinutes == 0)
                    EndTime = 86399;
                else if (ltemphour == 24 && lCurrentMinutes > 0)
                {
                    ltemphour = 0;
                    EndTime = ((ltemphour * 3600) + lCurrentMinutes) - 1;
                }
                else
                    EndTime = ((ltemphour * 3600) + lCurrentMinutes) - 1;

                if (StartTime == 0 && EndTime == 86399)
                    Is24Hours = true;
                System.Diagnostics.Debug.WriteLine("Change in End Hour");

                OnPropertyChanged("EndHour");
                OnPropertyChanged("Preview");
            }
        }

        public bool IsValid
        {
            get
            {
                return (this.ActiveDays > 0 && this.StartTime != -1 && this.EndTime != -1);
            }
        }

        public string StartMinute
        {
            get
            {
                DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));
                return lStartTime.ToString("mm");
            }
            set
            {
                int ltempminute = System.Convert.ToInt32(value);
                ltempminute = ltempminute * 60;

                DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));
                int lcurrenthour = lStartTime.Hour;
                lcurrenthour = (lcurrenthour * 3600) + ltempminute;
                StartTime = lcurrenthour;

                if (StartTime == 0 && EndTime == 86399)
                    Is24Hours = true;

                System.Diagnostics.Debug.WriteLine("Change in Start Minute");

                OnPropertyChanged("StartMinute");
                OnPropertyChanged("Preview");
            }
        }

        public string EndMinute
        {
            get
            {
                DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                //We need to add one second for display
                lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime + 1, 0));
                return lEndTime.ToString("mm");
            }
            set
            {
                int ltempminute = System.Convert.ToInt32(value);
                ltempminute = (ltempminute * 60) - 1; //Subtract one on the save.  This is the format expected.

                DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime + 1, 0));
                int lcurrenthour = lEndTime.Hour;
                lcurrenthour = (lcurrenthour * 3600) + ltempminute;
                EndTime = lcurrenthour;

                if (StartTime == 0 && EndTime == 86399)
                    Is24Hours = true;
                System.Diagnostics.Debug.WriteLine("Change in End Minute");

                OnPropertyChanged("EndMinute");
                OnPropertyChanged("Preview");
            }
        }


        public string StartAMPM
        {
            get
            {
                DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));
                return lStartTime.ToString("tt");
            }
            set
            {
                string lnewValue = value;
                int lNewHours = 12 * 3600; //additional hours to add or subtract in seconds.

                if (lnewValue == "PM")
                {
                    if (StartTime < lNewHours)
                    {
                        StartTime += lNewHours;
                    }
                    else if (StartTime == 86399) //Special Case
                    {
                        StartTime -= lNewHours;
                    }
                }
                else
                {
                    if (StartTime >= lNewHours)
                    {
                        StartTime -= lNewHours;
                    }
                    else if (StartTime == 43199)
                    {
                        StartTime = 86399; //Special Case
                    }
                }

                if (StartTime == 0 && EndTime == 86399)
                    Is24Hours = true;

                System.Diagnostics.Debug.WriteLine("Change in Start AMPM");

                OnPropertyChanged("StartAMPM");
                OnPropertyChanged("Preview");

            }
        }

        public string EndAMPM
        {
            get
            {
                DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
                lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime + 1, 0));
                return lEndTime.ToString("tt");
            }
            set
            {
                string lnewValue = value;
                int lNewHours = 12 * 3600; //additional hours to add or subtract in seconds.

                if (lnewValue == "PM")
                {
                    if (EndTime < lNewHours)
                    {
                        EndTime += lNewHours;
                    }
                    else if (EndTime == 86399) //Special Case
                    {
                        EndTime -= lNewHours;
                    }
                }
                else
                {
                    if (EndTime >= lNewHours)
                    {
                        EndTime -= lNewHours;
                    }
                    else if (EndTime == 43199)
                    {
                        EndTime = 86399; //Special Case
                    }
                }

                if (StartTime == 0 && EndTime == 86399)
                    Is24Hours = true;

                System.Diagnostics.Debug.WriteLine("Change in End AMPM");


                OnPropertyChanged("EndAMPM");
                OnPropertyChanged("Preview");

            }
        }

        public string Preview
        {
            get
            {
                return this.ToLongString();
            }
        }

        public bool Is24Hours
        {
            get
            {
                return _Is24Hour;
            }
            set
            {
                if (_Is24Hour == value)
                    return;

                _Is24Hour = value;

                if (_Is24Hour)
                {
                    _LastEndTime = EndTime;
                    _LastStartTime = StartTime;

                    StartTime = 0;
                    EndTime = 86399;
                }
                else
                {
                    StartTime = _LastStartTime;
                    EndTime = _LastEndTime;
                }

                System.Diagnostics.Debug.WriteLine("Change in 24Hours");

                OnPropertyChanged("Is24Hours");
                OnPropertyChanged("IsTimeChangeEnabled");
                OnPropertyChanged("Preview");
            }

        }

        public bool IsTimeChangeEnabled
        {
            get { return !Is24Hours; }
        }
        //END
        #endregion

        public bool Weekdays
        {
            get
            {
                return Monday || Tuesday || Wednesday || Thursday || Friday;
            }
        }

        public bool Weekends
        {
            get
            {
                return Saturday || Sunday;
            }
        }

        public int ActiveDays
        {
            get
            {
                int lReturn = 0;

                if (this.Monday)
                    lReturn++;
                if (this.Tuesday)
                    lReturn++;
                if (this.Wednesday)
                    lReturn++;
                if (this.Thursday)
                    lReturn++;
                if (this.Friday)
                    lReturn++;
                if (this.Saturday)
                    lReturn++;
                if (this.Sunday)
                    lReturn++;

                return (lReturn);
            }
        }
        public int Hours
        {
            get
            {
                if (this.EndTime < this.StartTime)
                    return ((int)Math.Round(((86400.0 - (double)this.StartTime) + (double)this.RoundedEndTime) / 3600.0));
                else
                    return ((int)Math.Round((double)Math.Abs((double)this.EndTime - (double)this.StartTime) / 3600.0, 0));
            }
        }
        public int RoundedEndTime
        {
            get
            {
                if (this.EndTime % 60 != 0)
                    return (this.EndTime + 1);

                return (this.EndTime);
            }
        }
        public double TotalHours
        {
            get { return ((double)this.ActiveDays * (double)this.Hours); }
        }

        public bool OverlapsPrime
        {
            get
            {
                return (64800 <= this.EndTime && 86399 >= this.StartTime);
            }
        }

        public bool IsOvernight
        {
            get { return ((this.StartTime == 86400 || this.StartTime >= 0 && this.StartTime <= 21599) && this.EndTime >= 0 && this.EndTime <= 21599); }
        }

        public SpotSlot Quadrant
        {
            get
            {
                if (this.StartTime >= 0 && this.EndTime <= 21599)
                    return SpotSlot.Overnight;
                else if (this.StartTime >= 21600 && this.EndTime <= 43199)
                    return SpotSlot.Morning;
                else if (this.StartTime >= 43200 && this.EndTime <= 64799)
                    return SpotSlot.Afternoon;
                else if (this.StartTime >= 64800 && this.EndTime <= 86399)
                    return SpotSlot.Prime;

                return SpotSlot.None;
            }
        }

        public string GetMilitaryEndTime()
        {
            string lBuilder;
            DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime + 1, 0));
            lBuilder = lEndTime.ToString("HH:mm:ss");

            if (lBuilder == "00:00:00")
                lBuilder = "24:00:00";

            return lBuilder;
        }
        public string GetMilitaryStartTime()
        {
            string lBuilder;
            DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));
            lBuilder = lStartTime.ToString("HH:mm:ss");

            //if (lBuilder == "00:00:00")
            //    lBuilder = "24:00:00";

            return lBuilder;
        }

        public string GetMilitaryEndTimeWithoutSecond()
        {
            string lBuilder;
            DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime + 1, 0));
            lBuilder = lEndTime.ToString("HHmm");

            if (lBuilder == "0000")
                lBuilder = "2400";

            return lBuilder;
        }
        public string GetMilitaryStartTimeWithoutSeconds()
        {
            string lBuilder;
            DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));
            lBuilder = lStartTime.ToString("HHmm");

            //if (lBuilder == "00:00:00")
            //    lBuilder = "24:00:00";

            return lBuilder;
        }

        public int StartMediaDay
        {
            get
            {
                if (this.Monday)
                    return 0;
                else if (this.Tuesday)
                    return 1;
                else if (this.Wednesday)
                    return 2;
                else if (this.Thursday)
                    return 3;
                else if (this.Friday)
                    return 4;
                else if (this.Saturday)
                    return 5;
                else
                    return 6;
            }
        }

        public int EndMediaDay
        {
            get
            {
                if (this.Sunday)
                    return 6;
                else if (this.Saturday)
                    return 5;
                else if (this.Friday)
                    return 4;
                else if (this.Thursday)
                    return 3;
                else if (this.Wednesday)
                    return 2;
                else if (this.Tuesday)
                    return 1;
                else
                    return 0;
            }
        }

        public string StartDay
        {
            get
            {
                if (this.Monday)
                    return "M";
                else if (this.Tuesday)
                    return "Tu";
                else if (this.Wednesday)
                    return "W";
                else if (this.Thursday)
                    return "Th";
                else if (this.Friday)
                    return "F";
                else if (this.Saturday)
                    return "Sa";
                else
                    return "Su";
            }
        }

        public string EndDay
        {
            get
            {
                if (this.Sunday) return "Su";
                if (this.Saturday) return "Sa";
                if (this.Friday) return "F";
                if (this.Thursday) return "Th";
                if (this.Wednesday) return "W";
                if (this.Tuesday) return "Tu";
                return "M";
            }
        }

        public string AllDaysIn4AsFormat
        {
            get
            {
                StringBuilder lBuilder = new StringBuilder();
                lBuilder.Append(this.Monday ? "M" : " ");
                lBuilder.Append(this.Tuesday ? "T" : " ");
                lBuilder.Append(this.Wednesday ? "W" : " ");
                lBuilder.Append(this.Thursday ? "T" : " ");
                lBuilder.Append(this.Friday ? "F" : " ");
                lBuilder.Append(this.Saturday ? "S" : " ");
                lBuilder.Append(this.Sunday ? "S" : " ");
                return lBuilder.ToString();
            }
        }

        public List<DayOfWeek> Days
        {
            get
            {
                List<DayOfWeek> lReturn = new List<DayOfWeek>();
                if (this.Monday)
                    lReturn.Add(DayOfWeek.Monday);
                if (this.Tuesday)
                    lReturn.Add(DayOfWeek.Tuesday);
                if (this.Wednesday)
                    lReturn.Add(DayOfWeek.Wednesday);
                if (this.Thursday)
                    lReturn.Add(DayOfWeek.Thursday);
                if (this.Friday)
                    lReturn.Add(DayOfWeek.Friday);
                if (this.Saturday)
                    lReturn.Add(DayOfWeek.Saturday);
                if (this.Sunday)
                    lReturn.Add(DayOfWeek.Sunday);
                return (lReturn);
            }
        }

        //@todo is there an assumption that this is only done for a single day?
        //yes
        public List<DisplayDaypart> ApplyDaypartExclusion(DisplayDaypart exclusionDaypart)
        {
            if (this.ActiveDays > 1)
            {
                throw new Exception("This operation is only supported for Dayparts with one day.");
            }
            if (!DisplayDaypart.Intersects(this, exclusionDaypart))
            {
                return new List<DisplayDaypart> { this };
            }

            var daypartsFromExclusion = new List<DisplayDaypart>();

            var firstHalfDaypart = DisplayDaypart.ParseMsaDaypart(this.ToString());
            firstHalfDaypart.StartTime = this.StartTime;
            firstHalfDaypart.EndTime = Math.Min(this.EndTime, exclusionDaypart.StartTime - 1);
            if (firstHalfDaypart.StartTime < firstHalfDaypart.EndTime)
            {
                daypartsFromExclusion.Add(firstHalfDaypart);
            }

            var secondHalfDaypart = DisplayDaypart.ParseMsaDaypart(this.ToString());
            secondHalfDaypart.StartTime = Math.Max(this.StartTime, exclusionDaypart.EndTime + 1);
            secondHalfDaypart.EndTime = this.EndTime;
            if (secondHalfDaypart.StartTime < secondHalfDaypart.EndTime)
            {
                daypartsFromExclusion.Add(secondHalfDaypart);
            }

            return daypartsFromExclusion;
        }

        public List<DisplayDaypart> GetDaypartExclusions(List<DisplayDaypart> exclusionDayparts)
        {
            var daypartExclusions = new List<DisplayDaypart>();

            var dayparts = new List<DisplayDaypart>();
            dayparts.AddRange(exclusionDayparts);
            dayparts = dayparts.OrderBy(x => x.StartTime).ToList();

            DisplayDaypart excludedDaypart = null;
            var startTime = this.StartTime;
            var endTime = this.StartTime;

            while (startTime < this.EndTime)
            {
                var match = dayparts.FirstOrDefault(x => x.StartTime <= startTime);
                if (match != null)
                {
                    dayparts.Remove(match);
                    startTime = match.EndTime + 1;
                }
                else
                {
                    endTime = Math.Min(this.EndTime, dayparts.Count > 0 ? dayparts[0].StartTime - 1 : this.EndTime);
                    excludedDaypart = (DisplayDaypart)this.Clone();
                    excludedDaypart.StartTime = startTime;
                    excludedDaypart.EndTime = endTime;
                    daypartExclusions.Add(excludedDaypart);

                    startTime = endTime + 1;
                }
            }

            return daypartExclusions;
        }

        public DisplayDaypart()
        {
            this.Id = -1;
            this.Code = "";
            this.Name = "";
            this.Monday = false;
            this.Tuesday = false;
            this.Wednesday = false;
            this.Thursday = false;
            this.Friday = false;
            this.Saturday = false;
            this.Sunday = false;
        }
        public DisplayDaypart(object[] pItemArray)
        {
            this.Id = (int)pItemArray[0];
            this.Code = (string)pItemArray[1];
            this.Name = (string)pItemArray[2];
            this.StartTime = (int)pItemArray[3];
            this.EndTime = (int)pItemArray[4];
            this.Monday = (int)pItemArray[5] == 1;
            this.Tuesday = (int)pItemArray[6] == 1;
            this.Wednesday = (int)pItemArray[7] == 1;
            this.Thursday = (int)pItemArray[8] == 1;
            this.Friday = (int)pItemArray[9] == 1;
            this.Saturday = (int)pItemArray[10] == 1;
            this.Sunday = (int)pItemArray[11] == 1;
        }
        public DisplayDaypart(object[] pItemArray, int pOffset)
        {
            this.Id = (int)pItemArray[0 + pOffset];
            this.Code = (string)pItemArray[1 + pOffset];
            this.Name = (string)pItemArray[2 + pOffset];
            this.StartTime = (int)pItemArray[3 + pOffset];
            this.EndTime = (int)pItemArray[4 + pOffset];
            this.Monday = (int)pItemArray[5 + pOffset] == 1;
            this.Tuesday = (int)pItemArray[6 + pOffset] == 1;
            this.Wednesday = (int)pItemArray[7 + pOffset] == 1;
            this.Thursday = (int)pItemArray[8 + pOffset] == 1;
            this.Friday = (int)pItemArray[9 + pOffset] == 1;
            this.Saturday = (int)pItemArray[10 + pOffset] == 1;
            this.Sunday = (int)pItemArray[11 + pOffset] == 1;
        }
        public DisplayDaypart(int pId, int pStartTime, int pEndTime, bool pMonday, bool pTuesday, bool pWednesday, bool pThursday, bool pFriday, bool pSaturday, bool pSunday)
        {
            this.Id = pId;
            this.Code = "CUS";
            this.Name = "Custom";
            this.StartTime = pStartTime;
            this.EndTime = pEndTime;
            this.Monday = pMonday;
            this.Tuesday = pTuesday;
            this.Wednesday = pWednesday;
            this.Thursday = pThursday;
            this.Friday = pFriday;
            this.Saturday = pSaturday;
            this.Sunday = pSunday;
        }
        public DisplayDaypart(int pId, string pCode, string pName, int pStartTime, int pEndTime, bool pMonday, bool pTuesday, bool pWednesday, bool pThursday, bool pFriday, bool pSaturday, bool pSunday)
        {
            this.Id = pId;
            this.Code = pCode;
            this.Name = pName;
            this.StartTime = pStartTime;
            this.EndTime = pEndTime;
            this.Monday = pMonday;
            this.Tuesday = pTuesday;
            this.Wednesday = pWednesday;
            this.Thursday = pThursday;
            this.Friday = pFriday;
            this.Saturday = pSaturday;
            this.Sunday = pSunday;
        }



        public static int GetNumIntersectingDays(DisplayDaypart pDaypart1, DisplayDaypart pDaypart2)
        {
            if (pDaypart1 == null || pDaypart2 == null)
                return (0);

            int lReturn = 0;
            lReturn += pDaypart1.Monday && pDaypart2.Monday ? 1 : 0;
            lReturn += pDaypart1.Tuesday && pDaypart2.Tuesday ? 1 : 0;
            lReturn += pDaypart1.Wednesday && pDaypart2.Wednesday ? 1 : 0;
            lReturn += pDaypart1.Thursday && pDaypart2.Thursday ? 1 : 0;
            lReturn += pDaypart1.Friday && pDaypart2.Friday ? 1 : 0;
            lReturn += pDaypart1.Saturday && pDaypart2.Saturday ? 1 : 0;
            lReturn += pDaypart1.Sunday && pDaypart2.Sunday ? 1 : 0;

            return (lReturn);
        }
        public static int GetIntersectingHours(DisplayDaypart pDaypart1, DisplayDaypart pDaypart2)
        {
            int lReturn = 0;

            var pDaypart1StartTime = pDaypart1.StartTime;
            var pDaypart1EndTime = pDaypart1.EndTime;
            var pDaypart2StartTime = pDaypart2.StartTime;
            var pDaypart2EndTime = pDaypart2.EndTime;

            int lH1 = (pDaypart1StartTime < pDaypart1EndTime && ((0 >= pDaypart1StartTime && 0 <= pDaypart1EndTime) || (3599 >= pDaypart1StartTime && 3599 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (3599 >= pDaypart1StartTime || 3599 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((0 >= pDaypart2StartTime && 0 <= pDaypart2EndTime) || (3599 >= pDaypart2StartTime && 3599 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (3599 >= pDaypart2StartTime || 3599 <= pDaypart2EndTime) ? 1 : 0);

            int lH2 = (pDaypart1StartTime < pDaypart1EndTime && ((3600 >= pDaypart1StartTime && 3600 <= pDaypart1EndTime) || (7199 >= pDaypart1StartTime && 7199 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (7199 >= pDaypart1StartTime || 7199 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((3600 >= pDaypart2StartTime && 3600 <= pDaypart2EndTime) || (7199 >= pDaypart2StartTime && 7199 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (7199 >= pDaypart2StartTime || 7199 <= pDaypart2EndTime) ? 1 : 0);

            int lH3 = (pDaypart1StartTime < pDaypart1EndTime && ((7200 >= pDaypart1StartTime && 7200 <= pDaypart1EndTime) || (10799 >= pDaypart1StartTime && 10799 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (10799 >= pDaypart1StartTime || 10799 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((7200 >= pDaypart2StartTime && 7200 <= pDaypart2EndTime) || (10799 >= pDaypart2StartTime && 10799 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (10799 >= pDaypart2StartTime || 10799 <= pDaypart2EndTime) ? 1 : 0);

            int lH4 = (pDaypart1StartTime < pDaypart1EndTime && ((10800 >= pDaypart1StartTime && 10800 <= pDaypart1EndTime) || (14399 >= pDaypart1StartTime && 14399 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (14399 >= pDaypart1StartTime || 14399 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((10800 >= pDaypart2StartTime && 10800 <= pDaypart2EndTime) || (14399 >= pDaypart2StartTime && 14399 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (14399 >= pDaypart2StartTime || 14399 <= pDaypart2EndTime) ? 1 : 0);

            int lH5 = (pDaypart1StartTime < pDaypart1EndTime && ((14400 >= pDaypart1StartTime && 14400 <= pDaypart1EndTime) || (17999 >= pDaypart1StartTime && 17999 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (17999 >= pDaypart1StartTime || 17999 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((14400 >= pDaypart2StartTime && 14400 <= pDaypart2EndTime) || (17999 >= pDaypart2StartTime && 17999 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (17999 >= pDaypart2StartTime || 17999 <= pDaypart2EndTime) ? 1 : 0);

            int lH6 = (pDaypart1StartTime < pDaypart1EndTime && ((18000 >= pDaypart1StartTime && 18000 <= pDaypart1EndTime) || (21599 >= pDaypart1StartTime && 21599 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (21599 >= pDaypart1StartTime || 21599 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((18000 >= pDaypart2StartTime && 18000 <= pDaypart2EndTime) || (21599 >= pDaypart2StartTime && 21599 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (21599 >= pDaypart2StartTime || 21599 <= pDaypart2EndTime) ? 1 : 0);

            int lH7 = (pDaypart1StartTime < pDaypart1EndTime && ((21600 >= pDaypart1StartTime && 21600 <= pDaypart1EndTime) || (25199 >= pDaypart1StartTime && 25199 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (25199 >= pDaypart1StartTime || 25199 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((21600 >= pDaypart2StartTime && 21600 <= pDaypart2EndTime) || (25199 >= pDaypart2StartTime && 25199 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (25199 >= pDaypart2StartTime || 25199 <= pDaypart2EndTime) ? 1 : 0);

            int lH8 = (pDaypart1StartTime < pDaypart1EndTime && ((25200 >= pDaypart1StartTime && 25200 <= pDaypart1EndTime) || (28799 >= pDaypart1StartTime && 28799 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (28799 >= pDaypart1StartTime || 28799 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((25200 >= pDaypart2StartTime && 25200 <= pDaypart2EndTime) || (28799 >= pDaypart2StartTime && 28799 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (28799 >= pDaypart2StartTime || 28799 <= pDaypart2EndTime) ? 1 : 0);

            int lH9 = (pDaypart1StartTime < pDaypart1EndTime && ((28800 >= pDaypart1StartTime && 28800 <= pDaypart1EndTime) || (32399 >= pDaypart1StartTime && 32399 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (32399 >= pDaypart1StartTime || 32399 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((28800 >= pDaypart2StartTime && 28800 <= pDaypart2EndTime) || (32399 >= pDaypart2StartTime && 32399 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (32399 >= pDaypart2StartTime || 32399 <= pDaypart2EndTime) ? 1 : 0);

            int lH10 = (pDaypart1StartTime < pDaypart1EndTime && ((32400 >= pDaypart1StartTime && 32400 <= pDaypart1EndTime) || (35999 >= pDaypart1StartTime && 35999 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (35999 >= pDaypart1StartTime || 35999 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((32400 >= pDaypart2StartTime && 32400 <= pDaypart2EndTime) || (35999 >= pDaypart2StartTime && 35999 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (35999 >= pDaypart2StartTime || 35999 <= pDaypart2EndTime) ? 1 : 0);

            int lH11 = (pDaypart1StartTime < pDaypart1EndTime && ((36000 >= pDaypart1StartTime && 36000 <= pDaypart1EndTime) || (39599 >= pDaypart1StartTime && 39599 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (39599 >= pDaypart1StartTime || 39599 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((36000 >= pDaypart2StartTime && 36000 <= pDaypart2EndTime) || (39599 >= pDaypart2StartTime && 39599 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (39599 >= pDaypart2StartTime || 39599 <= pDaypart2EndTime) ? 1 : 0);

            int lH12 = (pDaypart1StartTime < pDaypart1EndTime && ((39600 >= pDaypart1StartTime && 39600 <= pDaypart1EndTime) || (43199 >= pDaypart1StartTime && 43199 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (43199 >= pDaypart1StartTime || 43199 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((39600 >= pDaypart2StartTime && 39600 <= pDaypart2EndTime) || (43199 >= pDaypart2StartTime && 43199 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (43199 >= pDaypart2StartTime || 43199 <= pDaypart2EndTime) ? 1 : 0);

            int lH13 = (pDaypart1StartTime < pDaypart1EndTime && ((43200 >= pDaypart1StartTime && 43200 <= pDaypart1EndTime) || (46799 >= pDaypart1StartTime && 46799 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (46799 >= pDaypart1StartTime || 46799 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((43200 >= pDaypart2StartTime && 43200 <= pDaypart2EndTime || 46799 >= pDaypart2StartTime && 46799 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (46799 >= pDaypart2StartTime || 46799 <= pDaypart2EndTime) ? 1 : 0);

            int lH14 = (pDaypart1StartTime < pDaypart1EndTime && ((46800 >= pDaypart1StartTime && 46800 <= pDaypart1EndTime) || (50399 >= pDaypart1StartTime && 50399 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (50399 >= pDaypart1StartTime || 50399 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((46800 >= pDaypart2StartTime && 46800 <= pDaypart2EndTime) || (50399 >= pDaypart2StartTime && 50399 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (50399 >= pDaypart2StartTime || 50399 <= pDaypart2EndTime) ? 1 : 0);

            int lH15 = (pDaypart1StartTime < pDaypart1EndTime && ((50400 >= pDaypart1StartTime && 50400 <= pDaypart1EndTime) || (53999 >= pDaypart1StartTime && 53999 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (53999 >= pDaypart1StartTime || 53999 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((50400 >= pDaypart2StartTime && 50400 <= pDaypart2EndTime) || (53999 >= pDaypart2StartTime && 53999 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (53999 >= pDaypart2StartTime || 53999 <= pDaypart2EndTime) ? 1 : 0);

            int lH16 = (pDaypart1StartTime < pDaypart1EndTime && ((54000 >= pDaypart1StartTime && 54000 <= pDaypart1EndTime || 57599 >= pDaypart1StartTime && 57599 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (57599 >= pDaypart1StartTime || 57599 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((54000 >= pDaypart2StartTime && 54000 <= pDaypart2EndTime) || (57599 >= pDaypart2StartTime && 57599 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (57599 >= pDaypart2StartTime || 57599 <= pDaypart2EndTime) ? 1 : 0);

            int lH17 = (pDaypart1StartTime < pDaypart1EndTime && ((57600 >= pDaypart1StartTime && 57600 <= pDaypart1EndTime) || (61199 >= pDaypart1StartTime && 61199 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (61199 >= pDaypart1StartTime || 61199 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((57600 >= pDaypart2StartTime && 57600 <= pDaypart2EndTime) || (61199 >= pDaypart2StartTime && 61199 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (61199 >= pDaypart2StartTime || 61199 <= pDaypart2EndTime) ? 1 : 0);

            int lH18 = (pDaypart1StartTime < pDaypart1EndTime && ((61200 >= pDaypart1StartTime && 61200 <= pDaypart1EndTime) || (64799 >= pDaypart1StartTime && 64799 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (64799 >= pDaypart1StartTime || 64799 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((61200 >= pDaypart2StartTime && 61200 <= pDaypart2EndTime) || (64799 >= pDaypart2StartTime && 64799 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (64799 >= pDaypart2StartTime || 64799 <= pDaypart2EndTime) ? 1 : 0);

            int lH19 = (pDaypart1StartTime < pDaypart1EndTime && ((64800 >= pDaypart1StartTime && 64800 <= pDaypart1EndTime) || (68399 >= pDaypart1StartTime && 68399 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (68399 >= pDaypart1StartTime || 68399 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((64800 >= pDaypart2StartTime && 64800 <= pDaypart2EndTime) || (68399 >= pDaypart2StartTime && 68399 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (68399 >= pDaypart2StartTime || 68399 <= pDaypart2EndTime) ? 1 : 0);

            int lH20 = (pDaypart1StartTime < pDaypart1EndTime && ((68400 >= pDaypart1StartTime && 68400 <= pDaypart1EndTime) || (71999 >= pDaypart1StartTime && 71999 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (71999 >= pDaypart1StartTime || 71999 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((68400 >= pDaypart2StartTime && 68400 <= pDaypart2EndTime) || (71999 >= pDaypart2StartTime && 71999 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (71999 >= pDaypart2StartTime || 71999 <= pDaypart2EndTime) ? 1 : 0);

            int lH21 = (pDaypart1StartTime < pDaypart1EndTime && ((72000 >= pDaypart1StartTime && 72000 <= pDaypart1EndTime) || (75599 >= pDaypart1StartTime && 75599 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (75599 >= pDaypart1StartTime || 75599 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((72000 >= pDaypart2StartTime && 72000 <= pDaypart2EndTime) || (75599 >= pDaypart2StartTime && 75599 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (75599 >= pDaypart2StartTime || 75599 <= pDaypart2EndTime) ? 1 : 0);

            int lH22 = (pDaypart1StartTime < pDaypart1EndTime && ((75600 >= pDaypart1StartTime && 75600 <= pDaypart1EndTime) || (79199 >= pDaypart1StartTime && 79199 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (79199 >= pDaypart1StartTime || 79199 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((75600 >= pDaypart2StartTime && 75600 <= pDaypart2EndTime) || (79199 >= pDaypart2StartTime && 79199 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (79199 >= pDaypart2StartTime || 79199 <= pDaypart2EndTime) ? 1 : 0);

            int lH23 = (pDaypart1StartTime < pDaypart1EndTime && ((79200 >= pDaypart1StartTime && 79200 <= pDaypart1EndTime) || (82799 >= pDaypart1StartTime && 82799 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (82799 >= pDaypart1StartTime || 82799 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((79200 >= pDaypart2StartTime && 79200 <= pDaypart2EndTime) || (82799 >= pDaypart2StartTime && 82799 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (82799 >= pDaypart2StartTime || 82799 <= pDaypart2EndTime) ? 1 : 0);

            int lH24 = (pDaypart1StartTime < pDaypart1EndTime && ((82800 >= pDaypart1StartTime && 82800 <= pDaypart1EndTime) || (86399 >= pDaypart1StartTime && 86399 <= pDaypart1EndTime)) ? 1 :
                       pDaypart1EndTime < pDaypart1StartTime && (86399 >= pDaypart1StartTime || 86399 <= pDaypart1EndTime) ? 1 : 0)
                       &
                      (pDaypart2StartTime < pDaypart2EndTime && ((82800 >= pDaypart2StartTime && 82800 <= pDaypart2EndTime) || (86399 >= pDaypart2StartTime && 86399 <= pDaypart2EndTime)) ? 1 :
                       pDaypart2EndTime < pDaypart2StartTime && (86399 >= pDaypart2StartTime || 86399 <= pDaypart2EndTime) ? 1 : 0);

            lReturn = lH1 + lH2 + lH3 + lH4 + lH5 + lH6 + lH7 + lH8 + lH9 + lH10 + lH11 + lH12 + lH13 + lH14 + lH15 + lH16 + lH17 + lH18 + lH19 + lH20 + lH21 + lH22 + lH23 + lH24;

            return (lReturn);
        }
        public static bool[] GetIntersectingHourArray(DisplayDaypart pDaypart1, DisplayDaypart pDaypart2)
        {
            bool[] lReturn = new bool[24];

            int lH1 = (pDaypart1.StartTime < pDaypart1.EndTime && ((0 >= pDaypart1.StartTime && 0 <= pDaypart1.EndTime) || (3599 >= pDaypart1.StartTime && 3599 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (3599 >= pDaypart1.StartTime || 3599 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((0 >= pDaypart2.StartTime && 0 <= pDaypart2.EndTime) || (3599 >= pDaypart2.StartTime && 3599 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (3599 >= pDaypart2.StartTime || 3599 <= pDaypart2.EndTime) ? 1 : 0);

            int lH2 = (pDaypart1.StartTime < pDaypart1.EndTime && ((3600 >= pDaypart1.StartTime && 3600 <= pDaypart1.EndTime) || (7199 >= pDaypart1.StartTime && 7199 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (7199 >= pDaypart1.StartTime || 7199 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((3600 >= pDaypart2.StartTime && 3600 <= pDaypart2.EndTime) || (7199 >= pDaypart2.StartTime && 7199 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (7199 >= pDaypart2.StartTime || 7199 <= pDaypart2.EndTime) ? 1 : 0);

            int lH3 = (pDaypart1.StartTime < pDaypart1.EndTime && ((7200 >= pDaypart1.StartTime && 7200 <= pDaypart1.EndTime) || (10799 >= pDaypart1.StartTime && 10799 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (10799 >= pDaypart1.StartTime || 10799 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((7200 >= pDaypart2.StartTime && 7200 <= pDaypart2.EndTime) || (10799 >= pDaypart2.StartTime && 10799 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (10799 >= pDaypart2.StartTime || 10799 <= pDaypart2.EndTime) ? 1 : 0);

            int lH4 = (pDaypart1.StartTime < pDaypart1.EndTime && ((10800 >= pDaypart1.StartTime && 10800 <= pDaypart1.EndTime) || (14399 >= pDaypart1.StartTime && 14399 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (14399 >= pDaypart1.StartTime || 14399 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((10800 >= pDaypart2.StartTime && 10800 <= pDaypart2.EndTime) || (14399 >= pDaypart2.StartTime && 14399 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (14399 >= pDaypart2.StartTime || 14399 <= pDaypart2.EndTime) ? 1 : 0);

            int lH5 = (pDaypart1.StartTime < pDaypart1.EndTime && ((14400 >= pDaypart1.StartTime && 14400 <= pDaypart1.EndTime) || (17999 >= pDaypart1.StartTime && 17999 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (17999 >= pDaypart1.StartTime || 17999 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((14400 >= pDaypart2.StartTime && 14400 <= pDaypart2.EndTime) || (17999 >= pDaypart2.StartTime && 17999 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (17999 >= pDaypart2.StartTime || 17999 <= pDaypart2.EndTime) ? 1 : 0);

            int lH6 = (pDaypart1.StartTime < pDaypart1.EndTime && ((18000 >= pDaypart1.StartTime && 18000 <= pDaypart1.EndTime) || (21599 >= pDaypart1.StartTime && 21599 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (21599 >= pDaypart1.StartTime || 21599 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((18000 >= pDaypart2.StartTime && 18000 <= pDaypart2.EndTime) || (21599 >= pDaypart2.StartTime && 21599 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (21599 >= pDaypart2.StartTime || 21599 <= pDaypart2.EndTime) ? 1 : 0);

            int lH7 = (pDaypart1.StartTime < pDaypart1.EndTime && ((21600 >= pDaypart1.StartTime && 21600 <= pDaypart1.EndTime) || (25199 >= pDaypart1.StartTime && 25199 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (25199 >= pDaypart1.StartTime || 25199 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((21600 >= pDaypart2.StartTime && 21600 <= pDaypart2.EndTime) || (25199 >= pDaypart2.StartTime && 25199 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (25199 >= pDaypart2.StartTime || 25199 <= pDaypart2.EndTime) ? 1 : 0);

            int lH8 = (pDaypart1.StartTime < pDaypart1.EndTime && ((25200 >= pDaypart1.StartTime && 25200 <= pDaypart1.EndTime) || (28799 >= pDaypart1.StartTime && 28799 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (28799 >= pDaypart1.StartTime || 28799 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((25200 >= pDaypart2.StartTime && 25200 <= pDaypart2.EndTime) || (28799 >= pDaypart2.StartTime && 28799 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (28799 >= pDaypart2.StartTime || 28799 <= pDaypart2.EndTime) ? 1 : 0);

            int lH9 = (pDaypart1.StartTime < pDaypart1.EndTime && ((28800 >= pDaypart1.StartTime && 28800 <= pDaypart1.EndTime) || (32399 >= pDaypart1.StartTime && 32399 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (32399 >= pDaypart1.StartTime || 32399 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((28800 >= pDaypart2.StartTime && 28800 <= pDaypart2.EndTime) || (32399 >= pDaypart2.StartTime && 32399 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (32399 >= pDaypart2.StartTime || 32399 <= pDaypart2.EndTime) ? 1 : 0);

            int lH10 = (pDaypart1.StartTime < pDaypart1.EndTime && ((32400 >= pDaypart1.StartTime && 32400 <= pDaypart1.EndTime) || (35999 >= pDaypart1.StartTime && 35999 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (35999 >= pDaypart1.StartTime || 35999 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((32400 >= pDaypart2.StartTime && 32400 <= pDaypart2.EndTime) || (35999 >= pDaypart2.StartTime && 35999 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (35999 >= pDaypart2.StartTime || 35999 <= pDaypart2.EndTime) ? 1 : 0);

            int lH11 = (pDaypart1.StartTime < pDaypart1.EndTime && ((36000 >= pDaypart1.StartTime && 36000 <= pDaypart1.EndTime) || (39599 >= pDaypart1.StartTime && 39599 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (39599 >= pDaypart1.StartTime || 39599 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((36000 >= pDaypart2.StartTime && 36000 <= pDaypart2.EndTime) || (39599 >= pDaypart2.StartTime && 39599 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (39599 >= pDaypart2.StartTime || 39599 <= pDaypart2.EndTime) ? 1 : 0);

            int lH12 = (pDaypart1.StartTime < pDaypart1.EndTime && ((39600 >= pDaypart1.StartTime && 39600 <= pDaypart1.EndTime) || (43199 >= pDaypart1.StartTime && 43199 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (43199 >= pDaypart1.StartTime || 43199 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((39600 >= pDaypart2.StartTime && 39600 <= pDaypart2.EndTime) || (43199 >= pDaypart2.StartTime && 43199 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (43199 >= pDaypart2.StartTime || 43199 <= pDaypart2.EndTime) ? 1 : 0);

            int lH13 = (pDaypart1.StartTime < pDaypart1.EndTime && ((43200 >= pDaypart1.StartTime && 43200 <= pDaypart1.EndTime) || (46799 >= pDaypart1.StartTime && 46799 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (46799 >= pDaypart1.StartTime || 46799 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((43200 >= pDaypart2.StartTime && 43200 <= pDaypart2.EndTime || 46799 >= pDaypart2.StartTime && 46799 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (46799 >= pDaypart2.StartTime || 46799 <= pDaypart2.EndTime) ? 1 : 0);

            int lH14 = (pDaypart1.StartTime < pDaypart1.EndTime && ((46800 >= pDaypart1.StartTime && 46800 <= pDaypart1.EndTime) || (50399 >= pDaypart1.StartTime && 50399 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (50399 >= pDaypart1.StartTime || 50399 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((46800 >= pDaypart2.StartTime && 46800 <= pDaypart2.EndTime) || (50399 >= pDaypart2.StartTime && 50399 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (50399 >= pDaypart2.StartTime || 50399 <= pDaypart2.EndTime) ? 1 : 0);

            int lH15 = (pDaypart1.StartTime < pDaypart1.EndTime && ((50400 >= pDaypart1.StartTime && 50400 <= pDaypart1.EndTime) || (53999 >= pDaypart1.StartTime && 53999 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (53999 >= pDaypart1.StartTime || 53999 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((50400 >= pDaypart2.StartTime && 50400 <= pDaypart2.EndTime) || (53999 >= pDaypart2.StartTime && 53999 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (53999 >= pDaypart2.StartTime || 53999 <= pDaypart2.EndTime) ? 1 : 0);

            int lH16 = (pDaypart1.StartTime < pDaypart1.EndTime && ((54000 >= pDaypart1.StartTime && 54000 <= pDaypart1.EndTime || 57599 >= pDaypart1.StartTime && 57599 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (57599 >= pDaypart1.StartTime || 57599 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((54000 >= pDaypart2.StartTime && 54000 <= pDaypart2.EndTime) || (57599 >= pDaypart2.StartTime && 57599 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (57599 >= pDaypart2.StartTime || 57599 <= pDaypart2.EndTime) ? 1 : 0);

            int lH17 = (pDaypart1.StartTime < pDaypart1.EndTime && ((57600 >= pDaypart1.StartTime && 57600 <= pDaypart1.EndTime) || (61199 >= pDaypart1.StartTime && 61199 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (61199 >= pDaypart1.StartTime || 61199 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((57600 >= pDaypart2.StartTime && 57600 <= pDaypart2.EndTime) || (61199 >= pDaypart2.StartTime && 61199 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (61199 >= pDaypart2.StartTime || 61199 <= pDaypart2.EndTime) ? 1 : 0);

            int lH18 = (pDaypart1.StartTime < pDaypart1.EndTime && ((61200 >= pDaypart1.StartTime && 61200 <= pDaypart1.EndTime) || (64799 >= pDaypart1.StartTime && 64799 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (64799 >= pDaypart1.StartTime || 64799 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((61200 >= pDaypart2.StartTime && 61200 <= pDaypart2.EndTime) || (64799 >= pDaypart2.StartTime && 64799 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (64799 >= pDaypart2.StartTime || 64799 <= pDaypart2.EndTime) ? 1 : 0);

            int lH19 = (pDaypart1.StartTime < pDaypart1.EndTime && ((64800 >= pDaypart1.StartTime && 64800 <= pDaypart1.EndTime) || (68399 >= pDaypart1.StartTime && 68399 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (68399 >= pDaypart1.StartTime || 68399 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((64800 >= pDaypart2.StartTime && 64800 <= pDaypart2.EndTime) || (68399 >= pDaypart2.StartTime && 68399 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (68399 >= pDaypart2.StartTime || 68399 <= pDaypart2.EndTime) ? 1 : 0);

            int lH20 = (pDaypart1.StartTime < pDaypart1.EndTime && ((68400 >= pDaypart1.StartTime && 68400 <= pDaypart1.EndTime) || (71999 >= pDaypart1.StartTime && 71999 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (71999 >= pDaypart1.StartTime || 71999 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((68400 >= pDaypart2.StartTime && 68400 <= pDaypart2.EndTime) || (71999 >= pDaypart2.StartTime && 71999 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (71999 >= pDaypart2.StartTime || 71999 <= pDaypart2.EndTime) ? 1 : 0);

            int lH21 = (pDaypart1.StartTime < pDaypart1.EndTime && ((72000 >= pDaypart1.StartTime && 72000 <= pDaypart1.EndTime) || (75599 >= pDaypart1.StartTime && 75599 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (75599 >= pDaypart1.StartTime || 75599 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((72000 >= pDaypart2.StartTime && 72000 <= pDaypart2.EndTime) || (75599 >= pDaypart2.StartTime && 75599 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (75599 >= pDaypart2.StartTime || 75599 <= pDaypart2.EndTime) ? 1 : 0);

            int lH22 = (pDaypart1.StartTime < pDaypart1.EndTime && ((75600 >= pDaypart1.StartTime && 75600 <= pDaypart1.EndTime) || (79199 >= pDaypart1.StartTime && 79199 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (79199 >= pDaypart1.StartTime || 79199 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((75600 >= pDaypart2.StartTime && 75600 <= pDaypart2.EndTime) || (79199 >= pDaypart2.StartTime && 79199 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (79199 >= pDaypart2.StartTime || 79199 <= pDaypart2.EndTime) ? 1 : 0);

            int lH23 = (pDaypart1.StartTime < pDaypart1.EndTime && ((79200 >= pDaypart1.StartTime && 79200 <= pDaypart1.EndTime) || (82799 >= pDaypart1.StartTime && 82799 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (82799 >= pDaypart1.StartTime || 82799 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((79200 >= pDaypart2.StartTime && 79200 <= pDaypart2.EndTime) || (82799 >= pDaypart2.StartTime && 82799 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (82799 >= pDaypart2.StartTime || 82799 <= pDaypart2.EndTime) ? 1 : 0);

            int lH24 = (pDaypart1.StartTime < pDaypart1.EndTime && ((82800 >= pDaypart1.StartTime && 82800 <= pDaypart1.EndTime) || (86399 >= pDaypart1.StartTime && 86399 <= pDaypart1.EndTime)) ? 1 :
                       pDaypart1.EndTime < pDaypart1.StartTime && (86399 >= pDaypart1.StartTime || 86399 <= pDaypart1.EndTime) ? 1 : 0)
                       &
                      (pDaypart2.StartTime < pDaypart2.EndTime && ((82800 >= pDaypart2.StartTime && 82800 <= pDaypart2.EndTime) || (86399 >= pDaypart2.StartTime && 86399 <= pDaypart2.EndTime)) ? 1 :
                       pDaypart2.EndTime < pDaypart2.StartTime && (86399 >= pDaypart2.StartTime || 86399 <= pDaypart2.EndTime) ? 1 : 0);

            lReturn[0] = lH1 == 1;
            lReturn[1] = lH2 == 1;
            lReturn[2] = lH3 == 1;
            lReturn[3] = lH4 == 1;
            lReturn[4] = lH5 == 1;
            lReturn[5] = lH6 == 1;
            lReturn[6] = lH7 == 1;
            lReturn[7] = lH8 == 1;
            lReturn[8] = lH9 == 1;
            lReturn[9] = lH10 == 1;
            lReturn[10] = lH11 == 1;
            lReturn[11] = lH12 == 1;
            lReturn[12] = lH13 == 1;
            lReturn[13] = lH14 == 1;
            lReturn[14] = lH15 == 1;
            lReturn[15] = lH16 == 1;
            lReturn[16] = lH17 == 1;
            lReturn[17] = lH18 == 1;
            lReturn[18] = lH19 == 1;
            lReturn[19] = lH20 == 1;
            lReturn[21] = lH21 == 1;
            lReturn[22] = lH22 == 1;
            lReturn[23] = lH23 == 1;

            return (lReturn);
        }
        public static bool GetIntersectingTimes(DisplayDaypart pDaypart1, DisplayDaypart pDaypart2, out int pStartTime, out int pEndTime)
        {
            pStartTime = -1;
            pEndTime = -1;
            int[] lTime = new int[48];

            lTime[0] = (pDaypart1.StartTime < pDaypart1.EndTime && ((0 >= pDaypart1.StartTime && 0 <= pDaypart1.EndTime) || (1799 >= pDaypart1.StartTime && 1799 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (1799 >= pDaypart1.StartTime || 1799 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((0 >= pDaypart2.StartTime && 0 <= pDaypart2.EndTime) || (1799 >= pDaypart2.StartTime && 1799 <= pDaypart2.EndTime)) ? 1 :
                        pDaypart2.EndTime < pDaypart2.StartTime && (1799 >= pDaypart2.StartTime || 1799 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[1] = (pDaypart1.StartTime < pDaypart1.EndTime && ((1800 >= pDaypart1.StartTime && 1800 <= pDaypart1.EndTime) || (3599 >= pDaypart1.StartTime && 3599 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (3599 >= pDaypart1.StartTime || 3599 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((1800 >= pDaypart2.StartTime && 1800 <= pDaypart2.EndTime) || (3599 >= pDaypart2.StartTime && 3599 <= pDaypart2.EndTime)) ? 1 :
                         pDaypart2.EndTime < pDaypart2.StartTime && (3599 >= pDaypart2.StartTime || 3599 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[2] = (pDaypart1.StartTime < pDaypart1.EndTime && ((3600 >= pDaypart1.StartTime && 3600 <= pDaypart1.EndTime) || (5399 >= pDaypart1.StartTime && 5399 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (5399 >= pDaypart1.StartTime || 5399 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((3600 >= pDaypart2.StartTime && 3600 <= pDaypart2.EndTime) || (5399 >= pDaypart2.StartTime && 5399 <= pDaypart2.EndTime)) ? 1 :
                         pDaypart2.EndTime < pDaypart2.StartTime && (5399 >= pDaypart2.StartTime || 5399 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[3] = (pDaypart1.StartTime < pDaypart1.EndTime && ((5400 >= pDaypart1.StartTime && 5400 <= pDaypart1.EndTime) || (7199 >= pDaypart1.StartTime && 7199 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (7199 >= pDaypart1.StartTime || 7199 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((5400 >= pDaypart2.StartTime && 5400 <= pDaypart2.EndTime) || (7199 >= pDaypart2.StartTime && 7199 <= pDaypart2.EndTime)) ? 1 :
                         pDaypart2.EndTime < pDaypart2.StartTime && (7199 >= pDaypart2.StartTime || 7199 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[4] = (pDaypart1.StartTime < pDaypart1.EndTime && ((7200 >= pDaypart1.StartTime && 7200 <= pDaypart1.EndTime) || (8999 >= pDaypart1.StartTime && 8999 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (8999 >= pDaypart1.StartTime || 8999 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((7200 >= pDaypart2.StartTime && 7200 <= pDaypart2.EndTime) || (8999 >= pDaypart2.StartTime && 8999 <= pDaypart2.EndTime)) ? 1 :
                         pDaypart2.EndTime < pDaypart2.StartTime && (8999 >= pDaypart2.StartTime || 8999 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[5] = (pDaypart1.StartTime < pDaypart1.EndTime && ((9000 >= pDaypart1.StartTime && 9000 <= pDaypart1.EndTime) || (10799 >= pDaypart1.StartTime && 10799 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (10799 >= pDaypart1.StartTime || 10799 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((9000 >= pDaypart2.StartTime && 9000 <= pDaypart2.EndTime) || (10799 >= pDaypart2.StartTime && 10799 <= pDaypart2.EndTime)) ? 1 :
                         pDaypart2.EndTime < pDaypart2.StartTime && (10799 >= pDaypart2.StartTime || 10799 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[6] = (pDaypart1.StartTime < pDaypart1.EndTime && ((10800 >= pDaypart1.StartTime && 10800 <= pDaypart1.EndTime) || (12599 >= pDaypart1.StartTime && 12599 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (12599 >= pDaypart1.StartTime || 12599 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((10800 >= pDaypart2.StartTime && 10800 <= pDaypart2.EndTime) || (12599 >= pDaypart2.StartTime && 12599 <= pDaypart2.EndTime)) ? 1 :
                         pDaypart2.EndTime < pDaypart2.StartTime && (12599 >= pDaypart2.StartTime || 12599 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[7] = (pDaypart1.StartTime < pDaypart1.EndTime && ((12600 >= pDaypart1.StartTime && 12600 <= pDaypart1.EndTime) || (14399 >= pDaypart1.StartTime && 14399 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (14399 >= pDaypart1.StartTime || 14399 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((12600 >= pDaypart2.StartTime && 12600 <= pDaypart2.EndTime) || (14399 >= pDaypart2.StartTime && 14399 <= pDaypart2.EndTime)) ? 1 :
                        pDaypart2.EndTime < pDaypart2.StartTime && (14399 >= pDaypart2.StartTime || 14399 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[8] = (pDaypart1.StartTime < pDaypart1.EndTime && ((14400 >= pDaypart1.StartTime && 14400 <= pDaypart1.EndTime) || (16199 >= pDaypart1.StartTime && 16199 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (16199 >= pDaypart1.StartTime || 16199 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((14400 >= pDaypart2.StartTime && 14400 <= pDaypart2.EndTime) || (16199 >= pDaypart2.StartTime && 16199 <= pDaypart2.EndTime)) ? 1 :
                        pDaypart2.EndTime < pDaypart2.StartTime && (16199 >= pDaypart2.StartTime || 16199 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[9] = (pDaypart1.StartTime < pDaypart1.EndTime && ((16200 >= pDaypart1.StartTime && 16200 <= pDaypart1.EndTime) || (17999 >= pDaypart1.StartTime && 17999 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (17999 >= pDaypart1.StartTime || 17999 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((16200 >= pDaypart2.StartTime && 16200 <= pDaypart2.EndTime) || (17999 >= pDaypart2.StartTime && 17999 <= pDaypart2.EndTime)) ? 1 :
                        pDaypart2.EndTime < pDaypart2.StartTime && (17999 >= pDaypart2.StartTime || 17999 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[10] = (pDaypart1.StartTime < pDaypart1.EndTime && ((18000 >= pDaypart1.StartTime && 18000 <= pDaypart1.EndTime) || (19799 >= pDaypart1.StartTime && 19799 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (19799 >= pDaypart1.StartTime || 19799 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((18000 >= pDaypart2.StartTime && 18000 <= pDaypart2.EndTime) || (19799 >= pDaypart2.StartTime && 19799 <= pDaypart2.EndTime)) ? 1 :
                        pDaypart2.EndTime < pDaypart2.StartTime && (19799 >= pDaypart2.StartTime || 19799 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[11] = (pDaypart1.StartTime < pDaypart1.EndTime && ((19800 >= pDaypart1.StartTime && 19800 <= pDaypart1.EndTime) || (21599 >= pDaypart1.StartTime && 21599 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (21599 >= pDaypart1.StartTime || 21599 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((19800 >= pDaypart2.StartTime && 19800 <= pDaypart2.EndTime) || (21599 >= pDaypart2.StartTime && 21599 <= pDaypart2.EndTime)) ? 1 :
                        pDaypart2.EndTime < pDaypart2.StartTime && (21599 >= pDaypart2.StartTime || 21599 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[12] = (pDaypart1.StartTime < pDaypart1.EndTime && ((21600 >= pDaypart1.StartTime && 21600 <= pDaypart1.EndTime) || (23399 >= pDaypart1.StartTime && 23399 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (23399 >= pDaypart1.StartTime || 23399 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((21600 >= pDaypart2.StartTime && 21600 <= pDaypart2.EndTime) || (23399 >= pDaypart2.StartTime && 23399 <= pDaypart2.EndTime)) ? 1 :
                        pDaypart2.EndTime < pDaypart2.StartTime && (23399 >= pDaypart2.StartTime || 23399 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[13] = (pDaypart1.StartTime < pDaypart1.EndTime && ((23400 >= pDaypart1.StartTime && 23400 <= pDaypart1.EndTime) || (25199 >= pDaypart1.StartTime && 25199 <= pDaypart1.EndTime)) ? 1 :
                        pDaypart1.EndTime < pDaypart1.StartTime && (25199 >= pDaypart1.StartTime || 25199 <= pDaypart1.EndTime) ? 1 : 0)
                        &
                        (pDaypart2.StartTime < pDaypart2.EndTime && ((23400 >= pDaypart2.StartTime && 23400 <= pDaypart2.EndTime) || (25199 >= pDaypart2.StartTime && 25199 <= pDaypart2.EndTime)) ? 1 :
                        pDaypart2.EndTime < pDaypart2.StartTime && (25199 >= pDaypart2.StartTime || 25199 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[14] = (pDaypart1.StartTime < pDaypart1.EndTime && ((25200 >= pDaypart1.StartTime && 25200 <= pDaypart1.EndTime) || (26999 >= pDaypart1.StartTime && 26999 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (26999 >= pDaypart1.StartTime || 26999 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((25200 >= pDaypart2.StartTime && 25200 <= pDaypart2.EndTime) || (26999 >= pDaypart2.StartTime && 26999 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (26999 >= pDaypart2.StartTime || 26999 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[15] = (pDaypart1.StartTime < pDaypart1.EndTime && ((27000 >= pDaypart1.StartTime && 27000 <= pDaypart1.EndTime) || (28799 >= pDaypart1.StartTime && 28799 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (28799 >= pDaypart1.StartTime || 28799 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((27000 >= pDaypart2.StartTime && 27000 <= pDaypart2.EndTime) || (28799 >= pDaypart2.StartTime && 28799 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (28799 >= pDaypart2.StartTime || 28799 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[16] = (pDaypart1.StartTime < pDaypart1.EndTime && ((28800 >= pDaypart1.StartTime && 28800 <= pDaypart1.EndTime) || (30599 >= pDaypart1.StartTime && 30599 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (30599 >= pDaypart1.StartTime || 30599 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((28800 >= pDaypart2.StartTime && 28800 <= pDaypart2.EndTime) || (30599 >= pDaypart2.StartTime && 30599 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (30599 >= pDaypart2.StartTime || 30599 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[17] = (pDaypart1.StartTime < pDaypart1.EndTime && ((30600 >= pDaypart1.StartTime && 30600 <= pDaypart1.EndTime) || (32399 >= pDaypart1.StartTime && 32399 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (32399 >= pDaypart1.StartTime || 32399 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((30600 >= pDaypart2.StartTime && 30600 <= pDaypart2.EndTime) || (32399 >= pDaypart2.StartTime && 32399 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (32399 >= pDaypart2.StartTime || 32399 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[18] = (pDaypart1.StartTime < pDaypart1.EndTime && ((32400 >= pDaypart1.StartTime && 32400 <= pDaypart1.EndTime) || (34199 >= pDaypart1.StartTime && 34199 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (34199 >= pDaypart1.StartTime || 34199 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((32400 >= pDaypart2.StartTime && 32400 <= pDaypart2.EndTime) || (34199 >= pDaypart2.StartTime && 34199 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (34199 >= pDaypart2.StartTime || 34199 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[19] = (pDaypart1.StartTime < pDaypart1.EndTime && ((34200 >= pDaypart1.StartTime && 34200 <= pDaypart1.EndTime) || (35999 >= pDaypart1.StartTime && 35999 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (35999 >= pDaypart1.StartTime || 35999 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((34200 >= pDaypart2.StartTime && 34200 <= pDaypart2.EndTime) || (35999 >= pDaypart2.StartTime && 35999 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (35999 >= pDaypart2.StartTime || 35999 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[20] = (pDaypart1.StartTime < pDaypart1.EndTime && ((36000 >= pDaypart1.StartTime && 36000 <= pDaypart1.EndTime) || (37799 >= pDaypart1.StartTime && 37799 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (37799 >= pDaypart1.StartTime || 37799 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((36000 >= pDaypart2.StartTime && 36000 <= pDaypart2.EndTime) || (37799 >= pDaypart2.StartTime && 37799 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (37799 >= pDaypart2.StartTime || 37799 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[21] = (pDaypart1.StartTime < pDaypart1.EndTime && ((37800 >= pDaypart1.StartTime && 37800 <= pDaypart1.EndTime) || (39599 >= pDaypart1.StartTime && 39599 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (39599 >= pDaypart1.StartTime || 39599 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((37800 >= pDaypart2.StartTime && 37800 <= pDaypart2.EndTime) || (39599 >= pDaypart2.StartTime && 39599 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (39599 >= pDaypart2.StartTime || 39599 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[22] = (pDaypart1.StartTime < pDaypart1.EndTime && ((39600 >= pDaypart1.StartTime && 39600 <= pDaypart1.EndTime) || (41399 >= pDaypart1.StartTime && 41399 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (41399 >= pDaypart1.StartTime || 41399 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((39600 >= pDaypart2.StartTime && 39600 <= pDaypart2.EndTime) || (41399 >= pDaypart2.StartTime && 41399 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (41399 >= pDaypart2.StartTime || 41399 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[23] = (pDaypart1.StartTime < pDaypart1.EndTime && ((41400 >= pDaypart1.StartTime && 41400 <= pDaypart1.EndTime) || (43199 >= pDaypart1.StartTime && 43199 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (43199 >= pDaypart1.StartTime || 43199 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((41400 >= pDaypart2.StartTime && 41400 <= pDaypart2.EndTime) || (43199 >= pDaypart2.StartTime && 43199 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (43199 >= pDaypart2.StartTime || 43199 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[24] = (pDaypart1.StartTime < pDaypart1.EndTime && ((43200 >= pDaypart1.StartTime && 43200 <= pDaypart1.EndTime) || (44999 >= pDaypart1.StartTime && 44999 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (44999 >= pDaypart1.StartTime || 44999 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((43200 >= pDaypart2.StartTime && 43200 <= pDaypart2.EndTime) || (44999 >= pDaypart2.StartTime && 44999 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (44999 >= pDaypart2.StartTime || 44999 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[25] = (pDaypart1.StartTime < pDaypart1.EndTime && ((45000 >= pDaypart1.StartTime && 45000 <= pDaypart1.EndTime) || (46799 >= pDaypart1.StartTime && 46799 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (46799 >= pDaypart1.StartTime || 46799 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((45000 >= pDaypart2.StartTime && 45000 <= pDaypart2.EndTime) || (46799 >= pDaypart2.StartTime && 46799 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (46799 >= pDaypart2.StartTime || 46799 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[26] = (pDaypart1.StartTime < pDaypart1.EndTime && ((46800 >= pDaypart1.StartTime && 46800 <= pDaypart1.EndTime) || (48599 >= pDaypart1.StartTime && 48599 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (48599 >= pDaypart1.StartTime || 48599 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((46800 >= pDaypart2.StartTime && 46800 <= pDaypart2.EndTime) || (48599 >= pDaypart2.StartTime && 48599 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (48599 >= pDaypart2.StartTime || 48599 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[27] = (pDaypart1.StartTime < pDaypart1.EndTime && ((48600 >= pDaypart1.StartTime && 48600 <= pDaypart1.EndTime) || (50399 >= pDaypart1.StartTime && 50399 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (50399 >= pDaypart1.StartTime || 50399 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((48600 >= pDaypart2.StartTime && 48600 <= pDaypart2.EndTime) || (50399 >= pDaypart2.StartTime && 50399 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (50399 >= pDaypart2.StartTime || 50399 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[28] = (pDaypart1.StartTime < pDaypart1.EndTime && ((50400 >= pDaypart1.StartTime && 50400 <= pDaypart1.EndTime) || (52199 >= pDaypart1.StartTime && 52199 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (52199 >= pDaypart1.StartTime || 52199 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((50400 >= pDaypart2.StartTime && 50400 <= pDaypart2.EndTime) || (52199 >= pDaypart2.StartTime && 52199 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (52199 >= pDaypart2.StartTime || 52199 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[29] = (pDaypart1.StartTime < pDaypart1.EndTime && ((52200 >= pDaypart1.StartTime && 52200 <= pDaypart1.EndTime) || (53999 >= pDaypart1.StartTime && 53999 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (53999 >= pDaypart1.StartTime || 53999 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((52200 >= pDaypart2.StartTime && 52200 <= pDaypart2.EndTime) || (53999 >= pDaypart2.StartTime && 53999 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (53999 >= pDaypart2.StartTime || 53999 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[30] = (pDaypart1.StartTime < pDaypart1.EndTime && ((54000 >= pDaypart1.StartTime && 54000 <= pDaypart1.EndTime) || (55799 >= pDaypart1.StartTime && 55799 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (55799 >= pDaypart1.StartTime || 55799 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((54000 >= pDaypart2.StartTime && 54000 <= pDaypart2.EndTime) || (55799 >= pDaypart2.StartTime && 55799 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (55799 >= pDaypart2.StartTime || 55799 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[31] = (pDaypart1.StartTime < pDaypart1.EndTime && ((55800 >= pDaypart1.StartTime && 55800 <= pDaypart1.EndTime) || (57599 >= pDaypart1.StartTime && 57599 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (57599 >= pDaypart1.StartTime || 57599 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((55800 >= pDaypart2.StartTime && 55800 <= pDaypart2.EndTime) || (57599 >= pDaypart2.StartTime && 57599 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (57599 >= pDaypart2.StartTime || 57599 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[32] = (pDaypart1.StartTime < pDaypart1.EndTime && ((57600 >= pDaypart1.StartTime && 57600 <= pDaypart1.EndTime) || (59399 >= pDaypart1.StartTime && 59399 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (59399 >= pDaypart1.StartTime || 59399 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((57600 >= pDaypart2.StartTime && 57600 <= pDaypart2.EndTime) || (59399 >= pDaypart2.StartTime && 59399 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (59399 >= pDaypart2.StartTime || 59399 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[33] = (pDaypart1.StartTime < pDaypart1.EndTime && ((59400 >= pDaypart1.StartTime && 59400 <= pDaypart1.EndTime) || (61199 >= pDaypart1.StartTime && 61199 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (61199 >= pDaypart1.StartTime || 61199 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((59400 >= pDaypart2.StartTime && 59400 <= pDaypart2.EndTime) || (61199 >= pDaypart2.StartTime && 61199 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (61199 >= pDaypart2.StartTime || 61199 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[34] = (pDaypart1.StartTime < pDaypart1.EndTime && ((61200 >= pDaypart1.StartTime && 61200 <= pDaypart1.EndTime) || (62999 >= pDaypart1.StartTime && 62999 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (62999 >= pDaypart1.StartTime || 62999 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((61200 >= pDaypart2.StartTime && 61200 <= pDaypart2.EndTime) || (62999 >= pDaypart2.StartTime && 62999 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (62999 >= pDaypart2.StartTime || 62999 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[35] = (pDaypart1.StartTime < pDaypart1.EndTime && ((63000 >= pDaypart1.StartTime && 63000 <= pDaypart1.EndTime) || (64799 >= pDaypart1.StartTime && 64799 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (64799 >= pDaypart1.StartTime || 64799 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((63000 >= pDaypart2.StartTime && 63000 <= pDaypart2.EndTime) || (64799 >= pDaypart2.StartTime && 64799 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (64799 >= pDaypart2.StartTime || 64799 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[36] = (pDaypart1.StartTime < pDaypart1.EndTime && ((64800 >= pDaypart1.StartTime && 64800 <= pDaypart1.EndTime) || (66599 >= pDaypart1.StartTime && 66599 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (66599 >= pDaypart1.StartTime || 66599 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((64800 >= pDaypart2.StartTime && 64800 <= pDaypart2.EndTime) || (66599 >= pDaypart2.StartTime && 66599 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (66599 >= pDaypart2.StartTime || 66599 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[37] = (pDaypart1.StartTime < pDaypart1.EndTime && ((66600 >= pDaypart1.StartTime && 66600 <= pDaypart1.EndTime) || (68399 >= pDaypart1.StartTime && 68399 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (68399 >= pDaypart1.StartTime || 68399 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((66600 >= pDaypart2.StartTime && 66600 <= pDaypart2.EndTime) || (68399 >= pDaypart2.StartTime && 68399 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (68399 >= pDaypart2.StartTime || 68399 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[38] = (pDaypart1.StartTime < pDaypart1.EndTime && ((68400 >= pDaypart1.StartTime && 68400 <= pDaypart1.EndTime) || (70199 >= pDaypart1.StartTime && 70199 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (70199 >= pDaypart1.StartTime || 70199 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((68400 >= pDaypart2.StartTime && 68400 <= pDaypart2.EndTime) || (70199 >= pDaypart2.StartTime && 70199 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (70199 >= pDaypart2.StartTime || 70199 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[39] = (pDaypart1.StartTime < pDaypart1.EndTime && ((70200 >= pDaypart1.StartTime && 70200 <= pDaypart1.EndTime) || (71999 >= pDaypart1.StartTime && 71999 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (71999 >= pDaypart1.StartTime || 71999 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((70200 >= pDaypart2.StartTime && 70200 <= pDaypart2.EndTime) || (71999 >= pDaypart2.StartTime && 71999 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (71999 >= pDaypart2.StartTime || 71999 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[40] = (pDaypart1.StartTime < pDaypart1.EndTime && ((72000 >= pDaypart1.StartTime && 72000 <= pDaypart1.EndTime) || (73799 >= pDaypart1.StartTime && 73799 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (73799 >= pDaypart1.StartTime || 73799 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((72000 >= pDaypart2.StartTime && 72000 <= pDaypart2.EndTime) || (73799 >= pDaypart2.StartTime && 73799 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (73799 >= pDaypart2.StartTime || 73799 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[41] = (pDaypart1.StartTime < pDaypart1.EndTime && ((73800 >= pDaypart1.StartTime && 73800 <= pDaypart1.EndTime) || (75599 >= pDaypart1.StartTime && 75599 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (75599 >= pDaypart1.StartTime || 75599 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((73800 >= pDaypart2.StartTime && 73800 <= pDaypart2.EndTime) || (75599 >= pDaypart2.StartTime && 75599 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (75599 >= pDaypart2.StartTime || 75599 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[42] = (pDaypart1.StartTime < pDaypart1.EndTime && ((75600 >= pDaypart1.StartTime && 75600 <= pDaypart1.EndTime) || (77399 >= pDaypart1.StartTime && 77399 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (77399 >= pDaypart1.StartTime || 77399 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((75600 >= pDaypart2.StartTime && 75600 <= pDaypart2.EndTime) || (77399 >= pDaypart2.StartTime && 77399 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (77399 >= pDaypart2.StartTime || 77399 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[43] = (pDaypart1.StartTime < pDaypart1.EndTime && ((77400 >= pDaypart1.StartTime && 77400 <= pDaypart1.EndTime) || (79199 >= pDaypart1.StartTime && 79199 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (79199 >= pDaypart1.StartTime || 79199 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((77400 >= pDaypart2.StartTime && 77400 <= pDaypart2.EndTime) || (79199 >= pDaypart2.StartTime && 79199 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (79199 >= pDaypart2.StartTime || 79199 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[44] = (pDaypart1.StartTime < pDaypart1.EndTime && ((79200 >= pDaypart1.StartTime && 79200 <= pDaypart1.EndTime) || (80999 >= pDaypart1.StartTime && 80999 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (80999 >= pDaypart1.StartTime || 80999 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((79200 >= pDaypart2.StartTime && 79200 <= pDaypart2.EndTime) || (80999 >= pDaypart2.StartTime && 80999 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (80999 >= pDaypart2.StartTime || 80999 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[45] = (pDaypart1.StartTime < pDaypart1.EndTime && ((81000 >= pDaypart1.StartTime && 81000 <= pDaypart1.EndTime) || (82799 >= pDaypart1.StartTime && 82799 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (82799 >= pDaypart1.StartTime || 82799 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((81000 >= pDaypart2.StartTime && 81000 <= pDaypart2.EndTime) || (82799 >= pDaypart2.StartTime && 82799 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (82799 >= pDaypart2.StartTime || 82799 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[46] = (pDaypart1.StartTime < pDaypart1.EndTime && ((82800 >= pDaypart1.StartTime && 82800 <= pDaypart1.EndTime) || (84599 >= pDaypart1.StartTime && 84599 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (84599 >= pDaypart1.StartTime || 84599 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((82800 >= pDaypart2.StartTime && 82800 <= pDaypart2.EndTime) || (84599 >= pDaypart2.StartTime && 84599 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (84599 >= pDaypart2.StartTime || 84599 <= pDaypart2.EndTime) ? 1 : 0);

            lTime[47] = (pDaypart1.StartTime < pDaypart1.EndTime && ((84600 >= pDaypart1.StartTime && 84600 <= pDaypart1.EndTime) || (86399 >= pDaypart1.StartTime && 86399 <= pDaypart1.EndTime)) ? 1 :
                           pDaypart1.EndTime < pDaypart1.StartTime && (86399 >= pDaypart1.StartTime || 86399 <= pDaypart1.EndTime) ? 1 : 0)
                           &
                          (pDaypart2.StartTime < pDaypart2.EndTime && ((84600 >= pDaypart2.StartTime && 84600 <= pDaypart2.EndTime) || (86399 >= pDaypart2.StartTime && 86399 <= pDaypart2.EndTime)) ? 1 :
                           pDaypart2.EndTime < pDaypart2.StartTime && (86399 >= pDaypart2.StartTime || 86399 <= pDaypart2.EndTime) ? 1 : 0);

            for (int i = 0; i < lTime.Length; i++)
            {
                if (lTime[i] == 1)
                {
                    pStartTime = i * 1800;
                    break;
                }
            }
            for (int i = lTime.Length - 1; i >= 0; i--)
            {
                if (lTime[i] == 1)
                {
                    pEndTime = ((i + 1) * 1800) - 1;
                    break;
                }
            }

            return (pStartTime != -1 && pEndTime != -1);
        }
        public static DisplayDaypart ParseMsaDaypart(string pDaypartString)
        {
            string lDays = "";
            string lTimes = "";
            string[] lTimesSplit;

            DisplayDaypart lDisplayDaypart = new DisplayDaypart();
            lDisplayDaypart.Code = "";
            lDisplayDaypart.Name = "";

            #region Split Days/Times
            int lLastDayIndex = 0;
            for (int i = 0; i < pDaypartString.Length; i++)
            {
                if (!pDaypartString.Substring(i, 1).IsNumeric())
                {
                    lDays += pDaypartString.Substring(i, 1);
                    lLastDayIndex = i;
                }
                else
                    break;
            }

            lDays = lDays.Trim();
            lTimes = pDaypartString.Substring(lLastDayIndex, pDaypartString.Length - lLastDayIndex).Trim().Replace(" ", "");
            lTimesSplit = lTimes.Split(new char[] { '-' });
            if (lTimesSplit.Length != 2)
                return null;
            #endregion
            #region Days
            if (lDays.Contains(","))
            {
                string[] lDayGroups = lDays.Split(new char[] { ',' });
                ParseDays(lDisplayDaypart, lDayGroups);
            }
            else
            {
                ParseDays(lDisplayDaypart, new string[] { lDays });
            }
            #endregion
            #region Times
            lDisplayDaypart.StartTime = ParseTime(lTimesSplit[0]).Value;
            lDisplayDaypart.EndTime = ParseTime(lTimesSplit[1]).Value - 1;
            #endregion

            if (lDisplayDaypart.StartTime == 86400)
            {
                lDisplayDaypart.StartTime = 0;
            }

            return (lDisplayDaypart);
        }

        private static int? ParseTime(string pTimeString)
        {
            int lReturn = -1;

            int lHour;
            int lMinute;
            int lSecond;

            bool AM = pTimeString.Contains("A") || pTimeString.Contains("AM");
            bool PM = pTimeString.Contains("P") || pTimeString.Contains("PM");

            pTimeString = pTimeString.Replace("A", "").Replace("a", "").Replace("P", "").Replace("p", "").Replace("M", "").Replace("m", "");

            if (pTimeString.Contains(":"))
            {
                string[] lTimePieces = pTimeString.Split(new char[] { ':' });
                if (lTimePieces.Length == 2)
                {
                    if (!int.TryParse(lTimePieces[0], out lHour))
                        return (null);
                    if (!int.TryParse(lTimePieces[1], out lMinute))
                        return (null);

                    if (PM && lHour < 12)
                        lHour += 12;
                    if (AM && lHour >= 12)
                        lHour -= 12;

                    lReturn = (lHour * 3600) + (lMinute * 60);
                }
                else if (lTimePieces.Length == 3)
                {
                    if (!int.TryParse(lTimePieces[0], out lHour))
                        return (null);
                    if (!int.TryParse(lTimePieces[1], out lMinute))
                        return (null);
                    if (!int.TryParse(lTimePieces[2], out lSecond))
                        return (null);

                    if (PM && lHour < 12)
                        lHour += 12;
                    if (AM && lHour >= 12)
                        lHour -= 12;

                    lReturn = (lHour * 3600) + (lMinute * 60) + lSecond;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (!int.TryParse(pTimeString, out lHour))
                    return (null);

                if (PM && lHour < 12)
                    lHour += 12;
                if (AM && lHour >= 12)
                    lHour -= 12;

                if (lHour == 0)
                    lHour = 24;

                lReturn = (lHour * 3600);
            }

            return lReturn == -1 ? null : (int?)lReturn;
        }

        private static void ParseDays(DisplayDaypart pDaypart, string[] pDayGroups)
        {
            string[] lDayConstants1 = new string[7] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };
            string[] lDayConstants2 = new string[7] { "M", "TU", "W", "TH", "F", "SA", "SU" };
            string[] lDayConstants3 = new string[7] { "MO", "TU", "WE", "TH", "FR", "SA", "SU" };
            string[] lDayConstants4 = new string[7] { "M", "T", "W", "R", "F", "S", "SU" };

            foreach (string lDayGroup in pDayGroups)
            {
                if (lDayGroup.Trim() == "")
                    continue;

                if (lDayGroup.Contains("-"))
                {
                    int lEndIndex = 0;
                    int lStartIndex = 0;
                    string[] lSplitDays = lDayGroup.Split(new char[] { '-' });
                    for (int i = 0; i < lSplitDays.Length; i++)
                        lSplitDays[i] = lSplitDays[i].Trim();

                    for (int i = 0; i < 7; i++)
                    {
                        if (lDayConstants1[i] == lSplitDays[0].ToUpper() ||
                            lDayConstants2[i] == lSplitDays[0].ToUpper() ||
                            lDayConstants3[i] == lSplitDays[0].ToUpper() ||
                            lDayConstants4[i] == lSplitDays[0].ToUpper())
                        {
                            lStartIndex = i;
                        }
                        else if (lDayConstants1[i] == lSplitDays[1].ToUpper() ||
                                 lDayConstants2[i] == lSplitDays[1].ToUpper() ||
                                 lDayConstants3[i] == lSplitDays[1].ToUpper() ||
                                 lDayConstants4[i] == lSplitDays[1].ToUpper())
                        {
                            lEndIndex = i;
                        }
                    }
                    for (int i = 0; i < 7; i++)
                    {
                        if (i >= lStartIndex && i <= lEndIndex)
                        {
                            switch (i)
                            {
                                case (0):
                                    pDaypart.Monday = true;
                                    break;
                                case (1):
                                    pDaypart.Tuesday = true;
                                    break;
                                case (2):
                                    pDaypart.Wednesday = true;
                                    break;
                                case (3):
                                    pDaypart.Thursday = true;
                                    break;
                                case (4):
                                    pDaypart.Friday = true;
                                    break;
                                case (5):
                                    pDaypart.Saturday = true;
                                    break;
                                case (6):
                                    pDaypart.Sunday = true;
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (lDayConstants1[i] == lDayGroup.ToUpper() ||
                            lDayConstants2[i] == lDayGroup.ToUpper() ||
                            lDayConstants3[i] == lDayGroup.ToUpper() ||
                            lDayConstants4[i] == lDayGroup.ToUpper())
                        {
                            switch (i)
                            {
                                case (0):
                                    pDaypart.Monday = true;
                                    break;
                                case (1):
                                    pDaypart.Tuesday = true;
                                    break;
                                case (2):
                                    pDaypart.Wednesday = true;
                                    break;
                                case (3):
                                    pDaypart.Thursday = true;
                                    break;
                                case (4):
                                    pDaypart.Friday = true;
                                    break;
                                case (5):
                                    pDaypart.Saturday = true;
                                    break;
                                case (6):
                                    pDaypart.Sunday = true;
                                    break;
                            }
                        }
                    }
                }
            }
        }
        public static DisplayDaypart ParseNielsenNadDaypart(string pDaypartString)
        {
            string lDays = "";
            string lTimes = "";
            string[] lTimesSplit;

            DisplayDaypart lDisplayDaypart = new DisplayDaypart();
            lDisplayDaypart.Code = "";
            lDisplayDaypart.Name = "";

            #region Split Days/Times
            int lLastDayIndex = 0;
            for (int i = 0; i < pDaypartString.Length; i++)
            {
                if (!pDaypartString.Substring(i, 1).IsNumeric())
                {
                    lDays += pDaypartString.Substring(i, 1);
                    lLastDayIndex = i;
                }
                else
                    break;
            }

            lDays = lDays.Trim();
            lTimes = pDaypartString.Substring(lLastDayIndex, pDaypartString.Length - lLastDayIndex).Trim().Replace(" ", "");
            lTimesSplit = lTimes.Split(new char[] { '-' });
            if (lTimesSplit.Length != 2)
                return null;
            #endregion
            #region Days
            if (lDays.Contains(","))
            {
                string[] lDayGroups = lDays.Split(new char[] { ',' });
                ParseDays(lDisplayDaypart, lDayGroups);
            }
            else
            {
                ParseDays(lDisplayDaypart, new string[] { lDays });
            }
            #endregion
            #region Times
            lDisplayDaypart.StartTime = ParseTime(lTimesSplit[0]).Value;
            lDisplayDaypart.EndTime = ParseTime(lTimesSplit[1]).Value;
            if (lDisplayDaypart.EndTime == 0)
                lDisplayDaypart.EndTime = 86399;
            else
                lDisplayDaypart.EndTime -= 1;
            #endregion

            if (lDisplayDaypart.StartTime == 86400)
            {
                lDisplayDaypart.StartTime = 0;
            }

            return (lDisplayDaypart);
        }
        public static DisplayDaypart ParseFileMakerDaypart(string pDaypartString)
        {
            int lHour = 0;
            int lMinute = 0;
            int lEndIndex = 0;
            int lStartIndex = 0;
            string lDays;
            string[] lDaysSplit;
            string lTimes;
            string[] lTimesSplit;
            string[] lTime;

            DisplayDaypart lDisplayDaypart = new DisplayDaypart();
            lDisplayDaypart.Code = "";
            lDisplayDaypart.Name = "";

            #region Days
            if (pDaypartString.Split(new char[] { ' ' }).Length != 2)
                return (null);

            lDays = pDaypartString.Split(new char[] { ' ' })[0].Trim();
            lTimes = pDaypartString.Split(new char[] { ' ' })[1].Trim();

            lDaysSplit = lDays.Split(new char[] { '-' });
            if (lDaysSplit.Length != 2)
                return (null);
            lTimesSplit = lTimes.Split(new char[] { '-' });
            if (lTimesSplit.Length != 2)
                return (null);
            string[] lDayConstants = new string[7] { "M", "TU", "W", "TH", "FR", "SA", "SU" };
            for (int i = 0; i < lDayConstants.Length; i++)
            {
                if (lDayConstants[i] == lDaysSplit[0])
                    lStartIndex = i;
                else if (lDayConstants[i] == lDaysSplit[1])
                    lEndIndex = i;
            }
            for (int i = 0; i < 7; i++)
            {
                if (i >= lStartIndex && i <= lEndIndex)
                {
                    switch (i)
                    {
                        case (0):
                            lDisplayDaypart.Monday = true;
                            break;
                        case (1):
                            lDisplayDaypart.Tuesday = true;
                            break;
                        case (2):
                            lDisplayDaypart.Wednesday = true;
                            break;
                        case (3):
                            lDisplayDaypart.Thursday = true;
                            break;
                        case (4):
                            lDisplayDaypart.Friday = true;
                            break;
                        case (5):
                            lDisplayDaypart.Saturday = true;
                            break;
                        case (6):
                            lDisplayDaypart.Sunday = true;
                            break;
                    }
                }
            }
            #endregion
            #region Times
            #region Start Time
            lTime = lTimesSplit[0].Split(new char[] { ':' });
            if (lTime.Length != 2)
                return (null);

            if (!int.TryParse(lTime[0], out lHour))
                return (null);
            if (!int.TryParse(lTime[1], out lMinute))
                return (null);

            lDisplayDaypart.StartTime = (lHour * 3600) + (lMinute * 60);
            #endregion
            #region End Time
            lTime = lTimesSplit[1].Split(new char[] { ':' });
            if (lTime.Length != 2)
                return (null);

            if (!int.TryParse(lTime[0], out lHour))
                return (null);
            if (!int.TryParse(lTime[1], out lMinute))
                return (null);

            lDisplayDaypart.EndTime = (lHour * 3600) + (lMinute * 60);
            #endregion
            #endregion

            return (lDisplayDaypart);
        }
        public static DisplayDaypart Union(DisplayDaypart pDaypart1, DisplayDaypart pDaypart2)
        {
            DisplayDaypart lDisplayDaypart = new DisplayDaypart();
            lDisplayDaypart.Code = "CUS";
            lDisplayDaypart.Name = "Custom";

            lDisplayDaypart.Id = -1;
            lDisplayDaypart.StartTime = pDaypart1.StartTime < pDaypart2.StartTime ? pDaypart1.StartTime : pDaypart2.StartTime;
            lDisplayDaypart.EndTime = (pDaypart1.EndTime < pDaypart1.StartTime ? pDaypart1.EndTime + 86400 : pDaypart1.EndTime) > (pDaypart2.EndTime < pDaypart2.StartTime ? pDaypart2.EndTime + 86400 : pDaypart2.EndTime) ? pDaypart1.EndTime : pDaypart2.EndTime;
            lDisplayDaypart.Monday = pDaypart1.Monday | pDaypart2.Monday;
            lDisplayDaypart.Tuesday = pDaypart1.Tuesday | pDaypart2.Tuesday;
            lDisplayDaypart.Wednesday = pDaypart1.Wednesday | pDaypart2.Wednesday;
            lDisplayDaypart.Thursday = pDaypart1.Thursday | pDaypart2.Thursday;
            lDisplayDaypart.Friday = pDaypart1.Friday | pDaypart2.Friday;
            lDisplayDaypart.Saturday = pDaypart1.Saturday | pDaypart2.Saturday;
            lDisplayDaypart.Sunday = pDaypart1.Sunday | pDaypart2.Sunday;

            return (lDisplayDaypart);
        }

        public static DisplayDaypart Union(params DisplayDaypart[] dayparts)
        {
            DisplayDaypart daypart = null;
            for (var i = 0; i < dayparts.Length; i++)
            {
                if (i == 0)
                {
                    daypart = dayparts[i];
                    continue;
                }
                daypart = Union(daypart, dayparts[i]);
            }
            return daypart;
        }

        public static DisplayDaypart Intersect(DisplayDaypart pDaypart1, DisplayDaypart pDaypart2)
        {
            DisplayDaypart lDisplayDaypart = new DisplayDaypart();
            lDisplayDaypart.Code = "CUS";
            lDisplayDaypart.Name = "Custom";
            int outstart = lDisplayDaypart.StartTime;
            int outend = lDisplayDaypart.EndTime;
            DisplayDaypart.GetIntersectingTimes(pDaypart1, pDaypart2, out outstart, out outend);
            lDisplayDaypart.StartTime = outstart;
            lDisplayDaypart.EndTime = outend;

            lDisplayDaypart.Id = -1;
            lDisplayDaypart.Monday = pDaypart1.Monday & pDaypart2.Monday;
            lDisplayDaypart.Tuesday = pDaypart1.Tuesday & pDaypart2.Tuesday;
            lDisplayDaypart.Wednesday = pDaypart1.Wednesday & pDaypart2.Wednesday;
            lDisplayDaypart.Thursday = pDaypart1.Thursday & pDaypart2.Thursday;
            lDisplayDaypart.Friday = pDaypart1.Friday & pDaypart2.Friday;
            lDisplayDaypart.Saturday = pDaypart1.Saturday & pDaypart2.Saturday;
            lDisplayDaypart.Sunday = pDaypart1.Sunday & pDaypart2.Sunday;

            return (lDisplayDaypart);
        }

        public static bool Intersects(DisplayDaypart pDaypart1, DisplayDaypart pDaypart2)
        {
            if (pDaypart1 == null || pDaypart2 == null)
                return (false);

            bool lAnyDayOverlaps =
                (pDaypart1.Monday & pDaypart2.Monday) |
                (pDaypart1.Tuesday & pDaypart2.Tuesday) |
                (pDaypart1.Wednesday & pDaypart2.Wednesday) |
                (pDaypart1.Thursday & pDaypart2.Thursday) |
                (pDaypart1.Friday & pDaypart2.Friday) |
                (pDaypart1.Saturday & pDaypart2.Saturday) |
                (pDaypart1.Sunday & pDaypart2.Sunday);

            if (pDaypart1.EndTime < pDaypart1.StartTime && pDaypart2.EndTime < pDaypart2.StartTime)
            {
                // dual overnight dayparts
                return (lAnyDayOverlaps);
            }
            else if (pDaypart1.EndTime >= pDaypart1.StartTime && pDaypart2.EndTime < pDaypart2.StartTime)
            {
                // only daypart 2 is overnight
                if (IsBetween(pDaypart2.StartTime, pDaypart1.StartTime, pDaypart1.EndTime) || IsBetween(86400, pDaypart1.StartTime, pDaypart1.EndTime) ||
                    IsBetween(0, pDaypart1.StartTime, pDaypart1.EndTime) || IsBetween(pDaypart2.EndTime, pDaypart1.StartTime, pDaypart1.EndTime) ||
                    IsBetween(pDaypart1.StartTime, pDaypart2.StartTime, 86400) || IsBetween(pDaypart1.StartTime, 0, pDaypart2.EndTime) ||
                    IsBetween(pDaypart1.EndTime, pDaypart2.StartTime, 86400) || IsBetween(pDaypart1.EndTime, 0, pDaypart2.EndTime))
                    return (lAnyDayOverlaps);
            }
            else if (pDaypart1.EndTime < pDaypart1.StartTime && pDaypart2.EndTime >= pDaypart2.StartTime)
            {
                // only daypart 1 is overnight
                if (IsBetween(pDaypart1.StartTime, pDaypart2.StartTime, pDaypart2.EndTime) || IsBetween(86400, pDaypart2.StartTime, pDaypart2.EndTime) ||
                    IsBetween(0, pDaypart2.StartTime, pDaypart2.EndTime) || IsBetween(pDaypart1.EndTime, pDaypart2.StartTime, pDaypart2.EndTime) ||
                    IsBetween(pDaypart2.StartTime, pDaypart1.StartTime, 86400) || IsBetween(pDaypart2.StartTime, 0, pDaypart1.EndTime) ||
                    IsBetween(pDaypart2.EndTime, pDaypart1.StartTime, 86400) || IsBetween(pDaypart2.EndTime, 0, pDaypart1.EndTime))
                    return (lAnyDayOverlaps);
            }
            else
            {
                // neither are overnight
                if (IsBetween(pDaypart1.StartTime, pDaypart2.StartTime, pDaypart2.EndTime) || IsBetween(pDaypart1.EndTime, pDaypart2.StartTime, pDaypart2.EndTime) ||
                    IsBetween(pDaypart2.StartTime, pDaypart1.StartTime, pDaypart1.EndTime) || IsBetween(pDaypart2.EndTime, pDaypart1.StartTime, pDaypart1.EndTime))
                    return (lAnyDayOverlaps);
            }
            return (false);
        }
        public static bool Intersects(int pStartTime1, int pEndTime1, int pStartTime2, int pEndTime2)
        {
            if (pEndTime1 < pStartTime1 && pEndTime2 < pStartTime2)
            {
                // dual overnight dayparts
                return (true);
            }
            else if (pEndTime1 >= pStartTime1 && pEndTime2 < pStartTime2)
            {
                // only daypart 2 is overnight
                if (IsBetween(pStartTime2, pStartTime1, pEndTime1) || IsBetween(86400, pStartTime1, pEndTime1) ||
                    IsBetween(0, pStartTime1, pEndTime1) || IsBetween(pEndTime2, pStartTime1, pEndTime1) ||
                    IsBetween(pStartTime1, pStartTime2, 86400) || IsBetween(pStartTime1, 0, pEndTime2) ||
                    IsBetween(pEndTime1, pStartTime2, 86400) || IsBetween(pEndTime1, 0, pEndTime2))
                    return (true);
            }
            else if (pEndTime1 < pStartTime1 && pEndTime2 >= pStartTime2)
            {
                // only daypart 1 is overnight
                if (IsBetween(pStartTime1, pStartTime2, pEndTime2) || IsBetween(86400, pStartTime2, pEndTime2) ||
                    IsBetween(0, pStartTime2, pEndTime2) || IsBetween(pEndTime1, pStartTime2, pEndTime2) ||
                    IsBetween(pStartTime2, pStartTime1, 86400) || IsBetween(pStartTime2, 0, pEndTime1) ||
                    IsBetween(pEndTime2, pStartTime1, 86400) || IsBetween(pEndTime2, 0, pEndTime1))
                    return (true);
            }
            else
            {
                // neither are overnight
                if (IsBetween(pStartTime1, pStartTime2, pEndTime2) || IsBetween(pEndTime1, pStartTime2, pEndTime2) ||
                    IsBetween(pStartTime2, pStartTime1, pEndTime1) || IsBetween(pEndTime2, pStartTime1, pEndTime1))
                    return (true);
            }
            return (false);
        }
        public static bool InsersectsAny(DisplayDaypart pDaypart1, DisplayDaypart[] pDaypartsToSearch)
        {
            if (pDaypart1 == null || pDaypartsToSearch == null)
                return (false);

            foreach (DisplayDaypart lDaypart in pDaypartsToSearch)
            {
                if (DisplayDaypart.Intersects(pDaypart1, lDaypart))
                    return (true);
            }
            return (false);
        }
        public static DisplayDaypart[] GetIntersectingDayparts(DisplayDaypart pDaypart, DisplayDaypart[] pDaypartsToSearch)
        {
            if (pDaypart == null || pDaypartsToSearch == null)
                return (new DisplayDaypart[0]);

            List<DisplayDaypart> lReturn = new List<DisplayDaypart>();
            foreach (DisplayDaypart lDaypart in pDaypartsToSearch)
            {
                if (Intersects(pDaypart, lDaypart))
                    lReturn.Add(lDaypart);
            }
            return (lReturn.ToArray());
        }
        public static bool IsBetween(int pTimeToCheck, int pStartTime, int pEndTime)
        {
            return (pTimeToCheck >= pStartTime && pTimeToCheck <= pEndTime);
        }
        public static bool IsContained(DisplayDaypart daypart1, DisplayDaypart daypart2)
        {
            // check days of the week
            var startDayEqualOrAfter = daypart1.StartMediaDay >= daypart2.StartMediaDay;
            var endDayEqualOrBefore = daypart1.EndMediaDay <= daypart2.EndMediaDay;
            if (!startDayEqualOrAfter || !endDayEqualOrBefore)
            {
                return false;
            }

            // check time
            var startTimeEqualOrAfter = daypart1.StartTime >= daypart2.StartTime;
            var endTimeEqualOrBefore = daypart1.EndTime <= daypart2.EndTime;
            return startTimeEqualOrAfter && endTimeEqualOrBefore;
        }

        public void EnableTime(int pStartTime, int pEndTime)
        {
            if (this._StartTime == 0 && this._EndTime == 0)
            {
                this._StartTime = pStartTime;
                this._EndTime = pEndTime;
                return;
            }

            bool[] lSeconds = new bool[86400];
            if (this._StartTime > this._EndTime)
            {
                // overnight
                for (int i = this._StartTime; i <= 86399; i++)
                    lSeconds[i] = true;
                for (int i = 0; i <= this._EndTime; i++)
                    lSeconds[i] = true;
            }
            else
            {
                for (int i = this._StartTime; i <= this._EndTime; i++)
                    lSeconds[i] = true;
            }

            if (pStartTime > pEndTime)
            {
                // overnight
                for (int i = pStartTime; i <= 86399; i++)
                    lSeconds[i] = true;
                for (int i = 0; i <= pEndTime; i++)
                    lSeconds[i] = true;
            }
            else
            {
                for (int i = pStartTime; i <= pEndTime; i++)
                    lSeconds[i] = true;
            }

            if (lSeconds[86399] && lSeconds[0] && this.CountSeconds(lSeconds) != 86400)
            {
                // overnight
                int lStart = 0;
                for (int i = 86399; i >= 0; i--)
                {
                    if (!lSeconds[i])
                    {
                        lStart = i + 1;
                        break;
                    }
                }
                this._StartTime = lStart > 86399 ? 86399 : lStart;

                int lEnd = 0;
                for (int i = 0; i <= 86399; i++)
                {
                    if (!lSeconds[i])
                    {
                        lEnd = i - 1;
                        break;
                    }
                }
                this._EndTime = lEnd < 0 ? 0 : lEnd;
            }
            else
            {
                int lStart = 0;
                for (int i = 0; i <= 86399; i++)
                {
                    if (lSeconds[i])
                    {
                        lStart = i;
                        break;
                    }
                }
                this._StartTime = lStart < 0 ? 0 : lStart;

                int lEnd = 0;
                for (int i = 86399; i >= 0; i--)
                {
                    if (lSeconds[i])
                    {
                        lEnd = i;
                        break;
                    }
                }
                this._EndTime = lEnd > 86399 ? 86399 : lEnd;
            }
        }
        private int CountSeconds(bool[] pSeconds)
        {
            int lReturn = 0;
            for (int i = 0; i < pSeconds.Length; i++)
                lReturn += pSeconds[i] ? 1 : 0;
            return lReturn;
        }
        public static bool AreEqual(DisplayDaypart pSourceDaypart, DisplayDaypart[] pSubDayparts)
        {
            Dictionary<DayOfWeek, DisplayDaypart> lCombinedDayparts = new Dictionary<DayOfWeek, DisplayDaypart>();
            if (pSourceDaypart.Monday)
                lCombinedDayparts[DayOfWeek.Monday] = new DisplayDaypart(-1, "", "", 0, 0, true, false, false, false, false, false, false);
            if (pSourceDaypart.Tuesday)
                lCombinedDayparts[DayOfWeek.Tuesday] = new DisplayDaypart(-1, "", "", 0, 0, false, true, false, false, false, false, false);
            if (pSourceDaypart.Wednesday)
                lCombinedDayparts[DayOfWeek.Wednesday] = new DisplayDaypart(-1, "", "", 0, 0, false, false, true, false, false, false, false);
            if (pSourceDaypart.Thursday)
                lCombinedDayparts[DayOfWeek.Thursday] = new DisplayDaypart(-1, "", "", 0, 0, false, false, false, true, false, false, false);
            if (pSourceDaypart.Friday)
                lCombinedDayparts[DayOfWeek.Friday] = new DisplayDaypart(-1, "", "", 0, 0, false, false, false, false, true, false, false);
            if (pSourceDaypart.Saturday)
                lCombinedDayparts[DayOfWeek.Saturday] = new DisplayDaypart(-1, "", "", 0, 0, false, false, false, false, false, true, false);
            if (pSourceDaypart.Sunday)
                lCombinedDayparts[DayOfWeek.Sunday] = new DisplayDaypart(-1, "", "", 0, 0, false, false, false, false, false, false, true);

            foreach (DisplayDaypart lDaypart in pSubDayparts)
            {
                foreach (DayOfWeek lDay in lDaypart.Days)
                {
                    if (lCombinedDayparts.ContainsKey(lDay))
                    {
                        int lStartTime;
                        int lEndTime;
                        DisplayDaypart.GetIntersectingTimes(lDaypart, pSourceDaypart, out lStartTime, out lEndTime);

                        if (lStartTime != -1 && lEndTime != -1)
                            lCombinedDayparts[lDay].EnableTime(lStartTime, lEndTime);
                    }
                }
            }
            // check times
            double lTotalHours = 0D;
            foreach (KeyValuePair<DayOfWeek, DisplayDaypart> lDaypart in lCombinedDayparts)
            {
                lTotalHours += lDaypart.Value.TotalHours;

                if (lDaypart.Value.StartTime != pSourceDaypart.StartTime ||
                    lDaypart.Value.EndTime != pSourceDaypart.EndTime)
                    return (false);
            }
            if (lTotalHours != pSourceDaypart.TotalHours)
                return (false);

            return (true);
        }
        public string GetStartDayOfWeek(DateTime pDateTime)
        {
            return pDateTime.ToString("ddd");
        }
        /// <summary>
        /// Converts the passed hour in 24 hour format to a readable hour such as 12A, 12P, 1A, 1P, etc...
        /// </summary>
        /// <param name="pHour">1-24</param>
        /// <returns>Formatted hour as </returns>
        public static string FormatHour(int pHour)
        {
            if (pHour == 0 || pHour == 24)
                return "12A";
            if (pHour == 12)
                return "12P";
            if (pHour < 12)
                return pHour + "A";
            return (pHour - 12) + "P";
        }

        public static Predicate<DisplayDaypart> Find(string name)
        {
            return delegate (DisplayDaypart item)
            {
                return item.Name == name;
            };
        }
        public static Predicate<DisplayDaypart> Find(int id)
        {
            return delegate (DisplayDaypart item)
            {
                return item.Id == id;
            };
        }

        public static bool TryParse(string value, out global::Tam.Maestro.Services.ContractInterfaces.Common.DisplayDaypart result)
        {
            result = null;

            if (!string.IsNullOrEmpty(value))
            {
                string lDays;
                string lTimes;
                string[] lTimesSplit;

                DisplayDaypart lDisplayDaypart = new DisplayDaypart();
                lDisplayDaypart.Code = "CUS";
                lDisplayDaypart.Name = "Custom";

                if (value.Split(new char[] { ' ' }).Length != 2)
                    return (false);

                lDays = value.Split(new char[] { ' ' })[0].Trim();
                lTimes = value.Split(new char[] { ' ' })[1].Trim();

                lTimesSplit = lTimes.Split(new char[] { '-' });
                if (lTimesSplit.Length != 2)
                    return (false);

                #region Days
                if (lDays.Contains(","))
                {
                    string[] lDayGroups = lDays.Split(new char[] { ',' });
                    ParseDays(lDisplayDaypart, lDayGroups);
                }
                else
                {
                    ParseDays(lDisplayDaypart, new string[] { lDays });
                }
                #endregion
                #region Times
                #region Start Time
                int? lStartTime = ParseTime(lTimesSplit[0]).Value;
                if (!lStartTime.HasValue)
                    return false;
                lDisplayDaypart.StartTime = lStartTime.Value;
                #endregion
                #region End Time
                int? lEndTime = ParseTime(lTimesSplit[1]).Value;
                if (!lEndTime.HasValue)
                    return false;
                lDisplayDaypart.EndTime = lEndTime.Value - 1;
                #endregion
                #endregion

                if (lDisplayDaypart.StartTime == 86400)
                {
                    lDisplayDaypart.StartTime = 0;
                }

                result = lDisplayDaypart;
            }

            return (result != null);
        }

        public string ToLongString(bool onlyWeekDays = false)
        {
            List<string> lPairs = new List<string>();
            StringBuilder lBuilder = new StringBuilder();
            StringBuilder lDaysBuilder = new StringBuilder();

            bool[] lDaysEnabled = new bool[7] { this.Monday, this.Tuesday, this.Wednesday, this.Thursday, this.Friday, this.Saturday, this.Sunday };
            string[] lDayAbbreviationStrings = new string[7] { "M", "TU", "W", "TH", "F", "SA", "SU" };

            int lStartDay = 0;
            int lEndDay = 0;
            int lCurrentDay = 0;

            while (lCurrentDay <= 6)
            {
                if (lDaysEnabled[(int)lCurrentDay])
                {
                    lStartDay = lCurrentDay;
                    while (++lCurrentDay <= 6 && lDaysEnabled[(int)lCurrentDay])
                        lEndDay = lCurrentDay;
                    if (lEndDay > lStartDay)
                        lDaysBuilder.Append(string.Format("{0}{1}-{2}", ((lDaysBuilder.Length > 0) ? "," : ""), lDayAbbreviationStrings[lStartDay].ToString(), lDayAbbreviationStrings[lEndDay].ToString()));
                    else
                        lDaysBuilder.Append(string.Format("{0}{1}", ((lDaysBuilder.Length > 0) ? "," : ""), lDayAbbreviationStrings[lStartDay].ToString()));
                }
                else
                    lCurrentDay++;
            }

            lBuilder.Append(lDaysBuilder.ToString());
            if (onlyWeekDays)
                return lBuilder.ToString();

            lBuilder.Append(" ");

            DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));

            DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime, 0));

            // append the start/end times

            // adjust 59 minutes to full hour for display purposes
            if (lEndTime.Second == 59)
                lEndTime = lEndTime.AddSeconds(1.0);

            // special case
            if (((TimeSpan)lEndTime.Subtract(lStartTime)).TotalHours == 24.0)
            {
                lBuilder.Append("24HR");
            }
            else
            {
                if (lStartTime.Minute == 0)
                    lBuilder.Append(lStartTime.ToString("htt"));
                else
                    lBuilder.Append(lStartTime.ToString("h:mmtt"));

                lBuilder.Append("-");

                if (lEndTime.Minute == 0)
                    lBuilder.Append(lEndTime.ToString("htt"));
                else
                    lBuilder.Append(lEndTime.ToString("h:mmtt"));
            }

            return (lBuilder.ToString());
        }
        public string ToDayString()
        {
            List<string> lPairs = new List<string>();
            StringBuilder lBuilder = new StringBuilder();
            StringBuilder lDaysBuilder = new StringBuilder();

            bool[] lDaysEnabled = new bool[7] { this.Monday, this.Tuesday, this.Wednesday, this.Thursday, this.Friday, this.Saturday, this.Sunday };
            string[] lDayAbbreviationStrings = new string[7] { "M", "TU", "W", "TH", "F", "SA", "SU" };

            int lStartDay = 0;
            int lEndDay = 0;
            int lCurrentDay = 0;

            while (lCurrentDay <= 6)
            {
                if (lDaysEnabled[(int)lCurrentDay])
                {
                    lStartDay = lCurrentDay;
                    while (++lCurrentDay <= 6 && lDaysEnabled[(int)lCurrentDay])
                        lEndDay = lCurrentDay;
                    if (lEndDay > lStartDay)
                        lDaysBuilder.Append(string.Format("{0}{1}-{2}", ((lDaysBuilder.Length > 0) ? "," : ""), lDayAbbreviationStrings[lStartDay].ToString(), lDayAbbreviationStrings[lEndDay].ToString()));
                    else
                        lDaysBuilder.Append(string.Format("{0}{1}", ((lDaysBuilder.Length > 0) ? "," : ""), lDayAbbreviationStrings[lStartDay].ToString()));
                }
                else
                    lCurrentDay++;
            }

            lBuilder.Append(lDaysBuilder.ToString());

            return (lBuilder.ToString());
        }
        public string ToTimeString()
        {
            List<string> lPairs = new List<string>();
            StringBuilder lBuilder = new StringBuilder();

            DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));

            DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime, 0));

            // append the start/end times

            // adjust 59 minutes to full hour for display purposes
            if (lEndTime.Second == 59)
                lEndTime = lEndTime.AddSeconds(1.0);

            // special case
            if (((TimeSpan)lEndTime.Subtract(lStartTime)).TotalHours == 24.0)
            {
                lBuilder.Append("24HR");
            }
            else
            {
                if (lStartTime.Minute == 0)
                    lBuilder.Append(lStartTime.ToString("htt"));
                else
                    lBuilder.Append(lStartTime.ToString("h:mmtt"));

                lBuilder.Append("-");

                if (lEndTime.Minute == 0)
                    lBuilder.Append(lEndTime.ToString("htt"));
                else
                    lBuilder.Append(lEndTime.ToString("h:mmtt"));
            }

            return (lBuilder.ToString());
        }
        public string ToActualStartTimeString()
        {
            string lReturn = "";

            double lHour = Math.Floor(this._StartTime / 3600.0);
            double lMinute = Math.Floor((this._StartTime % 3600.0) / 60.0);
            double lSecond = this._StartTime - ((lHour * 3600.0) + (lMinute * 60.0));

            lReturn = lHour.ToString("00") + ":" + lMinute.ToString("00") + ":" + lSecond.ToString("00");

            return (lReturn);
        }
        public string ToActualEndTimeString()
        {
            string lReturn = "";

            double lHour = Math.Floor(this._EndTime / 3600.0);
            double lMinute = Math.Floor((this._EndTime % 3600.0) / 60.0);
            double lSecond = this._EndTime - ((lHour * 3600.0) + (lMinute * 60.0));

            lReturn = lHour.ToString() + ":" + lMinute.ToString() + ":" + lSecond.ToString();

            return (lReturn);
        }
        public string ToRoundedEndTimeString()
        {
            string lReturn = "";

            double lHour = Math.Floor(this.RoundedEndTime / 3600.0);
            double lMinute = Math.Floor((this.RoundedEndTime % 3600.0) / 60.0);
            double lSecond = this.RoundedEndTime - ((lHour * 3600.0) + (lMinute * 60.0));

            if (lHour == 24)
                lReturn = "00:00:00";
            else
                lReturn = lHour.ToString("00") + ":" + lMinute.ToString("00") + ":" + lSecond.ToString("00");

            return (lReturn);
        }
        public string ToActualSecondString()
        {
            List<string> lPairs = new List<string>();
            StringBuilder lBuilder = new StringBuilder();
            StringBuilder lDaysBuilder = new StringBuilder();

            bool[] lDaysEnabled = new bool[7] { this.Monday, this.Tuesday, this.Wednesday, this.Thursday, this.Friday, this.Saturday, this.Sunday };
            string[] lDayAbbreviationStrings = new string[7] { "M", "TU", "W", "TH", "F", "SA", "SU" };

            int lStartDay = 0;
            int lEndDay = 0;
            int lCurrentDay = 0;

            while (lCurrentDay <= 6)
            {
                if (lDaysEnabled[(int)lCurrentDay])
                {
                    lStartDay = lCurrentDay;
                    while (++lCurrentDay <= 6 && lDaysEnabled[(int)lCurrentDay])
                        lEndDay = lCurrentDay;
                    if (lEndDay > lStartDay)
                        lDaysBuilder.Append(string.Format("{0}{1}-{2}", ((lDaysBuilder.Length > 0) ? "," : ""), lDayAbbreviationStrings[lStartDay].ToString(), lDayAbbreviationStrings[lEndDay].ToString()));
                    else
                        lDaysBuilder.Append(string.Format("{0}{1}", ((lDaysBuilder.Length > 0) ? "," : ""), lDayAbbreviationStrings[lStartDay].ToString()));
                }
                else
                    lCurrentDay++;
            }

            lBuilder.Append(lDaysBuilder.ToString());
            lBuilder.Append(" ");

            DateTime lStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lStartTime = lStartTime.Add(new TimeSpan(0, 0, 0, this.StartTime, 0));

            DateTime lEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            lEndTime = lEndTime.Add(new TimeSpan(0, 0, 0, this.EndTime, 0));

            // append the start/end times

            // adjust 59 minutes to full hour for display purposes
            //if (lEndTime.Second == 59)
            //    lEndTime = lEndTime.AddSeconds(1.0);

            // special case
            //if (((TimeSpan)lEndTime.Subtract(lStartTime)).TotalHours == 24.0)
            //{
            //    lBuilder.Append("24HR");
            //}
            //else
            //{
            lBuilder.Append(lStartTime.ToString("HH:mm"));
            lBuilder.Append("-");
            lBuilder.Append(lEndTime.ToString("HH:mm"));
            //}

            return (lBuilder.ToString());
        }
        public string ToUniqueString()
        {
            return (string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}", this.StartTime, this.EndTime, this.Monday, this.Tuesday, this.Wednesday, this.Thursday, this.Friday, this.Saturday, this.Sunday));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DisplayDaypart))
                return (false);
            return (this == (DisplayDaypart)obj);
        }
        public override int GetHashCode()
        {
            return this.ToLongString().GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var displayDaypart = obj as DisplayDaypart;
            if (displayDaypart == null)
            {
                throw new ArgumentException("Object is not a DisplayDaypart");
            }

            if (this.StartMediaDay < displayDaypart.StartMediaDay)
            {
                return -1;
            }
            else if (this.StartMediaDay > displayDaypart.StartMediaDay)
            {
                return 1;
            }

            if (this.StartTime < displayDaypart.StartTime)
            {
                return -1;
            }
            else if (this.StartTime == displayDaypart.StartTime)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public override string ToString()
        {
            //if (this.Code == "CUS" || this.Name.Length == 0)
            return (this.ToLongString());
            //return (this.Name);
        }
        public static bool operator ==(DisplayDaypart a, DisplayDaypart b)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
                return false;

            // Return true if the fields match:
            return (
                a.EndTime == b.EndTime &&
                a.Friday == b.Friday &&
                a.Monday == b.Monday &&
                a.Saturday == b.Saturday &&
                a.StartTime == b.StartTime &&
                a.Sunday == b.Sunday &&
                a.Thursday == b.Thursday &&
                a.Tuesday == b.Tuesday &&
                a.Wednesday == b.Wednesday);
        }
        public static bool operator !=(DisplayDaypart a, DisplayDaypart b)
        {
            return !(a == b);
        }
        public object Clone()
        {
            DisplayDaypart lClone = new DisplayDaypart(
                this.Id,
                this.Code,
                this.Name,
                this.StartTime,
                this.EndTime,
                this.Monday,
                this.Tuesday,
                this.Wednesday,
                this.Thursday,
                this.Friday,
                this.Saturday,
                this.Sunday);

            return (lClone);
        }
    }
}
