using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.IntegrationTests.TestData;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.EntitiesUnitTests
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class CampaignReportDataUnitTest
    {
        [Test]
        public void ProjectGuaranteedAudienceDataByWeek_VPVH_WithoutRunPricing()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            WeeklyBreakdownWeek planWeek = _GetWeeklyBreakdownWeek();
            Dictionary<int, List<PlanPricingResultsDaypartDto>> planPricingResultsDayparts = null;
            var testClass = new CampaignReportData
            {                
            };
            var projection = _GetPlanProjectionForCampaignExport();

            // Action
            testClass._ProjectGuaranteedAudienceDataByWeek(plan, planWeek, projection, planPricingResultsDayparts);

            // Assert
            Assert.AreEqual(0, projection.GuaranteedAudience.VPVH);
        }

        [Test]
        public void ProjectGuaranteedAudienceDataByWeek_VPVH_WithRunPricing()
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.Id = 1;
            WeeklyBreakdownWeek planWeek = _GetWeeklyBreakdownWeek();
            Dictionary<int, List<PlanPricingResultsDaypartDto>> planPricingResultsDayparts = _GetPlanPricingResultsDayparts();
            var testClass = new CampaignReportData
            {                
            };
            var projection = _GetPlanProjectionForCampaignExport();

            // Action
            testClass._ProjectGuaranteedAudienceDataByWeek(plan, planWeek, projection, planPricingResultsDayparts);

            // Assert
            Assert.AreEqual(0.546, projection.GuaranteedAudience.VPVH);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ProjectHHAudienceData_WithoutRunPricing(bool vpvhFlagEnabled)
        {
            // Arrange
            const double expectedHhImpressions = 200.0;
            const double expectedHhRatings = 12.2;
            PlanDto plan = _GetNewPlan();
            plan.Id = 1;
            plan.HHImpressions = 200;
            plan.HHRatingPoints = 12.2;
            WeeklyBreakdownWeek planWeek = _GetWeeklyBreakdownWeek();
            Dictionary<int, List<PlanPricingResultsDaypartDto>> planPricingResultsDayparts = null;
            var testClass = new CampaignReportData
            {                
            };
            var projection = _GetPlanProjectionForCampaignExport();

            // Action
            testClass._ProjectHHAudienceData(plan, planWeek, projection, planPricingResultsDayparts);

            // Assert
            Assert.AreEqual(expectedHhImpressions, projection.TotalHHImpressions);
            Assert.AreEqual(expectedHhRatings, projection.TotalHHRatingPoints);
        }

        [Test]       
        [TestCase(200, 12.2, 183.15018315018312, 11.17216117216117)]
        public void ProjectHHAudienceData_VPVH_WithRunPricing(
            double planHhImpressions, double planHhRatingPoints,
            double expectedHhImpressions, double expectedHhRatings)
        {
            // Arrange
            PlanDto plan = _GetNewPlan();
            plan.Id = 1;
            plan.HHImpressions = planHhImpressions;
            plan.HHRatingPoints = planHhRatingPoints;
            WeeklyBreakdownWeek planWeek = _GetWeeklyBreakdownWeek();
            Dictionary<int, List<PlanPricingResultsDaypartDto>> planPricingResultsDayparts = _GetPlanPricingResultsDayparts(2);
            var testClass = new CampaignReportData();
            var projection = _GetPlanProjectionForCampaignExport();

            // Action
            testClass._ProjectHHAudienceData(plan, planWeek, projection, planPricingResultsDayparts);

            // Assert
            Assert.AreEqual(expectedHhImpressions, projection.TotalHHImpressions);
            Assert.AreEqual(expectedHhRatings, projection.TotalHHRatingPoints);
        }

        [Test]
        public void ProjectHHAudienceData_AduOnlyPLan()
        {
            // Arrange
            PlanDto plan = _GetNewAduOnlyPlan();
            WeeklyBreakdownWeek planWeek = new WeeklyBreakdownWeek()
            {
                WeeklyImpressions = 0,
                WeeklyRatings = 0,
                DaypartCodeId = 2
            };
            Dictionary<int, List<PlanPricingResultsDaypartDto>> planPricingResultsDayparts = _GetPlanPricingResultsDayparts(2);
            var testClass = new CampaignReportData
            {
            };
            var projection = _GetPlanProjectionForCampaignExport();

            // Action
            testClass._ProjectHHAudienceData(plan, planWeek, projection, planPricingResultsDayparts);

            // Assert
            Assert.AreEqual(0, projection.TotalHHImpressions);
            Assert.AreEqual(0, projection.TotalHHRatingPoints);
        }

        [Test]
        public void PopulateTotalAdusForFlowchartAduTable_AduFlagEnabled()
        {
            // Arrange
            var tableData = new FlowChartQuarterTableData
            {
                UnitsValues = new List<object> {"ADU"}
            };

            var testClass = new CampaignReportData();

            // Action
            testClass._PopulateTotalAdusForFlowchartAduTable(tableData);

            // Assert
            var toVerify = new
            {
                tableData.DistributionPercentages,
                tableData.UnitsValues,
                tableData.ImpressionsValues,
                tableData.CostValues,
                tableData.CPMValues
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }

        [Test]
        public void PopulateTotalAdusForFlowchartAduTable_WithoutAdu()
        {
            // Arrange
            var tableData = new FlowChartQuarterTableData();
            var testClass = new CampaignReportData();

            // Action
            testClass._PopulateTotalAdusForFlowchartAduTable(tableData);

            // Assert
            Assert.AreEqual(0, tableData.DistributionPercentages.Count);
            Assert.AreEqual(0, tableData.UnitsValues.Count);
            Assert.AreEqual(0, tableData.ImpressionsValues.Count);
            Assert.AreEqual(0, tableData.CostValues.Count);
            Assert.AreEqual(0, tableData.CPMValues.Count);
        }

        [Test]
        public void PopulateTotalAdusForFlowchartAduTable_AduFlagDisabled()
        {
            // Arrange
            var tableData = new FlowChartQuarterTableData
            {
                UnitsValues = new List<object> { 2.5 }
            };

            var testClass = new CampaignReportData();

            // Action
            testClass._PopulateTotalAdusForFlowchartAduTable(tableData);

            // Assert
            var toVerify = new
            {
                tableData.DistributionPercentages,
                tableData.UnitsValues,
                tableData.ImpressionsValues,
                tableData.CostValues,
                tableData.CPMValues
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }

        [Test]
        public void PopulateFlowchartAduTableData_AduFlagEnabled()
        {
            // Arrange
            var testClass = new CampaignReportData();

            var startDate = new DateTime();

            List<PlanDto> plans = new List<PlanDto>
            {
                new PlanDto
                {
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek{ },
                        new WeeklyBreakdownWeek{ },
                        new WeeklyBreakdownWeek{ StartDate = startDate, AduImpressions = 5 },
                        new WeeklyBreakdownWeek{ },
                    }
                }
            };
            FlowChartQuarterTableData tableData = new FlowChartQuarterTableData();
            FlowChartQuarterTableData firstTable = new FlowChartQuarterTableData
            {
                WeeksStartDate = new List<object>{ null, null, startDate, null }
            };

            int weekIndex = 2;

            // Act
            testClass._PopulateFlowchartAduTableData(plans, tableData, firstTable, weekIndex);

            // Assert
            var toVerify = new
            {
                tableData.DistributionPercentages,
                tableData.UnitsValues,
                tableData.ImpressionsValues,
                tableData.CostValues,
                tableData.CPMValues
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }

        [Test]
        public void PopulateFlowchartAduTableData_WithoutAdu_AduFlagEnabled()
        {
            // Arrange
            var testClass = new CampaignReportData();

            var startDate = new DateTime();

            List<PlanDto> plans = new List<PlanDto>
            {
                new PlanDto
                {
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek{ },
                        new WeeklyBreakdownWeek{ },
                        new WeeklyBreakdownWeek{ StartDate = startDate, AduImpressions = 0 },
                        new WeeklyBreakdownWeek{ },
                    }
                }
            };
            FlowChartQuarterTableData tableData = new FlowChartQuarterTableData();
            FlowChartQuarterTableData firstTable = new FlowChartQuarterTableData
            {
                WeeksStartDate = new List<object> { null, null, startDate, null }
            };

            int weekIndex = 2;

            // Act
            testClass._PopulateFlowchartAduTableData(plans, tableData, firstTable, weekIndex);

            // Assert
            var toVerify = new
            {
                tableData.DistributionPercentages,
                tableData.UnitsValues,
                tableData.ImpressionsValues,
                tableData.CostValues,
                tableData.CPMValues
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }

        [Test]
        public void PopulateFlowchartAduTableData_AduFlagDisabled()
        {
            // Arrange
            const double calculatedAdu = 1.21;
            var weeklyBreakdownEngine = new Mock<IWeeklyBreakdownEngine>();
            weeklyBreakdownEngine.Setup(s => s.CalculateADUWithDecimals(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns(calculatedAdu);

            var testClass = new CampaignReportData
            {
                _WeeklyBreakdownEngine = weeklyBreakdownEngine.Object
            };

            var startDate = new DateTime(2021,7,12);
            var spotLengthId = 2;

            List<PlanDto> plans = new List<PlanDto>
            {
                new PlanDto
                {
                    CreativeLengths = new List<CreativeLength>
                    {
                        new CreativeLength
                        {
                            SpotLengthId = spotLengthId
                        }
                    },
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek{ StartDate = startDate.AddDays(-14), AduImpressions = 0, SpotLengthId = spotLengthId },
                        new WeeklyBreakdownWeek{ StartDate = startDate.AddDays(-7), AduImpressions = 0, SpotLengthId = spotLengthId },
                        new WeeklyBreakdownWeek{ StartDate = startDate, AduImpressions = 5, SpotLengthId = spotLengthId },
                        new WeeklyBreakdownWeek{ StartDate = startDate.AddDays(7), AduImpressions = 0, SpotLengthId = spotLengthId },
                    },
                    ImpressionsPerUnit = 1
                }
            };
            FlowChartQuarterTableData tableData = new FlowChartQuarterTableData();
            FlowChartQuarterTableData firstTable = new FlowChartQuarterTableData
            {
                WeeksStartDate = new List<object> { null, null, startDate, null }
            };

            int weekIndex = 2;

            // Act
            testClass._PopulateFlowchartAduTableData(plans, tableData, firstTable, weekIndex);

            // Assert
            var toVerify = new
            {
                tableData.DistributionPercentages,
                tableData.UnitsValues,
                tableData.ImpressionsValues,
                tableData.CostValues,
                tableData.CPMValues
            };
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(toVerify));
        }

        [Test]
        public void PopulateContractAduTable_AduFlagEnabled()
        {
            // Arrange
            var testClass = new CampaignReportData
            {
                _AllSpotLengths = SpotLengthTestData.GetAllSpotLengths(),
            };

            var startDate = new DateTime(2021, 7, 12);
            var spotLengthId = 2;

            List<PlanDto> plans = new List<PlanDto>
            {
                new PlanDto
                {
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek{ StartDate = startDate.AddDays(-14), AduImpressions = 0, SpotLengthId = spotLengthId },
                        new WeeklyBreakdownWeek{ StartDate = startDate.AddDays(-7), AduImpressions = 0, SpotLengthId = spotLengthId },
                        new WeeklyBreakdownWeek{ StartDate = startDate, AduImpressions = 2.5, SpotLengthId = spotLengthId },
                        new WeeklyBreakdownWeek{ StartDate = startDate.AddDays(7), AduImpressions = 0, SpotLengthId = spotLengthId },
                    }
                }
            };
            var table = new ContractQuarterTableData();

            // Act
            testClass._PopulateContractAduTable(table, plans, spotLengthId, startDate);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(table.Rows));
        }

        [Test]
        public void PopulateContractAduTable_WithoutAdu_AduFlagEnabled()
        {
            // Arrange
            var testClass = new CampaignReportData
            {
                _AllSpotLengths = SpotLengthTestData.GetAllSpotLengths(),
            };

            var startDate = new DateTime(2021, 7, 12);
            var spotLengthId = 2;

            List<PlanDto> plans = new List<PlanDto>
            {
                new PlanDto
                {
                    WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                    {
                        new WeeklyBreakdownWeek{ StartDate = startDate.AddDays(-14), AduImpressions = 0, SpotLengthId = spotLengthId },
                        new WeeklyBreakdownWeek{ StartDate = startDate.AddDays(-7), AduImpressions = 0, SpotLengthId = spotLengthId },
                        new WeeklyBreakdownWeek{ StartDate = startDate, AduImpressions = 0, SpotLengthId = spotLengthId },
                        new WeeklyBreakdownWeek{ StartDate = startDate.AddDays(7), AduImpressions = 0, SpotLengthId = spotLengthId },
                    }
                }
            };
            var table = new ContractQuarterTableData();

            // Act
            testClass._PopulateContractAduTable(table, plans, spotLengthId, startDate);

            // Assert
            Assert.AreEqual(0, table.Rows.Count);
        }

        [Test]
        public void CalculateTotalsForContractADUTable_AduFlagEnabled()
        {
            // Arrange
            const string quarterLabel = "ThisQuarterIsTHEQuarter";

            var testClass = new CampaignReportData();

            //units are the third value in the row
            var rows = new List<List<object>>
            {
                new List<object> { null, null, "ADU", null }
            };

            var quarterTable = new ContractQuarterTableData { Rows = rows };

            // Act
            testClass._CalculateTotalsForContractADUTable(quarterTable, quarterLabel);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarterTable.TotalRow));
        }

        [Test]
        public void CalculateTotalsForContractADUTable_WithoutAdu_AduFlagEnabled()
        {
            // Arrange
            const string quarterLabel = "ThisQuarterIsTHEQuarter";

            var testClass = new CampaignReportData();

            //units are the third value in the row
            var rows = new List<List<object>>
            {
                new List<object> { null, null, null, null }
            };

            var quarterTable = new ContractQuarterTableData{Rows = rows};

            // Act
            testClass._CalculateTotalsForContractADUTable(quarterTable, quarterLabel);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarterTable.TotalRow));
        }

        [Test]
        public void CalculateTotalsForContractADUTable_AduFlagDisabled()
        {
            // Arrange
            const string quarterLabel = "ThisQuarterIsTHEQuarter";

            var testClass = new CampaignReportData();

            //units are the third value in the row
            var rows = new List<List<object>>
            {
                new List<object> { null, null, 2.5, null }
            };

            var quarterTable = new ContractQuarterTableData { Rows = rows };

            // Act
            testClass._CalculateTotalsForContractADUTable(quarterTable, quarterLabel);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarterTable.TotalRow));
        }

        [Test]
        public void CalculateTotalsForContractADUTable_WithoutAdu_AduFlagDisabled()
        {
            // Arrange
            const string quarterLabel = "ThisQuarterIsTHEQuarter";

            var testClass = new CampaignReportData();

            //units are the third value in the row
            var rows = new List<List<object>>
            {
                new List<object> { null, null, null, null }
            };

            var quarterTable = new ContractQuarterTableData { Rows = rows };

            // Act
            testClass._CalculateTotalsForContractADUTable(quarterTable, quarterLabel);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(quarterTable.TotalRow));
        }

        [Test]
        public void ExternalExportNotes_IsExternalNoteExportEnabled()
        {
            // Arrange
            var testClass = new CampaignReportData();
            List<PlanDto> plans = new List<PlanDto>
            {
                new PlanDto
                {
                    Id = 1,
                    FlightNotes = "No Family Guy"
                },
                new PlanDto
                {
                    Id = 2,
                    FlightNotes = null
                },
                new PlanDto
                {
                    Id = 3,
                    FlightNotes =  "News Only"
                },
                 new PlanDto
                {
                    Id = 3,
                    FlightNotes = null
                },
            };                   
            // Act
            testClass._PopulateNotes(plans);

            // Assert
             Assert.AreEqual("All CPMs are derived from 100% broadcast deliveries, no cable unless otherwise noted.\r\nNo Family Guy (Plan ID 1); News Only (Plan ID 3); ", testClass.Notes);
        }

        [Test]
        [TestCase(0, 0, false)]
        [TestCase(2400, 1.2, false)]
        [TestCase(0, 0, true)]
        [TestCase(2400, .6, true)]
        public void CalculateUnitsForWeekComponent(double weeklyImpressions, double expectedResult, bool equivalized)
        {
            // Arrange
            var planWeek = new WeeklyBreakdownWeek
            {
                WeeklyImpressions = weeklyImpressions,
                SpotLengthId = 2 // :60s
            };
            var planImpressionsPerUnit = 2;
            var spotLengthDeliveryMultipliers = SpotLengthTestData.GetDeliveryMultipliersBySpotLengthId();

            // Act
            var result = CampaignReportData._CalculateUnitsForWeekComponent(planWeek, planImpressionsPerUnit, equivalized, spotLengthDeliveryMultipliers);

            Assert.AreEqual(expectedResult, result);
        }
        
        private static PlanDto _GetNewAduOnlyPlan()
        {
            var plan = _GetNewPlan();
            plan.IsAduEnabled = true;
            plan.IsAduPlan = true;
            plan.Budget = null;
            plan.TargetCPM = null;
            plan.TargetImpressions = null;
            plan.ImpressionsPerUnit = null;
            plan.Vpvh = null;
            plan.TargetRatingPoints = null;
            plan.TargetCPP = null;
            return plan;
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
                        DaypartTypeId=DaypartTypeEnum.EntertainmentNonNews,
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
                        DaypartTypeId=DaypartTypeEnum.EntertainmentNonNews,
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
                WeeklyRatings = 50,
                DaypartCodeId = 2
            };
        }
        private static CampaignReportData.PlanProjectionForCampaignExport _GetPlanProjectionForCampaignExport()
        {
            return new CampaignReportData.PlanProjectionForCampaignExport()
            {
                DaypartCodeId = 15
            };
        }
        private static Dictionary<int, List<PlanPricingResultsDaypartDto>> _GetPlanPricingResultsDayparts(int standardDaypartId = 15)
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
                            StandardDaypartId = standardDaypartId,
                            CalculatedVpvh = 0.546
                        }
                    }
                }
            };
        }
    }
}
