using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Helpers;
using System;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    [Category("short_running")]
    public class DaypartTimeHelperUnitTests
    {
        [Test]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds), 400, 401)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds), BroadcastConstants.OneDayInSeconds - 1, 0)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds), 0, 1)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.StartTimeSeconds), 400, 400)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(DaypartDefaultFullDto), nameof(DaypartDefaultFullDto.DefaultEndTimeSeconds), 400, 401)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(DaypartDefaultFullDto), nameof(DaypartDefaultFullDto.DefaultStartTimeSeconds), 400, 400)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds), 400, 399)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds), 0, BroadcastConstants.OneDayInSeconds - 1)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds), BroadcastConstants.OneDayInSeconds - 1, BroadcastConstants.OneDayInSeconds - 2)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.StartTimeSeconds), 400, 400)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(DaypartDefaultFullDto), nameof(DaypartDefaultFullDto.DefaultEndTimeSeconds), 400, 399)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(DaypartDefaultFullDto), nameof(DaypartDefaultFullDto.DefaultStartTimeSeconds), 400, 400)]
        public void InvokeMethodAndCheckProperty(string methodName, Type t, string propertyName, int testValue, int expectedValue)
        {
            var candidates = ReflectionTestHelper.CreateGenericList(t);
            candidates.Add(ReflectionTestHelper.CreateInstanceAndSetProperty(t, propertyName, testValue));
            candidates.Add(ReflectionTestHelper.CreateInstanceAndSetProperty(t, propertyName, testValue));
            var genericMethod = ReflectionTestHelper.GetGenericMethod(t, typeof(DaypartTimeHelper), methodName);

            genericMethod.Invoke(null, new object[] { candidates });

            Assert.AreEqual(2, candidates.Count);
            Assert.AreEqual(expectedValue, (int)t.GetProperty(propertyName).GetValue(candidates[0]));
            Assert.AreEqual(expectedValue, (int)t.GetProperty(propertyName).GetValue(candidates[1]));
        }

        [Test]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime))]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime))]
        public void AddOrSubtractWithInvalid(string methodName)
        {
            Type t = typeof(object);
            var candidates = ReflectionTestHelper.CreateGenericList(t);
            var genericMethod = ReflectionTestHelper.GetGenericMethod(t, typeof(DaypartTimeHelper), methodName);
            Exception caught = null;

            try
            {
                genericMethod.Invoke(null, new object[] {candidates});
            }
            catch (Exception e)
            {
                caught = e;
            }

            Assert.IsNotNull(caught);
            Assert.IsNotNull(caught.InnerException);
            Assert.IsTrue(caught.InnerException is InvalidOperationException);
            Assert.AreEqual(caught.InnerException.Message, "Invalid type provided in list.");
        }

        [Test]
        [TestCase(3600, 7199, 1d)]
        [TestCase(7200, 3599, 23d)]
        [TestCase(7200, 7199, 24d)]
        [TestCase(7200, 7200, 0.00027777777777777778d)]
        public void GetOneDayHoursTest(int startTime, int endTime, double expectedHours)
        {
            var daypart = new DisplayDaypart
            {
                Monday = true,
                Tuesday = true,
                StartTime = startTime,
                EndTime = endTime
            };

            Assert.AreEqual(expectedHours, daypart.GetDurationPerDayInHours());
        }

        [Test]
        // NO OVERNIGHTS:
        // 1. no intersections
        [TestCase(100, 199, 50, 99, 0)]
        [TestCase(100, 199, 200, 250, 0)]

        // 2. left intersection
        [TestCase(100, 199, 50, 100, 1)]
        [TestCase(100, 199, 50, 149, 50)]

        // 3. inner intersection
        [TestCase(100, 199, 100, 149, 50)]
        [TestCase(100, 199, 100, 199, 100)]

        // 4. right intersection
        [TestCase(100, 199, 100, 200, 100)]
        [TestCase(100, 199, 150, 200, 50)]
        [TestCase(100, 199, 199, 200, 1)]


        // FIRST RANGE HAS OVERNIGHTS:
        // 1. inner intersection
        [TestCase(100, 49, 200, 299, 100)]


        // SECOND RANGE HAS OVERNIGHTS:
        // 1. no intersections
        [TestCase(100, 199, 200, 99, 0)]

        // 2. left intersection
        [TestCase(100, 199, 200, 100, 1)]
        [TestCase(100, 199, 200, 199, 100)]
        [TestCase(100, 199, 205, 204, 100)]
        [TestCase(100, 199, 205, 198, 99)]

        // 3. right intersection
        [TestCase(100, 199, 180, 49, 20)]

        // 4. both left and right intersections
        [TestCase(100, 199, 180, 129, 50)]

        // BOTH RANGES HAVE OVERNIGHTS
        // 1. left intersection
        [TestCase(100, 49, 200, 48, BroadcastConstants.OneDayInSeconds - 200 + 48 + 1)]
        [TestCase(100, 49, 200, 49, BroadcastConstants.OneDayInSeconds - 200 + 49 + 1)]
        [TestCase(100, 49, 200, 50, BroadcastConstants.OneDayInSeconds - 200 + 49 + 1)]

        // VALUES THAT NEEDS TO BE ADJUSTED
        [TestCase(0, 99, 200, -BroadcastConstants.OneDayInSeconds, 1)]          // becomes 0
        [TestCase(0, 99, 200, -86399, 2)]                                       // becomes 1
        [TestCase(0, 99, 200, -86399 - BroadcastConstants.OneDayInSeconds, 2)]  // becomes 1
        [TestCase(0, 99, 200, BroadcastConstants.OneDayInSeconds, 1)]           // becomes 0
        [TestCase(0, 99, 200, BroadcastConstants.OneDayInSeconds + 1, 2)]       // becomes 1
        [TestCase(0, 99, 200, BroadcastConstants.OneDayInSeconds * 2 + 1, 2)]   // becomes 1
        public void GetIntersectingTotalTimeTest(
            int firstDaypartStartTime,
            int firstDaypartEndTime,
            int secondDaypartStartTime,
            int secondDaypartEndTime,
            int expected)
        {
            var firstDaypart = new TimeRange { StartTime = firstDaypartStartTime, EndTime = firstDaypartEndTime };
            var secondDaypart = new TimeRange { StartTime = secondDaypartStartTime, EndTime = secondDaypartEndTime };

            var actual = DaypartTimeHelper.GetIntersectingTotalTime(firstDaypart, secondDaypart);
            var actualReversed = DaypartTimeHelper.GetIntersectingTotalTime(secondDaypart, firstDaypart);

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actualReversed);
        }
    }
}