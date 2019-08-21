using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class DaypartTimeHelperUnitTests
    {
        [Test]
        public void AddOneSecondToEndTime_WithDaypartCodeDefaultDto()
        {
            var candidates = new List<DaypartCodeDefaultDto>
            {
                new DaypartCodeDefaultDto
                {
                    Id = 1, Code = "DP1", FullName = "Daypart1", DaypartType = DaypartTypeEnum.EntertainmentNonNews,
                    DefaultStartTimeSeconds = 300, DefaultEndTimeSeconds = 400
                },
                new DaypartCodeDefaultDto
                {
                    Id = 2, Code = "DP2", FullName = "Daypart2", DaypartType = DaypartTypeEnum.EntertainmentNonNews,
                    DefaultStartTimeSeconds = 300, DefaultEndTimeSeconds = 400
                },
            };

            DaypartTimeHelper.AddOneSecondToEndTime(candidates);

            Assert.AreEqual(2, candidates.Count);
            Assert.AreEqual(300, candidates[0].DefaultStartTimeSeconds);
            Assert.AreEqual(401, candidates[0].DefaultEndTimeSeconds);
            Assert.AreEqual(300, candidates[1].DefaultStartTimeSeconds);
            Assert.AreEqual(401, candidates[1].DefaultEndTimeSeconds);
        }

        [Test]
        public void AddOneSecondToEndTime_WithPlanDaypartDto()
        {
            var candidates = new List<PlanDaypartDto>
            {
                new PlanDaypartDto {DaypartCodeId = 1, StartTimeSeconds = 300, EndTimeSeconds = 400},
                new PlanDaypartDto {DaypartCodeId = 2, StartTimeSeconds = 300, EndTimeSeconds = 400},
            };

            DaypartTimeHelper.AddOneSecondToEndTime(candidates);

            Assert.AreEqual(2, candidates.Count);
            Assert.AreEqual(300, candidates[0].StartTimeSeconds);
            Assert.AreEqual(401, candidates[0].EndTimeSeconds);
            Assert.AreEqual(300, candidates[1].StartTimeSeconds);
            Assert.AreEqual(401, candidates[1].EndTimeSeconds);
        }

        [Test]
        public void AddOneSecondToEndTime_WithInvalid()
        {
            var candidates = new List<PlanDto>
            {
                new PlanDto(),
                new PlanDto()
            };

            var caught = Assert.Throws<InvalidOperationException>(() => DaypartTimeHelper.AddOneSecondToEndTime(candidates));
            Assert.AreEqual(caught.Message, "Invalid type provided in list.");
        }

        [Test]
        public void SubtractOneSecondToEndTime_WithDaypartCodeDefaultDto()
        {
            var candidates = new List<DaypartCodeDefaultDto>
            {
                new DaypartCodeDefaultDto
                {
                    Id = 1, Code = "DP1", FullName = "Daypart1", DaypartType = DaypartTypeEnum.EntertainmentNonNews,
                    DefaultStartTimeSeconds = 300, DefaultEndTimeSeconds = 400
                },
                new DaypartCodeDefaultDto
                {
                    Id = 2, Code = "DP2", FullName = "Daypart2", DaypartType = DaypartTypeEnum.EntertainmentNonNews,
                    DefaultStartTimeSeconds = 300, DefaultEndTimeSeconds = 400
                },
            };

            DaypartTimeHelper.SubtractOneSecondToEndTime(candidates);

            Assert.AreEqual(2, candidates.Count);
            Assert.AreEqual(300, candidates[0].DefaultStartTimeSeconds);
            Assert.AreEqual(399, candidates[0].DefaultEndTimeSeconds);
            Assert.AreEqual(300, candidates[1].DefaultStartTimeSeconds);
            Assert.AreEqual(399, candidates[1].DefaultEndTimeSeconds);
        }

        [Test]
        public void SubtractOneSecondToEndTime_WithPlanDaypartDto()
        {
            var candidates = new List<PlanDaypartDto>
            {
                new PlanDaypartDto {DaypartCodeId = 1, StartTimeSeconds = 300, EndTimeSeconds = 400},
                new PlanDaypartDto {DaypartCodeId = 2, StartTimeSeconds = 300, EndTimeSeconds = 400},
            };

            DaypartTimeHelper.SubtractOneSecondToEndTime(candidates);

            Assert.AreEqual(2, candidates.Count);
            Assert.AreEqual(300, candidates[0].StartTimeSeconds);
            Assert.AreEqual(399, candidates[0].EndTimeSeconds);
            Assert.AreEqual(300, candidates[1].StartTimeSeconds);
            Assert.AreEqual(399, candidates[1].EndTimeSeconds);
        }

        [Test]
        public void SubtractOneSecondToEndTime_WithInvalid()
        {
            var candidates = new List<PlanDto>
            {
                new PlanDto(),
                new PlanDto()
            };

            var caught = Assert.Throws<InvalidOperationException>(() => DaypartTimeHelper.SubtractOneSecondToEndTime(candidates));
            Assert.AreEqual(caught.Message, "Invalid type provided in list.");
        }
    }
}