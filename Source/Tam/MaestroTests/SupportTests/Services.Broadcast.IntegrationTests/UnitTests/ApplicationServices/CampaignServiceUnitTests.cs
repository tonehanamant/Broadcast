using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Campaign;
using Tam.Maestro.Services.ContractInterfaces;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.StationInventory;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    public class CampaignServiceUnitTests
    {
        private const string _User = "UnitTest_User";
        private const string _TemplatesFilePath = "\\templates";
        private readonly DateTime _CurrentDate = new DateTime(2017, 10, 17, 7, 30, 23);

        private Mock<IBroadcastLockingManagerApplicationService> _LockingManagerApplicationServiceMock;
        private Mock<IDataRepositoryFactory> _DataRepositoryFactoryMock;
        private Mock<ICampaignRepository> _CampaignRepositoryMock;
        private Mock<ICampaignValidator> _CampaignValidatorMock;
        private Mock<IMediaMonthAndWeekAggregateCache> _MediaMonthAndWeekAggregateCacheMock;
        private Mock<IQuarterCalculationEngine> _QuarterCalculationEngineMock;
        private Mock<ICampaignAggregator> _CampaignAggregatorMock;
        private Mock<ITrafficApiCache> _TrafficApiCacheMock;
        private Mock<IAudienceService> _AudienceServiceMock;
        private Mock<ISpotLengthService> _SpotLengthServiceMock;
        private Mock<IDaypartDefaultService> _DaypartDefaultServiceMock;
        private Mock<ISharedFolderService> _SharedFolderServiceMock;
        private Mock<ICampaignAggregationJobTrigger> _CampaignAggregationJobTriggerMock;
        private Mock<ICampaignSummaryRepository> _CampaignSummaryRepositoryMock;
        private Mock<IPlanRepository> _PlanRepositoryMock;
        private Mock<IInventoryRepository> _InventoryRepositoryMock;
        private Mock<IMarketCoverageRepository> _MarketCoverageRepositoryMock;
        private Mock<IStationProgramRepository> _StationProgramRepositoryMock;
        private Mock<IStandartDaypartEngine> _StandartDaypartEngineMock;

        [SetUp]
        public void SetUp()
        {
            _LockingManagerApplicationServiceMock = new Mock<IBroadcastLockingManagerApplicationService>();
            _DataRepositoryFactoryMock = new Mock<IDataRepositoryFactory>();
            _CampaignRepositoryMock = new Mock<ICampaignRepository>();
            _CampaignValidatorMock = new Mock<ICampaignValidator>();
            _MediaMonthAndWeekAggregateCacheMock = new Mock<IMediaMonthAndWeekAggregateCache>();
            _QuarterCalculationEngineMock = new Mock<IQuarterCalculationEngine>();
            _CampaignAggregatorMock = new Mock<ICampaignAggregator>();
            _TrafficApiCacheMock = new Mock<ITrafficApiCache>();
            _TrafficApiCacheMock = new Mock<ITrafficApiCache>();
            _AudienceServiceMock = new Mock<IAudienceService>();
            _SpotLengthServiceMock = new Mock<ISpotLengthService>();
            _DaypartDefaultServiceMock = new Mock<IDaypartDefaultService>();
            _SharedFolderServiceMock = new Mock<ISharedFolderService>();
            _CampaignAggregationJobTriggerMock = new Mock<ICampaignAggregationJobTrigger>();
            _CampaignSummaryRepositoryMock = new Mock<ICampaignSummaryRepository>();
            _PlanRepositoryMock = new Mock<IPlanRepository>();
            _MarketCoverageRepositoryMock = new Mock<IMarketCoverageRepository>();
            _InventoryRepositoryMock = new Mock<IInventoryRepository>();
            _StationProgramRepositoryMock = new Mock<IStationProgramRepository>();
            _StandartDaypartEngineMock = new Mock<IStandartDaypartEngine>();

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IStationProgramRepository>())
                .Returns(_StationProgramRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IInventoryRepository>())
                .Returns(_InventoryRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IPlanRepository>())
                .Returns(_PlanRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ICampaignRepository>())
                .Returns(_CampaignRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<ICampaignSummaryRepository>())
                .Returns(_CampaignSummaryRepositoryMock.Object);

            _DataRepositoryFactoryMock
                .Setup(x => x.GetDataRepository<IMarketCoverageRepository>())
                .Returns(_MarketCoverageRepositoryMock.Object);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsFilteredCampaigns()
        {
            // Arrange
            var agency = new AgencyDto { Id = 1, Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 };
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

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<PlanStatusEnum>())).Returns(_GetCampaignWithsummeriesReturn(agency, advertiser));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29), new DateTime(2009, 3, 29)));

            _TrafficApiCacheMock
                .Setup(x => x.GetAgency(It.IsAny<int>()))
                .Returns(agency);

            _TrafficApiCacheMock
                .Setup(x => x.GetAdvertiser(It.IsAny<int>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetCampaigns(new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Year = 2019,
                    Quarter = 2
                }, 
                PlanStatus = PlanStatusEnum.ClientApproval
            }, _CurrentDate);

            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<PlanStatusEnum>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsCampaignsFilteredUsingDefaultFilter()
        {
            // Arrange
            var agency = new AgencyDto { Id = 1, Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 };
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
            
            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null)).Returns(_GetCampaignWithsummeriesReturn(agency, advertiser));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterRangeByDate(_CurrentDate)).Returns(new QuarterDetailDto
            {
                Year = 2019,
                Quarter = 2
            });
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29), new DateTime(2009, 3, 29)));

            _TrafficApiCacheMock
                .Setup(x => x.GetAgency(It.IsAny<int>()))
                .Returns(agency);

            _TrafficApiCacheMock
                .Setup(x => x.GetAdvertiser(It.IsAny<int>()))
                .Returns(advertiser);

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetCampaigns(null, _CurrentDate);

            // Assert
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null), Times.Once);
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

            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(It.IsAny<int?>(), It.IsAny<int?>())).Returns(new DateRange(null, null));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterRangeByDate(_CurrentDate)).Returns(new QuarterDetailDto());
            _CampaignRepositoryMock
                .Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), null))
                .Callback(() => throw new Exception(expectedMessage));

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<Exception>(() => tc.GetCampaigns(It.IsAny<CampaignFilterDto>(), _CurrentDate));

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

            var tc = _BuildCampaignService();

            // Act
            tc.SaveCampaign(campaign, _User, _CurrentDate);

            // Assert
            _CampaignValidatorMock.Verify(x => x.Validate(
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

            var tc = _BuildCampaignService();

            // Act
            tc.SaveCampaign(campaign, _User, _CurrentDate);

            // Assert
            _CampaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId),
                _User,
                _CurrentDate), Times.Once);
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

            _CampaignValidatorMock
                .Setup(s => s.Validate(It.IsAny<SaveCampaignDto>()))
                .Callback<SaveCampaignDto>(x => throw new Exception(expectedMessage));

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, _User, _CurrentDate));

            // Assert
            _CampaignValidatorMock.Verify(x => x.Validate(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId)), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.CreateCampaign(It.IsAny<SaveCampaignDto>(), _User, _CurrentDate), Times.Never);
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

            _CampaignRepositoryMock
                .Setup(s => s.CreateCampaign(It.IsAny<SaveCampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback(() => throw new Exception(expectedMessage));

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<Exception>(() => tc.SaveCampaign(campaign, _User, _CurrentDate));

            // Assert
            _CampaignRepositoryMock.Verify(x => x.CreateCampaign(
                It.Is<SaveCampaignDto>(c => c.Id == campaignId &&
                                        c.Name == campaignName &&
                                        c.Notes == campaignNotes &&
                                        c.AdvertiserId == advertiserId &&
                                        campaign.AgencyId == agencyId),
                _User,
                _CurrentDate), Times.Once);
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void GetQuarters()
        {
            // Arrange
            var getCampaignsDateRangesReturn = new List<DateRange>
            {
                new DateRange(null, null),
                new DateRange(new DateTime(2019, 1, 1), null),
                new DateRange(new DateTime(2019, 2, 1), new DateTime(2019, 9, 1))
            };
            _CampaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(null)).Returns(getCampaignsDateRangesReturn);
            

            _QuarterCalculationEngineMock
                .Setup(x => x.GetQuartersForDateRanges(It.IsAny<List<DateRange>>()))
                .Returns(new List<QuarterDetailDto>
                {
                    new QuarterDetailDto
                    {
                        Quarter = 1,
                        Year = 2019,
                        StartDate = new DateTime(2018, 12, 31),
                        EndDate = new DateTime(2019, 03, 31, 23, 59, 59)
                    },
                    new QuarterDetailDto
                    {
                        Quarter = 2,
                        Year = 2019,
                        StartDate = new DateTime(2019, 04, 01),
                        EndDate = new DateTime(2019, 06, 30, 23, 59, 59)
                    },
                    new QuarterDetailDto
                    {
                        Quarter = 3,
                        Year = 2019,
                        StartDate = new DateTime(2018, 07, 01),
                        EndDate = new DateTime(2019, 09, 29, 23, 59, 59)
                    }
                });

            _QuarterCalculationEngineMock
                .Setup(x => x.GetQuarterRangeByDate(It.IsAny<DateTime>()))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 3,
                    Year = 2019,
                    StartDate = new DateTime(2018, 07, 01),
                    EndDate = new DateTime(2019, 09, 29, 23, 59, 59)
                });

            var tc = _BuildCampaignService();

            // Act
            var campaignQuarters = tc.GetQuarters(null, new DateTime(2019, 8, 20));

            // Assert
            Assert.IsNotNull(campaignQuarters);
            Assert.AreEqual(3, campaignQuarters.Quarters.Count);
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

            _CampaignRepositoryMock
                .Setup(x => x.CreateCampaign(It.IsAny<SaveCampaignDto>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(newCampaignId);
            _CampaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(It.IsAny<PlanStatusEnum?>())).Returns(new List<DateRange>());
            _CampaignSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>())).Returns(1);

            var tc = _BuildCampaignService();

            tc.SaveCampaign(campaign, _User, _CurrentDate);

            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(newCampaignId, _User), Times.Never);
        }

        [Test]
        public void TriggerCampaignAggregation()
        {
            const int campaignId = 1;

            var tc = _BuildCampaignService();

            tc.TriggerCampaignAggregationJob(campaignId, _User);

            _CampaignAggregationJobTriggerMock.Verify(s => s.TriggerJob(campaignId, _User), Times.Once);
        }

        [Test]
        public void ProcessAggregation()
        {
            const int campaignId = 1;

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(It.IsAny<PlanStatusEnum?>())).Returns(new List<DateRange>());
            _CampaignAggregatorMock.Setup(s => s.Aggregate(It.IsAny<int>()))
                .Returns(new CampaignSummaryDto());
            _CampaignSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()))
                .Returns(1);

            var tc = _BuildCampaignService();

            tc.ProcessCampaignAggregation(campaignId);

            _CampaignAggregatorMock.Verify(s => s.Aggregate(campaignId), Times.Once);
            _CampaignSummaryRepositoryMock.Verify(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()), Times.Once);
            _CampaignSummaryRepositoryMock.Verify(s => s.SetSummaryProcessingStatusToError(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ProcessAggregation_WithError()
        {
            const int campaignId = 1;

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsDateRanges(It.IsAny<PlanStatusEnum?>())).Returns(new List<DateRange>());
            _CampaignAggregatorMock.Setup(s => s.Aggregate(It.IsAny<int>()))
                .Returns(new CampaignSummaryDto());
            _CampaignSummaryRepositoryMock.Setup(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()))
                .Throws(new Exception("Error from SaveSummary"));

            var tc = _BuildCampaignService();

            var caught = Assert.Throws<Exception>(() => tc.ProcessCampaignAggregation(campaignId));

            Assert.AreEqual("Error from SaveSummary", caught.Message);
            _CampaignAggregatorMock.Verify(s => s.Aggregate(campaignId), Times.Once);
            _CampaignSummaryRepositoryMock.Verify(s => s.SaveSummary(It.IsAny<CampaignSummaryDto>()), Times.Once);
            _CampaignSummaryRepositoryMock.Verify(s => s.SetSummaryProcessingStatusToError(campaignId, $"Exception caught during processing : Error from SaveSummary"), Times.Once);
        }

        [Test]
        public void CanNotGenerateReport_WhenCampaignIsLocked()
        {
            // Arrange
            const int campaignId = 5;
            const string lockedUserName = "UnitTestsUser2";
            var expectedMessage = $"Campaign with id {campaignId} has been locked by {lockedUserName}";

            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                ExportType = CampaignExportTypeEnum.Contract
            };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(new CampaignDto
                {
                    Id = campaignId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetCampaignLockingKey(campaignId)))
                .Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = lockedUserName
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<ApplicationException>(() => tc.GetAndValidateCampaignReportData(request));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CanNotGenerateReport_WhenPlanIsLocked()
        {
            // Arrange
            const int campaignId = 5;
            const int planId = 7;
            const string lockedUserName = "UnitTestsUser2";
            var expectedMessage = $"Plan with id {planId} has been locked by {lockedUserName}";

            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                SelectedPlans = new List<int> { planId },
                ExportType = CampaignExportTypeEnum.Contract
            };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = planId,
                            PostingType = PostingTypeEnum.NSI,
                            Status = PlanStatusEnum.Complete
                        }
                    }
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetCampaignLockingKey(campaignId)))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetPlanLockingKey(planId)))
                .Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = lockedUserName
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<ApplicationException>(() => tc.GetAndValidateCampaignReportData(request));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CanNotGenerateReport_WhenPlanAggregation_IsInProgress()
        {
            // Arrange
            const int campaignId = 5;
            const int planId = 7;
            var expectedMessage = $"Data aggregation for the plan with id {planId} is in progress. Please wait until the process is done";

            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                SelectedPlans = new List<int> { planId },
                ExportType = CampaignExportTypeEnum.Contract
            };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = planId,
                            PostingType = PostingTypeEnum.NSI,
                            Status = PlanStatusEnum.Complete,
                            ProcessingStatus = PlanAggregationProcessingStatusEnum.InProgress
                        }
                    }
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetCampaignLockingKey(campaignId)))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetPlanLockingKey(planId)))
                .Returns(new LockResponse
                {
                    Success = true
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<ApplicationException>(() => tc.GetAndValidateCampaignReportData(request));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CanNotGenerateReport_WhenPlanAggregation_HasFailed()
        {
            // Arrange
            const int campaignId = 5;
            const int planId = 7;
            var expectedMessage = $"Data aggregation for the plan with id {planId} has failed. Please contact the support";

            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                SelectedPlans = new List<int> { planId },
                ExportType = CampaignExportTypeEnum.Contract
            };

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(new CampaignDto
                {
                    Id = campaignId,
                    Plans = new List<PlanSummaryDto>
                    {
                        new PlanSummaryDto
                        {
                            PlanId = planId,
                            PostingType = PostingTypeEnum.NSI,
                            Status = PlanStatusEnum.Complete,
                            ProcessingStatus = PlanAggregationProcessingStatusEnum.Error
                        }
                    }
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetCampaignLockingKey(campaignId)))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetPlanLockingKey(planId)))
                .Returns(new LockResponse
                {
                    Success = true
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<ApplicationException>(() => tc.GetAndValidateCampaignReportData(request));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_OnProgramLineupReportGeneration_WhenNoPlansSelected()
        {
            // Arrange
            var expectedMessage = $"Choose at least one plan";
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int>()
            };

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<ApplicationException>(() => tc.GetProgramLineupReportData(request, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void ThrowsException_OnProgramLineupReportGeneration_WhenNoPricingRunsDone()
        {
            // Arrange
            const string expectedMessage = "There are no completed pricing runs for the chosen plan. Please run pricing";
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { 1 }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto());
            
            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<ApplicationException>(() => tc.GetProgramLineupReportData(request, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        [TestCase(BackgroundJobProcessingStatus.Failed, "The latest pricing run was failed. Please run pricing again or contact the support")]
        [TestCase(BackgroundJobProcessingStatus.Queued, "There is a pricing run in progress right now. Please wait until it is completed")]
        [TestCase(BackgroundJobProcessingStatus.Processing, "There is a pricing run in progress right now. Please wait until it is completed")]
        public void ThrowsException_OnProgramLineupReportGeneration_WhenPricingJobStatusIsNotAcceptable(
            BackgroundJobProcessingStatus jobStatus,
            string expectedMessage)
        {
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { 1 }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto());

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = jobStatus
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<ApplicationException>(() => tc.GetProgramLineupReportData(request, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsData_ForProgramLineupReport_45spot_notEquivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            const int agencyId = 4;
            const int advertiserId = 5;
            const int audienceId = 6;
            const int spotLengthId = 7;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    SpotLengthId = spotLengthId,
                    Equivalized = false,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250
                });

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyId = agencyId,
                    AdvertiserId = advertiserId
                });

            _SetupBaseProgramLineupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetLatestPricingJob(firstPlanId), Times.Once);
            _TrafficApiCacheMock.Verify(x => x.GetAgency(agencyId), Times.Once);
            _TrafficApiCacheMock.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthServiceMock.Verify(x => x.GetAllSpotLengths(), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpots(firstPlanId), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))), 
                Times.Once);
            
            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 3002 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsData_ForProgramLineupReport_45spot_Equivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            const int agencyId = 4;
            const int advertiserId = 5;
            const int audienceId = 6;
            const int spotLengthId = 7;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    SpotLengthId = spotLengthId,
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250
                });

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyId = agencyId,
                    AdvertiserId = advertiserId
                });

            _SetupBaseProgramLineupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetLatestPricingJob(firstPlanId), Times.Once);
            _TrafficApiCacheMock.Verify(x => x.GetAgency(agencyId), Times.Once);
            _TrafficApiCacheMock.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthServiceMock.Verify(x => x.GetAllSpotLengths(), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpots(firstPlanId), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 3002 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsData_ForProgramLineupReport_30spot_Equivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            const int agencyId = 4;
            const int advertiserId = 5;
            const int audienceId = 6;
            const int spotLengthId = 8;

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { firstPlanId, secondPlanId }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                {
                    CampaignId = campaignId,
                    AudienceId = audienceId,
                    Name = "Brave Plan",
                    FlightStartDate = new DateTime(2020, 03, 1),
                    FlightEndDate = new DateTime(2020, 03, 14),
                    SpotLengthId = spotLengthId,
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250
                });

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyId = agencyId,
                    AdvertiserId = advertiserId
                });

            _SetupBaseProgramLineupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            // Assert
            _PlanRepositoryMock.Verify(x => x.GetPlan(firstPlanId, null), Times.Once);
            _CampaignRepositoryMock.Verify(x => x.GetCampaign(campaignId), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetLatestPricingJob(firstPlanId), Times.Once);
            _TrafficApiCacheMock.Verify(x => x.GetAgency(agencyId), Times.Once);
            _TrafficApiCacheMock.Verify(x => x.GetAdvertiser(advertiserId), Times.Once);
            _AudienceServiceMock.Verify(x => x.GetAudienceById(audienceId), Times.Once);
            _SpotLengthServiceMock.Verify(x => x.GetAllSpotLengths(), Times.Once);
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpots(firstPlanId), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 3002 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private void _SetupBaseProgramLineupTestData()
        {
            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime(2020, 02, 12)
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpots(It.IsAny<int>()))
                .Returns(_GetPlanPricingAllocatedSpots());

            _InventoryRepositoryMock
                .Setup(x => x.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetStationInventoryManifests());

            _TrafficApiCacheMock
                .Setup(x => x.GetAgency(It.IsAny<int>()))
                .Returns(new AgencyDto
                {
                    Name = "1st Image Marketing"
                });

            _TrafficApiCacheMock
                .Setup(x => x.GetAdvertiser(It.IsAny<int>()))
                .Returns(new AdvertiserDto
                {
                    Name = "PetMed Express, Inc"
                });

            _AudienceServiceMock
                .Setup(x => x.GetAudienceById(It.IsAny<int>()))
                .Returns(new PlanAudienceDisplay
                {
                    Code = "A18-20"
                });

            _SpotLengthServiceMock
                .Setup(x => x.GetAllSpotLengths())
                .Returns(new List<LookupDto>
                {
                    new LookupDto { Id = 7, Display = "45" },
                    new LookupDto { Id = 8, Display = "30" }
                });

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoveragesWithStations())
                .Returns(_GetMarketCoverages());

            _StationProgramRepositoryMock
                .Setup(x => x.GetPrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, PlanPricingInventoryProgram.ManifestDaypart.Program>
                {
                    {
                        1001,
                        new PlanPricingInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "Movie",
                            Name = "Joker"
                        }
                    },
                    {
                        2001,
                        new PlanPricingInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "News",
                            Name = "Late News"
                        }
                    },
                    {
                        3001,
                        new PlanPricingInventoryProgram.ManifestDaypart.Program
                        {
                            Genre = "Movie",
                            Name = "1917"
                        }
                    }
                });

            _StandartDaypartEngineMock
                .Setup(x => x.GetDaypartCodeByGenreAndTimeRange(It.IsAny<string>(), It.IsAny<TimeRange>()))
                .Returns(new DaypartDefaultFullDto
                {
                    DaypartType = DaypartTypeEnum.News,
                    DefaultStartTimeSeconds = 80,
                    DefaultEndTimeSeconds = 139,
                    Code = "EMN"
                });
        }

        private MarketCoverageByStation _GetMarketCoverages()
        {
            return new MarketCoverageByStation
            {
                Markets = new List<MarketCoverageByStation.Market>
                {
                    new MarketCoverageByStation.Market
                    {
                        MarketCode = 100,
                        Rank = 2,
                        MarketName = "Binghamton"
                    },
                    new MarketCoverageByStation.Market
                    {
                        MarketCode = 200,
                        Rank = 1,
                        MarketName = "New York"
                    },
                    new MarketCoverageByStation.Market
                    {
                        MarketCode = 300,
                        Rank = 3,
                        MarketName = "Macon"
                    }
                }
            };
        }

        private List<PlanPricingAllocatedSpot> _GetPlanPricingAllocatedSpots()
        {
            return new List<PlanPricingAllocatedSpot>
            {
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 10,
                    Daypart = new DisplayDaypart
                    {
                        Id = 10001
                    },
                    Spots = 1,
                    Impressions = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 20,
                    Daypart = new DisplayDaypart
                    {
                        Id = 20001
                    },
                    Spots = 1,
                    Impressions = 20
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 30,
                    Daypart = new DisplayDaypart
                    {
                        Id = 30002
                    },
                    Spots = 2,
                    Impressions = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 40,
                    Daypart = new DisplayDaypart
                    {
                        Id = 40001
                    },
                    Spots = 1,
                    Impressions = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 50,
                    Daypart = new DisplayDaypart
                    {
                        Id = 50001
                    },
                    Spots = 5,
                    Impressions = 10
                }
            };
        }

        private List<StationInventoryManifest> _GetStationInventoryManifests()
        {
            return new List<StationInventoryManifest>
                {
                    new StationInventoryManifest
                    {
                        Id = 10,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 100,
                            LegacyCallLetters = "KSTP",
                            Affiliation = "ABC"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 1001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 10001,
                                    StartTime = 100,
                                    EndTime = 199,
                                    Monday = true,
                                    Tuesday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 20,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 200,
                            LegacyCallLetters = "WTTV",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 2001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 20001,
                                    StartTime = 200,
                                    EndTime = 299,
                                    Wednesday = true,
                                    Thursday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 30,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 300,
                            LegacyCallLetters = "KELO",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 3001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 30001,
                                    StartTime = 300,
                                    EndTime = 399,
                                    Friday = true,
                                    Sunday = true
                                }
                            },
                            new StationInventoryManifestDaypart
                            {
                                Id = 3002,
                                ProgramName = "The Shawshank Redemption",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 30002,
                                    StartTime = 300,
                                    EndTime = 399,
                                    Friday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 40,
                        Station = null
                    },
                    new StationInventoryManifest
                    {
                        Id = 50,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = null
                        }
                    }
                };
        }

        private CampaignService _BuildCampaignService()
        {
            return new CampaignService(
                _DataRepositoryFactoryMock.Object,
                _CampaignValidatorMock.Object,
                _MediaMonthAndWeekAggregateCacheMock.Object,
                _QuarterCalculationEngineMock.Object,
                _LockingManagerApplicationServiceMock.Object,
                _CampaignAggregatorMock.Object,
                _CampaignAggregationJobTriggerMock.Object,
                _TrafficApiCacheMock.Object,
                _AudienceServiceMock.Object,
                _SpotLengthServiceMock.Object,
                _DaypartDefaultServiceMock.Object,
                _SharedFolderServiceMock.Object,
                _StandartDaypartEngineMock.Object);
        }
    }
}
