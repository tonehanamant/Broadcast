using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using IntegrationTests.Common;
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

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class CampaignServiceUnitTests
    {
        private const string _CreatedBy = "TestUser";
        private readonly DateTime _CreatedDate = new DateTime(2017, 10, 17, 7, 30, 23);
        private readonly ITrafficApiClient _TrafficApiClient = new TrafficApiClientStub();
        private readonly Mock<ILockingManagerApplicationService> _LockingManagerApplicationServiceMock = new Mock<ILockingManagerApplicationService>();

        [Test]
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
            var getCampaignsReturn = new List<CampaignDto>
            {
                new CampaignDto
                {
                    Id = 1,
                    Name = "CampaignOne",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignOne.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17)
                },
                new CampaignDto
                {
                    Id = 2,
                    Name = "CampaignTwo",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignTwo.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17)
                },
                new CampaignDto
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
            var expectedResult = IntegrationTestHelper.ConvertToJson(getCampaignsReturn);

            trafficApiClientMock.Setup(x => x.GetAgency(It.IsAny<int>())).Returns(agency);
            trafficApiClientMock.Setup(x => x.GetAdvertisersByAgencyId(It.IsAny<int>())).Returns(new List<AdvertiserDto> { advertiser });
            campaignRepositoryMock.Setup(x => x.GetCampaigns(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<PlanStatusEnum>())).Returns(getCampaignsReturn);
            quarterCalculationEngineMock.Setup(x => x.GetQuarterDetail(2, 2019)).Returns(new QuarterDetailDto
            {
                Year = 2019,
                Quarter = 2
            });
            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object, 
                campaignValidatorMock.Object, 
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                trafficApiClientMock.Object,
                _LockingManagerApplicationServiceMock.Object);

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
            Assert.AreEqual(expectedResult, IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
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
            var getCampaignsReturn = new List<CampaignDto>
            {
                new CampaignDto
                {
                    Id = 1,
                    Name = "CampaignOne",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignOne.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17)
                },
                new CampaignDto
                {
                    Id = 2,
                    Name = "CampaignTwo",
                    Agency = agency,
                    Advertiser = advertiser,
                    Notes = "Notes for CampaignTwo.",
                    ModifiedBy = "TestUser",
                    ModifiedDate = new DateTime(2017,10,17)
                },
                new CampaignDto
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
            var expectedResult = IntegrationTestHelper.ConvertToJson(getCampaignsReturn);

            trafficApiClientMock.Setup(x => x.GetAgency(It.IsAny<int>())).Returns(agency);
            trafficApiClientMock.Setup(x => x.GetAdvertisersByAgencyId(It.IsAny<int>())).Returns(new List<AdvertiserDto> { advertiser });
            campaignRepositoryMock.Setup(x => x.GetCampaigns(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).Returns(getCampaignsReturn);
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

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                trafficApiClientMock.Object,
                _LockingManagerApplicationServiceMock.Object);

            // Act
            var result = tc.GetCampaigns(null, _CreatedDate);

            // Assert
            campaignRepositoryMock.Verify(x => x.GetCampaigns(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null), Times.Once);
            var actualResult = IntegrationTestHelper.ConvertToJson(result);
            Assert.AreEqual(expectedResult, actualResult);
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
                _LockingManagerApplicationServiceMock.Object);

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
                Advertiser = new AdvertiserDto { Id = advertiserId },
                Agency = new AgencyDto { Id = agencyId },
                Notes = campaignNotes
            };
            
            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();

            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object);

            // Act
            tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate);

            // Assert
            campaignValidatorMock.Verify(x => x.Validate(
                It.Is<CampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.Advertiser.Id == advertiserId &&
                                        campaign.Agency.Id == agencyId)), Times.Once);
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
                Advertiser = new AdvertiserDto { Id = advertiserId },
                Agency = new AgencyDto { Id = agencyId },
                Notes = campaignNotes
            };

            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();

            dataRepositoryFactoryMock.Setup(s => s.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object);

            // Act
            tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate);

            // Assert
            campaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<CampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.Advertiser.Id == advertiserId &&
                                        campaign.Agency.Id == agencyId),
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
                Advertiser = new AdvertiserDto { Id = advertiserId },
                Agency = new AgencyDto { Id = agencyId },
                Notes = campaignNotes
            };

            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();

            campaignValidatorMock
                .Setup(s => s.Validate(It.IsAny<CampaignDto>()))
                .Callback<CampaignDto>(x => throw new Exception(expectedMessage));

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaMonthAndWeekAggregateCache.Object,
                quarterCalculationEngineMock.Object,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate));

            // Assert
            campaignValidatorMock.Verify(x => x.Validate(
                It.Is<CampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.Advertiser.Id == advertiserId &&
                                        campaign.Agency.Id == agencyId)), Times.Once);
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
                Advertiser = new AdvertiserDto { Id = advertiserId },
                Agency = new AgencyDto { Id = agencyId },
                Notes = campaignNotes
            };

            var dataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            var campaignRepositoryMock = new Mock<ICampaignRepository>();
            var campaignValidatorMock = new Mock<ICampaignValidator>();
            var mediaMonthAndWeekAggregateCache = new Mock<IMediaMonthAndWeekAggregateCache>();
            var quarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();

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
                _LockingManagerApplicationServiceMock.Object);

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, _CreatedBy, _CreatedDate));

            // Assert
            campaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<CampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.Advertiser.Id == advertiserId &&
                                        campaign.Agency.Id == agencyId),
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
                _LockingManagerApplicationServiceMock.Object);

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

            dataRepositoryFactoryMock.Setup(x => x.GetDataRepository<ICampaignRepository>()).Returns(campaignRepositoryMock.Object);
            campaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(null)).Returns(getCampaignsDateRangesReturn);

            var tc = new CampaignService(
                dataRepositoryFactoryMock.Object,
                campaignValidatorMock.Object,
                mediaAggregateCache,
                quarterCalculationEngine,
                _TrafficApiClient,
                _LockingManagerApplicationServiceMock.Object);

            // Act
            var campaignQuarters = tc.GetQuarters(null, new DateTime(2019, 8, 20));

            // Assert
            Assert.IsNotNull(campaignQuarters);
            Assert.AreEqual(1, campaignQuarters.Quarters.Count);
            Assert.AreEqual(3, campaignQuarters.DefaultQuarter.Quarter);
            Assert.AreEqual(2019, campaignQuarters.DefaultQuarter.Year);
        }
    }
}
