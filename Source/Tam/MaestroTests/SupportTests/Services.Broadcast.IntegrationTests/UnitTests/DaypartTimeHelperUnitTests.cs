using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Helpers;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class DaypartTimeHelperUnitTests
    {
        [Test]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds), 400, 401)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.StartTimeSeconds), 400, 400)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(DaypartCodeDefaultDto), nameof(DaypartCodeDefaultDto.DefaultEndTimeSeconds), 400, 401)]
        [TestCase(nameof(DaypartTimeHelper.AddOneSecondToEndTime), typeof(DaypartCodeDefaultDto), nameof(DaypartCodeDefaultDto.DefaultStartTimeSeconds), 400, 400)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.EndTimeSeconds), 400, 399)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(PlanDaypartDto), nameof(PlanDaypartDto.StartTimeSeconds), 400, 400)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(DaypartCodeDefaultDto), nameof(DaypartCodeDefaultDto.DefaultEndTimeSeconds), 400, 399)]
        [TestCase(nameof(DaypartTimeHelper.SubtractOneSecondToEndTime), typeof(DaypartCodeDefaultDto), nameof(DaypartCodeDefaultDto.DefaultStartTimeSeconds), 400, 400)]
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
    }
}