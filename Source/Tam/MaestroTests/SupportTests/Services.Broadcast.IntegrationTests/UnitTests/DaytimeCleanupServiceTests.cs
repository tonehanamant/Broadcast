using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Maintenance;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]
    public class DaytimeCleanupServiceTests
    {
        private DaypartCleanupService _daypartCleanupService;
        private Mock<IDisplayDaypartRepository> _displayDaypartRespositoryMock;

        [SetUp]
        public void Init()
        {
            var mock = new Mock<IDisplayDaypartRepository>();
            var factory = new Mock<IDataRepositoryFactory>();
            factory.Setup(f => f.GetDataRepository<IDisplayDaypartRepository>()).Returns(mock.Object);
            _daypartCleanupService = new DaypartCleanupService(factory.Object);
        }

        [Test]
        [TestCase("W-F", new int[] { 3, 4, 5 })]
        [TestCase("SA-SU", new int[] { 6, 7 })]
        [TestCase("M-TH,SU", new int[] { 1, 2, 3, 4, 7 })]
        [TestCase("W-SU", new int[] { 3, 4, 5, 6, 7 })]
        [TestCase("M-TH,SA-SU", new int[] { 1, 2, 3, 4, 6, 7 })]
        public void ValidateDaypartText(string expectedResult, int[] daypartDays)
        {
            var result = GroupHelper.GroupWeekDays(new List<int>(daypartDays));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase("W-F", new int[] { 3, 4, 5 })]
        [TestCase("SA-SU", new int[] { 6, 7 })]
        [TestCase("M-TH,SU", new int[] { 1, 2, 3, 4, 7 })]
        [TestCase("W-SU", new int[] { 3, 4, 5, 6, 7 })]
        [TestCase("M-TH,SA-SU", new int[] { 1, 2, 3, 4, 6, 7 })]
        public void ValidateDaypartDaysByTextCalculator(string daypartDaysText, int[] expectedResult)
        {
            var result = _daypartCleanupService.CalculateDaypartDaysByText(daypartDaysText);

            Assert.AreEqual(expectedResult, result.ToArray());
        }

        [Test]
        [TestCase("11AM-9PM", 39600, 75600 )]
        [TestCase("8AM-10:30AM", 28800, 37800 )]
        [TestCase("9:30PM-10:15PM", 77400, 80100 )]
        public void ValidateDaypartTimespanText(string expectedResult, int startTime, int endTime)
        {
            var result = _daypartCleanupService.CalculateDaypartTimespanText(startTime, endTime);

            Assert.AreEqual(expectedResult, result);
        }
    }
}
