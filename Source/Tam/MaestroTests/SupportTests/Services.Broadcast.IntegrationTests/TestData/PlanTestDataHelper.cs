using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class PlanTestDataHelper
    {
        public static PlanDto GetPlanForAllocationModelRun()
        {
            var planDto = new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                CreativeLengths = new List<CreativeLength> {
                    new CreativeLength { SpotLengthId = 2 }
                },
                Status = PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2018, 10, 1),
                FlightEndDate = new DateTime(2018, 10, 31),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightNotes = "Sample notes",
                FlightHiatusDays = new List<DateTime>(),
                AudienceId = BroadcastConstants.HouseholdAudienceId,       //HH
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
                                ShowTypes = new List<LookupDto>()
                            },
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Genres = new List<LookupDto>()
                            },
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Programs = new List<ProgramDto>()
                            },
                            AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Affiliates = new List<LookupDto>()
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
                        StartDate = new DateTime(2018,12,31), EndDate = new DateTime(2019,1,6),
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

            return planDto;
        }

        public static PlanDto GetPlanForAllocationModelRunMultiSpot()
        {
            var planDto = new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                CreativeLengths = new List<CreativeLength> {
                    new CreativeLength { SpotLengthId = 1, Weight = 50 },
                    new CreativeLength { SpotLengthId = 2 }
                },
                Status = PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2018, 10, 1),
                FlightEndDate = new DateTime(2018, 10, 31),
                FlightDays = new List<int> { 1, 2, 3, 4, 5, 6, 7 },
                FlightNotes = "Sample notes",
                FlightHiatusDays = new List<DateTime>(),
                AudienceId = BroadcastConstants.HouseholdAudienceId,       //HH
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
                                ShowTypes = new List<LookupDto>()
                            },
                            GenreRestrictions = new PlanDaypartDto.RestrictionsDto.GenreRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Genres = new List<LookupDto>()
                            },
                            ProgramRestrictions = new PlanDaypartDto.RestrictionsDto.ProgramRestrictionDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Programs = new List<ProgramDto>()
                            },
                            AffiliateRestrictions = new PlanDaypartDto.RestrictionsDto.AffiliateRestrictionsDto
                            {
                                ContainType = ContainTypeEnum.Exclude,
                                Affiliates = new List<LookupDto>()
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
                        StartDate = new DateTime(2018,12,31), EndDate = new DateTime(2019,1,6),
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

            return planDto;
        }
    }
}
