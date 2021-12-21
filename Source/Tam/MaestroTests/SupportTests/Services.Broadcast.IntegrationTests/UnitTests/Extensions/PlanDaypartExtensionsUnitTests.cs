using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using System.Collections.Generic;
using static Services.Broadcast.Entities.Plan.PlanCustomDaypartDto;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class PlanDaypartExtensionsUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OrderDaypartDatas()
        {
            var dayparts = new List<DaypartData>
            {
                new DaypartData{ DaypartCode = "EMN", FlightDays = "W-F", StartTime = "8:00", EndTime= "9:00"},
                new DaypartData{ DaypartCode = "EMN", FlightDays = "F", StartTime = "8:00", EndTime= "9:00"},
                new DaypartData{ DaypartCode = "EMN", FlightDays = "M-F", StartTime = "11:00", EndTime= "12:00"},
                new DaypartData{ DaypartCode = "EMN", FlightDays = "M-F", StartTime = "11:00", EndTime= "11:30"},
                new DaypartData{ DaypartCode = "AMN", FlightDays = "M-Su", StartTime = "8:00", EndTime= "9:00"},
            };

            var result = dayparts.OrderDayparts();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OrderDaypartDataswithCustomDaypart()
        {
            var dayparts = new List<DaypartData>
            {
                new DaypartData{ DaypartCode = "EMN", FlightDays = "W-F", StartTime = "8:00", EndTime= "9:00"},
                new DaypartData{ DaypartCode = "EMN", FlightDays = "F", StartTime = "8:00", EndTime= "9:00"},
                new DaypartData{ DaypartCode = "EMN", FlightDays = "M-F", StartTime = "11:00", EndTime= "12:00"},
                new DaypartData{ DaypartCode = "EMN", FlightDays = "M-F", StartTime = "11:00", EndTime= "11:30"},
                new DaypartData{ DaypartCode = "CSP",CustomDayartOrganizationName = "NBA", CustomDaypartName="Knicks Vs Bulls",FlightDays = "M-Su", StartTime = "8:00", EndTime= "9:00"},
            };

            var result = dayparts.OrderDayparts();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void OrderPlanDaypartsCorrectly()
        {
            var planDayparts = new List<PlanDaypart>
            {
                new PlanDaypart { DaypartCodeId = 1, StartTimeSeconds = 0, EndTimeSeconds = 2000, FlightDays = new List<int> { 1 }, WeekdaysWeighting = 60, WeekendWeighting = 40 },
                new PlanDaypart { DaypartCodeId = 2, StartTimeSeconds = 1500, EndTimeSeconds = 2788, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 3, StartTimeSeconds = 2788, EndTimeSeconds = 3500, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 3500, EndTimeSeconds = 3600, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 1500, EndTimeSeconds = 3500, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 1499, EndTimeSeconds = 2788, FlightDays = new List<int> { 3 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 1500, EndTimeSeconds = 2788, FlightDays = new List<int> { 2 } }
            };

            var standardDayparts = new List<StandardDaypartDto>
            {
                new StandardDaypartDto { Id = 1, Code = "OVN" },
                new StandardDaypartDto { Id = 2, Code = "EF" },
                new StandardDaypartDto { Id = 3, Code = "LF" },
                new StandardDaypartDto { Id = 4, Code = "PA" },
            };

            var orderedPlanDayparts = planDayparts.OrderDayparts(standardDayparts);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(orderedPlanDayparts));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SortPlanDayparts()
        {
            var planDayparts = new List<PlanDaypart>
            {
                new PlanDaypart { DaypartCodeId = 1, StartTimeSeconds = 0, EndTimeSeconds = 2000, FlightDays = new List<int> { 1 }, WeekdaysWeighting = 60, WeekendWeighting = 40 },
                new PlanDaypart { DaypartCodeId = 2, StartTimeSeconds = 1500, EndTimeSeconds = 2788, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 3, StartTimeSeconds = 2788, EndTimeSeconds = 3500, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 3500, EndTimeSeconds = 3600, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 1500, EndTimeSeconds = 3500, FlightDays = new List<int> { 1 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 1499, EndTimeSeconds = 2788, FlightDays = new List<int> { 3 } },
                new PlanDaypart { DaypartCodeId = 4, StartTimeSeconds = 1500, EndTimeSeconds = 2788, FlightDays = new List<int> { 2 } },
                 new PlanDaypart { DaypartCodeId = 24,DaypartOrganizationId = 7,CustomName = "sportsnhl",DaypartOrganizationName = "NHL", StartTimeSeconds = 1500, EndTimeSeconds = 2788, FlightDays = new List<int> { 2 } }
            };

            var standardDayparts = new List<StandardDaypartDto>
            {
                new StandardDaypartDto { Id = 1, Code = "OVN" },
                new StandardDaypartDto { Id = 2, Code = "EF" },
                new StandardDaypartDto { Id = 3, Code = "LF" },
                new StandardDaypartDto { Id = 4, Code = "PA" },
                new StandardDaypartDto { Id = 24, Code = "OVN" },
            };

            var orderedPlanDayparts = planDayparts.OrderDayparts(standardDayparts);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(orderedPlanDayparts));
        }
    }
}
