using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Linq;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.ContractInterfaces;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.IntegrationTests.Stubs;
using System.IO;
using Common.Services;
using Services.Broadcast.ReportGenerators.ProgramLineup;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.ReportGenerators.CampaignExport;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class CampaignServiceIntegrationTests
    {
        private const string IntegrationTestUser = "IntegrationTestUser";
        private readonly DateTime CreatedDate = new DateTime(2019, 5, 14);
        private readonly ICampaignService _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();
        private readonly IPlanPricingService _PlanPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
        private readonly ICampaignSummaryRepository _CampaignSummaryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ICampaignSummaryRepository>();
        private readonly IPlanRepository _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
        private static readonly bool WRITE_FILE_TO_DISK = false;

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCampaignsTest()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var campaigns = _CampaignService.GetCampaigns(new CampaignFilterDto
                {
                    Quarter = new QuarterDto
                    {
                        Quarter = 3,
                        Year = 2019
                    },
                    PlanStatus = PlanStatusEnum.Contracted
                }, new DateTime(2019, 04, 01));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCampaignsFilteredByStatusTest()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var campaigns = _CampaignService.GetCampaigns(new CampaignFilterDto
                {
                    PlanStatus = PlanStatusEnum.Contracted
                }, new DateTime(2019, 04, 01));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns, _GetJsonSettings()));
            }
        }

        [Test]
        public void GetCampaignsFilteredByStatusWorking()
        {
            using (new TransactionScopeWrapper())
            {
                var campaignWithoutSummary = _GetValidCampaignForSave();
                _CampaignService.SaveCampaign(campaignWithoutSummary, IntegrationTestUser, new DateTime(2017, 11, 20));

                var campaignWithoutCampaignStatus = _GetValidCampaignForSave();
                var campaignIdWithoutCampaignStatus = _CampaignService.SaveCampaign(campaignWithoutCampaignStatus, IntegrationTestUser, new DateTime(2017, 11, 20));

                var summaryWithoutCampaignStatus = GetSummary(campaignIdWithoutCampaignStatus, IntegrationTestUser, CreatedDate);
                summaryWithoutCampaignStatus.CampaignStatus = null;
                summaryWithoutCampaignStatus.FlightStartDate = new DateTime(2017, 11, 1);
                summaryWithoutCampaignStatus.FlightStartDate = new DateTime(2017, 11, 11);
                _CampaignSummaryRepository.SaveSummary(summaryWithoutCampaignStatus);

                var campaignWithWorkingStatus = _GetValidCampaignForSave();
                var campaignIdWithWorkingStatus = _CampaignService.SaveCampaign(campaignWithWorkingStatus, IntegrationTestUser, new DateTime(2017, 11, 20));

                var summaryWithWorkingStatus = GetSummary(campaignIdWithWorkingStatus, IntegrationTestUser, CreatedDate);
                summaryWithWorkingStatus.CampaignStatus = PlanStatusEnum.Working;
                summaryWithWorkingStatus.FlightStartDate = new DateTime(2017, 11, 1);
                summaryWithWorkingStatus.FlightStartDate = new DateTime(2017, 11, 11);
                _CampaignSummaryRepository.SaveSummary(summaryWithWorkingStatus);

                var campaigns = _CampaignService.GetCampaigns(new CampaignFilterDto
                {
                    PlanStatus = PlanStatusEnum.Working,
                    Quarter = new QuarterDto
                    {
                        Quarter = 4,
                        Year = 2017
                    }
                }, new DateTime(2017, 04, 01));

                Assert.IsTrue(campaigns.All(c => c.CampaignStatus == PlanStatusEnum.Working));
                Assert.AreEqual(3, campaigns.Count);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCampaignById()
        {
            // Data already exists for campaign id 2 : campaign, summary, plan
            var campaignId = 2;
            using (new TransactionScopeWrapper())
            {
                var foundCampaign = _CampaignService.GetCampaignById(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(foundCampaign, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCampaignById_WithoutPlans()
        {
            // Data already exists for campaign id 5 : campaign
            var campaignId = 5;
            using (new TransactionScopeWrapper())
            {
                var foundCampaign = _CampaignService.GetCampaignById(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(foundCampaign, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignValidCampaignTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaignForSave();

                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                Assert.IsTrue(campaignId > 0);
            }
        }

        [Test]
        public void CanNotUpdateLockedCampaign()
        {
            const string expectedMessage = "The chosen campaign has been locked by IntegrationUser";

            using (new TransactionScopeWrapper(System.Transactions.IsolationLevel.ReadCommitted))
            {
                var lockingManagerApplicationServiceMock = new Mock<IBroadcastLockingManagerApplicationService>();
                lockingManagerApplicationServiceMock.Setup(x => x.LockObject(It.IsAny<string>())).Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = "IntegrationUser"
                });

                var service = new CampaignService(
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IDataRepositoryFactory>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignValidator>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IMediaMonthAndWeekAggregateCache>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IQuarterCalculationEngine>(),
                    lockingManagerApplicationServiceMock.Object,
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregator>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ITrafficApiCache>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IAudienceService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ISpotLengthService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IDaypartDefaultService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ISharedFolderService>());

                var campaign = _GetValidCampaignForSave();
                campaign.Id = 1;

                var exception = Assert.Throws<ApplicationException>(() => service.SaveCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.AreEqual(expectedMessage, exception.Message);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignNameMaxLengthTest()
        {
            const int maxNameLength = 255;
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaignForSave();
                campaign.Name = StringHelper.CreateStringOfLength(maxNameLength);

                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                Assert.IsTrue(campaignId > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignNotesMaxLengthTest()
        {
            const int maxNotesLength = 1024;
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaignForSave();
                campaign.Notes = StringHelper.CreateStringOfLength(maxNotesLength);

                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                Assert.IsTrue(campaignId > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateCampaignTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaignForSave();
                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);

                _CampaignSummaryRepository.SaveSummary(GetSummary(campaignId, IntegrationTestUser, CreatedDate));
                CampaignDto foundCampaign = _CampaignService.GetCampaignById(campaignId);
                var campaignToSave = new SaveCampaignDto
                {
                    Id = foundCampaign.Id,
                    Name = foundCampaign.Name,
                    AgencyId = foundCampaign.AgencyId,
                    AdvertiserId = foundCampaign.AdvertiserId,
                    Notes = foundCampaign.Notes,
                    ModifiedBy = foundCampaign.ModifiedBy,
                    ModifiedDate = foundCampaign.ModifiedDate
                };

                campaignToSave.Name = "Updated name of Campaign1";
                int updatedCampaignId = _CampaignService.SaveCampaign(campaignToSave, IntegrationTestUser, CreatedDate);
                CampaignDto updatedCampaign = _CampaignService.GetCampaignById(updatedCampaignId);

                Assert.AreEqual(updatedCampaign.Id, campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedCampaign, _GetJsonSettings()));
            }
        }

        private CampaignSummaryDto GetSummary(int campaignId, string createdBy, DateTime createdDate)
        {
            return new CampaignSummaryDto
            {
                QueuedBy = createdBy,
                QueuedAt = createdDate,
                ProcessingStatus = CampaignAggregationProcessingStatusEnum.Completed,
                LastAggregated = new DateTime(2019, 09, 04, 16, 41, 0),
                CampaignId = campaignId,
                CampaignStatus = PlanStatusEnum.Contracted,
                Budget = 1500.0m,
                HHCPM = 2500.0m,
                HHRatingPoints = 23.0,
                HHImpressions = 5500.0,
                FlightStartDate = new DateTime(2019, 08, 01),
                FlightEndDate = new DateTime(2019, 08, 30),
                FlightActiveDays = 26,
                FlightHiatusDays = 4,
                PlanStatusCountWorking = 0,
                PlanStatusCountReserved = 0,
                PlanStatusCountClientApproval = 1,
                PlanStatusCountContracted = 1,
                PlanStatusCountLive = 0,
                PlanStatusCountComplete = 0,
                ComponentsModified = new DateTime(2019, 08, 28, 12, 30, 32)
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignInvalidAdvertiserIdTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaignForSave();

                // Invalid advertiser id.
                campaign.AdvertiserId = 666;

                var exception = Assert.Throws<InvalidOperationException>(() => _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.That(exception.Message, Is.EqualTo(CampaignValidator.InvalidAdvertiserErrorMessage));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetQuartersTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetQuarters(null, new DateTime(2019, 5, 1));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetQuartersWithPlanStatusTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetQuarters(PlanStatusEnum.ClientApproval, new DateTime(2019, 5, 1));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("      ")]
        [TestCase("\t")]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignInvalidCampaignNameTest(string campaignName)
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaignForSave();

                campaign.Name = campaignName;

                var exception = Assert.Throws<InvalidOperationException>(() => _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.That(exception.Message, Is.EqualTo(CampaignValidator.InvalidCampaignNameErrorMessage));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCampaignsWithFilters()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var filter = new CampaignFilterDto
                {
                    PlanStatus = PlanStatusEnum.Contracted,
                    Quarter = new QuarterDto
                    {
                        Quarter = 3,
                        Year = 2019
                    }
                };

                var campaigns = _CampaignService.GetCampaigns(filter, new DateTime(2019, 02, 01));

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStatuses()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetStatuses(3, 2019);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStatuses_CampaignWithoutSummary()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaignForSave();
                var campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);

                var campaigns = _CampaignService.GetStatuses(2, 2019);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStatuses_WithoutParams()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetStatuses(null, null);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStatuses_WithCampaignStatusNull()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaignForSave();
                var campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);

                var summary = GetSummary(campaignId, IntegrationTestUser, CreatedDate);
                summary.CampaignStatus = null;
                _CampaignSummaryRepository.SaveSummary(summary);

                var campaigns = _CampaignService.GetStatuses(null, null);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TriggerCampaignAggregation()
        {
            // Data already exists for campaign id 2 : campaign, summary, plan
            const int campaignId = 2;
            using (new TransactionScopeWrapper())
            {
                _CampaignService.TriggerCampaignAggregationJob(campaignId, IntegrationTestUser);
                var summary = _CampaignSummaryRepository.GetSummaryForCampaign(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(summary, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessCampaignAggregation()
        {
            // Data already exists for campaign id 2 : campaign, summary, plan
            const int campaignId = 2;
            using (new TransactionScopeWrapper())
            {
                _CampaignService.ProcessCampaignAggregation(campaignId);
                var fullCampaign = _CampaignService.GetCampaignById(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(fullCampaign, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessCampaignAggregation_WithoutPlans()
        {
            // Data already exists for campaign id 2 : campaign, summary
            // the intent is to run for a campaign that has no plans.
            const int campaignId = 1;
            using (new TransactionScopeWrapper())
            {
                _CampaignService.ProcessCampaignAggregation(campaignId);
                var fullCampaign = _CampaignService.GetCampaignById(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(fullCampaign));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessAggregation_WithError()
        {
            // this ID should have no backing data.
            const int campaignId = int.MaxValue;
            using (new TransactionScopeWrapper())
            {
                Assert.Throws<Exception>(() => _CampaignService.ProcessCampaignAggregation(campaignId));
            }
        }

        private SaveCampaignDto _GetValidCampaignForSave()
        {
            return new SaveCampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(CampaignDto), "Id");
            jsonResolver.Ignore(typeof(CampaignListItemDto), "Id");
            jsonResolver.Ignore(typeof(PlanSummaryDto), "PlanId");
            jsonResolver.Ignore(typeof(PlanSummaryDto), "VersionId");
            jsonResolver.Ignore(typeof(CampaignSummaryDto), "QueuedAt");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        private static PlanDto _GetNewPlan()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                SpotLengthId = 1,
                Status = PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019,1,20),
                    new DateTime(2019,4,15)
                },
                AudienceId = 31,        //HH
                AudienceType = AudienceTypeEnum.Nielsen,
                HUTBookId = 437,
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,
                Budget = 100m,
                TargetCPM = 12m,
                TargetCPP = 200951583.9999m,
                TargetImpressions = 100d,
                TargetRatingPoints = 6d,
                CoverageGoalPercent = 80.5,
                Currency = PlanCurrenciesEnum.Impressions,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.Even,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 48, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn"},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York"}
                },
                BlackoutMarkets = new List<PlanBlackoutMarketDto>
                {
                    new PlanBlackoutMarketDto {MarketCode = 123, MarketCoverageFileId = 1, PercentageOfUS = 5.5, Rank = 5, Market = "Burlington-Plattsburgh" },
                    new PlanBlackoutMarketDto {MarketCode = 234, MarketCoverageFileId = 1, PercentageOfUS = 2.5, Rank = 8, Market = "Amarillo" },
                },
                ModifiedBy = "Integration test",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto{ DaypartCodeId = 2, StartTimeSeconds = 0, EndTimeSeconds = 2000, WeightingGoalPercent = 28.0 },
                    new PlanDaypartDto{ DaypartCodeId = 11, StartTimeSeconds = 1500, EndTimeSeconds = 2788, WeightingGoalPercent = 33.2 }
                },
                Vpvh = 0.101,
                HHUniverse = 1000000,
                HHImpressions = 10000,
                HHCPM = 0.01m,
                HHRatingPoints = 1,
                HHCPP = 10000,
                TargetUniverse = 3000000,
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_ContractOnlyPlans()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1852, 1853 }
                });

                //write excel file to file system(this is used for manual testing only)
                var reportOutput = new CampaignReportGenerator(@".\Files\Excel templates").Generate(reportData);
                reportOutput.Filename = "CampaignExport_ContractOnlyPlans.xlsx";
                _WriteFileToLocalFileSystem(reportOutput);

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
                Assert.AreEqual(reportOutput.Stream.Length,
                    File.ReadAllBytes(@".\Files\Campaign export\CampaignExport_ContractOnlyPlans.xlsx").LongLength);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_ContractOnlyPlansWithDaypartsSorting()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1852, 2233 }
                });

                //write excel file to file system(this is used for manual testing only)
                var reportOutput = new CampaignReportGenerator(@".\Files\Excel templates").Generate(reportData);
                reportOutput.Filename = "CampaignExport_ContractOnlyPlansWithDaypartsSorting.xlsx";
                _WriteFileToLocalFileSystem(reportOutput);

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
                Assert.AreEqual(reportOutput.Stream.Length,
                    File.ReadAllBytes(@".\Files\Campaign export\CampaignExport_ContractOnlyPlansWithDaypartsSorting.xlsx").LongLength);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_GenerateCampaignReport()
        {
            var now = new DateTime(2020, 1, 1);
            var user = "IntegrationTestsUser";

            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>()));
            var campaignService = _SetupCampaignService(fileServiceMock.Object);
            var fileId = Guid.Empty;
            var sharedFolderRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ISharedFolderFilesRepository>();
            SharedFolderFile sharedFolderFile = null;

            using (new TransactionScopeWrapper())
            {
                fileId = campaignService.GenerateCampaignReport(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1852, 1853 }
                }, user, now, "./Files/Excel templates");

                sharedFolderFile = sharedFolderRepository.GetFileById(fileId);
            }

            Assert.AreNotEqual(Guid.Empty, fileId);

            fileServiceMock.Verify(x => x.Create(
                @"\\cadfs11\Broadcast\IntegrationTests\CampaignExportReports",
                fileId + ".xlsx",
                It.Is<Stream>(y => y != null && y.Length > 0)), Times.Once);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(sharedFolderFile, _GetJsonSettingsForCampaignExport()));
        }

        private ICampaignService _SetupCampaignService(IFileService fileService)
        {
            var sharedFolderService = new SharedFolderService(
                fileService,
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory);

            var campaignService = new CampaignService(
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignValidator>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IMediaMonthAndWeekAggregateCache>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IQuarterCalculationEngine>(),

                IntegrationTestApplicationServiceFactory.Instance.Resolve<IBroadcastLockingManagerApplicationService>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregator>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                new TrafficApiCacheStub(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IAudienceService>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ISpotLengthService>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IDaypartDefaultService>(),
                sharedFolderService);

            return campaignService;
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_ReservedPlanWithConstraints()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 1850, 1851 }
                });

                //write excel file to file system(this is used for manual testing only)
                var reportOutput = new CampaignReportGenerator(@".\Files\Excel templates").Generate(reportData);
                reportOutput.Filename = "CampaignExport_ReservedPlanWithConstraints.xlsx";

                _WriteFileToLocalFileSystem(reportOutput);

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
                Assert.AreEqual(reportOutput.Stream.Length,
                    File.ReadAllBytes(@".\Files\Campaign export\CampaignExport_ReservedPlanWithConstraints.xlsx").LongLength);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_ContractTypeWithRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1850, 1851, 1852, 1853 }
                });

                //write excel file to file system(this is used for manual testing only)
                var reportOutput = new CampaignReportGenerator(@".\Files\Excel templates").Generate(reportData);
                reportOutput.Filename = "CampaignExport_ContractTypeWithRestrictions.xlsx";

                _WriteFileToLocalFileSystem(reportOutput);

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
                Assert.AreEqual(reportOutput.Stream.Length,
                    File.ReadAllBytes(@".\Files\Campaign export\CampaignExport_ContractTypeWithRestrictions.xlsx").LongLength);
            }
        }

        [Test]
        public void CampaignExport_ValidateExportType_Contracted()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var exception = Assert.Throws<ApplicationException>(() =>
                _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 1852, 1853, 1854 }
                }));
                Assert.That(exception.Message, Is.EqualTo("Invalid export type for selected plans."));
            }
        }

        [Test]
        public void CampaignExport_ValidateGuaranteedAudience()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var exception = Assert.Throws<ApplicationException>(() =>
                _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 1854, 1855 }
                }));
                Assert.That(exception.Message, Is.EqualTo("Cannot have multiple guaranteed audiences in the export. Please select only plans with the same guaranteed audience."));
            }
        }

        [Test]
        public void CampaignExport_ValidateSecondaryAudiences()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var exception = Assert.Throws<ApplicationException>(() =>
                _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 1848, 2541 }
                }));
                Assert.That(exception.Message, Is.EqualTo("Cannot have multiple plans with varying secondary audiences in the export. Please select only plans with the same secondary audiences."));
            }
        }

        [Test]
        public void CampaignExport_ValidateExportType_ContractedWithProposal()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var exception = Assert.Throws<ApplicationException>(() =>
                _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 1851, 1849, 1852, 1853, 1854 }
                }));
                Assert.That(exception.Message, Is.EqualTo("Invalid export type for selected plans."));
            }
        }

        [Test]
        public void CampaignExport_ValidateExportType_ContractedWithOther()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var exception = Assert.Throws<ApplicationException>(() =>
                _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1853, 1856 }
                }));
                Assert.That(exception.Message, Is.EqualTo("Invalid export type for selected plans."));
            }
        }

        [Test]
        public void CampaignExport_ValidateExportType_ProposalOrOther()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var exception = Assert.Throws<ApplicationException>(() =>
                _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1849, 1856 }
                }));
                Assert.That(exception.Message, Is.EqualTo("Invalid export type for selected plans."));
            }
        }

        [Test]
        public void CampaignExport_ValidateExportType_Other()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var exception = Assert.Throws<ApplicationException>(() =>
                _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1855, 1856 }
                }));
                Assert.That(exception.Message, Is.EqualTo("Invalid export type for selected plans."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_SecondaryAudiences_SinglePlan()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 1848 }
                });

                //write excel file to file system(this is used for manual testing only)
                var reportOutput = new CampaignReportGenerator(@".\Files\Excel templates").Generate(reportData);
                reportOutput.Filename = "CampaignExport_SecondaryAudiences_SinglePlan.xlsx";

                _WriteFileToLocalFileSystem(reportOutput);

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
                Assert.AreEqual(reportOutput.Stream.Length,
                    File.ReadAllBytes(@".\Files\Campaign export\CampaignExport_SecondaryAudiences_SinglePlan.xlsx").LongLength);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_SecondaryAudiences_MultiplePlans()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 1848, 2052 }
                });

                //write excel file to file system(this is used for manual testing only)
                var reportOutput = new CampaignReportGenerator(@".\Files\Excel templates").Generate(reportData);
                reportOutput.Filename = "CampaignExport_SecondaryAudiences_MultiplePlans.xlsx";

                _WriteFileToLocalFileSystem(reportOutput);

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
                Assert.AreEqual(reportOutput.Stream.Length,
                    File.ReadAllBytes(@".\Files\Campaign export\CampaignExport_SecondaryAudiences_MultiplePlans.xlsx").LongLength);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_PlansWith13And14Weeks()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 2190, 2191, 2192, 2193 }
                });

                //write excel file to file system(this is used for manual testing only)
                var reportOutput = new CampaignReportGenerator(@".\Files\Excel templates").Generate(reportData);
                reportOutput.Filename = "CampaignExport_PlansWith13And14Weeks.xlsx";
                _WriteFileToLocalFileSystem(reportOutput);
                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
                Assert.AreEqual(reportOutput.Stream.Length,
                    File.ReadAllBytes(@".\Files\Campaign export\CampaignExport_PlansWith13And14Weeks.xlsx").LongLength);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CampaignExport_DuplicateHiatusDaysOnFlowChart()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 2541, 2579 }
                });
                                
                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        private JsonSerializerSettings _GetJsonSettingsForCampaignExport()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(CampaignReportData), "CreatedDate");
            jsonResolver.Ignore(typeof(SharedFolderFile), "Id");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProgramLineupExport()
        {
            const int planId = 1197;
            var now = new DateTime(2020, 4, 4);

            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                var planPricingRequestDto = new PlanPricingParametersDto
                {
                    PlanId = planId,
                    MaxCpm = 100m,
                    MinCpm = 1m,
                    Budget = 1000,
                    CompetitionFactor = 0.1,
                    CPM = 5m,
                    DeliveryImpressions = 50000,
                    InflationFactor = 0.5,
                    ProprietaryBlend = 0.2,
                    UnitCaps = 10,
                    UnitCapsType = UnitCapEnum.PerDay,
                    InventorySourcePercentages = new List<PlanPricingInventorySourceDto>
                    {
                        new PlanPricingInventorySourceDto{Id = 3, Percentage = 12},
                        new PlanPricingInventorySourceDto{Id = 5, Percentage = 13},
                        new PlanPricingInventorySourceDto{Id = 6, Percentage = 14},
                        new PlanPricingInventorySourceDto{Id = 7, Percentage = 15},
                        new PlanPricingInventorySourceDto{Id = 10, Percentage = 16},
                        new PlanPricingInventorySourceDto{Id = 11, Percentage = 17},
                        new PlanPricingInventorySourceDto{Id = 12, Percentage = 8},
                    }
                };

                var job = _PlanPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4));
                _PlanPricingService.RunPricingJob(planPricingRequestDto, job.Id);

                var reportData = _CampaignService.GetProgramLineupReportData(new ProgramLineupReportRequest
                {
                    SelectedPlans = new List<int> { planId }
                }, now);

                var reportOutput = new ProgramLineupReportGenerator(@".\Files\Excel templates").Generate(reportData);
                reportOutput.Filename = "ProgramLineupExport.xlsx";
                _WriteFileToLocalFileSystem(reportOutput);

                Assert.AreEqual(reportData.ReportGeneratedDate, now.ToString("M/d/yy"));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForProgramLineupExport()));
                Assert.AreEqual(reportOutput.Stream.Length,
                    File.ReadAllBytes(@".\Files\Program lineup\ProgramLineupExport.xlsx").LongLength);
            }
        }

        private JsonSerializerSettings _GetJsonSettingsForProgramLineupExport()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(ProgramLineupReportData), "ReportGeneratedDate");
            jsonResolver.Ignore(typeof(ProgramLineupReportData), "AccuracyEstimateDate");
            jsonResolver.Ignore(typeof(SharedFolderFile), "Id");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        private static void _WriteFileToLocalFileSystem(ReportOutput reportOutput)
        {
            if (WRITE_FILE_TO_DISK)
            {
                using (var destinationFileStream = new FileStream($@"C:\Users\sroibu\Downloads\integration_tests_exports\{reportOutput.Filename}", FileMode.OpenOrCreate))
                {
                    while (reportOutput.Stream.Position < reportOutput.Stream.Length)
                    {
                        destinationFileStream.WriteByte((byte)reportOutput.Stream.ReadByte());
                    }
                }
            }
        }
    }
}

