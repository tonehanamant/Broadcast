using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    public class StandartDaypartEngineUnitTests
    {
        private readonly Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private readonly Mock<IDaypartDefaultRepository> _DaypartDefaultRepositoryMock;

        public StandartDaypartEngineUnitTests()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _DaypartDefaultRepositoryMock = new Mock<IDaypartDefaultRepository>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IDaypartDefaultRepository>())
                .Returns(_DaypartDefaultRepositoryMock.Object);
        }

        [Test]
        [TestCase("News", 138, 350, "MDN")]
        [TestCase("Not News", 138, 350, "LN")]
        public void GetDaypartCodeByGenreAndTimeRange_Test(string genre, int startTime, int endTime, string expectedCode)
        {
            // Arrange
            _DaypartDefaultRepositoryMock
                .Setup(x => x.GetAllDaypartDefaultsWithAllData())
                .Returns(new List<DaypartDefaultFullDto>
                {
                    new DaypartDefaultFullDto
                    {
                        DaypartType = DaypartTypeEnum.News,
                        DefaultStartTimeSeconds = 80,
                        DefaultEndTimeSeconds = 139,
                        Code = "EMN"
                    },
                    new DaypartDefaultFullDto
                    {
                        DaypartType = DaypartTypeEnum.News,
                        DefaultStartTimeSeconds = 140,
                        DefaultEndTimeSeconds = 215,
                        Code = "MDN"
                    },
                    new DaypartDefaultFullDto
                    {
                        DaypartType = DaypartTypeEnum.EntertainmentNonNews,
                        DefaultStartTimeSeconds = 216,
                        DefaultEndTimeSeconds = 236,
                        Code = "EN"
                    },
                    new DaypartDefaultFullDto
                    {
                        DaypartType = DaypartTypeEnum.ROS,
                        DefaultStartTimeSeconds = 250,
                        DefaultEndTimeSeconds = 350,
                        Code = "LN"
                    }
                });

            var tc = new StandartDaypartEngine(_DataRepositoryFactoryMock.Object);

            // Act
            var daypartCode = tc.GetDaypartCodeByGenreAndTimeRange(genre, new TimeRange { StartTime = startTime, EndTime = endTime });

            // Assert
            Assert.AreEqual(expectedCode, daypartCode.Code);
        }
    }
}
