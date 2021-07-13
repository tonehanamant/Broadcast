using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.Helpers
{
    [TestFixture]
    public class PlanComparisonHelperUnitTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void MinorPropertyListCheck()
        {
            var minorProperties = PlanComparisonHelper.MinorProperties;

            var toVerify = minorProperties.Select(p => new
            {
                ClassTypeName = p.ClassType.Name,
                p.PropertyName
            }).ToList();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }

        [Test]
        public void DidPlanPricingInputsChangeNoChange()
        {
            var plan = _GetNewPlan();

            var result = PlanComparisonHelper.DidPlanPricingInputsChange(plan, plan);

            Assert.IsFalse(result);
        }

        [Test]
        public void DidPlanPricingInputsChangeMinorChange()
        {
            var beforePlan = _GetNewPlan();
            var afterPlan = _GetNewPlan();
            // These properties some minor properties
            afterPlan.VersionId++;
            afterPlan.ProductId++;
            afterPlan.Name = beforePlan.Name + " Edited";
            afterPlan.IsDraft = !afterPlan.IsDraft;

            var result = PlanComparisonHelper.DidPlanPricingInputsChange(beforePlan, afterPlan);

            Assert.IsFalse(result);
        }

        [Test]
        public void DidPlanPricingInputsChangeMajorChange()
        {
            var beforePlan = _GetNewPlan();
            var afterPlan = _GetNewPlan();
            // These properties some major properties
            afterPlan.Budget += 10m;
            afterPlan.CoverageGoalPercent += 10;

            var result = PlanComparisonHelper.DidPlanPricingInputsChange(beforePlan, afterPlan);

            Assert.IsTrue(result);
        }

        [Test]
        public void DidPlanPricingInputsChange_ChangeStatusAndGoalBreakdown()
        {
            var beforePlan = _GetNewPlan();
            var afterPlan = _GetNewPlan();

            beforePlan.Status = PlanStatusEnum.Working;
            beforePlan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.EvenDelivery;

            afterPlan.Status = PlanStatusEnum.Contracted;
            afterPlan.GoalBreakdownType = PlanGoalBreakdownTypeEnum.CustomByWeek;

            var result = PlanComparisonHelper.DidPlanPricingInputsChange(beforePlan, afterPlan);

            Assert.IsFalse(result);
        }

        private static PlanDto _GetNewPlan()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
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
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 20, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn"},
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
    }
}
