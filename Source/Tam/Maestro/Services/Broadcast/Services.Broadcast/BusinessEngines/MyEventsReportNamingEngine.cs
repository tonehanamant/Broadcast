using Common.Services.ApplicationServices;
using System;

namespace Services.Broadcast.BusinessEngines
{
    public interface IMyEventsReportNamingEngine : IApplicationService
    {
        string GetDefaultMyEventsReportName(string daypartCode, int spotLength, DateTime weekStart, string advertiser);
    }

    public class MyEventsReportNamingEngine : IMyEventsReportNamingEngine
    {
        public string GetDefaultMyEventsReportName(string daypartCode, int spotLength, DateTime weekStart, string advertiser)
        {
            const int MyEventsReportNameMaxLength = 25;
            var partialReportName = $" {daypartCode} {spotLength} {weekStart:MM-dd-yy}";
            var partialReportNameLength = partialReportName.Length;
            var remainingLength = MyEventsReportNameMaxLength - partialReportNameLength;

            if (advertiser.Length + partialReportNameLength > MyEventsReportNameMaxLength)
                return advertiser.Substring(0, remainingLength) + partialReportName;

            return advertiser + partialReportName;
        }
    }
}
