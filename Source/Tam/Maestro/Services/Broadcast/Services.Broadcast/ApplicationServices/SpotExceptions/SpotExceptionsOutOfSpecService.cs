using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.ProgramMapping;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Plan;
using static log4net.Appender.RollingFileAppender;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsOutOfSpecService : IApplicationService
    {
        /// <summary>
        /// Gets the spot exceptions out of specs plans asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecsPlansRequest">The spot exceptions outof specs active plans request dto.</param>
        /// <returns></returns>
        Task<SpotExceptionsOutOfSpecPlansResultDto> GetSpotExceptionsOutOfSpecsPlansAsync(SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutOfSpecsPlansRequest);

        Task<SpotExceptionsOutOfSpecPlanSpotsResultDto> GetSpotExceptionsOutOfSpecSpotsAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);

        Task<List<string>> GetSpotExceptionsOutOfSpecSpotsInventorySourcesAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);

        Task<List<SpotExceptionsOutOfSpecReasonCodeResultDto>> GetSpotExceptionsOutOfSpecReasonCodesAsync();

        Task<List<string>> GetSpotExceptionsOutOfSpecMarketsAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);

        Task<List<SpotExceptionsOutOfSpecProgramsDto>> GetSpotExceptionsOutOfSpecProgramsAsync(string programNameQuery, string userName);

        Task<List<string>> GetSpotExceptionsOutOfSpecAdvertisersAsync(SpotExceptionsOutOfSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest);

        Task<List<string>> GetSpotExceptionsOutOfSpecStationsAsync(SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest);

        Task<bool> HandleSaveSpotExceptionsOutOfSpecAsync(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest, string userName);
    }

    public class SpotExceptionsOutOfSpecService : BroadcastBaseClass, ISpotExceptionsOutOfSpecService
    {
        const string flightStartDateFormat = "MM/dd";
        const string flightEndDateFormat = "MM/dd/yyyy";

        private readonly ISpotExceptionsOutOfSpecRepository _SpotExceptionsOutOfSpecRepository;
        private readonly IPlanRepository _PlanRepository;

        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IAabEngine _AabEngine;
        private readonly IGenreCache _GenreCache;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        public SpotExceptionsOutOfSpecService(
            IDataRepositoryFactory dataRepositoryFactory,
            IDateTimeEngine dateTimeEngine,
            IAabEngine aabEngine,
            IGenreCache genreCache,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsOutOfSpecRepository = dataRepositoryFactory.GetDataRepository<ISpotExceptionsOutOfSpecRepository>();
            _PlanRepository = dataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _DateTimeEngine = dateTimeEngine;
            _AabEngine = aabEngine;
            _GenreCache = genreCache;
            _FeatureToggleHelper = featureToggleHelper;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsOutOfSpecPlansResultDto> GetSpotExceptionsOutOfSpecsPlansAsync(SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutOfSpecsPlansReques)
        {
            var outOfSpecPlans = new SpotExceptionsOutOfSpecPlansResultDto();

            try
            {
                var outOfSpecToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecToDoAsync(spotExceptionsOutOfSpecsPlansReques.WeekStartDate, spotExceptionsOutOfSpecsPlansReques.WeekEndDate);

                if (outOfSpecToDo?.Any() ?? false)
                {
                    outOfSpecPlans.Active = outOfSpecToDo.GroupBy(recommendedPlan => recommendedPlan.RecommendedPlanId)
                    .Select(recommendedPlan =>
                    {
                        var planDetails = recommendedPlan.First();
                        var advertiserName = _GetAdvertiserName(planDetails.AdvertiserMasterId);
                        return new SpotExceptionsOutOfSpecToDoPlansDto
                        {
                            PlanId = planDetails.PlanId,
                            AdvertiserName = advertiserName,
                            PlanName = planDetails.RecommendedPlanName,
                            AffectedSpotsCount = recommendedPlan.Count(),
                            Impressions = recommendedPlan.Select(i => i.Impressions).Sum() / 1000,
                            SyncedTimestamp = null,
                            SpotLengthString = planDetails.SpotLength != null ? $":{planDetails.SpotLength.Length}" : null,
                            AudienceName = planDetails.Audience?.Name,
                            FlightString = planDetails.FlightStartDate.HasValue && planDetails.FlightEndDate.HasValue ? $"{Convert.ToDateTime(planDetails.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(planDetails.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(planDetails.FlightStartDate), Convert.ToDateTime(planDetails.FlightEndDate)).ToString() + " " + "Weeks"})" : null,
                        };
                    }).ToList();
                }

                var outOfSpecDone = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecDoneAsync(spotExceptionsOutOfSpecsPlansReques.WeekStartDate, spotExceptionsOutOfSpecsPlansReques.WeekEndDate);

                if (outOfSpecDone?.Any() ?? false)
                {
                    outOfSpecPlans.Completed = outOfSpecDone.GroupBy(recommendedPlan => recommendedPlan.RecommendedPlanId)
                    .Select(recommendedPlan =>
                    {
                        var planDetails = recommendedPlan.First();
                        var advertiserName = _GetAdvertiserName(planDetails.AdvertiserMasterId);
                        return new SpotExceptionsOutOfSpecDonePlansDto
                        {
                            PlanId = planDetails.PlanId,
                            AdvertiserName = advertiserName,
                            PlanName = planDetails.RecommendedPlanName,
                            AffectedSpotsCount = recommendedPlan.Count(),
                            Impressions = recommendedPlan.Select(i => i.Impressions).Sum() / 1000,
                            SyncedTimestamp = null,
                            SpotLengthString = planDetails.SpotLength != null ? $":{planDetails.SpotLength.Length}" : null,
                            AudienceName = planDetails.Audience?.Name,
                            FlightString = planDetails.FlightStartDate.HasValue && planDetails.FlightEndDate.HasValue ? $"{Convert.ToDateTime(planDetails.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(planDetails.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(planDetails.FlightStartDate), Convert.ToDateTime(planDetails.FlightEndDate)).ToString() + " " + "Weeks"})" : null,
                        };
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return outOfSpecPlans;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsOutOfSpecPlanSpotsResultDto> GetSpotExceptionsOutOfSpecSpotsAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var outOfSpecPlanSpots = new SpotExceptionsOutOfSpecPlanSpotsResultDto();

            try
            {
                var outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsToDoAsync(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);

                if (outOfSpecSpotsToDo?.Any() ?? false)
                {
                    var planIds = outOfSpecSpotsToDo.Select(p => p.PlanId).Distinct().ToList();
                    var daypartsList = _PlanRepository.GetPlanDaypartsByPlanIds(planIds);

                    outOfSpecPlanSpots.Active = outOfSpecSpotsToDo
                    .Select(activePlan =>
                    {
                        return new SpotExceptionsOutOfSpecPlanSpotsDto
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
                }

                var outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsDoneAsync(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);

                if (outOfSpecSpotsDone?.Any() ?? false)
                {
                    var planIds = outOfSpecSpotsDone.Select(p => p.PlanId).Distinct().ToList();
                    var daypartsList = _PlanRepository.GetPlanDaypartsByPlanIds(planIds);

                    outOfSpecPlanSpots.Queued = outOfSpecSpotsDone.Where(syncedSpot => syncedSpot.SpotExceptionsOutOfSpecDoneDecision.SyncedAt == null)
                    .Select(queuedPlan =>
                    {
                        return new SpotExceptionsOutOfSpecDonePlanSpotsDto
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
                            GenreName = queuedPlan.SpotExceptionsOutOfSpecDoneDecision.GenreName == null ? queuedPlan.GenreName : queuedPlan.SpotExceptionsOutOfSpecDoneDecision.GenreName,
                            HouseIsci = queuedPlan.HouseIsci,
                            ClientIsci = queuedPlan.IsciName,
                            ProgramAirDate = queuedPlan.ProgramAirTime.ToShortDateString(),
                            ProgramAirTime = queuedPlan.ProgramAirTime.ToLongTimeString(),
                            FlightEndDate = queuedPlan.FlightEndDate.ToString(),
                            ProgramName = queuedPlan.SpotExceptionsOutOfSpecDoneDecision.ProgramName == null ? queuedPlan.ProgramName : queuedPlan.SpotExceptionsOutOfSpecDoneDecision.ProgramName,
                            SpotLengthString = queuedPlan.SpotLength != null ? $":{queuedPlan.SpotLength.Length}" : null,
                            DaypartCode = queuedPlan.SpotExceptionsOutOfSpecDoneDecision.DaypartCode == null ? queuedPlan.DaypartCode : queuedPlan.SpotExceptionsOutOfSpecDoneDecision.DaypartCode,
                            DecisionString = queuedPlan.SpotExceptionsOutOfSpecDoneDecision.DecisionNotes,
                            Comments = queuedPlan.Comments,
                            PlanDaypartCodes = daypartsList.Where(d => d.PlanId == queuedPlan.PlanId).Select(s => s.Code).Distinct().ToList(),
                            InventorySourceName = queuedPlan.InventorySourceName
                        };
                    }).ToList();

                    outOfSpecPlanSpots.Synced = outOfSpecSpotsDone.Where(syncedSpot => syncedSpot.SpotExceptionsOutOfSpecDoneDecision.SyncedAt != null)
                    .Select(syncedPlan =>
                    {
                        return new SpotExceptionsOutOfSpecDonePlanSpotsDto
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
                            GenreName = syncedPlan.SpotExceptionsOutOfSpecDoneDecision.GenreName == null ? syncedPlan.GenreName : syncedPlan.SpotExceptionsOutOfSpecDoneDecision.GenreName,
                            HouseIsci = syncedPlan.HouseIsci,
                            ClientIsci = syncedPlan.IsciName,
                            ProgramAirDate = syncedPlan.ProgramAirTime.ToShortDateString(),
                            ProgramAirTime = syncedPlan.ProgramAirTime.ToLongTimeString(),
                            FlightEndDate = syncedPlan.FlightEndDate.ToString(),
                            ProgramName = syncedPlan.SpotExceptionsOutOfSpecDoneDecision.ProgramName == null ? syncedPlan.ProgramName : syncedPlan.SpotExceptionsOutOfSpecDoneDecision.ProgramName,
                            SpotLengthString = syncedPlan.SpotLength != null ? $":{syncedPlan.SpotLength.Length}" : null,
                            DaypartCode = syncedPlan.SpotExceptionsOutOfSpecDoneDecision.DaypartCode == null ? syncedPlan.DaypartCode : syncedPlan.SpotExceptionsOutOfSpecDoneDecision.DaypartCode,
                            DecisionString = syncedPlan.SpotExceptionsOutOfSpecDoneDecision.DecisionNotes,
                            SyncedTimestamp = syncedPlan.SpotExceptionsOutOfSpecDoneDecision.SyncedAt.ToString(),
                            Comments = syncedPlan.Comments,
                            PlanDaypartCodes = daypartsList.Where(d => d.PlanId == syncedPlan.PlanId).Select(s => s.Code).Distinct().ToList(),
                            InventorySourceName = syncedPlan.InventorySourceName
                        };
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }
            return outOfSpecPlanSpots;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetSpotExceptionsOutOfSpecSpotsInventorySourcesAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var outOfSpecSpotsToDo = new List<string>();
            var outOfSpecSpotsDone = new List<string>();

            try
            {
                outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsToDoInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);
                outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsDoneInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            var inventorySources = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).Distinct().OrderBy(inventorySource => inventorySource).ToList();
            return inventorySources;
        }

        /// <inheritdoc />
        public async Task<List<SpotExceptionsOutOfSpecReasonCodeResultDto>> GetSpotExceptionsOutOfSpecReasonCodesAsync()
        {
            var spotExceptionsOutOfSpecReasonCodeResults = new List<SpotExceptionsOutOfSpecReasonCodeResultDto>();

            try
            {
                var spotExceptionsOutOfSpecReasonCodes = await _SpotExceptionsOutOfSpecRepository.GetSpotExceptionsOutOfSpecReasonCodesAsync();

                spotExceptionsOutOfSpecReasonCodeResults = spotExceptionsOutOfSpecReasonCodes.Select(spotExceptionsOutOfSpecReasonCode => new SpotExceptionsOutOfSpecReasonCodeResultDto
                {
                    Id = spotExceptionsOutOfSpecReasonCode.Id,
                    ReasonCode = spotExceptionsOutOfSpecReasonCode.ReasonCode,
                    Description = spotExceptionsOutOfSpecReasonCode.Reason,
                    Label = spotExceptionsOutOfSpecReasonCode.Label
                }).ToList();
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return spotExceptionsOutOfSpecReasonCodeResults;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetSpotExceptionsOutOfSpecMarketsAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var outOfSpecSpotsToDo = new List<string>();
            var outOfSpecSpotsDone = new List<string>();

            try
            {
                outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsToDoMarketsAsync(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);
                outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsDoneMarketsAsync(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            var markets = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).Distinct().OrderBy(market => market).ToList();

            return markets;
        }

        /// <inheritdoc />
        public async Task<List<SpotExceptionsOutOfSpecProgramsDto>> GetSpotExceptionsOutOfSpecProgramsAsync(string programNameQuery, string userName)
        {
            SearchRequestProgramDto searchRequest = new SearchRequestProgramDto();
            searchRequest.ProgramName = programNameQuery;
            var programList = new List<SpotExceptionsOutOfSpecProgramsDto>();

            try
            {
                programList = await _LoadProgramFromProgramsAsync(searchRequest);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return programList;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetSpotExceptionsOutOfSpecAdvertisersAsync(SpotExceptionsOutOfSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest)
        {
            List<Guid?> outOfSpecSpotsToDo = null;
            List<Guid?> outOfSpecSpotsDone = null;
            List<string> advertiserName = new List<string>();

            try
            {
                outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsToDoAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest.WeekStartDate, spotExceptionsOutofSpecAdvertisersRequest.WeekEndDate);
                outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsDoneAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest.WeekStartDate, spotExceptionsOutofSpecAdvertisersRequest.WeekEndDate);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            var advertisersMasterIds = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).Distinct().ToList();
            if (advertisersMasterIds.Any())
            {
                advertiserName = advertisersMasterIds.Select(n => _GetAdvertiserName(n.Value) ?? "Unknown").ToList();
            }

            advertiserName = advertiserName.OrderBy(n => n).ToList();
            return advertiserName;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetSpotExceptionsOutOfSpecStationsAsync(SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest)
        {
            List<string> outOfSpecStationsToDo = null;
            List<string> outOfSpecStationsDone = null;
            List<string> outOfSpecStations = new List<string>();

            try
            {
                outOfSpecStationsToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsToDoStationsAsync(spotExceptionsOutofSpecsStationRequest.WeekStartDate, spotExceptionsOutofSpecsStationRequest.WeekEndDate);
                outOfSpecStationsDone = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsDoneStationsAsync(spotExceptionsOutofSpecsStationRequest.WeekStartDate, spotExceptionsOutofSpecsStationRequest.WeekEndDate);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            outOfSpecStations = outOfSpecStationsToDo.Concat(outOfSpecStationsDone).Distinct().OrderBy(s => s).ToList();
            return outOfSpecStations;
        }

        public async Task<bool> HandleSaveSpotExceptionsOutOfSpecAsync(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest, string userName)
        {
            var decidedAt = _DateTimeEngine.GetCurrentMoment();
            bool isSaved = false;

            try
            {
                foreach (var spotExceptionsOutOfSpec in spotExceptionsOutOfSpecSaveRequest.Decisions)
                {
                    var decisionsPlans = await _GetSpotExceptionsRecommendedPlanByDecisionAsync(spotExceptionsOutOfSpec);

                    var countAdded = await _AddSpotExceptionsRecommendedPlanToDoneAsync(decisionsPlans);

                    var countDeleted = await _DeleteSpotExceptionsOutOfSpecFromToDoAsync(decisionsPlans.Id);

                    isSaved = await _SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(spotExceptionsOutOfSpec, userName, decidedAt);
                }
            }
            catch (Exception ex)
            {
                var msg = $"Could not save the decision";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        private async Task<SpotExceptionsOutOfSpecsToDoDto> _GetSpotExceptionsRecommendedPlanByDecisionAsync(SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto spotExceptionsOutOfSpec)
        {
            var outOfSpec = new SpotExceptionsOutOfSpecsToDoDto();
            try
            {
                outOfSpec = await _SpotExceptionsOutOfSpecRepository.GetSpotExceptionOutOfSpecByDecisionAsync(spotExceptionsOutOfSpec);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return outOfSpec;
        }

        private async Task<int> _AddSpotExceptionsRecommendedPlanToDoneAsync(SpotExceptionsOutOfSpecsToDoDto decisionsPlans)
        {
            int addedCount;

            try
            {
                var transformedToDone = await _TransformToDoToDone(decisionsPlans);

                addedCount = await _SpotExceptionsOutOfSpecRepository.AddSpotExceptionsOutOfSpecToDoneAsync(transformedToDone);
            }
            catch (Exception ex)
            {
                var msg = $"Could not save to the the Database";
                throw new CadentException(msg, ex);
            }

            return addedCount;
        }

        private async Task<int> _DeleteSpotExceptionsOutOfSpecFromToDoAsync(int spotExceptionOutOfSpecId)
        {
            int deletedCunt;
            try
            {
                deletedCunt = await _SpotExceptionsOutOfSpecRepository.DeleteSpotExceptionsOutOfSpecFromToDoAsync(spotExceptionOutOfSpecId);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return deletedCunt;
        }

        private async Task<bool> _SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto spotExceptionsOutOfSpec, string userName, DateTime decidedAt)
        {
            bool isSpotExceptionsOutOfSpecDoneDecisionSaved;
            try
            {
                var spotExceptionsOutOfSpecDoneDecision = new SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto
                {
                    Id = spotExceptionsOutOfSpec.Id,
                    AcceptAsInSpec = spotExceptionsOutOfSpec.AcceptAsInSpec,
                    Comments = spotExceptionsOutOfSpec.Comments,
                    ProgramName = spotExceptionsOutOfSpec.ProgramName,
                    GenreName = spotExceptionsOutOfSpec.GenreName,
                    DaypartCode = spotExceptionsOutOfSpec.DaypartCode
                };

                isSpotExceptionsOutOfSpecDoneDecisionSaved = await _SpotExceptionsOutOfSpecRepository.SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(spotExceptionsOutOfSpecDoneDecision, userName, decidedAt);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return isSpotExceptionsOutOfSpecDoneDecisionSaved;
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

        private async Task<List<SpotExceptionsOutOfSpecProgramsDto>> _LoadProgramFromProgramsAsync(SearchRequestProgramDto searchRequest)
        {
            List<string> programNames = new List<string>();
            var result = new List<SpotExceptionsOutOfSpecProgramsDto>();

            try
            {
                var programs = await _SpotExceptionsOutOfSpecRepository.FindProgramFromProgramsAsync(searchRequest.ProgramName);
                var programsSpotExceptionDecisions = await _SpotExceptionsOutOfSpecRepository.FindProgramFromSpotExceptionDecisionsAsync(searchRequest.ProgramName);

                if (programsSpotExceptionDecisions != null)
                {
                    programs.AddRange(programsSpotExceptionDecisions);
                }

                _RemoveVariousAndUnmatchedFromPrograms(programs);

                programNames = programs.Select(x => x.OfficialProgramName).Distinct().ToList();
                foreach (var program in programNames)
                {
                    var listOfProgramNames = programs.Where(x => x.OfficialProgramName.ToLower() == program.ToLower()).ToList();
                    SpotExceptionsOutOfSpecProgramsDto spotExceptionsOutOfSpecProgram = new SpotExceptionsOutOfSpecProgramsDto();
                    spotExceptionsOutOfSpecProgram.ProgramName = program;
                    foreach (var programName in listOfProgramNames)
                    {
                        spotExceptionsOutOfSpecProgram.Genres.Add(_GenreCache.GetGenreLookupDtoById(programName.GenreId).Display);
                    }
                    result.Add(spotExceptionsOutOfSpecProgram);
                }
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }
            return result;
        }

        private void _RemoveVariousAndUnmatchedFromPrograms(List<ProgramNameDto> result)
        {
            result.RemoveAll(x => _GenreCache.GetGenreLookupDtoById(x.GenreId).Display.Equals("Various", StringComparison.OrdinalIgnoreCase)
                    || x.OfficialProgramName.Equals("Unmatched", StringComparison.OrdinalIgnoreCase));
        }

        private async Task<SpotExceptionsOutOfSpecsDoneDto> _TransformToDoToDone(SpotExceptionsOutOfSpecsToDoDto todoPlan)
        {
            var donePlan = new SpotExceptionsOutOfSpecsDoneDto
            {
                Id = todoPlan.Id,
                SpotUniqueHashExternal = todoPlan.SpotUniqueHashExternal,
                ReasonCodeMessage = todoPlan.ReasonCodeMessage,
                EstimateId = todoPlan.EstimateId,
                IsciName = todoPlan.IsciName,
                HouseIsci = todoPlan.HouseIsci,
                RecommendedPlanId = todoPlan.RecommendedPlanId,
                RecommendedPlanName = todoPlan.RecommendedPlanName,
                ProgramName = todoPlan.ProgramName,
                StationLegacyCallLetters = todoPlan.StationLegacyCallLetters,
                DaypartCode = todoPlan.DaypartCode,
                GenreName = todoPlan.GenreName,
                Affiliate = todoPlan.Affiliate,
                Market = todoPlan.Market,
                SpotLength = todoPlan.SpotLength,
                Audience = todoPlan.Audience,
                ProgramAirTime = todoPlan.ProgramAirTime,
                IngestedBy = todoPlan.IngestedBy,
                IngestedAt = todoPlan.IngestedAt,
                IngestedMediaWeekId = todoPlan.IngestedMediaWeekId,
                Impressions = todoPlan.Impressions,
                PlanId = todoPlan.PlanId,
                FlightStartDate = todoPlan.FlightStartDate,
                FlightEndDate = todoPlan.FlightEndDate,
                AdvertiserMasterId = todoPlan.AdvertiserMasterId,
                Product = todoPlan.Product,
                SpotExceptionsOutOfSpecReasonCode = todoPlan.SpotExceptionsOutOfSpecReasonCode,
                MarketCode = todoPlan.MarketCode,
                MarketRank = todoPlan.MarketRank,
                Comments = todoPlan.Comments,
                InventorySourceName = todoPlan.InventorySourceName
            };

            return donePlan;
        }
    }
}
