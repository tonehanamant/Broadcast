using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.Program;
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
        /// <param name="isIntegrationTestDatabase">True when we are running integration tests against an integration tests database.</param>
        bool AddSpotExceptionData(bool isIntegrationTestDatabase = false);

        /// <summary>
        /// Clear mocked data from spot exceptions tables.
        /// </summary>   
        bool ClearSpotExceptionMockData();

        /// <summary>
        /// Clear all data from spot exceptions tables.
        /// </summary>   
        bool ClearSpotExceptionAllData();

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

        /// <summary>
        /// Gets the spot exception out of spec spots
        /// </summary>
        /// <param name="spotExceptionsOutofSpecSpotsRequest">Spot Exception out of spec spots</param>
        /// <returns>it gave the response for active, queued and synced plans</returns>
        SpotExceptionsOutOfSpecPlanSpotsResultDto GetSpotExceptionsOutofSpecSpots(SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutofSpecSpotsRequest);

        /// <summary>
        /// Gets the markets of active spot exception out of spec spots
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest">The spot exceptions out of spec spots request parameters</param>
        /// <returns>The markets of active spot exception out of spec spots</returns>
        List<string> GetSpotExceptionsOutOfSpecMarkets(SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);

        /// <summary>
        /// Gets the NoPlan And NoReelRoster spot exceptions
        /// </summary>
        /// <param name="spotExceptionOutOfSpecUnpostedRequest">The spot exceptions out of spec Unposted plans</param>
        /// <returns>The NoPlan And NoReelRoster spot exceptions</returns>
        SpotExceptionOutOfSpecUnpostedResultDto GetSpotExceptionsUnposted(SpotExceptionOutOfSpecUnpostedRequestDto spotExceptionOutOfSpecUnpostedRequest);

        /// <summary>
        /// Save SpotExceptionsOutOfSpecs Decisions data 
        /// </summary>
        /// <param name="spotExceptionOutOfSpecSaveDecisionsPlansRequest">The SpotExceptionsOutOfSpecDecisions Request </param>
        /// <param name="userName">user name</param>
        /// <returns>true or false</returns>
        bool SaveOutofSpecDecisionsPlans(SpotExceptionSaveDecisionsPlansRequestDto spotExceptionOutOfSpecSaveDecisionsPlansRequest, string userName);

        /// <summary>
        /// Sync Decision Data Assigning the SyncedAt  and SyncedBy Indicators.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">User Name</param>
        /// <returns>Return true when decision data is synced or else returning false</returns>
        bool TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest);

        /// <summary>
        /// Get the Queued Decision Count
        /// </summary>
        /// <returns>Count of Decision Data</returns>
        int GetQueuedDecisionCount();

        /// <summary>
        /// Gets the Spot exceptions recommanded plans in active and completed mode.
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlansRequest">Week start and End date</param>
        /// <returns>Return list of Active and completed Plans</returns>
        SpotExceptionsRecommendedPlansResultsDto GetRecommendedPlans(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest);

        /// <summary>
        /// Gets all genres filter by source.
        /// </summary>
        /// <param name="genre">genre name</param>
        /// <returns>List of genres</returns>
        List<SpotExceptionsOutOfSpecGenreDto> GetSpotExceptionsOutOfSpecGenres(string genre);

        /// <summary>
        /// Get all the programs based on program search query.
        /// </summary>       
        /// <param name="programNameQuery">input program search query</param>
        /// <param name="userName">The user name</param>
        /// <returns>return all the programs based on program search query</returns>
        List<SpotExceptionsOutOfSpecProgramsDto> GetSpotExceptionsOutOfSpecPrograms(string programNameQuery, string userName);

        /// <summary>
        /// Gets the Recommended Plan Spots
        /// </summary>
        /// <param name="recomendedPlansRequest">Plan Id, week start date and End date</param>
        /// <returns>Returns the Recommended Plan Spots(Active,Synced,Queued)</returns>
        SpotExceptionsRecommendedPlanSpotsResultDto GetSpotExceptionsRecommendedPlanSpots(RecomendedPlansRequestDto recomendedPlansRequest);

        /// <summary>
        /// Get the Filters for Recommended Plans
        /// </summary>
        /// <param name="recomendedPlansRequest">PlanId, weekStartDate and weekEndDate</param>
        /// <returns>Returns Filters string array</returns>
        RecommendedPlanFiltersResultDto GetRecommendedPlansFilters(RecomendedPlansRequestDto recomendedPlansRequest);

        /// <summary>
        /// Get the Inventory Sources of Out of Spec
        /// </summary>
        /// <param name="spotExceptionsOutofSpecSpotsRequest">Plan Id, WeekStartDate And WeekEndDate</param>
        /// <returns>Returns list of Inventory Sources</returns>
        List<string> GetOutOfSpecInventorySources(SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutofSpecSpotsRequest);
    }

    public class SpotExceptionService : BroadcastBaseClass, ISpotExceptionService
    {
        private readonly ISpotExceptionRepository _SpotExceptionRepository;
        private readonly IPlanRepository _PlanRepository;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IAabEngine _AabEngine;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly IProgramService _ProgramService;
        public SpotExceptionService(
            IDataRepositoryFactory dataRepositoryFactory,
            IAabEngine aabEngine,
            IProgramService programService,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper,
            IDateTimeEngine dateTimeEngine)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionRepository = dataRepositoryFactory.GetDataRepository<ISpotExceptionRepository>();
            _DateTimeEngine = dateTimeEngine;
            _AabEngine = aabEngine;
            _PlanRepository = dataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _SpotLengthRepository = dataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _FeatureToggleHelper = featureToggleHelper;
            _ProgramService = programService;
        }

        public bool AddSpotExceptionData(bool isIntegrationTestDatabase = false)
        {
            List<SpotExceptionsRecommendedPlansDto> spotExceptionsRecommendedPlans = _GetSpotExceptionsRecommendedPlansMock();
            List<SpotExceptionsOutOfSpecsDto> spotExceptionsOutOfSpecs = _GetSpotExceptionsOutOfSpecsMock();

            spotExceptionsRecommendedPlans.ForEach(s =>
            {
                s.CreatedAt = DateTime.Now;
                s.CreatedBy = "Mock Data";
                s.ModifiedAt = DateTime.Now;
                s.ModifiedBy = "Mock Date";
            });

            spotExceptionsOutOfSpecs.ForEach(s =>
            {
                s.CreatedAt = DateTime.Now;
                s.CreatedBy = "Mock Data";
                s.ModifiedAt = DateTime.Now;
                s.ModifiedBy = "Mock Date";
            });

            if (isIntegrationTestDatabase)
            {
                // align this mock data with existing plan data within the integration tests databases                
                spotExceptionsRecommendedPlans.SelectMany(s => s.SpotExceptionsRecommendedPlanDetails).ToList().ForEach(s => s.RecommendedPlanId = 524);
                spotExceptionsOutOfSpecs.ForEach(s =>
                {
                    s.RecommendedPlanId = 524;
                    s.GenreName = "Comedy";
                });
            }

            var result = _SpotExceptionRepository.AddSpotExceptionData(spotExceptionsRecommendedPlans, spotExceptionsOutOfSpecs);
            return result;
        }

        public bool ClearSpotExceptionMockData()
        {
            var result = _SpotExceptionRepository.ClearSpotExceptionMockData();
            return result;
        }

        public bool ClearSpotExceptionAllData()
        {
            var result = _SpotExceptionRepository.ClearSpotExceptionAllData();
            return result;
        }

        private List<SpotExceptionsRecommendedPlansDto> _GetSpotExceptionsRecommendedPlansMock()
        {
            var spotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlansDto>
            {
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=+",
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
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
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
                            PlanSpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                            PlanExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,

                            SpotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
                            {
                                UserName = "MockData",
                                CreatedAt = DateTime.Now,
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
                            PlanSpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                            PlanExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA5MTAwMQ=+",
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
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
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
                            PlanSpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                            PlanExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
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
                            PlanSpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                            PlanExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDI3MjQ2NQ=+",
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
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
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
                            PlanSpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                            PlanExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
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
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDIwMjA4Nw=+",
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
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
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
                            PlanSpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                            PlanExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
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
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDIwODcxNw=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6718,
                    InventorySource = "Ference POD - Scripps",
                    HouseIsci = "718MAY2918H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 04, 10, 09, 45, 23),
                    StationLegacyCallLetters = "NRIS",
                    Affiliate = "CW",
                    MarketCode = 200,
                    MarketRank = 130,
                    ProgramName = "The Steve Wilkos Show",
                    ProgramGenre = "REALITY TALK",
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 2306,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 0,
                            DeliveredImpressions = 0,
                            IsRecommendedPlan=true,
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
                            PlanSpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                            PlanExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 2305,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 0,
                            DeliveredImpressions = 1,
                            IsRecommendedPlan=true,
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
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDIyMzAzMw=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 2009,
                    InventorySource = "Business First AM",
                    HouseIsci = "009UPX0030H",
                    ClientIsci = "DUPX0030000H",
                    SpotLengthId = 2,
                    ProgramAirTime = new DateTime(2022, 04, 10, 11, 44, 35),
                    StationLegacyCallLetters = "WMNN",
                    Affiliate = "IND",
                    MarketCode = 140,
                    MarketRank = 118,
                    ProgramName = "Business First AM",
                    ProgramGenre = "INFORMATIONAL/NEWS",
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 323,
                            ExecutionTraceId = 2384,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 0,
                            DeliveredImpressions = 1,
                            IsRecommendedPlan=true,
                            PlanClearancePercentage = null,
                            DaypartCode = "DAY",
                            StartTime = 36000,
                            EndTime = 57599,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            PlanSpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                            PlanExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 322,
                            ExecutionTraceId = 2385,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 0,
                            DeliveredImpressions = 0,
                            IsRecommendedPlan=true,
                            PlanClearancePercentage = null,
                            DaypartCode = "MDN",
                            StartTime = 39600,
                            EndTime = 46799,
                            Monday = 1,
                            Tuesday = 1,
                            Wednesday = 1,
                            Thursday = 1,
                            Friday = 1,
                            Saturday = 1,
                            Sunday = 1,
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            SpotExceptionsRecommendedPlanDecision = null
                        }
                    }
                },
                new SpotExceptionsRecommendedPlansDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDM2NTgwMw=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6288,
                    InventorySource = "Sinclair Corp - Syn EM 6-10a",
                    HouseIsci = "288R2Y1F81H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 04, 10, 09, 14, 45),
                    StationLegacyCallLetters = "NVCW",
                    Affiliate = "IND",
                    MarketCode = 439,
                    MarketRank = 40,
                    ProgramName = "25 Words or Less",
                    ProgramGenre = "GAME SHOW",
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    SpotExceptionsRecommendedPlanDetails=new List<SpotExceptionsRecommendedPlanDetailsDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 2553,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 0,
                            DeliveredImpressions = 0,
                            IsRecommendedPlan=true,
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
                            PlanSpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=F",
                            PlanExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            SpotExceptionsRecommendedPlanDecision = null
                        },
                        new SpotExceptionsRecommendedPlanDetailsDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 2552,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 0,
                            DeliveredImpressions = 1,
                            IsRecommendedPlan=true,
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
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
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
                  SpotUniqueHashExternal = "TE9DQUwtMTA1ODgzNzk3Nw==",
                  HouseIsci = "612NM15082H",
                  ReasonCodeMessage="",
                  EstimateId= 191756,
                  IsciName="AB82TXT2H",
                  RecommendedPlanId= 215,
                  ProgramName="Q13 news at 10",
                  StationLegacyCallLetters="KOB",
                  SpotLengthId= 12,
                  AudienceId= 431,
                  DaypartCode="EMN",
                  GenreName="Documentary",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 04, 10),
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
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "sIncorrect Time",
                        Label = "Daypart"
                    },
                  InventorySourceName = "Open Market",
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1ODgzODk5MA==",
                  HouseIsci = "613NM15082H",
                  ReasonCodeMessage="",
                  EstimateId= 191757,
                  IsciName="AB82VR58",
                  RecommendedPlanId= 216,
                  ProgramName="FOX 13 10:00 News",
                  StationLegacyCallLetters="KSTP",
                  SpotLengthId= 11,
                  AudienceId= 430,
                  Product="Spotify",
                  DaypartCode="TDNS",
                  GenreName="Comedy",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 04, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=20000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 8,
                        ReasonCode = 10,
                        Reason = "Incorrect Genre",
                        Label = "Genre"
                    },
                  InventorySourceName = "TTWN",
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDMwOA==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191758,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 217,
                  ProgramName="TEN O'CLOCK NEWS",
                  StationLegacyCallLetters="KHGI",
                  SpotLengthId= 12,
                  AudienceId= 430,
                  DaypartCode="TDNS",
                  GenreName="Comedy",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 04, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=30000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 9,
                        ReasonCode = 11,
                        Reason = "Incorrect Affiliate",
                        Label = "Affiliate"
                    },
                  InventorySourceName = "TTWN",
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDYzNg==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191759,
                  IsciName="AB21QR58",
                  RecommendedPlanId= 218,
                  ProgramName="Product1",
                  StationLegacyCallLetters="KWCH",
                  SpotLengthId= 11,
                  AudienceId= 430,
                  DaypartCode="TDNS",
                  GenreName="Comedy",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 04, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=40000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 10,
                        ReasonCode = 12,
                        Reason = "Incorrect Program",
                        Label = "Program"
                    },
                  InventorySourceName = "CNN",
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0NDAxOA==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191760,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 219,
                  ProgramName="TProduct2",
                  StationLegacyCallLetters="WDAY",
                  SpotLengthId= 10,
                  AudienceId= 429,
                  DaypartCode="ROSP",
                  GenreName="Crime",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2020, 10, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=50000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 8,
                        ReasonCode = 10,
                        Reason = "Incorrect Genre",
                        Label = "Genre"
                    },
                  InventorySourceName = "Sinclair"
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTA5ODgwOQ==",
                  HouseIsci = "613NM13290H",
                  ReasonCodeMessage="",
                  EstimateId= 191761,
                  IsciName="AB33PR58",
                  RecommendedPlanId= 220,
                  ProgramName="TEN O'CLOCK NEWS",
                  StationLegacyCallLetters="KPNX",
                  SpotLengthId= 10,
                  AudienceId= 428,
                  DaypartCode="EM",
                  GenreName="Drama",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 04, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=60000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "Incorrect Time",
                        Label = "Daypart"
                    },
                  InventorySourceName = "LilaMax"
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191762,
                  IsciName="AB79PR58",
                  RecommendedPlanId= 221,
                  ProgramName="Product4",
                  StationLegacyCallLetters="KELO",
                  SpotLengthId= 09,
                  AudienceId= 427,
                  DaypartCode="EM",
                  GenreName="Drama",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 04, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=70000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 8,
                        ReasonCode = 10,
                        Reason = "Incorrect Genre",
                        Label = "Genre"
                    },
                  InventorySourceName = "MLB"
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMjQ4OQ==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191763,
                  IsciName="AB81GR58",
                  RecommendedPlanId= 222,
                  ProgramName="Product3",
                  StationLegacyCallLetters="KXMC",
                  SpotLengthId= 08,
                  AudienceId= 426,
                  DaypartCode="EM",
                  GenreName="Drama",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 04, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=80000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 9,
                        ReasonCode = 11,
                        Reason = "Incorrect Affiliate",
                        Label = "Affiliate"
                    },
                  InventorySourceName = "Ference Media"
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxNTkzNQ==",
                  HouseIsci = "OMGN1016000H",
                  ReasonCodeMessage="",
                  EstimateId= 191764,
                  IsciName="AB87GR58",
                  RecommendedPlanId= 223,
                  ProgramName="Product6",
                  StationLegacyCallLetters="WTTV",
                  SpotLengthId= 07,
                  AudienceId= 425,
                  DaypartCode="EM",
                  GenreName="Entertainment",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 04, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=90000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 10,
                        ReasonCode = 12,
                        Reason = "Incorrect Program",
                        Label = "Program"
                    },
                  InventorySourceName = "20th Century Fox (Twentieth Century)",
                },
                new SpotExceptionsOutOfSpecsDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxOTY3MA==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191765,
                  IsciName="AB83PR58",
                  RecommendedPlanId= 224,
                  ProgramName="Product8",
                  StationLegacyCallLetters="WCCO",
                  SpotLengthId= 06,
                  AudienceId= 424,
                  DaypartCode="PT",
                  GenreName="Horror",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 04, 10),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  SpotExceptionsOutOfSpecDecision = null,
                  Impressions=100000,
                  SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 8,
                        ReasonCode = 10,
                        Reason = "Incorrect Genre",
                        Label = "Genre"
                    },
                  InventorySourceName = "CBS Synd",
                }
            };

            return spotExceptionsOutOfSpecs;
        }


        private string _GetAdvertiserName(Guid? masterId)
        {
            string advertiserName = null;
            if (masterId.HasValue)
            {
                advertiserName = _AabEngine.GetAdvertiser(masterId.Value)?.Name;
            }
            return advertiserName;
        }

        private string _GetProductName(Guid? productId, Guid? masterId)
        {
            string productName = null;
            if (masterId.HasValue)
            {
                productName = _AabEngine.GetAdvertiserProduct(productId.Value, masterId.Value)?.Name;
            }
            return productName;
        }

        private List<string> _GetAdvertiserNames(List<Guid> masterIds)
        {
            List<string> advertiserNames = new List<string>();
            foreach (var masterId in masterIds)
            {
                var advertiserName = _AabEngine.GetAdvertiser(masterId)?.Name;
                advertiserNames.Add(advertiserName ?? "Unknown");
            }
            return advertiserNames;
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
                    var advertiserName = _GetAdvertiserName(spotExceptionsOutOfSpec.AdvertiserMasterId);
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
                        AudienceName = spotExceptionsOutOfSpec.Audience?.Name,
                        ProductName = spotExceptionsOutOfSpec.Product,
                        AdvertiserName = advertiserName,
                        DaypartCode = spotExceptionsOutOfSpec.DaypartDetail?.Code,
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
                DaypartCode = spotExceptionsOutOfSpecDetail.DaypartDetail?.Code,
                Network = spotExceptionsOutOfSpecDetail.ProgramNetwork,
                AudienceName = spotExceptionsOutOfSpecDetail.Audience?.Name,
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
        public SpotExceptionsRecommendedPlanDetailsResultDto GetSpotExceptionsRecommendedPlanDetails(int spotExceptionsRecommendedPlanId)
        {
            var spotExceptionsRecommendedPlanDetailsResult = new SpotExceptionsRecommendedPlanDetailsResultDto();

            var spotExceptionsRecommendedPlan = _SpotExceptionRepository.GetSpotExceptionsRecommendedPlanById(spotExceptionsRecommendedPlanId);
            if (spotExceptionsRecommendedPlan == null)
            {
                return spotExceptionsRecommendedPlanDetailsResult;
            }

            var recommendedPlan = spotExceptionsRecommendedPlan.SpotExceptionsRecommendedPlanDetails.First(x => x.IsRecommendedPlan);

            const string programAirDateFormat = "MM/dd/yyyy";
            const string programAirTimeFormat = "hh:mm:ss tt";
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";

            spotExceptionsRecommendedPlanDetailsResult = new SpotExceptionsRecommendedPlanDetailsResultDto
            {
                Id = spotExceptionsRecommendedPlan.Id,
                EstimateId = spotExceptionsRecommendedPlan.EstimateId,
                SpotLengthString = spotExceptionsRecommendedPlan.SpotLength != null ? $":{spotExceptionsRecommendedPlan.SpotLength.Length}" : null,
                Product = _GetProductName(recommendedPlan.RecommendedPlanDetail.AdvertiserMasterId, recommendedPlan.RecommendedPlanDetail.ProductMasterId),
                FlightStartDate = recommendedPlan.RecommendedPlanDetail.FlightStartDate.ToString(),
                FlightEndDate = recommendedPlan.RecommendedPlanDetail.FlightEndDate.ToString(),
                FlightDateString = $"{Convert.ToDateTime(recommendedPlan.RecommendedPlanDetail.FlightStartDate).ToString(flightStartDateFormat)}-{Convert.ToDateTime(recommendedPlan.RecommendedPlanDetail.FlightEndDate).ToString(flightEndDateFormat)}",
                ProgramName = spotExceptionsRecommendedPlan.ProgramName,
                ProgramAirDate = spotExceptionsRecommendedPlan.ProgramAirTime.ToString(programAirDateFormat),
                ProgramAirTime = spotExceptionsRecommendedPlan.ProgramAirTime.ToString(programAirTimeFormat),
                InventorySourceName = spotExceptionsRecommendedPlan.InventorySource,
                Plans = spotExceptionsRecommendedPlan.SpotExceptionsRecommendedPlanDetails.Select(spotExceptionsRecommendedPlanDetail => new RecommendedPlanDetailResultDto
                {
                    Id = spotExceptionsRecommendedPlanDetail.Id,
                    Name = spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.Name,
                    SpotLengthString = string.Join(", ", spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.SpotLengths.Select(spotLength => $":{spotLength.Length}")),
                    FlightStartDate = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightStartDate}",
                    FlightEndDate = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightEndDate}",
                    FlightDateString = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightStartDate.ToString(flightStartDateFormat)}-{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightEndDate.ToString(flightEndDateFormat)}",
                    IsRecommendedPlan = spotExceptionsRecommendedPlanDetail.IsRecommendedPlan,
                    IsSelected = spotExceptionsRecommendedPlanDetail.SpotExceptionsRecommendedPlanDecision != null,
                    Pacing = _calculatePacing(spotExceptionsRecommendedPlanDetail.DeliveredImpressions, spotExceptionsRecommendedPlanDetail.ContractedImpressions) + "%",
                    AcceptedAsInSpec = spotExceptionsRecommendedPlanDetail.SpotExceptionsRecommendedPlanDecision?.AcceptedAsInSpec,
                    RecommendedPlanId = spotExceptionsRecommendedPlanDetail.RecommendedPlanId,
                    AudienceName = spotExceptionsRecommendedPlanDetail.AudienceName,
                    Product = _GetProductName(recommendedPlan.RecommendedPlanDetail.AdvertiserMasterId, recommendedPlan.RecommendedPlanDetail.ProductMasterId),
                    DaypartCode = spotExceptionsRecommendedPlanDetail.DaypartCode,
                    Impressions = spotExceptionsRecommendedPlanDetail.SpotDeliveredImpressions/1000,
                    TotalContractedImpressions = spotExceptionsRecommendedPlanDetail.PlanTotalContractedImpressions /1000,
                    TotalDeliveredImpressionsSelected = (spotExceptionsRecommendedPlanDetail.SpotDeliveredImpressions + spotExceptionsRecommendedPlanDetail.PlanTotalDeliveredImpressions) / 1000,
                    TotalPacingSelected = ((spotExceptionsRecommendedPlanDetail.PlanTotalDeliveredImpressions + spotExceptionsRecommendedPlanDetail.SpotDeliveredImpressions) / spotExceptionsRecommendedPlanDetail.PlanTotalContractedImpressions) * 100,
                    TotalDeliveredImpressionsUnselected = spotExceptionsRecommendedPlanDetail.PlanTotalDeliveredImpressions/1000,
                    TotalPacingUnselected = (spotExceptionsRecommendedPlanDetail.PlanTotalDeliveredImpressions / spotExceptionsRecommendedPlanDetail.PlanTotalContractedImpressions) * 100,
                    WeeklyContractedImpressions = spotExceptionsRecommendedPlanDetail.ContractedImpressions / 1000,
                    WeeklyDeliveredImpressionsSelected = (spotExceptionsRecommendedPlanDetail.DeliveredImpressions + spotExceptionsRecommendedPlanDetail.SpotDeliveredImpressions) / 1000,
                    WeeklyPacingSelected = _calculateWeeklyPacingSelected(spotExceptionsRecommendedPlanDetail.DeliveredImpressions,spotExceptionsRecommendedPlanDetail.SpotDeliveredImpressions,spotExceptionsRecommendedPlanDetail.ContractedImpressions), 
                    WeeklyDeliveredImpressionsUnselected = spotExceptionsRecommendedPlanDetail.DeliveredImpressions / 1000,
                    WeeklyPacingUnselected = _calculateWeeklyPacingUnselected(spotExceptionsRecommendedPlanDetail.DeliveredImpressions,spotExceptionsRecommendedPlanDetail.ContractedImpressions), 
                }).ToList()
            };
            if (spotExceptionsRecommendedPlanDetailsResult.Plans != null &&
                spotExceptionsRecommendedPlanDetailsResult.Plans.Any(x => x.IsSelected))
            {
                foreach (var planDetail in spotExceptionsRecommendedPlanDetailsResult.Plans)
                {
                    planDetail.IsRecommendedPlan = planDetail.IsSelected;
                }
            }
            return spotExceptionsRecommendedPlanDetailsResult;
        }

        private double? _calculateWeeklyPacingSelected(double? deliveredImp,double? spotDeliveredImp,double? contractedImp)
        {
            if(contractedImp == 0)
            {
                return 0;
            }
            else
            {
                double? weeklyPacingSelected = ((deliveredImp + spotDeliveredImp) / contractedImp) * 100;
                return weeklyPacingSelected;
            }
        }

        private double? _calculateWeeklyPacingUnselected(double? deliveredImp,double? contractedImp)
        {
            if (deliveredImp == 0 || contractedImp == 0)
            {
                return 0;
            }
            else
            {
                double? weeklyPacingUnselected = (deliveredImp / contractedImp) * 100;
                return weeklyPacingUnselected;
            }
        }

        private double? _calculatePacing(double? deliveredImp, double? contractedImp)
        {
            if (deliveredImp == 0 || contractedImp == 0)
            {
                return 0;
            }
            else
            {
                double? weeklyPacing = (deliveredImp / contractedImp) * 100;
                return weeklyPacing;
            }
        }

        /// <inheritdoc />
        public List<string> GetSpotExceptionsRecommendedPlansAdvertisers(SpotExceptionsRecommendedPlansAdvertisersRequestDto spotExceptionsRecommendedPlansAdvertisersRequest)
        {
            List<string> advertisers = new List<string>();

            SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto();
            spotExceptionsRecommendedPlansRequest.WeekStartDate = spotExceptionsRecommendedPlansAdvertisersRequest.WeekStartDate;
            spotExceptionsRecommendedPlansRequest.WeekEndDate = spotExceptionsRecommendedPlansAdvertisersRequest.WeekEndDate;
            var recommendedPlanAdvertiserMasterIdsPerWeek = _GetRecommendedPlanAdvertiserMasterIdsPerWeek(spotExceptionsRecommendedPlansRequest.WeekStartDate, spotExceptionsRecommendedPlansRequest.WeekEndDate);

            if (recommendedPlanAdvertiserMasterIdsPerWeek == null)
            {
                return advertisers;
            }
            advertisers = _GetAdvertiserNames(recommendedPlanAdvertiserMasterIdsPerWeek);
            return advertisers;
        }

        /// <inheritdoc />
        protected List<Guid> _GetRecommendedPlanAdvertiserMasterIdsPerWeek (DateTime weekStartDate, DateTime weekEndDate)
        {
            var recommendedPlanIds = _SpotExceptionRepository.GetRecommendedPlanAdvertiserMasterIdsPerWeek(weekStartDate, weekEndDate);

            return recommendedPlanIds;
        }

        /// <inheritdoc />
        public bool SaveSpotExceptionsRecommendedPlan(SpotExceptionsRecommendedPlanSaveRequestDto spotExceptionsRecommendedPlanSaveRequest, string userName)
        {
            int saveCount = 0;
            bool isSaved = false;
            foreach (var spotExceptionsRecommendedPlan in spotExceptionsRecommendedPlanSaveRequest.SpotExceptionsRecommendedPlans)
            {
                var spotExceptionsRecommendedPlanDecision = new SpotExceptionsRecommendedPlanDecisionDto
                {
                    SpotExceptionsId = spotExceptionsRecommendedPlan.Id,
                    SpotExceptionsRecommendedPlanId = spotExceptionsRecommendedPlan.SelectedPlanId,
                    UserName = userName,
                    CreatedAt = _DateTimeEngine.GetCurrentMoment(),
                    AcceptedAsInSpec = spotExceptionsRecommendedPlanSaveRequest.AcceptAsInSpec,
                    SyncedAt = null,
                    SyncedBy = null
                };
                bool isSpotExceptionsRecommendedPlanDecisionSaved = false;
                isSpotExceptionsRecommendedPlanDecisionSaved = _SpotExceptionRepository.SaveSpotExceptionsRecommendedPlanDecision(spotExceptionsRecommendedPlanDecision);
                saveCount += isSpotExceptionsRecommendedPlanDecisionSaved ? 1 : 0;
            }
            isSaved = saveCount > 0 ? true : false;
            return isSaved;
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

            var stations = _SpotExceptionRepository.GetSpotExceptionsRecommendedPlanStations(spotExceptionsRecommendedPlansRequest.WeekStartDate, spotExceptionsRecommendedPlansRequest.WeekEndDate);

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
                    var advertiserName = _GetAdvertiserName(planDetails.AdvertiserMasterId);
                    return new SpotExceptionsOutOfSpecToDoPlansDto
                    {
                        PlanId = planDetails.PlanId,
                        AdvertiserName = advertiserName,
                        PlanName = planDetails.RecommendedPlanName,
                        AffectedSpotsCount = activePlan.Count(),
                        Impressions = planDetails.Impressions / 1000,
                        SyncedTimestamp = null,
                        SpotLengthString = planDetails.SpotLength != null ? $":{planDetails.SpotLength.Length}" : null,
                        AudienceName = planDetails.Audience?.Name,
                        FlightString = planDetails.FlightStartDate.HasValue && planDetails.FlightEndDate.HasValue ? $"{Convert.ToDateTime(planDetails.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(planDetails.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(planDetails.FlightStartDate), Convert.ToDateTime(planDetails.FlightEndDate)).ToString() + " " + "Weeks"})" : null,
                    };
                }).ToList();

                spotExceptionsOutOfSpecPlansResults.Completed = completedPlans.GroupBy(completedPlan => new { completedPlan.RecommendedPlanId })
                .Select(completedPlan =>
                {
                    var planDetails = completedPlan.First();
                    var advertiserName = _GetAdvertiserName(planDetails.AdvertiserMasterId);
                    return new SpotExceptionsOutOfSpecCompletedPlansDto
                    {
                        PlanId = planDetails.PlanId,
                        AdvertiserName = advertiserName,
                        PlanName = planDetails.RecommendedPlanName,
                        AffectedSpotsCount = completedPlan.Count(),
                        Impressions = planDetails.Impressions / 1000,
                        SyncedTimestamp = null,
                        SpotLengthString = planDetails.SpotLength != null ? $":{planDetails.SpotLength.Length}" : null,
                        AudienceName = planDetails.Audience?.Name,
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

        /// <inheritdoc />
        public SpotExceptionsOutOfSpecPlanSpotsResultDto GetSpotExceptionsOutofSpecSpots(SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutofSpecSpotsRequest)
        {
            var spotExceptionsOutOfSpecPlanSpotsResult = new SpotExceptionsOutOfSpecPlanSpotsResultDto();
            List<SpotExceptionsOutOfSpecsDto> activePlans = null;
            List<SpotExceptionsOutOfSpecsDto> queuedPlans = null;

            var spotExceptionsOutOfSpecsPlanSpots = _SpotExceptionRepository.GetSpotExceptionsOutOfSpecPlanSpots(spotExceptionsOutofSpecSpotsRequest.PlanId,
                spotExceptionsOutofSpecSpotsRequest.WeekStartDate, spotExceptionsOutofSpecSpotsRequest.WeekEndDate);

            if (spotExceptionsOutOfSpecsPlanSpots?.Any() ?? false)
            {
                var planIds = spotExceptionsOutOfSpecsPlanSpots.Select(p => p.PlanId).Distinct().ToList();
                var daypartsList = _PlanRepository.GetPlanDaypartsByPlanIds(planIds);
                activePlans = spotExceptionsOutOfSpecsPlanSpots.Where(spotExceptionDecisionPlans => spotExceptionDecisionPlans.SpotExceptionsOutOfSpecDecision == null).ToList();
                queuedPlans = spotExceptionsOutOfSpecsPlanSpots.Where(spotExceptionDecisionPlans => spotExceptionDecisionPlans.SpotExceptionsOutOfSpecDecision != null).ToList();

                spotExceptionsOutOfSpecPlanSpotsResult.Active = activePlans
                .Select(activePlan =>
                {
                    return new SpotExceptionsOutOfSpecActivePlanSpotsDto
                    {
                        Id = activePlan.Id,
                        EstimateId = activePlan.EstimateId,
                        Reason = activePlan.SpotExceptionsOutOfSpecReasonCode.Reason,
                        MarketRank = activePlan.MarketRank,
                        DMA = activePlan.DMA,
                        Market = activePlan.Market,
                        Station = activePlan.StationLegacyCallLetters,
                        TimeZone = activePlan.TimeZone,
                        Affiliate = activePlan.Affiliate,
                        Day = activePlan.ProgramAirTime.DayOfWeek.ToString(),
                        GenreName = activePlan.GenreName,
                        HouseIsci = activePlan.HouseIsci,
                        ClientIsci = activePlan.IsciName,
                        ProgramAirDate = activePlan.ProgramAirTime.ToShortDateString(),
                        ProgramAirTime = activePlan.ProgramAirTime.ToLongTimeString(),
                        ProgramName = activePlan.ProgramName,
                        SpotLengthString = activePlan.SpotLength != null ? $":{activePlan.SpotLength.Length}" : null,
                        DaypartCode = activePlan.DaypartCode,
                        Comments = activePlan.Comments,
                        PlanDaypartCodes = daypartsList.Where(d => d.PlanId == activePlan.PlanId).Select(s => s.Code).Distinct().ToList(),
                        InventorySourceName = activePlan.InventorySourceName
                    };
                }).ToList();

                spotExceptionsOutOfSpecPlanSpotsResult.Queued = queuedPlans.Where(syncedSpot => syncedSpot.SpotExceptionsOutOfSpecDecision.SyncedAt == null)
                .Select(queuedPlan =>
                {
                    return new SpotExceptionsOutOfSpecQueuedPlanSpotsDto
                    {
                        Id = queuedPlan.Id,
                        EstimateId = queuedPlan.EstimateId,
                        Reason = queuedPlan.SpotExceptionsOutOfSpecReasonCode.Reason,
                        MarketRank = queuedPlan.MarketRank,
                        DMA = queuedPlan.DMA,
                        Market = queuedPlan.Market,
                        Station = queuedPlan.StationLegacyCallLetters,
                        TimeZone = queuedPlan.TimeZone,
                        Affiliate = queuedPlan.Affiliate,
                        Day = queuedPlan.ProgramAirTime.DayOfWeek.ToString(),
                        GenreName = queuedPlan.SpotExceptionsOutOfSpecDecision.GenreName == null ? queuedPlan.GenreName : queuedPlan.SpotExceptionsOutOfSpecDecision.GenreName,
                        HouseIsci = queuedPlan.HouseIsci,
                        ClientIsci = queuedPlan.IsciName,
                        ProgramAirDate = queuedPlan.ProgramAirTime.ToShortDateString(),
                        ProgramAirTime = queuedPlan.ProgramAirTime.ToLongTimeString(),
                        FlightEndDate = queuedPlan.FlightEndDate.ToString(),
                        ProgramName = queuedPlan.SpotExceptionsOutOfSpecDecision.ProgramName == null ? queuedPlan.ProgramName : queuedPlan.SpotExceptionsOutOfSpecDecision.ProgramName,
                        SpotLengthString = queuedPlan.SpotLength != null ? $":{queuedPlan.SpotLength.Length}" : null,
                        DaypartCode = queuedPlan.SpotExceptionsOutOfSpecDecision.DaypartCode == null ? queuedPlan.DaypartCode : queuedPlan.SpotExceptionsOutOfSpecDecision.DaypartCode,
                        DecisionString = queuedPlan.SpotExceptionsOutOfSpecDecision.DecisionNotes,
                        Comments = queuedPlan.Comments,
                        PlanDaypartCodes = daypartsList.Where(d => d.PlanId == queuedPlan.PlanId).Select(s => s.Code).Distinct().ToList(),
                        InventorySourceName = queuedPlan.InventorySourceName
                    };
                }).ToList();

                spotExceptionsOutOfSpecPlanSpotsResult.Synced = queuedPlans.Where(syncedSpot => syncedSpot.SpotExceptionsOutOfSpecDecision.SyncedAt != null)
                .Select(syncedPlan =>
                {
                    return new SpotExceptionsOutOfSpecSyncedPlanSpotsDto
                    {
                        Id = syncedPlan.Id,
                        EstimateId = syncedPlan.EstimateId,
                        Reason = syncedPlan.SpotExceptionsOutOfSpecReasonCode.Reason,
                        MarketRank = syncedPlan.MarketRank,
                        DMA = syncedPlan.DMA,
                        Market = syncedPlan.Market,
                        Station = syncedPlan.StationLegacyCallLetters,
                        TimeZone = syncedPlan.TimeZone,
                        Affiliate = syncedPlan.Affiliate,
                        Day = syncedPlan.ProgramAirTime.DayOfWeek.ToString(),
                        GenreName = syncedPlan.SpotExceptionsOutOfSpecDecision.GenreName == null ? syncedPlan.GenreName : syncedPlan.SpotExceptionsOutOfSpecDecision.GenreName,
                        HouseIsci = syncedPlan.HouseIsci,
                        ClientIsci = syncedPlan.IsciName,
                        ProgramAirDate = syncedPlan.ProgramAirTime.ToShortDateString(),
                        ProgramAirTime = syncedPlan.ProgramAirTime.ToLongTimeString(),
                        FlightEndDate = syncedPlan.FlightEndDate.ToString(),
                        ProgramName = syncedPlan.SpotExceptionsOutOfSpecDecision.ProgramName == null ? syncedPlan.ProgramName : syncedPlan.SpotExceptionsOutOfSpecDecision.ProgramName,
                        SpotLengthString = syncedPlan.SpotLength != null ? $":{syncedPlan.SpotLength.Length}" : null,
                        DaypartCode = syncedPlan.SpotExceptionsOutOfSpecDecision.DaypartCode == null ? syncedPlan.DaypartCode : syncedPlan.SpotExceptionsOutOfSpecDecision.DaypartCode,
                        DecisionString = syncedPlan.SpotExceptionsOutOfSpecDecision.DecisionNotes,
                        SyncedTimestamp = syncedPlan.SpotExceptionsOutOfSpecDecision.SyncedAt.ToString(),
                        Comments = syncedPlan.Comments,
                        PlanDaypartCodes = daypartsList.Where(d => d.PlanId == syncedPlan.PlanId).Select(s => s.Code).Distinct().ToList(),
                        InventorySourceName = syncedPlan.InventorySourceName
                    };
                }).ToList();
            }
            return spotExceptionsOutOfSpecPlanSpotsResult;
        }

        /// <inheritdoc />
        public List<string> GetSpotExceptionsOutOfSpecMarkets(SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var spotExceptionsOutOfSpecsPlanSpots = _SpotExceptionRepository.GetSpotExceptionsOutOfSpecPlanSpots(spotExceptionsOutOfSpecSpotsRequest.PlanId,
                spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);

            if (spotExceptionsOutOfSpecsPlanSpots == null)
            {
                return null;
            }

            var markets = spotExceptionsOutOfSpecsPlanSpots.Select(activeSpotExceptionsOutOfSpecSpotsResult => activeSpotExceptionsOutOfSpecSpotsResult.Market ?? "Unknown").Distinct().OrderBy(market => market).ToList();
            return markets;
        }

        /// <inheritdoc />
        public SpotExceptionOutOfSpecUnpostedResultDto GetSpotExceptionsUnposted(SpotExceptionOutOfSpecUnpostedRequestDto spotExceptionOutOfSpecUnpostedRequest)
        {
            var spotExceptionOutOfSpecUnpostedResult = new SpotExceptionOutOfSpecUnpostedResultDto();

            var spotExceptionUnpostedNoPlanResult = _SpotExceptionRepository.GetSpotExceptionUnpostedNoPlan(spotExceptionOutOfSpecUnpostedRequest.WeekStartDate, spotExceptionOutOfSpecUnpostedRequest.WeekEndDate);
            var spotExceptionUnpostedNoReelRosterResult = _SpotExceptionRepository.GetSpotExceptionUnpostedNoReelRoster(spotExceptionOutOfSpecUnpostedRequest.WeekStartDate, spotExceptionOutOfSpecUnpostedRequest.WeekEndDate);

            spotExceptionOutOfSpecUnpostedResult.NoPlan = spotExceptionUnpostedNoPlanResult.Select(x => new SpotExceptionOutOfSpecNoPlanDto
            {
                HouseIsci = x.HouseIsci,
                ClientIsci = x.ClientIsci,
                ClientSpotLength = ":" + x.ClientSpotLengthId.ToString(),
                AffectedSpotsCount = x.Count,
                ProgramAirDate = x.ProgramAirTime.ToShortDateString(),
                EstimateId = x.EstimateID
            }).ToList();

            spotExceptionOutOfSpecUnpostedResult.NoReelRoster = spotExceptionUnpostedNoReelRosterResult.Select(x => new SpotExceptionOutOfSpecNoReelRosterDto
            {
                HouseIsci = x.HouseIsci,
                AffectedSpotsCount = x.Count,
                ProgramAirDate = x.ProgramAirTime.ToShortDateString(),
                EstimateId = x.EstimateId
            }).ToList();

            return spotExceptionOutOfSpecUnpostedResult;
        }

        /// <inheritdoc />
        public List<SpotExceptionsOutOfSpecProgramsDto> GetSpotExceptionsOutOfSpecPrograms(string programNameQuery, string userName)
        {
            SearchRequestProgramDto searchRequest = new SearchRequestProgramDto();
            List<SpotExceptionsOutOfSpecProgramsDto> result = new List<SpotExceptionsOutOfSpecProgramsDto>();
            searchRequest.ProgramName = programNameQuery;
            var programList = _ProgramService.GetPrograms(searchRequest, userName);
            programList.ForEach(p =>
            {
                SpotExceptionsOutOfSpecProgramsDto program = new SpotExceptionsOutOfSpecProgramsDto();
                program.ProgramName = p.Name;
                program.GenreName = p.Genre.Display;
                result.Add(program);
            }
                );
            return result;
        }

        /// <inheritdoc />
        public bool SaveOutofSpecDecisionsPlans(SpotExceptionSaveDecisionsPlansRequestDto spotExceptionSaveDecisionsPlansRequest, string userName)
        {
            var createdAt = _DateTimeEngine.GetCurrentMoment();

            var isSpotExceptionsOutOfSpecDecisionSaved = _SpotExceptionRepository.SaveSpotExceptionsOutOfSpecsDecisionsPlans(spotExceptionSaveDecisionsPlansRequest, userName, createdAt);

            return isSpotExceptionsOutOfSpecDecisionSaved;
        }

        /// <inheritdoc />
        public bool TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest)
        {
            var dateTime = DateTime.Now;
            var isSyncedOutOfSpecDecision = _SpotExceptionRepository.SyncOutOfSpecDecision(triggerDecisionSyncRequest, dateTime);
            var isSyncedRecommandedPlanDecision = _SpotExceptionRepository.SyncRecommandedPlanDecision(triggerDecisionSyncRequest, dateTime);
            bool isSynced;
            if (isSyncedOutOfSpecDecision == false && isSyncedRecommandedPlanDecision == false)
            {
                isSynced = false;
            }
            else
            {
                isSynced = true;
            }
            return isSynced;
        }

        /// <inheritdoc />
        public int GetQueuedDecisionCount()
        {
            var outOfSpecDecisonQueuedCount = _SpotExceptionRepository.GetDecisionQueuedCount();
            var recommandedPlanDecisonQueuedCount = _SpotExceptionRepository.GetRecommandedPlanDecisionQueuedCount();
            int totalDecisionCount = outOfSpecDecisonQueuedCount + recommandedPlanDecisonQueuedCount;
            return totalDecisionCount;
        }

        /// <inheritdoc />
        public SpotExceptionsRecommendedPlansResultsDto GetRecommendedPlans(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest)
        {
            var spotExceptionsRecommendedPlansResults = new SpotExceptionsRecommendedPlansResultsDto();
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";
            List<SpotExceptionsRecommendedPlansDto> activePlans = null;
            List<SpotExceptionsRecommendedPlansDto> completedPlans = null;

            var spotExceptionsoutRecommendedPlans = _SpotExceptionRepository.GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest.WeekStartDate, spotExceptionsRecommendedPlansRequest.WeekEndDate);

            if (spotExceptionsoutRecommendedPlans?.Any() ?? false)
            {
                activePlans = spotExceptionsoutRecommendedPlans.Where(spotExceptionDecisionPlans => spotExceptionDecisionPlans.SpotExceptionsRecommendedPlanDetails.Any(x => x.SpotExceptionsRecommendedPlanDecision == null)).ToList();
                completedPlans = spotExceptionsoutRecommendedPlans.Where(spotExceptionDecisionPlans => spotExceptionDecisionPlans.SpotExceptionsRecommendedPlanDetails.Exists(x => x.SpotExceptionsRecommendedPlanDecision != null)).ToList();

                spotExceptionsRecommendedPlansResults.Active = activePlans.GroupBy(activePlan => new { activePlan.SpotExceptionsRecommendedPlanDetails.First(y => y.IsRecommendedPlan).RecommendedPlanId })
                    .Select(activePlan =>
                    {
                        var planDetails = activePlan.First();
                        var planAdvertiserMasterId = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.AdvertiserMasterId).First();
                        var flightStartDate = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.FlightStartDate).First();
                        var flightEndDate = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.FlightEndDate).First();
                        return new SpotExceptionsRecommandedToDoPlansDto
                        {
                            PlanId = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).First(),
                            AdvertiserName = _GetAdvertiserName(planAdvertiserMasterId),
                            PlanName = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).First(),
                            AffectedSpotsCount = activePlan.Count(),
                            Impressions = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.SpotDeliveredImpressions).First() / 1000,
                            SpotLengthString = $"{_SpotLengthRepository.GetSpotLengthById(planDetails.SpotLengthId ?? 0)}" ?? null,
                            AudienceName = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.AudienceName).First(),
                            FlightString = $"{Convert.ToDateTime(flightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(flightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(flightStartDate), Convert.ToDateTime(flightEndDate)).ToString() + " " + "Weeks"})",
                        };
                    }).ToList();

                spotExceptionsRecommendedPlansResults.Completed = completedPlans?.GroupBy(completedPlan => new { completedPlan.SpotExceptionsRecommendedPlanDetails.First(y => y.IsRecommendedPlan).RecommendedPlanId })
                .Select(completedPlan =>
                {
                    var planDetails = completedPlan.First();
                    var planAdvertiserMasterId = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.AdvertiserMasterId).First();
                    var flightStartDate = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.FlightStartDate).First();
                    var flightEndDate = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.FlightEndDate).First();
                    return new SpotExceptionsRecommandedCompletedPlansDto
                    {
                        PlanId = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).First(),
                        AdvertiserName = _GetAdvertiserName(planAdvertiserMasterId),
                        PlanName = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).First(),
                        AffectedSpotsCount = completedPlan.Count(),
                        Impressions = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.SpotDeliveredImpressions).First() / 1000,
                        SpotLengthString = $"{_SpotLengthRepository.GetSpotLengthById(planDetails.SpotLengthId ?? 0)}" ?? null,
                        AudienceName = planDetails.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.AudienceName).First(),
                        FlightString = $"{Convert.ToDateTime(flightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(flightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(flightStartDate), Convert.ToDateTime(flightEndDate)).ToString() + " " + "Weeks"})",
                    };
                }).ToList();
            }         
            return spotExceptionsRecommendedPlansResults;
        }

        /// <inheritdoc />
        public List<SpotExceptionsOutOfSpecGenreDto> GetSpotExceptionsOutOfSpecGenres(string genre)
        {
            var genres = new List<SpotExceptionsOutOfSpecGenreDto>();
            if (string.IsNullOrEmpty(genre))
            {
                genres = _SpotExceptionRepository.GetSpotExceptionsOutOfSpecGenresBySourceId();
                _RemoveUnmatched(genres);
                if (!_FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_VARIOUS_GENRE_RESTRICTION))
                {
                    _RemoveVarious(genres);
                }
            }
            else
            {
                genres = _GetGenresGroupMock(genre);
            }
            return genres;
        }

        private List<SpotExceptionsOutOfSpecGenreDto> _GetGenresGroupMock(string genre)
        {
            var mockedGenres = new List<SpotExceptionsOutOfSpecGenreDto>();
            for (int genreCount = 0; genreCount < 3; genreCount++)
            {
                var mockedGenre = new SpotExceptionsOutOfSpecGenreDto()
                {
                    Id = genreCount + 1,
                    GenreName = genreCount == 0 ? genre : genre + "" + genreCount
                };
                mockedGenres.Add(mockedGenre);
            }
            return mockedGenres;
        }

        private void _RemoveUnmatched(List<SpotExceptionsOutOfSpecGenreDto> genres)
        {
            genres.RemoveAll(x => x.GenreName.Equals("Unmatched", StringComparison.OrdinalIgnoreCase));
        }

        private void _RemoveVarious(List<SpotExceptionsOutOfSpecGenreDto> genres)
        {
            genres.RemoveAll(x => x.GenreName.Equals("Various", StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public SpotExceptionsRecommendedPlanSpotsResultDto GetSpotExceptionsRecommendedPlanSpots(RecomendedPlansRequestDto recomendedPlansRequest)
        {
            var spotExceptionsRecommendedPlanSpotsResult = new SpotExceptionsRecommendedPlanSpotsResultDto();
            List<SpotExceptionsRecommendedPlansDto> activePlans = null;
            List<SpotExceptionsRecommendedPlansDto> queuedPlans = null;

            var spotExceptionsRecommendedPlanSpots = _SpotExceptionRepository.GetSpotExceptionRecommendedPlanSpots(recomendedPlansRequest.PlanId,
                recomendedPlansRequest.WeekStartDate, recomendedPlansRequest.WeekEndDate);

            if (spotExceptionsRecommendedPlanSpots?.Any() ?? false)
            {
                activePlans = spotExceptionsRecommendedPlanSpots?.Where(spotExceptionDecisionPlans => spotExceptionDecisionPlans.SpotExceptionsRecommendedPlanDetails.All(x => x.SpotExceptionsRecommendedPlanDecision == null)).ToList();
                queuedPlans = spotExceptionsRecommendedPlanSpots?.Where(spotExceptionDecisionPlans => spotExceptionDecisionPlans.SpotExceptionsRecommendedPlanDetails.Exists(x => x.SpotExceptionsRecommendedPlanDecision != null)).ToList();
                foreach (var plan in queuedPlans)
                {
                    foreach (var detail in plan.SpotExceptionsRecommendedPlanDetails)
                    {
                        detail.RecommendedPlanId = _GetRecommendedPlanId(plan);
                        detail.RecommendedPlanDetail.Name = _GetRecommendedPlanName(plan);
                    }
                }
                spotExceptionsRecommendedPlanSpotsResult.Active = activePlans
               .Select(activePlan =>
               {
                   var planDetails = activePlan.SpotExceptionsRecommendedPlanDetails;
                   var activePlanDetails = activePlans.First();
                   return new SpotExceptionsRecommendedActivePlanSpotsDto
                   {
                       Id = activePlan.Id,
                       EstimateId = activePlan.EstimateId,
                       IsciName = activePlan.ClientIsci,
                       ProgramAirDate = activePlan.ProgramAirTime.ToShortDateString(),
                       ProgramAirTime = activePlan.ProgramAirTime.ToLongTimeString(),
                       RecommendedPlan = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).FirstOrDefault(),
                       Impressions = activePlanDetails.SpotExceptionsRecommendedPlanDetails.Select(x => x.SpotDeliveredImpressions).First() / 1000,
                       ProgramName = activePlan.ProgramName,
                       Affiliate = activePlan.Affiliate,
                       PlanId = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).FirstOrDefault(),
                       Market = _GetMarketName(activePlan.MarketCode ?? 0),
                       Station = activePlan.StationLegacyCallLetters,
                       InventorySource = activePlan.InventorySource
                   };
               }).ToList();

                spotExceptionsRecommendedPlanSpotsResult.Queued = queuedPlans?.Where(syncedSpot => syncedSpot.SpotExceptionsRecommendedPlanDetails.Any(x => x.SpotExceptionsRecommendedPlanDecision != null && x.SpotExceptionsRecommendedPlanDecision.SyncedAt == null))
                .Select(queuedPlan =>
                {
                    var planDetails = queuedPlan.SpotExceptionsRecommendedPlanDetails;
                    var queuedPlanDetails = queuedPlans.First();
                    return new SpotExceptionsRecommendedQueuedPlanSpotsDto
                    {
                        Id = queuedPlan.Id,
                        EstimateId = queuedPlan.EstimateId,
                        IsciName = queuedPlan.ClientIsci,
                        ProgramAirDate = queuedPlan.ProgramAirTime.ToShortDateString(),
                        ProgramAirTime = queuedPlan.ProgramAirTime.ToLongTimeString(),
                        RecommendedPlan = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).FirstOrDefault(),
                        Impressions = queuedPlanDetails.SpotExceptionsRecommendedPlanDetails.Select(x => x.SpotDeliveredImpressions).First() / 1000,
                        ProgramName = queuedPlan.ProgramName,
                        Affiliate = queuedPlan.Affiliate,
                        PlanId = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).FirstOrDefault(),
                        Market = _GetMarketName(queuedPlan.MarketCode ?? 0),
                        Station = queuedPlan.StationLegacyCallLetters,
                        InventorySource = queuedPlan.InventorySource,
                        DecisionString = planDetails.Where(x => x.SpotExceptionsRecommendedPlanDecision != null).Select(y => y.SpotExceptionsRecommendedPlanDecision.AcceptedAsInSpec).FirstOrDefault() ? "In" : "Out"
                    };
                }).ToList();

                spotExceptionsRecommendedPlanSpotsResult.Synced = queuedPlans.Where(syncedSpot => syncedSpot.SpotExceptionsRecommendedPlanDetails.Any(x => x.SpotExceptionsRecommendedPlanDecision != null && x.SpotExceptionsRecommendedPlanDecision.SyncedAt != null))
                .Select(syncedPlan =>
                {
                    var planDetails = syncedPlan.SpotExceptionsRecommendedPlanDetails;
                    var syncedPlanDetails = queuedPlans.First();
                    return new SpotExceptionsRecommendedSyncedPlanSpotsDto
                    {
                        Id = syncedPlan.Id,
                        EstimateId = syncedPlan.EstimateId,
                        IsciName = syncedPlan.ClientIsci,
                        ProgramAirDate = syncedPlan.ProgramAirTime.ToShortDateString(),
                        ProgramAirTime = syncedPlan.ProgramAirTime.ToLongTimeString(),
                        RecommendedPlan = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).FirstOrDefault(),
                        Impressions = syncedPlanDetails.SpotExceptionsRecommendedPlanDetails.Select(x => x.SpotDeliveredImpressions).First() / 1000,
                        ProgramName = syncedPlan.ProgramName,
                        Affiliate = syncedPlan.Affiliate,
                        PlanId = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).FirstOrDefault(),
                        Market = _GetMarketName(syncedPlan.MarketCode ?? 0),
                        Station = syncedPlan.StationLegacyCallLetters,
                        SyncedTimestamp = syncedPlan.SpotExceptionsRecommendedPlanDetails.Where(x => x.SpotExceptionsRecommendedPlanDecision != null).Select(x => x.SpotExceptionsRecommendedPlanDecision.SyncedAt).FirstOrDefault().ToString(),
                        InventorySource = syncedPlan.InventorySource,
                        DecisionString = syncedPlan.SpotExceptionsRecommendedPlanDetails.Where(x => x.SpotExceptionsRecommendedPlanDecision != null).Select(y => y.SpotExceptionsRecommendedPlanDecision.AcceptedAsInSpec).FirstOrDefault() ? "In" : "Out"
                    };
                }).ToList();
            }
            return spotExceptionsRecommendedPlanSpotsResult;
        }

        /// <summary>Gets the name of the market.</summary>
        /// <param name="marketCode">The market code.</param>
        /// <returns></returns>
        private string _GetMarketName(int marketCode)
        {
            var marketName = _SpotExceptionRepository.GetMarketName(marketCode);
            return marketName;
        }

        /// <summary>Gets the market names.</summary>
        /// <param name="marketCodes">The market codes.</param>
        /// <returns></returns>
        protected List<string> _GetMarketNames(List<int> marketCodes)
        {
            var marketNames = new List<string>();
            if (marketCodes != null)
            {
                foreach (var marketCode in marketCodes)
                {
                    var marketName = _SpotExceptionRepository.GetMarketName(marketCode);
                    marketNames.Add(marketName ?? "Unknown");
                }
            }
            return marketNames;
        }

        /// <inheritdoc />
        public RecommendedPlanFiltersResultDto GetRecommendedPlansFilters(RecomendedPlansRequestDto recommendedPlansRequest)
        {
            var recommendedPlanFiltersResult = new RecommendedPlanFiltersResultDto();
            var spotExceptionsRecommendedSpotsResult = _SpotExceptionRepository.GetSpotExceptionRecommendedPlanSpots(recommendedPlansRequest.PlanId,
                recommendedPlansRequest.WeekStartDate, recommendedPlansRequest.WeekEndDate);

            if (spotExceptionsRecommendedSpotsResult == null)
            {
                return recommendedPlanFiltersResult;
            }

            var marketCodes = spotExceptionsRecommendedSpotsResult.Select(x => x.MarketCode ?? 0).ToList();
            recommendedPlanFiltersResult.Markets = _GetMarketNames(marketCodes).Distinct().OrderBy(market => market).ToList();
            recommendedPlanFiltersResult.Stations = spotExceptionsRecommendedSpotsResult.Select(activeSpotExceptionsOutOfSpecSpotsResult => activeSpotExceptionsOutOfSpecSpotsResult.StationLegacyCallLetters ?? "Unknown").Distinct().OrderBy(station => station).ToList();
            recommendedPlanFiltersResult.InventorySources = spotExceptionsRecommendedSpotsResult.Select(activeSpotExceptionsOutOfSpecSpotsResult => activeSpotExceptionsOutOfSpecSpotsResult.InventorySource ?? "Unknown").Distinct().OrderBy(inventorySource => inventorySource).ToList();
            return recommendedPlanFiltersResult;
        }

        private string _GetRecommendedPlanName(SpotExceptionsRecommendedPlansDto spotExceptionsRecommendedPlans)
        {
            string planName = spotExceptionsRecommendedPlans.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail.Name).FirstOrDefault();
            if (spotExceptionsRecommendedPlans.SpotExceptionsRecommendedPlanDetails != null)
            {
                int planDetailId = 0;
                var planDetail = spotExceptionsRecommendedPlans.SpotExceptionsRecommendedPlanDetails.
                   FirstOrDefault(x => x.SpotExceptionsRecommendedPlanDecision != null);
                if (planDetail != null)
                {
                    planDetailId = planDetail.SpotExceptionsRecommendedPlanDecision.SpotExceptionsRecommendedPlanDetailId;
                    var planDetails = spotExceptionsRecommendedPlans.SpotExceptionsRecommendedPlanDetails.FirstOrDefault(x => x.Id == planDetailId);
                    if (planDetails != null)
                    {
                        planName = planDetail.RecommendedPlanDetail.Name;
                    }
                }
            }
            return planName;
        }

        private int _GetRecommendedPlanId(SpotExceptionsRecommendedPlansDto spotExceptionsRecommendedPlans)
        {
            int planId = spotExceptionsRecommendedPlans.SpotExceptionsRecommendedPlanDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).FirstOrDefault();
            if (spotExceptionsRecommendedPlans.SpotExceptionsRecommendedPlanDetails != null)
            {
                int planDetailId = 0;
                var planDetail = spotExceptionsRecommendedPlans.SpotExceptionsRecommendedPlanDetails.FirstOrDefault(x => x.SpotExceptionsRecommendedPlanDecision != null);
                if (planDetail != null)
                {
                    planDetailId = planDetail.SpotExceptionsRecommendedPlanDecision.SpotExceptionsRecommendedPlanDetailId;
                    var planDetails = spotExceptionsRecommendedPlans.SpotExceptionsRecommendedPlanDetails.FirstOrDefault(x => x.Id == planDetailId);
                    if (planDetails != null)
                    {
                        planId = planDetails.RecommendedPlanId;
                    }
                }
            }
            return planId;
        }

        private string _GetPacing()
        {
            string pacing = "30%";
            return pacing;
        }

        /// <inheritdoc />
        public List<string> GetOutOfSpecInventorySources(SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutofSpecSpotsRequest)
        {
            var spotExceptionsOutOfSpecResult = _SpotExceptionRepository.GetSpotExceptionsOutOfSpecPlanSpots(spotExceptionsOutofSpecSpotsRequest.PlanId,
                spotExceptionsOutofSpecSpotsRequest.WeekStartDate, spotExceptionsOutofSpecSpotsRequest.WeekEndDate);
            var inventorySources = spotExceptionsOutOfSpecResult.Select(x => x.InventorySourceName ?? "Unknown").Distinct().OrderBy(inventorySource => inventorySource).ToList();
            return inventorySources;
        }
    }
}
