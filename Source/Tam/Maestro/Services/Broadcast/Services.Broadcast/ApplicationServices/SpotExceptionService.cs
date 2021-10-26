using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface ISpotExceptionService : IApplicationService
    {
        /// <summary>
        /// Add mock data to spot exceptions tables.
        /// </summary>   
        bool AddSpotExceptionData();
        /// <summary>
        /// Clear data from spot exceptions tables.
        /// </summary>   
        bool ClearSpotExceptionData();
        /// <summary>
        /// Gets the available outofspecPost within the start and end week
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecPostsRequest">The media week start and end date</param>
        /// <returns>List of SpotExceptionsOutOfSpecPostsResultDto object</returns>
        List<SpotExceptionsOutOfSpecPostsResultDto> GetSpotExceptionsOutOfSpecsPosts(SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest);

        /// <summary>
        /// Gets spot exceptions recommended plans
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlansRequest">The spot exceptions recommended plans request parameters</param>
        /// <returns>The spot exceptions recommended plans</returns>
        List<SpotExceptionsRecommendedPlansResultDto> GetSpotExceptionsRecommendedPlans(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest);

        /// <summary>
        /// Gets spot exceptions recommended plan details
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanDetailsRequest">The spot exceptions recommended plan detail request parameters</param>
        /// <returns>The spot exceptions recommended plan details</returns>
        SpotExceptionsRecommendedPlanDetailsResultDto GetSpotExceptionsRecommendedPlanDetails(SpotExceptionsRecommendedPlanDetailsRequestDto spotExceptionsRecommendedPlanDetailsRequest);

        /// <summary>
        /// Save SpotExceptionsOutOfSpecs Decisions data
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecDecisionsPostsRequest">The SpotExceptionsOutOfSpecDecisions Request</param>
        /// <param name="userName">User Name</param>
        /// <returns>true or false</returns>
        bool SaveSpotExceptionsOutOfSpecsDecisions(SpotExceptionsOutOfSpecDecisionsPostsRequestDto spotExceptionsOutOfSpecDecisionsPostsRequest, string userName);
    }

    public class SpotExceptionService : BroadcastBaseClass, ISpotExceptionService
    {
        private readonly ISpotExceptionRepository _SpotExceptionRepository;
        private readonly IDateTimeEngine _DateTimeEngine;
        public SpotExceptionService(IDataRepositoryFactory dataRepositoryFactory, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper, IDateTimeEngine dateTimeEngine) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionRepository = dataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();
            _DateTimeEngine = dateTimeEngine;
        }

        public bool AddSpotExceptionData()
        {
            List<SpotExceptionsRecommendedPlansDto> spotExceptionsRecommendedPlans = _GetSpotExceptionsRecommendedPlansMock();
            List<SpotExceptionsRecommendedPlanDetailsDto> spotExceptionsRecommendedPlanDetails = _GetSpotExceptionsRecommendedPlanDetailsMock();
            List<SpotExceptionsRecommendedPlanDecisionDto> spotExceptionsRecommendedPlanDecision = _GetSpotExceptionsRecommendedPlanDecisionMock();
            List<SpotExceptionsOutOfSpecDecisionsDto> spotExceptionsOutOfSpecDecisions = _GetSpotExceptionsOutOfSpecDecisionsMock();
            List<SpotExceptionsOutOfSpecsDto> spotExceptionsOutOfSpecs = _GetSpotExceptionsOutOfSpecsMock();

            var result = _SpotExceptionRepository.AddSpotExceptionData(spotExceptionsRecommendedPlans, spotExceptionsRecommendedPlanDetails,
spotExceptionsRecommendedPlanDecision, spotExceptionsOutOfSpecs, spotExceptionsOutOfSpecDecisions);
            return result;
        }
        public bool ClearSpotExceptionData()
        {
            var result = _SpotExceptionRepository.ClearSpotExceptionData();
            return result;
        }
        private List<SpotExceptionsRecommendedPlansDto> _GetSpotExceptionsRecommendedPlansMock()
        {
            var spotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlansDto>
            {
                new SpotExceptionsRecommendedPlansDto
                {
                        Id = 1,
                        EstimateId =191756,
                        IsciName = "AB82TXT2H",
                        RecommendedPlanId = 215,
                        ProgramName = "Q13 news at 10",
                        ProgramAirTime = new DateTime(2020, 10, 10),
                        StationLegacyCallLetters = "KOB",
                        Cost = 675,
                        Impressions = 765,
                        SpotLengthId = 12,
                        AudienceId = 431,
                        Product = "Pizza Hut",
                        FlightStartDate = new DateTime(2019, 8, 1),
                        FlightEndDate = new DateTime(2019, 9, 1),
                        DaypartId = 70615,
                        IngestedBy="Mock Data",
                        IngestedAt=DateTime.Now

                },
                new SpotExceptionsRecommendedPlansDto
                {
                        Id = 2,
                        EstimateId =191757,
                        IsciName = "AB82VR58",
                        RecommendedPlanId = 216,
                        ProgramName = "FOX 13 10:00 News",
                        ProgramAirTime = new DateTime(2021, 10, 10),
                        StationLegacyCallLetters = "KSTP",
                        Cost = 700,
                        Impressions = 879,
                        SpotLengthId = 11,
                        AudienceId = 430,
                        Product = "Spotify",
                        FlightStartDate = new DateTime(2018, 7, 2),
                        FlightEndDate = new DateTime(2018, 8, 2),
                        DaypartId = 70615,
                        IngestedBy="Mock Data",
                        IngestedAt=DateTime.Now
                },
                new SpotExceptionsRecommendedPlansDto
                {
                        Id = 3,
                        EstimateId =191758,
                        IsciName = "AB44NR58",
                        RecommendedPlanId = 217,
                        ProgramName = "TEN O'CLOCK NEWS",
                        ProgramAirTime = new DateTime(2021, 10, 10),
                        StationLegacyCallLetters="KHGI",
                        Cost = 0,
                        Impressions = 877,
                        SpotLengthId = 12,
                        AudienceId = 431,
                        Product = "Nike",
                        FlightStartDate = new DateTime(2021, 3, 6),
                        FlightEndDate = new DateTime(2021, 4, 6),
                        DaypartId = 70616,
                        IngestedBy="Mock Data",
                        IngestedAt=DateTime.Now
                },
                new SpotExceptionsRecommendedPlansDto
                {
                        Id = 4,
                        EstimateId =191759,
                        IsciName = "AB21QR58",
                        RecommendedPlanId = 218,
                        ProgramName = "Product1",
                        ProgramAirTime = new DateTime(2021, 10, 10),
                        StationLegacyCallLetters="KWCH" ,
                        Cost = 987,
                        Impressions = 987,
                        SpotLengthId = 11,
                        AudienceId = 430,
                        Product = "Nike",
                        FlightStartDate = new DateTime(2018, 3, 6),
                        FlightEndDate = new DateTime(2018, 4, 6),
                        DaypartId = 70617,
                        IngestedBy="Mock Data",
                        IngestedAt=DateTime.Now
                },
                new SpotExceptionsRecommendedPlansDto
                {
                        Id = 5,
                        EstimateId =191760,
                        IsciName = "AB44NR58",
                        RecommendedPlanId = 219,
                        ProgramName = "TProduct2",
                        ProgramAirTime = new DateTime(2021, 10, 10),
                        StationLegacyCallLetters="WDAY" ,
                        Cost = 555,
                        Impressions = 9878,
                        SpotLengthId = 10,
                        AudienceId = 429,
                        Product = "Nike",
                        FlightStartDate = new DateTime(2019, 7, 3),
                        FlightEndDate = new DateTime(2019, 8, 3),
                        DaypartId = 70618,
                        IngestedBy="Mock Data",
                        IngestedAt=DateTime.Now
                },
                new SpotExceptionsRecommendedPlansDto
                {
                        Id = 6,
                        EstimateId =191761,
                        IsciName = "AB33PR58",
                        RecommendedPlanId = 220,
                        ProgramName = "TEN O'CLOCK NEWS",
                        ProgramAirTime = new DateTime(2021, 10, 10),
                        StationLegacyCallLetters="KPNX" ,
                        Cost = 987,
                        Impressions = 999,
                        SpotLengthId = 10,
                        AudienceId = 428,
                        Product = "Nike",
                        FlightStartDate = new DateTime(2019, 7, 1),
                        FlightEndDate = new DateTime(2019, 8, 1),
                        DaypartId = 70619,
                        IngestedBy="Mock Data",
                        IngestedAt=DateTime.Now
                },
                new SpotExceptionsRecommendedPlansDto
                {
                        Id = 7,
                        EstimateId =191762,
                        IsciName = "AB79PR58",
                        RecommendedPlanId = 221,
                        ProgramName = "Product4",
                        ProgramAirTime = new DateTime(2021, 10, 10),
                        StationLegacyCallLetters="KELO" ,
                        Cost = 907,
                        Impressions = 5467,
                        SpotLengthId = 09,
                        AudienceId = 427,
                        Product = "Nike",
                        FlightStartDate = new DateTime(2021, 8, 6),
                        FlightEndDate = new DateTime(2021, 9, 6),
                        DaypartId = 70620,
                        IngestedBy="Mock Data",
                        IngestedAt=DateTime.Now
                },
                new SpotExceptionsRecommendedPlansDto
                {
                        Id = 8,
                        EstimateId =191763,
                        IsciName = "AB81GR58",
                        RecommendedPlanId = 222,
                        ProgramName = "Product3",
                        ProgramAirTime = new DateTime(2021, 10, 10),
                        StationLegacyCallLetters="KXMC" ,
                        Cost = 453,
                        Impressions = 8795,
                        SpotLengthId = 08,
                        AudienceId = 426,
                        Product = "Nike",
                        FlightStartDate = new DateTime(2020, 8, 2),
                        FlightEndDate = new DateTime(2020, 9, 2),
                        DaypartId = 70621,
                        IngestedBy="Mock Data",
                        IngestedAt=DateTime.Now
                },
                new SpotExceptionsRecommendedPlansDto
                {
                        Id = 9,
                        EstimateId =191764,
                        IsciName = "AB87GR58",
                        RecommendedPlanId = 223,
                        ProgramName = "Product6",
                        ProgramAirTime = new DateTime(2021, 10, 10),
                        StationLegacyCallLetters="WTTV" ,
                        Cost = 987,
                        Impressions = 8767,
                        SpotLengthId = 07,
                        AudienceId = 425,
                        Product = "Nike",
                        FlightStartDate = new DateTime(2020, 6, 6),
                        FlightEndDate = new DateTime(2020, 7, 6),
                        DaypartId = 70622,
                        IngestedBy="Mock Data",
                        IngestedAt=DateTime.Now
                },
                new SpotExceptionsRecommendedPlansDto
                {
                        Id = 10,
                        EstimateId =191765,
                        IsciName = "AB83PR58",
                        RecommendedPlanId = 224,
                        ProgramName = "Product8",
                        ProgramAirTime = new DateTime(2021, 10, 10),
                        StationLegacyCallLetters="WCCO" ,
                        Cost = 767,
                        Impressions = 9832,
                        SpotLengthId = 06,
                        AudienceId = 424,
                        Product = "Nike",
                        FlightStartDate = new DateTime(2020, 5, 6),
                        FlightEndDate = new DateTime(2020, 6, 6),
                        DaypartId = 70623,
                        IngestedBy="Mock Data",
                        IngestedAt=DateTime.Now
                }
            };

            return spotExceptionsRecommendedPlans;
        }

        private List<SpotExceptionsRecommendedPlanDetailsDto> _GetSpotExceptionsRecommendedPlanDetailsMock()
        {
            var spotExceptionsRecommendedPlanDetails = new List<SpotExceptionsRecommendedPlanDetailsDto>
            {
                new SpotExceptionsRecommendedPlanDetailsDto
                {
                  Id = 1,
                  SpotExceptionsRecommendedPlanId=1,
                  RecommendedPlanId=215,
                  MetricPercent=20,
                  IsRecommendedPlan=true
                },
                new SpotExceptionsRecommendedPlanDetailsDto
                {
                  Id = 2,
                  SpotExceptionsRecommendedPlanId=2,
                  RecommendedPlanId=216,
                  MetricPercent=45,
                  IsRecommendedPlan=true
                },
                new SpotExceptionsRecommendedPlanDetailsDto
                {
                        Id = 3,
                        SpotExceptionsRecommendedPlanId=3,
                        RecommendedPlanId = 217,
                        MetricPercent=28,
                        IsRecommendedPlan=true
                },
                new SpotExceptionsRecommendedPlanDetailsDto
                {
                        Id = 4,
                        SpotExceptionsRecommendedPlanId=4,
                        RecommendedPlanId = 218,
                        MetricPercent=56,
                        IsRecommendedPlan=true
                },
                new SpotExceptionsRecommendedPlanDetailsDto
                {
                        Id = 5,
                        SpotExceptionsRecommendedPlanId=5,
                        RecommendedPlanId = 219,
                        MetricPercent=76,
                        IsRecommendedPlan=true
                },
                new SpotExceptionsRecommendedPlanDetailsDto
                {
                        Id = 6,
                        SpotExceptionsRecommendedPlanId=6,
                        RecommendedPlanId = 220,
                        MetricPercent=87,
                        IsRecommendedPlan=true
                },
                new SpotExceptionsRecommendedPlanDetailsDto
                {
                           Id = 7,
                        SpotExceptionsRecommendedPlanId=7,
                        RecommendedPlanId = 221,
                        MetricPercent=82,
                  IsRecommendedPlan=true
                },
                new SpotExceptionsRecommendedPlanDetailsDto
                {
                        Id = 8,
                        SpotExceptionsRecommendedPlanId=8,
                        RecommendedPlanId = 222,
                        MetricPercent=79,
                        IsRecommendedPlan=true
                },
                new SpotExceptionsRecommendedPlanDetailsDto
                {
                        Id = 9,
                        SpotExceptionsRecommendedPlanId=9,
                        RecommendedPlanId = 223,
                        MetricPercent=91,
                        IsRecommendedPlan=true
                },
                new SpotExceptionsRecommendedPlanDetailsDto
                {
                        Id = 10,
                        SpotExceptionsRecommendedPlanId=10,
                        RecommendedPlanId = 224,
                        MetricPercent=96,
                        IsRecommendedPlan=true
                }
            };

            return spotExceptionsRecommendedPlanDetails;
        }

        private List<SpotExceptionsRecommendedPlanDecisionDto> _GetSpotExceptionsRecommendedPlanDecisionMock()
        {
            var spotExceptionsRecommendedPlanDecision = new List<SpotExceptionsRecommendedPlanDecisionDto>
            {
                new SpotExceptionsRecommendedPlanDecisionDto
                {
                  Id = 1,
                  SpotExceptionsRecommendedPlanDetailId=1,
                  UserName="MockData",
                  CreatedAt=DateTime.Now
                }
            };

            return spotExceptionsRecommendedPlanDecision;
        }
        private List<SpotExceptionsOutOfSpecDecisionsDto> _GetSpotExceptionsOutOfSpecDecisionsMock()
        {
            var spotExceptionsOutOfSpecDecisions = new List<SpotExceptionsOutOfSpecDecisionsDto>
            {
                new SpotExceptionsOutOfSpecDecisionsDto
                {
                  Id = 1,
                  SpotExceptionsOutOfSpecId=1,
                  AcceptedAsInSpec=true,
                  DecisionNotes="",
                  UserName = "MockData",
                  CreatedAt = DateTime.Now
                }
            };

            return spotExceptionsOutOfSpecDecisions;
        }

        private List<SpotExceptionsOutOfSpecsDto> _GetSpotExceptionsOutOfSpecsMock()
        {
            var spotExceptionsOutOfSpecs = new List<SpotExceptionsOutOfSpecsDto>
            {
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 1,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191756,
                  IsciName="AB82TXT2H",
                  RecommendedPlanId= 215,
                  ProgramName="Q13 news at 10",
                  StationLegacyCallLetters="KOB",
                  SpotLengthId= 12,
                  AudienceId= 431,
                  Product="Pizza Hut",
                  FlightStartDate =  new DateTime(2020, 6, 2),
                  FlightEndDate = new DateTime(2020, 7, 2),
                  DaypartId= 70612,
                  ProgramDaypartId= 70641,
                  ProgramFlightStartDate= new DateTime(2020, 6, 2),
                  ProgramFlightEndDate = new DateTime(2020, 7, 2),
                  ProgramNetwork = "",
                  ProgramAudienceId = 431,
                  ProgramAirTime = new DateTime(2021, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 2,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191757,
                  IsciName="AB82VR58",
                  RecommendedPlanId= 216,
                  ProgramName="FOX 13 10:00 News",
                  StationLegacyCallLetters="KSTP",
                  SpotLengthId= 11,
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartId= 70642,
                  ProgramDaypartId= 70613,
                  ProgramFlightStartDate= new DateTime(2018, 7, 2),
                  ProgramFlightEndDate = new DateTime(2018, 8, 2),
                  ProgramNetwork = "",
                  ProgramAudienceId = 430,
                  ProgramAirTime = new DateTime(2021, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 3,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191758,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 217,
                  ProgramName="TEN O'CLOCK NEWS",
                  StationLegacyCallLetters="KHGI",
                  SpotLengthId= 12,
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 7, 2),
                  FlightEndDate = new DateTime(2018, 8, 2),
                  DaypartId= 70643,
                  ProgramDaypartId= 70614,
                  ProgramFlightStartDate= new DateTime(2018, 7, 2),
                  ProgramFlightEndDate = new DateTime(2018, 8, 2),
                  ProgramNetwork = "",
                  ProgramAudienceId = 431,
                  ProgramAirTime = new DateTime(2021, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 4,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191759,
                  IsciName="AB21QR58",
                  RecommendedPlanId= 218,
                  ProgramName="Product1",
                  StationLegacyCallLetters="KWCH",
                  SpotLengthId= 11,
                  AudienceId= 430,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 3, 6),
                  FlightEndDate = new DateTime(2018, 4, 6),
                  DaypartId= 70644,
                  ProgramDaypartId= 70615,
                  ProgramFlightStartDate= new DateTime(2018, 3, 6),
                  ProgramFlightEndDate = new DateTime(2018, 4, 6),
                  ProgramNetwork = "",
                  ProgramAudienceId = 430,
                  ProgramAirTime = new DateTime(2021, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 5,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191760,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 219,
                  ProgramName="TProduct2",
                  StationLegacyCallLetters="WDAY",
                  SpotLengthId= 10,
                  AudienceId= 429,
                  Product="Spotify",
                  FlightStartDate =  new DateTime(2018, 3, 6),
                  FlightEndDate = new DateTime(2018, 4, 6),
                  DaypartId= 70616,
                  ProgramDaypartId= 70645,
                  ProgramFlightStartDate= new DateTime(2019, 7, 3),
                  ProgramFlightEndDate = new DateTime(2019, 8, 3),
                  ProgramNetwork = "",
                  ProgramAudienceId = 429,
                  ProgramAirTime = new DateTime(2020, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 6,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191761,
                  IsciName="AB33PR58",
                  RecommendedPlanId= 220,
                  ProgramName="TEN O'CLOCK NEWS",
                  StationLegacyCallLetters="KPNX",
                  SpotLengthId= 10,
                  AudienceId= 428,
                  Product="Nike",
                  FlightStartDate =  new DateTime(2019, 7, 1),
                  FlightEndDate = new DateTime(2019, 8, 1),
                  DaypartId= 70617,
                  ProgramDaypartId= 70646,
                  ProgramFlightStartDate= new DateTime(2019, 7, 1),
                  ProgramFlightEndDate = new DateTime(2019, 8, 1),
                  ProgramNetwork = "",
                  ProgramAudienceId = 428,
                  ProgramAirTime = new DateTime(2021, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 7,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191762,
                  IsciName="AB79PR58",
                  RecommendedPlanId= 221,
                  ProgramName="Product4",
                  StationLegacyCallLetters="KELO",
                  SpotLengthId= 09,
                  AudienceId= 427,
                  Product="Nike",
                  FlightStartDate =  new DateTime(2021, 8, 6),
                  FlightEndDate = new DateTime(2021, 9, 6),
                  DaypartId= 70618,
                  ProgramDaypartId= 70647,
                  ProgramFlightStartDate= new DateTime(2021, 8, 6),
                  ProgramFlightEndDate = new DateTime(2021, 9, 6),
                  ProgramNetwork = "",
                  ProgramAudienceId = 427,
                  ProgramAirTime = new DateTime(2021, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 8,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191763,
                  IsciName="AB81GR58",
                  RecommendedPlanId= 222,
                  ProgramName="Product3",
                  StationLegacyCallLetters="KXMC",
                  SpotLengthId= 08,
                  AudienceId= 426,
                  Product="Nike",
                  FlightStartDate =  new DateTime(2020, 8, 2),
                  FlightEndDate = new DateTime(2021, 9, 6),
                  DaypartId= 70619,
                  ProgramDaypartId= 70648,
                  ProgramFlightStartDate= new DateTime(2020, 8, 2),
                  ProgramFlightEndDate = new DateTime(2020, 9, 2),
                  ProgramNetwork = "",
                  ProgramAudienceId = 426,
                  ProgramAirTime = new DateTime(2021, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 9,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191764,
                  IsciName="AB87GR58",
                  RecommendedPlanId= 223,
                  ProgramName="Product6",
                  StationLegacyCallLetters="WTTV",
                  SpotLengthId= 07,
                  AudienceId= 425,
                  Product="Nike",
                  FlightStartDate =  new DateTime(2020, 6, 6),
                  FlightEndDate = new DateTime(2020, 7, 6),
                  DaypartId= 70620,
                  ProgramDaypartId= 70649,
                  ProgramFlightStartDate= new DateTime(2020, 6, 6),
                  ProgramFlightEndDate = new DateTime(2020, 7, 6),
                  ProgramNetwork = "",
                  ProgramAudienceId = 425,
                  ProgramAirTime = new DateTime(2021, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  Id = 10,
                  ReasonCode="",
                  ReasonCodeMessage="",
                  EstimateId= 191765,
                  IsciName="AB83PR58",
                  RecommendedPlanId= 224,
                  ProgramName="Product8",
                  StationLegacyCallLetters="WCCO",
                  SpotLengthId= 06,
                  AudienceId= 424,
                  Product="Nike",
                  FlightStartDate =  new DateTime(2020, 5, 6),
                  FlightEndDate = new DateTime(2020, 7, 6),
                  DaypartId= 70621,
                  ProgramDaypartId= 70650,
                  ProgramFlightStartDate= new DateTime(2020, 5, 6),
                  ProgramFlightEndDate = new DateTime(2020, 6, 6),
                  ProgramNetwork = "",
                  ProgramAudienceId = 424,
                  ProgramAirTime = new DateTime(2021, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now
                }
            };

            return spotExceptionsOutOfSpecs;
        }
        public List<SpotExceptionsOutOfSpecPostsResultDto> GetSpotExceptionsOutOfSpecsPosts(SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest)
        {
            var spotExceptionsOutOfSpecPostsResult = new List<SpotExceptionsOutOfSpecPostsResultDto>();
            const string programAirDateFormat = "MM/dd/yyyy";
            const string programAirTimeFormat = "hh:mm:ss tt";

            var spotExceptionsoutOfSpecsPosts = _SpotExceptionRepository.GetSpotExceptionsOutOfSpecPosts(spotExceptionsOutOfSpecPostsRequest.WeekStartDate, spotExceptionsOutOfSpecPostsRequest.WeekEndDate);
            if (spotExceptionsoutOfSpecsPosts?.Any() ?? false)
            {
                spotExceptionsOutOfSpecPostsResult = spotExceptionsoutOfSpecsPosts.Select(spotExceptionsOutOfSpecs => new SpotExceptionsOutOfSpecPostsResultDto
                {
                    Id = spotExceptionsOutOfSpecs.Id,
                    Status = spotExceptionsOutOfSpecs.SpotExceptionsOutOfSpecId != null,
                    EstimateId = spotExceptionsOutOfSpecs.EstimateId,
                    IsciName = spotExceptionsOutOfSpecs.IsciName,
                    RecommendedPlan = spotExceptionsOutOfSpecs.RecommendedPlanName, 
                    Reason=spotExceptionsOutOfSpecs.ReasonCodeMessage,
                    Station = spotExceptionsOutOfSpecs.StationLegacyCallLetters,                  
                    SpotLengthString = spotExceptionsOutOfSpecs.SpotLengthString,
                    AudienceName = spotExceptionsOutOfSpecs.AudienceName,
                    ProductName = spotExceptionsOutOfSpecs.Product,
                    DaypartCode=spotExceptionsOutOfSpecs.DaypartCode,
                    FlightStartDate=spotExceptionsOutOfSpecs.ProgramFlightStartDate.ToString(),
                    FlightEndDate=spotExceptionsOutOfSpecs.ProgramFlightEndDate.ToString(),
                    ProgramName = spotExceptionsOutOfSpecs.ProgramName,
                    ProgramAirDate = spotExceptionsOutOfSpecs.ProgramAirTime.ToString(programAirDateFormat),
                    ProgramAirTime = spotExceptionsOutOfSpecs.ProgramAirTime.ToString(programAirTimeFormat)
                }).ToList();
            }
            return spotExceptionsOutOfSpecPostsResult;
        }

        /// <inheritdoc />
        public List<SpotExceptionsRecommendedPlansResultDto> GetSpotExceptionsRecommendedPlans(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest)
        {
            var spotExceptionsRecommendedPlansResults = new List<SpotExceptionsRecommendedPlansResultDto>();
            const string programAirDateFormat = "MM/dd/yyyy";
            const string programAirTimeFormat = "hh:mm:ss tt";

            var spotExceptionsRecommendedPlans = _SpotExceptionRepository.GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest.WeekStartDate, spotExceptionsRecommendedPlansRequest.WeekEndDate);
            if (spotExceptionsRecommendedPlans?.Any() ?? false)
            {
                spotExceptionsRecommendedPlansResults = spotExceptionsRecommendedPlans.Select(spotExceptionsRecommendedPlan =>
                {
                    var spotExceptionsRecommendedPlanDetailWithDecision = spotExceptionsRecommendedPlan.SpotExceptionsRecommendedPlanDetails.SingleOrDefault(spotExceptionsRecommendedPlanDetail => spotExceptionsRecommendedPlanDetail.SpotExceptionsRecommendedPlanDecision != null);

                    var spotExceptionsRecommendedPlansResult = new SpotExceptionsRecommendedPlansResultDto
                    {
                        Id = spotExceptionsRecommendedPlan.Id,
                        Status = spotExceptionsRecommendedPlanDetailWithDecision != null,
                        EstimateId = spotExceptionsRecommendedPlan.EstimateId,
                        IsciName = spotExceptionsRecommendedPlan.IsciName,
                        RecommendedPlan = spotExceptionsRecommendedPlan.RecommendedPlanName,
                        Affiliate = spotExceptionsRecommendedPlan.Affiliate,
                        Market = spotExceptionsRecommendedPlan.Market,
                        Station = spotExceptionsRecommendedPlan.StationLegacyCallLetters,
                        Cost = spotExceptionsRecommendedPlan.Cost ?? 0,
                        Impressions = spotExceptionsRecommendedPlan.Impressions ?? 0,
                        SpotLengthString = spotExceptionsRecommendedPlan.SpotLength != null ? $":{spotExceptionsRecommendedPlan.SpotLength.Length}" : null,
                        AudienceName = spotExceptionsRecommendedPlan.Audience?.Name,
                        ProductName = spotExceptionsRecommendedPlan.Product,
                        ProgramName = spotExceptionsRecommendedPlan.ProgramName,
                        ProgramAirDate = spotExceptionsRecommendedPlan.ProgramAirTime.ToString(programAirDateFormat),
                        ProgramAirTime = spotExceptionsRecommendedPlan.ProgramAirTime.ToString(programAirTimeFormat)
                    };
                    return spotExceptionsRecommendedPlansResult;
                }).ToList();
            }
            return spotExceptionsRecommendedPlansResults;
        }

        /// <inheritdoc />
        public SpotExceptionsRecommendedPlanDetailsResultDto GetSpotExceptionsRecommendedPlanDetails(SpotExceptionsRecommendedPlanDetailsRequestDto spotExceptionsRecommendedPlanDetailsRequest)
        {
            var spotExceptionsRecommendedPlan = _SpotExceptionRepository.GetSpotExceptionsRecommendedPlanById(spotExceptionsRecommendedPlanDetailsRequest.Id);
            if (spotExceptionsRecommendedPlan == null)
            {
                return null;
            }

            const string programAirDateFormat = "MM/dd/yyyy";
            const string programAirTimeFormat = "hh:mm:ss tt";

            var spotExceptionsRecommendedPlanDetailsResult = new SpotExceptionsRecommendedPlanDetailsResultDto
            {
                Id = spotExceptionsRecommendedPlan.Id,
                EstimateId = spotExceptionsRecommendedPlan.EstimateId,
                SpotLengthString = spotExceptionsRecommendedPlan.SpotLength != null ? $":{spotExceptionsRecommendedPlan.SpotLength.Length}" : null,
                DaypartCode = spotExceptionsRecommendedPlan.DaypartDetail?.Code,
                AudienceName = spotExceptionsRecommendedPlan.Audience?.Name,
                Product = spotExceptionsRecommendedPlan.Product,
                FlightStartDate = spotExceptionsRecommendedPlan.FlightStartDate?.ToString(),
                FlightEndDate = spotExceptionsRecommendedPlan.FlightEndDate?.ToString(),
                ProgramName = spotExceptionsRecommendedPlan.ProgramName,
                ProgramAirDate = spotExceptionsRecommendedPlan.ProgramAirTime.ToString(programAirDateFormat),
                ProgramAirTime = spotExceptionsRecommendedPlan.ProgramAirTime.ToString(programAirTimeFormat),
                Plans = spotExceptionsRecommendedPlan.SpotExceptionsRecommendedPlanDetails.Select(spotExceptionsRecommendedPlanDetail => new RecommendedPlanDetailResultDto
                {
                    Name = spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.Name,
                    SpotLengthString = string.Join(", ", spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.SpotLengths.Select(spotLength => $":{spotLength.Length}")),
                    FlightStartDate = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightStartDate}",
                    FlightEndDate = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightEndDate}",
                    IsRecommendedPlan = spotExceptionsRecommendedPlanDetail.IsRecommendedPlan,
                    IsSelected = spotExceptionsRecommendedPlanDetail.SpotExceptionsRecommendedPlanDecision != null
                }).ToList()
            };
            return spotExceptionsRecommendedPlanDetailsResult;
        }

        public bool SaveSpotExceptionsOutOfSpecsDecisions(SpotExceptionsOutOfSpecDecisionsPostsRequestDto spotExceptionsOutOfSpecDecisionsPostsRequest, string userName)
        {
            var createdAt = _DateTimeEngine.GetCurrentMoment();

            var isSpotExceptionsOutOfSpecDecision = _SpotExceptionRepository.SaveSpotExceptionsOutOfSpecsDecisions(spotExceptionsOutOfSpecDecisionsPostsRequest, userName, createdAt);

            return isSpotExceptionsOutOfSpecDecision;
        }        
    }
}
