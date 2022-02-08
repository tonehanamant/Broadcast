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
        /// Gets spot exceptions out of specs details
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecId">The spot exceptions out of spec id</param>
        /// <returns>The spot exceptions out of spec details</returns>
        SpotExceptionsOutOfSpecDetailsResultDto GetSpotExceptionOutofSpecsDetails(int spotExceptionsOutOfSpecId);

        /// <summary>
        /// Gets spot exceptions recommended plans
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlansRequest">The spot exceptions recommended plans request parameters</param>
        /// <returns>The spot exceptions recommended plans</returns>
        List<SpotExceptionsRecommendedPlansResultDto> GetSpotExceptionsRecommendedPlans(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest);

        /// <summary>
        /// Gets spot exceptions recommended plan details
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanId">The spot exceptions recommended plan id</param>
        /// <returns>The spot exceptions recommended plan details</returns>
        SpotExceptionsRecommendedPlanDetailsResultDto GetSpotExceptionsRecommendedPlanDetails(int spotExceptionsRecommendedPlanId);

        /// <summary>
        /// Saves spot exception recommended plan
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanSaveRequest">The spot exceptions recommended plan save request parameters</param>
        /// <param name="userName">The user name</param>
        /// <returns>True if spot exception recommended plan saves successfully otherwise false</returns>
        bool SaveSpotExceptionsRecommendedPlan(SpotExceptionsRecommendedPlanSaveRequestDto spotExceptionsRecommendedPlanSaveRequest, string userName);

        /// <summary>
        /// Save SpotExceptionsOutOfSpecs Decisions data
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecDecisionsPostsRequest">The SpotExceptionsOutOfSpecDecisions Request</param>
        /// <param name="userName">User Name</param>
        /// <returns>true or false</returns>
        bool SaveSpotExceptionsOutOfSpecsDecisions(SpotExceptionsOutOfSpecDecisionsPostsRequestDto spotExceptionsOutOfSpecDecisionsPostsRequest, string userName);

        /// <summary>
        /// Get the result of recommended plans advertiser. 
        /// </summary>       
        /// <param name="spotExceptionsRecommendedPlansAdvertisersRequest">The spot exceptions recommended plans request parameters</param>     
        /// <returns>The list of advertiser name from recommended plans result</returns>
        List<string> GetSpotExceptionsRecommendedPlansAdvertisers(SpotExceptionsRecommendedPlansAdvertisersRequestDto spotExceptionsRecommendedPlansAdvertisersRequest);

        /// <summary>
        /// Get the result of Out of Spec advertiser. 
        /// </summary>       
        /// <param name="spotExceptionsOutofSpecAdvertisersRequest">The spot exceptions recommended plans request parameters</param>     
        /// <returns>The list of advertiser name from  Out of Spec result</returns>
        List<string> GetSpotExceptionsOutofSpecAdvertisers(SpotExceptionsOutofSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest);

        /// <summary>
        /// Gets spot exceptions out of spec reason codes
        /// </summary>
        /// <returns>The spot exceptions out of spec reason codes</returns>
        List<SpotExceptionsOutOfSpecReasonCodeResultDto> GetSpotExceptionsOutOfSpecReasonCodes();

        /// <summary>
        /// Gets spot exceptions recommended plans stations
        /// </summary>       
        /// <param name="spotExceptionsRecommendedPlansStationRequest">The spot exceptions recommended plans station request parameters</param>     
        /// <returns>The spot exceptions recommended plans stations</returns>
        List<string> GetSpotExceptionsRecommendedPlansStations(SpotExceptionsRecommendedPlansStationRequestDto spotExceptionsRecommendedPlansStationRequest);

        /// <summary>
        /// Gets spot exceptions out of specs stations
        /// </summary>       
        /// <param name="spotExceptionsOutofSpecsStationRequest">The spot exceptions out of specs station request parameters</param>     
        /// <returns>The spot exceptions out of specs stations</returns>
        List<string> GetSpotExceptionsOutofSpecsStations(SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest);

        /// <summary>
        /// Gets The Spot Exception Out Of specs Plans
        /// </summary>
        /// <param name="spotExceptionsOutofSpecsActivePlansRequestDto"></param>
        /// <returns>Returns Spot Exceptions Active and completed Plans</returns>
        SpotExceptionsOutOfSpecPlansResultDto GetSpotExceptionsOutofSpecsPlans(SpotExceptionsOutofSpecsPlansRequestDto spotExceptionsOutofSpecsActivePlansRequestDto);
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
            List<SpotExceptionsOutOfSpecsDto> spotExceptionsOutOfSpecs = _GetSpotExceptionsOutOfSpecsMock();

            var result = _SpotExceptionRepository.AddSpotExceptionData(spotExceptionsRecommendedPlans, spotExceptionsOutOfSpecs);
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
                        EstimateId =191756,
                        IsciName = "AB82TXT2H",
                        RecommendedPlanId = 215,
                        ProgramName = "Q13 news at 10",
                        ProgramAirTime = new DateTime(2021, 10, 4),
                        AdvertiserName="Abbott Labs (Original)",
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
                        IngestedAt=DateTime.Now,
                        SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                        {
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId=215,
                                MetricPercent=20,
                                IsRecommendedPlan=true,
                                SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
                                {
                                    UserName = "MockData",
                                    CreatedAt = DateTime.Now
                                }
                            },
                                new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId=221,
                                MetricPercent=22,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            },
                                new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId=222,
                                MetricPercent=24,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            },
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId=223,
                                MetricPercent=26,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            }
                        }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                        EstimateId =191757,
                        IsciName = "AB82VR58",
                        RecommendedPlanId = 216,
                        ProgramName = "FOX 13 10:00 News",
                        ProgramAirTime = new DateTime(2021, 10, 04),
                        AdvertiserName="Allergan",
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
                        IngestedAt=DateTime.Now,
                        SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                        {
                                new SpotExceptionsRecommendedPlanDetailsDto
                                {
                                  RecommendedPlanId=216,
                                  MetricPercent=45,
                                  IsRecommendedPlan=true,
                                  SpotExceptionsRecommendedPlanDecision = null
                                },
                                 new SpotExceptionsRecommendedPlanDetailsDto
                                {
                                  RecommendedPlanId=224,
                                  MetricPercent=50,
                                  IsRecommendedPlan=false,
                                  SpotExceptionsRecommendedPlanDecision = null
                                }
                        }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    EstimateId =191758,
                    IsciName = "AB44NR58",
                    RecommendedPlanId = 217,
                    ProgramName = "TEN O'CLOCK NEWS",
                    ProgramAirTime = new DateTime(2021, 10, 10),
                    AdvertiserName="Ally Bank",
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
                    IngestedAt=DateTime.Now,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 217,
                            MetricPercent=28,
                            IsRecommendedPlan=true,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 225,
                            MetricPercent=38,
                            IsRecommendedPlan=false,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                        EstimateId =191759,
                        IsciName = "AB21QR58",
                        RecommendedPlanId = 218,
                        ProgramName = "Product1",
                        ProgramAirTime = new DateTime(2021, 09, 06),
                        AdvertiserName="Ally Bank",
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
                        IngestedAt=DateTime.Now,
                        SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                        {
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 218,
                                MetricPercent=56,
                                IsRecommendedPlan=true,
                                SpotExceptionsRecommendedPlanDecision = null
                            },
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 226,
                                MetricPercent=60,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            }
                        }
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        EstimateId =191760,
                        IsciName = "AB44NR58",
                        RecommendedPlanId = 219,
                        ProgramName = "TProduct2",
                        ProgramAirTime = new DateTime(2021, 09, 13),
                        AdvertiserName="Amgen",
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
                        IngestedAt=DateTime.Now,
                        SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                        {
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 219,
                                MetricPercent=76,
                                IsRecommendedPlan=true,
                                SpotExceptionsRecommendedPlanDecision = null
                            },
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 227,
                                MetricPercent=80,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            }
                        }
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        EstimateId =191761,
                        IsciName = "AB33PR58",
                        RecommendedPlanId = 220,
                        ProgramName = "TEN O'CLOCK NEWS",
                        ProgramAirTime = new DateTime(2021, 09, 20),
                        AdvertiserName="Boston Scientific",
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
                        IngestedAt=DateTime.Now,
                        SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                        {
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 220,
                                MetricPercent=87,
                                IsRecommendedPlan=true,
                                SpotExceptionsRecommendedPlanDecision = null
                            },
                                new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 228,
                                MetricPercent=70,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            }
                        }
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        EstimateId =191762,
                        IsciName = "AB79PR58",
                        RecommendedPlanId = 221,
                        ProgramName = "Product4",
                        ProgramAirTime = new DateTime(2021, 09, 27),
                        AdvertiserName="Chattem",
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
                        IngestedAt=DateTime.Now,
                        SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                        {
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 221,
                                MetricPercent=82,
                                IsRecommendedPlan=true,
                                SpotExceptionsRecommendedPlanDecision = null
                            },
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 229,
                                MetricPercent=50,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            }
                        }
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        EstimateId =191763,
                        IsciName = "AB81GR58",
                        RecommendedPlanId = 222,
                        ProgramName = "Product3",
                        ProgramAirTime = new DateTime(2021, 09, 27),
                        AdvertiserName=null,
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
                        IngestedAt=DateTime.Now,
                        SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                        {
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 222,
                                MetricPercent=75,
                                IsRecommendedPlan=true,
                                SpotExceptionsRecommendedPlanDecision = null
                            },
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 230,
                                MetricPercent=79,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            }
                        }
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        EstimateId =191764,
                        IsciName = "AB87GR58",
                        RecommendedPlanId = 223,
                        ProgramName = "Product6",
                        ProgramAirTime = new DateTime(2021, 09, 13),
                        AdvertiserName=null,
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
                        IngestedAt=DateTime.Now,
                        SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                        {
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 223,
                                MetricPercent=90,
                                IsRecommendedPlan=true,
                                SpotExceptionsRecommendedPlanDecision = null
                            },
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 231,
                                MetricPercent=92,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            },
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 232,
                                MetricPercent=94,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            },
                            new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 233,
                                MetricPercent=96,
                                IsRecommendedPlan=false,
                                SpotExceptionsRecommendedPlanDecision = null
                            }
                        }
                    },
                    new SpotExceptionsRecommendedPlansDto
                    {
                        EstimateId =191765,
                        IsciName = "AB83PR58",
                        RecommendedPlanId = 224,
                        ProgramName = "Product8",
                        ProgramAirTime = new DateTime(2021, 09, 06),
                        AdvertiserName=null,
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
                        IngestedAt=DateTime.Now,
                        SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                        {
                                new SpotExceptionsRecommendedPlanDetailsDto
                            {
                                RecommendedPlanId = 224,
                                MetricPercent=96,
                                IsRecommendedPlan=true,
                                SpotExceptionsRecommendedPlanDecision = null
                            }
                        }
                    }
                };

            return spotExceptionsRecommendedPlans;
        }

        private List<SpotExceptionsOutOfSpecsDto> _GetSpotExceptionsOutOfSpecsMock()
        {
            var spotExceptionsOutOfSpecs = new List<SpotExceptionsOutOfSpecsDto>
            {
                new SpotExceptionsOutOfSpecsDto
                {
                  ReasonCodeMessage="",
                  EstimateId= 191756,
                  IsciName="AB82TXT2H",
                  RecommendedPlanId= 215,
                  ProgramName="Q13 news at 10",
                  AdvertiserName="Chattem",
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
                  IngestedAt=DateTime.Now,
                  Impressions=10000,
                  SpotExceptionsOutOfSpecDecision = new SpotExceptionsOutOfSpecDecisionsDto
                    {
                      AcceptedAsInSpec=true,
                      DecisionNotes="",
                      UserName = "MockData",
                      CreatedAt = DateTime.Now
                    },
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
                  ReasonCodeMessage="",
                  EstimateId= 191757,
                  IsciName="AB82VR58",
                  RecommendedPlanId= 216,
                  ProgramName="FOX 13 10:00 News",
                  AdvertiserName="Chattem",
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
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=20000,
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
                  ReasonCodeMessage="",
                  EstimateId= 191758,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 217,
                  ProgramName="TEN O'CLOCK NEWS",
                  AdvertiserName="Costant Contact",
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
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=30000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  ReasonCodeMessage="",
                  EstimateId= 191759,
                  IsciName="AB21QR58",
                  RecommendedPlanId= 218,
                  ProgramName="Product1",
                  AdvertiserName="Curiosity Stream",
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
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=40000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 5,
                        ReasonCode = 4,
                        Reason = "program content restriction",
                        Label = "Program"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  ReasonCodeMessage="",
                  EstimateId= 191760,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 219,
                  ProgramName="TProduct2",
                  AdvertiserName="Dupixent",
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
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=50000,
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
                  ReasonCodeMessage="",
                  EstimateId= 191761,
                  IsciName="AB33PR58",
                  RecommendedPlanId= 220,
                  ProgramName="TEN O'CLOCK NEWS",
                  AdvertiserName="Eli Lilly - Trulicity (EN)",
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
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=60000,
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
                  ReasonCodeMessage="",
                  EstimateId= 191762,
                  IsciName="AB79PR58",
                  RecommendedPlanId= 221,
                  ProgramName="Product4",
                  AdvertiserName="Eli Lilly - Trulicity (EN)",
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
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=70000,
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
                  ReasonCodeMessage="",
                  EstimateId= 191763,
                  IsciName="AB81GR58",
                  RecommendedPlanId= 222,
                  ProgramName="Product3",
                  AdvertiserName="GAP (EM)",
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
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=80000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 4,
                        ReasonCode = 3,
                        Reason = "affiliate content restriction",
                        Label = "Affiliate"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  ReasonCodeMessage="",
                  EstimateId= 191764,
                  IsciName="AB87GR58",
                  RecommendedPlanId= 223,
                  ProgramName="Product6",
                  AdvertiserName="Hershey - DA",
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
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=90000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 5,
                        ReasonCode = 4,
                        Reason = "program content restriction",
                        Label = "Program"
                    }
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  ReasonCodeMessage="",
                  EstimateId= 191765,
                  IsciName="AB83PR58",
                  RecommendedPlanId= 224,
                  ProgramName="Product8",
                  AdvertiserName=null,
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
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=100000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 3,
                        ReasonCode = 2,
                        Reason = "genre content restriction",
                        Label = "Genre"
                    }
                }
            };

            return spotExceptionsOutOfSpecs;
        }

        /// <inheritdoc />
        public List<SpotExceptionsOutOfSpecPostsResultDto> GetSpotExceptionsOutOfSpecsPosts(SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest)
        {
            var spotExceptionsOutOfSpecPostsResults = new List<SpotExceptionsOutOfSpecPostsResultDto>();
            const string programAirDateFormat = "MM/dd/yyyy";
            const string programAirTimeFormat = "hh:mm:ss tt";
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";

            var spotExceptionsoutOfSpecsPosts = _SpotExceptionRepository.GetSpotExceptionsOutOfSpecPosts(spotExceptionsOutOfSpecPostsRequest.WeekStartDate, spotExceptionsOutOfSpecPostsRequest.WeekEndDate);
            if (spotExceptionsoutOfSpecsPosts?.Any() ?? false)
            {
                spotExceptionsOutOfSpecPostsResults = spotExceptionsoutOfSpecsPosts.Select(spotExceptionsOutOfSpec =>
                {
                    var spotExceptionsOutOfSpecPostsResult = new SpotExceptionsOutOfSpecPostsResultDto
                    {
                        Id = spotExceptionsOutOfSpec.Id,
                        Status = spotExceptionsOutOfSpec.SpotExceptionsOutOfSpecDecision != null,
                        EstimateId = spotExceptionsOutOfSpec.EstimateId,
                        IsciName = spotExceptionsOutOfSpec.IsciName,
                        RecommendedPlan = spotExceptionsOutOfSpec.RecommendedPlanName,
                        Reason = spotExceptionsOutOfSpec.SpotExceptionsOutOfSpecReasonCode.Label,
                        Station = spotExceptionsOutOfSpec.StationLegacyCallLetters,
                        Market = spotExceptionsOutOfSpec.Market,
                        SpotLengthString = spotExceptionsOutOfSpec.SpotLength != null ? $":{spotExceptionsOutOfSpec.SpotLength.Length}" : null,
                        AudienceName = spotExceptionsOutOfSpec.ProgramAudience?.Name,
                        ProductName = spotExceptionsOutOfSpec.Product,
                        AdvertiserName = spotExceptionsOutOfSpec.AdvertiserName,
                        DaypartCode = spotExceptionsOutOfSpec.ProgramDaypartDetail?.Code,
                        FlightStartDate = spotExceptionsOutOfSpec.FlightStartDate?.ToString(),
                        FlightEndDate = spotExceptionsOutOfSpec.FlightEndDate?.ToString(),
                        FlightDateString = spotExceptionsOutOfSpec.FlightStartDate.HasValue && spotExceptionsOutOfSpec.FlightEndDate.HasValue ? $"{Convert.ToDateTime(spotExceptionsOutOfSpec.FlightStartDate).ToString(flightStartDateFormat)}-{Convert.ToDateTime(spotExceptionsOutOfSpec.FlightEndDate).ToString(flightEndDateFormat)}" : null,
                        ProgramName = spotExceptionsOutOfSpec.ProgramName,
                        ProgramAirDate = spotExceptionsOutOfSpec.ProgramAirTime.ToString(programAirDateFormat),
                        ProgramAirTime = spotExceptionsOutOfSpec.ProgramAirTime.ToString(programAirTimeFormat)
                    };
                    return spotExceptionsOutOfSpecPostsResult;
                }).ToList();
            }
            return spotExceptionsOutOfSpecPostsResults;
        }

        /// <inheritdoc />
        public SpotExceptionsOutOfSpecDetailsResultDto GetSpotExceptionOutofSpecsDetails(int spotExceptionsOutOfSpecId)
        {
            var spotExceptionsOutOfSpecDetail = _SpotExceptionRepository.GetSpotExceptionsOutOfSpecById(spotExceptionsOutOfSpecId);
            if (spotExceptionsOutOfSpecDetail == null)
            {
                return null;
            }

            const string programAirDateFormat = "MM/dd/yyyy";
            const string programAirTimeFormat = "hh:mm:ss tt";
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";

            var spotExceptionsOutOfSpecDetailResult = new SpotExceptionsOutOfSpecDetailsResultDto
            {
                Id = spotExceptionsOutOfSpecDetail.Id,
                Reason = spotExceptionsOutOfSpecDetail.SpotExceptionsOutOfSpecReasonCode.Label,
                DaypartCode = spotExceptionsOutOfSpecDetail.ProgramDaypartDetail?.Code,
                Network = spotExceptionsOutOfSpecDetail.ProgramNetwork,
                AudienceName = spotExceptionsOutOfSpecDetail.ProgramAudience?.Name,
                FlightStartDate = spotExceptionsOutOfSpecDetail.FlightStartDate?.ToString(),
                FlightEndDate = spotExceptionsOutOfSpecDetail.FlightEndDate?.ToString(),
                FlightDateString = spotExceptionsOutOfSpecDetail.FlightStartDate.HasValue && spotExceptionsOutOfSpecDetail.FlightEndDate.HasValue ? $"{Convert.ToDateTime(spotExceptionsOutOfSpecDetail.FlightStartDate).ToString(flightStartDateFormat)}-{Convert.ToDateTime(spotExceptionsOutOfSpecDetail.FlightEndDate).ToString(flightEndDateFormat)}" : null,
                AcceptedAsInSpec = spotExceptionsOutOfSpecDetail.SpotExceptionsOutOfSpecDecision?.AcceptedAsInSpec,
                DecisionNotes = spotExceptionsOutOfSpecDetail.SpotExceptionsOutOfSpecDecision?.DecisionNotes,
                ProgramName = spotExceptionsOutOfSpecDetail.ProgramName,
                ProgramAirDate = spotExceptionsOutOfSpecDetail.ProgramAirTime.ToString(programAirDateFormat),
                ProgramAirTime = spotExceptionsOutOfSpecDetail.ProgramAirTime.ToString(programAirTimeFormat)
            };
            return spotExceptionsOutOfSpecDetailResult;
        }

        /// <inheritdoc />
        public List<string> GetSpotExceptionsOutofSpecAdvertisers(SpotExceptionsOutofSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest)
        {
            SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutOfSpecPostsRequestDto();
            spotExceptionsOutOfSpecPostsRequest.WeekStartDate = spotExceptionsOutofSpecAdvertisersRequest.WeekStartDate;
            spotExceptionsOutOfSpecPostsRequest.WeekEndDate = spotExceptionsOutofSpecAdvertisersRequest.WeekEndDate;

            var advertisers = GetSpotExceptionsOutOfSpecsPosts(spotExceptionsOutOfSpecPostsRequest).Select(s => s.AdvertiserName ?? "Unknown").Distinct().ToList();

            return advertisers;
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
                        AdvertiserName = spotExceptionsRecommendedPlan.AdvertiserName,
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
        public SpotExceptionsRecommendedPlanDetailsResultDto GetSpotExceptionsRecommendedPlanDetails(int spotExceptionsRecommendedPlanId)
        {
            var spotExceptionsRecommendedPlan = _SpotExceptionRepository.GetSpotExceptionsRecommendedPlanById(spotExceptionsRecommendedPlanId);
            if (spotExceptionsRecommendedPlan == null)
            {
                return null;
            }

            const string programAirDateFormat = "MM/dd/yyyy";
            const string programAirTimeFormat = "hh:mm:ss tt";
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";

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
                FlightDateString = spotExceptionsRecommendedPlan.FlightStartDate.HasValue && spotExceptionsRecommendedPlan.FlightEndDate.HasValue ? $"{Convert.ToDateTime(spotExceptionsRecommendedPlan.FlightStartDate).ToString(flightStartDateFormat)}-{Convert.ToDateTime(spotExceptionsRecommendedPlan.FlightEndDate).ToString(flightEndDateFormat)}" : null,
                ProgramName = spotExceptionsRecommendedPlan.ProgramName,
                ProgramAirDate = spotExceptionsRecommendedPlan.ProgramAirTime.ToString(programAirDateFormat),
                ProgramAirTime = spotExceptionsRecommendedPlan.ProgramAirTime.ToString(programAirTimeFormat),
                Plans = spotExceptionsRecommendedPlan.SpotExceptionsRecommendedPlanDetails.Select(spotExceptionsRecommendedPlanDetail => new RecommendedPlanDetailResultDto
                {
                    Id = spotExceptionsRecommendedPlanDetail.Id,
                    Name = spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.Name,
                    SpotLengthString = string.Join(", ", spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.SpotLengths.Select(spotLength => $":{spotLength.Length}")),
                    FlightStartDate = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightStartDate}",
                    FlightEndDate = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightEndDate}",
                    FlightDateString = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightStartDate.ToString(flightStartDateFormat)}-{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightEndDate.ToString(flightEndDateFormat)}",
                    IsRecommendedPlan = spotExceptionsRecommendedPlanDetail.IsRecommendedPlan,
                    IsSelected = spotExceptionsRecommendedPlanDetail.SpotExceptionsRecommendedPlanDecision != null
                }).ToList()
            };
            return spotExceptionsRecommendedPlanDetailsResult;
        }

        /// <inheritdoc />
        public List<string> GetSpotExceptionsRecommendedPlansAdvertisers(SpotExceptionsRecommendedPlansAdvertisersRequestDto spotExceptionsRecommendedPlansAdvertisersRequest)
        {
            SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto();
            spotExceptionsRecommendedPlansRequest.WeekStartDate = spotExceptionsRecommendedPlansAdvertisersRequest.WeekStartDate;
            spotExceptionsRecommendedPlansRequest.WeekEndDate = spotExceptionsRecommendedPlansAdvertisersRequest.WeekEndDate;
            var advertisers = GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest).Select(s => s.AdvertiserName ?? "Unknown").Distinct().ToList();

            return advertisers;
        }
        /// <inheritdoc />
        public bool SaveSpotExceptionsRecommendedPlan(SpotExceptionsRecommendedPlanSaveRequestDto spotExceptionsRecommendedPlanSaveRequest, string userName)
        {
            var spotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
            {
                SpotExceptionsRecommendedPlanId = spotExceptionsRecommendedPlanSaveRequest.Id,
                SpotExceptionsRecommendedPlanDetailId = spotExceptionsRecommendedPlanSaveRequest.SelectedPlanId,
                UserName = userName,
                CreatedAt = _DateTimeEngine.GetCurrentMoment()
            };

            bool isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated = false;
            bool isSpotExceptionsRecommendedPlanDecisionSaved = _SpotExceptionRepository.SaveSpotExceptionsRecommendedPlanDecision(spotExceptionsRecommendedPlanDecision);
            if (isSpotExceptionsRecommendedPlanDecisionSaved)
            {
                var spotExceptionsRecommendedPlanDetail = new SpotExceptionsRecommendedPlanDetailsDto
                {
                    Id = spotExceptionsRecommendedPlanSaveRequest.SelectedPlanId,
                    SpotExceptionsRecommendedPlanId = spotExceptionsRecommendedPlanSaveRequest.Id
                };
                isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated = _SpotExceptionRepository.UpdateRecommendedPlanOfSpotExceptionsRecommendedPlan(spotExceptionsRecommendedPlanDetail);
            }
            return isSpotExceptionsRecommendedPlanDecisionSaved && isRecommendedPlanOfSpotExceptionsRecommendedPlanUpdated;
        }

        public bool SaveSpotExceptionsOutOfSpecsDecisions(SpotExceptionsOutOfSpecDecisionsPostsRequestDto spotExceptionsOutOfSpecDecisionsPostsRequest, string userName)
        {
            var createdAt = _DateTimeEngine.GetCurrentMoment();

            var isSpotExceptionsOutOfSpecDecision = _SpotExceptionRepository.SaveSpotExceptionsOutOfSpecsDecisions(spotExceptionsOutOfSpecDecisionsPostsRequest, userName, createdAt);

            return isSpotExceptionsOutOfSpecDecision;
        }

        /// <inheritdoc />
        public List<SpotExceptionsOutOfSpecReasonCodeResultDto> GetSpotExceptionsOutOfSpecReasonCodes()
        {
            var spotExceptionsOutOfSpecReasonCodes = _SpotExceptionRepository.GetSpotExceptionsOutOfSpecReasonCodes();

            var spotExceptionsOutOfSpecReasonCodeResults = spotExceptionsOutOfSpecReasonCodes.Select(spotExceptionsOutOfSpecReasonCode => new SpotExceptionsOutOfSpecReasonCodeResultDto
            {
                Id = spotExceptionsOutOfSpecReasonCode.Id,
                ReasonCode = spotExceptionsOutOfSpecReasonCode.ReasonCode,
                Description = spotExceptionsOutOfSpecReasonCode.Reason,
                Label = spotExceptionsOutOfSpecReasonCode.Label
            }).ToList();
            return spotExceptionsOutOfSpecReasonCodeResults;
        }

        /// <inheritdoc />
        public List<string> GetSpotExceptionsRecommendedPlansStations(SpotExceptionsRecommendedPlansStationRequestDto spotExceptionsRecommendedPlansStationRequest)
        {
            var spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto()
            {
                WeekStartDate = spotExceptionsRecommendedPlansStationRequest.WeekStartDate,
                WeekEndDate = spotExceptionsRecommendedPlansStationRequest.WeekEndDate
            };
            var spotExceptionsRecommendedPlansResults = GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest);
            if (spotExceptionsRecommendedPlansResults == null)
            {
                return null;
            }

            var stations = spotExceptionsRecommendedPlansResults.Select(spotExceptionsRecommendedPlansResult => spotExceptionsRecommendedPlansResult.Station ?? "Unknown").Distinct().OrderBy(station => station).ToList();
            return stations;
        }

        /// <inheritdoc />
        public List<string> GetSpotExceptionsOutofSpecsStations(SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest)
        {
            var spotExceptionsOutOfSpecPostsRequest = new SpotExceptionsOutOfSpecPostsRequestDto()
            {
                WeekStartDate = spotExceptionsOutofSpecsStationRequest.WeekStartDate,
                WeekEndDate = spotExceptionsOutofSpecsStationRequest.WeekEndDate
            };
            var spotExceptionsOutOfSpecPostsResults = GetSpotExceptionsOutOfSpecsPosts(spotExceptionsOutOfSpecPostsRequest);
            if (spotExceptionsOutOfSpecPostsResults == null)
            {
                return null;
            }

            var stations = spotExceptionsOutOfSpecPostsResults.Select(spotExceptionsOutOfSpecPostsResult => spotExceptionsOutOfSpecPostsResult.Station ?? "Unknown").Distinct().OrderBy(station => station).ToList();
            return stations;
        }

        public SpotExceptionsOutOfSpecPlansResultDto GetSpotExceptionsOutofSpecsPlans(SpotExceptionsOutofSpecsPlansRequestDto spotExceptionsOutofSpecsPlansRequestDto)
        {
            var spotExceptionsOutOfSpecPlansResults = new SpotExceptionsOutOfSpecPlansResultDto();
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";
            List<SpotExceptionsOutOfSpecsDto> activePlans = null;
            List<SpotExceptionsOutOfSpecsDto> completedPlans = null;

            var spotExceptionsoutOfSpecsPlans = _SpotExceptionRepository.GetSpotExceptionsOutOfSpecPosts(spotExceptionsOutofSpecsPlansRequestDto.WeekStartDate, spotExceptionsOutofSpecsPlansRequestDto.WeekEndDate);
            if (spotExceptionsoutOfSpecsPlans?.Any() ?? false)
            {
                activePlans = spotExceptionsoutOfSpecsPlans.Where(spotExceptionDecisionPlans => spotExceptionDecisionPlans.SpotExceptionsOutOfSpecDecision == null).ToList();
                completedPlans = spotExceptionsoutOfSpecsPlans.Where(spotExceptionDecisionPlans => spotExceptionDecisionPlans.SpotExceptionsOutOfSpecDecision != null).ToList();

                spotExceptionsOutOfSpecPlansResults.Active = activePlans.GroupBy(activePlan => new { activePlan.RecommendedPlanId })
                .Select(activePlan =>
                {
                    var planDetails = activePlan.First();
                    return new SpotExceptionsOutOfSpecToDoPlansDto
                    {
                        PlanId = planDetails.PlanId,
                        AdvertiserName = planDetails.AdvertiserName,
                        PlanName = planDetails.RecommendedPlanName,
                        AffectedSpotsCount = activePlan.Count(),
                        Impressions = planDetails.Impressions / 1000,
                        SyncedTimestamp = null,
                        SpotLengthString = planDetails.SpotLength != null ? $":{planDetails.SpotLength.Length}" : null,
                        AudienceName = planDetails.ProgramAudience?.Name,
                        FlightString = planDetails.FlightStartDate.HasValue && planDetails.FlightEndDate.HasValue ? $"{Convert.ToDateTime(planDetails.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(planDetails.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(planDetails.FlightStartDate), Convert.ToDateTime(planDetails.FlightEndDate)).ToString() + " " + "Weeks"})" : null,
                    };
                }).ToList();

                spotExceptionsOutOfSpecPlansResults.Completed = completedPlans.GroupBy(completedPlan => new { completedPlan.RecommendedPlanId })
                .Select(completedPlan =>
                {
                    var planDetails = completedPlan.First();
                    return new SpotExceptionsOutOfSpecCompletedPlansDto
                    {
                        PlanId = planDetails.PlanId,
                        AdvertiserName = planDetails.AdvertiserName,
                        PlanName = planDetails.RecommendedPlanName,
                        AffectedSpotsCount = completedPlan.Count(),
                        Impressions = planDetails.Impressions / 1000,
                        SyncedTimestamp = null,
                        SpotLengthString = planDetails.SpotLength != null ? $":{planDetails.SpotLength.Length}" : null,
                        AudienceName = planDetails.ProgramAudience?.Name,
                        FlightString = planDetails.FlightStartDate.HasValue && planDetails.FlightEndDate.HasValue ? $"{Convert.ToDateTime(planDetails.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(planDetails.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(planDetails.FlightStartDate), Convert.ToDateTime(planDetails.FlightEndDate)).ToString() + " " + "Weeks"})" : null,
                    };
                }).ToList();
            }
            return spotExceptionsOutOfSpecPlansResults;
        }

        private int _GetTotalNumberOfWeeks(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new Exception("EndDate should be greater than StartDate");
            }
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1);
            var dateDifference = endDate - startDate;
            var totalDays = dateDifference.TotalDays;
            int numberOfWeeks = Convert.ToInt32(totalDays / 7);
            var reminder = totalDays % 7;
            numberOfWeeks = reminder > 0 ? numberOfWeeks + 1 : numberOfWeeks;
            return numberOfWeeks;
        }
    }
}
