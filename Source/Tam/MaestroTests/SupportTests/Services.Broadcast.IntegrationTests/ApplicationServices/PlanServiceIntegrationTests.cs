using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanServiceIntegrationTests
    {
        private readonly IPlanService _PlanService;
        private readonly ICampaignService _CampaignService;
        private readonly IPlanPricingService _PlanPricingService;

        private const int MEDIA_MONTH_ID = 450;
        private const int AUDIENCE_ID = 31;

        public PlanServiceIntegrationTests()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IPricingApiClient, PricingApiClientStub>();
            _PlanService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanService>();
            _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();
            _PlanPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanStatuses()
        {
            var statuses = _PlanService.GetPlanStatuses();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(statuses));
        }

        [Test]
        public void CreateNewPlan()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateNewPlan_WithAduDisabled()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.IsAduEnabled = false;

                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
                var planVersion = _PlanService.GetPlan(newPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(planVersion, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreateNewPlan_NullHutBook()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.HUTBookId = null;
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                
                var planVersion = _PlanService.GetPlan(newPlanId);

                Assert.IsTrue(newPlanId > 0);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(planVersion, _GetJsonSettings()));
            }
        }

        [Test]
        public void CreateNewDraft()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Id = 523;   //existing plan in database
                newPlan.VersionId = 1;
                newPlan.IsDraft = true;

                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                var draftId = _PlanService.CheckForDraft(newPlanId);

                Assert.IsTrue(draftId > 0);
                Assert.IsTrue(newPlanId > 0);
            }
        }

        [Test]
        public void SavePlan_InvalidMarketCoverage_PRI17598()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 197, MarketCoverageFileId = 1, PercentageOfUS = 0.051, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Parkersburg"},
                    new PlanAvailableMarketDto { MarketCode = 261, MarketCoverageFileId = 1, PercentageOfUS = 0.048, Rank = 2, ShareOfVoicePercent = 34.5, Market = "San Angelo"},
                    new PlanAvailableMarketDto { MarketCode = 359, MarketCoverageFileId = 1, PercentageOfUS = 0.048, Rank = 2, ShareOfVoicePercent = 34.5, Market = "Cheyenne-Scottsbluf"},
                    new PlanAvailableMarketDto { MarketCode = 367, MarketCoverageFileId = 1, PercentageOfUS = 0.048, Rank = 2, ShareOfVoicePercent = 34.5, Market = "Casper-Riverton"},
                    new PlanAvailableMarketDto { MarketCode = 340, MarketCoverageFileId = 1, PercentageOfUS = 0.048, Rank = 2, ShareOfVoicePercent = 34.5, Market = "North Platte"},
                };
                newPlan.CoverageGoalPercent = 0.2;

                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
            }
        }

        [Test]
        public void SavePlan_InvalidDayparts_DuplicateDayparts()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                var daypart = newPlan.Dayparts.First();
                newPlan.Dayparts.Add(daypart);
                var hasDuplicates = newPlan.Dayparts.GroupBy(d => d.DaypartCodeId).Any(d => d.Count() > 1);

                var exception = Assert.Throws<Exception>(() => _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01)));

                Assert.IsTrue(hasDuplicates);
                Assert.AreEqual(exception.Message, "Invalid dayparts.  Each daypart can be entered only once.");
            }
        }

        [Test]
        public void DeleteDraft()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                //get the plan and format the impressions
                PlanDto plan = _PlanService.GetPlan(newPlanId);

                //save draft
                plan.IsDraft = true;
                _PlanService.SavePlan(plan, "integration_test", new DateTime(2019, 01, 07));

                var draftId = _PlanService.CheckForDraft(newPlanId);
                Assert.IsTrue(draftId > 0);

                //delete draft and check
                _PlanService.DeletePlanDraft(newPlanId);
                var draftIdDeleted = _PlanService.CheckForDraft(newPlanId);
                Assert.IsTrue(draftIdDeleted == 0);
            }
        }

        [Test]
        public void DeleteDraft_InvalidDraft()
        {
            using (new TransactionScopeWrapper())
            {
                //try to delete non existing draft                
                var exception = Assert.Throws<Exception>(() => _PlanService.DeletePlanDraft(999));

                Assert.That(exception.Message, Is.EqualTo("Cannot delete invalid draft."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanVersion()
        {
            using (new TransactionScopeWrapper())
            {
                var planVersion = _PlanService.GetPlan(523, 1);

                Assert.IsTrue(planVersion.Id > 0);
                Assert.IsTrue(planVersion.VersionId > 0);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(planVersion, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanCreateNewPlanWithCanceledStatus()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Status = PlanStatusEnum.Canceled;

                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
                PlanDto finalPlan = _PlanService.GetPlan(newPlanId);

                Assert.AreEqual(PlanStatusEnum.Canceled, finalPlan.Status);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanHistory()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                //get the plan and format the impressions
                PlanDto plan = _PlanService.GetPlan(newPlanId);

                //save version 2
                plan.Budget = 222;  //we change the budget to have different data between versions    

                plan.WeeklyBreakdownWeeks.FirstOrDefault().WeeklyBudget = 142;
                _PlanService.SavePlan(plan, "integration_test", new DateTime(2019, 01, 05));
                _ForceCompletePlanPricingJob(newPlanId);

                //save version 3
                plan.Dayparts.RemoveAt(1); //we remove a daypart to have different data between versions
                plan.TargetImpressions = plan.TargetImpressions / 1000;
                foreach(var week in plan.WeeklyBreakdownWeeks)
                {
                    week.WeeklyImpressions = week.WeeklyImpressions / 1000;
                }
                _PlanService.SavePlan(plan, "integration_test", new DateTime(2019, 01, 07));

                var planHistory = _PlanService.GetPlanHistory(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(planHistory, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanHistory_WithDraft()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                //get the plan and format the impressions
                PlanDto plan = _PlanService.GetPlan(newPlanId);

                //save version 2
                plan.Budget = 222;
                plan.WeeklyBreakdownWeeks.FirstOrDefault().WeeklyBudget = 142;
                _PlanService.SavePlan(plan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                //save draft
                plan.IsDraft = true;
                plan.TargetImpressions = plan.TargetImpressions / 1000;
                foreach (var week in plan.WeeklyBreakdownWeeks)
                {
                    week.WeeklyImpressions = week.WeeklyImpressions / 1000;
                }
                _PlanService.SavePlan(plan, "integration_test", new DateTime(2019, 01, 01));

                var planHistory = _PlanService.GetPlanHistory(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(planHistory, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreatingPlanSetsVersionNumber()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01));

                //get the plan
                PlanDto plan = _PlanService.GetPlan(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(plan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreatingNewVersionForPlanSetsVersionNumber()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                //get the plan
                PlanDto plan = _PlanService.GetPlan(newPlanId);

                //save version 2
                plan.Budget = 222;
                plan.WeeklyBreakdownWeeks.FirstOrDefault().WeeklyBudget = 142;
                _PlanService.SavePlan(plan, "integration_test", new DateTime(2019, 01, 01));

                //get the plan again
                plan = _PlanService.GetPlan(newPlanId);

                Assert.AreEqual(222, plan.PricingParameters.Budget);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(plan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreatingDraftForPlanDoesntUpdateVersionNumber()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01));

                _ForceCompletePlanPricingJob(newPlanId);

                //get the plan
                PlanDto plan = _PlanService.GetPlan(newPlanId);

                //create draft
                plan.Budget = 222;
                plan.WeeklyBreakdownWeeks.LastOrDefault().WeeklyBudget = 142;
                plan.IsDraft = true;
                _PlanService.SavePlan(plan, "integration_test", new DateTime(2019, 01, 01));

                //get the plan again
                plan = _PlanService.GetPlan(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(plan, _GetJsonSettings()));
            }
        }

        [Test]
        public void CreatingPlansCheckCampaignAggregation()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto firstNewPlan = _GetNewPlan();
                firstNewPlan.Status = PlanStatusEnum.Canceled;

                var firstNewPlanId = _PlanService.SavePlan(firstNewPlan, "integration_test", new DateTime(2019, 10, 17));

                PlanDto secondNewPlan = _GetNewPlan();
                secondNewPlan.Status = PlanStatusEnum.Rejected;

                var secondNewPlanId = _PlanService.SavePlan(secondNewPlan, "integration_test", new DateTime(2019, 10, 17));

                PlanDto thirdNewPlan = _GetNewPlan();
                thirdNewPlan.Status = PlanStatusEnum.Scenario;

                var thirdNewPlanId = _PlanService.SavePlan(thirdNewPlan, "integration_test", new DateTime(2019, 10, 17));

                Assert.IsTrue(firstNewPlanId > 0);
                Assert.IsTrue(secondNewPlanId > 0);
                Assert.IsTrue(thirdNewPlanId > 0);

                _CampaignService.ProcessCampaignAggregation(1);

                var campaign = _CampaignService.GetCampaignById(1);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaign, _GetJsonSettings()));
            }
        }

        [Test]
        public void CreatingASingleCanceledPlanFiltersCampaignsCorrectly()
        {
            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiCache>(new TrafficApiCacheStub());
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                SaveCampaignDto newCampaign = _GetNewCampaign();
                var newCampaignId = _CampaignService.SaveCampaign(newCampaign, "integration_test", new DateTime(2019, 10, 30));

                Assert.IsTrue(newCampaignId > 0);

                PlanDto newCanceledPlan = _GetNewPlan();
                newCanceledPlan.CampaignId = newCampaignId;
                newCanceledPlan.Status = PlanStatusEnum.Canceled;

                var newPlanId = _PlanService.SavePlan(newCanceledPlan, "integration_test", new DateTime(2019, 10, 30));

                Assert.IsTrue(newPlanId > 0);

                _CampaignService.ProcessCampaignAggregation(newCampaignId);

                var filterredCampaignList = _CampaignService.GetCampaigns(new CampaignFilterDto
                {
                    Quarter = new QuarterDto
                    {
                        Quarter = 4,
                        Year = 2019
                    }
                }, new DateTime(2019, 10, 30));

                Assert.That(filterredCampaignList.Any(campaign => campaign.Id == newCampaignId));
            }
        }

        [Test]
        public void CreatingDraftPlanPopulatesGetCampaignFlagsCorrectly()
        {
            using (new TransactionScopeWrapper())
            {
                SaveCampaignDto newCampaign = _GetNewCampaign();
                var newCampaignId = _CampaignService.SaveCampaign(newCampaign, "integration_test", new DateTime(2019, 10, 30));

                Assert.IsTrue(newCampaignId > 0);

                PlanDto newPlan = _GetNewPlan();
                newPlan.CampaignId = newCampaignId;
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 10, 30), true);

                _ForceCompletePlanPricingJob(newPlanId);

                var planFromDB = _PlanService.GetPlan(newPlanId);
                planFromDB.IsDraft = true;
                var draftPLan = _PlanService.SavePlan(planFromDB, "integration_test", new DateTime(2019, 10, 30), true);

                _CampaignService.ProcessCampaignAggregation(newCampaignId);
                var campaign = _CampaignService.GetCampaignById(newCampaignId);

                Assert.AreEqual(1, campaign.Plans.Count);
                Assert.AreEqual("integration_test", campaign.Plans.First().DraftModifiedBy);
                Assert.AreEqual(new DateTime(2019, 10, 30), campaign.Plans.First().DraftModifiedDate);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanCreateNewPlanWithRejectedStatus()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Status = PlanStatusEnum.Rejected;

                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
                PlanDto finalPlan = _PlanService.GetPlan(newPlanId);

                Assert.AreEqual(PlanStatusEnum.Rejected, finalPlan.Status);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        public void CreatePlan_InvalidSpotLengthId()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SpotLengthId = 100;

                var exception = Assert.Throws<Exception>(() => _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01)));

                Assert.That(exception.Message, Is.EqualTo("Invalid spot length"));
            }
        }

        [Test]
        public void CreatePlan_NotExistingProduct()
        {
            const int notExistingProductId = 666;

            using (new TransactionScopeWrapper())
            {
                IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<ITrafficApiClient>(new TrafficApiClientStub());

                PlanDto newPlan = _GetNewPlan();
                newPlan.ProductId = notExistingProductId;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01)),
                    Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid product"));
            }
        }

        [Test]
        public void CreatePlan_InvalidName()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Name = null;

                var exception = Assert.Throws<Exception>(() => _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01)));

                Assert.That(exception.Message, Is.EqualTo("Invalid plan name"));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlan()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.SecondaryAudiences = new List<PlanAudienceDto>
                {
                    new PlanAudienceDto
                    {
                        AudienceId = 6,
                        CPM = 10,
                        CPP = 12,
                        Impressions = 100000,
                        RatingPoints = 25,
                        Type = AudienceTypeEnum.Nielsen,
                        Universe = 123123,
                        Vpvh =0.456
                    }
                };
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(testPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlan()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                // modify the plan
                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.Name = "Renamed Plan";
                testPlan.ProductId = 2;
                // modify the flight.
                testPlan.FlightNotes = "Changed the flight notes";
                testPlan.FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019, 1, 28),
                    new DateTime(2019, 1, 4)
                };

                var modifedPlanId = _PlanService.SavePlan(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlan_WithAduDisabled()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                // modify the plan
                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.IsAduEnabled = false;

                var modifedPlanId = _PlanService.SavePlan(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanAndRemoveHiatusDays()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                // modify the plan
                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.Name = "Renamed Plan";
                testPlan.ProductId = 2;
                // modify the flight.
                testPlan.FlightNotes = "Changed the flight notes";
                testPlan.FlightHiatusDays = new List<DateTime>();

                var modifedPlanId = _PlanService.SavePlan(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        public void SavePlan_WithoutFlightInfo()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto testPlan = _GetNewPlan();
                testPlan.FlightStartDate = null;
                testPlan.FlightEndDate = null;
                testPlan.FlightNotes = null;
                testPlan.FlightHiatusDays.Clear();

                Assert.That(() => _PlanService.SavePlan(testPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidFlightDays()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.FlightStartDate = new DateTime(2019, 10, 1);
                newPlan.FlightEndDate = new DateTime(2018, 01, 01);

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight dates.  The end date cannot be before the start date."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidHiatusDays()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(1968, 1, 28),
                    new DateTime(1976, 6, 4)
                };

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight hiatus day.  All days must be within the flight date range."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanWithDayparts()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                var planId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanWithDayparts_WithoutRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                newPlan.Dayparts.First().Restrictions = null;

                var planId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanWithDayparts_WithoutShowTypeRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                newPlan.Dayparts.First().Restrictions.ShowTypeRestrictions = null;

                var planId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanWithDayparts_WithoutGenreRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                newPlan.Dayparts.First().Restrictions.GenreRestrictions = null;

                var planId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanWithDayparts_WithoutProgramRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                newPlan.Dayparts.First().Restrictions.ProgramRestrictions = null;

                var planId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanWithDayparts_WithoutAffiliateRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                newPlan.Dayparts.First().Restrictions.AffiliateRestrictions = null;

                var planId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanAddDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                var planId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 15));
                _ForceCompletePlanPricingJob(planId);

                PlanDto modifiedPlan = _PlanService.GetPlan(planId);
                modifiedPlan.Dayparts.Add(new PlanDaypartDto
                {
                    DaypartCodeId = 9,
                    DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews,
                    StartTimeSeconds = 54000,
                    EndTimeSeconds = 64799,
                    WeightingGoalPercent = 13.8,
                    Restrictions = new PlanDaypartDto.RestrictionsDto
                    {
                        ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Include,
                            ShowTypes = new List<LookupDto> { new LookupDto { Id = 5 } }
                        },
                        GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                        {
                            ContainType = ContainTypeEnum.Exclude,
                            Genres = new List<LookupDto> { new LookupDto { Id = 12 } }
                        }
                    }
                });

                var modifiedPlanId = _PlanService.SavePlan(modifiedPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifiedPlanId);

                Assert.AreEqual(4, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanRemoveDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                var planId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 15));
                _ForceCompletePlanPricingJob(planId);
                PlanDto modifiedPlan = _PlanService.GetPlan(planId);
                modifiedPlan.Dayparts.RemoveAt(modifiedPlan.Dayparts.Count - 1);


                var modifiedPlanId = _PlanService.SavePlan(modifiedPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifiedPlanId);

                Assert.AreEqual(2, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        public void SavePlan_WithInvalidWeightingGoalTooLittle()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = 8900, EndTimeSeconds = 4600, WeightingGoalPercent = 0.0 });

                Assert.Throws<Exception>(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), "Invalid daypart weighting goal.");
            }
        }

        [Test]
        public void SavePlan_WithInvalidWeightingGoalTooHigh()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = 4600, EndTimeSeconds = 8900, WeightingGoalPercent = 111.0 });

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart weighting goal."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlan_WithSecondaryAudiences()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SecondaryAudiences = new List<PlanAudienceDto>()
                {
                    new PlanAudienceDto {AudienceId = 7, Type = Entities.Enums.AudienceTypeEnum.Nielsen, Vpvh = 0.435},
                    new PlanAudienceDto {AudienceId = 6, Type = Entities.Enums.AudienceTypeEnum.Nielsen, Vpvh = 0.635}
                };

                var planId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01));
                Assert.IsTrue(planId > 0);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_PlanService.GetPlan(planId), _GetJsonSettings()));
            }
        }

        [Test]
        public void SavePlan_WithInvalidSecondaryAudience()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SecondaryAudiences = new List<PlanAudienceDto>()
                {
                    new PlanAudienceDto {AudienceId = 0, Type = Entities.Enums.AudienceTypeEnum.Nielsen},
                };

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid audience"));
            }
        }

        [Test]
        public void SavePlan_WithInvalidSecondaryAudienceDuplicate()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SecondaryAudiences = new List<PlanAudienceDto>()
                {
                    new PlanAudienceDto {AudienceId = 31, Type = Entities.Enums.AudienceTypeEnum.Nielsen},
                };

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("An audience cannot appear multiple times"));
            }
        }

        [Test]
        public void SavePlan_WithInvalidFlightNotes()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto testPlan = _GetNewPlan();
                testPlan.FlightNotes = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque porttitor tellus at ante tempus vehicula ac at sapien. Pellentesque lorem velit, sodales in ex quis, laoreet dictum risus. Quisque odio sapien, dignissim a lacus et, dignissim auctor urna. Vestibulum tempus dui tortor, nec fermentum massa pharetra sit amet. Morbi fermentum ornare scelerisque. Proin ut lectus in nisl vulputate mattis in in ex. Nam erat sem, convallis condimentum velit blandit, scelerisque condimentum dolor. Maecenas fermentum feugiat lectus. Phasellus et sem in velit hendrerit sodales. Suspendisse porta nec felis ac blandit. In eu nisi ut dui tristique mattis. Vivamus vulputate, elit sit amet porta molestie, justo mauris cursus ipsum, et rhoncus arcu odio id enim. Pellentesque elementum posuere nibh ac rutrum. Donec eget erat nec lorem feugiat ornare vel congue nibh. Nulla cursus bibendum sollicitudin. Quisque viverra ante massa, sed molestie augue rutrum sed. Aenean tempus vitae purus sed lobortis. Sed cursus tempor erat ac pulvinar.";

                Assert.That(() => _PlanService.SavePlan(testPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Flight notes cannot be longer than 1024 characters."));
            }
        }

        [Test]
        public void SavePlan_NullFlightDays()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _GetNewPlan();
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01));
                var testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.FlightDays = null;

                Assert.That(() => _PlanService.SavePlan(testPlan, "integration_test", new DateTime(2019, 01, 15)), 
                    Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight days. The plan should have at least one flight day"));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlan_EmptyFlightDays()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _GetNewPlan();
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01));
                var testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.FlightDays = new List<int>();

                Assert.That(() => _PlanService.SavePlan(testPlan, "integration_test", new DateTime(2019, 01, 15)),
                    Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight days. The plan should have at least one flight day"));
            }
        }

        [Test]
        public void PlanAutomaticStatusTransition_ContractedToLive()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto testPlan = _GetNewPlan();
                testPlan.Status = PlanStatusEnum.Contracted;

                var newPlanId = _PlanService.SavePlan(testPlan, "integration_test",
                    new DateTime(2019, 01, 01));

                var newPlan = _PlanService.GetPlan(newPlanId);
                Assert.AreEqual(newPlan.VersionNumber, 1);

                _PlanService.AutomaticStatusTransitions(new DateTime(2019, 01, 01), "integration_test", new DateTime(2019, 01, 01));

                var updatedPlan = _PlanService.GetPlan(newPlanId);

                Assert.AreEqual(updatedPlan.Status, PlanStatusEnum.Live);
                Assert.AreEqual(updatedPlan.VersionNumber, 2);
            }
        }

        [Test]
        public void PlanAutomaticStatusTransition_LiveToComplete()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto testPlan = _GetNewPlan();
                testPlan.Status = PlanStatusEnum.Live;

                var newPlanId = _PlanService.SavePlan(testPlan, "integration_test",
                    new DateTime(2019, 01, 01));

                var newPlan = _PlanService.GetPlan(newPlanId);
                Assert.AreEqual(newPlan.VersionNumber, 1);

                _PlanService.AutomaticStatusTransitions(new DateTime(2019, 02, 01), "integration_test", new DateTime(2019, 02, 01));

                var updatedPlan = _PlanService.GetPlan(newPlanId);

                Assert.AreEqual(updatedPlan.Status, PlanStatusEnum.Complete);
                Assert.AreEqual(updatedPlan.VersionNumber, 2);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanCurrencies()
        {
            var currencies = _PlanService.GetPlanCurrencies();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(currencies));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanDeliveryTypes()
        {
            var deliveryTypes = _PlanService.PlanGoalBreakdownTypes();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(deliveryTypes));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase1()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                Budget = 100m,
                Impressions = 3000d,
                AudienceId = AUDIENCE_ID,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase2()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPM = 100m,
                Impressions = 3000d,
                AudienceId = AUDIENCE_ID,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase3()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPM = 100m,
                Budget = 3000m,
                AudienceId = AUDIENCE_ID,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase4()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPP = 100m,
                Budget = 3000m,
                AudienceId = AUDIENCE_ID,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase5()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPP = 100m,
                RatingPoints = 3000d,
                AudienceId = AUDIENCE_ID,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase6()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                Budget = 100m,
                RatingPoints = 3000d,
                AudienceId = AUDIENCE_ID,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase7()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                Budget = 1000,
                RatingPoints = 0,
                Impressions = 25000,
                AudienceId = 34,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase8()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPM = 13.45m,
                Impressions = 13.456d,
                AudienceId = AUDIENCE_ID,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase9()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                Budget = 3000,
                Impressions = 3000d,
                AudienceId = AUDIENCE_ID,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase10()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPM = 1,
                Impressions = 3000d,
                AudienceId = AUDIENCE_ID,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase11()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPM = 1,
                Budget = 3000,
                AudienceId = AUDIENCE_ID,
                MediaMonthId = MEDIA_MONTH_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void Calculator_WithoutMediaMonthId()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.Calculate(new PlanDeliveryBudget
                {
                    Budget = null,
                    CPM = null,
                    Impressions = null,
                    AudienceId = 5
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Cannot calculate goal without media month and audience"));
            }
        }

        [Test]
        public void Calculator_WithoutAudienceId()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.Calculate(new PlanDeliveryBudget
                {
                    Budget = null,
                    CPM = null,
                    Impressions = null,
                    MediaMonthId = 437,
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Cannot calculate goal without media month and audience"));
            }
        }

        [Test]
        public void Calculator_InvalidObject()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.Calculate(new PlanDeliveryBudget
                {
                    Budget = null,
                    CPM = null,
                    Impressions = null,
                    AudienceId = AUDIENCE_ID,
                    MediaMonthId = MEDIA_MONTH_ID
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("At least 2 values needed to calculate goal amount"));
            }
        }

        [Test]
        public void Calculator_InvalidObject2()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.Calculate(new PlanDeliveryBudget
                {
                    Budget = 1,
                    CPM = 1,
                    Impressions = 1,
                    AudienceId = 0
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Cannot calculate goal without media month and audience"));
            }
        }

        [Test]
        public void Calculator_InvalidObject_NegativeValues()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.Calculate(new PlanDeliveryBudget
                {
                    Budget = -1,
                    CPM = null,
                    Impressions = null
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid budget values passed"));
            }
        }

        [Test]
        public void SavePlan_WithInvalidStartTime_TooLittle()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = -2, EndTimeSeconds = 4600, WeightingGoalPercent = 111.0 });

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidStartTime_TooMuch()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = 999999999, EndTimeSeconds = 4600, WeightingGoalPercent = 111.0 });

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidEndTime_TooLittle()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = 8900, EndTimeSeconds = -2, WeightingGoalPercent = 111.0 });

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlan_WithEmptyCoverageGoal()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.CoverageGoalPercent = null;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid coverage goal value."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidCoverageGoalTooSmall()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.CoverageGoalPercent = -1;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid coverage goal value."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidCoverageGoalTooBig()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.CoverageGoalPercent = 120;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid coverage goal value."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlan_WithEmptyBlackoutMarkets()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.BlackoutMarkets.Clear();
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);
                // modify the plan
                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.Name = "Renamed Plan";
                testPlan.ProductId = 2;
                // modify the flight.
                testPlan.FlightNotes = "Changed the flight notes";
                testPlan.FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019, 1, 28),
                    new DateTime(2019, 1, 4)
                };

                var modifedPlanId = _PlanService.SavePlan(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        public void SavePlan_WithEmptyAvailableMarkets()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets.Clear();

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid total market coverage."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidAvailableMarketShareOfVoiceTooSmall()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets[0].ShareOfVoicePercent = -1;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid share of voice for market."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidAvailableMarketShareOfVoiceTooBig()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets[0].ShareOfVoicePercent = 120;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid share of voice for market."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidEndTime_TooMuch()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = 8900, EndTimeSeconds = 999999999, WeightingGoalPercent = 111.0 });

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
            }
        }

        [Test]
        public void Plan_WeeklyBreakdown_InvalidRequest()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(null), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid request"));
            }
        }

        [Test]
        public void Plan_WeeklyBreakdown_InvalidFlight()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = default,
                    FlightStartDate = default,
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
            }
        }

        [Test]
        public void Plan_WeeklyBreakdown_InvalidFlightStartDate()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = default,
                    FlightStartDate = new DateTime(2019, 01, 01),
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomDelivery_InitialRequest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = new DateTime(2019, 03, 05),
                    FlightStartDate = new DateTime(2019, 02, 01),
                    FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                    DeliveryType = PlanGoalBreakdownTypeEnum.Custom,
                    FlightHiatusDays = new List<DateTime>(),
                    TotalImpressions = 10000,
                    TotalRatings = 0.000907291831869388,
                    WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
                });
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_OneHiatusDay()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 08, 03),
                FlightEndDate = new DateTime(2019, 08, 27),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 1000,
                TotalRatings = 0.000907291831869388,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_WithFlightDays()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 08, 03),
                FlightEndDate = new DateTime(2019, 08, 27),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                FlightDays = new List<int> { 1, 2, 3, 4, 7 },
                TotalImpressions = 1000,
                TotalRatings = 0.000907291831869388,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_RoundShareOfVoiceTotal()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 08, 05),
                FlightEndDate = new DateTime(2019, 09, 19),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 1000,
                TotalRatings = 0.000907291831869388,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_PRI15094()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = Entities.Enums.PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 09, 29),
                FlightEndDate = new DateTime(2019, 10, 13),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 10, 10), new DateTime(2019, 10, 12), new DateTime(2019, 10, 4), new DateTime(2019, 10, 2) },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 20095158400,
                TotalRatings = 896090.698153806, // not accurate to the total impressions, but that doesn't matter for the test.
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_OneWeekHiatus()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = Entities.Enums.PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 08, 01),
                FlightEndDate = new DateTime(2019, 08, 31),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 5), new DateTime(2019, 8, 6), new DateTime(2019, 8, 7), new DateTime(2019, 8, 8),
                                                        new DateTime(2019, 8, 9), new DateTime(2019, 8, 10), new DateTime(2019, 8, 11)},
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 1000,
                TotalRatings = 0.000907291831869388,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomDelivery()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = Entities.Enums.PlanGoalBreakdownTypeEnum.Custom,
                FlightStartDate = new DateTime(2019, 08, 03),
                FlightEndDate = new DateTime(2019, 08, 20),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 1000,
                TotalRatings = 0.000907291831869388,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                Weeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek {
                      ActiveDays= "Sa,Su",
                      EndDate= new DateTime(2019,08,4),
                      WeeklyImpressions= 200.0,
                      WeeklyRatings = 0.0001814583663738776,
                      MediaWeekId= 814,
                      NumberOfActiveDays= 2,
                      WeeklyImpressionsPercentage= 20.0,
                      StartDate= new DateTime(2019,07,29),
                      WeekNumber= 1
                }}
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomDelivery_ChangeRatings()
        {
            var request = _GetBaseRequestForCustomWeeklyBreakdown();
            request.Weeks.First(w => w.WeekNumber == 2).WeeklyRatings = .03;
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Ratings;

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomDelivery_ChangeImpression()
        {
            var request = _GetBaseRequestForCustomWeeklyBreakdown();
            request.Weeks.First(w => w.WeekNumber == 2).WeeklyImpressions = 300;

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);
            
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomDelivery_ChangePercentage()
        {
            var request = _GetBaseRequestForCustomWeeklyBreakdown();
            request.Weeks.First(w => w.WeekNumber == 2).WeeklyImpressionsPercentage = 30;
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Percentage;

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_UpdateHiatusDaysInCustomDelivery()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Custom,
                FlightStartDate = new DateTime(2019, 09, 30),
                FlightEndDate = new DateTime(2019, 10, 13),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 10, 2) },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 1000,
                TotalRatings = 0.000907291831869388,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                Weeks = new List<WeeklyBreakdownWeek> {
                    new WeeklyBreakdownWeek {
                      ActiveDays= "",
                      EndDate= new DateTime(2019,10,6),
                      WeeklyImpressions= 500,
                      MediaWeekId= 814,
                      NumberOfActiveDays= 7,
                      WeeklyImpressionsPercentage = 50,
                      WeeklyRatings = 0.00045364591593469400,
                      StartDate= new DateTime(2019,09,30),
                      WeekNumber= 1
                    },
                    new WeeklyBreakdownWeek {
                      ActiveDays= "",
                      EndDate= new DateTime(2019,10,13),
                      WeeklyImpressions= 500,
                      MediaWeekId= 814,
                      NumberOfActiveDays= 7,
                      WeeklyImpressionsPercentage = 50,
                      WeeklyRatings = 0.00045364591593469400,
                      StartDate= new DateTime(2019,10,7),
                      WeekNumber= 2
                    },
                }
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_EvenDelivery()
        {
            var request = new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 12, 01),
                FlightEndDate = new DateTime(2019, 12, 31),
                FlightHiatusDays = new List<DateTime>(),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 500,
                TotalRatings = 0.453645915934694,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PlanServiceGetPlanDefaultsTest()
        {
            var defaults = _PlanService.GetPlanDefaults();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(defaults));
        }

        [Test]
        [TestCase("12/09/2019", "12/16/2019 00:00:00")]
        [TestCase("12/10/2019", "12/16/2019 00:00:00")]
        [TestCase("12/11/2019", "12/16/2019 00:00:00")]
        [TestCase("12/12/2019", "12/16/2019 00:00:00")]
        [TestCase("12/13/2019", "12/16/2019 00:00:00")]
        [TestCase("12/14/2019", "12/16/2019 00:00:00")]
        [TestCase("12/15/2019", "12/16/2019 00:00:00")]
        [TestCase("12/16/2019", "12/23/2019 00:00:00")]
        public void GetCurrentQuarters_VerifyStartDate(string currentDateTimeString, string expectedStartDateTimeString)
        {
            var currentDateTime = DateTime.Parse(currentDateTimeString);
            var expectedStartDateTime = DateTime.Parse(expectedStartDateTimeString);

            var currentQuartersResult = _PlanService.GetCurrentQuarters(currentDateTime);

            Assert.AreEqual(expectedStartDateTime, currentQuartersResult.FirstStartDate);
            Assert.AreEqual(5, currentQuartersResult.Quarters.Count);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetCurrentQuarters()
        {
            // this is a Thursday
            var currentDateTime = new DateTime(2019, 12, 12);

            var results = _PlanService.GetCurrentQuarters(currentDateTime);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_OneImpressionPerWeek()
        {
            var request = new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Custom,
                FlightStartDate = new DateTime(2020, 01, 13),
                FlightEndDate = new DateTime(2020, 02, 16),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 5,
                TotalRatings = 0,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                TotalBudget = 85000,
                FlightHiatusDays = new List<DateTime>()
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_LessImpressionThenWeeks()
        {
            var request = new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Custom,
                FlightStartDate = new DateTime(2019, 12, 30),
                FlightEndDate = new DateTime(2020, 02, 02),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 3,
                TotalRatings = 0.002721875495608163,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                TotalBudget = 75000,
                FlightHiatusDays = new List<DateTime>(),
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 836,
                        StartDate = new DateTime(2019,12,30),
                        EndDate = new DateTime(2020,01,05),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 0.4,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyBudget = 15000
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2,
                        MediaWeekId = 837,
                        StartDate = new DateTime(2020,01,06),
                        EndDate = new DateTime(2020,01,12),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 0.4,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyBudget = 15000
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3,
                        MediaWeekId = 838,
                        StartDate = new DateTime(2020,01,13),
                        EndDate = new DateTime(2020,01,19),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 0.4,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyBudget = 15000,
                        WeeklyRatings = 0.00036291673274775507
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4,
                        MediaWeekId = 839,
                        StartDate = new DateTime(2020,01,20),
                        EndDate = new DateTime(2020,01,26),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 0.4,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyBudget = 15000,
                        WeeklyRatings = 0.00036291673274775507
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 5,
                        MediaWeekId = 840,
                        StartDate = new DateTime(2020,01,27),
                        EndDate = new DateTime(2020,02,02),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 0.4,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyBudget = 15000,
                        WeeklyRatings = 0.00036291673274775507
                    }
                }
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_EvenDelivery_WithADU()
        {
            var request = new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 12, 30),
                FlightEndDate = new DateTime(2020, 02, 02),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 5,
                TotalRatings = 0.002721875495608163,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                TotalBudget = 75000,
                FlightHiatusDays = new List<DateTime>(),
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 836,
                        StartDate = new DateTime(2019,12,30),
                        EndDate = new DateTime(2020,01,05),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 1,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyBudget = 15000,
                        WeeklyAdu = 1

                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2,
                        MediaWeekId = 837,
                        StartDate = new DateTime(2020,01,06),
                        EndDate = new DateTime(2020,01,12),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 1,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyBudget = 15000,
                        WeeklyAdu = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3,
                        MediaWeekId = 838,
                        StartDate = new DateTime(2020,01,13),
                        EndDate = new DateTime(2020,01,19),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 1,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyBudget = 15000,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyAdu = 3
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4,
                        MediaWeekId = 839,
                        StartDate = new DateTime(2020,01,20),
                        EndDate = new DateTime(2020,01,26),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 1,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyBudget = 15000,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyAdu = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 5,
                        MediaWeekId = 840,
                        StartDate = new DateTime(2020,01,27),
                        EndDate = new DateTime(2020,02,02),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 1,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyBudget = 15000,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyAdu = 5
                    }
                }
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomDelivery_WithADU()
        {
            var request = new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Custom,
                FlightStartDate = new DateTime(2019, 12, 30),
                FlightEndDate = new DateTime(2020, 02, 02),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 5,
                TotalRatings = 0.002721875495608163,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                TotalBudget = 75000,
                FlightHiatusDays = new List<DateTime>(),
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1,
                        MediaWeekId = 836,
                        StartDate = new DateTime(2019,12,30),
                        EndDate = new DateTime(2020,01,05),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 2,
                        WeeklyImpressionsPercentage = 40,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyBudget = 30000,
                        WeeklyAdu = 1

                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2,
                        MediaWeekId = 837,
                        StartDate = new DateTime(2020,01,06),
                        EndDate = new DateTime(2020,01,12),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 0,
                        WeeklyImpressionsPercentage = 0,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyBudget = 0,
                        WeeklyAdu = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3,
                        MediaWeekId = 838,
                        StartDate = new DateTime(2020,01,13),
                        EndDate = new DateTime(2020,01,19),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 1,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyBudget = 15000,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyAdu = 3
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4,
                        MediaWeekId = 839,
                        StartDate = new DateTime(2020,01,20),
                        EndDate = new DateTime(2020,01,26),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 1,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyBudget = 15000,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyAdu = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 5,
                        MediaWeekId = 840,
                        StartDate = new DateTime(2020,01,27),
                        EndDate = new DateTime(2020,02,02),
                        NumberOfActiveDays = 7,
                        ActiveDays = "M-Su",
                        WeeklyImpressions = 1,
                        WeeklyImpressionsPercentage = 20,
                        WeeklyBudget = 15000,
                        WeeklyRatings = 0.00036291673274775507,
                        WeeklyAdu = 5
                    }
                }
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_EvenDelivery_EmptyWeeks()
        {
            var request = new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 12, 30),
                FlightEndDate = new DateTime(2020, 02, 02),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 5,
                TotalRatings = 0.002721875495608163,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                TotalBudget = 75000,
                FlightHiatusDays = new List<DateTime>(),
                Weeks = null
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomDelivery_EmptyWeeks()
        {
            var request = new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 12, 30),
                FlightEndDate = new DateTime(2020, 02, 02),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 5,
                TotalRatings = 0.002721875495608163,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                TotalBudget = 75000,
                FlightHiatusDays = new List<DateTime>(),
                Weeks = null
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [Ignore("PRI-23204 Pricing is not queued on save, anymore")]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_SaveNewPlan_PricingIsQueued()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _GetNewPlan();

                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                var execution = _PlanPricingService.GetCurrentPricingExecution(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(execution, _GetJsonSettings()));
            }
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
                FlightEndDate = new DateTime(2019, 1, 31),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightNotes = "Sample notes",
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019,1,20),
                    new DateTime(2019,1,24)
                },
                AudienceId = AUDIENCE_ID,       //HH
                AudienceType = AudienceTypeEnum.Nielsen,
                HUTBookId = 437,
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,
                Budget = 100m,
                TargetCPM = 12m,
                TargetImpressions = 100d,
                TargetCPP = 12m,
                Currency = PlanCurrenciesEnum.Impressions,
                TargetRatingPoints = 100d,
                CoverageGoalPercent = 80.5,
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
                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 2,
                        DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews,
                        StartTimeSeconds = 0,
                        EndTimeSeconds = 2000,
                        WeightingGoalPercent = 28.0,
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                ShowTypes = new List<LookupDto> { new LookupDto { Id = 1 } }
                            },
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                Genres = new List<LookupDto> { new LookupDto { Id = 25 } }
                            },
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        ContentRating = "PG-13",
                                        Genre = new LookupDto { Id = 25},
                                        Name = "Young Sheldon"
                                    }
                                }
                            },
                            AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Affiliates = new List<LookupDto> { new LookupDto { Id = 20 } }
                            }
                        }
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 11,
                        DaypartTypeId = DaypartTypeEnum.News,
                        StartTimeSeconds = 1500,
                        EndTimeSeconds = 2788,
                        WeightingGoalPercent = 33.2,
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                ShowTypes = new List<LookupDto> { new LookupDto { Id = 2 } }
                            },
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                Genres = new List<LookupDto> { new LookupDto { Id = 20 } }
                            },
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        ContentRating = "G",
                                        Genre = new LookupDto { Id = 25},
                                        Name = "Teletubbies"
                                    }
                                }
                            }
                        }
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 3,
                        DaypartTypeId = DaypartTypeEnum.ROS,
                        StartTimeSeconds = 57600,
                        EndTimeSeconds = 68400,
                        WeightingGoalPercent = 25,
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                ShowTypes = new List<LookupDto>
                                {
                                    new LookupDto { Id = 9 },
                                    new LookupDto { Id = 11 }
                                }
                            },
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Genres = new List<LookupDto>
                                {
                                    new LookupDto { Id = 12 },
                                    new LookupDto { Id = 14 }
                                }
                            },
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                Programs = new List<ProgramDto>
                                {
                                    new ProgramDto
                                    {
                                        ContentRating = "R",
                                        Genre = new LookupDto { Id = 25},
                                        Name = "Power Rangers"
                                    }
                                }
                            }
                        }
                    }
                },
                Vpvh = 0.012,
                IsAduEnabled = true,
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1, MediaWeekId = 784,
                        StartDate = new DateTime(2018,12,31), EndDate = new DateTime(2019,01,06),
                        NumberOfActiveDays = 6, ActiveDays = "Tu-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyAdu = 5
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2, MediaWeekId = 785,
                        StartDate = new DateTime(2019,01,07), EndDate = new DateTime(2019,01,13),
                        NumberOfActiveDays = 7, ActiveDays = "M-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3, MediaWeekId = 786,
                        StartDate = new DateTime(2019,01,14), EndDate = new DateTime(2019,01,20),
                        NumberOfActiveDays = 6, ActiveDays = "M-Sa", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4, MediaWeekId = 787,
                        StartDate = new DateTime(2019,01,21), EndDate = new DateTime(2019,01,27),
                        NumberOfActiveDays = 6, ActiveDays = "M-W,F-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyAdu = 0
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 5, MediaWeekId = 788,
                        StartDate = new DateTime(2019,01,28), EndDate = new DateTime(2019,02,03),
                        NumberOfActiveDays = 4, ActiveDays = "M-Th", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyAdu = 30
                    }
                }
            };
        }

        private static SaveCampaignDto _GetNewCampaign()
        {
            return new SaveCampaignDto
            {
                Name = "Campaign1",
                AdvertiserId = 1,
                AgencyId = 1,
                Notes = "Notes for CampaignOne."
            };
        }

        private WeeklyBreakdownRequest _GetBaseRequestForCustomWeeklyBreakdown()
        {
            return new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Custom,
                FlightStartDate = new DateTime(2019, 08, 03),
                FlightEndDate = new DateTime(2019, 08, 20),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 1000,
                TotalRatings = .1,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                TotalBudget = 20000,
                Weeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "Sa,Su",
                        EndDate = new DateTime(2019, 08, 4),
                        MediaWeekId = 814,
                        NumberOfActiveDays = 2,
                        StartDate = new DateTime(2019, 07, 29),
                        WeekNumber = 1,
                        WeeklyImpressions = 250,
                        WeeklyImpressionsPercentage = 25.0,
                        WeeklyRatings = 0.025,
                        WeeklyBudget = 5000,
                        WeeklyAdu = 0
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        EndDate = new DateTime(2019, 08, 11),
                        MediaWeekId = 815,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2019, 08, 5),
                        WeekNumber = 2,
                        WeeklyImpressions = 250,
                        WeeklyImpressionsPercentage = 25.0,
                        WeeklyRatings = 0.025,
                        WeeklyBudget = 5000
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-W,F-Su",
                        EndDate = new DateTime(2019, 08, 18),
                        MediaWeekId = 816,
                        NumberOfActiveDays = 6,
                        StartDate = new DateTime(2019, 08, 12),
                        WeekNumber = 3,
                        WeeklyImpressions = 250,
                        WeeklyImpressionsPercentage = 25.0,
                        WeeklyRatings = 0.025,
                        WeeklyBudget = 5000,
                        WeeklyAdu = 5
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M,Tu",
                        EndDate = new DateTime(2019, 08, 25),
                        MediaWeekId = 817,
                        NumberOfActiveDays = 2,
                        StartDate = new DateTime(2019, 08, 19),
                        WeekNumber = 4,
                        WeeklyImpressions = 250,
                        WeeklyImpressionsPercentage = 25.0,
                        WeeklyRatings = 0.025,
                        WeeklyBudget = 5000,
                        WeeklyAdu = 30
                    }
                }
            };
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(PlanDto), "Id");
            jsonResolver.Ignore(typeof(PlanDto), "VersionId");
            jsonResolver.Ignore(typeof(PlanDaypartDto), "Id");
            jsonResolver.Ignore(typeof(PlanMarketDto), "Id");
            jsonResolver.Ignore(typeof(PlanSummaryDto), "PlanId");
            jsonResolver.Ignore(typeof(PlanSummaryDto), "VersionId");
            jsonResolver.Ignore(typeof(PlanVersionDto), "VersionId");
            jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
            jsonResolver.Ignore(typeof(PlanPricingJob), "PlanVersionId");
            jsonResolver.Ignore(typeof(PlanPricingParametersDto), "PlanId");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        private void _ForceCompletePlanPricingJob(int planId)
        {
            var planRepo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            var job = planRepo.GetLatestPricingJob(planId);
            if(job != null)
            {
                job.Status = BackgroundJobProcessingStatus.Succeeded;
                job.Completed = DateTime.Now;
                planRepo.UpdatePlanPricingJob(job);
            }
        }
    }
}
