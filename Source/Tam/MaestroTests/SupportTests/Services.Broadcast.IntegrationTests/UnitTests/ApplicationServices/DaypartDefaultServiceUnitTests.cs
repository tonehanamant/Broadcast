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
    public class DaypartDefaultServiceUnitTests
    {
        private DaypartDefaultService _GetService(bool enableDaypartWKD = true)
        {
            // setup feature flags
            var launchDarklyClientStub = new LaunchDarklyClientStub();
            launchDarklyClientStub.FeatureToggles.Add(FeatureToggles.ENABLE_DAYPART_WKD, enableDaypartWKD);
            var featureToggleHelper = new FeatureToggleHelper(launchDarklyClientStub);

            // setup data repos
            var daypartDefaultRepository = new Mock<IDaypartDefaultRepository>();
            daypartDefaultRepository.Setup(s => s.GetAllDaypartDefaults())
                .Returns(DaypartsTestData.GetAllDaypartDefaultsWithBaseData);
            daypartDefaultRepository.Setup(s => s.GetAllDaypartDefaultsWithAllData())
                .Returns(_GetDeepCopy(DaypartsTestData.GetAllDaypartDefaultsWithFullData()));

            var repoFactory  = new Mock<IDataRepositoryFactory>();
            repoFactory.Setup(s => s.GetDataRepository<IDaypartDefaultRepository>())
                .Returns(daypartDefaultRepository.Object);
            
            // create the service for return
            var service = new DaypartDefaultService(repoFactory.Object, featureToggleHelper);
            return service;
        }

        private List<DaypartDefaultFullDto> _GetDeepCopy(List<DaypartDefaultFullDto> toCopy)
        {
            var copy = toCopy.Select(c => new DaypartDefaultFullDto
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
        public void GetAllDaypartDefaults()
        {
            // Arrange
            var service = _GetService();

            // Act
            var results = service.GetAllDaypartDefaults();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDaypartDefaultsEnableWKDOff()
        {
            // Arrange
            const bool enableDaypartWKD = false;
            var service = _GetService(enableDaypartWKD);

            // Act
            var results = service.GetAllDaypartDefaultsWithAllData();

            // Assert
            // daypart with code 'WKD' should be filtered out.
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDaypartDefaultsWithAllData()
        {
            // Arrange
            var service = _GetService();

            // Act
            var results = service.GetAllDaypartDefaultsWithAllData();

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllDaypartDefaultsWithAllDataEnableWKDOff()
        {
            // Arrange
            const bool enableDaypartWKD = false;
            var service = _GetService(enableDaypartWKD);

            // Act
            var results = service.GetAllDaypartDefaultsWithAllData();

            // Assert
            // daypart with code 'WKD' should be filtered out.
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }
    }
}