using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsRecommendedPlanService : IApplicationService
    {
        Task<SpotExceptionsRecommendedPlansResultsDto> GetSpotExceptionRecommendedPlansAsync(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest);

        Task<SpotExceptionsRecommendedPlanSpotsResultDto> GetSpotExceptionsRecommendedPlanSpotsAsync(RecomendedPlansRequestDto recomendedPlansRequest);

        Task<SpotExceptionsRecommendedPlanDetailsResultDto> GetSpotExceptionsRecommendedPlanDetails(int spotExceptionsRecommendedPlanId);

        Task<List<string>> GetSpotExceptionsRecommendedPlanAdvertisersAsync(SpotExceptionsRecommendedPlanAdvertisersRequestDto spotExceptionsRecommendedPlansAdvertisersRequest);

        Task<List<string>> GetSpotExceptionsRecommendedPlansStationsAsync(SpotExceptionsRecommendedPlanStationRequestDto spotExceptionsRecommendedPlansStationRequest);

        Task<bool> HandleSaveSpotExceptionsRecommendedPlanAsync(SpotExceptionsRecommendedPlanSaveDecisionsRequestDto spotExceptionsRecommendedPlanSaveRequest, string userName);

        Task<RecommendedPlanFiltersResultDto> GetRecommendedPlansFilters(RecomendedPlansRequestDto recomendedPlansRequest);
    }

    public class SpotExceptionsRecommendedPlanService : BroadcastBaseClass, ISpotExceptionsRecommendedPlanService
    {
        private readonly ISpotExceptionsRecommendedPlanRepository _SpotExceptionsRecommendedPlanRepository;
        private readonly ISpotLengthRepository _SpotLengthRepository;

        private readonly IDateTimeEngine _DateTimeEngine;
        private readonly IAabEngine _AabEngine;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        public SpotExceptionsRecommendedPlanService(
            IDataRepositoryFactory dataRepositoryFactory,
            IDateTimeEngine dateTimeEngine,
            IAabEngine aabEngine,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper)
            : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsRecommendedPlanRepository = dataRepositoryFactory.GetDataRepository<ISpotExceptionsRecommendedPlanRepository>();
            _SpotLengthRepository = dataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _DateTimeEngine = dateTimeEngine;
            _AabEngine = aabEngine;
            _FeatureToggleHelper = featureToggleHelper;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsRecommendedPlansResultsDto> GetSpotExceptionRecommendedPlansAsync(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest)
        {
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";
            var recommendedPlans = new SpotExceptionsRecommendedPlansResultsDto();

            try
            {
                var recommendedPlanToDo = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlansToDoAsync(spotExceptionsRecommendedPlansRequest.WeekStartDate, spotExceptionsRecommendedPlansRequest.WeekEndDate);
                var recommendedPlanDone = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlansDoneAsync(spotExceptionsRecommendedPlansRequest.WeekStartDate, spotExceptionsRecommendedPlansRequest.WeekEndDate);

                if (recommendedPlanToDo?.Any() ?? false)
                {
                    var recommendedImpressionCount = recommendedPlanToDo.SelectMany(y => y.SpotExceptionsRecommendedPlanDetailsToDo).Select(x => x.SpotDeliveredImpressions).Sum() / 1000;
                    recommendedPlans.Active = recommendedPlanToDo.GroupBy(recommendedPlan => new { recommendedPlan.SpotExceptionsRecommendedPlanDetailsToDo.First(y => y.IsRecommendedPlan).RecommendedPlanId })
                        .Select(activeRecommendedPlan =>
                        {
                            var planDetails = activeRecommendedPlan.First();
                            var planAdvertiserMasterId = planDetails.SpotExceptionsRecommendedPlanDetailsToDo.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail.AdvertiserMasterId).First();
                            var flightStartDate = planDetails.SpotExceptionsRecommendedPlanDetailsToDo.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.FlightStartDate).First();
                            var flightEndDate = planDetails.SpotExceptionsRecommendedPlanDetailsToDo.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.FlightEndDate).First();
                            return new SpotExceptionsRecommendedToDoPlansDto
                            {
                                PlanId = planDetails.SpotExceptionsRecommendedPlanDetailsToDo.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).First(),
                                AdvertiserName = _GetAdvertiserName(planAdvertiserMasterId),
                                PlanName = planDetails.SpotExceptionsRecommendedPlanDetailsToDo.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).First(),
                                AffectedSpotsCount = activeRecommendedPlan.Count(),
                                Impressions = recommendedImpressionCount,
                                SpotLengthString = $":{_SpotLengthRepository.GetSpotLengthById(planDetails.SpotLengthId ?? 0)}" ?? null,
                                AudienceName = planDetails.SpotExceptionsRecommendedPlanDetailsToDo.Where(x => x.IsRecommendedPlan).Select(x => x.AudienceName).First(),
                                FlightString = $"{Convert.ToDateTime(flightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(flightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(flightStartDate), Convert.ToDateTime(flightEndDate)).ToString() + " " + "Weeks"})",
                            };
                        }).ToList();
                }

                if (recommendedPlanDone?.Any() ?? false)
                {
                    var completedImpressionCount = recommendedPlanDone.SelectMany(y => y.SpotExceptionsRecommendedPlanDetailsDone).Select(x => x.SpotDeliveredImpressions).Sum() / 1000;
                    recommendedPlans.Completed = recommendedPlanDone.GroupBy(recommendedPlan => new { recommendedPlan.SpotExceptionsRecommendedPlanDetailsDone.First(y => y.IsRecommendedPlan).RecommendedPlanId })
                        .Select(completedRecommendedPlan =>
                        {
                            var planDetails = completedRecommendedPlan.First();
                            var planAdvertiserMasterId = planDetails.SpotExceptionsRecommendedPlanDetailsDone.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.AdvertiserMasterId).First();
                            var flightStartDate = planDetails.SpotExceptionsRecommendedPlanDetailsDone.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.FlightStartDate).First();
                            var flightEndDate = planDetails.SpotExceptionsRecommendedPlanDetailsDone.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.FlightEndDate).First();
                            return new SpotExceptionsRecommendedDonePlansDto
                            {
                                PlanId = planDetails.SpotExceptionsRecommendedPlanDetailsDone.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).First(),
                                AdvertiserName = _GetAdvertiserName(planAdvertiserMasterId),
                                PlanName = planDetails.SpotExceptionsRecommendedPlanDetailsDone.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).First(),
                                AffectedSpotsCount = completedRecommendedPlan.Count(),
                                Impressions = planDetails.SpotExceptionsRecommendedPlanDetailsDone.Select(x => x.SpotDeliveredImpressions).Sum() / 1000,
                                SpotLengthString = $":{_SpotLengthRepository.GetSpotLengthById(planDetails.SpotLengthId ?? 0)}" ?? null,
                                AudienceName = planDetails.SpotExceptionsRecommendedPlanDetailsDone.Where(x => x.IsRecommendedPlan).Select(x => x.AudienceName).First(),
                                FlightString = $"{Convert.ToDateTime(flightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(flightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(flightStartDate), Convert.ToDateTime(flightEndDate)).ToString() + " " + "Weeks"})",
                            };
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return recommendedPlans;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetSpotExceptionsRecommendedPlanAdvertisersAsync(SpotExceptionsRecommendedPlanAdvertisersRequestDto spotExceptionsRecommendedPlansAdvertisersRequest)
        {
            SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto();

            List<Guid> recommendedPlanAdvertisersToDo = null;
            List<Guid> recommendedPlanadvertisersDone = null;
            List<string> advertiserName = new List<string>();

            try
            {
                recommendedPlanAdvertisersToDo = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanSpotsToDoAdvertisersAsync(spotExceptionsRecommendedPlansAdvertisersRequest.WeekStartDate, spotExceptionsRecommendedPlansAdvertisersRequest.WeekEndDate);
                recommendedPlanadvertisersDone = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanSpotsDoneAdvertisersAsync(spotExceptionsRecommendedPlansAdvertisersRequest.WeekStartDate, spotExceptionsRecommendedPlansAdvertisersRequest.WeekEndDate);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            var advertisersMasterIds = recommendedPlanAdvertisersToDo.Concat(recommendedPlanadvertisersDone).Distinct().ToList();
            if (advertisersMasterIds.Any())
            {
                advertiserName = advertisersMasterIds.Select(n => _GetAdvertiserName(n) ?? "Unknown").ToList();
            }

            return advertiserName;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetSpotExceptionsRecommendedPlansStationsAsync(SpotExceptionsRecommendedPlanStationRequestDto spotExceptionsRecommendedPlansStationRequest)
        {
            SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto();

            List<string> recommendedPlanStationsToDo;
            List<string> recommendedPlanStationsDone;
            List<string> stations;

            try
            {
                recommendedPlanStationsToDo = await _SpotExceptionsRecommendedPlanRepository.GetSpotExceptionsRecommendedPlanToDoStationsAsync(spotExceptionsRecommendedPlansRequest.WeekStartDate, spotExceptionsRecommendedPlansRequest.WeekEndDate);
                recommendedPlanStationsDone = await _SpotExceptionsRecommendedPlanRepository.GetSpotExceptionsRecommendedPlanDoneStationsAsync(spotExceptionsRecommendedPlansRequest.WeekStartDate, spotExceptionsRecommendedPlansRequest.WeekEndDate);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            stations = recommendedPlanStationsToDo.Concat(recommendedPlanStationsDone).Distinct().ToList();

            return stations;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsRecommendedPlanSpotsResultDto> GetSpotExceptionsRecommendedPlanSpotsAsync(RecomendedPlansRequestDto recomendedPlansRequest)
        {
            var recommendedPlanSpots = new SpotExceptionsRecommendedPlanSpotsResultDto();

            try
            {
                var recommendedPlanSpotsToDo = await _SpotExceptionsRecommendedPlanRepository.GetSpotExceptionRecommendedPlanToDoSpots(recomendedPlansRequest.PlanId, recomendedPlansRequest.WeekStartDate, recomendedPlansRequest.WeekEndDate);

                if (recommendedPlanSpotsToDo?.Any() ?? false)
                {
                    //var planIds = recommendedPlanToDo.Select(p => p.PlanId).Distinct().ToList();
                    //var daypartsList = _PlanRepository.GetPlanDaypartsByPlanIds(planIds);

                    recommendedPlanSpots.Active = recommendedPlanSpotsToDo
                    .Select(activePlan =>
                    {
                        var planDetails = activePlan.SpotExceptionsRecommendedPlanDetailsToDo;
                        var activePlanDetails = recommendedPlanSpotsToDo.First();
                        return new SpotExceptionsRecommendedToDoPlanSpotsDto
                        {
                            Id = activePlan.Id,
                            EstimateId = activePlan.EstimateId,
                            IsciName = activePlan.ClientIsci,
                            ProgramAirDate = activePlan.ProgramAirTime.ToShortDateString(),
                            ProgramAirTime = activePlan.ProgramAirTime.ToLongTimeString(),
                            RecommendedPlan = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).FirstOrDefault(),
                            Impressions = planDetails.Select(x => x.SpotDeliveredImpressions).First() / 1000,
                            ProgramName = activePlan.ProgramName,
                            Affiliate = activePlan.Affiliate,
                            PlanId = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).FirstOrDefault(),
                            Market = _GetMarketName(activePlan.MarketCode ?? 0),
                            Station = activePlan.StationLegacyCallLetters,
                            InventorySource = activePlan.InventorySource
                        };
                    }).ToList();
                }

                var recommendedPlanSpotsDone = await _SpotExceptionsRecommendedPlanRepository.GetSpotExceptionRecommendedPlanDoneSpots(recomendedPlansRequest.PlanId, recomendedPlansRequest.WeekStartDate, recomendedPlansRequest.WeekEndDate);

                if (recommendedPlanSpotsDone?.Any() ?? false)
                {
                    //var planIds = recommendedPlanSpotsDone.Select(p => p.PlanId).Distinct().ToList();
                    //var daypartsList = _PlanRepository.GetPlanDaypartsByPlanIds(planIds);

                    recommendedPlanSpots.Queued = recommendedPlanSpotsDone.Where(d => d.SpotExceptionsRecommendedPlanDetailsDone.Any(s => s.SpotExceptionsRecommendedPlanDoneDecisions.SyncedAt == null))
                    .Select(queuedPlan =>
                    {
                        var planDetails = queuedPlan.SpotExceptionsRecommendedPlanDetailsDone;
                        var queuedPlanDetails = recommendedPlanSpotsDone.First();
                        return new SpotExceptionsRecommendedDonePlanSpotsDto
                        {
                            Id = queuedPlan.Id,
                            EstimateId = queuedPlan.EstimateId,
                            IsciName = queuedPlan.ClientIsci,
                            ProgramAirDate = queuedPlan.ProgramAirTime.ToShortDateString(),
                            ProgramAirTime = queuedPlan.ProgramAirTime.ToLongTimeString(),
                            RecommendedPlan = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).FirstOrDefault(),
                            Impressions = queuedPlanDetails.SpotExceptionsRecommendedPlanDetailsDone.Select(x => x.SpotDeliveredImpressions).First() / 1000,
                            ProgramName = queuedPlan.ProgramName,
                            Affiliate = queuedPlan.Affiliate,
                            PlanId = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).FirstOrDefault(),
                            Market = _GetMarketName(queuedPlan.MarketCode ?? 0),
                            Station = queuedPlan.StationLegacyCallLetters,
                            InventorySource = queuedPlan.InventorySource
                        };
                    }).ToList();

                    recommendedPlanSpots.Synced = recommendedPlanSpotsDone.Where(d => d.SpotExceptionsRecommendedPlanDetailsDone.Any(s => s.SpotExceptionsRecommendedPlanDoneDecisions.SyncedAt != null))
                    .Select(syncedPlan =>
                    {
                        var planDetails = syncedPlan.SpotExceptionsRecommendedPlanDetailsDone;
                        var syncedPlanDetails = recommendedPlanSpotsDone.First();
                        return new SpotExceptionsRecommendedDonePlanSpotsDto
                        {
                            Id = syncedPlan.Id,
                            EstimateId = syncedPlan.EstimateId,
                            IsciName = syncedPlan.ClientIsci,
                            ProgramAirDate = syncedPlan.ProgramAirTime.ToShortDateString(),
                            ProgramAirTime = syncedPlan.ProgramAirTime.ToLongTimeString(),
                            RecommendedPlan = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).FirstOrDefault(),
                            Impressions = syncedPlanDetails.SpotExceptionsRecommendedPlanDetailsDone.Select(x => x.SpotDeliveredImpressions).First() / 1000,
                            ProgramName = syncedPlan.ProgramName,
                            Affiliate = syncedPlan.Affiliate,
                            PlanId = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).FirstOrDefault(),
                            Market = _GetMarketName(syncedPlan.MarketCode ?? 0),
                            Station = syncedPlan.StationLegacyCallLetters,
                            InventorySource = syncedPlan.InventorySource,
                        };
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }
            return recommendedPlanSpots;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsRecommendedPlanDetailsResultDto> GetSpotExceptionsRecommendedPlanDetails(int spotExceptionsRecommendedPlanId)
        {
            var spotExceptionsRecommendedPlanDetailsResult = new SpotExceptionsRecommendedPlanDetailsResultDto();

            var spotExceptionsRecommendedPlan = await _SpotExceptionsRecommendedPlanRepository.GetSpotExceptionsRecommendedPlanById(spotExceptionsRecommendedPlanId);
            if (spotExceptionsRecommendedPlan == null)
            {
                return spotExceptionsRecommendedPlanDetailsResult;
            }

            var recommendedPlan = spotExceptionsRecommendedPlan.SpotExceptionsRecommendedPlanDetailsToDo.First(x => x.IsRecommendedPlan);

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
                Plans = spotExceptionsRecommendedPlan.SpotExceptionsRecommendedPlanDetailsToDo.Select(spotExceptionsRecommendedPlanDetail => new RecommendedPlanDetailResultDto
                {
                    Id = spotExceptionsRecommendedPlanDetail.Id,
                    Name = spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.Name,
                    SpotLengthString = string.Join(", ", spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.SpotLengths.Select(spotLength => $":{spotLength.Length}")),
                    FlightStartDate = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightStartDate}",
                    FlightEndDate = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightEndDate}",
                    FlightDateString = $"{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightStartDate.ToString(flightStartDateFormat)}-{spotExceptionsRecommendedPlanDetail.RecommendedPlanDetail.FlightEndDate.ToString(flightEndDateFormat)}",
                    IsRecommendedPlan = spotExceptionsRecommendedPlanDetail.IsRecommendedPlan,
                    Pacing = _calculatePacing(spotExceptionsRecommendedPlanDetail.DeliveredImpressions, spotExceptionsRecommendedPlanDetail.ContractedImpressions) + "%",
                    RecommendedPlanId = spotExceptionsRecommendedPlanDetail.RecommendedPlanId,
                    AudienceName = spotExceptionsRecommendedPlanDetail.AudienceName,
                    Product = _GetProductName(recommendedPlan.RecommendedPlanDetail.AdvertiserMasterId, recommendedPlan.RecommendedPlanDetail.ProductMasterId),
                    DaypartCode = spotExceptionsRecommendedPlanDetail.DaypartCode,
                    Impressions = spotExceptionsRecommendedPlanDetail.SpotDeliveredImpressions / 1000,
                    TotalContractedImpressions = spotExceptionsRecommendedPlanDetail.PlanTotalContractedImpressions / 1000,
                    TotalDeliveredImpressionsSelected = (spotExceptionsRecommendedPlanDetail.SpotDeliveredImpressions + spotExceptionsRecommendedPlanDetail.PlanTotalDeliveredImpressions) / 1000,
                    TotalPacingSelected = ((spotExceptionsRecommendedPlanDetail.PlanTotalDeliveredImpressions + spotExceptionsRecommendedPlanDetail.SpotDeliveredImpressions) / spotExceptionsRecommendedPlanDetail.PlanTotalContractedImpressions) * 100,
                    TotalDeliveredImpressionsUnselected = spotExceptionsRecommendedPlanDetail.PlanTotalDeliveredImpressions / 1000,
                    TotalPacingUnselected = (spotExceptionsRecommendedPlanDetail.PlanTotalDeliveredImpressions / spotExceptionsRecommendedPlanDetail.PlanTotalContractedImpressions) * 100,
                    WeeklyContractedImpressions = spotExceptionsRecommendedPlanDetail.ContractedImpressions / 1000,
                    WeeklyDeliveredImpressionsSelected = (spotExceptionsRecommendedPlanDetail.DeliveredImpressions + spotExceptionsRecommendedPlanDetail.SpotDeliveredImpressions) / 1000,
                    WeeklyPacingSelected = _calculateWeeklyPacingSelected(spotExceptionsRecommendedPlanDetail.DeliveredImpressions, spotExceptionsRecommendedPlanDetail.SpotDeliveredImpressions, spotExceptionsRecommendedPlanDetail.ContractedImpressions),
                    WeeklyDeliveredImpressionsUnselected = spotExceptionsRecommendedPlanDetail.DeliveredImpressions / 1000,
                    WeeklyPacingUnselected = _calculateWeeklyPacingUnselected(spotExceptionsRecommendedPlanDetail.DeliveredImpressions, spotExceptionsRecommendedPlanDetail.ContractedImpressions),
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

        /// <inheritdoc />
        public async Task<RecommendedPlanFiltersResultDto> GetRecommendedPlansFilters(RecomendedPlansRequestDto recommendedPlansRequest)
        {
            var recommendedPlanFiltersResult = new RecommendedPlanFiltersResultDto();

            List<SpotExceptionsRecommendedPlansToDoDto> spotExceptionsRecommendedToDoSpotsResult;

            try
            { 
                spotExceptionsRecommendedToDoSpotsResult = await _SpotExceptionsRecommendedPlanRepository.GetSpotExceptionRecommendedPlanToDoSpots(recommendedPlansRequest.PlanId,
                    recommendedPlansRequest.WeekStartDate, recommendedPlansRequest.WeekEndDate);

                if (spotExceptionsRecommendedToDoSpotsResult == null)
                {
                    return recommendedPlanFiltersResult;
                }

                var marketCodes = spotExceptionsRecommendedToDoSpotsResult.Select(x => x.MarketCode ?? 0).ToList();
                recommendedPlanFiltersResult.Markets = _GetMarketNames(marketCodes).Distinct().OrderBy(market => market).ToList();
                recommendedPlanFiltersResult.Stations = spotExceptionsRecommendedToDoSpotsResult.Select(spotResults => spotResults.StationLegacyCallLetters ?? "Unknown").Distinct().OrderBy(station => station).ToList();
                recommendedPlanFiltersResult.InventorySources = spotExceptionsRecommendedToDoSpotsResult.Select(spotResults => spotResults.InventorySource ?? "Unknown").Distinct().OrderBy(inventorySource => inventorySource).ToList();
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return recommendedPlanFiltersResult;
        }

        /// <inheritdoc />
        public async Task<bool> HandleSaveSpotExceptionsRecommendedPlanAsync(SpotExceptionsRecommendedPlanSaveDecisionsRequestDto spotExceptionsRecommendedPlanSaveRequest, string userName)
        {
            bool isSaved = false;

            try
            {
                foreach (var spotExceptionsRecommendedPlan in spotExceptionsRecommendedPlanSaveRequest.SpotExceptionsRecommendedPlans)
                {
                    var decisionsPlans = await _GetSpotExceptionsRecommendedPlanByDecisionAsync(spotExceptionsRecommendedPlan);

                    var countAdded = await _AddSpotExceptionsRecommendedPlanToDoneAsync(decisionsPlans);

                    var countDeleted = await _DeleteSpotExceptionsRecommendedPlanFromToDoAsync(decisionsPlans.id, decisionsPlans.spot_exceptions_recommended_plan_details.Select(x => x.id).First());

                    isSaved = await _SaveSpotExceptionsRecommendedPlanDoneDecisionsAsync(spotExceptionsRecommendedPlan, userName);
                }
            }
            catch (Exception ex)
            {
                var msg = $"Could not save the decision";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        private async Task<spot_exceptions_recommended_plans> _GetSpotExceptionsRecommendedPlanByDecisionAsync(SpotExceptionsRecommendedPlanSaveDto spotExceptionsRecommendedPlan)
        {
            var recommendedPlans = new spot_exceptions_recommended_plans();
            try
            {                
                recommendedPlans = await _SpotExceptionsRecommendedPlanRepository.GetSpotExceptionRecommendedPlanByDecisionAsync(spotExceptionsRecommendedPlan);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return recommendedPlans;
        }

        private async Task<int> _AddSpotExceptionsRecommendedPlanToDoneAsync(spot_exceptions_recommended_plans decisionsPlans)
        {
            int addedCount;

            try
            {
                addedCount = await _SpotExceptionsRecommendedPlanRepository.AddSpotExceptionsRecommendedPlanToDoneAsync(decisionsPlans);
            }
            catch (Exception ex)
            {
                var msg = $"Could not save to the the Database";
                throw new CadentException(msg, ex);
            }

            return addedCount;
        }

        private async Task<int> _DeleteSpotExceptionsRecommendedPlanFromToDoAsync(int spotExceptionRecommendedPlanId, int spotExceptionRecommendedPlanDetailId)
        {
            int deletedCunt;
            try
            {
                deletedCunt = await _SpotExceptionsRecommendedPlanRepository.DeleteSpotExceptionsRecommendedPlanFromToDoAsync(spotExceptionRecommendedPlanId, spotExceptionRecommendedPlanDetailId);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return deletedCunt;
        }

        private async Task<bool> _SaveSpotExceptionsRecommendedPlanDoneDecisionsAsync(SpotExceptionsRecommendedPlanSaveDto spotExceptionsRecommendedPlan, string userName)
        {
            bool isSpotExceptionsRecommendedPlanDecisionSaved;
            try
            {
                var spotExceptionsRecommendedPlanDoneDecision = new SpotExceptionsRecommendedPlanDoneDecisionsDto
                {
                    SpotExceptionsId = spotExceptionsRecommendedPlan.Id,
                    SpotExceptionsRecommendedPlanDetailsDoneId = spotExceptionsRecommendedPlan.SelectedPlanId,
                    DecidedBy = userName,
                    DecidedAt = _DateTimeEngine.GetCurrentMoment(),
                    SyncedAt = null,
                    SyncedBy = null
                };

                isSpotExceptionsRecommendedPlanDecisionSaved = await _SpotExceptionsRecommendedPlanRepository.SaveSpotExceptionsRecommendedPlanDoneDecisionsAsync(spotExceptionsRecommendedPlanDoneDecision);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return isSpotExceptionsRecommendedPlanDecisionSaved;
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

        internal List<string> _GetAdvertiserNames(List<Guid> masterIds)
        {
            List<string> advertiserNames = new List<string>();
            foreach (var masterId in masterIds)
            {
                var advertiserName = _AabEngine.GetAdvertiser(masterId)?.Name;
                advertiserNames.Add(advertiserName ?? null);
            }
            return advertiserNames;
        }

        private List<string> _GetMarketNames(List<int> marketCodes)
        {
            var marketNames = new List<string>();
            if (marketCodes != null)
            {
                foreach (var marketCode in marketCodes)
                {
                    var marketName = _SpotExceptionsRecommendedPlanRepository.GetMarketName(marketCode);
                    marketNames.Add(marketName ?? "Unknown");
                }
            }
            return marketNames;
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

        private string _GetMarketName(int marketCode)
        {
            var marketName = _SpotExceptionsRecommendedPlanRepository.GetMarketName(marketCode);
            return marketName;
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

        private double? _calculateWeeklyPacingSelected(double? deliveredImp, double? spotDeliveredImp, double? contractedImp)
        {
            if (contractedImp == 0)
            {
                return 0;
            }
            else
            {
                double? weeklyPacingSelected = ((deliveredImp + spotDeliveredImp) / contractedImp) * 100;
                return weeklyPacingSelected;
            }
        }

        private double? _calculateWeeklyPacingUnselected(double? deliveredImp, double? contractedImp)
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
    }
}
