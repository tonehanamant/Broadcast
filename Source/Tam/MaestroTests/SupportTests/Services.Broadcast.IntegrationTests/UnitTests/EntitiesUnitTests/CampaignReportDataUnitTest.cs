using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Services.Broadcast.Entities.Campaign.CampaignReportData;

namespace Services.Broadcast.IntegrationTests.UnitTests.EntitiesUnitTests
{
    [TestFixture]
    public class CampaignReportDataUnitTest
    {
        [Test]       
        public void ProjectGuaranteedAudienceDataByWeek_VPVH_WithoutRunPricing()
        {
            PlanDto plan = _GetNewPlan();
            WeeklyBreakdownWeek planWeek = _GetWeeklyBreakdownWeek();
            PlanProjectionForCampaignExport projection = _GetPlanProjectionForCampaignExport();
            Dictionary<int, List<PlanPricingResultsDaypartDto>> planPricingResultsDayparts = null;
            CampaignReportData._IsVPVHDemoEnabled= new Lazy<bool>(() => true);
            _ProjectGuaranteedAudienceDataByWeek(plan, planWeek, projection, planPricingResultsDayparts);
            Assert.AreEqual(0, projection.GuaranteedAudience.VPVH);
        }

        [Test]
        public void ProjectGuaranteedAudienceDataByWeek_VPVH_WithRunPricing()
        {
            PlanDto plan = _GetNewPlan();
            plan.Id = 1;
            WeeklyBreakdownWeek planWeek = _GetWeeklyBreakdownWeek();
            PlanProjectionForCampaignExport projection = _GetPlanProjectionForCampaignExport();
            Dictionary<int, List<PlanPricingResultsDaypartDto>> planPricingResultsDayparts = _GetPlanPricingResultsDayparts();
            CampaignReportData._IsVPVHDemoEnabled = new Lazy<bool>(() => true);
            _ProjectGuaranteedAudienceDataByWeek(plan, planWeek, projection, planPricingResultsDayparts);
            Assert.AreEqual(0.546, projection.GuaranteedAudience.VPVH);
        }

        private static PlanDto _GetNewPlan()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                ProductMasterId = new Guid("C8C76C3B-8C39-42CF-9657-B7AD2B8BA320"),
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 50 } },
                Status = Entities.Enums.PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 7, 31),
                FlightNotes = "Sample notes",
                FlightNotesInternal = "Internal sample notes",
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019,1,20),
                    new DateTime(2019,4,15)
                },
                AudienceId = 31,        //HH
                AudienceType = AudienceTypeEnum.Nielsen,
                HUTBookId = 436,
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,
                Budget = 100m,
                TargetCPM = 12m,
                TargetImpressions = 100d,
                CoverageGoalPercent = 80.5,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 20, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn", IsUserShareOfVoicePercent = true},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York", IsUserShareOfVoicePercent = true}
                },
                AvailableMarketsSovTotal = 56.7,
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
                        StartTimeSeconds = 0,
                        EndTimeSeconds = 2000,
                        WeightingGoalPercent = 28.0,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.5,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        }
                    },
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 11,
                        StartTimeSeconds = 1500,
                        EndTimeSeconds = 2788,
                        WeightingGoalPercent = 33.2,
                        VpvhForAudiences = new List<PlanDaypartVpvhForAudienceDto>
                        {
                            new PlanDaypartVpvhForAudienceDto
                            {
                                AudienceId = 31,
                                Vpvh = 0.5,
                                VpvhType = VpvhTypeEnum.FourBookAverage,
                                StartingPoint = new DateTime(2019, 01, 12, 12, 30, 29)
                            }
                        }
                    }
                },
                Vpvh = 0.234543,
                TargetRatingPoints = 50,
                TargetCPP = 50,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery,
                ImpressionsPerUnit = 20,
                PricingParameters = new PlanPricingParametersDto
                {
                    AdjustedBudget = 80m,
                    AdjustedCPM = 10m,
                    CPM = 12m,
                    Budget = 100m,
                    Currency = PlanCurrenciesEnum.Impressions,
                    DeliveryImpressions = 100d,
                    InflationFactor = 10,
                    JobId = 1,
                    PlanId = 1,
                    PlanVersionId = 1,
                    ProprietaryInventory = new List<InventoryProprietarySummary>
                    {
                        new InventoryProprietarySummary
                        {
                            Id = 1
                        }
                    }
                }
            };
        }
        private static WeeklyBreakdownWeek _GetWeeklyBreakdownWeek()
        {
            return new WeeklyBreakdownWeek()
            {
                WeeklyImpressions = 100,
                WeeklyRatings = 50
            };
        }
        private static PlanProjectionForCampaignExport _GetPlanProjectionForCampaignExport()
        {
            return new PlanProjectionForCampaignExport()
            {
                DaypartCodeId = 15
            };
        }
        private static Dictionary<int, List<PlanPricingResultsDaypartDto>> _GetPlanPricingResultsDayparts()
        {
            return new Dictionary<int, List<PlanPricingResultsDaypartDto>>()
            {
                {
                    1,new List<PlanPricingResultsDaypartDto>()
                    {
                        new PlanPricingResultsDaypartDto()
                        {
                            Id = 101,
                            PlanVersionPricingResultId = 10,
                            StandardDaypartId = 15,
                            CalculatedVpvh = 0.546
                        }
                    }
                }
            };
        }
    }
}
