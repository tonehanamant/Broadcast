using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.IntegrationTests.Stubbs;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Campaign;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class CampaignServiceUnitTests
    {
        private const string _CreatedBy = "UnitTest_User";
        private readonly DateTime _CreatedDate = new DateTime(2017, 10, 17, 7, 30, 23);
        private readonly Mock<ILockingManagerApplicationService> _LockingManagerApplicationServiceMock = new Mock<ILockingManagerApplicationService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsFilteredCampaigns()
        {
            // Arrange
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            var trafficApiCache = new TrafficApiCache(new TrafficApiClientStub());
            var agency = new AgencyDto { Id = 1, Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 };
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var getCampaignsReturn = new List<CampaignListItemDto>
            {
                new CampaignListItemDto
                {
                    Id = 1,
                    Name = "CampaignOne",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignOne.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17)
                },
                new CampaignListItemDto
                {
                    Id = 2,
                    Name = "CampaignTwo",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignTwo.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17)
                },
                new CampaignListItemDto
                {
                    Id = 3,
                    Name = "CampaignThree",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignThree.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17)
                }
            };

            campaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<PlanStatusEnum>())).Returns(_GetCampaignWithsummeriesReturn(agency, advertiser));
            quarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29), new DateTime(2009, 3, 29)));
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object, 
                campaignValidatorMock.Object, 
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                trafficApiCache);
            var serExpectedResult = IntegrationTestHelper.ConvertToJson(getCampaignsReturn);

            // Act
            var result = tc.GetCampaigns(new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Year = 2019,
                    Quarter = 2
                }, 
                PlanStatus = PlanStatusEnum.ClientApproval
            }, _CreatedDate);

            // Assert
            campaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<PlanStatusEnum>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsCampaignsFilteredUsingDefaultFilter()
        {
            // Arrange
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            var trafficApiCache = new TrafficApiCache(new TrafficApiClientStub());
            var agency = new AgencyDto { Id = 1, Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 };
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var getCampaignsReturn = new List<CampaignListItemDto>
            {
                new CampaignListItemDto
                {
                    Id = 1,
                    Name = "CampaignOne",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignOne.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17)
                },
                new CampaignListItemDto
                {
                    Id = 2,
                    Name = "CampaignTwo",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignTwo.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17)
                },
                new CampaignListItemDto
                {
                    Id = 3,
                    Name = "CampaignThree",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignThree.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17)
                }
            };
            
            campaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).Returns(_GetCampaignWithsummeriesReturn(agency, advertiser));
            quarterCalculationEngineMock.Setup(x => x.GetQuarterRangeByDate(_CreatedDate)).Returns(new QuarterDetailDto
            {
                Year = 2019,
                Quarter = 2
            });
            quarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29), new DateTime(2009, 3, 29)));
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                trafficApiCache);

            // Act
            var result = tc.GetCampaigns(null, _CreatedDate);

            // Assert
            campaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null), Times.Once);
            //campaignSummaryRepository.Verify(x => x.GetSummaryForCampaign(It.IsAny<int>()), Times.Exactly(3));
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private static List<CampaignWithSummary> _GetCampaignWithsummeriesReturn(AgencyDto agency, AdvertiserDto advertiser)
        {
            return new List<CampaignWithSummary>
            {
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 1,
                        Name = "CampaignOne",
                        AgencyId = agency.Id,
                        AdvertiserId = advertiser.Id,
                        Notes = "Notes for CampaignOne.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working
                    },
                },
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 2,
                        Name = "CampaignTwo",
                        AgencyId = agency.Id,
                        AdvertiserId = advertiser.Id,
                        Notes = "Notes for CampaignTwo.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working
                    }
                },
                new CampaignWithSummary
                {
                    Campaign = new CampaignDto
                    {
                        Id = 3,
                        Name = "CampaignThree",
                        AgencyId = agency.Id,
                        AdvertiserId = advertiser.Id,
                        Notes = "Notes for CampaignThree.",
                        ModifiedBy = "TestUser",
                        ModifiedDate = new DateTime(2017,10,17),
                        CampaignStatus = PlanStatusEnum.Working
                    }
                }
            };
        }

        private static List<CampaignListItemDto> _GetCampaignsReturn(AgencyDto agency, AdvertiserDto advertiser)
        {
            return new List<CampaignListItemDto>
            {
                new CampaignListItemDto
                {
                    Id = 1,
                    Name = "CampaignOne",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignOne.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17),
                    CampaignStatus = PlanStatusEnum.Working
                },
                new CampaignListItemDto
                {
                    Id = 2,
                    Name = "CampaignTwo",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignTwo.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17),
                    CampaignStatus = PlanStatusEnum.Working
                },
                new CampaignListItemDto
                {
                    Id = 3,
                    Name = "CampaignThree",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignThree.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17),
                    CampaignStatus = PlanStatusEnum.Working
                }
            };
        }

        [Test]
        public void ThrowsException_WhenCanNotGetAllCampaignsFromRepository()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from GetAllCampaigns";

            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var trafficApiCache = new Mock<ITrafficApiCache>();
            quarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(It.IsAny<int?>(), It.IsAny<int?>())).Returns(new DateRange(null, null));
            quarterCalculationEngineMock.Setup(x => x.GetQuarterRangeByDate(_CreatedDate)).Returns(new QuarterDetailDto());
            campaignRepositoryMock
                .Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), null))
                .Callback(() => throw new Exception(expectedMessage));
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                trafficApiCache.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetCampaigns(It.IsAny<CampaignFilterDto>(), _CreatedDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }
        
        [Test]
        public void ValidatesCampaign_WhenCreatingCampaign()
        {
            // Arrange
            const int campaignId = 0;
            const string campaignName = "CampaignOne";
            const string campaignNotes = "Notes for CampaignOne.";
            const int advertiserId = 1;
            const int agencyId = 1;
            var campaign = new SaveCampaignDto
            {
                Id = campaignId,
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId,
                Notes = campaignNotes
            };
            
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var campaignAggregationJobTrigger = new Mock<ICampaignAggregationJobTrigger>();
            var campaignSummaryRepo = new Mock<ICampaignSummaryRepository>();
            var trafficApiCache = new Mock<ITrafficApiCache>();
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepo.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                campaignAggregationJobTrigger.Object,
                trafficApiCache.Object);

            // Act
            tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate);

            // Assert
            campaignValidatorMock.Verify(x => x.Validate(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId)), Times.Once);
        }

        [Test]
        public void CreatesCampaign()
        {
            // Arrange
            const int campaignId = 0;
            const string campaignName = "CampaignOne";
            const string campaignNotes = "Notes for CampaignOne.";
            const int advertiserId = 1;
            const int agencyId = 1;
            var campaign = new SaveCampaignDto
            {
                Id = campaignId,
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId,
                Notes = campaignNotes
            };

            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var campaignSummaryRepo = new Mock<ICampaignSummaryRepository>();
            var campaignAggregationJobTrigger = new Mock<ICampaignAggregationJobTrigger>();
            var trafficApiCache = new Mock<ITrafficApiCache>();

            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepo.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                campaignAggregationJobTrigger.Object,
                trafficApiCache.Object);

            // Act
            tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate);

            // Assert
            campaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId),
                _CreatedBy,
                _CreatedDate), Times.Once);
        }

        [Test]
        public void ThrowsException_WhenCreatingInvalidCampaign()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from Validate.";
            const int campaignId = 0;
            const string campaignName = "CampaignOne";
            const string campaignNotes = "Notes for CampaignOne.";
            const int advertiserId = 1;
            const int agencyId = 1;
            var campaign = new SaveCampaignDto
            {
                Id = campaignId,
                Name = campaignName,
                AdvertiserId = advertiserId,
                AgencyId = agencyId,
                Notes = campaignNotes
            };

            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var trafficApiCache = new Mock<ITrafficApiCache>();

            campaignValidatorMock
                .Setup(s => s.Validate(It.IsAny<SaveCampaignDto>()))
                .Callback<SaveCampaignDto>(x => throw new Exception(expectedMessage));

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                trafficApiCache.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate));

            // Assert
            campaignValidatorMock.Verify(x => x.Validate(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId)), Times.Once);
            campaignRepositoryMock.Verify(x => x.CreateCampaign(It.IsAny<SaveCampaignDto>(), _CreatedBy, _CreatedDate), Times.Never);
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_WhenCanNotCreateCampaign()
        {
            // Arrange
            const string expectedMessage = "This is a test exception thrown from CreateCampaign.";
            const int campaignId = 0;
            const string campaignName = "CampaignOne";
            const string campaignNotes = "Notes for CampaignOne.";
            const int advertiserId = 1;
            const int agencyId = 1;
            var campaign = new SaveCampaignDto
            {
                Id = campaignId,
                Name = campaignName,
                AdvertiserId =advertiserId,
                AgencyId = agencyId,
                Notes = campaignNotes
            };

            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var trafficApiCache = new Mock<ITrafficApiCache>();

            campaignRepositoryMock
                .Setup(s => s.CreateCampaign(It.IsAny<SaveCampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => throw new Exception(expectedMessage));
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                trafficApiCache.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate));

            // Assert
            campaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId),
                _CreatedBy,
                _CreatedDate), Times.Once);
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void GetQuarters()
        {
            // Arrange
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaAggregateCache = IntegrationTestApplicationServiceFactory.MediaMonthAndWeekAggregateCache;
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var trafficApiCache = new Mock<ITrafficApiCache>();
            var getCampaignsDateRangesReturn = new List<DateRange>
            {
                new DateRange(null, null),
                new DateRange(new DateTime(2019, 1, 1), null),
                new DateRange(new DateTime(2019, 2, 1), new DateTime(2019, 9, 1))
            };
            
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            campaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(null)).Returns(getCampaignsDateRangesReturn);
            
            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaAggregateCache,
                quarterCalculationEngine,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                trafficApiCache.Object);

            // Act
            var campaignQuarters = tc.GetQuarters(null, new DateTime(2019, 8, 20));

            // Assert
            Assert.IsNotNull(campaignQuarters);
            Assert.AreEqual(3, campaignQuarters.Quarters.Count);
            Assert.AreEqual(3, campaignQuarters.DefaultQuarter.Quarter);
            Assert.AreEqual(2019, campaignQuarters.DefaultQuarter.Year);
        }

        [Test]
        public void GetQuartersDefaultQuarter()
        {
            // Arrange
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaAggregateCache = IntegrationTestApplicationServiceFactory.MediaMonthAndWeekAggregateCache;
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var getCampaignsDateRangesReturn = new List<DateRange>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var trafficApiCache = new Mock<ITrafficApiCache>();
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            campaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(null)).Returns(getCampaignsDateRangesReturn);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaAggregateCache,
                quarterCalculationEngine,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                trafficApiCache.Object);

            // Act
            var campaignQuarters = tc.GetQuarters(null, new DateTime(2019, 8, 20));

            // Assert
            Assert.IsNotNull(campaignQuarters);
            Assert.AreEqual(1, campaignQuarters.Quarters.Count);
            Assert.AreEqual(3, campaignQuarters.DefaultQuarter.Quarter);
            Assert.AreEqual(2019, campaignQuarters.DefaultQuarter.Year);
        }

        [Test]
        public void SaveDoesNotTriggersAggregation()
        {
            const int newCampaignId = 1;
            var campaign = new SaveCampaignDto
            {
                Id = 0,
                Name = "CampaignOne",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaAggregateCache = IntegrationTestApplicationServiceFactory.MediaMonthAndWeekAggregateCache;
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var getCampaignsDateRangesReturn = new List<DateRange>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var campaignSummaryRepository = new Mock<ICampaignSummaryRepository>();
            var trafficApiCache = new Mock<ITrafficApiCache>();
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepository.Object);
            campaignRepositoryMock
                .Setup(x => x.CreateCampaign(It.IsAny<SaveCampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(newCampaignId);
            campaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(It.IsAny<PlanStatusEnum?>())).Returns(getCampaignsDateRangesReturn);
            campaignSummaryRepository.Setup(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>())).Returns(1);
            var campaignAggJobTrigger = new Mock<ICampaignAggregationJobTrigger>();

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaAggregateCache,
                quarterCalculationEngine,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                campaignAggJobTrigger.Object,
                trafficApiCache.Object);

            tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate);

            campaignAggJobTrigger.Verify(s => s.TriggerJob(newCampaignId, _CreatedBy), Times.Never);
        }

        [Test]
        public void TriggerCampaignAggregation()
        {
            const int campaignId = 1;
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaAggregateCache = IntegrationTestApplicationServiceFactory.MediaMonthAndWeekAggregateCache;
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var campaignAggJobTrigger = new Mock<ICampaignAggregationJobTrigger>();
            var trafficApiCache = new Mock<ITrafficApiCache>();
            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaAggregateCache,
                quarterCalculationEngine,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                campaignAggJobTrigger.Object,
                trafficApiCache.Object);

            tc.TriggerCampaignAggregationJob(campaignId, _CreatedBy);

            campaignAggJobTrigger.Verify(s => s.TriggerJob(campaignId, _CreatedBy), Times.Once);
        }

        [Test]
        public void ProcessAggregation()
        {
            const int campaignId = 1;
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaAggregateCache = IntegrationTestApplicationServiceFactory.MediaMonthAndWeekAggregateCache;
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var getCampaignsDateRangesReturn = new List<DateRange>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var campaignSummaryRepository = new Mock<ICampaignSummaryRepository>();
            var trafficApiCache = new Mock<ITrafficApiCache>();
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepository.Object);
            campaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(It.IsAny<PlanStatusEnum?>())).Returns(getCampaignsDateRangesReturn);
            campaignAggregator.Setup(s => s.Aggregate(It.IsAny<int>()))
                .Returns(new CampaignSummaryDto());
            campaignSummaryRepository.Setup(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()))
                .Returns(1);
            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaAggregateCache,
                quarterCalculationEngine,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                trafficApiCache.Object);

            tc.ProcessCampaignAggregation(campaignId);
            
            campaignAggregator.Verify(s => s.Aggregate(campaignId), Times.Once);
            campaignSummaryRepository.Verify(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()), Times.Once);
            campaignSummaryRepository.Verify(s => s.SetSummaryProcessingStatusToError(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ProcessAggregation_WithError()
        {
            const int campaignId = 1;
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaAggregateCache = IntegrationTestApplicationServiceFactory.MediaMonthAndWeekAggregateCache;
            var quarterCalculationEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IQuarterCalculationEngine>();
            var getCampaignsDateRangesReturn = new List<DateRange>();
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var campaignSummaryRepository = new Mock<ICampaignSummaryRepository>();
            var trafficApiCache = new Mock<ITrafficApiCache>();
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepository.Object);
            campaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(It.IsAny<PlanStatusEnum?>())).Returns(getCampaignsDateRangesReturn);
            campaignAggregator.Setup(s => s.Aggregate(It.IsAny<int>()))
                .Returns(new CampaignSummaryDto());
            campaignSummaryRepository.Setup(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()))
                .Throws(new Exception("Error from SaveSummary"));
            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaAggregateCache,
                quarterCalculationEngine,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                trafficApiCache.Object);

            var caught = Assert.Throws<Exception>(() => tc.ProcessCampaignAggregation(campaignId));

            Assert.AreEqual("Error from SaveSummary", caught.Message);
            campaignAggregator.Verify(s => s.Aggregate(campaignId), Times.Once);
            campaignSummaryRepository.Verify(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()), Times.Once);
            campaignSummaryRepository.Verify(s => s.SetSummaryProcessingStatusToError(campaignId, $"Exception caught during processing : Error from SaveSummary"), Times.Once);
        }
    }
}
