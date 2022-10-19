using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsService : IApplicationService
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

        Task<bool> TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest);
        Task<int> GetQueuedDecisionCount();
    }

    public class SpotExceptionsService : BroadcastBaseClass, ISpotExceptionsService
    {
        const string flightStartDateFormat = "MM/dd";
        const string flightEndDateFormat = "MM/dd/yyyy";

        private readonly ISpotExceptionsRepository _SpotExceptionsRepository;

        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        public SpotExceptionsService(
            IDataRepositoryFactory dataRepositoryFactory,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsRepository = dataRepositoryFactory.GetDataRepository<ISpotExceptionsRepository>();
            _FeatureToggleHelper = featureToggleHelper;
        }

        public bool AddSpotExceptionData(bool isIntegrationTestDatabase = false)
        {
            List<SpotExceptionsRecommendedPlanSpotsToDoDto> spotExceptionsRecommendedPlansToDo = _GetSpotExceptionsRecommendedPlansToDoMock();
            List<SpotExceptionsRecommendedPlanSpotsDoneDto> spotExceptionsRecommendedPlansDone = _GetSpotExceptionsRecommendedPlansDoneMock();
            List<SpotExceptionsOutOfSpecsToDoDto> spotExceptionsOutOfSpecsToDo = _GetSpotExceptionsOutOfSpecsToDoMock();
            List<SpotExceptionsOutOfSpecsDoneDto> spotExceptionsOutOfSpecsDone = _GetSpotExceptionsOutOfSpecsDoneMock();

            if (isIntegrationTestDatabase)
            {
                // align this mock data with existing plan data within the integration tests databases                
                spotExceptionsRecommendedPlansToDo.SelectMany(s => s.SpotExceptionsRecommendedPlanDetailsToDo).ToList().ForEach(s => s.RecommendedPlanId = 524);
                spotExceptionsRecommendedPlansDone.SelectMany(s => s.SpotExceptionsRecommendedPlanDetailsDone).ToList().ForEach(s => s.RecommendedPlanId = 524);
                spotExceptionsOutOfSpecsToDo.ForEach(s =>
                {
                    s.RecommendedPlanId = 524;
                    s.GenreName = "Comedy";
                });
                spotExceptionsOutOfSpecsDone.ForEach(s =>
                {
                    s.RecommendedPlanId = 524;
                    s.GenreName = "Comedy";
                });
            }

            var result = _SpotExceptionsRepository.AddSpotExceptionData(spotExceptionsRecommendedPlansToDo, spotExceptionsRecommendedPlansDone, spotExceptionsOutOfSpecsToDo, spotExceptionsOutOfSpecsDone);
            return result;
        }

        public bool ClearSpotExceptionMockData()
        {
            var result = _SpotExceptionsRepository.ClearSpotExceptionMockData();
            return result;
        }

        public bool ClearSpotExceptionAllData()
        {
            var result = _SpotExceptionsRepository.ClearSpotExceptionAllData();
            return result;
        }

        /// <inheritdoc />
        public async Task<bool> TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest)
        {
            var dateTime = DateTime.Now;
            bool isSynced;

            try
            {
                var isSyncedOutOfSpecDecision = await _SpotExceptionsRepository.SyncOutOfSpecDecisionsAsync(triggerDecisionSyncRequest, dateTime);
                var isSyncedRecommandedPlanDecision = await _SpotExceptionsRepository.SyncRecommendedPlanDecisionsAsync(triggerDecisionSyncRequest, dateTime);

                if (isSyncedOutOfSpecDecision == false && isSyncedRecommandedPlanDecision == false)
                {
                    isSynced = false;
                }
                else
                {
                    isSynced = true;
                }
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return isSynced;
        }

        /// <inheritdoc />
        public async Task<int> GetQueuedDecisionCount()
        {
            int totalDecisionCount;
            try
            {
                var outOfSpecDecisonQueuedCount = await _SpotExceptionsRepository.GetOutOfSpecDecisionQueuedCountAsync();
                var recommandedPlanDecisonQueuedCount = await _SpotExceptionsRepository.GetRecommendedPlanDecisionQueuedCountAsync();
                totalDecisionCount = outOfSpecDecisonQueuedCount + recommandedPlanDecisonQueuedCount;
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return totalDecisionCount;
        }

        private List<SpotExceptionsRecommendedPlanSpotsToDoDto> _GetSpotExceptionsRecommendedPlansToDoMock()
        {
            var spotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSpotsToDoDto>
            {
                new SpotExceptionsRecommendedPlanSpotsToDoDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6840,
                    InventorySource = "Tegna",
                    HouseIsci = "840T42AY13H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 09, 09, 08, 28, 28),
                    StationLegacyCallLetters = "WBNS",
                    Affiliate = "CBS",
                    MarketCode = 135,
                    MarketRank = 33,
                    ProgramName = "CBS Mornings",
                    ProgramGenre = "INFORMATIONAL/NEWS",
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    IngestedMediaWeekId = 1,
                    SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
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
                            SpotDeliveredImpressions = 10000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        }
                    }
                },
                new SpotExceptionsRecommendedPlanSpotsToDoDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA5MTAwMQ=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6616,
                    InventorySource = "Ference POD",
                    HouseIsci = "616MAY2913H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 09, 09, 08, 41, 57),
                    StationLegacyCallLetters = "WDKA",
                    Affiliate = "IND",
                    MarketCode = 232,
                    MarketRank = 3873,
                    ProgramName = "Mike & Molly",
                    ProgramGenre = "COMEDY",
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    IngestedMediaWeekId = 1,
                    SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
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
                            SpotDeliveredImpressions = 20000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDA5MTAwMQ=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        },
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
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
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDA5MTAwMQ=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        }
                    }
                },
                new SpotExceptionsRecommendedPlanSpotsToDoDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDI3MjQ2NQ=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6289,
                    InventorySource = "Sinclair Corp - Day Syn 10a-4p",
                    HouseIsci = "289IT2Y3P2H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 09, 09, 09, 58, 55),
                    StationLegacyCallLetters = "KOMO",
                    Affiliate = "ABC",
                    MarketCode = 419,
                    MarketRank = 11,
                    ProgramName = "LIVE with Kelly and Ryan",
                    ProgramGenre = "TALK",
                    IngestedBy = "Mock Data",
                    IngestedAt = DateTime.Now,
                    IngestedMediaWeekId = 1,
                    SpotExceptionsRecommendedPlanDetailsToDo=new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 1824,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 30000,
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
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDI3MjQ2NQ=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        },
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 1923,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 40000,
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
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDI3MjQ2NQ=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        }
                    }
                },
                new SpotExceptionsRecommendedPlanSpotsToDoDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDIwMjA4Nw=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 5711,
                    InventorySource = "TVB Syndication/ROS",
                    HouseIsci = "711N51AY18H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 09, 09, 08, 29, 23),
                    StationLegacyCallLetters = "WEVV",
                    Affiliate = "CBS",
                    MarketCode = 249,
                    MarketRank = 106,
                    ProgramName = "Funny You Should Ask",
                    ProgramGenre = "GAME SHOW",
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    IngestedMediaWeekId = 1,
                    SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
                        {
                            RecommendedPlanId = 332,
                            ExecutionTraceId = 2222,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 15000,
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
                            SpotDeliveredImpressions = 50000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDIwMjA4Nw=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        },
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 2223,
                            Rate = 0.00m,
                            AudienceName = "Women 25-54",
                            ContractedImpressions = 100000,
                            DeliveredImpressions = 25000,
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
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDIwMjA4Nw=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        }
                    }
                },
                new SpotExceptionsRecommendedPlanSpotsToDoDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDIwODcxNw=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6718,
                    InventorySource = "Ference POD - Scripps",
                    HouseIsci = "718MAY2918H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 09, 09, 09, 45, 23),
                    StationLegacyCallLetters = "NRIS",
                    Affiliate = "CW",
                    MarketCode = 200,
                    MarketRank = 130,
                    ProgramName = "The Steve Wilkos Show",
                    ProgramGenre = "REALITY TALK",
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    IngestedMediaWeekId = 1,
                    SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
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
                            SpotDeliveredImpressions = 35000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDIwODcxNw=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        },
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
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
                            SpotDeliveredImpressions = 45000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDIwODcxNw=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        }
                    }
                },
                new SpotExceptionsRecommendedPlanSpotsToDoDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDIyMzAzMw=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 2009,
                    InventorySource = "Business First AM",
                    HouseIsci = "009UPX0030H",
                    ClientIsci = "DUPX0030000H",
                    SpotLengthId = 2,
                    ProgramAirTime = new DateTime(2022, 09, 09, 11, 44, 35),
                    StationLegacyCallLetters = "WMNN",
                    Affiliate = "IND",
                    MarketCode = 140,
                    MarketRank = 118,
                    ProgramName = "Business First AM",
                    ProgramGenre = "INFORMATIONAL/NEWS",
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    IngestedMediaWeekId = 1,
                    SpotExceptionsRecommendedPlanDetailsToDo=new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
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
                            SpotDeliveredImpressions = 5000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDIyMzAzMw=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        },
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
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
                            SpotDeliveredImpressions = 55000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDIyMzAzMw=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        }
                    }
                },
                new SpotExceptionsRecommendedPlanSpotsToDoDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDM2NTgwMw=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6288,
                    InventorySource = "Sinclair Corp - Syn EM 6-10a",
                    HouseIsci = "288R2Y1F81H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 09, 09, 09, 14, 45),
                    StationLegacyCallLetters = "NVCW",
                    Affiliate = "IND",
                    MarketCode = 439,
                    MarketRank = 40,
                    ProgramName = "25 Words or Less",
                    ProgramGenre = "GAME SHOW",
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    IngestedMediaWeekId = 1,
                    SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
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
                            SpotDeliveredImpressions = 52000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDM2NTgwMw=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        },
                        new SpotExceptionsRecommendedPlanDetailsToDoDto
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
                            SpotDeliveredImpressions = 42000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDM2NTgwMw=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy"
                        }
                    }
                }
            };
            return spotExceptionsRecommendedPlans;
        }

        private List<SpotExceptionsRecommendedPlanSpotsDoneDto> _GetSpotExceptionsRecommendedPlansDoneMock()
        {
            var spotExceptionsRecommendedPlans = new List<SpotExceptionsRecommendedPlanSpotsDoneDto>
            {
                new SpotExceptionsRecommendedPlanSpotsDoneDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=+",
                    AmbiguityCode = 1,
                    ExecutionIdExternal = "220609090855BRt8EHXqSy",
                    EstimateId = 6840,
                    InventorySource = "Tegna",
                    HouseIsci = "840T42AY13H",
                    ClientIsci = "QMAY2913OS1H",
                    SpotLengthId = 3,
                    ProgramAirTime = new DateTime(2022, 09, 09, 08, 28, 28),
                    StationLegacyCallLetters = "WBNS",
                    Affiliate = "CBS",
                    MarketCode = 135,
                    MarketRank = 33,
                    ProgramName = "CBS Mornings",
                    ProgramGenre = "INFORMATIONAL/NEWS",
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    IngestedMediaWeekId = 1,
                    SpotExceptionsRecommendedPlanDetailsDone = new List<SpotExceptionsRecommendedPlanDetailsDoneDto>
                    {
                        new SpotExceptionsRecommendedPlanDetailsDoneDto
                        {
                            RecommendedPlanId = 334,
                            ExecutionTraceId = 73,
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
                            SpotDeliveredImpressions = 10000,
                            PlanTotalContractedImpressions = 50000,
                            PlanTotalDeliveredImpressions = 50000,
                            IngestedMediaWeekId = 1,
                            IngestedBy="Mock Data",
                            IngestedAt=DateTime.Now,
                            SpotUniqueHashExternal = "TE9DQUwtMTE0MDA3MDYxNg=+",
                            ExecutionIdExternal = "220609090855BRt8EHXqSy",
                            SpotExceptionsRecommendedPlanDoneDecisions = new SpotExceptionsRecommendedPlanSpotDecisionsDoneDto
                            {
                                DecidedBy = "Mock Data",
                                DecidedAt = DateTime.Now
                            }
                        }
                    }
                }
            };
            return spotExceptionsRecommendedPlans;
        }

        private List<SpotExceptionsOutOfSpecsToDoDto> _GetSpotExceptionsOutOfSpecsToDoMock()
        {
            var spotExceptionsOutOfSpecs = new List<SpotExceptionsOutOfSpecsToDoDto>
            {
                new SpotExceptionsOutOfSpecsToDoDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1ODgzODk5MA==",
                  HouseIsci = "613NM15082H",
                  ReasonCodeMessage="",
                  EstimateId= 191757,
                  IsciName="AB82VR58",
                  RecommendedPlanId= 216,
                  ProgramName="11 NEWS SUNDAY MORNING",
                  StationLegacyCallLetters="KSTP",
                  SpotLengthId= 11,
                  AudienceId= 430,
                  Product="Spotify",
                  DaypartCode="TDNS",
                  GenreName="News",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 09, 09),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  IngestedMediaWeekId = 1,
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
                new SpotExceptionsOutOfSpecsToDoDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDMwOA==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191758,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 217,
                  ProgramName="11 TV HILL",
                  StationLegacyCallLetters="KHGI",
                  SpotLengthId= 12,
                  AudienceId= 430,
                  DaypartCode="TDNS",
                  GenreName="News",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 09, 09),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  IngestedMediaWeekId = 1,
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
                new SpotExceptionsOutOfSpecsToDoDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0MDYzNg==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191759,
                  IsciName="AB21QR58",
                  RecommendedPlanId= 218,
                  ProgramName="11ALIVE NEWS AT NOON",
                  StationLegacyCallLetters="KWCH",
                  SpotLengthId= 11,
                  AudienceId= 430,
                  DaypartCode="TDNS",
                  GenreName="News",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 09, 09),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  IngestedMediaWeekId = 1,
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
                new SpotExceptionsOutOfSpecsToDoDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTA0NDAxOA==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191760,
                  IsciName="AB44NR58",
                  RecommendedPlanId= 219,
                  ProgramName="12 ANGRY MEN",
                  StationLegacyCallLetters="WDAY",
                  SpotLengthId= 10,
                  AudienceId= 429,
                  DaypartCode="ROSP",
                  GenreName="Movie",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 09, 09),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  IngestedMediaWeekId = 1,
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
                new SpotExceptionsOutOfSpecsToDoDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTA5ODgwOQ==",
                  HouseIsci = "613NM13290H",
                  ReasonCodeMessage="",
                  EstimateId= 191761,
                  IsciName="AB33PR58",
                  RecommendedPlanId= 220,
                  ProgramName="12 NEWS @ 11PM",
                  StationLegacyCallLetters="KPNX",
                  SpotLengthId= 10,
                  AudienceId= 428,
                  DaypartCode="EM",
                  GenreName="News",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 09, 09),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  IngestedMediaWeekId = 1,
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
                new SpotExceptionsOutOfSpecsToDoDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMDc5NA==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191762,
                  IsciName="AB79PR58",
                  RecommendedPlanId= 221,
                  ProgramName="12 NEWS TODAY",
                  StationLegacyCallLetters="KELO",
                  SpotLengthId= 09,
                  AudienceId= 427,
                  DaypartCode="EM",
                  GenreName="News",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 09, 09),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  IngestedMediaWeekId = 1,
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
                new SpotExceptionsOutOfSpecsToDoDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxMjQ4OQ==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191763,
                  IsciName="AB81GR58",
                  RecommendedPlanId= 222,
                  ProgramName="13ABC ACTION NEWS AT 5:00AM",
                  StationLegacyCallLetters="KXMC",
                  SpotLengthId= 08,
                  AudienceId= 426,
                  DaypartCode="EM",
                  GenreName="News",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 09, 09),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  IngestedMediaWeekId = 1,
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
                new SpotExceptionsOutOfSpecsToDoDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxNTkzNQ==",
                  HouseIsci = "OMGN1016000H",
                  ReasonCodeMessage="",
                  EstimateId= 191764,
                  IsciName="AB87GR58",
                  RecommendedPlanId= 223,
                  ProgramName="13WMAZ EYEWITNESS NEWS AT 6",
                  StationLegacyCallLetters="WTTV",
                  SpotLengthId= 07,
                  AudienceId= 425,
                  DaypartCode="EM",
                  GenreName="News",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 09, 09),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  IngestedMediaWeekId = 1,
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
                new SpotExceptionsOutOfSpecsToDoDto
                {
                  SpotUniqueHashExternal = "TE9DQUwtMTA1OTAxOTY3MA==",
                  HouseIsci = "289J76GN16H",
                  ReasonCodeMessage="",
                  EstimateId= 191765,
                  IsciName="AB83PR58",
                  RecommendedPlanId= 224,
                  ProgramName="WIN A DATE WITH TAD HAMILTON!",
                  StationLegacyCallLetters="WCCO",
                  SpotLengthId= 06,
                  AudienceId= 424,
                  DaypartCode="PT",
                  GenreName="Movie",
                  DaypartDetail = new DaypartDetailDto
                  {
                      Id = 70612,
                      Code = "MDN"
                  },
                  MarketCode = 120,
                  MarketRank = 50,
                  ProgramNetwork = "ABC",
                  ProgramAirTime = new DateTime(2022, 09, 09),
                  IngestedBy="Mock Data",
                  IngestedAt=DateTime.Now,
                  IngestedMediaWeekId = 1,
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

        private List<SpotExceptionsOutOfSpecsDoneDto> _GetSpotExceptionsOutOfSpecsDoneMock()
        {
            var spotExceptionsOutOfSpecs = new List<SpotExceptionsOutOfSpecsDoneDto>
            {
                new SpotExceptionsOutOfSpecsDoneDto
                {
                    SpotUniqueHashExternal = "TE9DQUwtMTA1ODgzNzk3Nw==",
                    HouseIsci = "612NM15082H",
                    ReasonCodeMessage="",
                    EstimateId= 191756,
                    IsciName="AB82TXT2H",
                    RecommendedPlanId= 215,
                    ProgramName="11 ALIVE SPORTS EXTRA",
                    StationLegacyCallLetters="KOB",
                    SpotLengthId= 12,
                    AudienceId= 431,
                    DaypartCode="EMN",
                    GenreName="Sports",
                    DaypartDetail = new DaypartDetailDto
                    {
                        Id = 70612,
                        Code = "MDN"
                    },
                    MarketCode = 120,
                    MarketRank = 50,
                    ProgramNetwork = "ABC",
                    ProgramAirTime = new DateTime(2022, 09, 09),
                    IngestedBy="Mock Data",
                    IngestedAt=DateTime.Now,
                    IngestedMediaWeekId = 1,
                    Impressions=10000,
                    SpotExceptionsOutOfSpecDoneDecision = new SpotExceptionsOutOfSpecDoneDecisionsDto
                    {
                        AcceptedAsInSpec=true,
                        DecisionNotes="",
                        DecidedBy = "Mock Data",
                        DecidedAt = DateTime.Now
                    },
                    SpotExceptionsOutOfSpecReasonCode = new SpotExceptionsOutOfSpecReasonCodeDto
                    {
                        Id = 7,
                        ReasonCode = 9,
                        Reason = "sIncorrect Time",
                        Label = "Daypart"
                    },
                    InventorySourceName = "Open Market",
                }
            };

            return spotExceptionsOutOfSpecs;
        }
    }
}
