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

            DateTime weekStartDate = new DateTime(2010, 01, 04);
            DateTime weekEndDate = new DateTime(2010, 01, 10);

            var spotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlansDto>
            {
                new SpotExceptionsRecommendedPlansDto
                {
                    Id = 1,
                    EstimateId =191756,
                    IsciName = "AB82TXT2H",
                    RecommendedPlanId = 1848,
                    ProgramName = "Q13 news at 10",
                    ProgramAirTime = new DateTime(2010,1,4,8,7,15),
                    StationLegacyCallLetters = "KOB",
                    Cost = 675,
                    Impressions = 765,
                    SpotLengthId = 12,
                    AudienceId = 431,
                    Product = "Pizza Hut",
                    FlightStartDate = new DateTime(2010, 1, 1),
                    FlightEndDate = new DateTime(2010, 3, 1),
                    DaypartId = 70615,
                    IngestedBy= ingestedBy,
                    IngestedAt= ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 1848,
                            MetricPercent = 20,
                            IsRecommendedPlan = true
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 1853,
                            MetricPercent = 40,
                            IsRecommendedPlan = false
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    Id = 2,
                    EstimateId =191757,
                    IsciName = "AB82VR58",
                    RecommendedPlanId = 1849,
                    ProgramName = "FOX 13 10:00 News",
                    ProgramAirTime = new DateTime(2010, 10, 10),
                    StationLegacyCallLetters = "KSTP",
                    Cost = 700,
                    Impressions = 879,
                    SpotLengthId = 11,
                    AudienceId = 430,
                    Product = "Spotify",
                    FlightStartDate = new DateTime(2010, 7, 2, 4, 10, 30),
                    FlightEndDate = new DateTime(2010, 11, 2),
                    DaypartId = 70615,
                    IngestedBy = ingestedBy,
                    IngestedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 1849,
                            MetricPercent = 70,
                            IsRecommendedPlan = true
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 1854,
                            MetricPercent = 40,
                            IsRecommendedPlan = false
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    Id = 3,
                    EstimateId =191758,
                    IsciName = "AB44NR58",
                    RecommendedPlanId = 1850,
                    ProgramName = "TEN O'CLOCK NEWS",
                    ProgramAirTime = new DateTime(2010,1,6,11,15,30),
                    StationLegacyCallLetters="KHGI",
                    Cost = 0,
                    Impressions = 877,
                    SpotLengthId = 12,
                    AudienceId = 431,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2010, 1, 2),
                    FlightEndDate = new DateTime(2020, 3, 1),
                    DaypartId = 70616,
                    IngestedBy = ingestedBy,
                    IngestedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 1850,
                            MetricPercent = 70,
                            IsRecommendedPlan = true
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 1855,
                            MetricPercent = 40,
                            IsRecommendedPlan = false
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    Id = 4,
                    EstimateId =191759,
                    IsciName = "AB21QR58",
                    RecommendedPlanId = 1851,
                    ProgramName = "Product1",
                    ProgramAirTime = new DateTime(2010, 3, 10,2,5,15),
                    StationLegacyCallLetters="KWCH" ,
                    Cost = 987,
                    Impressions = 987,
                    SpotLengthId = 11,
                    AudienceId = 430,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2010, 3, 6),
                    FlightEndDate = new DateTime(2010, 4, 6),
                    DaypartId = 70617,
                    IngestedBy = ingestedBy,
                    IngestedAt = ingestedDateTime,
                    SpotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 1851,
                            MetricPercent = 80,
                            IsRecommendedPlan = true
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 1856,
                            MetricPercent = 40,
                            IsRecommendedPlan = false
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    Id = 5,
                    EstimateId =191760,
                    IsciName = "AB44NR58",
                    ProgramName = "TProduct2",
                    ProgramAirTime = new DateTime(2010,1,10,23,45,00),
                    StationLegacyCallLetters="WDAY" ,
                    Cost = 555,
                    Impressions = 9878,
                    SpotLengthId = 10,
                    AudienceId = 429,
                    Product = "Nike",
                    FlightStartDate = new DateTime(2010, 1, 5),
                    FlightEndDate = new DateTime(2010, 3, 3),
                    DaypartId = 70618,
                    IngestedBy = ingestedBy,
                    IngestedAt = ingestedDateTime
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
                      DaypartId= 57,
                      ProgramDaypartId= 28,
                      ProgramFlightStartDate= new DateTime(2020, 6, 2),
                      ProgramFlightEndDate = new DateTime(2020, 7, 2),
                      ProgramNetwork = "",
                      ProgramAudienceId = 4,
                      ProgramAirTime = new DateTime(2010,1,4,8,7,15),
                      IngestedBy=ingestedBy,
                      IngestedAt=ingestedDateTime,
                      SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                      {
                          Id = 2,
                          ReasonCode = 1,
                          Reason = "spot aired outside daypart",
                          Label = "Daypart"
                      }
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
                      DaypartId= 28,
                      ProgramDaypartId= 56,
                      ProgramFlightStartDate= new DateTime(2018, 7, 2),
                      ProgramFlightEndDate = new DateTime(2018, 8, 2),
                      ProgramNetwork = "",
                      ProgramAudienceId = 5,
                      ProgramAirTime = new DateTime(2010,1,4,8,7,15),
                      IngestedBy=ingestedBy,
                      IngestedAt=ingestedDateTime,
                      SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                      {
                          Id = 3,
                          ReasonCode = 2,
                          Reason = "genre content restriction",
                          Label = "Genre"
                      }
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
                      DaypartId= 56,
                      ProgramDaypartId= 57,
                      ProgramFlightStartDate= new DateTime(2018, 7, 2),
                      ProgramFlightEndDate = new DateTime(2018, 8, 2),
                      ProgramNetwork = "",
                      ProgramAudienceId = 6,
                      ProgramAirTime = new DateTime(2010,1,11,8,7,15),
                      IngestedBy=ingestedBy,
                      IngestedAt=ingestedDateTime,
                      SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                      {
                          Id = 4,
                          ReasonCode = 3,
                          Reason = "affiliate content restriction",
                          Label = "Affiliate"
                      }
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
    }
}


