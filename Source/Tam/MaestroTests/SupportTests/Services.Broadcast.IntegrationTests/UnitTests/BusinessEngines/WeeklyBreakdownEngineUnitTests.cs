using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [TestFixture]
    [Category("short_running")]
    public class WeeklyBreakdownEngineUnitTests
    {
        private readonly WeeklyBreakdownEngine _WeeklyBreakdownEngine = new WeeklyBreakdownEngine();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GroupsWeeklyBreakdownByWeek()
        {
            var result = _WeeklyBreakdownEngine.GroupWeeklyBreakdownByWeek(_WeeklyBreakdown);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsWeekNumberByMediaWeekDictionary()
        {
            var result = _WeeklyBreakdownEngine.GetWeekNumberByMediaWeekDictionary(_WeeklyBreakdown);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private IEnumerable<WeeklyBreakdownWeek> _WeeklyBreakdown = new List<WeeklyBreakdownWeek>
        {
            new WeeklyBreakdownWeek
            {
                WeekNumber = 3,
                MediaWeekId = 502,
                StartDate = new DateTime(2020, 5, 4),
                EndDate = new DateTime(2020, 5, 10),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 1000,
                WeeklyBudget = 100,
                SpotLengthId = 1
            },
            new WeeklyBreakdownWeek
            {
                WeekNumber = 3,
                MediaWeekId = 502,
                StartDate = new DateTime(2020, 5, 4),
                EndDate = new DateTime(2020, 5, 10),
                NumberOfActiveDays = 5,
                ActiveDays = "M,Tu,W,Th,F",
                WeeklyImpressions = 1500,
                WeeklyBudget = 200,
                SpotLengthId = 2
            },
            new WeeklyBreakdownWeek
            {
                WeekNumber = 2,
                MediaWeekId = 501,
                StartDate = new DateTime(2020, 4, 27),
                EndDate = new DateTime(2020, 5, 3),
                NumberOfActiveDays = 4,
                ActiveDays = "M,Tu,W,Th",
                WeeklyImpressions = 800,
                WeeklyBudget = 70,
                SpotLengthId = 1
            },
            new WeeklyBreakdownWeek
            {
                WeekNumber = 1,
                MediaWeekId = 500,
                StartDate = new DateTime(2020, 4, 20),
                EndDate = new DateTime(2020, 4, 26),
                NumberOfActiveDays = 3,
                ActiveDays = "M,Tu,W",
                WeeklyImpressions = 500,
                WeeklyBudget = 30,
                SpotLengthId = 1
            }
        };
    }
}
