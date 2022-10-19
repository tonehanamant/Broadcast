using Amazon.Runtime;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using Unity.Injection;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsRecommendedPlanService : IApplicationService
    {
        /// <summary>
        /// Gets the spot exception recommended plans asynchronous.
        /// </summary>
        /// <param name="recommendedPlansRequest">The spot exceptions recommended plans request.</param>
        /// <returns></returns>
        Task<SpotExceptionsRecommendedPlansResultsDto> GetRecommendedPlansAsync(SpotExceptionsRecommendedPlansRequestDto recommendedPlansRequest);

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
        Task<RecommendedPlanFiltersResultDto> GetRecommendedPlanFilters(SpotExceptionsRecommendedPlansRequestDto recomendedPlanRequest);

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
        public async Task<SpotExceptionsRecommendedPlansResultsDto> GetRecommendedPlansAsync(SpotExceptionsRecommendedPlansRequestDto recommendedPlansRequest)
        {
            const string flightStartDateFormat = "MM/dd";
            const string flightEndDateFormat = "MM/dd/yyyy";
            var recommendedPlans = new SpotExceptionsRecommendedPlansResultsDto();

            try
            {
                _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plans");
                var recommendedPlanToDo = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlansToDoAsync(recommendedPlansRequest.WeekStartDate, recommendedPlansRequest.WeekEndDate);
                var recommendedPlanDone = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlansDoneAsync(recommendedPlansRequest.WeekStartDate, recommendedPlansRequest.WeekEndDate);

                if (recommendedPlanToDo?.Any() ?? false)
                {
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
                                AffectedSpotsCount = recommendedPlanToDo.SelectMany(x => x.SpotExceptionsRecommendedPlanDetailsToDo.Where(y => y.RecommendedPlanId == activeRecommendedPlan.Key.RecommendedPlanId)).Count(),
                                Impressions = planDetails.SpotExceptionsRecommendedPlanDetailsToDo.Where(x => x.IsRecommendedPlan).Select(x => x.SpotDeliveredImpressions).First() / 1000,
                                SpotLengthString = $":{_SpotLengthRepository.GetSpotLengthById(planDetails.SpotLengthId ?? 0)}" ?? null,
                                AudienceName = planDetails.SpotExceptionsRecommendedPlanDetailsToDo.Where(x => x.IsRecommendedPlan).Select(x => x.AudienceName).First(),
                                FlightString = $"{Convert.ToDateTime(flightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(flightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(flightStartDate), Convert.ToDateTime(flightEndDate)).ToString() + " " + "Weeks"})",
                            };
                        }).ToList();
                }

                if (recommendedPlanDone?.Any() ?? false)
                {
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
                                AffectedSpotsCount = recommendedPlanDone.SelectMany(x => x.SpotExceptionsRecommendedPlanDetailsDone.Where(y => y.RecommendedPlanId == completedRecommendedPlan.Key.RecommendedPlanId)).Count(),
                                Impressions = planDetails.SpotExceptionsRecommendedPlanDetailsDone.Where(x => x.IsRecommendedPlan).Select(x => x.SpotDeliveredImpressions).First() / 1000,
                                SpotLengthString = $":{_SpotLengthRepository.GetSpotLengthById(planDetails.SpotLengthId ?? 0)}" ?? null,
                                AudienceName = planDetails.SpotExceptionsRecommendedPlanDetailsDone.Where(x => x.IsRecommendedPlan).Select(x => x.AudienceName).First(),
                                FlightString = $"{Convert.ToDateTime(flightStartDate).ToString(flightStartDateFormat)} - {Convert.ToDateTime(flightEndDate).ToString(flightEndDateFormat)}" + " " + $"({_GetTotalNumberOfWeeks(Convert.ToDateTime(flightStartDate), Convert.ToDateTime(flightEndDate)).ToString() + " " + "Weeks"})",
                            };
                        }).ToList();
                }
                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plans");
            }
            catch (CadentException ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Recommended Plans";
                throw new CadentException(msg, ex);
            }

            return recommendedPlans;
        }

        /// <inheritdoc />
        public async Task<SpotExceptionsRecommendedPlanSpotsResultDto> GetRecommendedPlanSpotsAsync(SpotExceptionsRecommendedPlanSpotsRequestDto recomendedPlanSpotsRequest)
        {
            var recommendedPlanSpots = new SpotExceptionsRecommendedPlanSpotsResultDto();

            try
            {
                _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Spots");
                var recommendedPlanSpotsToDo = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanSpotsToDo(recomendedPlanSpotsRequest.PlanId, recomendedPlanSpotsRequest.WeekStartDate, recomendedPlanSpotsRequest.WeekEndDate);

                if (recommendedPlanSpotsToDo?.Any() ?? false)
                {
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
                            RecommendedPlan = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).First(),
                            Impressions = planDetails.Select(x => x.SpotDeliveredImpressions).First() / 1000,
                            ProgramName = activePlan.ProgramName,
                            Affiliate = activePlan.Affiliate,
                            PlanId = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).First(),
                            Market = _GetMarketName(activePlan.MarketCode ?? 0),
                            Station = activePlan.StationLegacyCallLetters,
                            InventorySource = activePlan.InventorySource
                        };
                    }).ToList();
                }

                var recommendedPlanSpotsDone = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanSpotsDone(recomendedPlanSpotsRequest.PlanId, recomendedPlanSpotsRequest.WeekStartDate, recomendedPlanSpotsRequest.WeekEndDate);

                if (recommendedPlanSpotsDone?.Any() ?? false)
                {
                    recommendedPlanSpots.Queued = recommendedPlanSpotsDone?.Where(d => d.SpotExceptionsRecommendedPlanDetailsDone.Any(s => s.SpotExceptionsRecommendedPlanDoneDecisions.SyncedAt == null))
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

                    recommendedPlanSpots.Synced = recommendedPlanSpotsDone?.Where(d => d.SpotExceptionsRecommendedPlanDetailsDone.Any(s => s.SpotExceptionsRecommendedPlanDoneDecisions.SyncedAt != null))
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
                            RecommendedPlan = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanDetail).Select(y => y.Name).First(),
                            Impressions = syncedPlanDetails.SpotExceptionsRecommendedPlanDetailsDone.Select(x => x.SpotDeliveredImpressions).First() / 1000,
                            ProgramName = syncedPlan.ProgramName,
                            Affiliate = syncedPlan.Affiliate,
                            PlanId = planDetails.Where(x => x.IsRecommendedPlan).Select(x => x.RecommendedPlanId).First(),
                            Market = _GetMarketName(syncedPlan.MarketCode ?? 0),
                            Station = syncedPlan.StationLegacyCallLetters,
                            InventorySource = syncedPlan.InventorySource,
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
                    var recommendedPlanDetails = recommendedPlanDetailsToDo.SpotExceptionsRecommendedPlanDetailsToDo.First(x => x.IsRecommendedPlan);

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
                    if (recommendedPlanDetailsResult.Plans != null &&
                        recommendedPlanDetailsResult.Plans.Any(x => x.IsSelected))
                    {
                        foreach (var planDetail in recommendedPlanDetailsResult.Plans)
                        {
                            planDetail.IsRecommendedPlan = planDetail.IsSelected;
                        }
                    }
                }
                else if(recommendedPlanDetailsDone != null)
                {
                    var recommendedPlanDetails = recommendedPlanDetailsDone.SpotExceptionsRecommendedPlanDetailsDone.First(x => x.IsRecommendedPlan);

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
            SpotExceptionsRecommendedPlansRequestDto recommendedPlanStationsToDoRequest = new SpotExceptionsRecommendedPlansRequestDto();

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
        public async Task<RecommendedPlanFiltersResultDto> GetRecommendedPlanFilters(SpotExceptionsRecommendedPlansRequestDto recommendedPlanFiltersRequest)
        {
            var recommendedPlanFiltersResult = new RecommendedPlanFiltersResultDto();

            _LogInfo($"Starting: Retrieving Spot Exceptions Recommended Plan Filters");
            try
            {
                var recommendedSpotsToDoResult = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlansToDoAsync(recommendedPlanFiltersRequest.WeekStartDate, recommendedPlanFiltersRequest.WeekEndDate);
                var marketCodesToDo = recommendedSpotsToDoResult.Select(x => x.MarketCode ?? 0).ToList();
                var stationsToDo = recommendedSpotsToDoResult.Select(spotResults => spotResults.StationLegacyCallLetters ?? "Unknown").Distinct().ToList();
                var inventorySourcesToDo = recommendedSpotsToDoResult.Select(spotResults => spotResults.InventorySource ?? "Unknown").Distinct().ToList();

                var recommendedSpotsDoneResult = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlansDoneAsync(recommendedPlanFiltersRequest.WeekStartDate, recommendedPlanFiltersRequest.WeekEndDate);
                var marketCodesDone = recommendedSpotsDoneResult.Select(x => x.MarketCode ?? 0).ToList();
                var stationsDone = recommendedSpotsDoneResult.Select(spotResults => spotResults.StationLegacyCallLetters ?? "Unknown").Distinct().ToList();
                var inventorySourcesDone = recommendedSpotsDoneResult.Select(spotResults => spotResults.InventorySource ?? "Unknown").Distinct().ToList();

                if (recommendedSpotsToDoResult == null && recommendedSpotsDoneResult == null)
                {
                    return recommendedPlanFiltersResult;
                }

                var marketCodes = marketCodesToDo.Concat(marketCodesDone).Distinct().ToList();
                recommendedPlanFiltersResult.Markets = _GetMarketNames(marketCodes).OrderBy(market => market).ToList();

                recommendedPlanFiltersResult.Stations = stationsToDo.Concat(stationsDone).Distinct().OrderBy(station => station).ToList();

                recommendedPlanFiltersResult.InventorySources = inventorySourcesToDo.Concat(inventorySourcesDone).Distinct().OrderBy(inventorySource => inventorySource).ToList();
                
                _LogInfo($"Finished: Retrieving Spot Exceptions Recommended Plan Filters");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Recommended Plan Filters";
                throw new CadentException(msg, ex);
            }

            return recommendedPlanFiltersResult;
        }

        /// <inheritdoc />
        public async Task<bool> HandleSaveRecommendedPlanDecisionsAsync(SpotExceptionsRecommendedPlanSaveDecisionsRequestDto recommendedPlanDecisionsSaveRequest, string userName)
        {
            bool isSaved = false;

            _LogInfo($"Starting: Handle the saving of the Spot Exceptions Decisions");
            try
            {
                foreach (var recommendedPlans in recommendedPlanDecisionsSaveRequest.SpotExceptionsRecommendedPlans)
                {
                    var recommendedPlansToDo = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanDetailsToDoByRecommendedPlanId(recommendedPlans.Id, recommendedPlans.SelectedPlanId);
                    var recommendedPlansDone = await _SpotExceptionsRecommendedPlanRepository.GetRecommendedPlanDetailsDoneByRecommendedPlanId(recommendedPlans.Id, recommendedPlans.SelectedPlanId);

                    if (recommendedPlansToDo == null && recommendedPlansDone == null)
                    {
                        return false;
                    }

                    if (recommendedPlansToDo != null)
                    {
                        isSaved = await _SaveRecommendedPlanDecisionsToDoAsync(recommendedPlans, recommendedPlansToDo, userName);
                    }
                    else
                    {
                        isSaved = await _SaveRecommendedPlanDecisionsDoneAsync(recommendedPlans, recommendedPlansDone, userName);
                    }
                }
                _LogInfo($"Finished: Handle the saving of the Spot Exceptions Decisions");
            }
            catch (Exception ex)
            {
                var msg = $"Could not save the Decisions";
                throw new CadentException(msg, ex);
            }

            return isSaved;
        }

        private async Task<bool> _SaveRecommendedPlanDecisionsToDoAsync(SpotExceptionsRecommendedPlanSaveDto planToSave, SpotExceptionsRecommendedPlanSpotsToDoDto recommendedPlansToDo, string userName)
        {
            bool isMoved = false;
            var currentDate = _DateTimeEngine.GetCurrentMoment();

            _LogInfo($"Starting: Moving the Spot Exception Plan by Decision to Done");
            try
            {
                isMoved = await _SpotExceptionsRecommendedPlanRepository.SaveRecommendedPlanToDoDecisionsAsync(planToSave, recommendedPlansToDo, userName, currentDate);


                _LogInfo($"Finished: Moving the Spot Exception Plan by Decision to Done");
            }
            catch (Exception ex)
            {
                var msg = $"Could not move Spot Exception Recommended Plan to Done";
                throw new CadentException(msg, ex);
            }

            return isMoved;
        }

        private async Task<bool> _SaveRecommendedPlanDecisionsDoneAsync(SpotExceptionsRecommendedPlanSaveDto spotExceptionsRecommendedPlan, SpotExceptionsRecommendedPlanSpotsDoneDto recommendedPlansDone, string userName)
        {
            bool isrecommendedPlanDecisionSaved;
            try
            {
                var recommendedPlanDoneDecision = new SpotExceptionsRecommendedPlanSpotDecisionsDoneDto
                {
                    SpotExceptionsId = spotExceptionsRecommendedPlan.Id,
                    SpotExceptionsRecommendedPlanId = spotExceptionsRecommendedPlan.SelectedPlanId,
                    SpotExceptionsRecommendedPlanDetailsDoneId = recommendedPlansDone.SpotExceptionsRecommendedPlanDetailsDone.Where(x => x.Id == spotExceptionsRecommendedPlan.Id && x.RecommendedPlanId == spotExceptionsRecommendedPlan.SelectedPlanId).Select(f => f.SpotExceptionsRecommendedPlanDoneDecisions.SpotExceptionsRecommendedPlanDetailsDoneId).First(),
                    DecidedBy = userName,
                    DecidedAt = _DateTimeEngine.GetCurrentMoment(),
                    SyncedAt = null,
                    SyncedBy = null
                };

                isrecommendedPlanDecisionSaved = await _SpotExceptionsRecommendedPlanRepository.SaveRecommendedPlanDoneDecisionsAsync(recommendedPlanDoneDecision);
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve the data from the Database";
                throw new CadentException(msg, ex);
            }

            return isrecommendedPlanDecisionSaved;
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
