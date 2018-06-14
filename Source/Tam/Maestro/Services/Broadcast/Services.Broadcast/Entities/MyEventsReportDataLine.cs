using System;

namespace Services.Broadcast.Entities
{
    public class MyEventsReportDataLine
    {
        public string ReportableName { get; set; }
        public string Advertiser { get; set; }
        public string DaypartCode { get; set; }
        public int SpotLength { get; set; }
        public DateTime ScheduleStartDate
        {
            get
            {
                var differenceToMonday = (LineupStartDate.DayOfWeek - DayOfWeek.Monday);

                if (differenceToMonday < 0)
                    differenceToMonday += 7;

                return LineupStartDate.AddDays(-differenceToMonday);
            }
        }
        public DateTime ScheduleEndDate
        {
            get
            {
                return ScheduleStartDate.AddDays(6);
            }
        }
        public string StationCallLetters { get; set; }
        public char LineupPOT { get { return 'O'; } }
        public string LineupDOW
        {
            get
            {
                var days = new char[] { '-', '-', '-', '-', '-', '-', '-' };
                days[((int)LineupStartDate.DayOfWeek + 6) % 7] = LineupStartDate.DayOfWeek.ToString()[0];
                return new string(days);
            }
        }
        public char OffAir { get { return 'N'; } }
        public DateTime LineupStartDate { get; set; }
        public int LineupNumberOfWeeks { get { return 1; } }
        public DateTime LineupStartTime { get; set; }
        public int Duration { get { return 3; } }
        public int MOP { get { return 1; } }
        public string CVT { get { return "SUN"; } }
        public int LineupMultiBarter { get { return 1; } }
        public char LineupCashOnly { get { return 'N'; } }
        public int LineupAiringNumber { get { return 1; } }
        public int AdvertiserId { get; set; }
        public int SpotLengthId { get; set; }
        public DateTime AirDate { get; set; }
    }
}
