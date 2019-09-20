using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using Services.Broadcast.Cache;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.ContractInterfaces;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class CampaignServiceIntegrationTests
    {
        private const string IntegrationTestUser = "IntegrationTestUser";
        private readonly DateTime CreatedDate = new DateTime(2019, 5, 14);
        private readonly ICampaignService _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();
        private readonly IPlanService _PlanService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanService>();
        private readonly ICampaignSummaryRepository _CampaignSummaryRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ICampaignSummaryRepository>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCampaignsTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetCampaigns(new CampaignFilterDto
                {
                    Quarter = new QuarterDto
                    {
                        Quarter = 2,
                        Year = 2019
                    }
                }, new DateTime(2019, 04, 01));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCampaignsFilteredByStatusTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaigns = _CampaignService.GetCampaigns(new CampaignFilterDto
                {
                    PlanStatus = PlanStatusEnum.Working
                }, new DateTime(2019, 04, 01));
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns));
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
                var campaign = _GetValidCampaign();

                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                Assert.IsTrue(campaignId > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanNotUpdateLockedCampaign()
        {
            const string expectedMessage = "The chosen campaign has been locked by IntegrationUser";

            using (new TransactionScopeWrapper(System.Transactions.IsolationLevel.ReadCommitted))
            {
                var lockingManagerApplicationServiceMock = new Mock<ILockingManagerApplicationService>();
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
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ITrafficApiClient>(),
                    lockingManagerApplicationServiceMock.Object,
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregator>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<ICampaignAggregationJobTrigger>(),
                    IntegrationTestApplicationServiceFactory.Instance.Resolve<IAgencyCache>()
                    );

                var campaign = _GetValidCampaign();
                campaign.Id = 1;

                var exception = Assert.Throws<Exception>(() => service.SaveCampaign(campaign, IntegrationTestUser, CreatedDate));

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
                var campaign = _GetValidCampaign();
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
                var campaign = _GetValidCampaign();
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
                var campaign = _GetValidCampaign();
                int campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);

                _CampaignSummaryRepository.SaveSummary(GetSummary(campaignId, IntegrationTestUser, CreatedDate));
                CampaignDto foundCampaign = _CampaignService.GetCampaignById(campaignId);

                foundCampaign.Name = "Updated name of Campaign1";
                int updatedCampaignId = _CampaignService.SaveCampaign(foundCampaign, IntegrationTestUser, CreatedDate);
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
                LastAggregated = new DateTime(2019,09,04,16,41,0),
                CampaignId = campaignId,
                CampaignStatus = PlanStatusEnum.Contracted,
                Budget = 1500.0m,
                HouseholdCPM = 2500.0m,
                HouseholdRatingPoints = 23.0,
                HouseholdImpressions = 5500.0,
                FlightStartDate = new DateTime(2019, 08, 01),
                FlightEndDate = new DateTime(2019, 08, 30),
                FlightActiveDays = 26,
                FlightHiatusDays = 4,
                PlanStatusCountWorking = 0,
                PlanStatusCountReserved =0,
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
                var campaign = _GetValidCampaign();

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
                var campaign = _GetValidCampaign();

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
                var campaign = _GetValidCampaign();
                var campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                var plan = _GetNewPlan();                
                plan.Status = PlanStatusEnum.ClientApproval;
                plan.CampaignId = campaignId;

                var secondCampaign = _GetValidCampaign();
                var secondCampaignId = _CampaignService.SaveCampaign(secondCampaign, IntegrationTestUser, CreatedDate);
                var secondPlan = _GetNewPlan();
                secondPlan.FlightStartDate = new DateTime(2018, 02, 01);
                secondPlan.FlightEndDate = new DateTime(2018, 08, 31);
                secondPlan.FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2018, 3, 20),
                    new DateTime(2018, 4, 15)
                };
                secondPlan.Status = PlanStatusEnum.ClientApproval;
                secondPlan.CampaignId = secondCampaignId;

                _PlanService.SavePlan(plan, "integration_test", new DateTime(2019, 01, 01), aggregatePlanSynchronously: true);
                _PlanService.SavePlan(secondPlan, "integration_test", new DateTime(2019, 01, 01), aggregatePlanSynchronously: true);

                _CampaignSummaryRepository.SaveSummary(GetSummary(campaignId, IntegrationTestUser, CreatedDate));
                _CampaignSummaryRepository.SaveSummary(GetSummary(secondCampaignId, IntegrationTestUser, CreatedDate));

                var filter = new CampaignFilterDto
                {
                    PlanStatus = PlanStatusEnum.ClientApproval,
                    Quarter = new QuarterDto
                    {
                        Quarter = 2,
                        Year = 2019
                    }
                };

                var campaigns = _CampaignService.GetCampaigns(filter, new DateTime(2019, 02, 01));

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaigns, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetStatusesTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();
                var campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                var plan = _GetNewPlan();
                plan.CampaignId = campaignId;
                _PlanService.SavePlan(plan, "integration_test", new DateTime(2019, 01, 01), aggregatePlanSynchronously: true);

                var campaigns = _CampaignService.GetStatuses(2, 2019);
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

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(fullCampaign));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProcessCampaignAggregation_WithoutPlans()
        {
            // Data already exists for campaign id 3 : campaign, summary
            const int campaignId = 3;
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

        private CampaignDto _GetValidCampaign()
        {
            return new CampaignDto
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
                Status = Entities.Enums.PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019,1,20),
                    new DateTime(2019,4,15)
                },
                AudienceId = 31,        //HH
                AudienceType = Entities.Enums.AudienceTypeEnum.Nielsen,
                HUTBookId = 437,
                PostingType = Entities.Enums.PostingTypeEnum.NTI,
                ShareBookId = 437,
                Budget = 100m,
                CPM = 12m,
                CPP = 200951583.9999m,
                DeliveryImpressions = 100d,
                DeliveryRatingPoints = 6d,
                CoverageGoalPercent = 80.5,
                Currency = PlanCurrenciesEnum.Impressions,
                GoalBreakdownType = Entities.Enums.PlanGoalBreakdownTypeEnum.Even,
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
                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto{ DaypartCodeId = 2, StartTimeSeconds = 0, EndTimeSeconds = 2000, WeightingGoalPercent = 28.0 },
                    new PlanDaypartDto{ DaypartCodeId = 11, StartTimeSeconds = 1500, EndTimeSeconds = 2788, WeightingGoalPercent = 33.2 }
                },
                Vpvh = 0.101,
                HouseholdUniverse = 1000000,
                HouseholdDeliveryImpressions = 10000,
                HouseholdCPM = 0.01m,
                HouseholdRatingPoints = 1,
                HouseholdCPP = 10000,
                Universe = 3000000,
            };
        }
    }
}
