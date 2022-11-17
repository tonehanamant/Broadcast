﻿using Common.Services.ApplicationServices;
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
using System.Web.WebPages;
using Tam.Maestro.Common.DataLayer;

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
        const int fourHundred = 400;

        private readonly ISpotExceptionsOutOfSpecRepository _SpotExceptionsOutOfSpecRepository;
        private readonly IPlanRepository _PlanRepository;

        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IAabEngine _AabEngine;
        private readonly IGenreCache _GenreCache;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly Lazy<bool> _IsSpotExceptionEnabled;

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
            _IsSpotExceptionEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SPOT_EXCEPTIONS));
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
                            Impressions = Math.Floor(x.Impressions / 1000),
                            SyncedTimestamp = null,
                            SpotLengthString = string.Join(", ", x.SpotLengths.OrderBy(y => y.Length).Select(spotLength => $":{spotLength.Length}")),
                            AudienceName = x.AudienceName,
                            FlightString = $"{Convert.ToDateTime(x.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(x.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(x.FlightStartDate), Convert.ToDateTime(x.FlightEndDate)).ToString() + " " + "Weeks"})"
                        };
                    }).OrderBy(x => x.AdvertiserName).ThenBy(x => x.PlanName).ToList();
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
                            Impressions = Math.Floor(x.Impressions / 1000),
                            SyncedTimestamp = x.SyncedTimestamp,
                            SpotLengthString = string.Join(", ", x.SpotLengths.OrderBy(y => y.Length).Select(spotLength => $":{spotLength.Length}")),
                            AudienceName = x.AudienceName,
                            FlightString = $"{Convert.ToDateTime(x.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(x.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(x.FlightStartDate), Convert.ToDateTime(x.FlightEndDate)).ToString() + " " + "Weeks"})"
                        };
                    }).OrderBy(x => x.AdvertiserName).ThenBy(x => x.PlanName).ToList();
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
            int marketRank = 0;
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
                        marketRank = activePlan.MarketRank == null ? 0 : activePlan.MarketRank.Value;
                        return new SpotExceptionsOutOfSpecPlanSpotsDto
                        {
                            Id = activePlan.Id,
                            EstimateId = activePlan.EstimateId,
                            Reason = activePlan.SpotExceptionsOutOfSpecReasonCode.Reason,
                            MarketRank = activePlan.MarketRank,                            
                            DMA = _IsSpotExceptionEnabled.Value? marketRank + fourHundred: activePlan.DMA,
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
                        marketRank = queuedPlan.MarketRank == null ? 0 : queuedPlan.MarketRank.Value;
                        return new SpotExceptionsOutOfSpecDonePlanSpotsDto
                        {
                            Id = queuedPlan.Id,
                            EstimateId = queuedPlan.EstimateId,
                            Reason = queuedPlan.SpotExceptionsOutOfSpecReasonCode.Reason,
                            MarketRank = queuedPlan.MarketRank,
                            DMA = _IsSpotExceptionEnabled.Value ? marketRank + fourHundred : queuedPlan.DMA,
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
                        marketRank = syncedPlan.MarketRank == null ? 0 : syncedPlan.MarketRank.Value;
                        return new SpotExceptionsOutOfSpecDonePlanSpotsDto
                        {
                            Id = syncedPlan.Id,
                            EstimateId = syncedPlan.EstimateId,
                            Reason = syncedPlan.SpotExceptionsOutOfSpecReasonCode.Reason,
                            MarketRank = syncedPlan.MarketRank,
                            DMA = _IsSpotExceptionEnabled.Value ? marketRank + fourHundred : syncedPlan.DMA,
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
            List<string> inventorySources = new List<string>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Inventory Sources");
            try
            {
                outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsToDoInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);
                outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsDoneInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);

                inventorySources = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).Distinct().OrderBy(inventorySource => inventorySource).ToList();
                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Inventory Sources");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Inventory Sources";
                throw new CadentException(msg, ex);
            }

            return inventorySources;
        }

        /// <inheritdoc />
        public async Task<List<SpotExceptionsOutOfSpecReasonCodeResultDto>> GetSpotExceptionsOutOfSpecReasonCodesAsync()
        {
            var spotExceptionsOutOfSpecReasonCodeResults = new List<SpotExceptionsOutOfSpecReasonCodeResultDto>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Reason Codes");
            try
            {
                var spotExceptionsOutOfSpecReasonCodes = await _SpotExceptionsOutOfSpecRepository.GetSpotExceptionsOutOfSpecReasonCodesAsync();

                spotExceptionsOutOfSpecReasonCodeResults = spotExceptionsOutOfSpecReasonCodes.Select(spotExceptionsOutOfSpecReasonCode => new SpotExceptionsOutOfSpecReasonCodeResultDto
                {
                    Id = spotExceptionsOutOfSpecReasonCode.Id,
                    ReasonCode = spotExceptionsOutOfSpecReasonCode.ReasonCode,
                    Description = spotExceptionsOutOfSpecReasonCode.Reason,
                    Label = spotExceptionsOutOfSpecReasonCode.Label
                }).OrderBy(x => x.Label).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Reason Codes");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Reason Codes";
                throw new CadentException(msg, ex);
            }

            return spotExceptionsOutOfSpecReasonCodeResults;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetSpotExceptionsOutOfSpecMarketsAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var outOfSpecSpotsToDo = new List<string>();
            var outOfSpecSpotsDone = new List<string>();
            List<string> markets = new List<string>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Markets");
            try
            {
                outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsToDoMarketsAsync(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);
                outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsDoneMarketsAsync(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);

                markets = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).Distinct().OrderBy(market => market).ToList();
                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Markets");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Markets";
                throw new CadentException(msg, ex);
            }

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

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Advertisers");
            try
            {
                outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsToDoAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest.WeekStartDate, spotExceptionsOutofSpecAdvertisersRequest.WeekEndDate);
                outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsDoneAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest.WeekStartDate, spotExceptionsOutofSpecAdvertisersRequest.WeekEndDate);

                var advertisersMasterIds = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).Distinct().ToList();
                if (advertisersMasterIds.Any())
                {
                    advertiserName = advertisersMasterIds.Select(n => _GetAdvertiserName(n.Value) ?? "Unknown").ToList();
                }

                advertiserName = advertiserName.OrderBy(n => n).ToList();
                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Advertisers");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Advertisers";
                throw new CadentException(msg, ex);
            }

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
            var isSaved = false;

            _LogInfo($"Starting: Saving Decisions to Out of Spec");
            try
            {
                if(spotExceptionsOutOfSpecSaveRequest.Decisions.All(x => x.TodoId != null))
                {
                    isSaved = await _SaveOutOfSpecToDoDecisionsAsync(spotExceptionsOutOfSpecSaveRequest, userName);
                }
                else if (spotExceptionsOutOfSpecSaveRequest.Decisions.All(x => x.DoneId != null))
                {
                    isSaved = await _SaveOutOfSpecDoneDecisionsAsync(spotExceptionsOutOfSpecSaveRequest, userName);
                }
                else
                {
                    isSaved = false;
                }

                _LogInfo($"Finished: Saving Decisions to Out of Spec");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save Decisions to Out of Spec";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        /// <inheritdoc />
        public async Task<bool> _SaveOutOfSpecToDoDecisionsAsync(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest, string userName)
        {
            var isSaved = false;

            _LogInfo($"Starting: Saving Decisions to Out of Spec ToDo");
            try
            {
                if (!spotExceptionsOutOfSpecSaveRequest.Decisions.Select(x => x.Comments).First().IsEmpty())
                {
                    isSaved = await _SaveOutOfSpecCommentsToDoAsync(spotExceptionsOutOfSpecSaveRequest);
                }
                else
                {
                    isSaved = await _SaveOutOfSpecDecisionsToDoAsync(spotExceptionsOutOfSpecSaveRequest, userName);
                }

                _LogInfo($"Finished: Saving Decisions to Out of Spec ToDo");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save Decisions to Out of Spec ToDo";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        /// <inheritdoc />
        public async Task<bool> _SaveOutOfSpecDoneDecisionsAsync(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest, string userName)
        {
            var isSaved = false;

            _LogInfo($"Starting: Saving Decisions to Out of Spec Done");
            try
            {
                if(!spotExceptionsOutOfSpecSaveRequest.Decisions.Select(x => x.Comments).First().IsEmpty())
                {
                    isSaved = await _SaveOutOfSpecCommentsDoneAsync(spotExceptionsOutOfSpecSaveRequest);
                }
                else
                {
                    isSaved = await _SaveOutOfSpecDecisionsDoneAsync(spotExceptionsOutOfSpecSaveRequest, userName);
                }

                _LogInfo($"Finished: Saving Decisions to Out of Spec Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save Decisions to Out of Spec Done";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        private async Task<bool> _SaveOutOfSpecCommentsToDoAsync(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest)
        {
            bool isCommentSaved;
            var spotExceptionsOutOfSpecToDo = new List<SpotExceptionsOutOfSpecsToDoDto>();

            _LogInfo($"Starting: Saving Comments to Out of Spec ToDo");
            try
            {
                foreach (var spotExceptionsOutOfSpec in spotExceptionsOutOfSpecSaveRequest.Decisions)
                {
                    var spot = new SpotExceptionsOutOfSpecsToDoDto
                    {
                        Id = spotExceptionsOutOfSpec.TodoId ?? default,
                        Comments = spotExceptionsOutOfSpec.Comments
                    };

                    spotExceptionsOutOfSpecToDo.Add(spot);
                }

                isCommentSaved = await _SpotExceptionsOutOfSpecRepository.SaveOutOfSpecCommentsToDoAsync(spotExceptionsOutOfSpecToDo);
                _LogInfo($"Finished: Saving Comments to Out of Spec ToDo");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save Comments to Out of Spec ToDo";
                throw new CadentException(msg, ex);
            }

            return isCommentSaved;
        }

        private async Task<bool> _SaveOutOfSpecDecisionsToDoAsync(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest, string userName)
        {
            bool isSaved;

            _LogInfo($"Starting: Moving the Spot Exception Plan by Decision to Done");
            try
            {
                var doneOutOfSpecsToAdd = new List<SpotExceptionsOutOfSpecsDoneDto>();
                var doneOutOfSpecsEditedToAdd = new List<SpotExceptionsOutOfSpecsDoneDto>();

                var existingOutOfSpecsToDo = await _SpotExceptionsOutOfSpecRepository.GetOutOfSpecSpotsToDoByIds(spotExceptionsOutOfSpecSaveRequest.Decisions.Select(x => x.TodoId).ToList());

                var firstRequest = spotExceptionsOutOfSpecSaveRequest.Decisions.First();
                if (string.IsNullOrEmpty(firstRequest.ProgramName) && string.IsNullOrEmpty(firstRequest.GenreName) && string.IsNullOrEmpty(firstRequest.DaypartCode))
                {
                    doneOutOfSpecsToAdd = existingOutOfSpecsToDo.Select(existingOutOfSpecToDo => new SpotExceptionsOutOfSpecsDoneDto
                    {
                        SpotUniqueHashExternal = existingOutOfSpecToDo.SpotUniqueHashExternal,
                        ExecutionIdExternal = existingOutOfSpecToDo.ExecutionIdExternal,
                        ReasonCodeMessage = existingOutOfSpecToDo.ReasonCodeMessage,
                        EstimateId = existingOutOfSpecToDo.EstimateId,
                        IsciName = existingOutOfSpecToDo.IsciName,
                        HouseIsci = existingOutOfSpecToDo.HouseIsci,
                        RecommendedPlanId = existingOutOfSpecToDo.RecommendedPlanId,
                        RecommendedPlanName = existingOutOfSpecToDo.RecommendedPlanName,
                        ProgramName = existingOutOfSpecToDo.ProgramName,
                        StationLegacyCallLetters = existingOutOfSpecToDo.StationLegacyCallLetters,
                        DaypartCode = existingOutOfSpecToDo.DaypartCode,
                        GenreName = existingOutOfSpecToDo.GenreName,
                        Affiliate = existingOutOfSpecToDo.Affiliate,
                        Market = existingOutOfSpecToDo.Market,
                        SpotLength = existingOutOfSpecToDo.SpotLength,
                        Audience = existingOutOfSpecToDo.Audience,
                        ProgramAirTime = existingOutOfSpecToDo.ProgramAirTime,
                        ProgramNetwork = existingOutOfSpecToDo.ProgramNetwork,
                        IngestedBy = existingOutOfSpecToDo.IngestedBy,
                        IngestedAt = existingOutOfSpecToDo.IngestedAt,
                        IngestedMediaWeekId = existingOutOfSpecToDo.IngestedMediaWeekId,
                        Impressions = existingOutOfSpecToDo.Impressions,
                        PlanId = existingOutOfSpecToDo.PlanId,
                        FlightStartDate = existingOutOfSpecToDo.FlightStartDate,
                        FlightEndDate = existingOutOfSpecToDo.FlightEndDate,
                        AdvertiserMasterId = existingOutOfSpecToDo.AdvertiserMasterId,
                        Product = existingOutOfSpecToDo.Product,
                        SpotExceptionsOutOfSpecReasonCode = existingOutOfSpecToDo.SpotExceptionsOutOfSpecReasonCode,
                        MarketCode = existingOutOfSpecToDo.MarketCode,
                        MarketRank = existingOutOfSpecToDo.MarketRank,
                        Comments = existingOutOfSpecToDo.Comments,
                        InventorySourceName = existingOutOfSpecToDo.InventorySourceName,
                        SpotExceptionsOutOfSpecDoneDecision = new SpotExceptionsOutOfSpecDoneDecisionsDto()
                        {
                            AcceptedAsInSpec = spotExceptionsOutOfSpecSaveRequest.Decisions.Where(x => x.TodoId == existingOutOfSpecToDo.Id).Select(x => x.AcceptAsInSpec).Single(),
                            DecisionNotes = spotExceptionsOutOfSpecSaveRequest.Decisions.Where(x => x.TodoId == existingOutOfSpecToDo.Id).Select(x => x.AcceptAsInSpec ? "In" : "Out").Single(),
                            DecidedBy = userName,
                            DecidedAt = _DateTimeEngine.GetCurrentMoment()
                        }
                    }).ToList();
                }
                else
                {
                    doneOutOfSpecsEditedToAdd = existingOutOfSpecsToDo.Select(existingOutOfSpecToDo => new SpotExceptionsOutOfSpecsDoneDto
                    {
                        SpotUniqueHashExternal = existingOutOfSpecToDo.SpotUniqueHashExternal,
                        ExecutionIdExternal = existingOutOfSpecToDo.ExecutionIdExternal,
                        ReasonCodeMessage = existingOutOfSpecToDo.ReasonCodeMessage,
                        EstimateId = existingOutOfSpecToDo.EstimateId,
                        IsciName = existingOutOfSpecToDo.IsciName,
                        HouseIsci = existingOutOfSpecToDo.HouseIsci,
                        RecommendedPlanId = existingOutOfSpecToDo.RecommendedPlanId,
                        RecommendedPlanName = existingOutOfSpecToDo.RecommendedPlanName,
                        ProgramName = existingOutOfSpecToDo.ProgramName,
                        StationLegacyCallLetters = existingOutOfSpecToDo.StationLegacyCallLetters,
                        DaypartCode = existingOutOfSpecToDo.DaypartCode,
                        GenreName = existingOutOfSpecToDo.GenreName,
                        Affiliate = existingOutOfSpecToDo.Affiliate,
                        Market = existingOutOfSpecToDo.Market,
                        SpotLength = existingOutOfSpecToDo.SpotLength,
                        Audience = existingOutOfSpecToDo.Audience,
                        ProgramAirTime = existingOutOfSpecToDo.ProgramAirTime,
                        ProgramNetwork = existingOutOfSpecToDo.ProgramNetwork,
                        IngestedBy = existingOutOfSpecToDo.IngestedBy,
                        IngestedAt = existingOutOfSpecToDo.IngestedAt,
                        IngestedMediaWeekId = existingOutOfSpecToDo.IngestedMediaWeekId,
                        Impressions = existingOutOfSpecToDo.Impressions,
                        PlanId = existingOutOfSpecToDo.PlanId,
                        FlightStartDate = existingOutOfSpecToDo.FlightStartDate,
                        FlightEndDate = existingOutOfSpecToDo.FlightEndDate,
                        AdvertiserMasterId = existingOutOfSpecToDo.AdvertiserMasterId,
                        Product = existingOutOfSpecToDo.Product,
                        SpotExceptionsOutOfSpecReasonCode = existingOutOfSpecToDo.SpotExceptionsOutOfSpecReasonCode,
                        MarketCode = existingOutOfSpecToDo.MarketCode,
                        MarketRank = existingOutOfSpecToDo.MarketRank,
                        Comments = existingOutOfSpecToDo.Comments,
                        InventorySourceName = existingOutOfSpecToDo.InventorySourceName,
                        SpotExceptionsOutOfSpecDoneDecision = new SpotExceptionsOutOfSpecDoneDecisionsDto()
                        {
                            AcceptedAsInSpec = spotExceptionsOutOfSpecSaveRequest.Decisions.Where(x => x.TodoId == existingOutOfSpecToDo.Id).Select(x => x.AcceptAsInSpec).Single(),
                            DecisionNotes = spotExceptionsOutOfSpecSaveRequest.Decisions.Where(x => x.TodoId == existingOutOfSpecToDo.Id).Select(x => x.AcceptAsInSpec ? "In" : "Out").Single(),
                            ProgramName = firstRequest.ProgramName,
                            GenreName = firstRequest.GenreName,
                            DaypartCode = firstRequest.DaypartCode,
                            DecidedBy = userName,
                            DecidedAt = _DateTimeEngine.GetCurrentMoment()
                        }
                    }).ToList();
                }

                using (var transaction = new TransactionScopeWrapper())
                {
                    if (doneOutOfSpecsToAdd.Any())
                    {
                        _SpotExceptionsOutOfSpecRepository.AddOutOfSpecToDone(doneOutOfSpecsToAdd);
                    }
                    if (doneOutOfSpecsEditedToAdd.Any())
                    {
                        _SpotExceptionsOutOfSpecRepository.AddOutOfSpecEditedToDone(doneOutOfSpecsEditedToAdd);
                    }
                    _SpotExceptionsOutOfSpecRepository.DeleteOutOfSpecsFromToDo(existingOutOfSpecsToDo.Select(x => x.Id).ToList());

                    transaction.Complete();
                    isSaved = true;
                }

                _LogInfo($"Finished: Moving the Spot Exception Plan by Decision to Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not move Spot Exception Plan by Decision to Done";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        private async Task<bool> _SaveOutOfSpecCommentsDoneAsync(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest)
        {
            bool isCommentSaved;
            var spotExceptionsOutOfSpecDone = new List<SpotExceptionsOutOfSpecsDoneDto>();

            _LogInfo($"Starting: Saving Comments to Out of Spec Done");
            try
            {
                foreach (var spotExceptionsOutOfSpec in spotExceptionsOutOfSpecSaveRequest.Decisions)
                {
                    var spot = new SpotExceptionsOutOfSpecsDoneDto
                    {
                        Id = spotExceptionsOutOfSpec.DoneId.Value,
                        Comments = spotExceptionsOutOfSpec.Comments
                    };
                    spotExceptionsOutOfSpecDone.Add(spot);
                }

                isCommentSaved = await _SpotExceptionsOutOfSpecRepository.SaveOutOfSpecCommentsDoneAsync(spotExceptionsOutOfSpecDone);
                _LogInfo($"Finished: Saving Comments to Out of Spec Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save Comments to Out of Spec Done";
                throw new CadentException(msg, ex);
            }

            return isCommentSaved;
        }

        private async Task<bool> _SaveOutOfSpecDecisionsDoneAsync(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest, string userName)
        {
            bool isSpotExceptionsOutOfSpecDoneDecisionSaved;
            var spotExceptionsOutOfSpecDone = new List<SpotExceptionsOutOfSpecDoneDecisionsDto>();
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            _LogInfo($"Starting: Saving decisions for the Done");
            try
            {
                foreach (var spotExceptionsOutOfSpec in spotExceptionsOutOfSpecSaveRequest.Decisions)
                {
                    var spotExceptionsOutOfSpecDoneDecision = new SpotExceptionsOutOfSpecDoneDecisionsDto
                    {
                        Id = spotExceptionsOutOfSpec.DoneId.Value,
                        AcceptedAsInSpec = spotExceptionsOutOfSpec.AcceptAsInSpec,
                        ProgramName = spotExceptionsOutOfSpec.ProgramName,
                        GenreName = spotExceptionsOutOfSpec.GenreName,
                        DaypartCode = spotExceptionsOutOfSpec.DaypartCode
                    };
                    spotExceptionsOutOfSpecDone.Add(spotExceptionsOutOfSpecDoneDecision);
                }

                isSpotExceptionsOutOfSpecDoneDecisionSaved = await _SpotExceptionsOutOfSpecRepository.SaveSpotExceptionsOutOfSpecDoneDecisionsAsync(spotExceptionsOutOfSpecDone, userName, currentDate);
                _LogInfo($"Starting: Saving decisions for the Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save decisions for the Done";
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
                    foreach(var decision in programsSpotExceptionDecisions)
                    {
                        if(!programs.Any(x => x.OfficialProgramName.ToLower().Contains(decision.OfficialProgramName.ToLower())))
                        {
                            programs.Add(decision);
                        }
                    }
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
                        var genre = _GenreCache.GetGenreLookupDtoById(programName.GenreId).Display.ToUpper();
                        var genres = HandleFlexGenres(genre);
                        spotExceptionsOutOfSpecProgram.Genres.AddRange(genres);
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

        internal static List<string> HandleFlexGenres(string genre)
        {
            const string flexGenreToken = "/";
            var genres = new List<string> { genre };

            if (genre.Contains(flexGenreToken))
            {
                var split = genre.Split(flexGenreToken.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                genres.AddRange(split.Select(s => s.Trim()).ToList());
                genres = genres.OrderBy(s => s).ToList();
            }
            return genres;
        }

        private void _RemoveVariousAndUnmatchedFromPrograms(List<ProgramNameDto> result)
        {
            result.RemoveAll(x => _GenreCache.GetGenreLookupDtoById(x.GenreId).Display.Equals("Various", StringComparison.OrdinalIgnoreCase)
                    || x.OfficialProgramName.Equals("Unmatched", StringComparison.OrdinalIgnoreCase));
        }
    }
}
