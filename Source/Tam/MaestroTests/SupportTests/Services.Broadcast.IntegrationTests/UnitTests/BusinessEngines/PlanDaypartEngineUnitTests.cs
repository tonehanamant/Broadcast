using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class PlanDaypartEngineUnitTests
    {
        [Test]
        [TestCase("News", 138, 350, 3)]
        [TestCase("Not News", 138, 350, 2)]
        public void GetDaypartCodeByGenreAndTimeRange_Test(string genre, int startTime, int endTime, int expectedDaypartCodeId)
        {
            // Arrange
            var planDayparts = new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartTypeId = DaypartTypeEnum.News,
                    StartTimeSeconds = 80,
                    EndTimeSeconds = 139,
                    DaypartCodeId = 1
                },
                new PlanDaypartDto
                {
                    DaypartTypeId = DaypartTypeEnum.News,
                    StartTimeSeconds = 140,
                    EndTimeSeconds = 215,
                    DaypartCodeId = 3
                },
                new PlanDaypartDto
                {
                    DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews,
                    StartTimeSeconds = 216,
                    EndTimeSeconds = 236,
                    DaypartCodeId = 4
                },
                new PlanDaypartDto
                {
                    DaypartTypeId = DaypartTypeEnum.ROS,
                    StartTimeSeconds = 250,
                    EndTimeSeconds = 350,
                    DaypartCodeId = 2
                }
            };

            var tc = new PlanDaypartEngine();

            // Act
            var daypartCode = tc.FindPlanDaypartWithMostIntersectingTime(
                planDayparts,
                genre, 
                new TimeRange { StartTime = startTime, EndTime = endTime });

            // Assert
            Assert.AreEqual(expectedDaypartCodeId, daypartCode.DaypartCodeId);
        }
    }
}
