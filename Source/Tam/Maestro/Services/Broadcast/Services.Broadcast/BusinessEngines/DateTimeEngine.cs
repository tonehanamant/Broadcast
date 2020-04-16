using Common.Services.ApplicationServices;
using System;

namespace Services.Broadcast.BusinessEngines
{
    public interface IDateTimeEngine : IApplicationService
    {
        DateTime GetCurrentMoment();
    }

    public class DateTimeEngine : IDateTimeEngine
    {
        public DateTime GetCurrentMoment()
        {
            return DateTime.Now;
        }
    }
}
