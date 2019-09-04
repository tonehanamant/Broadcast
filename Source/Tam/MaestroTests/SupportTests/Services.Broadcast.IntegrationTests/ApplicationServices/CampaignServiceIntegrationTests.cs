using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities;
using Services.Broadcast.IntegrationTests.Helpers;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;
using Services.Broadcast.Validators;
using Common.Services.ApplicationServices;
using Microsoft.Practices.Unity;
using Moq;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Clients;
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
        public void GetCampaignById()
        {
            var campaignId = 2;
            using (new TransactionScopeWrapper())
            {
                var foundCampaign = _CampaignService.GetCampaignById(campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(foundCampaign, _GetJsonSettings()));
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCampaignById_WithPlans()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();
                var campaignId = _CampaignService.SaveCampaign(campaign, IntegrationTestUser, CreatedDate);
                var plan = _GetNewPlan();
                plan.CampaignId = campaignId;

                _PlanService.SavePlan(plan, "integration_test", new DateTime(2019, 01, 01), aggregatePlanSynchronously: true);
                
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

            using (new TransactionScopeWrapper())
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
                    lockingManagerApplicationServiceMock.Object);

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

                CampaignDto foundCampaign = _CampaignService.GetCampaignById(campaignId);

                foundCampaign.Name = "Updated name of Campaign1";
                int updatedCampaignId = _CampaignService.SaveCampaign(foundCampaign, IntegrationTestUser, CreatedDate);
                CampaignDto updatedCampaign = _CampaignService.GetCampaignById(campaignId);

                Assert.AreEqual(updatedCampaign.Id, campaignId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedCampaign, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateCampaignInvalidAdvertiserIdTest()
        {
            using (new TransactionScopeWrapper())
            {
                var campaign = _GetValidCampaign();

                // Invalid advertiser id.
                campaign.Advertiser = new AdvertiserDto { Id = 666 };

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
                var campaigns = _CampaignService.GetQuarters(new DateTime(2019, 5, 1));
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

        private CampaignDto _GetValidCampaign()
        {
            return new CampaignDto
            {
                Name = "Campaign1",
                Advertiser = new AdvertiserDto { Id = 1 },
                Agency = new AgencyDto { Id = 1 },
                Notes = "Notes for CampaignOne."
            };
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(CampaignDto), "Id");
            jsonResolver.Ignore(typeof(PlanSummaryDto), "PlanId");

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
                HUTBookId = null,
                PostingType = Entities.Enums.PostingTypeEnum.NTI,
                ShareBookId = 437,
                Budget = 100m,
                CPM = 12m,
                DeliveryImpressions = 100d,
                DeliveryRatingPoints = 6d,
                CoverageGoalPercent = 80.5,
                GoalBreakdownType = Entities.Enums.PlanGloalBreakdownTypeEnum.Even,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUs = 20, Rank = 1, ShareOfVoicePercent = 22.2},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUs = 32.5, Rank = 2, ShareOfVoicePercent = 34.5}
                },
                BlackoutMarkets = new List<PlanBlackoutMarketDto>
                {
                    new PlanBlackoutMarketDto {MarketCode = 123, MarketCoverageFileId = 1, PercentageOfUs = 5.5, Rank = 5 },
                    new PlanBlackoutMarketDto {MarketCode = 234, MarketCoverageFileId = 1, PercentageOfUs = 2.5, Rank = 8 },
                },
                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto{ DaypartCodeId = 2, StartTimeSeconds = 0, EndTimeSeconds = 2000, WeightingGoalPercent = 28.0 },
                    new PlanDaypartDto{ DaypartCodeId = 11, StartTimeSeconds = 1500, EndTimeSeconds = 2788, WeightingGoalPercent = 33.2 }
                }
            };
        }
    }
}
