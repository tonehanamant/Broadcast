﻿using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class SpotExceptionServiceUnitTests
    {
        private SpotExceptionService _SpotExceptionService;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ISpotExceptionRepository> _SpotExceptionRepositoryMock;
        private Mock<IFeatureToggleHelper> _FeatureToggleMock;
        private Mock<IConfigurationSettingsHelper> _ConfigurationSettingsHelperMock;

        [SetUp]
        public void SetUp()
        {
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _SpotExceptionRepositoryMock = new Mock<ISpotExceptionRepository>();
            _FeatureToggleMock = new Mock<IFeatureToggleHelper>();
            _ConfigurationSettingsHelperMock = new Mock<IConfigurationSettingsHelper>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ISpotExceptionRepository>())
                .Returns(_SpotExceptionRepositoryMock.Object);

            _SpotExceptionService = new SpotExceptionService(_DataRepositoryFactoryMock.Object, _FeatureToggleMock.Object, _ConfigurationSettingsHelperMock.Object);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlans_RecommendedPlan_DoesNotExist()
        {
            // Arrange
            var spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<SpotExceptionsRecommendedPlansDto>());

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlans_RecommendedPlans_Exist()
        {
            // Arrange
            var spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<SpotExceptionsRecommendedPlansDto>
                {
                    new SpotExceptionsRecommendedPlansDto
                    {
                        Id = 2,
                        EstimateId =191757,
                        IsciName = "BB82TXT4P",
                        RecommendedPlanId = 11725,
                        RecommendedPlanName = "2Q' 21 Reynolds Foil TDN and SYN Upfront",
                        ProgramName = "FOX 13 10:00 News",
                        ProgramAirTime = new DateTime(2020,1,4,8,7,15),
                        StationLegacyCallLetters = "KSTP",
                        Affiliate = "ABC",
                        Market = "Lincoln & Hastings-Krny",
                        Cost = 700,
                        Impressions = 1000,
                        SpotLengthId = 14,
                        SpotLengthString ="15",
                        AudienceId = 425,
                        AudienceName = "Men 18-24",
                        Product = "Spotify",
                        FlightStartDate = new DateTime(2019, 12, 1),
                        FlightEndDate = new DateTime(2020, 2, 1),
                        DaypartId = 71646,
                        IngestedAt = new DateTime(2019,1,1),
                        IngestedBy = "Repository Test User",
                        SpotExceptionsRecommendedPlanDecisionId = null
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        Id = 3,
                        EstimateId =191758,
                        IsciName = "CC42TXT4P",
                        RecommendedPlanId = 11726,
                        RecommendedPlanName = "4Q' 21 Reynolds Foil TDN and SYN Upfront",
                        ProgramName = "Reynolds Foil @9",
                        ProgramAirTime = new DateTime(2020,1,6,11,15,30),
                        StationLegacyCallLetters = "WDAY",
                        Affiliate = "NBC",
                        Market = "Phoenix (Prescott)",
                        Cost = 800,
                        Impressions = 1500,
                        SpotLengthId = 15,
                        SpotLengthString ="30",
                        AudienceId = 425,
                        AudienceName = "Women 18-24",
                        Product = "Spotify",
                        FlightStartDate = new DateTime(2019, 12, 1),
                        FlightEndDate = new DateTime(2020, 2, 1),
                        DaypartId = 71646,
                        IngestedAt = new DateTime(2019,1,1),
                        IngestedBy = "Repository Test User",
                        SpotExceptionsRecommendedPlanDecisionId = 1
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        Id = 4,
                        EstimateId =191760,
                        IsciName = "CC44ZZPT4",
                        RecommendedPlanId = 11728,
                        RecommendedPlanName = "3Q' 21 Reckitt HYHO Early Morning Upfront",
                        ProgramName = "Reckitt HYHO",
                        ProgramAirTime = new DateTime(2020,1,10,23,45,00),
                        StationLegacyCallLetters = "KXMC",
                        Affiliate = "CBS",
                        Market = "Minot-Bsmrck-Dcknsn(Wlstn)",
                        Cost = 450,
                        Impressions = 1752,
                        SpotLengthId = 16,
                        SpotLengthString ="45",
                        AudienceId = 426,
                        AudienceName = "Men 50-64",
                        Product = "Nike",
                        FlightStartDate = new DateTime(2019, 12, 1),
                        FlightEndDate = new DateTime(2020, 2, 1),
                        DaypartId = 71646,
                        IngestedAt = new DateTime(2019,1,1),
                        IngestedBy = "Repository Test User",
                        SpotExceptionsRecommendedPlanDecisionId = 1
                    }
                });

            // Act
            var result = _SpotExceptionService.GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlans_ThrowsException()
        {
            // Arrange
            var spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto
            {
                WeekStartDate = new DateTime(2021, 01, 04),
                WeekEndDate = new DateTime(2021, 01, 10)
            };

            _SpotExceptionRepositoryMock
                .Setup(s => s.GetSpotExceptionsRecommendedPlans(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback(() =>
                {
                    throw new Exception("Throwing a test exception.");
                });

            // Act
            var result = Assert.Throws<Exception>(() => _SpotExceptionService.GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest));

            // Assert
            Assert.AreEqual("Throwing a test exception.", result.Message);
        }
    }
}