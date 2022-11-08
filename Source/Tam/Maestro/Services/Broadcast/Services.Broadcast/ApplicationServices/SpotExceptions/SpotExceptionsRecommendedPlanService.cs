using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Microsoft.EntityFrameworkCore.Internal;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsRecommendedPlanService : IApplicationService
    {
        /// <summary>
        /// Gets the spot exception recommended plans asynchronous.
        /// </summary>
        /// <param name="recommendedPlansRequest">The spot exceptions recommended plans request.</param>
        /// <returns></returns>
        Task<SpotExceptionsRecommendedPlanGroupingResults> GetRecommendedPlanGroupingAsync(SpotExceptionsRecommendedPlansRequestDto recommendedPlansRequest);

        /// <summary>
        /// Gets the spot exceptions recommended plan spots asynchronous.
        /// </summary>
        /// <param name="recomendedPlanSpotsRequest">The recomended plan spots request.</param>
        /// <returns></returns>
        Task<SpotExceptionsRecommendedPlanSpotsResultDto> GetRecommendedPlanSpotsAsync(SpotExceptionsRecommendedPlanSpotsRequestDto recomendedPlanSpotsRequest);

        /// <summary>
        /// Gets the recommended plan details asynchronous.
        /// </summary>
        /// <param name="recommendedDetailsPlanId">The spot exceptions recommended plan identifier.</param>
        /// <returns></returns>
        Task<SpotExceptionsRecommendedPlanDetailsResultDto> GetRecommendedPlanDetailsAsync(int recommendedDetailsPlanId);

        /// <summary>
        /// Gets the recommended plan advertisers asynchronous.
        /// </summary>
        /// <param name="recommendedPlanAdvertisersRequest">The spot exceptions recommended plans advertisers request.</param>
        /// <returns></returns>
        Task<List<string>> GetRecommendedPlanAdvertisersAsync(SpotExceptionsRecommendedPlanAdvertisersRequestDto recommendedPlanAdvertisersRequest);

        /// <summary>
        /// Gets the recommended plan stations asynchronous.
        /// </summary>
        /// <param name="recommendedPlanStationsRequest">The recommended plan stations request.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        Task<List<string>> GetRecommendedPlanStationsAsync(SpotExceptionsRecommendedPlanStationsRequestDto recommendedPlanStationsRequest);

        /// <summary>
        /// Gets the recommended plans filters.
        /// </summary>
        /// <param name="recomendedPlanRequest">The recomended plans request.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        Task<SpotExceptionsRecommendedPlanFiltersResultDto> GetRecommendedPlanFilters(SpotExceptionsRecommendedPlansRequestDto recomendedPlanRequest);

        /// <summary>
        /// Handles the save recommended plan decisions asynchronous.
        /// </summary>
        /// <param name="recommendedPlanDecisionsSaveRequest">The recommended plan decisions save request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        Task<bool> HandleSaveRecommendedPlanDecisionsAsync(SpotExceptionsRecommendedPlanSaveDecisionsRequestDto recommendedPlanDecisionsSaveRequest, string userName);
    }

    public class SpotExceptionsRecommendedPlanService : BroadcastBaseClass, ISpotExceptionsRecommendedPlanService
    {
        private readonly ISpotExceptionsRecommendedPlanRepository _SpotExceptionsRecommendedPlanRepository;

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
            _DateTimeEngine = dateTimeEngine;
            _AabEngine = aabEngine;
            _FeatureToggleHelper = featureToggleHelper;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsRecommendedPlanGroupingResults> GetRecommendedPlanGroupingAsync(SpotExceptionsRecommendedPlansRequestDto recommendedPlansRequest)
        {
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";
            var groupedPlans = new SpotExceptionsRecommendedPlanGroupingResults();

            try
            {
                _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Groupings");
                var recommendedPlanGroupingToDo = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanGroupingToDoAsync(recommendedPlansRequest.WeekStartDate, recommendedPlansRequest.WeekEndDate);
                var recommendedPlanGroupingDone = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanGroupingDoneAsync(recommendedPlansRequest.WeekStartDate, recommendedPlansRequest.WeekEndDate);

                if (recommendedPlanGroupingToDo?.Any() ?? false)
                {
                    groupedPlans.Active = recommendedPlanGroupingToDo.Select(x =>
                    {
                        return new SpotExceptionsRecommendedPlanGroupingToDoResults
                        {
                            PlanId = x.PlanId,
                            AdvertiserName = _GetAdvertiserName(x.AdvertiserMasterId),
                            PlanName = x.PlanName,
                            AffectedSpotsCount = x.AffectedSpotsCount,
                            Impressions = Math.Floor((double)(x.Impressions / 1000.00)),
                            SpotLengthString = string.Join(", ", x.SpotLengths.OrderBy(y => y.Length).Select(spotLength => $":{spotLength.Length}")),
                            AudienceName = x.AudienceName,
                            FlightString = $"{Convert.ToDateTime(x.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(x.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(x.FlightStartDate), Convert.ToDateTime(x.FlightEndDate)).ToString() + " " + "Weeks"})"
                        };

                    }).ToList();
                }

                if (recommendedPlanGroupingDone?.Any() ?? false)
                {
                    groupedPlans.Completed = recommendedPlanGroupingDone.Select(x =>
                    {
                        return new SpotExceptionsRecommendedPlanGroupingDoneResults
                        {
                            PlanId = x.PlanId,
                            AdvertiserName = _GetAdvertiserName(x.AdvertiserMasterId),
                            PlanName = x.PlanName,
                            AffectedSpotsCount = x.AffectedSpotsCount,
                            Impressions = Math.Floor((double)(x.Impressions / 1000)),
                            SpotLengthString = string.Join(", ", x.SpotLengths.OrderBy(y => y.Length).Select(spotLength => $":{spotLength.Length}")),
                            AudienceName = x.AudienceName,
                            FlightString = $"{Convert.ToDateTime(x.FlightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(x.FlightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(x.FlightStartDate), Convert.ToDateTime(x.FlightEndDate)).ToString() + " " + "Weeks"})"
                        };

                    }).ToList();
                }
                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Groupings");
            }
            catch (CadentException ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Recommended Plan Groupings";
                throw new CadentException(msg, ex);
            }

            return groupedPlans;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsRecommendedPlanSpotsResultDto> GetRecommendedPlanSpotsAsync(SpotExceptionsRecommendedPlanSpotsRequestDto recomendedPlanSpotsRequest)
        {
            var recommendedPlanSpots = new SpotExceptionsRecommendedPlanSpotsResultDto();

            try
            {
                _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Spots");
                var recommendedPlanSpotsToDo = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanSpotsToDo(recomendedPlanSpotsRequest.PlanId, recomendedPlanSpotsRequest.WeekStartDate, recomendedPlanSpotsRequest.WeekEndDate);
                var recommendedPlanSpotsQueued = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanSpotsQueued(recomendedPlanSpotsRequest.PlanId, recomendedPlanSpotsRequest.WeekStartDate, recomendedPlanSpotsRequest.WeekEndDate);
                var recommendedPlanSpotsSynced = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanSpotsSynced(recomendedPlanSpotsRequest.PlanId, recomendedPlanSpotsRequest.WeekStartDate, recomendedPlanSpotsRequest.WeekEndDate);

                if (recommendedPlanSpotsToDo?.Any() ?? false)
                {
                    recommendedPlanSpots.Active = recommendedPlanSpotsToDo
                    .Select(activePlan =>
                    {
                        return new SpotExceptionsRecommendedToDoPlanSpotsDto
                        {
                            Id = activePlan.Id,
                            EstimateId = activePlan.EstimateId,
                            IsciName = activePlan.IsciName,
                            ProgramAirDate = activePlan.ProgramAirTime.ToShortDateString(),
                            ProgramAirTime = activePlan.ProgramAirTime.ToLongTimeString(),
                            PlanId = activePlan.PlanId,
                            RecommendedPlan = activePlan.RecommendedPlanName,
                            Impressions = Math.Floor((double)(activePlan.Impressions / 1000)),
                            SpotLengthString = $":{activePlan.SpotLength}",
                            ProgramName = activePlan.ProgramName,
                            Affiliate = activePlan.Affiliate,
                            Market = activePlan.MarketName,
                            Station = activePlan.Station,
                            InventorySource = activePlan.InventorySource
                        };
                    }).ToList();
                }

                if (recommendedPlanSpotsQueued?.Any() ?? false)
                {
                    recommendedPlanSpots.Queued = recommendedPlanSpotsQueued
                    .Select(gueuedPlan =>
                    {
                        return new SpotExceptionsRecommendedDonePlanSpotsDto
                        {
                            Id = gueuedPlan.Id,
                            EstimateId = gueuedPlan.EstimateId,
                            IsciName = gueuedPlan.IsciName,
                            ProgramAirDate = gueuedPlan.ProgramAirTime.ToShortDateString(),
                            ProgramAirTime = gueuedPlan.ProgramAirTime.ToLongTimeString(),
                            PlanId = gueuedPlan.PlanId,
                            RecommendedPlan = gueuedPlan.RecommendedPlanName,
                            Impressions = Math.Floor((double)(gueuedPlan.Impressions / 1000)),
                            SpotLengthString = $":{gueuedPlan.SpotLength}",
                            ProgramName = gueuedPlan.ProgramName,
                            Affiliate = gueuedPlan.Affiliate,
                            Market = gueuedPlan.MarketName,
                            Station = gueuedPlan.Station,
                            InventorySource = gueuedPlan.InventorySource
                        };
                    }).ToList();
                }

                if (recommendedPlanSpotsSynced?.Any() ?? false)
                {
                    recommendedPlanSpots.Synced = recommendedPlanSpotsSynced
                    .Select(syncedPlan =>
                    {
                        return new SpotExceptionsRecommendedDonePlanSpotsDto
                        {
                            Id = syncedPlan.Id,
                            EstimateId = syncedPlan.EstimateId,
                            IsciName = syncedPlan.IsciName,
                            ProgramAirDate = syncedPlan.ProgramAirTime.ToShortDateString(),
                            ProgramAirTime = syncedPlan.ProgramAirTime.ToLongTimeString(),
                            PlanId = syncedPlan.PlanId,
                            RecommendedPlan = syncedPlan.RecommendedPlanName,
                            Impressions = Math.Floor((double)(syncedPlan.Impressions / 1000)),
                            SpotLengthString = $":{syncedPlan.SpotLength}",
                            ProgramName = syncedPlan.ProgramName,
                            Affiliate = syncedPlan.Affiliate,
                            Market = syncedPlan.MarketName,
                            Station = syncedPlan.Station,
                            InventorySource = syncedPlan.InventorySource
                        };
                    }).ToList();
                }
                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Spots");
            }
            catch (CadentException ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Recommended Plan Spots";
                throw new CadentException(msg, ex);
            }

            return recommendedPlanSpots;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsRecommendedPlanDetailsResultDto> GetRecommendedPlanDetailsAsync(int detailsId)
        {
            const string programAirDateFormat = "MM/dd/yyyy";
            const string programAirTimeFormat = "hh:mm:ss tt";
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";

            var recommendedPlanDetailsResult = new SpotExceptionsRecommendedPlanDetailsResultDto();

            try
            {
                _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Details");
                var recommendedPlanDetailsToDo = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanDetailsToDoById(detailsId);
                var recommendedPlanDetailsDone = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanDetailsDoneById(detailsId);

                if (recommendedPlanDetailsToDo == null && recommendedPlanDetailsDone == null)
                {
                    return recommendedPlanDetailsResult;
                }
                if (recommendedPlanDetailsToDo != null)
                {
                    var recommendedPlanDetails = recommendedPlanDetailsToDo.SpotExceptionsRecommendedPlanDetailsToDo.First();

                    recommendedPlanDetailsResult = new SpotExceptionsRecommendedPlanDetailsResultDto
                    {
                        Id = recommendedPlanDetailsToDo.Id,
                        EstimateId = recommendedPlanDetailsToDo.EstimateId,
                        SpotLengthString = recommendedPlanDetailsToDo.SpotLength != null ? $":{recommendedPlanDetailsToDo.SpotLength.Length}" : null,
                        Product = _GetProductName(recommendedPlanDetails.RecommendedPlanDetail.AdvertiserMasterId, recommendedPlanDetails.RecommendedPlanDetail.ProductMasterId),
                        FlightStartDate = recommendedPlanDetails.RecommendedPlanDetail.FlightStartDate.ToString(),
                        FlightEndDate = recommendedPlanDetails.RecommendedPlanDetail.FlightEndDate.ToString(),
                        FlightDateString = $"{Convert.ToDateTime(recommendedPlanDetails.RecommendedPlanDetail.FlightStartDate).ToString(flightStartDateFormat)}-{Convert.ToDateTime(recommendedPlanDetails.RecommendedPlanDetail.FlightEndDate).ToString(flightEndDateFormat)}",
                        ProgramName = recommendedPlanDetailsToDo.ProgramName,
                        ProgramAirDate = recommendedPlanDetailsToDo.ProgramAirTime.ToString(programAirDateFormat),
                        ProgramAirTime = recommendedPlanDetailsToDo.ProgramAirTime.ToString(programAirTimeFormat),
                        InventorySourceName = recommendedPlanDetailsToDo.InventorySource,
                        Plans = recommendedPlanDetailsToDo.SpotExceptionsRecommendedPlanDetailsToDo.Select(recommendedPlanDetail => new RecommendedPlanDetailResultDto
                        {
                            Id = recommendedPlanDetail.Id,
                            Name = recommendedPlanDetail.RecommendedPlanDetail.Name,
                            SpotLengthString = string.Join(", ", recommendedPlanDetail.RecommendedPlanDetail.SpotLengths.Select(spotLength => $":{spotLength.Length}")),
                            FlightStartDate = $"{recommendedPlanDetail.RecommendedPlanDetail.FlightStartDate}",
                            FlightEndDate = $"{recommendedPlanDetail.RecommendedPlanDetail.FlightEndDate}",
                            FlightDateString = $"{recommendedPlanDetail.RecommendedPlanDetail.FlightStartDate.ToString(flightStartDateFormat)}-{recommendedPlanDetail.RecommendedPlanDetail.FlightEndDate.ToString(flightEndDateFormat)}",
                            IsRecommendedPlan = recommendedPlanDetail.IsRecommendedPlan,
                            Pacing = _CalculatePacing(recommendedPlanDetail.DeliveredImpressions, recommendedPlanDetail.ContractedImpressions) + "%",
                            RecommendedPlanId = recommendedPlanDetail.RecommendedPlanId,
                            AudienceName = recommendedPlanDetail.AudienceName,
                            Product = _GetProductName(recommendedPlanDetails.RecommendedPlanDetail.AdvertiserMasterId, recommendedPlanDetails.RecommendedPlanDetail.ProductMasterId),
                            DaypartCode = recommendedPlanDetail.DaypartCode,
                            Impressions = recommendedPlanDetail.SpotDeliveredImpressions / 1000,
                            TotalContractedImpressions = recommendedPlanDetail.PlanTotalContractedImpressions / 1000,
                            TotalDeliveredImpressionsSelected = (recommendedPlanDetail.SpotDeliveredImpressions + recommendedPlanDetail.PlanTotalDeliveredImpressions) / 1000,
                            TotalPacingSelected = ((recommendedPlanDetail.PlanTotalDeliveredImpressions + recommendedPlanDetail.SpotDeliveredImpressions) / recommendedPlanDetail.PlanTotalContractedImpressions) * 100,
                            TotalDeliveredImpressionsUnselected = recommendedPlanDetail.PlanTotalDeliveredImpressions / 1000,
                            TotalPacingUnselected = (recommendedPlanDetail.PlanTotalDeliveredImpressions / recommendedPlanDetail.PlanTotalContractedImpressions) * 100,
                            WeeklyContractedImpressions = recommendedPlanDetail.ContractedImpressions / 1000,
                            WeeklyDeliveredImpressionsSelected = (recommendedPlanDetail.DeliveredImpressions + recommendedPlanDetail.SpotDeliveredImpressions) / 1000,
                            WeeklyPacingSelected = _CalculateWeeklyPacingSelected(recommendedPlanDetail.DeliveredImpressions, recommendedPlanDetail.SpotDeliveredImpressions, recommendedPlanDetail.ContractedImpressions),
                            WeeklyDeliveredImpressionsUnselected = recommendedPlanDetail.DeliveredImpressions / 1000,
                            WeeklyPacingUnselected = _calculateWeeklyPacingUnselected(recommendedPlanDetail.DeliveredImpressions, recommendedPlanDetail.ContractedImpressions),
                        }).ToList()
                    };
                    if (recommendedPlanDetailsResult.Plans != null && recommendedPlanDetailsResult.Plans.Any(x => x.IsSelected))
                    {
                        foreach (var planDetail in recommendedPlanDetailsResult.Plans)
                        {
                            planDetail.IsRecommendedPlan = planDetail.IsSelected;
                        }
                    }
                }
                else if(recommendedPlanDetailsDone != null)
                {
                    var recommendedPlanDetails = recommendedPlanDetailsDone.SpotExceptionsRecommendedPlanDetailsDone.First();

                    recommendedPlanDetailsResult = new SpotExceptionsRecommendedPlanDetailsResultDto
                    {
                        Id = recommendedPlanDetailsDone.Id,
                        EstimateId = recommendedPlanDetailsDone.EstimateId,
                        SpotLengthString = recommendedPlanDetailsDone.SpotLength != null ? $":{recommendedPlanDetailsDone.SpotLength.Length}" : null,
                        Product = _GetProductName(recommendedPlanDetails.RecommendedPlanDetail.AdvertiserMasterId, recommendedPlanDetails.RecommendedPlanDetail.ProductMasterId),
                        FlightStartDate = recommendedPlanDetails.RecommendedPlanDetail.FlightStartDate.ToString(),
                        FlightEndDate = recommendedPlanDetails.RecommendedPlanDetail.FlightEndDate.ToString(),
                        FlightDateString = $"{Convert.ToDateTime(recommendedPlanDetails.RecommendedPlanDetail.FlightStartDate).ToString(flightStartDateFormat)}-{Convert.ToDateTime(recommendedPlanDetails.RecommendedPlanDetail.FlightEndDate).ToString(flightEndDateFormat)}",
                        ProgramName = recommendedPlanDetailsDone.ProgramName,
                        ProgramAirDate = recommendedPlanDetailsDone.ProgramAirTime.ToString(programAirDateFormat),
                        ProgramAirTime = recommendedPlanDetailsDone.ProgramAirTime.ToString(programAirTimeFormat),
                        InventorySourceName = recommendedPlanDetailsDone.InventorySource,
                        Plans = recommendedPlanDetailsDone.SpotExceptionsRecommendedPlanDetailsDone.Select(recommendedPlanDetail => new RecommendedPlanDetailResultDto
                        {
                            Id = recommendedPlanDetail.Id,
                            Name = recommendedPlanDetail.RecommendedPlanDetail.Name,
                            SpotLengthString = string.Join(", ", recommendedPlanDetail.RecommendedPlanDetail.SpotLengths.Select(spotLength => $":{spotLength.Length}")),
                            FlightStartDate = $"{recommendedPlanDetail.RecommendedPlanDetail.FlightStartDate}",
                            FlightEndDate = $"{recommendedPlanDetail.RecommendedPlanDetail.FlightEndDate}",
                            FlightDateString = $"{recommendedPlanDetail.RecommendedPlanDetail.FlightStartDate.ToString(flightStartDateFormat)}-{recommendedPlanDetail.RecommendedPlanDetail.FlightEndDate.ToString(flightEndDateFormat)}",
                            IsRecommendedPlan = recommendedPlanDetail.IsRecommendedPlan,
                            Pacing = _CalculatePacing(recommendedPlanDetail.DeliveredImpressions, recommendedPlanDetail.ContractedImpressions) + "%",
                            RecommendedPlanId = recommendedPlanDetail.RecommendedPlanId,
                            AudienceName = recommendedPlanDetail.AudienceName,
                            Product = _GetProductName(recommendedPlanDetails.RecommendedPlanDetail.AdvertiserMasterId, recommendedPlanDetails.RecommendedPlanDetail.ProductMasterId),
                            DaypartCode = recommendedPlanDetail.DaypartCode,
                            Impressions = recommendedPlanDetail.SpotDeliveredImpressions / 1000,
                            TotalContractedImpressions = recommendedPlanDetail.PlanTotalContractedImpressions / 1000,
                            TotalDeliveredImpressionsSelected = (recommendedPlanDetail.SpotDeliveredImpressions + recommendedPlanDetail.PlanTotalDeliveredImpressions) / 1000,
                            TotalPacingSelected = ((recommendedPlanDetail.PlanTotalDeliveredImpressions + recommendedPlanDetail.SpotDeliveredImpressions) / recommendedPlanDetail.PlanTotalContractedImpressions) * 100,
                            TotalDeliveredImpressionsUnselected = recommendedPlanDetail.PlanTotalDeliveredImpressions / 1000,
                            TotalPacingUnselected = (recommendedPlanDetail.PlanTotalDeliveredImpressions / recommendedPlanDetail.PlanTotalContractedImpressions) * 100,
                            WeeklyContractedImpressions = recommendedPlanDetail.ContractedImpressions / 1000,
                            WeeklyDeliveredImpressionsSelected = (recommendedPlanDetail.DeliveredImpressions + recommendedPlanDetail.SpotDeliveredImpressions) / 1000,
                            WeeklyPacingSelected = _CalculateWeeklyPacingSelected(recommendedPlanDetail.DeliveredImpressions, recommendedPlanDetail.SpotDeliveredImpressions, recommendedPlanDetail.ContractedImpressions),
                            WeeklyDeliveredImpressionsUnselected = recommendedPlanDetail.DeliveredImpressions / 1000,
                            WeeklyPacingUnselected = _calculateWeeklyPacingUnselected(recommendedPlanDetail.DeliveredImpressions, recommendedPlanDetail.ContractedImpressions),
                        }).ToList()
                    };
                    if (recommendedPlanDetailsResult.Plans != null &&
                        recommendedPlanDetailsResult.Plans.Any(x => x.IsSelected))
                    {
                        foreach (var planDetail in recommendedPlanDetailsResult.Plans)
                        {
                            planDetail.IsRecommendedPlan = planDetail.IsSelected;
                        }
                    }
                }

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Details");
            }
            catch (CadentException ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Recommended Plan Details";
                throw new CadentException(msg, ex);
            }

            return recommendedPlanDetailsResult;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetRecommendedPlanAdvertisersAsync(SpotExceptionsRecommendedPlanAdvertisersRequestDto recommendedPlansAdvertisersRequest)
        {
            SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest = new SpotExceptionsRecommendedPlansRequestDto();

            List<Guid> recommendedPlanAdvertisersToDo = null;
            List<Guid> recommendedPlanadvertisersDone = null;
            List<string> advertiserName = new List<string>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Advertisers");
            try
            {
                recommendedPlanAdvertisersToDo = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanAdvertisersToDoAsync(recommendedPlansAdvertisersRequest.WeekStartDate, recommendedPlansAdvertisersRequest.WeekEndDate);
                recommendedPlanadvertisersDone = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanAdvertisersDoneAsync(recommendedPlansAdvertisersRequest.WeekStartDate, recommendedPlansAdvertisersRequest.WeekEndDate);

                var advertisersMasterIds = recommendedPlanAdvertisersToDo.Concat(recommendedPlanadvertisersDone).Distinct().ToList();
                if (advertisersMasterIds.Any())
                {
                    advertiserName = advertisersMasterIds.Select(n => _GetAdvertiserName(n) ?? "Unknown").ToList();
                }

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Advertisers");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Recommended Plan Advertisers";
                throw new CadentException(msg, ex);
            }

            return advertiserName;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetRecommendedPlanStationsAsync(SpotExceptionsRecommendedPlanStationsRequestDto recommendedPlanStationsRequest)
        {
            List<string> recommendedPlanStationsToDo;
            List<string> recommendedPlanStationsDone;
            List<string> stations;

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Stations");
            try
            {
                recommendedPlanStationsToDo = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanStationsToDoAsync(recommendedPlanStationsRequest.WeekStartDate, recommendedPlanStationsRequest.WeekEndDate);
                recommendedPlanStationsDone = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanStationsDoneAsync(recommendedPlanStationsRequest.WeekStartDate, recommendedPlanStationsRequest.WeekEndDate);

                stations = recommendedPlanStationsToDo.Concat(recommendedPlanStationsDone).Distinct().ToList();
                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Stations");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Recommended Plan Stations";
                throw new CadentException(msg, ex);
            }

            return stations;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsRecommendedPlanFiltersResultDto> GetRecommendedPlanFilters(SpotExceptionsRecommendedPlansRequestDto recommendedPlanFiltersRequest)
        {
            var recommendedPlanFiltersResult = new SpotExceptionsRecommendedPlanFiltersResultDto();

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Filters");
            try
            {
                var marketFilters = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanMarketFiltersAsync(recommendedPlanFiltersRequest.WeekStartDate, recommendedPlanFiltersRequest.WeekEndDate);
                var legacyCallLetterFilters = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanLegacyCallLetterFiltersAsync(recommendedPlanFiltersRequest.WeekStartDate, recommendedPlanFiltersRequest.WeekEndDate);
                var inventorySourceFilters = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanInventorySourceFiltersAsync(recommendedPlanFiltersRequest.WeekStartDate, recommendedPlanFiltersRequest.WeekEndDate);

                if (marketFilters == null && legacyCallLetterFilters == null && inventorySourceFilters == null)
                {
                    return recommendedPlanFiltersResult;
                }
                recommendedPlanFiltersResult.Markets = marketFilters;
                recommendedPlanFiltersResult.Stations = legacyCallLetterFilters;
                recommendedPlanFiltersResult.InventorySources = inventorySourceFilters;

                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Filters");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Recommended Plan Filters";
                throw new CadentException(msg, ex);
            }

            return recommendedPlanFiltersResult;
        }

        public async Task<bool> HandleSaveRecommendedPlanDecisionsAsync(SpotExceptionsRecommendedPlanSaveDecisionsRequestDto recommendedPlanDecisionsSaveRequest, string userName)
        {
            var isSaved = false;

            _LogInfo($"Starting: Saving Decisions to Recommended Plan");
            try
            {
                if (recommendedPlanDecisionsSaveRequest.SpotExceptionsRecommendedPlans.All(x => x.TodoId != null))
                {
                    isSaved = await _SaveRecommendedPlanToDoDecisionsAsync(recommendedPlanDecisionsSaveRequest, userName);
                }
                else if (recommendedPlanDecisionsSaveRequest.SpotExceptionsRecommendedPlans.All(x => x.DoneId != null))
                {
                    isSaved = await _SaveRecommendedPlanDecisionsDoneAsync(recommendedPlanDecisionsSaveRequest, userName);
                }
                else
                {
                    isSaved = false;
                }

                _LogInfo($"Finished: Saving Decisions to Recommended Plan");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save Decisions to Recommended Plan";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        public async Task<bool> _SaveRecommendedPlanToDoDecisionsAsync(SpotExceptionsRecommendedPlanSaveDecisionsRequestDto recommendedPlanDecisionsSaveRequest, string userName)
        {
            var isSaved = false;
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            _LogInfo($"Starting: Moving the Spot Exception Plan by Decision to Done");
            try
            {
                using (var transaction = new TransactionScopeWrapper())
                {
                    recommendedPlanDecisionsSaveRequest.SpotExceptionsRecommendedPlans.ForEach(async recommendedPlan =>
                    {
                        var todoRecommendedPlan = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanSpot(recommendedPlan.TodoId);
                        var doneRecommendedPlanToAdd = _MapRecommendedPlanDoneToDto(todoRecommendedPlan);

                        foreach (var entity in doneRecommendedPlanToAdd.SpotExceptionsRecommendedPlanDetailsDone)
                        {
                            if (entity.RecommendedPlanId == recommendedPlan.SelectedPlanId)
                            {
                                entity.SpotExceptionsRecommendedPlanDoneDecisions.DecidedBy = userName;
                                entity.SpotExceptionsRecommendedPlanDoneDecisions.DecidedAt = currentDate;
                            }
                        }

                        _SpotExceptionsRecommendedPlanRepository.AddRecommendedPlanToDone(doneRecommendedPlanToAdd, recommendedPlan.SelectedPlanId);
                        _SpotExceptionsRecommendedPlanRepository.DeleteRecommendedPlanFromToDo(todoRecommendedPlan.Id);
                    });
                    transaction.Complete();
                    isSaved = true;
                }
                _LogInfo($"Finished: Moving the Spot Exception Plan by Decision to Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not move the Spot Exception Plan by Decision to Done";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        private async Task<bool> _SaveRecommendedPlanDecisionsDoneAsync(SpotExceptionsRecommendedPlanSaveDecisionsRequestDto recommendedPlanDecisionsSaveRequest, string userName)
        {
            bool isSaved;
            var spotExceptionsRecommendedPlanDone = new List<SpotExceptionsRecommendedPlanSpotDecisionsDoneDto>();
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            _LogInfo($"Starting: Saving decisions for the Done");
            try
            {
                foreach (var spotExceptionsRecommendedPlan in recommendedPlanDecisionsSaveRequest.SpotExceptionsRecommendedPlans)
                {
                    var spotExceptionsRecommendedPlanSpotDoneDecision = new SpotExceptionsRecommendedPlanSpotDecisionsDoneDto
                    {
                        SpotExceptionsId = spotExceptionsRecommendedPlan.DoneId ?? default,
                        SpotExceptionsRecommendedPlanId = spotExceptionsRecommendedPlan.SelectedPlanId
                    };
                    spotExceptionsRecommendedPlanDone.Add(spotExceptionsRecommendedPlanSpotDoneDecision);
                }

                isSaved = await _SpotExceptionsRecommendedPlanRepository.SaveRecommendedPlanDoneDecisionsAsync(spotExceptionsRecommendedPlanDone, userName, currentDate);
                 _LogInfo($"Finished: Saving decisions for the Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save decisions for the Done";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        private SpotExceptionsRecommendedPlanSpotsDoneDto _MapRecommendedPlanDoneToDto(SpotExceptionsRecommendedPlanSpotsToDoDto recommendedPlanSpotsToDoEntity)
        {
            var recommendedPlanDoneEntity = new SpotExceptionsRecommendedPlanSpotsDoneDto
            {
                SpotUniqueHashExternal = recommendedPlanSpotsToDoEntity.SpotUniqueHashExternal,
                AmbiguityCode = recommendedPlanSpotsToDoEntity.AmbiguityCode,
                ExecutionIdExternal = recommendedPlanSpotsToDoEntity.ExecutionIdExternal,
                EstimateId = recommendedPlanSpotsToDoEntity.EstimateId,
                InventorySource = recommendedPlanSpotsToDoEntity.InventorySource,
                HouseIsci = recommendedPlanSpotsToDoEntity.HouseIsci,
                ClientIsci = recommendedPlanSpotsToDoEntity.ClientIsci,
                SpotLengthId = recommendedPlanSpotsToDoEntity.SpotLengthId,
                ProgramAirTime = recommendedPlanSpotsToDoEntity.ProgramAirTime,
                StationLegacyCallLetters = recommendedPlanSpotsToDoEntity.StationLegacyCallLetters,
                Affiliate = recommendedPlanSpotsToDoEntity.Affiliate,
                MarketCode = recommendedPlanSpotsToDoEntity.MarketCode,
                MarketRank = recommendedPlanSpotsToDoEntity.MarketRank,
                ProgramName = recommendedPlanSpotsToDoEntity.ProgramName,
                ProgramGenre = recommendedPlanSpotsToDoEntity.ProgramGenre,
                IngestedBy = recommendedPlanSpotsToDoEntity.IngestedBy,
                IngestedAt = recommendedPlanSpotsToDoEntity.IngestedAt,
                IngestedMediaWeekId = recommendedPlanSpotsToDoEntity.IngestedMediaWeekId,
                SpotLength = recommendedPlanSpotsToDoEntity.SpotLength,
                SpotExceptionsRecommendedPlanDetailsDone = recommendedPlanSpotsToDoEntity.SpotExceptionsRecommendedPlanDetailsToDo.Select(recommendedPlanDetailToDoDb =>
                {
                    var recommendedPlanDetailDone = new SpotExceptionsRecommendedPlanDetailsDoneDto
                    {
                        SpotExceptionsRecommendedPlanId = recommendedPlanDetailToDoDb.SpotExceptionsRecommendedPlanId,
                        RecommendedPlanId = recommendedPlanDetailToDoDb.RecommendedPlanId,
                        ExecutionTraceId = recommendedPlanDetailToDoDb.ExecutionTraceId,
                        Rate = recommendedPlanDetailToDoDb.Rate,
                        AudienceName = recommendedPlanDetailToDoDb.AudienceName,
                        ContractedImpressions = recommendedPlanDetailToDoDb.ContractedImpressions,
                        DeliveredImpressions = recommendedPlanDetailToDoDb.DeliveredImpressions,
                        IsRecommendedPlan = recommendedPlanDetailToDoDb.IsRecommendedPlan,
                        PlanClearancePercentage = recommendedPlanDetailToDoDb.PlanClearancePercentage,
                        DaypartCode = recommendedPlanDetailToDoDb.DaypartCode,
                        StartTime = recommendedPlanDetailToDoDb.StartTime,
                        EndTime = recommendedPlanDetailToDoDb.EndTime,
                        Monday = recommendedPlanDetailToDoDb.Monday,
                        Tuesday = recommendedPlanDetailToDoDb.Tuesday,
                        Wednesday = recommendedPlanDetailToDoDb.Wednesday,
                        Thursday = recommendedPlanDetailToDoDb.Thursday,
                        Friday = recommendedPlanDetailToDoDb.Friday,
                        Saturday = recommendedPlanDetailToDoDb.Saturday,
                        Sunday = recommendedPlanDetailToDoDb.Sunday,
                        SpotDeliveredImpressions = recommendedPlanDetailToDoDb.SpotDeliveredImpressions,
                        PlanTotalContractedImpressions = recommendedPlanDetailToDoDb.PlanTotalContractedImpressions,
                        PlanTotalDeliveredImpressions = recommendedPlanDetailToDoDb.PlanTotalDeliveredImpressions,
                        IngestedMediaWeekId = recommendedPlanDetailToDoDb.IngestedMediaWeekId,
                        IngestedBy = recommendedPlanDetailToDoDb.IngestedBy,
                        IngestedAt = recommendedPlanDetailToDoDb.IngestedAt,
                        SpotUniqueHashExternal = recommendedPlanDetailToDoDb.SpotUniqueHashExternal,
                        ExecutionIdExternal = recommendedPlanDetailToDoDb.ExecutionIdExternal
                    };
                    return recommendedPlanDetailDone;
                }).ToList()
            };
            return recommendedPlanDoneEntity;
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

        private string _GetProductName(Guid? productId, Guid? masterId)
        {
            string productName = null;
            if (masterId.HasValue)
            {
                productName = _AabEngine.GetAdvertiserProduct(productId.Value, masterId.Value)?.Name;
            }
            return productName;
        }

        private double? _CalculatePacing(double? deliveredImp, double? contractedImp)
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

        private double? _CalculateWeeklyPacingSelected(double? deliveredImp, double? spotDeliveredImp, double? contractedImp)
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
