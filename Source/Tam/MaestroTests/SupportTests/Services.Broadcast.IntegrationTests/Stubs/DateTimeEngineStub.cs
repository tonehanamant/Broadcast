using System;
using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class DateTimeEngineStub : IDateTimeEngine
    {
        public DateTime? UT_CurrentMoment { get; set; }

        public DateTime GetCurrentMoment()
        {
            return UT_CurrentMoment ?? DateTime.Now;
        }
    }
}