using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    public class StandardDaypartServiceUnitTests
    {
        private StandardDaypartService _GetService(bool enableDaypartWKD = true)
        {

            // setup data repos
            var standardDaypartRepository = new Mock<IStandardDaypartRepository>();
            standardDaypartRepository.Setup(s => s.GetAllStandardDayparts())
                .Returns(DaypartsTestData.GetAllStandardDaypartsWithBaseData);
            standardDaypartRepository.Setup(s => s.GetAllStandardDaypartsWithAllData())
                .Returns(_GetDeepCopy(DaypartsTestData.GetAllStandardDaypartsWithFullData()));

            var repoFactory  = new Mock<IDataRepositoryFactory>();
            repoFactory.Setup(s => s.GetDataRepository<IStandardDaypartRepository>())
                .Returns(standardDaypartRepository.Object);
            
            // create the service for return
            var service = new StandardDaypartService(repoFactory.Object);
            return service;
        }

        private List<StandardDaypartFullDto> _GetDeepCopy(List<StandardDaypartFullDto> toCopy)
        {
            var copy = toCopy.Select(c => new StandardDaypartFullDto
            {
                Id = c.Id,
                Code = c.Code,
                FullName = c.FullName,
                VpvhCalculationSourceType = c.VpvhCalculationSourceType,
                DaypartType = c.DaypartType,
                DefaultStartTimeSeconds = c.DefaultStartTimeSeconds,
                DefaultEndTimeSeconds = c.DefaultEndTimeSeconds
            }).ToList();
            return copy;
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllStandardDayparts()
        {
            // Arrange
            var service = _GetService();

            // Act
            var results = service.GetAllStandardDayparts();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllStandardDaypartsWithAllData()
        {
            // Arrange
            var service = _GetService();

            // Act
            var results = service.GetAllStandardDaypartsWithAllData();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }
    }
}