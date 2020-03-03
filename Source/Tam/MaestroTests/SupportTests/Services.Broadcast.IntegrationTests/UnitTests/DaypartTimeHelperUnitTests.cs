using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Helpers;
using System;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class DaypartTimeHelperUnitTests
    {
        [Test]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds), 400, 401)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.StartTimeSeconds), 400, 400)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(DaypartDefaultFullDto), nameof(DaypartDefaultFullDto.DefaultEndTimeSeconds), 400, 401)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(DaypartDefaultFullDto), nameof(DaypartDefaultFullDto.DefaultStartTimeSeconds), 400, 400)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds), 400, 399)]
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
    }
}