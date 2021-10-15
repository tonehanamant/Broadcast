using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
    public class SpotExceptionService : BroadcastBaseClass, ISpotExceptionService
    {
        private readonly ISpotExceptionRepository _SpotExceptionRepository;
        public SpotExceptionService(IDataRepositoryFactory dataRepositoryFactory, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionRepository = dataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();
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
                        SpotLenthId = 12,
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
                        SpotLenthId = 11,
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
                        SpotLenthId = 12,
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
                        SpotLenthId = 11,
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
                        SpotLenthId = 10,
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
                        SpotLenthId = 10,
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
                        SpotLenthId = 09,
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
                        SpotLenthId = 08,
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
                        SpotLenthId = 07,
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
                        SpotLenthId = 06,
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
                  SelectedDetailsId=1,
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
                  SpotExceptionsRecommendedPlanId=1,
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
                  SpotLenthId= 12,
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
                  SpotLenthId= 11,
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
                  SpotLenthId= 12,
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
                  SpotLenthId= 11,
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
                  SpotLenthId= 10,
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
                  SpotLenthId= 10,
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
                  SpotLenthId= 09,
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
                  SpotLenthId= 08,
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
                  SpotLenthId= 07,
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
                  SpotLenthId= 06,
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

    }
}
