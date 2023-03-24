using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.IntegrationTests.Stubs;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanServiceIntegrationTests
    {
        private IPlanService _PlanService;
        private ICampaignService _CampaignService;
        private IPlanPricingService _PlanPricingService;
        private IPlanRepository _PlanRepository;
        private LaunchDarklyClientStub _LaunchDarklyClientStub;

        private const int AUDIENCE_ID = 31;

        [SetUp]
        public void SetUp()
        {
            _LaunchDarklyClientStub = (LaunchDarklyClientStub)IntegrationTestApplicationServiceFactory.Instance.Resolve<ILaunchDarklyClient>();
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IPricingApiClient, PricingApiClientStub>();
            _PlanService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanService>();
            _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();
            _PlanPricingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
            _PlanRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
        }

        private void _SetFeatureToggle(string feature, bool activate)
        {
            if (_LaunchDarklyClientStub.FeatureToggles.ContainsKey(feature))
                _LaunchDarklyClientStub.FeatureToggles[feature] = activate;
            else
                _LaunchDarklyClientStub.FeatureToggles.Add(feature, activate);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetPlanStatuses()
        {
            var statuses = _PlanService.GetPlanStatuses();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(statuses));
        }

        [Test]
        [Category("long_running")]
        public async Task CreateNewPlan()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                DateTime nowDate = new DateTime(2019, 01, 01);
                string username = "integration_test";
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, username, nowDate);
                var campaign = _CampaignService.GetCampaignById(newPlan.CampaignId);
                Assert.IsTrue(newPlanId > 0);
                Assert.AreEqual(username, campaign.ModifiedBy);
                Assert.AreEqual(nowDate, campaign.ModifiedDate);
            }
        }
        [Test]
        [Category("long_running")]
        public async Task CopyPlans()
        {           
                using (new TransactionScopeWrapper())
                {
                    PlanDto newPlan = _GetNewUnifiedPlan();
                    var campaignId = newPlan.CampaignId;
                    string createdBy = "integration_test";
                    DateTime createdDate = new DateTime(2019, 01, 01);
                    var newPlanId = await _PlanService.SavePlanAsync(newPlan, createdBy, createdDate);
                    var campaign = _CampaignService.GetCampaignById(newPlan.CampaignId);
                    var campaignCopy = _GetSaveCampaignCopy(newPlanId);
                    _PlanService.CopyPlans(campaignId, campaignCopy, createdBy, createdDate);
                    var latestPlan = _PlanRepository.GetPlansForCampaign(campaignId).OrderByDescending(x=> x.Id).FirstOrDefault();
                    Assert.AreEqual(null, latestPlan.UnifiedTacticLineId);
                    Assert.AreEqual(null, latestPlan.UnifiedCampaignLastReceivedAt);
                    Assert.AreEqual(null, latestPlan.UnifiedCampaignLastSentAt);                   
                }       
                 

        }
        private SaveCampaignCopyDto _GetSaveCampaignCopy(int planId)
        {
            return new SaveCampaignCopyDto
            {
                Name = "Campaign1",
                AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                AgencyMasterId = new Guid("A8B10A69-FBD6-43FE-B143-156B7297B62D"),
                Plans = new List<SavePlansCopyDto>
                {
                    new SavePlansCopyDto
                    {
                          SourcePlanId =planId,
                          Name = "New Plan",
                          ProductMasterId ="6BEF080E-01ED-4D42-BE54-927110457907"
                    }
                }
            };
        }
        private static PlanDto _GetNewUnifiedPlan()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                ProductMasterId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907"),              
                CreativeLengths = new List<CreativeLength> {
                    new CreativeLength { SpotLengthId = 1, Weight = 50},
                    new CreativeLength{ SpotLengthId = 2}
                },
                Status = PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 1, 31),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightNotes = "Sample notes",
                FlightNotesInternal = "Sample internal notes",
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
                TargetUniverse = 111222d,
                Currency = PlanCurrenciesEnum.Impressions,
                TargetRatingPoints = 100d,
                CoverageGoalPercent = 80.5,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 48, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn", IsUserShareOfVoicePercent = true},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York", IsUserShareOfVoicePercent = true}
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
                        WeekdaysWeighting = 60,
                        WeekendWeighting = 40,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.8,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        },
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
                        WeightingGoalPercent = 33,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.9,
                                VpvhType = VpvhTypeEnum.LastYear,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        },
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
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.7,
                                VpvhType = VpvhTypeEnum.PreviousQuarter,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        },
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
                        WeeklyAdu = 5,
                        SpotLengthId = 1,
                        DaypartCodeId = 1,
                        PercentageOfWeek = 50,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2, MediaWeekId = 785,
                        StartDate = new DateTime(2019,01,07), EndDate = new DateTime(2019,01,13),
                        NumberOfActiveDays = 7, ActiveDays = "M-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3, MediaWeekId = 786,
                        StartDate = new DateTime(2019,01,14), EndDate = new DateTime(2019,01,20),
                        NumberOfActiveDays = 6, ActiveDays = "M-Sa", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4, MediaWeekId = 787,
                        StartDate = new DateTime(2019,01,21), EndDate = new DateTime(2019,01,27),
                        NumberOfActiveDays = 6, ActiveDays = "M-W,F-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 5, MediaWeekId = 788,
                        StartDate = new DateTime(2019,01,28), EndDate = new DateTime(2019,02,03),
                        NumberOfActiveDays = 4, ActiveDays = "M-Th", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyAdu = 30,
                        WeeklyUnits = 4
                    }
                },
                ImpressionsPerUnit = 5,
                UnifiedTacticLineId = "Test",
                UnifiedCampaignLastSentAt = DateTime.Now,
                UnifiedCampaignLastReceivedAt = DateTime.Now
            };
        }

        [Test]
        [Category("long_running")]
        public async Task CreateNewPlan_ContractedPlan()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Status = PlanStatusEnum.Contracted;

                DateTime nowDate = new DateTime(2019, 01, 01);
                string username = "integration_test";
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, username, nowDate);
                var plan = _PlanService.GetPlan(newPlanId);

                Assert.IsTrue(newPlanId > 0);
                Assert.AreEqual(plan.GoalBreakdownType, PlanGoalBreakdownTypeEnum.CustomByWeek);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task CreateNewPlan_DefaultWeight()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.CreativeLengths.RemoveAt(1);

                DateTime nowDate = new DateTime(2019, 01, 01);
                string username = "integration_test";
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, username, nowDate);
                var savedPlan = _PlanService.GetPlan(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task CreateNewPlan_WithAduDisabled()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.IsAduEnabled = false;

                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);

                var planVersion = _PlanService.GetPlan(newPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(planVersion), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task CreateNewPlan_NullHutBook()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.HUTBookId = null;
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                var planVersion = _PlanService.GetPlan(newPlanId);

                Assert.IsTrue(newPlanId > 0);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(planVersion), _GetJsonSettings()));
            }
        }

        [Test]
        [Category("long_running")]
        public async Task CreateNewDraft()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Id = 523;   //existing plan in database
                newPlan.VersionId = 1;
                newPlan.IsDraft = true;

                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                var draftId = _PlanService.CheckForDraft(newPlanId);

                Assert.IsTrue(draftId > 0);
                Assert.IsTrue(newPlanId > 0);
            }
        }

        [Test]
        [Category("long_running")]
        public async Task SavePlan_InvalidMarketCoverage_PRI17598()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 197, MarketCoverageFileId = 1, PercentageOfUS = 0.051, Rank = 1, ShareOfVoicePercent = 8, Market = "Parkersburg", IsUserShareOfVoicePercent = true},
                    new PlanAvailableMarketDto { MarketCode = 261, MarketCoverageFileId = 1, PercentageOfUS = 0.048, Rank = 2, ShareOfVoicePercent = 8, Market = "San Angelo", IsUserShareOfVoicePercent = true},
                    new PlanAvailableMarketDto { MarketCode = 359, MarketCoverageFileId = 1, PercentageOfUS = 0.048, Rank = 2, ShareOfVoicePercent = 8, Market = "Cheyenne-Scottsbluf", IsUserShareOfVoicePercent = true},
                    new PlanAvailableMarketDto { MarketCode = 367, MarketCoverageFileId = 1, PercentageOfUS = 0.048, Rank = 2, ShareOfVoicePercent = 8, Market = "Casper-Riverton", IsUserShareOfVoicePercent = true},
                    new PlanAvailableMarketDto { MarketCode = 340, MarketCoverageFileId = 1, PercentageOfUS = 0.048, Rank = 2, ShareOfVoicePercent = 8, Market = "North Platte", IsUserShareOfVoicePercent = true},
                };
                newPlan.CoverageGoalPercent = 0.2;

                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_InvalidDayparts_DuplicateDayparts()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                var daypart = newPlan.Dayparts.First();
                newPlan.Dayparts.Add(daypart);
                var hasDuplicates = newPlan.Dayparts.GroupBy(d => d.DaypartCodeId).Any(d => d.Count() > 1);

                PlanValidationException exception = null;

                try
                {
                    await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                }
                catch (PlanValidationException ex)
                {
                    exception = ex;
                }

                Assert.IsNotNull(exception);
                Assert.IsTrue(hasDuplicates);
                Assert.AreEqual(exception.Message, "Invalid dayparts.  Each daypart can be entered only once. Try to save the plan as draft");
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlan_GoalBreakdownType_ByWeekByDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _GetNewPlan();
                newPlan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart;
                newPlan.FlightStartDate = new DateTime(2020, 2, 24);
                newPlan.FlightEndDate = new DateTime(2020, 3, 29);
                newPlan.FlightHiatusDays = new List<DateTime> { };
                newPlan.FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
                newPlan.Dayparts.RemoveAt(2);
                newPlan.Dayparts[0].DaypartCodeId = 15;
                newPlan.Dayparts[0].WeightingGoalPercent = 60;
                newPlan.Dayparts[1].DaypartCodeId = 20;
                newPlan.Dayparts[0].WeightingGoalPercent = 40;
                newPlan.TargetImpressions = 5000;
                newPlan.ImpressionsPerUnit = 100;
                newPlan.TargetRatingPoints = 4.1;
                newPlan.TargetCPM = 0.1m;
                newPlan.Budget = 500;
                newPlan.WeeklyBreakdownTotals = new WeeklyBreakdownTotals
                {
                    TotalActiveDays = 35,
                    TotalBudget = 500,
                    TotalImpressions = 5000,
                    TotalImpressionsPercentage = 100,
                    TotalRatingPoints = 4.1,
                    TotalUnits = 50
                };
                newPlan.WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 15,
                        EndDate = new DateTime(2020, 3, 1),
                        MediaWeekId = 844,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,2,24),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 20,
                        EndDate = new DateTime(2020, 3, 1),
                        MediaWeekId = 844,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,2,24),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 1
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 15,
                        EndDate = new DateTime(2020, 3, 8),
                        MediaWeekId = 845,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,2),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 20,
                        EndDate = new DateTime(2020, 3, 8),
                        MediaWeekId = 845,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,2),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 2
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 15,
                        EndDate = new DateTime(2020, 3, 15),
                        MediaWeekId = 846,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,9),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 3
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 20,
                        EndDate = new DateTime(2020, 3, 15),
                        MediaWeekId = 846,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,9),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 3
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 15,
                        EndDate = new DateTime(2020, 3, 22),
                        MediaWeekId = 847,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,16),
                        PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 20,
                        EndDate = new DateTime(2020, 3, 22),
                        MediaWeekId = 847,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,16),
                        PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 15,
                        EndDate = new DateTime(2020, 3, 29),
                        MediaWeekId = 848,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,23),
                       PercentageOfWeek = 60,
                        WeeklyBudget = 60,
                        WeeklyImpressions = 600,
                        WeeklyImpressionsPercentage = 12,
                        WeeklyRatings = 0.48,
                        WeeklyUnits = 6,
                        WeekNumber = 5
                    },
                    new WeeklyBreakdownWeek
                    {
                        ActiveDays = "M-Su",
                        DaypartCodeId = 20,
                        EndDate = new DateTime(2020, 3, 29),
                        MediaWeekId = 848,
                        NumberOfActiveDays = 7,
                        StartDate = new DateTime(2020,3,23),
                       PercentageOfWeek = 40,
                        WeeklyBudget = 40,
                        WeeklyImpressions = 400,
                        WeeklyImpressionsPercentage = 8,
                        WeeklyRatings = 0.32,
                        WeeklyUnits = 4,
                        WeekNumber = 5
                    },
                };

                var nowDate = new DateTime(2020, 01, 01);
                var username = "integration_test";
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, username, nowDate);

                var savedPlan = _PlanService.GetPlan(newPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(savedPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [Category("long_running")]
        public async Task DeleteDraft()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                //get the plan and format the impressions
                PlanDto plan = _PlanService.GetPlan(newPlanId);

                //save draft
                plan.IsDraft = true;
                await _PlanService.SavePlanAsync(plan, "integration_test", new DateTime(2019, 01, 07));

                var draftId = _PlanService.CheckForDraft(newPlanId);
                Assert.IsTrue(draftId > 0);

                //delete draft and check
                _PlanService.DeletePlanDraft(newPlanId);
                var draftIdDeleted = _PlanService.CheckForDraft(newPlanId);
                Assert.IsTrue(draftIdDeleted == 0);
            }
        }

        [Test]
        [Category("short_running")]
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
        [Category("short_running")]
        public void GetPlanVersion()
        {
            using (new TransactionScopeWrapper())
            {
                var planVersion = _PlanService.GetPlan(523, 1);

                Assert.IsTrue(planVersion.Id > 0);
                Assert.IsTrue(planVersion.VersionId > 0);
                // order these for verification matching
                planVersion.AvailableMarkets = planVersion.AvailableMarkets.OrderBy(m => m.Rank).ToList();
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(planVersion, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task CanCreateNewPlanWithCanceledStatus()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Status = PlanStatusEnum.Canceled;

                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
                PlanDto finalPlan = _PlanService.GetPlan(newPlanId);

                Assert.AreEqual(PlanStatusEnum.Canceled, finalPlan.Status);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetPlanHistory()
        {
            using (new TransactionScopeWrapper())
            {
                var planHistory = _PlanService.GetPlanHistory(1848);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(planHistory, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task GetPlanHistory_WithDraft()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                //save draft
                PlanDto plan = _PlanService.GetPlan(newPlanId);
                plan.IsDraft = true;
                await _PlanService.SavePlanAsync(plan, "integration_test", new DateTime(2019, 01, 01));

                var planHistory = _PlanService.GetPlanHistory(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(planHistory, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task CreatingPlanSetsVersionNumber()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));

                //get the plan
                PlanDto plan = _PlanService.GetPlan(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(plan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task CreatingNewVersionForPlanSetsVersionNumber()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                //get the plan
                PlanDto plan = _PlanService.GetPlan(newPlanId);

                //save version 2
                plan.Budget = 222;
                plan.WeeklyBreakdownWeeks.FirstOrDefault().WeeklyBudget = 142;
                await _PlanService.SavePlanAsync(plan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                //get the plan again
                plan = _PlanService.GetPlan(newPlanId);

                Assert.AreEqual(222, plan.PricingParameters.Budget);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(plan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task CreatingDraftForPlanDoesntUpdateVersionNumber()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                //save version 1
                int newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));

                _ForceCompletePlanPricingJob(newPlanId);

                //get the plan
                PlanDto plan = _PlanService.GetPlan(newPlanId);

                //create draft
                plan.Budget = 222;
                plan.WeeklyBreakdownWeeks.LastOrDefault().WeeklyBudget = 142;
                plan.IsDraft = true;
                await _PlanService.SavePlanAsync(plan, "integration_test", new DateTime(2019, 01, 01));

                //get the plan again
                plan = _PlanService.GetPlan(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(plan), _GetJsonSettings()));
            }
        }

        [Test]
        [Category("long_running")]
        [UseReporter(typeof(DiffReporter))]
        public async Task CreatingPlansCheckCampaignAggregation()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto firstNewPlan = _GetNewPlan();
                firstNewPlan.Status = PlanStatusEnum.Canceled;

                var firstNewPlanId = await _PlanService.SavePlanAsync(firstNewPlan, "integration_test", new DateTime(2019, 10, 17));

                PlanDto secondNewPlan = _GetNewPlan();
                secondNewPlan.Status = PlanStatusEnum.Rejected;

                var secondNewPlanId = await _PlanService.SavePlanAsync(secondNewPlan, "integration_test", new DateTime(2019, 10, 17));

                PlanDto thirdNewPlan = _GetNewPlan();
                thirdNewPlan.Status = PlanStatusEnum.Scenario;

                var thirdNewPlanId = await _PlanService.SavePlanAsync(thirdNewPlan, "integration_test", new DateTime(2019, 10, 17));

                Assert.IsTrue(firstNewPlanId > 0);
                Assert.IsTrue(secondNewPlanId > 0);
                Assert.IsTrue(thirdNewPlanId > 0);

                _CampaignService.ProcessCampaignAggregation(1);

                var campaign = _CampaignService.GetCampaignById(1);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(campaign, _GetJsonSettings()));
            }
        }

        [Test]
        [Category("long_running")]
        public async Task CreatingASingleCanceledPlanFiltersCampaignsCorrectly()
        {
            using (new TransactionScopeWrapper())
            {
                var _CampaignService = IntegrationTestApplicationServiceFactory.GetApplicationService<ICampaignService>();

                SaveCampaignDto newCampaign = _GetNewCampaign();
                var newCampaignId = _CampaignService.SaveCampaign(newCampaign, "integration_test", new DateTime(2019, 10, 30));

                Assert.IsTrue(newCampaignId > 0);

                PlanDto newCanceledPlan = _GetNewPlan();
                newCanceledPlan.CampaignId = newCampaignId;
                newCanceledPlan.Status = PlanStatusEnum.Canceled;

                var newPlanId = await _PlanService.SavePlanAsync(newCanceledPlan, "integration_test", new DateTime(2019, 10, 30));

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
        [Category("long_running")]
        public async Task CreatingDraftPlanPopulatesGetCampaignFlagsCorrectly()
        {
            using (new TransactionScopeWrapper())
            {
                SaveCampaignDto newCampaign = _GetNewCampaign();
                var newCampaignId = _CampaignService.SaveCampaign(newCampaign, "integration_test", new DateTime(2019, 10, 30));

                Assert.IsTrue(newCampaignId > 0);

                PlanDto newPlan = _GetNewPlan();
                newPlan.CampaignId = newCampaignId;
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 10, 30), true);

                _ForceCompletePlanPricingJob(newPlanId);

                var planFromDB = _PlanService.GetPlan(newPlanId);
                planFromDB.IsDraft = true;
                var draftPLan = await _PlanService.SavePlanAsync(planFromDB, "integration_test", new DateTime(2019, 10, 30), true);

                _CampaignService.ProcessCampaignAggregation(newCampaignId);
                var campaign = _CampaignService.GetCampaignById(newCampaignId);

                Assert.AreEqual(1, campaign.Plans.Count);
                Assert.AreEqual("integration_test", campaign.Plans.First().DraftModifiedBy);
                Assert.AreEqual(new DateTime(2019, 10, 30), campaign.Plans.First().DraftModifiedDate);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task CanCreateNewPlanWithRejectedStatus()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Status = PlanStatusEnum.Rejected;

                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
                PlanDto finalPlan = _PlanService.GetPlan(newPlanId);

                Assert.AreEqual(PlanStatusEnum.Rejected, finalPlan.Status);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task CreatePlan_InvalidSpotLengthId()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 100, Weight = 50 } };

                PlanValidationException exception = null;

                try
                {
                    await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                }
                catch (PlanValidationException ex)
                {
                    exception = ex;
                }

                Assert.IsNotNull(exception);
                Assert.That(exception.Message, Is.EqualTo("Invalid spot length id 100 Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("long_running")]
        public async Task CreatePlan_NotExistingProduct()
        {
            Guid notExistingProductId = new Guid();
            using (new TransactionScopeWrapper())
            {
                var configurationSettingsHelper = new ConfigurationSettingsHelper();
                var featureToggleHelper = new FeatureToggleHelper(_LaunchDarklyClientStub);

                var planService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanService>();

                PlanDto newPlan = _GetNewPlan();
                newPlan.ProductMasterId = notExistingProductId;

                PlanValidationException caught = null;

                try
                {
                    await planService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                }
                catch (PlanValidationException ex)
                {
                    caught = ex;
                }

                Assert.IsNotNull(caught);
                Assert.AreEqual("Invalid product Try to save the plan as draft", caught.Message);
            }
        }

        [Test]
        [Category("short_running")]
        public async Task CreatePlan_InvalidName()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Name = null;

                PlanValidationException exception = null;

                try
                {
                    await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                }
                catch (PlanValidationException ex)
                {
                    exception = ex;
                }

                Assert.IsNotNull(exception);
                Assert.That(exception.Message, Is.EqualTo("Invalid plan name. Try to save the plan as draft"));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetPlan()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _PlanService.GetPlan(2190);
                newPlan.WeeklyBreakdownWeeks = newPlan.WeeklyBreakdownWeeks.OrderBy(x => x.WeekNumber).ToList();

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(newPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlan()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                // modify the plan
                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.Name = "Renamed Plan";
                testPlan.ProductId = 2;
                // modify the flight.
                testPlan.FlightNotes = "Changed the flight notes";
                testPlan.FlightNotesInternal = "Changed the internal flight notes";
                testPlan.FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019, 1, 28),
                    new DateTime(2019, 1, 4)
                };

                var modifedPlanId = await _PlanService.SavePlanAsync(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlan_ProgramRestrictions_DuplicateProgramName()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 2,
                        DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews,
                        StartTimeSeconds = 0,
                        EndTimeSeconds = 2000,
                        WeightingGoalPercent = 28.0,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.8,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        },
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
                                    },
                                    new ProgramDto
                                    {
                                        ContentRating = "PG-13",
                                        Genre = new LookupDto { Id = 9},
                                        Name = "Young Sheldon"
                                    },
                                }
                            },
                            AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Affiliates = new List<LookupDto> { new LookupDto { Id = 20 } }
                            }
                        }
                    }
                };

                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(testPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlan_WithAduDisabled()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                // modify the plan
                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.IsAduEnabled = false;

                var modifedPlanId = await _PlanService.SavePlanAsync(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlanAndRemoveHiatusDays()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);

                // modify the plan
                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.Name = "Renamed Plan";
                testPlan.ProductId = 2;
                // modify the flight.
                testPlan.FlightNotes = "Changed the flight notes";
                testPlan.FlightNotesInternal = "Changed the internal flight notes";
                testPlan.FlightHiatusDays = new List<DateTime>();

                var modifedPlanId = await _PlanService.SavePlanAsync(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithoutFlightInfo()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto testPlan = _GetNewPlan();
                testPlan.FlightStartDate = null;
                testPlan.FlightEndDate = null;
                testPlan.FlightNotes = null;
                testPlan.FlightNotesInternal = null;
                testPlan.FlightHiatusDays.Clear();

                Assert.That(async () => await _PlanService.SavePlanAsync(testPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid flight start/end date. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidFlightDays()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.FlightStartDate = new DateTime(2019, 10, 1);
                newPlan.FlightEndDate = new DateTime(2018, 01, 01);

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid flight dates.  The end date cannot be before the start date. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidHiatusDays()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(1968, 1, 28),
                    new DateTime(1976, 6, 4)
                };

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid flight hiatus day.  All days must be within the flight date range. Try to save the plan as draft"));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlanWithDayparts()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                var planId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlanWithDayparts_WithoutRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                newPlan.Dayparts.First().Restrictions = null;

                var planId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlanWithDayparts_WithoutShowTypeRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                newPlan.Dayparts.First().Restrictions.ShowTypeRestrictions = null;

                var planId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        private PlanDto _OrderPlanData(PlanDto plan)
        {
            // just any ordering that makes sure data is the same for different test runs
            plan.Dayparts = plan.Dayparts
                .OrderBy(x => x.DaypartCodeId)
                .ThenBy(x => x.Restrictions.AffiliateRestrictions.Affiliates.Count)
                .ThenBy(x => x.Restrictions.GenreRestrictions.Genres.Count)
                .ThenBy(x => x.Restrictions.ShowTypeRestrictions.ShowTypes.Count)
                .ToList();

            return plan;
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlanWithDayparts_WithoutGenreRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                newPlan.Dayparts.First().Restrictions.GenreRestrictions = null;

                var planId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlanWithDayparts_WithoutProgramRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                newPlan.Dayparts.First().Restrictions.ProgramRestrictions = null;

                var planId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlanWithDayparts_WithoutAffiliateRestrictions()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                newPlan.Dayparts.First().Restrictions.AffiliateRestrictions = null;

                var planId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [Category("long_running")]
        [UseReporter(typeof(DiffReporter))]
        public async Task SavePlanAddDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                var planId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 15));
                _ForceCompletePlanPricingJob(planId);

                PlanDto modifiedPlan = _PlanService.GetPlan(planId);
                modifiedPlan.Dayparts.Add(new PlanDaypartDto
                {
                    DaypartCodeId = 9,
                    DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews,
                    StartTimeSeconds = 54000,
                    EndTimeSeconds = 64799,
                    WeightingGoalPercent = 13.8,
                    VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                    {
                        new PlanDaypartVpvhForAudienceDto
                        {
                            AudienceId = 31,
                            Vpvh = 0.8,
                            VpvhType = VpvhTypeEnum.FourBookAverage,
                            StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                        }
                    },
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

                var modifiedPlanId = await _PlanService.SavePlanAsync(modifiedPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifiedPlanId);

                Assert.AreEqual(4, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlanRemoveDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                var planId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 15));
                _ForceCompletePlanPricingJob(planId);
                PlanDto modifiedPlan = _OrderPlanData(_PlanService.GetPlan(planId));
                modifiedPlan.Dayparts.RemoveAt(modifiedPlan.Dayparts.Count - 1);


                var modifiedPlanId = await _PlanService.SavePlanAsync(modifiedPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifiedPlanId);

                Assert.AreEqual(2, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidWeightingGoalTooLittle()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartTypeId = DaypartTypeEnum.News, DaypartCodeId = 1, StartTimeSeconds = 8900, EndTimeSeconds = 4600, WeightingGoalPercent = 0.0 });

                Assert.Throws<PlanValidationException>(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), "Invalid daypart weighting goal.");
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidWeightingGoalTooHigh()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartTypeId = DaypartTypeEnum.News, DaypartCodeId = 1, StartTimeSeconds = 4600, EndTimeSeconds = 8900, WeightingGoalPercent = 111.0 });

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid daypart weighting goal. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("long_running")]
        [UseReporter(typeof(DiffReporter))]
        public async Task SavePlan_WithSecondaryAudiences()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SecondaryAudiences = new List<PlanAudienceDto>()
                {
                    new PlanAudienceDto { AudienceId = 7, Type = AudienceTypeEnum.Nielsen },
                    new PlanAudienceDto { AudienceId = 6, Type = AudienceTypeEnum.Nielsen }
                };
                newPlan.Dayparts[0].VpvhForAudiences.Add(new PlanDaypartVpvhForAudienceDto
                {
                    AudienceId = 7,
                    Vpvh = 0.8,
                    VpvhType = VpvhTypeEnum.FourBookAverage,
                    StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                });
                newPlan.Dayparts[0].VpvhForAudiences.Add(new PlanDaypartVpvhForAudienceDto
                {
                    AudienceId = 6,
                    Vpvh = 0.8,
                    VpvhType = VpvhTypeEnum.FourBookAverage,
                    StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                });
                newPlan.Dayparts[1].VpvhForAudiences.Add(new PlanDaypartVpvhForAudienceDto
                {
                    AudienceId = 7,
                    Vpvh = 0.8,
                    VpvhType = VpvhTypeEnum.FourBookAverage,
                    StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                });
                newPlan.Dayparts[1].VpvhForAudiences.Add(new PlanDaypartVpvhForAudienceDto
                {
                    AudienceId = 6,
                    Vpvh = 0.8,
                    VpvhType = VpvhTypeEnum.FourBookAverage,
                    StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                });
                newPlan.Dayparts[2].VpvhForAudiences.Add(new PlanDaypartVpvhForAudienceDto
                {
                    AudienceId = 7,
                    Vpvh = 0.8,
                    VpvhType = VpvhTypeEnum.FourBookAverage,
                    StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                });
                newPlan.Dayparts[2].VpvhForAudiences.Add(new PlanDaypartVpvhForAudienceDto
                {
                    AudienceId = 6,
                    Vpvh = 0.8,
                    VpvhType = VpvhTypeEnum.FourBookAverage,
                    StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                });

                var planId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                var finalPlan = _PlanService.GetPlan(planId);
                Assert.IsTrue(planId > 0);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidSecondaryAudience()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SecondaryAudiences = new List<PlanAudienceDto>()
                {
                    new PlanAudienceDto {AudienceId = 0, Type = Entities.Enums.AudienceTypeEnum.Nielsen},
                };

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid audience. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidSecondaryAudienceDuplicate()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SecondaryAudiences = new List<PlanAudienceDto>()
                {
                    new PlanAudienceDto {AudienceId = 31, Type = Entities.Enums.AudienceTypeEnum.Nielsen},
                };

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("An audience cannot appear multiple times. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidFlightNotes()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto testPlan = _GetNewPlan();
                testPlan.FlightNotes = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque porttitor tellus at ante tempus vehicula ac at sapien. Pellentesque lorem velit, sodales in ex quis, laoreet dictum risus. Quisque odio sapien, dignissim a lacus et, dignissim auctor urna. Vestibulum tempus dui tortor, nec fermentum massa pharetra sit amet. Morbi fermentum ornare scelerisque. Proin ut lectus in nisl vulputate mattis in in ex. Nam erat sem, convallis condimentum velit blandit, scelerisque condimentum dolor. Maecenas fermentum feugiat lectus. Phasellus et sem in velit hendrerit sodales. Suspendisse porta nec felis ac blandit. In eu nisi ut dui tristique mattis. Vivamus vulputate, elit sit amet porta molestie, justo mauris cursus ipsum, et rhoncus arcu odio id enim. Pellentesque elementum posuere nibh ac rutrum. Donec eget erat nec lorem feugiat ornare vel congue nibh. Nulla cursus bibendum sollicitudin. Quisque viverra ante massa, sed molestie augue rutrum sed. Aenean tempus vitae purus sed lobortis. Sed cursus tempor erat ac pulvinar.";

                Assert.That(async () => await _PlanService.SavePlanAsync(testPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Flight notes cannot be longer than 1024 characters. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("long_running")]
        public async Task SavePlan_NullFlightDays()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _GetNewPlan();
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);
                var testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.FlightDays = null;

                Assert.That(async() => await _PlanService.SavePlanAsync(testPlan, "integration_test", new DateTime(2019, 01, 15)),
                    Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid flight days. The plan should have at least one flight day. Try to save the plan as draft"));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlan_EmptyFlightDays()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _GetNewPlan();
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new DateTime(2019, 01, 01));
                _ForceCompletePlanPricingJob(newPlanId);
                var testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.FlightDays = new List<int>();

                Assert.That(async () => await _PlanService.SavePlanAsync(testPlan, "integration_test", new DateTime(2019, 01, 15)),
                    Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid flight days. The plan should have at least one flight day. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("long_running")]
        public void AutomaticStatusTransition_ContractedToLive()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _PlanService.GetPlan(1852);
                var transitionDateToLive = newPlan.FlightStartDate.Value;
                var updatedDate = newPlan.FlightStartDate.Value.AddDays(1);
                Assert.AreEqual(6, newPlan.VersionNumber);

                _PlanService.AutomaticStatusTransitions(transitionDateToLive, "integration_test", updatedDate);

                var updatedPlan = _PlanService.GetPlan(1852);

                Assert.AreEqual(PlanStatusEnum.Live, updatedPlan.Status);
                Assert.AreEqual(7, updatedPlan.VersionNumber);
                Assert.AreEqual(updatedPlan.PricingParameters.JobId, newPlan.PricingParameters.JobId);
            }
        }

        [Test]
        [Category("long_running")]
        public void AutomaticStatusTransition_LiveToComplete()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _PlanService.GetPlan(1853);

                var transitionDateToComplete = newPlan.FlightEndDate.Value.AddDays(1);
                var updatedDate = newPlan.FlightEndDate.Value;
                Assert.AreEqual(6, newPlan.VersionNumber);

                _PlanService.AutomaticStatusTransitions(transitionDateToComplete, "integration_test", updatedDate);

                var updatedPlan = _PlanService.GetPlan(1853);

                Assert.AreEqual(PlanStatusEnum.Complete, updatedPlan.Status);
                Assert.AreEqual(7, updatedPlan.VersionNumber);
                Assert.AreEqual(updatedPlan.PricingParameters.JobId, newPlan.PricingParameters.JobId);
            }
        }

        [Test]
        [Category("long_running")]
        public void AutomaticStatusTransition_RemainsLive()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _PlanService.GetPlan(1853);

                var transitionDateToComplete = newPlan.FlightEndDate.Value;
                var updatedDate = newPlan.FlightEndDate.Value;
                Assert.AreEqual(6, newPlan.VersionNumber);

                _PlanService.AutomaticStatusTransitions(transitionDateToComplete, "integration_test", updatedDate);

                var samePlan = _PlanService.GetPlan(1853);

                Assert.AreEqual(PlanStatusEnum.Live, samePlan.Status);
                Assert.AreEqual(6, samePlan.VersionNumber);
                Assert.AreEqual(samePlan.PricingParameters.JobId, newPlan.PricingParameters.JobId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetPlanCurrencies()
        {
            var currencies = _PlanService.GetPlanCurrencies();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(currencies));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void GetPlanDeliveryTypes()
        {
            var deliveryTypes = _PlanService.PlanGoalBreakdownTypes();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(deliveryTypes));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase1()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                Budget = 100m,
                Impressions = 3000d,
                AudienceId = AUDIENCE_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase2()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                CPM = 100m,
                Impressions = 3000d,
                AudienceId = AUDIENCE_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase3()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                CPM = 100m,
                Budget = 3000m,
                AudienceId = AUDIENCE_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase4()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                CPP = 100m,
                Budget = 3000m,
                AudienceId = AUDIENCE_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase5()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                CPP = 100m,
                RatingPoints = 3000d,
                AudienceId = AUDIENCE_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase6()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                Budget = 100m,
                RatingPoints = 3000d,
                AudienceId = AUDIENCE_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase7()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                Budget = 1000,
                RatingPoints = 0,
                Impressions = 25000,
                AudienceId = 34
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase8()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                CPM = 13.45m,
                Impressions = 13.456d,
                AudienceId = AUDIENCE_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase9()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                Budget = 3000,
                Impressions = 3000d,
                AudienceId = AUDIENCE_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase10()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                CPM = 1,
                Impressions = 3000d,
                AudienceId = AUDIENCE_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Calculator_TestCase11()
        {
            var result = _PlanService.CalculateBudget(new PlanDeliveryBudget
            {
                CPM = 1,
                Budget = 3000,
                AudienceId = AUDIENCE_ID
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [Category("short_running")]
        public void Calculator_InvalidObject()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculateBudget(new PlanDeliveryBudget
                {
                    Budget = null,
                    CPM = null,
                    Impressions = null,
                    AudienceId = AUDIENCE_ID
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("At least 2 values needed to calculate goal amount"));
            }
        }

        [Test]
        [Category("short_running")]
        public void Calculator_WithoutAudienceId()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculateBudget(new PlanDeliveryBudget
                {
                    Budget = 1,
                    CPM = 1,
                    Impressions = 1,
                    AudienceId = 0
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Cannot calculate goal without an audience"));
            }
        }

        [Test]
        [Category("short_running")]
        public void Calculator_InvalidObject_NegativeValues()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculateBudget(new PlanDeliveryBudget
                {
                    Budget = -1,
                    CPM = null,
                    Impressions = null
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid budget values passed"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidStartTime_TooLittle()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartTypeId = DaypartTypeEnum.News, DaypartCodeId = 1, StartTimeSeconds = -2, EndTimeSeconds = 4600, WeightingGoalPercent = 111.0 });

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid daypart times. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidStartTime_TooMuch()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartTypeId = DaypartTypeEnum.News, DaypartCodeId = 1, StartTimeSeconds = 999999999, EndTimeSeconds = 4600, WeightingGoalPercent = 111.0 });

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid daypart times. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidEndTime_TooLittle()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartTypeId = DaypartTypeEnum.News, DaypartCodeId = 1, StartTimeSeconds = 8900, EndTimeSeconds = -2, WeightingGoalPercent = 111.0 });

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid daypart times. Try to save the plan as draft"));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public async Task SavePlan_WithEmptyCoverageGoal()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.CoverageGoalPercent = null;

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid coverage goal value. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidCoverageGoalTooSmall()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.CoverageGoalPercent = -1;

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid coverage goal value. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidCoverageGoalTooBig()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.CoverageGoalPercent = 120;

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid coverage goal value. Try to save the plan as draft"));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public async Task SavePlan_WithEmptyBlackoutMarkets()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.BlackoutMarkets.Clear();
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
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

                var modifedPlanId = await _PlanService.SavePlanAsync(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithEmptyAvailableMarkets()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets.Clear();

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid total market coverage. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidAvailableMarketShareOfVoiceTooSmall()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets[0].ShareOfVoicePercent = -1;

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid share of voice for market. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidAvailableMarketShareOfVoiceTooBig()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets[0].ShareOfVoicePercent = 120;

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid share of voice for market. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public async Task SavePlan_WithInvalidEndTime_TooMuch()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartTypeId = DaypartTypeEnum.News, DaypartCodeId = 1, StartTimeSeconds = 8900, EndTimeSeconds = 999999999, WeightingGoalPercent = 111.0 });

                Assert.That(async () => await _PlanService.SavePlanAsync(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid daypart times. Try to save the plan as draft"));
            }
        }

        [Test]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_InvalidRequest()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(null, false), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid request"));
            }
        }

        [Test]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_InvalidFlight()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = default,
                    FlightStartDate = default,
                }, false), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid flight start/end date."));
            }
        }

        [Test]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_InvalidFlightStartDate()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = default,
                    FlightStartDate = new DateTime(2019, 01, 01),
                }, false), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Invalid flight start/end date."));
            }
        }

        [Test]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_InvalidImpressionsPerUnit()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = new DateTime(2019, 05, 31),
                    FlightStartDate = new DateTime(2019, 01, 01),
                    TotalImpressions = 1000,
                    ImpressionsPerUnit = 1001,
                    FlightDays = new List<int> { 1, 2, 3, 4, 5 },
                    CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1 } },
                    Dayparts = new List<PlanDaypartDto>()
                    {
                        new PlanDaypartDto { DaypartCodeId = 0}
                    },
                    IsAduOnly = false
                }, false), Throws.TypeOf<PlanValidationException>().With.Message.EqualTo("Impressions per Unit must be less or equal to delivery impressions."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_CustomDelivery_InitialRequest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    Dayparts = new List<PlanDaypartDto>()
                    {
                        new PlanDaypartDto { DaypartCodeId = 0}
                    },

                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength {SpotLengthId = 1 }
                    },
                    FlightEndDate = new DateTime(2019, 03, 05),
                    FlightStartDate = new DateTime(2019, 02, 01),
                    FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },                   
                    DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek,
                    FlightHiatusDays = new List<DateTime>(),
                    TotalImpressions = 10000,
                    TotalRatings = 0.000907291831869388,
                    WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                    ImpressionsPerUnit = 100
                }, false);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_CustomByWeekByDaypart_WithoutDayparts()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new WeeklyBreakdownRequest
                {
                    FlightEndDate = new DateTime(2019, 03, 05),
                    FlightStartDate = new DateTime(2019, 02, 01),
                    FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                    DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart,
                    FlightHiatusDays = new List<DateTime>(),
                    TotalImpressions = 10000,
                    TotalRatings = 0.000907291831869388,
                    WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                    ImpressionsPerUnit = 100
                };

                var exception = Assert.Throws<PlanValidationException>(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false));

                Assert.That(exception.Message, Is.EqualTo("For the chosen delivery type, dayparts are required"));
            }
        }

        [Test]
        [Category("short_running")]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomByWeekByDaypart_WithoutWeeks()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = new DateTime(2019, 03, 05),
                    FlightStartDate = new DateTime(2019, 02, 01),
                    FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                    DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart,
                    FlightHiatusDays = new List<DateTime>(),
                    TotalImpressions = 10000,
                    TotalRatings = 10,
                    WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                    ImpressionsPerUnit = 100,
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength {SpotLengthId = 1 }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto{ DaypartCodeId = 1, WeightingGoalPercent = 60 },
                        new PlanDaypartDto { DaypartCodeId = 2 }
                    }
                }, false);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("short_running")]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomByWeekByDaypart_WKD()
        {
            const int standardDaypartIdWKD = 23;

            using (new TransactionScopeWrapper())
            {
                var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = new DateTime(2019, 03, 05),
                    FlightStartDate = new DateTime(2019, 02, 01),
                    FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                    DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart,
                    FlightHiatusDays = new List<DateTime>(),
                    TotalImpressions = 10000,
                    TotalRatings = 10,
                    WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                    ImpressionsPerUnit = 100,
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength {SpotLengthId = 1 }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto{ DaypartCodeId = 1, WeightingGoalPercent = 60 },
                        new PlanDaypartDto { DaypartCodeId = standardDaypartIdWKD }
                    }
                }, false);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [Category("short_running")]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomByWeekByDaypart_WithWeeks()
        {
            using (new TransactionScopeWrapper())
            {
                var request = new WeeklyBreakdownRequest
                {
                    FlightEndDate = new DateTime(2019, 03, 05),
                    FlightStartDate = new DateTime(2019, 02, 01),
                    FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                    DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeekByDaypart,
                    FlightHiatusDays = new List<DateTime>(),
                    TotalImpressions = 10000,
                    TotalRatings = 10,
                    WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                    ImpressionsPerUnit = 100,
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength {SpotLengthId = 1 }
                    },
                    Dayparts = new List<PlanDaypartDto>
                    {
                        new PlanDaypartDto{ DaypartCodeId = 1, WeightingGoalPercent = 60 },
                        new PlanDaypartDto { DaypartCodeId = 2 }
                    },
                    Weeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek
                        {
                            ActiveDays = "F-Su",
                            DaypartCodeId = 1,
                            EndDate = new DateTime(2019, 2, 3),
                            MediaWeekId = 788,
                            NumberOfActiveDays = 3,
                            PercentageOfWeek = 0,
                            StartDate = new DateTime(2019, 1, 28),
                            WeeklyImpressions = 0,
                            WeeklyImpressionsPercentage = 0,
                            WeeklyUnits = 0,
                            WeekNumber = 1
                        },
                        new WeeklyBreakdownWeek
                        {
                            ActiveDays = "F-Su",
                            DaypartCodeId = 2,
                            EndDate = new DateTime(2019, 2, 3),
                            MediaWeekId = 788,
                            NumberOfActiveDays = 3,
                            PercentageOfWeek = 0,
                            StartDate = new DateTime(2019, 1, 28),
                            WeeklyImpressions = 0,
                            WeeklyImpressionsPercentage = 0,
                            WeeklyUnits = 0,
                            WeekNumber = 1
                        },
                        new WeeklyBreakdownWeek
                        {
                            ActiveDays = "M-Su",
                            DaypartCodeId = 1,
                            EndDate = new DateTime(2019, 2, 10),
                            MediaWeekId = 789,
                            NumberOfActiveDays = 7,
                            PercentageOfWeek = 60,
                            StartDate = new DateTime(2019, 2, 4),
                            WeeklyImpressions = 999.6,
                            WeeklyImpressionsPercentage = 60,
                            WeeklyRatings = 6,
                            WeeklyUnits = 9.996,
                            WeekNumber = 2
                        },
                        new WeeklyBreakdownWeek
                        {
                            ActiveDays = "M-Su",
                            DaypartCodeId = 2,
                            EndDate = new DateTime(2019, 2, 10),
                            MediaWeekId = 789,
                            NumberOfActiveDays = 7,
                            PercentageOfWeek = 40,
                            StartDate = new DateTime(2019, 2, 4),
                            WeeklyImpressions = 666.4,
                            WeeklyImpressionsPercentage = 40,
                            WeeklyUnits = 6.664,
                            WeekNumber = 2,
                            WeeklyRatings = 4
                        }
                    }
                };

                var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_OneHiatusDay()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                FlightStartDate = new DateTime(2019, 08, 03),
                FlightEndDate = new DateTime(2019, 08, 27),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 1000,
                TotalRatings = 0.000907291831869388,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                ImpressionsPerUnit = 100
            }, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_WithFlightDays()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                FlightStartDate = new DateTime(2019, 08, 03),
                FlightEndDate = new DateTime(2019, 08, 27),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                FlightDays = new List<int> { 1, 2, 3, 4, 7 },
                TotalImpressions = 1000,
                ImpressionsPerUnit = 250,
                TotalRatings = 0.000907291831869388,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions
            }, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_RoundShareOfVoiceTotal()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                FlightStartDate = new DateTime(2019, 08, 05),
                FlightEndDate = new DateTime(2019, 09, 19),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 1000,
                TotalRatings = 0.000907291831869388,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                ImpressionsPerUnit = 100
            }, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_PRI15094()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                FlightStartDate = new DateTime(2019, 09, 29),
                FlightEndDate = new DateTime(2019, 10, 13),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 10, 10), new DateTime(2019, 10, 12), new DateTime(2019, 10, 4), new DateTime(2019, 10, 2) },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 20095158400,
                TotalRatings = 896090.698153806, // not accurate to the total impressions, but that doesn't matter for the test.
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                ImpressionsPerUnit = 10000000
            }, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_OneWeekHiatus()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = Entities.Enums.PlanGoalBreakdownTypeEnum.EvenDelivery,
                FlightStartDate = new DateTime(2019, 08, 01),
                FlightEndDate = new DateTime(2019, 08, 31),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 5), new DateTime(2019, 8, 6), new DateTime(2019, 8, 7), new DateTime(2019, 8, 8),
                                                        new DateTime(2019, 8, 9), new DateTime(2019, 8, 10), new DateTime(2019, 8, 11)},
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 1000,
                TotalRatings = 0.000907291831869388,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                ImpressionsPerUnit = 100
            }, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_CustomDelivery()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = Entities.Enums.PlanGoalBreakdownTypeEnum.CustomByWeek,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypart { DaypartCodeId = 0}
                },
                FlightStartDate = new DateTime(2019, 08, 03),
                FlightEndDate = new DateTime(2019, 08, 20),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 1000,
                TotalRatings = 0.000907291831869388,
                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
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
                }},
                ImpressionsPerUnit = 100
            }, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_CustomDelivery_ChangeRatings()
        {
            var request = _GetBaseRequestForCustomWeeklyBreakdown();
            request.Weeks.First(w => w.WeekNumber == 2).WeeklyRatings = .03;
            request.Weeks.First(w => w.WeekNumber == 2).IsUpdated = true;
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Ratings;

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void WeeklyBreakdown_Custom_ChangeUnits()
        {
            var request = _GetBaseRequestForCustomWeeklyBreakdown();
            request.Weeks.First(w => w.WeekNumber == 2).WeeklyUnits = 4;
            request.Weeks.First(w => w.WeekNumber == 2).IsUpdated = true;
            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Units;

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_CustomDelivery_ChangeImpression()
        {
            var request = _GetBaseRequestForCustomWeeklyBreakdown();
            request.Weeks.First(w => w.WeekNumber == 2).WeeklyImpressions = 300;
            request.Weeks.First(w => w.WeekNumber == 2).IsUpdated = true;

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_CustomDelivery_ChangePercentage()
        {
            var request = _GetBaseRequestForCustomWeeklyBreakdown();
            request.Weeks.First(w => w.WeekNumber == 2).WeeklyImpressionsPercentage = 30;
            request.Weeks.First(w => w.WeekNumber == 2).IsUpdated = true;

            request.WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Percentage;

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_UpdateHiatusDaysInCustomDelivery()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek,
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
                      MediaWeekId= 823,
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
                      MediaWeekId= 824,
                      NumberOfActiveDays= 7,
                      WeeklyImpressionsPercentage = 50,
                      WeeklyRatings = 0.00045364591593469400,
                      StartDate= new DateTime(2019,10,7),
                      WeekNumber= 2
                    },
                },
                ImpressionsPerUnit = 100
            }, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_EvenDelivery()
        {
            var request = new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                    {
                        new PlanDaypartDto { DaypartCodeId = 0}
                    },
                CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength {SpotLengthId = 1 }
                    },
                DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                FlightStartDate = new DateTime(2019, 12, 01),
                FlightEndDate = new DateTime(2019, 12, 31),
                FlightHiatusDays = new List<DateTime>(),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 500,
                TotalRatings = 0.453645915934694,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                ImpressionsPerUnit = 100
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
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
        [Category("short_running")]
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
        [Category("short_running")]
        public void GetCurrentQuarters()
        {
            // this is a Thursday
            var currentDateTime = new DateTime(2019, 12, 12);

            var results = _PlanService.GetCurrentQuarters(currentDateTime);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(results));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_OneImpressionPerWeek()
        {
            var request = new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek,
                FlightStartDate = new DateTime(2020, 01, 13),
                FlightEndDate = new DateTime(2020, 02, 16),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 5,
                TotalRatings = 1,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                TotalBudget = 85000,
                FlightHiatusDays = new List<DateTime>(),
                ImpressionsPerUnit = 1
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_LessImpressionThenWeeks()
        {
            var request = new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek,
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
                },
                ImpressionsPerUnit = 1
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_CustomDelivery_WithADU()
        {
            var request = new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                    {
                        new PlanDaypartDto { DaypartCodeId = 0}
                    },
                CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength {SpotLengthId = 1 }
                    },
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek,
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
                        WeeklyAdu = 1,
                        WeeklyUnits = 1

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
                        WeeklyAdu = 2,
                        WeeklyUnits = 1
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
                        WeeklyAdu = 3,
                        WeeklyUnits = 1
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
                        WeeklyAdu = 4,
                        WeeklyUnits = 1
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
                        WeeklyAdu = 5,
                        WeeklyUnits = 1
                    }
                },
                ImpressionsPerUnit = 1
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_EvenDelivery_EmptyWeeks()
        {
            var request = new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                FlightStartDate = new DateTime(2019, 12, 30),
                FlightEndDate = new DateTime(2020, 02, 02),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 5,
                TotalRatings = 0.002721875495608163,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                TotalBudget = 75000,
                FlightHiatusDays = new List<DateTime>(),
                ImpressionsPerUnit = 1
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("short_running")]
        public void Plan_WeeklyBreakdown_CustomDelivery_EmptyWeeks()
        {
            var request = new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                FlightStartDate = new DateTime(2019, 12, 30),
                FlightEndDate = new DateTime(2020, 02, 02),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                TotalImpressions = 5,
                TotalRatings = 0.002721875495608163,
                WeeklyBreakdownCalculationFrom = WeeklyBreakdownCalculationFrom.Impressions,
                TotalBudget = 75000,
                FlightHiatusDays = new List<DateTime>(),
                ImpressionsPerUnit = 1
            };

            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(request, false);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [Ignore("PRI-23204 Pricing is not queued on save, anymore")]
        [UseReporter(typeof(DiffReporter))]
        public async Task Plan_SaveNewPlan_PricingIsQueued()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _GetNewPlan();

                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                var execution = _PlanPricingService.GetCurrentPricingExecution(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(execution, _GetJsonSettings()));
            }
        }

        [Test]
        public async Task Plan_SaveNewPlan_FullWeekHiatus()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _GetNewPlan();
                newPlan.FlightHiatusDays = new List<DateTime> { new DateTime(2019,1,7), new DateTime(2019, 1, 8) , new DateTime(2019, 1, 9),
                    new DateTime(2019,1,10), new DateTime(2019,1,11), new DateTime(2019,1,12), new DateTime(2019,1,13)};
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task SavePlan_VersionOutOfSync()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _GetNewPlan();
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                var editPlan = _PlanService.GetPlan(newPlanId);
                editPlan.IsOutOfSync = true;
                var editPlanId = await _PlanService.SavePlanAsync(editPlan, "integration_test", new System.DateTime(2019, 01, 01));

                var finalPlan = _PlanService.GetPlan(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public async Task SavePlan_NewPlanOutOfSync()
        {
            using (new TransactionScopeWrapper())
            {
                var newPlan = _GetNewPlan();
                newPlan.IsOutOfSync = true;
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                var finalPlan = _PlanService.GetPlan(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_OrderPlanData(finalPlan), _GetJsonSettings()));
            }
        }

        [Test]
        [Category("long_running")]
        public async Task ValidateFlightTime()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                var expectedStartTime = "12:00:00";
                var expectedEndTime = "11:59:59";
                DateTime nowDate = new DateTime(2019, 01, 01);
                string username = "integration_test";
                var newPlanId = await _PlanService.SavePlanAsync(newPlan, username, nowDate);
                var plan = _PlanService.GetPlan(newPlanId);
                var resultStartTime = Convert.ToDateTime(plan.FlightStartDate).ToString("hh:mm:ss");
                var resultEndTime = Convert.ToDateTime(plan.FlightEndDate).ToString("hh:mm:ss");
                Assert.IsTrue(newPlanId > 0);
                Assert.AreEqual(expectedStartTime, resultStartTime);
                Assert.AreEqual(expectedEndTime, resultEndTime);
            }
        }

        private PricingParametersWithoutPlanDto _GetPricingParametersWithoutPlanDto()
        {
            var plan = _GetNewPlan();

            var pricingDefaults = _PlanPricingService.GetPlanPricingDefaults();
            var pricingParameters = new PricingParametersWithoutPlanDto
            {
                AudienceId = plan.AudienceId,
                AvailableMarkets = plan.AvailableMarkets,
                Budget = Convert.ToDecimal(plan.Budget),
                CPM = Convert.ToDecimal(plan.TargetCPM),
                CPP = Convert.ToDecimal(plan.TargetCPP),
                Currency = plan.Currency,
                DeliveryImpressions = Convert.ToDouble(plan.TargetImpressions) / 1000,
                DeliveryRatingPoints = Convert.ToDouble(plan.TargetRatingPoints),
                UnitCaps = pricingDefaults.UnitCaps,
                UnitCapsType = pricingDefaults.UnitCapsType,
                Margin = pricingDefaults.Margin,
                MarketGroup = pricingDefaults.MarketGroup,
                CoverageGoalPercent = plan.CoverageGoalPercent,
                CreativeLengths = plan.CreativeLengths,
                Dayparts = plan.Dayparts,
                Equivalized = plan.Equivalized,
                FlightDays = plan.FlightDays,
                FlightEndDate = plan.FlightEndDate,
                FlightStartDate = plan.FlightStartDate,
                FlightHiatusDays = plan.FlightHiatusDays,
                GoalBreakdownType = plan.GoalBreakdownType,
                HUTBookId = plan.HUTBookId,
                ImpressionsPerUnit = plan.ImpressionsPerUnit.Value,
                MaxCpm = 20,
                MinCpm = 5,
                PostingType = plan.PostingType,
                ShareBookId = plan.ShareBookId,
                TargetRatingPoints = plan.TargetRatingPoints,
                WeeklyBreakdownWeeks = plan.WeeklyBreakdownWeeks
            };

            return pricingParameters;
        }

        private static PlanDto _GetNewPlan()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                ProductMasterId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907"),
                CreativeLengths = new List<CreativeLength> {
                    new CreativeLength { SpotLengthId = 1, Weight = 50},
                    new CreativeLength{ SpotLengthId = 2}
                },
                Status = PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 1, 31),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightNotes = "Sample notes",
                FlightNotesInternal = "Sample internal notes",
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
                TargetUniverse = 111222d,
                Currency = PlanCurrenciesEnum.Impressions,
                TargetRatingPoints = 100d,
                CoverageGoalPercent = 80.5,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 48, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn", IsUserShareOfVoicePercent = true},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York", IsUserShareOfVoicePercent = true}
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
                        WeekdaysWeighting = 60,
                        WeekendWeighting = 40,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.8,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        },
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
                        WeightingGoalPercent = 33,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.9,
                                VpvhType = VpvhTypeEnum.LastYear,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        },
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
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.7,
                                VpvhType = VpvhTypeEnum.PreviousQuarter,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        },
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
                        WeeklyAdu = 5,
                        SpotLengthId = 1,
                        DaypartCodeId = 1,
                        PercentageOfWeek = 50,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2, MediaWeekId = 785,
                        StartDate = new DateTime(2019,01,07), EndDate = new DateTime(2019,01,13),
                        NumberOfActiveDays = 7, ActiveDays = "M-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3, MediaWeekId = 786,
                        StartDate = new DateTime(2019,01,14), EndDate = new DateTime(2019,01,20),
                        NumberOfActiveDays = 6, ActiveDays = "M-Sa", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4, MediaWeekId = 787,
                        StartDate = new DateTime(2019,01,21), EndDate = new DateTime(2019,01,27),
                        NumberOfActiveDays = 6, ActiveDays = "M-W,F-Su", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyUnits = 4
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 5, MediaWeekId = 788,
                        StartDate = new DateTime(2019,01,28), EndDate = new DateTime(2019,02,03),
                        NumberOfActiveDays = 4, ActiveDays = "M-Th", WeeklyImpressions = 20, WeeklyImpressionsPercentage  = 20,
                        WeeklyRatings = .0123,
                        WeeklyBudget = 20m,
                        WeeklyAdu = 30,
                        WeeklyUnits = 4
                    }
                },
                ImpressionsPerUnit = 5
            };
        }

        private static SaveCampaignDto _GetNewCampaign()
        {
            return new SaveCampaignDto
            {
                Name = "Campaign1",
                AdvertiserMasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"),
                AgencyMasterId = new Guid("A8B10A69-FBD6-43FE-B143-156B7297B62D"),
                Notes = "Notes for CampaignOne."
            };
        }

        private WeeklyBreakdownRequest _GetBaseRequestForCustomWeeklyBreakdown()
        {
            return new WeeklyBreakdownRequest
            {
                Dayparts = new List<PlanDaypartDto>()
                {
                    new PlanDaypartDto { DaypartCodeId = 0}
                },

                CreativeLengths = new List<CreativeLength>
                {
                    new CreativeLength {SpotLengthId = 1 }
                },
                DeliveryType = PlanGoalBreakdownTypeEnum.CustomByWeek,
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
                },
                ImpressionsPerUnit = 100
            };
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(PlanDto), "Id");
            jsonResolver.Ignore(typeof(PlanDto), "VersionId");
            jsonResolver.Ignore(typeof(PlanDto), "JobId");
            jsonResolver.Ignore(typeof(PlanDaypartDto), "Id");
            jsonResolver.Ignore(typeof(PlanMarketDto), "Id");
            jsonResolver.Ignore(typeof(PlanSummaryDto), "PlanId");
            jsonResolver.Ignore(typeof(PlanSummaryDto), "VersionId");
            jsonResolver.Ignore(typeof(PlanVersionDto), "VersionId");
            jsonResolver.Ignore(typeof(PlanPricingJob), "Id");
            jsonResolver.Ignore(typeof(PlanPricingJob), "PlanVersionId");
            jsonResolver.Ignore(typeof(PlanPricingParametersDto), "JobId");
            jsonResolver.Ignore(typeof(PlanPricingParametersDto), "PlanId");
            jsonResolver.Ignore(typeof(PlanPricingParametersDto), "PlanVersionId");
            jsonResolver.Ignore(typeof(PlanBuyingParametersDto), "JobId");
            jsonResolver.Ignore(typeof(PlanBuyingParametersDto), "PlanId");
            jsonResolver.Ignore(typeof(PlanBuyingParametersDto), "PlanVersionId");
            jsonResolver.Ignore(typeof(PlanDaypartDto), "PlanDaypartId");
            jsonResolver.Ignore(typeof(WeeklyBreakdownWeek), "PlanDaypartId");
            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }

        private void _ForceCompletePlanPricingJob(int planId)
        {
            var job = _PlanRepository.GetPricingJobForLatestPlanVersion(planId);
            _ForceCompletePlanPricingJob(job);
        }

        private void _ForceCompletePlanPricingJob(PlanPricingJob job)
        {
            if (job == null)
                return;

            job.Status = BackgroundJobProcessingStatus.Succeeded;
            job.Completed = DateTime.Now;
            _PlanRepository.UpdatePlanPricingJob(job);
        }

        private void _ForceCompletePlanPricingJobByJobId(int jobId)
        {
            var job = _PlanRepository.GetPlanPricingJob(jobId);
            _ForceCompletePlanPricingJob(job);
        }
    }
}
