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

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class CampaignServiceUnitTests
    {
        private const string _CreatedBy = "TestUser";
        private readonly DateTime _CreatedDate = new DateTime(2017, 10, 17, 7, 30, 23);
        private readonly ITrafficApiClient _TrafficApiClient = new TrafficApiClientStub();
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
            var trafficApiClientMock = new Mock<ITrafficApiClient>();
            var agency = new AgencyDto { Id = 1, Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 };
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var campaignSummaryRepository = new Mock<ICampaignSummaryRepository>();
            var agencyCache = new Mock<IAgencyCache>();
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

            agencyCache.Setup(x => x.GetAgency(It.IsAny<int>())).Returns(agency);
            trafficApiClientMock.Setup(x => x.GetAdvertisersByAgencyId(It.IsAny<int>())).Returns(new List<AdvertiserDto> { advertiser });
            campaignRepositoryMock.Setup(x => x.GetCampaigns(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<PlanStatusEnum>())).Returns(getCampaignsReturn);
            campaignSummaryRepository.Setup(x => x.GetSummaryForCampaign(It.IsAny<int>())).Returns((CampaignSummaryDto) null);
            quarterCalculationEngineMock.Setup(x => x.GetQuarterDetail(2, 2019)).Returns(new QuarterDetailDto
            {
                Year = 2019,
                Quarter = 2
            });
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepository.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object, 
                campaignValidatorMock.Object, 
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                trafficApiClientMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                agencyCache.Object);
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
            campaignRepositoryMock.Verify(x => x.GetCampaigns(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<PlanStatusEnum>()), Times.Once);
            campaignSummaryRepository.Verify(x => x.GetSummaryForCampaign(It.IsAny<int>()), Times.Exactly(3));
            // TODO: Bring this back.  Fails on CD test run build.
            //Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            // TODO: When bring that back remove this 
            Assert.AreEqual(serExpectedResult, IntegrationTestHelper.ConvertToJson(result));
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
            var trafficApiClientMock = new Mock<ITrafficApiClient>();
            var agency = new AgencyDto { Id = 1, Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 };
            var campaignAggregator = new Mock<ICampaignAggregator>();
            var campaignSummaryRepository = new Mock<ICampaignSummaryRepository>();
            var agencyCache = new Mock<IAgencyCache>();
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

            agencyCache.Setup(x => x.GetAgency(It.IsAny<int>())).Returns(agency);
            trafficApiClientMock.Setup(x => x.GetAdvertisersByAgencyId(It.IsAny<int>())).Returns(new List<AdvertiserDto> { advertiser });
            campaignRepositoryMock.Setup(x => x.GetCampaigns(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).Returns(getCampaignsReturn);
            campaignSummaryRepository.Setup(x => x.GetSummaryForCampaign(It.IsAny<int>())).Returns((CampaignSummaryDto)null);
            quarterCalculationEngineMock.Setup(x => x.GetQuarterRangeByDate(_CreatedDate)).Returns(new QuarterDetailDto
            {
                Year = 2019,
                Quarter = 2
            });
            quarterCalculationEngineMock.Setup(x => x.GetQuarterDetail(2, 2019)).Returns(new QuarterDetailDto
            {
                Year = 2019,
                Quarter = 2
            });
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepository.Object);
            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                trafficApiClientMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                agencyCache.Object);
            var serExpectedResult = IntegrationTestHelper.ConvertToJson(getCampaignsReturn);

            // Act
            var result = tc.GetCampaigns(null, _CreatedDate);

            // Assert
            campaignRepositoryMock.Verify(x => x.GetCampaigns(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null), Times.Once);
            campaignSummaryRepository.Verify(x => x.GetSummaryForCampaign(It.IsAny<int>()), Times.Exactly(3));
            // TODO: Bring this back.  Fails on CD test run build.
            //Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            // TODO: When bring that back remove this 
            Assert.AreEqual(serExpectedResult, IntegrationTestHelper.ConvertToJson(result));
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
            var agencyCache = new Mock<IAgencyCache>();
            quarterCalculationEngineMock.Setup(x => x.GetQuarterDetail(It.IsAny<int>(), It.IsAny<int>())).Returns(new QuarterDetailDto());
            quarterCalculationEngineMock.Setup(x => x.GetQuarterRangeByDate(_CreatedDate)).Returns(new QuarterDetailDto());
            campaignRepositoryMock
                .Setup(x => x.GetCampaigns(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
                .Callback(() => throw new Exception(expectedMessage));
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                agencyCache.Object);

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
            var campaign = new CampaignDto
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
            var agencyCache = new Mock<IAgencyCache>();
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepo.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                campaignAggregationJobTrigger.Object,
                agencyCache.Object);

            // Act
            tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate);

            // Assert
            campaignValidatorMock.Verify(x => x.Validate(
                It.Is<CampaignDto>(c => c.Id == campaignId &&
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
            var campaign = new CampaignDto
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
            var agencyCache = new Mock<IAgencyCache>();

            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepo.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                campaignAggregationJobTrigger.Object,
                agencyCache.Object);

            // Act
            tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate);

            // Assert
            campaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<CampaignDto>(c => c.Id == campaignId &&
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
            var campaign = new CampaignDto
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
            var agencyCache = new Mock<IAgencyCache>();

            campaignValidatorMock
                .Setup(s => s.Validate(It.IsAny<CampaignDto>()))
                .Callback<CampaignDto>(x => throw new Exception(expectedMessage));

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                agencyCache.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate));

            // Assert
            campaignValidatorMock.Verify(x => x.Validate(
                It.Is<CampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId)), Times.Once);
            campaignRepositoryMock.Verify(x => x.CreateCampaign(It.IsAny<CampaignDto>(), _CreatedBy, _CreatedDate), Times.Never);
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
            var campaign = new CampaignDto
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
            var agencyCache = new Mock<IAgencyCache>();

            campaignRepositoryMock
                .Setup(s => s.CreateCampaign(It.IsAny<CampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => throw new Exception(expectedMessage));
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                agencyCache.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate));

            // Assert
            campaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<CampaignDto>(c => c.Id == campaignId &&
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
            var agencyCache = new Mock<IAgencyCache>();
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
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                agencyCache.Object);

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
            var agencyCache = new Mock<IAgencyCache>();
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            campaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(null)).Returns(getCampaignsDateRangesReturn);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaAggregateCache,
                quarterCalculationEngine,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                agencyCache.Object);

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
            var campaign = new CampaignDto
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
            var agencyCache = new Mock<IAgencyCache>();
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignSummaryRepository>()).Returns(campaignSummaryRepository.Object);
            campaignRepositoryMock
                .Setup(x => x.CreateCampaign(It.IsAny<CampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(newCampaignId);
            campaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(It.IsAny<PlanStatusEnum?>())).Returns(getCampaignsDateRangesReturn);
            campaignSummaryRepository.Setup(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>())).Returns(1);
            var campaignAggJobTrigger = new Mock<ICampaignAggregationJobTrigger>();

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaAggregateCache,
                quarterCalculationEngine,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                campaignAggJobTrigger.Object,
                agencyCache.Object);

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
            var agencyCache = new Mock<IAgencyCache>();
            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaAggregateCache,
                quarterCalculationEngine,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                campaignAggJobTrigger.Object,
                agencyCache.Object);

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
            var agencyCache = new Mock<IAgencyCache>();
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
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                agencyCache.Object);

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
            var agencyCache = new Mock<IAgencyCache>();
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
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object,
                campaignAggregator.Object,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                agencyCache.Object);

            var caught = Assert.Throws<Exception>(() => tc.ProcessCampaignAggregation(campaignId));

            Assert.AreEqual("Error from SaveSummary", caught.Message);
            campaignAggregator.Verify(s => s.Aggregate(campaignId), Times.Once);
            campaignSummaryRepository.Verify(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()), Times.Once);
            campaignSummaryRepository.Verify(s => s.SetSummaryProcessingStatusToError(campaignId, $"Exception caught during processing : Error from SaveSummary"), Times.Once);
        }
    }
}
