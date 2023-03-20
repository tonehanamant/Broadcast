using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Common.Services.Repositories;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.ReportGenerators.CampaignExport;
using Services.Broadcast.ReportGenerators.ProgramLineup;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.ContractInterfaces;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class CampaignServiceIntegrationTests
    {
        private const string IntegrationTestUser = "IntegrationTestUser";
        private readonly DateTime CreatedDate = new DateTime(2019, 5, 14);
        private ICampaignService _CampaignService;
        private ICampaignSummaryRepository _CampaignSummaryRepository;
        private IWeeklyBreakdownEngine _WeeklyBreakdownEngine;
        private static readonly bool WRITE_FILE_TO_DISK = false;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;
        private static IFeatureToggleHelper _FeatureToggleHelper;

        [SetUp]
        public void SetUpCampaignServiceIntegrationTests()
        {
            _LaunchDarklyClientStub = (LaunchDarklyClientStub)IntegrationTestApplicationServiceFactory.Instance.Resolve<ILaunchDarklyClient>();
            _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();
            _CampaignSummaryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ICampaignSummaryRepository>();
            _WeeklyBreakdownEngine = IntegrationTestApplicationServiceFactory.GetApplicationService<IWeeklyBreakdownEngine>();            
            _FeatureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);
        }

        private void _SetFeatureToggle(string feature, bool activate)
        {
            if (_LaunchDarklyClientStub.FeatureToggles.ContainsKey(feature))
                _LaunchDarklyClientStub.FeatureToggles[feature] = activate;
            else
                _LaunchDarklyClientStub.FeatureToggles.Add(feature, activate);
        }

        [Test]
        [Category("short_running")]
        public void GetCampaignsFilteredByStatusTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetCampaigns(new CampaignFilterDto
                {
                    PlanStatus = PlanStatusEnum.Contracted
                }, new DateTime(2019, 04, 01));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns, _GetJsonSettings()));
            }
        }

        [Test]
        [Category("short_running")]
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
        [Category("short_running")]
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
        [Category("short_running")]
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
        [Category("short_running")]
        public void CampaignExportAvailablePlans_WithPlans()
        {
            // Data already exists for campaign id 2 : campaign, summary, plan
            var campaignId = 2;
            using (new TransactionScopeWrapper())
            {
                var foundCampaign = _CampaignService.CampaignExportAvailablePlans(campaignId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(foundCampaign, _GetJsonSettings()));
            }
        }
        [Test]
        [Category("short_running")]
        public void CampaignExportAvailablePlans_WithoutPlans()
        {
            // Data already exists for campaign id 5 : campaign
            var campaignId = 5;
            using (new TransactionScopeWrapper())
            {
                var foundCampaign = _CampaignService.CampaignExportAvailablePlans(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(foundCampaign, _GetJsonSettings()));
            }
        }

        [Test]
        [Category("short_running")]
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
        [Category("short_running")]
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
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IAudienceService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IStandardDaypartService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ISharedFolderService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IDateTimeEngine>(),
                    _WeeklyBreakdownEngine,
                    DaypartCache.Instance,
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IFeatureToggleHelper>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IAabEngine>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IConfigurationSettingsHelper>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ILockingEngine>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IPlanService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IPlanValidator>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignServiceApiClient>()
                );

                var campaign = _GetValidCampaignForSave();
                campaign.Id = 1;

                var exception = Assert.Throws<CadentException>(() => service.SaveCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.AreEqual(expectedMessage, exception.Message);
            }
        }

        [Test]
        [Category("short_running")]
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
        [Category("short_running")]
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
        [Category("short_running")]
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
                    AgencyMasterId = foundCampaign.AgencyMasterId,
                    AdvertiserMasterId = foundCampaign.AdvertiserMasterId,
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
        [Category("short_running")]
        public void CreateCampaignInvalidAdvertiserIdTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaignForSave();

                // Invalid advertiser id.
                campaign.AdvertiserMasterId = new Guid();

                var exception = Assert.Throws<CadentException>(() => _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.That(exception.Message, Is.EqualTo(CampaignValidator.InvalidAdvertiserErrorMessage));
            }
        }

        [Test]
        [Category("short_running")]
        public void GetQuartersTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetQuarters(null, new DateTime(2019, 5, 1));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }

        [Test]
        [Category("short_running")]
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
        [Category("short_running")]
        public void CreateCampaignInvalidCampaignNameTest(string campaignName)
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaignForSave();

                campaign.Name = campaignName;

                var exception = Assert.Throws<CadentException>(() => _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate));

                Assert.That(exception.Message, Is.EqualTo(CampaignValidator.InvalidCampaignNameErrorMessage));
            }
        }

        [Test]
        [Category("short_running")]
        public void GetCampaignsWithFilters()
        {
            using (new TransactionScopeWrapper())
            {
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
        [Category("short_running")]
        public void GetCampaignsWithAllStatus()
        {
            using (new TransactionScopeWrapper())
            {
                var filter = new CampaignFilterDto
                {
                    PlanStatus = null,
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
        [Category("short_running")]
        public void GetStatuses()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetStatuses(3, 2019);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }

        [Test]
        [Category("short_running")]
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
        [Category("short_running")]
        public void GetStatuses_WithoutParams()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetStatuses(null, null);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }

        [Test]
        [Category("short_running")]
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
        [Category("short_running")]
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
        [Category("short_running")]
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
        [Category("short_running")]
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
        [Category("short_running")]
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
                AdvertiserMasterId = new Guid("1806450a-e0a3-416d-b38d-913fb5cf3879"),
                AgencyMasterId = new Guid("89ab30c5-23a7-41c1-9b7d-f5d9b41dbe8b"),
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
            jsonResolver.Ignore(typeof(PlanSummaryDto), "Is_Draft");
            jsonResolver.Ignore(typeof(PlanSummaryDto), "Version_Number");
            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        [Test]
        [Category("long_running")]
        public void CampaignExport_ContractOnlyPlans()
        {
            using (new TransactionScopeWrapper())
            {
                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1852, 1853 }
                });

                _WriteFileToLocalFileSystem(reportData, "CampaignExport_ContractOnlyPlans.xlsx");

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        [Test]
        [Category("long_running")]
        public void CampaignExport_ContractOnlyPlansWithDaypartsSorting()
        {
            using (new TransactionScopeWrapper())
            {
                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1852, 2233 }
                });

                _WriteFileToLocalFileSystem(reportData, "CampaignExport_ContractOnlyPlansWithDaypartsSorting.xlsx");

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        [Test]
        [Category("long_running")]
        public void CampaignExport_GenerateCampaignReport()
        {
            using (new TransactionScopeWrapper())
            {
                var user = "IntegrationTestsUser";                
                var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
                var featureToggleHelperMock = new Mock<IFeatureToggleHelper>();
                var campaignService = _SetupCampaignService(featureToggleHelperMock.Object,configurationSettingsHelper.Object);
                var fileId = Guid.Empty;
                var sharedFolderRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<ISharedFolderFilesRepository>();
                SharedFolderFile sharedFolderFile = null;

                fileId = campaignService.GenerateCampaignReport(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1852, 1853 }
                }, user, "./Files/Excel templates");

                sharedFolderFile = sharedFolderRepository.GetFileById(fileId);
                Assert.AreNotEqual(Guid.Empty, fileId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(sharedFolderFile, _GetJsonSettingsForCampaignExport()));
            }
        }

        private ICampaignService _SetupCampaignService(IFeatureToggleHelper featureToggleHelper,IConfigurationSettingsHelper configurationSettingsHelper)
        {
            var sharedFolderService = new SharedFolderService(
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IAttachmentMicroServiceApiClient>(),
                 featureToggleHelper,configurationSettingsHelper);

            var campaignService = new CampaignService(
                IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignValidator>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IMediaMonthAndWeekAggregateCache>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IQuarterCalculationEngine>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IBroadcastLockingManagerApplicationService>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregator>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IAudienceService>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IStandardDaypartService>(),
                sharedFolderService,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IDateTimeEngine>(),
                _WeeklyBreakdownEngine,
                DaypartCache.Instance,
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IFeatureToggleHelper>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IAabEngine>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IConfigurationSettingsHelper>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ILockingEngine>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IPlanService>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<IPlanValidator>(),
                IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignServiceApiClient>()
                );

            return campaignService;
        }

        [Test]
        [Category("short_running")]
        public void CampaignExport_ReservedPlanWithConstraints()
        {
            using (new TransactionScopeWrapper())
            {
                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 1850, 1851 }
                });

                _WriteFileToLocalFileSystem(reportData, "CampaignExport_ReservedPlanWithConstraints.xlsx");

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        [Test]
        [Category("short_running")]
        public void CampaignExport_ContractTypeWithRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1850, 1851, 1852, 1853 }
                });

                _WriteFileToLocalFileSystem(reportData, "CampaignExport_ContractTypeWithRestrictions.xlsx");

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        [Test]
        [Category("short_running")]
        public void CampaignExport_ValidateExportType_Contracted()
        {
            using (new TransactionScopeWrapper())
            {
                var exception = Assert.Throws<CadentException>(() =>
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
        [Category("short_running")]
        public void CampaignExport_ValidateGuaranteedAudience()
        {
            using (new TransactionScopeWrapper())
            {
                var exception = Assert.Throws<CadentException>(() =>
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
        [Category("short_running")]
        public void CampaignExport_ValidateSecondaryAudiences()
        {
            using (new TransactionScopeWrapper())
            {
                var exception = Assert.Throws<CadentException>(() =>
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
        [Category("short_running")]
        public void CampaignExport_ValidateExportType_ContractedWithProposal()
        {
            using (new TransactionScopeWrapper())
            {
                var exception = Assert.Throws<CadentException>(() =>
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
        [Category("short_running")]
        public void CampaignExport_ValidateExportType_ContractedWithOther()
        {
            using (new TransactionScopeWrapper())
            {
                var exception = Assert.Throws<CadentException>(() =>
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
        [Category("short_running")]
        public void CampaignExport_ValidateExportType_ProposalOrOther()
        {
            using (new TransactionScopeWrapper())
            {
                var exception = Assert.Throws<CadentException>(() =>
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
        [Category("short_running")]
        public void CampaignExport_ValidateExportType_Other()
        {
            using (new TransactionScopeWrapper())
            {
                var exception = Assert.Throws<CadentException>(() =>
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
        [Category("short_running")]
        public void CampaignExport_SecondaryAudiences_SinglePlan()
        {
            using (new TransactionScopeWrapper())
            {
                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 1848 }
                });

                _WriteFileToLocalFileSystem(reportData, "CampaignExport_SecondaryAudiences_SinglePlan.xlsx");

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        [Test]
        [Category("short_running")]
        public void CampaignExport_SecondaryAudiences_MultiplePlans()
        {
            using (new TransactionScopeWrapper())
            {
                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 1848, 2052 }
                });

                _WriteFileToLocalFileSystem(reportData, "CampaignExport_SecondaryAudiences_MultiplePlans.xlsx");

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        [Test]
        [Category("short_running")]
        public void CampaignExport_PlansWith13And14Weeks()
        {
            using (new TransactionScopeWrapper())
            {
                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 2190, 2191, 2192, 2193 }
                });

                _WriteFileToLocalFileSystem(reportData, "CampaignExport_PlansWith13And14Weeks.xlsx");
                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        [Test]
        [Category("short_running")]
        public void CampaignExport_DuplicateHiatusDaysOnFlowChart()
        {
            using (new TransactionScopeWrapper())
            {
                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 2541, 2579 },
                });

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        [Test]
        [Category("long_running")]
        public void CampaignExport_30SecEquivalizedAndNot()
        {
            Dictionary<int, List<PlanPricingResultsDaypartDto>> planPricingResultsDayparts = new Dictionary<int, List<PlanPricingResultsDaypartDto>>();
            using (new TransactionScopeWrapper())
            {
                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 2191, 2192 }
                });

                _WriteFileToLocalFileSystem(reportData, "CampaignExport_30SecEquivalizedAndNot.xlsx");

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        [Test]
        [Category("short_running")]
        public void CampaignExport_ProposalTab_DataBasedOnImpressions()
        {
            using (new TransactionScopeWrapper())
            {
                var reportData = _CampaignService.GetAndValidateCampaignReportData(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Proposal,
                    SelectedPlans = new List<int> { 2580 }
                });

                Assert.IsTrue(DateTime.Now.ToString("MM/dd/yy").Equals(reportData.CreatedDate));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForCampaignExport()));
            }
        }

        private JsonSerializerSettings _GetJsonSettingsForCampaignExport()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(CampaignReportData), "CreatedDate");
            jsonResolver.Ignore(typeof(CampaignReportData), "CampaignExportFileName");
            jsonResolver.Ignore(typeof(SharedFolderFile), "Id");
            jsonResolver.Ignore(typeof(SharedFolderFile), "FileName");
            jsonResolver.Ignore(typeof(SharedFolderFile), "FileNameWithExtension");
            jsonResolver.Ignore(typeof(SharedFolderFile), "CreatedDate");
            jsonResolver.Ignore(typeof(SharedFolderFile), "SharedFolderFile");
            jsonResolver.Ignore(typeof(SharedFolderFile), "FolderPath");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        [Test]
        [Category("long_running")]
        public async Task ProgramLineupExport()
        {
            using (new TransactionScopeWrapper())
            {
                const int planId = 1197;
                var now = new DateTime(2020, 4, 4);

                // TODO SDE : this should be reworked for these to be true, as they are in production
                //_LaunchDarklyClientStub.FeatureToggles[FeatureToggles.ALLOW_MULTIPLE_CREATIVE_LENGTHS] = false;

                IntegrationTestApplicationServiceFactory.Instance.RegisterType<IPricingApiClient, PricingApiClientStub>();
                var planPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
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
                    UnitCapsType = UnitCapEnum.Per30Min,
                    PlanVersionId = 47
                };

                var job = planPricingService.QueuePricingJob(planPricingRequestDto, new DateTime(2019, 11, 4), "integration test");
                await planPricingService.RunPricingJobAsync(planPricingRequestDto, job.Id, CancellationToken.None);

                var reportData = _CampaignService.GetProgramLineupReportData(new ProgramLineupReportRequest
                {
                    SelectedPlans = new List<int> { planId },
                    PostingType = PostingTypeEnum.NTI,
                    SpotAllocationModelMode = SpotAllocationModelMode.Quality
                }, now);

                _WriteFileToLocalFileSystem(reportData, "ProgramLineupExport.xlsx");

                Assert.AreEqual(reportData.ReportGeneratedDate, now.ToString("M/d/yy"));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(reportData, _GetJsonSettingsForProgramLineupExport()));
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

        private static void _WriteFileToLocalFileSystem<T>(T reportData, string filename)
        {
            if (WRITE_FILE_TO_DISK)
            {
                if (typeof(T).Name.Equals("CampaignReportData"))
                {
                    var reportOutput = new CampaignReportGenerator(@".\Files\Excel templates", _FeatureToggleHelper, new ConfigurationSettingsHelper())
                        .Generate(reportData as CampaignReportData);
                    reportOutput.Filename = filename;
                    _WriteStream(reportOutput);
                    Assert.AreEqual(reportOutput.Stream.Length,
                        File.ReadAllBytes($@".\Files\Campaign export\{reportOutput.Filename}").LongLength);
                }

                if (typeof(T).Name.Equals("ProgramLineupReportData"))
                {
                    var reportOutput = new ProgramLineupReportGenerator(@".\Files\Excel templates").Generate(reportData as ProgramLineupReportData);
                    reportOutput.Filename = filename;
                    _WriteStream(reportOutput);
                    Assert.AreEqual(reportOutput.Stream.Length,
                        File.ReadAllBytes($@".\Files\Program lineup\{reportOutput.Filename}").LongLength);
                }
            }
        }

        private static void _WriteStream(ReportOutput reportOutput)
        {
            using (var destinationFileStream = new FileStream($@"C:\temp\{reportOutput.Filename}", FileMode.OpenOrCreate))
            {
                while (reportOutput.Stream.Position < reportOutput.Stream.Length)
                {
                    destinationFileStream.WriteByte((byte)reportOutput.Stream.ReadByte());
                }
            }
        }

        [Test]
        [Category("short_running")]
        public void GetCampaignCopy()
        {
            var campaignId = 2;
            using (new TransactionScopeWrapper())
            {
                var foundCampaign = _CampaignService.GetCampaignCopy(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(foundCampaign, _GetJsonSettings()));
            }
        }

        [Test]
        [Category("short_running")]
        public void GetCampaignCopy_WithoutPlans()
        {
            var campaignId = 5;
            using (new TransactionScopeWrapper())
            {
                var foundCampaign = _CampaignService.GetCampaignCopy(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(foundCampaign, _GetJsonSettings()));
            }
        }

        [Test]
        [Category("short_running")]
        public void GetCampaignCopy_WithDraftExcluded()
        {
            var campaignId = 2;
            using (new TransactionScopeWrapper())
            {
                var foundCampaign = _CampaignService.GetCampaignCopy(campaignId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(foundCampaign, _GetJsonSettings()));
            }
        }

        [Test]
        [Category("long_running")]
        public void CampaignExport_GenerateCampaignReport_UsingAttachmentMicroService_WithToggleOn()
        {
            using (new TransactionScopeWrapper())
            {
                var user = "IntegrationTestsUser";
                var attachmentId = new Guid("a11a76ba-6594-4c62-9ad7-aa8589d8e97b");
                var configurationSettingsHelper = new Mock<IConfigurationSettingsHelper>();
                var featureToggleHelperMock = new Mock<IFeatureToggleHelper>();
                var campaignService = _SetupCampaignService(featureToggleHelperMock.Object,configurationSettingsHelper.Object);
                var fileId = Guid.Empty;
                var sharedFolderRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                    .GetDataRepository<ISharedFolderFilesRepository>();
                SharedFolderFile sharedFolderFile = null;

                fileId = campaignService.GenerateCampaignReport(new CampaignReportRequest
                {
                    CampaignId = 652,
                    ExportType = CampaignExportTypeEnum.Contract,
                    SelectedPlans = new List<int> { 1852, 1853 }
                }, user, "./Files/Excel templates");

                sharedFolderFile = sharedFolderRepository.GetFileById(fileId);
                sharedFolderFile.AttachmentId = attachmentId;
                Assert.AreNotEqual(Guid.Empty, fileId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(sharedFolderFile, _GetJsonSettingsForCampaignExport()));
            }
        }

        [Test]
        [Category("short_running")]
        public void GetUnifiedCampaignById_WithToggleOn()
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_UNIFIED_CAMPAIGN, true);
            //var campaignId = 2;
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetCampaignForSave();

                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                var foundCampaign = _CampaignService.GetCampaignById(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(foundCampaign, _GetJsonSettings()));
            }
        }
        [Test]
        [Category("short_running")]
        public void GetUnifiedCampaignById_WithToggleOff()
        {
            _SetFeatureToggle(FeatureToggles.ENABLE_UNIFIED_CAMPAIGN, false);
            //var campaignId = 2;
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetCampaignForSave();
                var lockingManagerApplicationServiceMock = new Mock<IBroadcastLockingManagerApplicationService>();
                lockingManagerApplicationServiceMock.Setup(x => x.LockObject(It.IsAny<string>())).Returns(new LockResponse
                {
                    Success = false,
                    LockedUserName = "IntegrationUser"
                });
                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                string expectedMessage = "Could not find existing campaign with id "+ campaignId +"";
                var service = new CampaignService(
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IDataRepositoryFactory>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignValidator>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IMediaMonthAndWeekAggregateCache>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IQuarterCalculationEngine>(),
                    lockingManagerApplicationServiceMock.Object,
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregator>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IAudienceService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IStandardDaypartService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ISharedFolderService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IDateTimeEngine>(),
                    _WeeklyBreakdownEngine,
                    DaypartCache.Instance,
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IFeatureToggleHelper>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IAabEngine>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IConfigurationSettingsHelper>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ILockingEngine>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IPlanService>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IPlanValidator>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignServiceApiClient>()
                );

                var exception = Assert.Throws<CadentException>(() => service.GetCampaignById(campaignId));

                Assert.AreEqual(expectedMessage, exception.Message);
            }
        }
        private SaveCampaignDto _GetCampaignForSave()
        {
            return new SaveCampaignDto
            {
                Name = "Test Campaign",
                AdvertiserMasterId = new Guid("1806450a-e0a3-416d-b38d-913fb5cf3879"),
                AgencyMasterId = new Guid("89ab30c5-23a7-41c1-9b7d-f5d9b41dbe8b"),
                Notes = "Notes for CampaignOne.",
                UnifiedId = "D6A7417C-429A-45C4-AFD9-BDA5AEFCCAE0",
                UnifiedCampaignLastSentAt = new DateTime(),
                UnifiedCampaignLastReceivedAt = new DateTime()
            };
        }
    }
}

