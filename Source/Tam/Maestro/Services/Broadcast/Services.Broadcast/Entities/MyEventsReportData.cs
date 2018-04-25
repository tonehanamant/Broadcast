using System;

namespace Services.Broadcast.Entities
{
    public class MyEventsReportData
    {
        private const int ReportableNameMaxLength = 25;

        public string ReportableName
        {
            get
            {
                var partialReportableName = $" {DaypartCode} {SpotLength} {ScheduleStartDate:MM-dd-yy}";
                var partialReportableNameLength = partialReportableName.Length;
                var remainingLength = ReportableNameMaxLength - partialReportableNameLength;
                var advertiser = Advertiser;

                if (Advertiser.Length + partialReportableNameLength > ReportableNameMaxLength)
                    advertiser = advertiser.Substring(0, remainingLength);

                return advertiser + partialReportableName;
            }
        }
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
        public string CallLetter { get; set; }
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
    }
}
