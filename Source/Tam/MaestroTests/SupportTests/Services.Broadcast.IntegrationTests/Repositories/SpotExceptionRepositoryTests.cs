using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
    [UseReporter(typeof(DiffReporter))] 
    public class SpotExceptionRepositoryTests
    {
        [Test]
        public void GetSpotExceptionsRecommendedPlans_RecommendedPlan_DoesNotExist()
        {
            // Arrange
            DateTime weekStartDate = new DateTime(2017, 01, 02);
            DateTime weekEndDate = new DateTime(2017, 01, 08);
            var spotExceptionRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();

            // Act
            var result = spotExceptionRepository.GetSpotExceptionsRecommendedPlans(weekStartDate, weekEndDate);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlans_RecommendedPlans_Exist()
        {
            // Arrange
            var ingestedDateTime = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            DateTime weekStartDate = new DateTime(2022, 04, 04);
            DateTime weekEndDate = new DateTime(2022, 04, 10);

            var spotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlansDto>
            {
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6840,
                    InventorySource = "Tegna",
                    HouseIsci = "840T42AY13H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 04, 10, 08, 28, 28),
                    StationLegacyCallLetters = "WBNS",
                    Affiliate = "CBS",
                    MarketCode = 135,
                    MarketRank = 33,
                    ProgramName = "CBS Mornings",
                    ProgramGenre = "INFORMATIONAL/NEWS",
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    CreatedBy = ingestedBy,
                    CreatedAt = ingestedDateTime,
                    ModifiedBy = ingestedBy,
                    ModifiedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 73,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "SYN",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
                            {
                                UserName = ingestedBy,
                                CreatedAt = ingestedDateTime,
                                AcceptedAsInSpec = false
                            }
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 75,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA5MTAwMQ=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6616,
                    InventorySource = "Ference POD",
                    HouseIsci = "616MAY2913H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 04, 10, 08, 41, 57),
                    StationLegacyCallLetters = "WDKA",
                    Affiliate = "IND",
                    MarketCode = 232,
                    MarketRank = 3873,
                    ProgramName = "Mike & Molly",
                    ProgramGenre = "COMEDY",
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    CreatedBy = ingestedBy,
                    CreatedAt = ingestedDateTime,
                    ModifiedBy = ingestedBy,
                    ModifiedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 623,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 624,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "SYN",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDI3MjQ2NQ=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6289,
                    InventorySource = "Sinclair Corp - Day Syn 10a-4p",
                    HouseIsci = "289IT2Y3P2H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 04, 10, 09, 58, 55),
                    StationLegacyCallLetters = "KOMO",
                    Affiliate = "ABC",
                    MarketCode = 419,
                    MarketRank = 11,
                    ProgramName = "LIVE with Kelly and Ryan",
                    ProgramGenre = "TALK",
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    CreatedBy = ingestedBy,
                    CreatedAt = ingestedDateTime,
                    ModifiedBy = ingestedBy,
                    ModifiedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 1824,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 1923,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "SYN",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDIwMjA4Nw=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 5711,
                    InventorySource = "TVB Syndication/ROS",
                    HouseIsci = "711N51AY18H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 04, 10, 08, 29, 23),
                    StationLegacyCallLetters = "WEVV",
                    Affiliate = "CBS",
                    MarketCode = 249,
                    MarketRank = 106,
                    ProgramName = "Funny You Should Ask",
                    ProgramGenre = "GAME SHOW",
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    CreatedBy = ingestedBy,
                    CreatedAt = ingestedDateTime,
                    ModifiedBy = ingestedBy,
                    ModifiedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 2222,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 2223,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "SYN",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                }
            };

            List<SpotExceptionsRecommendedPlansDto> result;

            var spotExceptionRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();

            // Act
            using (new TransactionScopeWrapper())
            {
                spotExceptionRepository.AddSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlans);
                result = spotExceptionRepository.GetSpotExceptionsRecommendedPlans(weekStartDate, weekEndDate);
            }

            // Assert
            var settings = IntegrationTestHelper._GetJsonSettings();
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(SpotExceptionsRecommendedPlansDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(SpotExceptionsRecommendedPlanDetailsDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(SpotExceptionsRecommendedPlanDetailsDto), "SpotExceptionsRecommendedPlanId");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(SpotExceptionsRecommendedPlanDecisionDto), "Id");
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(SpotExceptionsRecommendedPlanDecisionDto), "SpotExceptionsRecommendedPlanDetailId");

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, settings));
        }

        [Test]
        public void GetSpotExceptionsOutOfSpecsPosts_OutOfSpec_DoesNotExist()
        {
            // Arrange
            DateTime weekStartDate = new DateTime(2017, 01, 02);
            DateTime weekEndDate = new DateTime(2017, 01, 08);
            var spotExceptionRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();

            // Act
            var result = spotExceptionRepository.GetSpotExceptionsOutOfSpecPosts(weekStartDate, weekEndDate);

            // Assert
            Assert.AreEqual(0, result.Count);
        }
        [Test]
        public void GetSpotExceptionsOutOfSpecsPosts_OutOfSpec_Exist()
        {
            // Arrange
            var ingestedDateTime = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";
            var createdDateTime = new DateTime(2010, 10, 12);

            DateTime weekStartDate = new DateTime(2010, 01, 04);
            DateTime weekEndDate = new DateTime(2010, 01, 10);

            var spotExceptionOutOfspec = new List<SpotExceptionsOutOfSpecsDto>
            {
                 new SpotExceptionsOutOfSpecsDto
                 {
                      Id=1,
                      ReasonCodeMessage="",
                      EstimateId= 191756,
                      IsciName="AB82TXT2H",
                      RecommendedPlanId= 1196,
                      ProgramName="Q13 news at 10",
                      StationLegacyCallLetters="KOB",
                      SpotLengthId= 1,
                      AudienceId= 4,
                      Product="Pizza Hut",
                      FlightStartDate =  new DateTime(2020, 6, 2),
                      FlightEndDate = new DateTime(2020, 7, 2),
                      DaypartCode="PT",
                      GenreName="Horror",
                      ProgramNetwork = "",
                      ProgramAirTime = new DateTime(2010,1,4,8,7,15),
                      IngestedBy=ingestedBy,
                      IngestedAt=ingestedDateTime,
                      CreatedBy=ingestedBy,
                      CreatedAt=ingestedDateTime,
                      ModifiedBy=ingestedBy,
                      ModifiedAt=ingestedDateTime,
                      SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                      {
                          Id = 2,
                          ReasonCode = 1,
                          Reason = "spot aired outside daypart",
                          Label = "Daypart"
                      },
                     SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxOTY3MA==",
                     HouseIsci = "OMGN1016000H",
                     InventorySourceName = "TVB"
                 },
                 new SpotExceptionsOutOfSpecsDto
                 {
                      Id = 2,
                      ReasonCodeMessage="",
                      EstimateId= 191757,
                      IsciName="AB82VR58",
                      RecommendedPlanId= 1197,
                      ProgramName="FOX 13 10:00 News",
                      StationLegacyCallLetters="KSTP",
                      SpotLengthId= 2,
                      AudienceId= 5,
                      Product="Spotify",
                      FlightStartDate =  new DateTime(2018, 7, 2),
                      FlightEndDate = new DateTime(2018, 8, 2),
                      DaypartCode="PT",
                      GenreName="Horror",
                      ProgramNetwork = "",
                      ProgramAirTime = new DateTime(2010,1,4,8,7,15),
                      IngestedBy=ingestedBy,
                      IngestedAt=ingestedDateTime,
                      CreatedBy=ingestedBy,
                      CreatedAt=ingestedDateTime,
                      ModifiedBy=ingestedBy,
                      ModifiedAt=ingestedDateTime,
                      SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                      {
                          Id = 3,
                          ReasonCode = 2,
                          Reason = "genre content restriction",
                          Label = "Genre"
                      },
                     SpotUniqueHashExternal = "TE9DQUwtMTA1OTAyMDk1OQ==",
                     HouseIsci = "289J76GN16H",
                     InventorySourceName = "TVB",
                 },
                 new SpotExceptionsOutOfSpecsDto
                 {
                      Id = 3,
                      ReasonCodeMessage="",
                      EstimateId= 191758,
                      IsciName="AB44NR58",
                      RecommendedPlanId= 1198,
                      ProgramName="TEN O'CLOCK NEWS",
                      StationLegacyCallLetters="KHGI",
                      SpotLengthId= 3,
                      AudienceId= 6,
                      Product="Spotify",
                      FlightStartDate =  new DateTime(2018, 7, 2),
                      FlightEndDate = new DateTime(2018, 8, 2),
                      DaypartCode="PT",
                      GenreName="Horror",
                      ProgramNetwork = "",
                      ProgramAirTime = new DateTime(2010,1,11,8,7,15),
                      IngestedBy=ingestedBy,
                      IngestedAt=ingestedDateTime,
                      CreatedBy=ingestedBy,
                      CreatedAt=ingestedDateTime,
                      ModifiedBy=ingestedBy,
                      ModifiedAt=ingestedDateTime,
                      SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                      {
                          Id = 4,
                          ReasonCode = 3,
                          Reason = "affiliate content restriction",
                          Label = "Affiliate"
                      },
                      SpotUniqueHashExternal = "TE9DQUwtMTA1OTAyMTE1NA==",
                     HouseIsci = "289J76GN16H",
                     InventorySourceName = "TVB",
                 }
            };
            List<SpotExceptionsOutOfSpecsDto> result;

            var spotExceptionRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();

            // Act
            using (new TransactionScopeWrapper())
            {
                spotExceptionRepository.AddOutOfSpecs(spotExceptionOutOfspec);
                result = spotExceptionRepository.GetSpotExceptionsOutOfSpecPosts(weekStartDate, weekEndDate);
            }

            // Assert
            var settings = IntegrationTestHelper._GetJsonSettings();
            ((IgnorableSerializerContractResolver)(settings.ContractResolver)).Ignore(typeof(SpotExceptionsOutOfSpecsDto), "Id");

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, settings));
            Assert.AreEqual(2, result.Count);

        }

        [Test]
        public void SaveSpotExceptionsRecommendedPlanDecision_RecommendedPlans_Exist()
        {
            // Arrange
            var ingestedDateTime = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";
            bool isSpotExceptionsRecommendedPlanDecisionSaved = false;
            DateTime weekStartDate = new DateTime(2022, 04, 04);
            DateTime weekEndDate = new DateTime(2022, 04, 10);

            var spotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlansDto>
            {
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6840,
                    InventorySource = "Tegna",
                    HouseIsci = "840T42AY13H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 04, 10, 08, 28, 28),
                    StationLegacyCallLetters = "WBNS",
                    Affiliate = "CBS",
                    MarketCode = 135,
                    MarketRank = 33,
                    ProgramName = "CBS Mornings",
                    ProgramGenre = "INFORMATIONAL/NEWS",
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    CreatedBy = ingestedBy,
                    CreatedAt = ingestedDateTime,
                    ModifiedBy = ingestedBy,
                    ModifiedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 73,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "SYN",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 75,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
            };

            var SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
            {
                SpotExceptionsRecommendedPlanId = 334,
                UserName = ingestedBy,
                CreatedAt = ingestedDateTime,
                AcceptedAsInSpec = true
            };

            var expectedResult = true;
            List<SpotExceptionsRecommendedPlansDto> result;
            var spotExceptionRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();
            
            // Act
            using (new TransactionScopeWrapper())
            {
                spotExceptionRepository.AddSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlans);
                result = spotExceptionRepository.GetSpotExceptionsRecommendedPlans(weekStartDate, weekEndDate);
                SpotExceptionsRecommendedPlanDecision.SpotExceptionsId = result[0].Id;
                SpotExceptionsRecommendedPlanDecision.SpotExceptionsRecommendedPlanDetailId = result[0].SpotExceptionsRecommendedPlanDetails[0].Id;
                isSpotExceptionsRecommendedPlanDecisionSaved = spotExceptionRepository.SaveSpotExceptionsRecommendedPlanDecision(SpotExceptionsRecommendedPlanDecision);
            }
            
            // Assert
            Assert.AreEqual(expectedResult, isSpotExceptionsRecommendedPlanDecisionSaved);
        }

        [Test]
        public void GetRecommendedPlanAdvertiserMasterIdsPerWeek()
        {
            // Arrange
            var ingestedDateTime = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            DateTime weekStartDate = new DateTime(2010, 04, 04);
            DateTime weekEndDate = new DateTime(2010, 04, 10);

            var spotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlansDto>
            {
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6840,
                    InventorySource = "Tegna",
                    HouseIsci = "840T42AY13H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2010, 04, 10, 08, 28, 28),
                    StationLegacyCallLetters = "WBNS",
                    Affiliate = "CBS",
                    MarketCode = 135,
                    MarketRank = 33,
                    ProgramName = "CBS Mornings",
                    ProgramGenre = "INFORMATIONAL/NEWS",
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    CreatedBy = ingestedBy,
                    CreatedAt = ingestedDateTime,
                    ModifiedBy = ingestedBy,
                    ModifiedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 73,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "SYN",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 75,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                }
            };

            List<Guid> result;

            var spotExceptionRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();

            // Act
            using (new TransactionScopeWrapper())
            {
                spotExceptionRepository.AddSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlans);
                result = spotExceptionRepository.GetRecommendedPlanAdvertiserMasterIdsPerWeek(weekStartDate, weekEndDate);
            }

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanStations()
        {
            // Arrange
            var ingestedDateTime = new DateTime(2010, 10, 12);
            var ingestedBy = "Repository Test User";

            DateTime weekStartDate = new DateTime(2010, 04, 04);
            DateTime weekEndDate = new DateTime(2010, 04, 10);

            var spotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlansDto>
            {
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6840,
                    InventorySource = "Tegna",
                    HouseIsci = "840T42AY13H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2010, 04, 10, 08, 28, 28),
                    StationLegacyCallLetters = "WBNS",
                    Affiliate = "CBS",
                    MarketCode = 135,
                    MarketRank = 33,
                    ProgramName = "CBS Mornings",
                    ProgramGenre = "INFORMATIONAL/NEWS",
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    CreatedBy = ingestedBy,
                    CreatedAt = ingestedDateTime,
                    ModifiedBy = ingestedBy,
                    ModifiedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 73,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "SYN",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
                            {
                                UserName = ingestedBy,
                                CreatedAt = ingestedDateTime,
                                AcceptedAsInSpec = false
                            }
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 75,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA5MTAwMQ=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6616,
                    InventorySource = "Ference POD",
                    HouseIsci = "616MAY2913H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2010, 04, 10, 08, 41, 57),
                    StationLegacyCallLetters = "WDKA",
                    Affiliate = "IND",
                    MarketCode = 232,
                    MarketRank = 3873,
                    ProgramName = "Mike & Molly",
                    ProgramGenre = "COMEDY",
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    CreatedBy = ingestedBy,
                    CreatedAt = ingestedDateTime,
                    ModifiedBy = ingestedBy,
                    ModifiedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 623,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 624,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "SYN",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDI3MjQ2NQ=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6289,
                    InventorySource = "Sinclair Corp - Day Syn 10a-4p",
                    HouseIsci = "289IT2Y3P2H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2010, 04, 10, 09, 58, 55),
                    StationLegacyCallLetters = "KOMO",
                    Affiliate = "ABC",
                    MarketCode = 419,
                    MarketRank = 11,
                    ProgramName = "LIVE with Kelly and Ryan",
                    ProgramGenre = "TALK",
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    CreatedBy = ingestedBy,
                    CreatedAt = ingestedDateTime,
                    ModifiedBy = ingestedBy,
                    ModifiedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 1824,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 1923,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "SYN",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDIwMjA4Nw=F",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 5711,
                    InventorySource = "TVB Syndication/ROS",
                    HouseIsci = "711N51AY18H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2010, 04, 10, 08, 29, 23),
                    StationLegacyCallLetters = null,
                    Affiliate = "CBS",
                    MarketCode = 249,
                    MarketRank = 106,
                    ProgramName = "Funny You Should Ask",
                    ProgramGenre = "GAME SHOW",
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    CreatedBy = ingestedBy,
                    CreatedAt = ingestedDateTime,
                    ModifiedBy = ingestedBy,
                    ModifiedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 2222,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = false,
                            PlanClearancePercentage = null,
                            DaypartCode = "EM",
                            StartTime = 18000,
                            EndTime = 35999,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 2223,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 50000,
                            IsRecommendedPlan = true,
                            PlanClearancePercentage = null,
                            DaypartCode = "SYN",
                            StartTime = 28800,
                            EndTime = 7199,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                }
            };

            List<string> result;

            var spotExceptionRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();

            // Act
            using (new TransactionScopeWrapper())
            {
                spotExceptionRepository.AddSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlans);
                result = spotExceptionRepository.GetSpotExceptionsRecommendedPlanStations(weekStartDate, weekEndDate);
            }

            // Assert
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void GetSpotExceptionsRecommendedPlanMarkets()
        {
            // Arrange
            int marketCode = 135;

            var spotExceptionRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();

            string result;

            // Act
            using (new TransactionScopeWrapper())
            {
                result = spotExceptionRepository.GetMarketName(marketCode);
            }

            // Assert
            Assert.AreEqual(result, "Columbus, OH");
        }
    }
}


