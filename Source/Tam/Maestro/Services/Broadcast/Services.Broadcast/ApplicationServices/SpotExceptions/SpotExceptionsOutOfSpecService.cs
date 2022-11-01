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

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsOutOfSpecService : IApplicationService
    {
        /// <summary>
        /// Gets the spot exceptions out of specs plans asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecsPlansRequest">The spot exceptions outof specs active plans request dto.</param>
        /// <returns></returns>
        Task<SpotExceptionsOutOfSpecGroupingResults> GetSpotExceptionsOutOfSpecGroupingAsync(SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutOfSpecsPlansRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec spots asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest">The spot exceptions out of spec spots request.</param>
        /// <returns></returns>
        Task<SpotExceptionsOutOfSpecSpotsResultDto> GetSpotExceptionsOutOfSpecSpotsAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec spots inventory sources asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest">The spot exceptions out of spec spots request.</param>
        /// <returns></returns>
        Task<List<string>> GetSpotExceptionsOutOfSpecSpotsInventorySourcesAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec reason codes asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<List<SpotExceptionsOutOfSpecReasonCodeResultDto>> GetSpotExceptionsOutOfSpecReasonCodesAsync();

        /// <summary>
        /// Gets the spot exceptions out of spec markets asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest">The spot exceptions out of spec spots request.</param>
        /// <returns></returns>
        Task<List<string>> GetSpotExceptionsOutOfSpecMarketsAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec programs asynchronous.
        /// </summary>
        /// <param name="programNameQuery">The program name query.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        Task<List<SpotExceptionsOutOfSpecProgramsDto>> GetSpotExceptionsOutOfSpecProgramsAsync(string programNameQuery, string userName);

        /// <summary>
        /// Gets the spot exceptions out of spec advertisers asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutofSpecAdvertisersRequest">The spot exceptions outof spec advertisers request.</param>
        /// <returns></returns>
        Task<List<string>> GetSpotExceptionsOutOfSpecAdvertisersAsync(SpotExceptionsOutOfSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest);

        /// <summary>
        /// Gets the spot exceptions out of spec stations asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutofSpecsStationRequest">The spot exceptions outof specs station request.</param>
        /// <returns></returns>
        Task<List<string>> GetSpotExceptionsOutOfSpecStationsAsync(SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest);

        /// <summary>
        /// Handles the save spot exceptions out of spec asynchronous.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSaveRequest">The spot exceptions out of spec save request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
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
        public async Task<SpotExceptionsOutOfSpecGroupingResults> GetSpotExceptionsOutOfSpecGroupingAsync(SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutOfSpecsPlansRequest)
        {
            var outOfSpecPlans = new SpotExceptionsOutOfSpecGroupingResults();

            try
            {
                _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Groupings");
                var outOfSpecToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecGroupingToDoAsync(spotExceptionsOutOfSpecsPlansRequest.WeekStartDate, spotExceptionsOutOfSpecsPlansRequest.WeekEndDate);
                var outOfSpecDone = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecGroupingDoneAsync(spotExceptionsOutOfSpecsPlansRequest.WeekStartDate, spotExceptionsOutOfSpecsPlansRequest.WeekEndDate);

                if (outOfSpecToDo?.Any() ?? false)
                {
                    outOfSpecPlans.Active = outOfSpecToDo.Select(x =>
                    {
                        return new SpotExceptionsOutOfSpecGroupingToDoResults
                        {
                            PlanId = x.PlanId,
                            AdvertiserName = _GetAdvertiserName(x.AdvertiserMasterId),
                            PlanName = x.PlanName,
                            AffectedSpotsCount = x.AffectedSpotsCount,
                            Impressions = x.Impressions / 1000,
                            SyncedTimestamp = null,
                            SpotLengthString = string.Join(", ", x.SpotLengths.OrderBy(y => y.Length).Select(spotLength => $":{spotLength.Length}")),
                            AudienceName = x.AudienceName,
                            FlightString = $"{Convert.ToDateTime(x.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(x.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(x.FlightStartDate), Convert.ToDateTime(x.FlightEndDate)).ToString() + " " + "Weeks"})"
                        };
                    }).ToList();
                }

                if (outOfSpecDone?.Any() ?? false)
                {
                    outOfSpecPlans.Completed = outOfSpecDone.Select(x =>
                    {
                        return new SpotExceptionsOutOfSpecGroupingDoneResults
                        {
                            PlanId = x.PlanId,
                            AdvertiserName = _GetAdvertiserName(x.AdvertiserMasterId),
                            PlanName = x.PlanName,
                            AffectedSpotsCount = x.AffectedSpotsCount,
                            Impressions = x.Impressions / 1000,
                            SyncedTimestamp = null,
                            SpotLengthString = string.Join(", ", x.SpotLengths.OrderBy(y => y.Length).Select(spotLength => $":{spotLength.Length}")),
                            AudienceName = x.AudienceName,
                            FlightString = $"{Convert.ToDateTime(x.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(x.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(x.FlightStartDate), Convert.ToDateTime(x.FlightEndDate)).ToString() + " " + "Weeks"})"
                        };
                    }).ToList();
                }
                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Groupings");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Groupings";
                throw new CadentException(msg, ex);
            }

            return outOfSpecPlans;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsOutOfSpecSpotsResultDto> GetSpotExceptionsOutOfSpecSpotsAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var outOfSpecPlanSpots = new SpotExceptionsOutOfSpecSpotsResultDto();

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

        /// <inheritdoc />
        public async Task<bool> HandleSaveSpotExceptionsOutOfSpecAsync(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest, string userName)
        {
            var decidedAt = _DateTimeEngine.GetCurrentMoment();
            bool isSaved = false;

            try
            {
                foreach (var spotExceptionsOutOfSpec in spotExceptionsOutOfSpecSaveRequest.Decisions)
                {

                    var outOfSpecToDo = await _SpotExceptionsOutOfSpecRepository.GetSpotExceptionOutOfSpecByDecisionToDoAsync(spotExceptionsOutOfSpec);
                    var outOfSpecDone = await _SpotExceptionsOutOfSpecRepository.GetSpotExceptionOutOfSpecByDecisionDoneAsync(spotExceptionsOutOfSpec);

                    if (outOfSpecToDo == null && outOfSpecDone == null)
                    {
                        return false;
                    }

                    if (outOfSpecToDo != null)
                    {
                        if (spotExceptionsOutOfSpec.Comments == null)
                        {
                            isSaved = await _SaveOutOfSpecDecisionsToDoAsync(spotExceptionsOutOfSpec, outOfSpecToDo, userName);
                        }
                        else
                        {
                            isSaved = await _SaveOutOfSpecCommentsToDoAsync(spotExceptionsOutOfSpec);
                        }
                    }
                    else
                    {
                        if (spotExceptionsOutOfSpec.Comments == null)
                        {
                            isSaved = await _SaveOutOfSpecDecisionsDoneAsync(spotExceptionsOutOfSpec, outOfSpecDone, userName);
                        }
                        else
                        {
                            isSaved = await _SaveOutOfSpecCommentsDoneAsync(spotExceptionsOutOfSpec);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = $"Could not save the decision";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        private async Task<bool> _SaveOutOfSpecDecisionsToDoAsync(SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto spotExceptionsOutOfSpec, SpotExceptionsOutOfSpecsToDoDto outOfSpecToDo, string userName)
        {
            bool isMoved = false;
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            _LogInfo($"Starting: Moving the Spot Exception Plan by Decision to Done");
            try
            {
                isMoved = await _SpotExceptionsOutOfSpecRepository.SaveSpotExceptionsOutOfSpecToDoDecisionsAsync(spotExceptionsOutOfSpec, outOfSpecToDo, userName, currentDate);


                _LogInfo($"Finished: Moving the Spot Exception Plan by Decision to Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not move Spot Exception Recommended Plan to Done";
                throw new CadentException(msg, ex);
            }

            return isMoved;
        }

        private async Task<bool> _SaveOutOfSpecCommentsToDoAsync(SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto spotExceptionsOutOfSpec)
        {
            bool isCommentSaved;

            _LogInfo($"Starting: Saving Comments for the ToDo");
            try
            {
                var spotExceptionsOutOfSpecToDo = new SpotExceptionsOutOfSpecsToDoDto
                {
                    Id = spotExceptionsOutOfSpec.Id,
                    Comments = spotExceptionsOutOfSpec.Comments
                };

                isCommentSaved = await _SpotExceptionsOutOfSpecRepository.SaveOutOfSpecCommentsToDoAsync(spotExceptionsOutOfSpecToDo);
                _LogInfo($"Starting: Saving Comments for the ToDo");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save Comments for the ToDo";
                throw new CadentException(msg, ex);
            }

            return isCommentSaved;
        }

        private async Task<bool> _SaveOutOfSpecDecisionsDoneAsync(SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto spotExceptionsOutOfSpecSaveRequest, SpotExceptionsOutOfSpecsDoneDto outOfSpecToDo, string userName)
        {
            bool isSpotExceptionsOutOfSpecDoneDecisionSaved;
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            _LogInfo($"Starting: Saving decisions for the Done");
            try
            {
                var spotExceptionsOutOfSpecDoneDecision = new SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto
                {
                    Id = outOfSpecToDo.Id,
                    AcceptAsInSpec = spotExceptionsOutOfSpecSaveRequest.AcceptAsInSpec,
                    ProgramName = outOfSpecToDo.ProgramName,
                    GenreName = outOfSpecToDo.GenreName,
                    DaypartCode = outOfSpecToDo.DaypartCode
                };

                isSpotExceptionsOutOfSpecDoneDecisionSaved = await _SpotExceptionsOutOfSpecRepository.SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(spotExceptionsOutOfSpecDoneDecision, userName, currentDate);
                _LogInfo($"Starting: Saving decisions for the Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save decisions for the Done";
                throw new CadentException(msg, ex);
            }

            return isSpotExceptionsOutOfSpecDoneDecisionSaved;
        }

        private async Task<bool> _SaveOutOfSpecCommentsDoneAsync(SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto spotExceptionsOutOfSpec)
        {
            bool isCommentSaved;
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            _LogInfo($"Starting: Saving Comments for the Done");
            try
            {
                var spotExceptionsOutOfSpecDone = new SpotExceptionsOutOfSpecsDoneDto
                {
                    Id = spotExceptionsOutOfSpec.Id,
                    Comments = spotExceptionsOutOfSpec.Comments
                };

                isCommentSaved = await _SpotExceptionsOutOfSpecRepository.SaveOutOfSpecCommentsDoneAsync(spotExceptionsOutOfSpecDone);
                _LogInfo($"Starting: Saving Comments for the Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save Comments for the Done";
                throw new CadentException(msg, ex);
            }

            return isCommentSaved;
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
    }
}
