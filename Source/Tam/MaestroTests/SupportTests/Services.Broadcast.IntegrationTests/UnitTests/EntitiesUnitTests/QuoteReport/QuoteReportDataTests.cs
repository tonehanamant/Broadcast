using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.IntegrationTests.TestData;
using Services.Broadcast.ReportGenerators.Quote;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.UnitTests.EntitiesUnitTests.QuoteReport
{
    [TestFixture]
    public class QuoteReportDataTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ConstructorTest()
        {
            // Arrange
            var request = _GetQuoteRequest();
            var generatedDateTime = new DateTime(2020, 8, 11, 15, 23, 45);
            var allMarkets = MarketsTestData.GetMarketsWithLatestCoverage();
            var allAudiences = AudienceTestData.GetAudiences();
            var inventory = _GetTestInventory();

            // Act
            var result = new QuoteReportData(request, generatedDateTime, allAudiences, allMarkets, inventory);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void ConstructorTestEmpty()
        {
            // Arrange
            var request = _GetQuoteRequest();
            var generatedDateTime = new DateTime(2020, 8, 11, 15, 23, 45);
            var allMarkets = MarketsTestData.GetMarketsWithLatestCoverage();
            var allAudiences = AudienceTestData.GetAudiences();
            var inventory = new List<QuoteProgram>();

            // Act
            var result = new QuoteReportData(request, generatedDateTime, allAudiences, allMarkets, inventory);

            // Assert
            Assert.IsEmpty(result.RateDetailsTableData);
        }


        [Test]
        public void GetExportedFileName()
        {
            // Arrange
            const string expectedResult = "PlanQuote_08112020_152345.xlsx";
            var generatedDateTime = new DateTime(2020, 8, 11, 15, 23, 45);

            // Act
            var result = QuoteReportData._GetExportedFileName(generatedDateTime);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void TransformProgramToRateDetailLines()
        {
            // Arrange
            var request = _GetQuoteRequest();
            var allMarkets = MarketsTestData.GetMarketsWithLatestCoverage();
            var inventory = _GetTestInventory();

            // Act
            var result = QuoteReportData._TransformProgramToRateDetailLines(request, allMarkets, inventory);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void TransformProgramToRateDetailLinesEmpty()
        {
            // Arrange
            var request = _GetQuoteRequest();
            var allMarkets = MarketsTestData.GetMarketsWithLatestCoverage();
            var inventory = new List<QuoteProgram>();

            // Act
            var result = QuoteReportData._TransformProgramToRateDetailLines(request, allMarkets, inventory);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetOrderedAudiences()
        {
            // Arrange
            var request = _GetQuoteRequest();
            var allAudiences = AudienceTestData.GetAudiences();

            // Act
            var result = QuoteReportData._GetOrderedAudiences(request, allAudiences);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetRateDetailsTableAudienceHeaders()
        {
            // Arrange
            var request = _GetQuoteRequest();
            var allAudiences = AudienceTestData.GetAudiences();
            var orderedAudiences = QuoteReportData._GetOrderedAudiences(request, allAudiences);

            // Act
            var result = QuoteReportData._GetRateDetailsTableAudienceHeaders(orderedAudiences);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetRateDetailsTableData()
        {
            // Arrange
            var request = _GetQuoteRequest();
            var allMarkets = MarketsTestData.GetMarketsWithLatestCoverage();
            var inventory = _GetTestInventory();
            var transformed = QuoteReportData._TransformProgramToRateDetailLines(request, allMarkets, inventory);
            var allAudiences = AudienceTestData.GetAudiences();
            var orderedAudiences = QuoteReportData._GetOrderedAudiences(request, allAudiences);

            // Act
            var result = QuoteReportData._GetRateDetailsTableData(transformed, orderedAudiences);

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetRateDetailsTableDataEmpty()
        {
            // Arrange
            var request = _GetQuoteRequest();
            var transformed = new List<QuoteReportRateDetailLine>();
            var allAudiences = AudienceTestData.GetAudiences();
            var orderedAudiences = QuoteReportData._GetOrderedAudiences(request, allAudiences);

            // Act
            var result = QuoteReportData._GetRateDetailsTableData(transformed, orderedAudiences);

            // Assert
            Assert.IsEmpty(result);
        }

        private static QuoteRequestDto _GetQuoteRequest()
        {
            var request = new QuoteRequestDto
            {
                FlightStartDate = new DateTime(2018, 12, 17),
                FlightEndDate = new DateTime(2018, 12, 23),
                FlightHiatusDays = new List<DateTime>(),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                CreativeLengths = new List<CreativeLength> { new CreativeLength { SpotLengthId = 1, Weight = 100 } },
                Equivalized = true,
                AudienceId = 31,
                SecondaryAudiences = new List<PlanAudienceDto>
                {
                    new PlanAudienceDto { Type = AudienceTypeEnum.Nielsen, AudienceId = 287 },
                    new PlanAudienceDto { Type = AudienceTypeEnum.Nielsen, AudienceId = 420 }
                },
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,
                HUTBookId = 434,
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto
                    {
                        DaypartCodeId = 8,
                        StartTimeSeconds = 0,
                        EndTimeSeconds = 14400,
                        Restrictions = new PlanDaypartDto.RestrictionsDto
                        {
                            ShowTypeRestrictions = new PlanDaypartDto.RestrictionsDto.ShowTypeRestrictionsDto(),
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto(),
                            AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto(),
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Include,
                                Programs = new List<ProgramDto> {new ProgramDto {Name = "Early news"}}
                            }
                        }
                    }
                }
            };
            return request;
        }

        private List<QuoteProgram> _GetTestInventory()
        {
            var inventory = new List<QuoteProgram>
            {
                new QuoteProgram
                {
                    ManifestId = 411045,
                    Station = new DisplayBroadcastStation
                    {
                        LegacyCallLetters = "WHYY",
                        MarketCode = 101,
                        Affiliation = "PBS"
                    },
                    DeliveryPerAudience = new List<QuoteProgram.ImpressionsPerAudience>
                    {
                        new QuoteProgram.ImpressionsPerAudience
                        {
                            AudienceId = 31,
                            ProjectedImpressions = 1000,
                            ProvidedImpressions = 2000,
                            CPM = 10.5m
                        },
                        new QuoteProgram.ImpressionsPerAudience
                        {
                            AudienceId = 287,
                            ProjectedImpressions = 3000,
                            ProvidedImpressions = 4000,
                            CPM = 20.5m
                        },
                        new QuoteProgram.ImpressionsPerAudience
                        {
                            AudienceId = 420,
                            ProjectedImpressions = 5000,
                            ProvidedImpressions = 6000,
                            CPM = 30.5m
                        }
                    },
                    ManifestRates = new List<BasePlanInventoryProgram.ManifestRate>
                    {
                        new BasePlanInventoryProgram.ManifestRate
                        {
                            Cost = 37.5m,
                            SpotLengthId = 1
                        }
                    },
                    ManifestDayparts = new List<BasePlanInventoryProgram.ManifestDaypart>
                    {
                        // there will be only one
                        new BasePlanInventoryProgram.ManifestDaypart
                        {
                            Daypart = new DisplayDaypart(1, 0, 3600, true, true, true, true, true, true, true),
                            PrimaryProgram = new BasePlanInventoryProgram.ManifestDaypart.Program
                            {
                                Name = "Test Program"
                            }
                        }
                    }
                }
            };
            return inventory;
        }
    }
}