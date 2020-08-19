using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.Plan.Pricing.PlanPricingInventoryProgram.ManifestDaypart;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices
{
    [Category("short_running")]
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
        private Mock<IDateTimeEngine> _DateTimeEngineMock;
        private Mock<IWeeklyBreakdownEngine> _WeeklyBreakdownEngineMock;

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
            _DateTimeEngineMock = new Mock<IDateTimeEngine>();
            _WeeklyBreakdownEngineMock = new Mock<IWeeklyBreakdownEngine>();

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

            var stubbedConfigurationClient = new StubbedConfigurationWebApiClient();
            SystemComponentParameterHelper.SetConfigurationClient(stubbedConfigurationClient);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsFilteredCampaigns()
        {
            // Arrange
            var agency = new AgencyDto { Id = 1, Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 };

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>())).Returns(_GetCampaignWithsummeriesReturn(agency, advertiser));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29)
                , new DateTime(2009, 3, 29)));

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
            _CampaignRepositoryMock.Verify(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>()
                , It.IsAny<PlanStatusEnum>()), Times.Once);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsCampaignsFilteredUsingDefaultFilter()
        {
            // Arrange
            var agency = new AgencyDto { Id = 1, Name = "Name1" };
            var advertiser = new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 };

            _CampaignRepositoryMock.Setup(x => x.GetCampaignsWithSummary(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null))
                .Returns(_GetCampaignWithsummeriesReturn(agency, advertiser));
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterRangeByDate(_CurrentDate)).Returns(new QuarterDetailDto
            {
                Year = 2019,
                Quarter = 2
            });
            _QuarterCalculationEngineMock.Setup(x => x.GetQuarterDateRange(2, 2019)).Returns(new DateRange(new DateTime(2008, 12, 29)
                , new DateTime(2009, 3, 29)));

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
                AdvertiserId = advertiserId,
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
            _CampaignSummaryRepositoryMock.Verify(s => 
                s.SetSummaryProcessingStatusToError(campaignId, $"Exception caught during processing : Error from SaveSummary"), Times.Once);
        }

        [Test]
        public void CanNotGenerateCampaignExport_WhenCampaignIsLocked()
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
        public void CanNotGenerateProgramLineup_WhenCampaignIsLocked()
        {
            // Arrange
            const int campaignId = 6;
            const int planId = 1197;
            const string lockedUserName = "UnitTestsUser2";
            var expectedMessage = $"Campaign with id {campaignId} has been locked by {lockedUserName}";

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { planId }
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

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), null))
                .Returns(_GetPlan(planId, campaignId));

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetPlanLockingKey(planId)))
                .Returns(new LockResponse
                {
                    Success = true,
                    LockedUserName = lockedUserName
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
            var caught = Assert.Throws<ApplicationException>(() => tc.GetProgramLineupReportData(request, DateTime.Now));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CanNotGenerateProgramLineup_WhenPlanIsLocked()
        {
            // Arrange
            const int campaignId = 6;
            const int planId = 1197;
            const string lockedUserName = "UnitTestsUser2";
            var expectedMessage = $"Plan with id {planId} has been locked by {lockedUserName}";

            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { planId }
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

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), null))
                .Returns(_GetPlan(planId, campaignId));

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(KeyHelper.GetPlanLockingKey(planId)))
                .Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = lockedUserName
                });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<ApplicationException>(() => tc.GetProgramLineupReportData(request, DateTime.Now));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        public void CanNotGenerateCampaignExport_WhenPlanIsLocked()
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
        public void CanNotGenerateCampaignExport_WhenPlanAggregation_IsInProgress()
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
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
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
        public void CanNotGenerateCampaignExport_WhenPlanAggregation_HasFailed()
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
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
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
        public void ThrowsException_ProgramLineup_WhenNoPlansSelected()
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
        public void ThrowsException_ProgramLineup_WhenNoPricingRunsDone()
        {
            // Arrange
            const string expectedMessage = "There are no completed pricing runs for the chosen plan. Please run pricing";
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { 1 }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                { 
                    Dayparts = _GetPlanDayparts()
                });

            _LockingManagerApplicationServiceMock
                 .Setup(x => x.GetLockObject(It.IsAny<string>()))
                 .Returns(new LockResponse
                 {
                     Success = true
                 });

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
        public void ThrowsException_ProgramLineup_PricingJobIsNotAcceptable(
            BackgroundJobProcessingStatus jobStatus,
            string expectedMessage)
        {
            var request = new ProgramLineupReportRequest
            {
                SelectedPlans = new List<int> { 1 }
            };

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), It.IsAny<int?>()))
                .Returns(new PlanDto
                { 
                    Dayparts = _GetPlanDayparts()
                });

            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = jobStatus
                });

            _LockingManagerApplicationServiceMock
                 .Setup(x => x.GetLockObject(It.IsAny<string>()))
                 .Returns(new LockResponse
                 {
                     Success = true
                 });

            var tc = _BuildCampaignService();

            // Act
            var caught = Assert.Throws<ApplicationException>(() => tc.GetProgramLineupReportData(request, _CurrentDate));

            // Assert
            Assert.AreEqual(expectedMessage, caught.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsData_ProgramLineupReport_45UnEquivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            const int agencyId = 4;
            const int advertiserId = 5;
            const int audienceId = 6;

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
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 8, Weight = 50 } },
                    Equivalized = false,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts()
                });

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyId = agencyId,
                    AdvertiserId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
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
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProgramLineup_MultipleCreativeLengths()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            const int agencyId = 4;
            const int advertiserId = 5;
            const int audienceId = 6;

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
                    CreativeLengths = new List<CreativeLength> {
                        new CreativeLength { SpotLengthId = 1, Weight = 50 },
                        new CreativeLength { SpotLengthId = 2, Weight = 20 },
                        new CreativeLength { SpotLengthId = 3, Weight = 30 }
                    },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts()
                });

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyId = agencyId,
                    AdvertiserId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
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
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private List<PlanDaypartDto> _GetPlanDayparts()
        {
            return new List<PlanDaypartDto>
            {
                new PlanDaypartDto
                {
                    DaypartCodeId = 10001,
                    StartTimeSeconds = 50,
                    EndTimeSeconds = 150
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 20001,
                    StartTimeSeconds = 230,
                    EndTimeSeconds = 280
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 30002,
                    StartTimeSeconds = 350,
                    EndTimeSeconds = 450
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 40001,
                    StartTimeSeconds = 100,
                    EndTimeSeconds = 200
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 50001,
                    StartTimeSeconds = 100,
                    EndTimeSeconds = 200
                },
                new PlanDaypartDto
                {
                    DaypartCodeId = 60001,
                    StartTimeSeconds = 200,
                    EndTimeSeconds = 299
                }
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProgramLineup_MultipleCreativeLengths_UnEquivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            const int agencyId = 4;
            const int advertiserId = 5;
            const int audienceId = 6;

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
                    CreativeLengths = new List<CreativeLength> {
                        new CreativeLength { SpotLengthId = 1, Weight = 50 },
                        new CreativeLength { SpotLengthId = 2, Weight = 20 },
                        new CreativeLength { SpotLengthId = 3, Weight = 30 }
                    },
                    Equivalized = false,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts()
                });

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyId = agencyId,
                    AdvertiserId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _SetupBaseProgramLineupTestData();

            var tc = _BuildCampaignService();

            // Act
            var result = tc.GetProgramLineupReportData(request, _CurrentDate);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsData_ProgramLineup_45Equivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            const int agencyId = 4;
            const int advertiserId = 5;
            const int audienceId = 6;

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
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 8, Weight = 50 } },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts()
                });

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyId = agencyId,
                    AdvertiserId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
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
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ReturnsData_ProgramLineup_30Equivalized()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            const int agencyId = 4;
            const int advertiserId = 5;
            const int audienceId = 6;

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
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = _GetPlanDayparts()
                });

            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyId = agencyId,
                    AdvertiserId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                 .Setup(x => x.GetLockObject(It.IsAny<string>()))
                 .Returns(new LockResponse
                 {
                     Success = true
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
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40, 50, 60, 70 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 6001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProgramLineup_RollupStationSpecificNewsPrograms()
        {
            // Arrange
            const int firstPlanId = 1;
            const int secondPlanId = 2;
            const int campaignId = 3;
            const int agencyId = 4;
            const int advertiserId = 5;
            const int audienceId = 6;

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
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                    Equivalized = true,
                    PostingType = PostingTypeEnum.NTI,
                    TargetImpressions = 250,
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 30001,
                            StartTimeSeconds = 57600,
                            EndTimeSeconds = 68399
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 10001,
                            StartTimeSeconds = 14400,
                            EndTimeSeconds = 35999
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 20001,
                            StartTimeSeconds = 39600,
                            EndTimeSeconds = 46799
                        },
                        new PlanDaypartDto
                        {
                            DaypartCodeId = 40001,
                            StartTimeSeconds = 72000,
                            EndTimeSeconds = 299
                        }
                    }
                });

        _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(It.IsAny<int>()))
                .Returns(new CampaignDto
                {
                    AgencyId = agencyId,
                    AdvertiserId = advertiserId
                });

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            _SetupBaseProgramLineupForRollupTestData();

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
            _PlanRepositoryMock.Verify(x => x.GetPlanPricingAllocatedSpotsByPlanId(firstPlanId), Times.Once);
            _MarketCoverageRepositoryMock.Verify(x => x.GetLatestMarketCoveragesWithStations(), Times.Once);

            var passedManifestIds = new List<int> { 10, 20, 30, 40 };
            _InventoryRepositoryMock.Verify(x => x.GetStationInventoryManifestsByIds(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestIds))),
                Times.Once);

            var passedManifestDaypartIds = new List<int> { 1001, 2001, 3001, 4001 };
            _StationProgramRepositoryMock.Verify(x => x.GetPrimaryProgramsForManifestDayparts(
                It.Is<IEnumerable<int>>(list => list.SequenceEqual(passedManifestDaypartIds))),
                Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_MultipleCreativeLengths()
        {
            // Arrange
            const int planId = 1853;
            const int campaignId = 652;

            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                ExportType = CampaignExportTypeEnum.Contract,
                SelectedPlans = new List<int> { planId }
            };

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                        , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(_GetWeeklyBreakdownByWeek());
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GetWeeklyBreakdownCombinations(It.IsAny<List<CreativeLength>>(), It.IsAny<List<PlanDaypartDto>>()))
                .Returns(_GetWeeklyBreakdownCombinations());
            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(_GetCampaignForExport(campaignId, request.SelectedPlans));

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            var planDto = _GetPlanForCampaignExport(planId, campaignId);
            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), null))
                .Returns(planDto);

            _SpotLengthServiceMock
                .Setup(x => x.GetAllSpotLengths())
                .Returns(_GetAllSpotLengths());

            _DaypartDefaultServiceMock
                .Setup(s => s.GetAllDaypartDefaults())
                .Returns(_GetDaypartDefaults());

            _QuarterCalculationEngineMock
                .Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<QuarterDetailDto>
                    {
                        new QuarterDetailDto { Quarter = 1, Year = 2020,
                            StartDate = new DateTime(2020,01,01),
                            EndDate = new DateTime(2020,03,31)
                        },
                        new QuarterDetailDto { Quarter = 2, Year = 2020,
                            StartDate = new DateTime(2020,04,01),
                            EndDate = new DateTime(2020,06,30)}
                    });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterRangeByDate(It.IsAny<DateTime>()))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 1,
                    Year = 2020
                });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterDetail(1, 2020))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 1,
                    Year = 2020,

                });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterDetail(2, 2020))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 2,
                    Year = 2020
                });

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetTheSameMediaWeeksAsThePlan(planDto));
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksInRange(new DateTime(2020, 01, 01), new DateTime(2020, 03, 31)))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(846, "2020-03-09", "2020-03-15", 462),
                    _GetMediaWeek(847, "2020-03-16", "2020-03-22", 462),
                    _GetMediaWeek(848, "2020-03-23", "2020-03-29", 462)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksInRange(new DateTime(2020, 04, 01), new DateTime(2020, 06, 30)))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(849, "2020-03-30", "2020-04-05", 463),
                    _GetMediaWeek(850, "2020-04-06", "2020-04-12", 463)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByMediaMonth(462))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(846, "2020-03-09", "2020-03-15", 462),
                    _GetMediaWeek(847, "2020-03-16", "2020-03-22", 462),
                    _GetMediaWeek(848, "2020-03-23", "2020-03-29", 462)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByMediaMonth(463))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(849, "2020-03-30", "2020-04-05", 463),
                    _GetMediaWeek(850, "2020-04-06", "2020-04-12", 463)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaMonthsBetweenDatesInclusive(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<MediaMonth> {
                    new MediaMonth{Id = 463, MediaMonthX = "April"}, new MediaMonth{ Id = 462, MediaMonthX = "March"}
                });

            _TrafficApiCacheMock
                .Setup(x => x.GetAgency(It.IsAny<int>()))
                .Returns(new AgencyDto { Id = 1, Name = "Name1" });
            _TrafficApiCacheMock
                .Setup(x => x.GetAdvertiser(It.IsAny<int>()))
                .Returns(new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 });

            _AudienceServiceMock
                .Setup(x => x.GetAudienceById(It.IsAny<int>()))
                .Returns(new PlanAudienceDisplay
                {
                    Code = "A18-20",
                    Id = 1
                });
            _AudienceServiceMock
                .Setup(x => x.GetAudiences())
                .Returns(new List<PlanAudienceDisplay> {
                    new PlanAudienceDisplay
                    {
                        Code = "A18-20",
                        Id = 1
                    }
                });

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2020, 01, 01));

            var tc = _BuildCampaignService();

            // Act
            var response = tc.GetAndValidateCampaignReportData(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_MultipleCreativeLengths_BP1_229()
        {
            // Arrange
            const int campaignId = 1;

            var request = new CampaignReportRequest
            {
                CampaignId = 1,
                ExportType = CampaignExportTypeEnum.Contract,
                SelectedPlans = new List<int> { 1, 2, 3 }
            };

            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(_GetWeeklyBreakdownByWeek());
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GetWeeklyBreakdownCombinations(It.IsAny<List<CreativeLength>>(), It.IsAny<List<PlanDaypartDto>>()))
                .Returns(_GetWeeklyBreakdownCombinations());
            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(_GetCampaignForExport(campaignId, request.SelectedPlans));

            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });

            var plan1 = _GetPlanForCampaignExport(1, campaignId);
            plan1.Equivalized = true;
            plan1.CreativeLengths = new List<CreativeLength>
            {
                new CreativeLength { SpotLengthId = 9, Weight = 10},
                new CreativeLength { SpotLengthId = 4, Weight = 2},
                new CreativeLength { SpotLengthId = 1, Weight = 75},
                new CreativeLength { SpotLengthId = 6, Weight = 13}
            };
            _PlanRepositoryMock
                .Setup(x => x.GetPlan(1, null))
                .Returns(plan1);

            var plan2 = _GetPlanForCampaignExport(2, campaignId);
            plan2.CreativeLengths = new List<CreativeLength>
            {
                new CreativeLength { SpotLengthId = 1, Weight = 10},
                new CreativeLength { SpotLengthId = 3, Weight = 80},
                new CreativeLength { SpotLengthId = 9, Weight = 10}
            };
            _PlanRepositoryMock
                .Setup(x => x.GetPlan(2, null))
                .Returns(plan2);

            var plan3 = _GetPlanForCampaignExport(3, campaignId);
            plan3.Equivalized = true;
            plan3.CreativeLengths = new List<CreativeLength>
            {
                new CreativeLength { SpotLengthId = 5, Weight = 27},
                new CreativeLength { SpotLengthId = 2, Weight = 19},
                new CreativeLength { SpotLengthId = 9, Weight = 53},
                new CreativeLength { SpotLengthId = 3, Weight = 1}
            };
            _PlanRepositoryMock
                .Setup(x => x.GetPlan(3, null))
                .Returns(plan3);

            _SpotLengthServiceMock
                .Setup(x => x.GetAllSpotLengths())
                .Returns(_GetAllSpotLengths());

            _DaypartDefaultServiceMock
                .Setup(s => s.GetAllDaypartDefaults())
                .Returns(_GetDaypartDefaults());

            _QuarterCalculationEngineMock
                .Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<QuarterDetailDto>
                    {
                        new QuarterDetailDto { Quarter = 1, Year = 2020,
                            StartDate = new DateTime(2020,01,01),
                            EndDate = new DateTime(2020,03,31)
                        },
                        new QuarterDetailDto { Quarter = 2, Year = 2020,
                            StartDate = new DateTime(2020,04,01),
                            EndDate = new DateTime(2020,06,30)}
                    });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterRangeByDate(It.IsAny<DateTime>()))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 1,
                    Year = 2020
                });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterDetail(1, 2020))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 1,
                    Year = 2020,

                });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterDetail(2, 2020))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 2,
                    Year = 2020
                });

            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetTheSameMediaWeeksAsThePlan(plan1));
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksInRange(new DateTime(2020, 01, 01), new DateTime(2020, 03, 31)))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(846, "2020-03-09", "2020-03-15", 462),
                    _GetMediaWeek(847, "2020-03-16", "2020-03-22", 462),
                    _GetMediaWeek(848, "2020-03-23", "2020-03-29", 462)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksInRange(new DateTime(2020, 04, 01), new DateTime(2020, 06, 30)))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(849, "2020-03-30", "2020-04-05", 463),
                    _GetMediaWeek(850, "2020-04-06", "2020-04-12", 463)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByMediaMonth(462))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(846, "2020-03-09", "2020-03-15", 462),
                    _GetMediaWeek(847, "2020-03-16", "2020-03-22", 462),
                    _GetMediaWeek(848, "2020-03-23", "2020-03-29", 462)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByMediaMonth(463))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(849, "2020-03-30", "2020-04-05", 463),
                    _GetMediaWeek(850, "2020-04-06", "2020-04-12", 463)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaMonthsBetweenDatesInclusive(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<MediaMonth> {
                    new MediaMonth{Id = 463, MediaMonthX = "April"}, new MediaMonth{ Id = 462, MediaMonthX = "March"}
                });

            _TrafficApiCacheMock
                .Setup(x => x.GetAgency(It.IsAny<int>()))
                .Returns(new AgencyDto { Id = 1, Name = "Name1" });
            _TrafficApiCacheMock
                .Setup(x => x.GetAdvertiser(It.IsAny<int>()))
                .Returns(new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 });

            _AudienceServiceMock
                .Setup(x => x.GetAudienceById(It.IsAny<int>()))
                .Returns(new PlanAudienceDisplay
                {
                    Code = "A18-20",
                    Id = 1
                });
            _AudienceServiceMock
                .Setup(x => x.GetAudiences())
                .Returns(new List<PlanAudienceDisplay> {
                    new PlanAudienceDisplay
                    {
                        Code = "A18-20",
                        Id = 1
                    }
                });

            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2020, 01, 01));

            var tc = _BuildCampaignService();

            // Act
            var response = tc.GetAndValidateCampaignReportData(request);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_DuplicateProgramName()
        {
            // Arrange
            const int planId = 1853;
            const int campaignId = 652;
            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                ExportType = CampaignExportTypeEnum.Contract,
                SelectedPlans = new List<int> { planId }
            };
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(_GetWeeklyBreakdownByWeek());
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GetWeeklyBreakdownCombinations(It.IsAny<List<CreativeLength>>(), It.IsAny<List<PlanDaypartDto>>()))
                .Returns(_GetWeeklyBreakdownCombinations());
            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(_GetCampaignForExport(campaignId, request.SelectedPlans));
            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });
            var planDto = _GetPlanForCampaignExport(planId, campaignId);
            planDto.Dayparts.First().Restrictions.ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
            {
                ContainType = ContainTypeEnum.Exclude,
                Programs = new List<Entities.DTO.Program.ProgramDto>
                {
                    new Entities.DTO.Program.ProgramDto
                    {
                         ContentRating = "PG",
                          Name = "Mom",
                           Genre = new LookupDto{ Id = 9 }
                    },
                    new Entities.DTO.Program.ProgramDto
                    {
                         ContentRating = "PG",
                          Name = "Mom",
                           Genre = new LookupDto{ Id = 53 }
                    }
                }
            };
            _PlanRepositoryMock
                .Setup(x => x.GetPlan(It.IsAny<int>(), null))
                .Returns(planDto);
            _SpotLengthServiceMock
                .Setup(x => x.GetAllSpotLengths())
                .Returns(_GetAllSpotLengths());
            _DaypartDefaultServiceMock
                .Setup(s => s.GetAllDaypartDefaults())
                .Returns(_GetDaypartDefaults());
            _QuarterCalculationEngineMock
                .Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<QuarterDetailDto>
                    {
                        new QuarterDetailDto { Quarter = 1, Year = 2020,
                            StartDate = new DateTime(2020,01,01),
                            EndDate = new DateTime(2020,03,31)
                        },
                        new QuarterDetailDto { Quarter = 2, Year = 2020,
                            StartDate = new DateTime(2020,04,01),
                            EndDate = new DateTime(2020,06,30)}
                    });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterRangeByDate(It.IsAny<DateTime>()))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 1,
                    Year = 2020
                });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterDetail(1, 2020))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 1,
                    Year = 2020,
                });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterDetail(2, 2020))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 2,
                    Year = 2020
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetTheSameMediaWeeksAsThePlan(planDto));
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksInRange(new DateTime(2020, 01, 01), new DateTime(2020, 03, 31)))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(846, "2020-03-09", "2020-03-15", 462),
                    _GetMediaWeek(847, "2020-03-16", "2020-03-22", 462),
                    _GetMediaWeek(848, "2020-03-23", "2020-03-29", 462)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksInRange(new DateTime(2020, 04, 01), new DateTime(2020, 06, 30)))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(849, "2020-03-30", "2020-04-05", 463),
                    _GetMediaWeek(850, "2020-04-06", "2020-04-12", 463)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByMediaMonth(462))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(846, "2020-03-09", "2020-03-15", 462),
                    _GetMediaWeek(847, "2020-03-16", "2020-03-22", 462),
                    _GetMediaWeek(848, "2020-03-23", "2020-03-29", 462)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByMediaMonth(463))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(849, "2020-03-30", "2020-04-05", 463),
                    _GetMediaWeek(850, "2020-04-06", "2020-04-12", 463)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaMonthsBetweenDatesInclusive(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<MediaMonth> {
                    new MediaMonth{Id = 463, MediaMonthX = "April"}, new MediaMonth{ Id = 462, MediaMonthX = "March"}
                });
            _TrafficApiCacheMock
                .Setup(x => x.GetAgency(It.IsAny<int>()))
                .Returns(new AgencyDto { Id = 1, Name = "Name1" });
            _TrafficApiCacheMock
                .Setup(x => x.GetAdvertiser(It.IsAny<int>()))
                .Returns(new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 });
            _AudienceServiceMock
                .Setup(x => x.GetAudienceById(It.IsAny<int>()))
                .Returns(new PlanAudienceDisplay
                {
                    Code = "A18-20",
                    Id = 1
                });
            _AudienceServiceMock
                .Setup(x => x.GetAudiences())
                .Returns(new List<PlanAudienceDisplay> {
                    new PlanAudienceDisplay
                    {
                        Code = "A18-20",
                        Id = 1
                    }
                });
            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2020, 01, 01));
            var tc = _BuildCampaignService();
            // Act
            var response = tc.GetAndValidateCampaignReportData(request);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_CustomDaypart()
        {
            // Arrange
            const int planId1 = 1;
            const int planId2 = 2;
            const int campaignId = 652;
            var request = new CampaignReportRequest
            {
                CampaignId = campaignId,
                ExportType = CampaignExportTypeEnum.Contract,
                SelectedPlans = new List<int> { planId1, planId2 }
            };
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GroupWeeklyBreakdownByWeek(It.IsAny<IEnumerable<WeeklyBreakdownWeek>>()
                    , It.IsAny<double>(), It.IsAny<List<CreativeLength>>(), It.IsAny<bool>()))
                .Returns(_GetWeeklyBreakdownByWeek());
            _WeeklyBreakdownEngineMock
                .Setup(x => x.GetWeeklyBreakdownCombinations(It.IsAny<List<CreativeLength>>(), It.IsAny<List<PlanDaypartDto>>()))
                .Returns(_GetWeeklyBreakdownCombinations());
            _CampaignRepositoryMock
                .Setup(x => x.GetCampaign(campaignId))
                .Returns(_GetCampaignForExport(campaignId, request.SelectedPlans));
            _LockingManagerApplicationServiceMock
                .Setup(x => x.GetLockObject(It.IsAny<string>()))
                .Returns(new LockResponse
                {
                    Success = true
                });
            var planDto = _GetPlanForCampaignExport(planId1, campaignId);
            planDto.Dayparts.First().Restrictions.ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
            {
                ContainType = ContainTypeEnum.Exclude,
                Programs = new List<Entities.DTO.Program.ProgramDto>
                {
                    new Entities.DTO.Program.ProgramDto
                    {
                         ContentRating = "PG",
                          Name = "Mom",
                           Genre = new LookupDto{ Id = 9 }
                    },
                    new Entities.DTO.Program.ProgramDto
                    {
                         ContentRating = "PG",
                          Name = "Mom",
                           Genre = new LookupDto{ Id = 53 }
                    }
                }
            };

            //change daypart start/end time
            var planDto2 = _GetPlanForCampaignExport(planId2, campaignId);
            planDto2.Dayparts.First().StartTimeSeconds = 42000;
            planDto2.Dayparts.First().EndTimeSeconds = 45000;

            _PlanRepositoryMock
                .Setup(x => x.GetPlan(1, null))
                .Returns(planDto);
            _PlanRepositoryMock
                .Setup(x => x.GetPlan(2, null))
                .Returns(planDto2);
            _SpotLengthServiceMock
                .Setup(x => x.GetAllSpotLengths())
                .Returns(_GetAllSpotLengths());
            _DaypartDefaultServiceMock
                .Setup(s => s.GetAllDaypartDefaults())
                .Returns(_GetDaypartDefaults());
            _QuarterCalculationEngineMock
                .Setup(s => s.GetAllQuartersBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<QuarterDetailDto>
                    {
                        new QuarterDetailDto { Quarter = 1, Year = 2020,
                            StartDate = new DateTime(2020,01,01),
                            EndDate = new DateTime(2020,03,31)
                        },
                        new QuarterDetailDto { Quarter = 2, Year = 2020,
                            StartDate = new DateTime(2020,04,01),
                            EndDate = new DateTime(2020,06,30)}
                    });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterRangeByDate(It.IsAny<DateTime>()))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 1,
                    Year = 2020
                });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterDetail(1, 2020))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 1,
                    Year = 2020,
                });
            _QuarterCalculationEngineMock
                .Setup(s => s.GetQuarterDetail(2, 2020))
                .Returns(new QuarterDetailDto
                {
                    Quarter = 2,
                    Year = 2020
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByFlight(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(_GetTheSameMediaWeeksAsThePlan(planDto));
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksInRange(new DateTime(2020, 01, 01), new DateTime(2020, 03, 31)))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(846, "2020-03-09", "2020-03-15", 462),
                    _GetMediaWeek(847, "2020-03-16", "2020-03-22", 462),
                    _GetMediaWeek(848, "2020-03-23", "2020-03-29", 462)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksInRange(new DateTime(2020, 04, 01), new DateTime(2020, 06, 30)))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(849, "2020-03-30", "2020-04-05", 463),
                    _GetMediaWeek(850, "2020-04-06", "2020-04-12", 463)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByMediaMonth(462))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(846, "2020-03-09", "2020-03-15", 462),
                    _GetMediaWeek(847, "2020-03-16", "2020-03-22", 462),
                    _GetMediaWeek(848, "2020-03-23", "2020-03-29", 462)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaWeeksByMediaMonth(463))
                .Returns(new List<MediaWeek> {
                    _GetMediaWeek(849, "2020-03-30", "2020-04-05", 463),
                    _GetMediaWeek(850, "2020-04-06", "2020-04-12", 463)
                });
            _MediaMonthAndWeekAggregateCacheMock
                .Setup(s => s.GetMediaMonthsBetweenDatesInclusive(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<MediaMonth> {
                    new MediaMonth{Id = 463, MediaMonthX = "April"}, new MediaMonth{ Id = 462, MediaMonthX = "March"}
                });
            _TrafficApiCacheMock
                .Setup(x => x.GetAgency(It.IsAny<int>()))
                .Returns(new AgencyDto { Id = 1, Name = "Name1" });
            _TrafficApiCacheMock
                .Setup(x => x.GetAdvertiser(It.IsAny<int>()))
                .Returns(new AdvertiserDto { Id = 2, Name = "Name2", AgencyId = 1 });
            _AudienceServiceMock
                .Setup(x => x.GetAudienceById(It.IsAny<int>()))
                .Returns(new PlanAudienceDisplay
                {
                    Code = "A18-20",
                    Id = 1
                });
            _AudienceServiceMock
                .Setup(x => x.GetAudiences())
                .Returns(new List<PlanAudienceDisplay> {
                    new PlanAudienceDisplay
                    {
                        Code = "A18-20",
                        Id = 1
                    }
                });
            _DateTimeEngineMock
                .Setup(x => x.GetCurrentMoment())
                .Returns(new DateTime(2020, 01, 01));
            var tc = _BuildCampaignService();
            // Act
            var response = tc.GetAndValidateCampaignReportData(request);
            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response));
        }

        private List<WeeklyBreakdownCombination> _GetWeeklyBreakdownCombinations()
        {
            return new List<WeeklyBreakdownCombination> {
                    new WeeklyBreakdownCombination{ SpotLengthId = 1, DaypartCodeId = 2, Weighting = 0.3},
                    new WeeklyBreakdownCombination{ SpotLengthId = 1, DaypartCodeId = 6, Weighting = 0.2},
                    new WeeklyBreakdownCombination{ SpotLengthId = 1, DaypartCodeId = 14, Weighting = 0.2},
                    new WeeklyBreakdownCombination{ SpotLengthId = 2, DaypartCodeId = 2, Weighting = 0.15},
                    new WeeklyBreakdownCombination{ SpotLengthId = 2, DaypartCodeId = 6, Weighting = 0.1},
                    new WeeklyBreakdownCombination{ SpotLengthId = 2, DaypartCodeId = 14, Weighting = 0.2},
                    new WeeklyBreakdownCombination{ SpotLengthId = 3, DaypartCodeId = 2, Weighting = 0.15},
                    new WeeklyBreakdownCombination{ SpotLengthId = 3, DaypartCodeId = 6, Weighting = 0.1},
                    new WeeklyBreakdownCombination{ SpotLengthId = 3, DaypartCodeId = 14, Weighting = 0.2}
                };
        }

        private static MediaWeek _GetMediaWeek(int id, string weekStartDate = null, string weekEndDate = null, int? mediaMonthId = null)
        {
            return new MediaWeek
            {
                Id = id,
                StartDate = weekStartDate == null ? new DateTime() : Convert.ToDateTime(weekStartDate),
                EndDate = weekEndDate == null ? new DateTime() : Convert.ToDateTime(weekEndDate),
                MediaMonthId = mediaMonthId ?? 0
            };
        }

        private List<MediaWeek> _GetTheSameMediaWeeksAsThePlan(PlanDto planDto)
        {
            var result = new List<MediaWeek>();
            planDto.WeeklyBreakdownWeeks.Select(x => x.MediaWeekId)
                .ToList()
                .ForEach(mediaWeekId => result.Add(
                    _GetMediaWeek(mediaWeekId)));
            return result;
        }

        private List<DaypartDefaultDto> _GetDaypartDefaults()
        {
            return new List<DaypartDefaultDto>
            {
                new DaypartDefaultDto
                {
                    Code = "MDN",
                    Id = 2
                },
                new DaypartDefaultDto
                {
                    Code = "EF",
                    Id = 6
                },
                new DaypartDefaultDto
                {
                    Code = "EM",
                    Id = 14
                },
            };
        }

        private void _SetupBaseProgramLineupForRollupTestData()
        {
            _PlanRepositoryMock
                .Setup(x => x.GetLatestPricingJob(It.IsAny<int>()))
                .Returns(new PlanPricingJob
                {
                    Status = BackgroundJobProcessingStatus.Succeeded,
                    Completed = new DateTime(2020, 02, 12)
                });

            _PlanRepositoryMock
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanId(It.IsAny<int>()))
                .Returns(_GetPlanPricingAllocatedSpotsForRollup());

            _InventoryRepositoryMock
                .Setup(x => x.GetStationInventoryManifestsByIds(It.IsAny<IEnumerable<int>>()))
                .Returns(_GetStationInventoryManifestsForRollup());

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
                .Returns(_GetAllSpotLengths());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoveragesWithStations())
                .Returns(_GetMarketCoverages());

            _StationProgramRepositoryMock
                .Setup(x => x.GetPrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, Program>
                {
                    {
                        1001,
                        new Program
                        {
                            Genre = "News",
                            Name = "KPLR 1001 Morning NEWS"
                        }
                    },
                    {
                        1002,
                        new Program
                        {
                            Genre = "News",
                            Name = "KPLR 1002 Morning NEWS"
                        }
                    },
                    {
                        2001,
                        new Program
                        {
                            Genre = "News",
                            Name = "KPLR 2001 Midday NEWS"
                        }
                    },
                     {
                        2002,
                        new Program
                        {
                            Genre = "News",
                            Name = "KPLR 2002 Midday NEWS"
                        }
                    },
                     {
                        3001,
                        new Program
                        {
                            Genre = "News",
                            Name = "KPLR 3001 Evening NEWS"
                        }
                    },
                     {
                        3002,
                        new Program
                        {
                            Genre = "News",
                            Name = "KPLR 3002 Evening NEWS"
                        }
                    },
                     {
                        4001,
                        new Program
                        {
                            Genre = "News",
                            Name = "KPLR 4001 Late NEWS"
                        }
                    },
                     {
                        4002,
                        new Program
                        {
                            Genre = "News",
                            Name = "KPLR 4002 Late NEWS"
                        }
                    },
                });
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
                .Setup(x => x.GetPlanPricingAllocatedSpotsByPlanId(It.IsAny<int>()))
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
                .Returns(_GetAllSpotLengths());

            _MarketCoverageRepositoryMock
                .Setup(x => x.GetLatestMarketCoveragesWithStations())
                .Returns(_GetMarketCoverages());

            _StationProgramRepositoryMock
                .Setup(x => x.GetPrimaryProgramsForManifestDayparts(It.IsAny<IEnumerable<int>>()))
                .Returns(new Dictionary<int, Program>
                {
                    {
                        1001,
                        new Program
                        {
                            Genre = "Movie",
                            Name = "Joker"
                        }
                    },
                    {
                        2001,
                        new Program
                        {
                            Genre = "News",
                            Name = "Late News"
                        }
                    },
                    {
                        3001,
                        new Program
                        {
                            Genre = "Movie",
                            Name = "1917"
                        }
                    }
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

        private List<PlanPricingAllocatedSpot> _GetPlanPricingAllocatedSpotsForRollup()
        {
            return new List<PlanPricingAllocatedSpot>
            {
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 10,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 10001,
                        Code = "EMN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 10,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 10002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 20,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 20001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 20
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 20,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 20002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 20
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 30,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 30001,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 30,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 30002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 40,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 40001,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 10
                },new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 40,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 40002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 10
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
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 10001,
                        Code = "EMN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 20,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 20001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 20
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 30,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 30002,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 2
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 40,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 40001,
                        Code = "EM"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 1
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 50,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 50001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 5
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 60,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 60001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 5
                        }
                    },
                    Impressions30sec = 10
                },
                new PlanPricingAllocatedSpot
                {
                    StationInventoryManifestId = 70,
                    StandardDaypart = new DaypartDefaultDto
                    {
                        Id = 60001,
                        Code = "LN"
                    },
                    SpotFrequencies = new List<SpotFrequency>
                    {
                        new SpotFrequency
                        {
                            Spots = 5
                        }
                    },
                    Impressions30sec = 10
                }
            };
        }

        private List<StationInventoryManifest> _GetStationInventoryManifestsForRollup()
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
                                    StartTime = 14400,
                                    EndTime = 35999,
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
                                    StartTime = 39600,
                                    EndTime = 46799,
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
                                ProgramName = "The Shawshank Redemption",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 30001,
                                    StartTime = 57600,
                                    EndTime = 68399,
                                    Friday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 40,
                        Station = new DisplayBroadcastStation
                        {
                            MarketCode = 300,
                            LegacyCallLetters = "KELOW",
                            Affiliation = "CBS"
                        },
                        ManifestDayparts = new List<StationInventoryManifestDaypart>
                        {
                            new StationInventoryManifestDaypart
                            {
                                Id = 4001,
                                Daypart = new DisplayDaypart
                                {
                                    Id = 40001,
                                    StartTime = 72000,
                                    EndTime = 299,
                                    Friday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
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
                                ProgramName = "The Shawshank Redemption",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 30001,
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
                    },
                    new StationInventoryManifest
                    {
                        Id = 60,
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
                                Id = 6001,
                                ProgramName = "Fallback program",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 60001,
                                    StartTime = 200,
                                    EndTime = 299,
                                    Saturday = true
                                }
                            }
                        }
                    },
                    new StationInventoryManifest
                    {
                        Id = 70,
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
                                Id = 6001,
                                ProgramName = "Fallback program",
                                Daypart = new DisplayDaypart
                                {
                                    Id = 60001,
                                    StartTime = 200,
                                    EndTime = 299,
                                    Saturday = true,
                                    Sunday = true
                                }
                            }
                        }
                    },
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
                _DateTimeEngineMock.Object,
                _WeeklyBreakdownEngineMock.Object);
        }

        private CampaignDto _GetCampaignForExport(int campaignId, List<int> selectedPlanIds)
        {
            var campaign = new CampaignDto
            {
                Id = campaignId,
                Plans = new List<PlanSummaryDto>()
            };
            selectedPlanIds.ForEach(id => campaign.Plans.Add(
                new PlanSummaryDto
                {
                    PlanId = id,
                    ProcessingStatus = PlanAggregationProcessingStatusEnum.Idle,
                    Status = PlanStatusEnum.Complete
                }));
            return campaign;
        }

        private PlanDto _GetPlan(int planId, int campaignId)
        {
            return new PlanDto
            {
                Id = planId,
                CampaignId = campaignId,
                Dayparts = _GetPlanDayparts()
            };
        }

        private PlanDto _GetPlanForCampaignExport(int planId, int campaignId)
        {
            var planDto = new PlanDto
            {
                Id = planId,
                CampaignId = campaignId,
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength
                    {
                        SpotLengthId = 1, //30
                        Weight = 50
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 2, //60
                        Weight = 40
                    },
                    new CreativeLength
                    {
                        SpotLengthId = 3, //15
                        Weight = 10
                    }
                },
                FlightStartDate = new DateTime(2020, 03, 14),
                FlightEndDate = new DateTime(2020, 04, 12),
                TargetImpressions = 20000000,
                ImpressionsPerUnit = 4000000,
                TargetRatingPoints = 149.446003664416,
                HHImpressions = 40000000,
                HHRatingPoints = 36.2916732747755,
                AudienceId = 1,
                Vpvh = 0.5,
                Budget = 500000.00M,
                PostingType = PostingTypeEnum.NTI,
                CoverageGoalPercent = 1,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { Id = 1, ShareOfVoicePercent = 1, Market = "Some Market"}
                },
                FlightDays = new List<int> { 3, 4, 5, 6, 7 },
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 2,
                        WeightingGoalPercent = 20,
                        StartTimeSeconds = 39600,
                        EndTimeSeconds = 46799,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 1,
                                Vpvh = 0.4,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2020, 2, 10)
                            }
                        }
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 6,
                        WeightingGoalPercent = 40,
                        StartTimeSeconds = 54000,
                        EndTimeSeconds = 64799,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 1,
                                Vpvh = 0.5,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2020, 2, 10)
                            }
                        }
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 14,
                        WeightingGoalPercent = null,
                        StartTimeSeconds = 21600,
                        EndTimeSeconds = 32399,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 1,
                                Vpvh = 0.6,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2020, 2, 10)
                            }
                        }
                    }
                }                
            };
            planDto.WeeklyBreakdownWeeks = _GetWeeklyBreakdownWeeks(planDto, new List<int> { 846, 847, 848, 849, 850 });
            return planDto;
        }

        private List<WeeklyBreakdownWeek> _GetWeeklyBreakdownWeeks(PlanDto planDto, List<int> weekIds)
        {
            List<WeeklyBreakdownWeek> result = new List<WeeklyBreakdownWeek>();
            foreach(int id in weekIds)
            {
                foreach (var c in planDto.CreativeLengths)
                {
                    foreach (var d in planDto.Dayparts)
                    {
                        result.Add(new WeeklyBreakdownWeek
                        {
                            MediaWeekId = id,
                            DaypartCodeId = d.DaypartCodeId,
                            SpotLengthId = c.SpotLengthId,
                            WeeklyAdu = 0,
                            WeeklyImpressions = 4000000,
                            WeeklyBudget = 100000.00M,
                            WeeklyRatings = 29.8892007328832,
                            UnitImpressions = 2000000
                        });
                    }
                }
            }
            return result;
        }

        private List<WeeklyBreakdownByWeek> _GetWeeklyBreakdownByWeek()
        {
            return new List<WeeklyBreakdownByWeek>
            {
                new WeeklyBreakdownByWeek
                {
                    MediaWeekId = 846,
                    Impressions = 4000000,
                    Budget = 100000.00M
                },
                new WeeklyBreakdownByWeek
                {
                    MediaWeekId = 847,
                    Impressions = 4000000,
                    Budget = 100000.00M
                },
                new WeeklyBreakdownByWeek
                {
                    MediaWeekId = 848,
                    Impressions = 4000000,
                    Budget = 100000.00M
                },
                new WeeklyBreakdownByWeek
                {
                    MediaWeekId = 849,
                    Impressions = 4000000,
                    Budget = 100000.00M
                },
                new WeeklyBreakdownByWeek
                {
                    MediaWeekId = 850,
                    Impressions = 4000000,
                    Budget = 100000.00M,
                }
            };
        }

        private List<LookupDto> _GetAllSpotLengths()
        {
            return new List<LookupDto>
                {
                new LookupDto { Id = 1, Display = "30" },
                new LookupDto { Id = 2, Display = "60" },
                new LookupDto { Id = 3, Display = "15" },
                new LookupDto { Id = 4, Display = "120" },
                new LookupDto { Id = 5, Display = "180" },
                new LookupDto { Id = 6, Display = "300" },
                new LookupDto { Id = 7, Display = "90" },
                new LookupDto { Id = 8, Display = "45" },
                new LookupDto { Id = 9, Display = "10" },
                new LookupDto { Id = 10, Display = "150" }
                };
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
    }
}
