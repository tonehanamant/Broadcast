using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.SystemComponentParameters;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    /// <summary>
    /// Operations related to the Campaign domain.
    /// </summary>
    /// <seealso cref="Common.Services.ApplicationServices.IApplicationService" />
    public interface ICampaignService : IApplicationService
    {
        /// <summary>
        /// Gets all campaigns.
        /// </summary>
        /// <returns></returns>
        List<CampaignListItemDto> GetCampaigns(CampaignFilterDto filter, DateTime currentDate);

        /// <summary>
        /// Gets the campaign.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        CampaignDto GetCampaignById(int campaignId);

        /// <summary>
        /// Saves the campaign.
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>Id of the new campaign</returns>
        int SaveCampaign(SaveCampaignDto campaign, string userName, DateTime createdDate);

        /// <summary>
        /// Gets the quarters.
        /// <param name="planStatus">The status to filter quarter by</param>
        /// <param name="currentDate">The date for the default quarter.</param>
        /// <returns></returns>
        CampaignQuartersDto GetQuarters(PlanStatusEnum? planStatus, DateTime currentDate);

        /// <summary>
        /// Gets the statuses based on the quarter.
        /// </summary>
        /// <param name="quarter">The quarter</param>
        /// <param name="year">The year</param>
        /// <returns></returns>
        List<LookupDto> GetStatuses(int? quarter, int? year);

        /// <summary>
        /// Triggers the campaign aggregation job.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="queuedBy">The queued by.</param>
        string TriggerCampaignAggregationJob(int campaignId, string queuedBy);

        /// <summary>
        /// Processes the campaign aggregation.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        [Queue("campaignaggregation")]
        [DisableConcurrentExecution(300)]
        void ProcessCampaignAggregation(int campaignId);

        /// <summary>
        /// Gets the campaign defaults.
        /// </summary>
        /// <returns>Gets the default values for creating a campaign in the form of <see cref="CampaignDefaultsDto"/></returns>
        CampaignDefaultsDto GetCampaignDefaults();
    }

    /// <summary>
    /// Operations related to the Campaign domain.
    /// </summary>
    /// <seealso cref="ICampaignService" />
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignValidator _CampaignValidator;
        private readonly ICampaignRepository _CampaignRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly ILockingManagerApplicationService _LockingManagerApplicationService;
        private readonly ICampaignAggregator _CampaignAggregator;
        private readonly ICampaignSummaryRepository _CampaignSummaryRepository;
        private readonly ICampaignAggregationJobTrigger _CampaignAggregationJobTrigger;
        private readonly ITrafficApiCache _TrafficApiCache;

        public CampaignService(
            IDataRepositoryFactory dataRepositoryFactory,
            ICampaignValidator campaignValidator,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            ILockingManagerApplicationService lockingManagerApplicationService,
            ICampaignAggregator campaignAggregator,
            ICampaignAggregationJobTrigger campaignAggregationJobTrigger,
            ITrafficApiCache trafficApiCache)
        {
            _CampaignRepository = dataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _CampaignValidator = campaignValidator;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _CampaignAggregator = campaignAggregator;
            _CampaignSummaryRepository = dataRepositoryFactory.GetDataRepository<ICampaignSummaryRepository>();
            _CampaignAggregationJobTrigger = campaignAggregationJobTrigger;
            _TrafficApiCache = trafficApiCache;
        }

        /// <inheritdoc />
        public List<CampaignListItemDto> GetCampaigns(CampaignFilterDto filter, DateTime currentDate)
        {
            if (!_IsFilterValid(filter))
                filter = _GetDefaultFilter(currentDate);

            var dateRange = _GetQuarterDateRange(filter.Quarter);
            var campaigns = _CampaignRepository.GetCampaignsWithSummary(dateRange.Start, dateRange.End, filter.PlanStatus)
                .Select(x => _MapToCampaignListItemDto(x)).ToList();
            var cacheAdvertisers = new BaseMemoryCache<List<AdvertiserDto>>("localAdvertisersCache");

            foreach (var campaign in campaigns)
            {
                if (!campaign.CampaignStatus.HasValue)
                {
                    campaign.CampaignStatus = PlanStatusEnum.Working;
                }

                campaign.Agency = _TrafficApiCache.GetAgency(campaign.Agency.Id);
                campaign.Advertiser = _TrafficApiCache.GetAdvertiser(campaign.Advertiser.Id);
            }

            return campaigns;
        }

        /// <inheritdoc />
        public CampaignDto GetCampaignById(int campaignId)
        {
            var campaign = _CampaignRepository.GetCampaign(campaignId);
            var summary = _CampaignSummaryRepository.GetSummaryForCampaign(campaignId);

            _HydrateCampaignWithSummary(campaign, summary);

            return campaign;
        }

        private void _HydrateCampaignWithSummary(CampaignDto campaign, CampaignSummaryDto summary)
        {
            if (summary == null)
            {
                return;
            }

            campaign.FlightStartDate = summary.FlightStartDate;
            campaign.FlightEndDate = summary.FlightEndDate;
            campaign.FlightHiatusDays = summary.FlightHiatusDays;
            campaign.FlightActiveDays = summary.FlightActiveDays;
            campaign.HasHiatus = campaign.FlightHiatusDays > 0;

            campaign.Budget = summary.Budget;
            campaign.HouseholdCPM = summary.HouseholdCPM;
            campaign.HouseholdImpressions = summary.HouseholdImpressions;
            campaign.HouseholdRatingPoints = summary.HouseholdRatingPoints;

            campaign.CampaignStatus = summary.CampaignStatus;
            campaign.PlanStatuses = _MapToPlanStatuses(summary);
        }
        
        private CampaignListItemDto _MapToCampaignListItemDto(CampaignWithSummary campaignAndCampaignSummary)
        {
            var campaign = new CampaignListItemDto
            {
                Id = campaignAndCampaignSummary.Campaign.Id,
                Name = campaignAndCampaignSummary.Campaign.Name,
                Advertiser = new AdvertiserDto { Id = campaignAndCampaignSummary.Campaign.AdvertiserId },
                Agency = new AgencyDto { Id = campaignAndCampaignSummary.Campaign.AgencyId },
                Notes = campaignAndCampaignSummary.Campaign.Notes,
                ModifiedDate = campaignAndCampaignSummary.Campaign.ModifiedDate,
                ModifiedBy = campaignAndCampaignSummary.Campaign.ModifiedBy,

                HasPlans = campaignAndCampaignSummary.Campaign.Plans != null &&
                    campaignAndCampaignSummary.Campaign.Plans.Any(),
                Plans = campaignAndCampaignSummary.Campaign.Plans,
            };
            campaign.Plans?.ForEach(plan => plan.HasHiatus = plan.TotalHiatusDays.HasValue && plan.TotalHiatusDays.Value > 0);

            if (campaignAndCampaignSummary.CampaignSummary != null)
            {
                campaign.FlightStartDate = campaignAndCampaignSummary.CampaignSummary.FlightStartDate;
                campaign.FlightEndDate = campaignAndCampaignSummary.CampaignSummary.FlightEndDate;
                campaign.FlightHiatusDays = campaignAndCampaignSummary.CampaignSummary.FlightHiatusDays;
                campaign.FlightActiveDays = campaignAndCampaignSummary.CampaignSummary.FlightActiveDays;
                campaign.HasHiatus = 
                    campaignAndCampaignSummary.CampaignSummary.FlightHiatusDays.HasValue && 
                    campaignAndCampaignSummary.CampaignSummary.FlightHiatusDays.Value > 0;

                campaign.Budget = campaignAndCampaignSummary.CampaignSummary.Budget;
                campaign.HouseholdCPM = campaignAndCampaignSummary.CampaignSummary.HouseholdCPM;
                campaign.HouseholdImpressions = campaignAndCampaignSummary.CampaignSummary.HouseholdImpressions;
                campaign.HouseholdRatingPoints = campaignAndCampaignSummary.CampaignSummary.HouseholdRatingPoints;
                campaign.CampaignStatus = campaignAndCampaignSummary.CampaignSummary.CampaignStatus;

                campaign.PlanStatuses = _MapToPlanStatuses(campaignAndCampaignSummary.CampaignSummary);
            }

            return campaign;
        }
        
        private List<PlansStatusCountDto> _MapToPlanStatuses(CampaignSummaryDto summary)
        {
            var statuses = new List<PlansStatusCountDto>();
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Working, summary.PlanStatusCountWorking);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Reserved, summary.PlanStatusCountReserved);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.ClientApproval, summary.PlanStatusCountClientApproval);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Contracted, summary.PlanStatusCountContracted);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Live, summary.PlanStatusCountLive);
            _EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Complete, summary.PlanStatusCountComplete);
            return statuses;
        }

        private void _EvaluateAndAddPlanStatus(List<PlansStatusCountDto> planStatuses, PlanStatusEnum status, int? candidate)
        {
            if (candidate > 0)
            {
                planStatuses.Add(new PlansStatusCountDto { PlanStatus = status, Count = candidate.Value });
            }
        }

        /// <inheritdoc />
        public int SaveCampaign(SaveCampaignDto campaign, string createdBy, DateTime createdDate)
        {
            _CampaignValidator.Validate(campaign);

            campaign.ModifiedBy = createdBy;
            campaign.ModifiedDate = createdDate;

            if (campaign.Id == 0)
            {
                return _CampaignRepository.CreateCampaign(campaign, createdBy, createdDate);
            }
            else
            {
                //if (SafeBroadcastServiceSystemParameter.EnableCampaignsLocking)
                if (true)
                {
                    var key = KeyHelper.GetCampaignLockingKey(campaign.Id);
                    var lockingResult = _LockingManagerApplicationService.LockObject(key);

                    if (lockingResult.Success)
                    {
                        return _CampaignRepository.UpdateCampaign(campaign);
                    }
                    else
                    {
                        throw new Exception($"The chosen campaign has been locked by {lockingResult.LockedUserName}");
                    }
                }
                else
                {
                    return _CampaignRepository.UpdateCampaign(campaign);
                }
            }
        }

        /// <inheritdoc />
        public CampaignQuartersDto GetQuarters(PlanStatusEnum? planStatus, DateTime currentDate)
        {
            var dates = _CampaignRepository.GetCampaignsDateRanges(planStatus);
            var validDateRanges = _ValidateDateRanges(dates);
            var allMediaMonths = new List<MediaMonth>();

            if (validDateRanges.Any())
            {
                var min = validDateRanges.Min(x => x.Start.Value);
                var max = validDateRanges.Max(x => x.End.Value);
                var mediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsBetweenDatesInclusive(min, max);

                foreach (var range in validDateRanges)
                {
                    var mediaMonthsForRange = mediaMonths.Where(x => x.StartDate <= range.End.Value && x.EndDate >= range.Start.Value);
                    allMediaMonths.AddRange(mediaMonthsForRange);
                }
            }

            var quarters = allMediaMonths.GroupBy(x => new { x.Quarter, x.Year })
                .Select(x => _QuarterCalculationEngine.GetQuarterDetail(x.Key.Quarter, x.Key.Year))
                .ToList();

            var currentQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(currentDate);

            if (!quarters.Any(x => x.Quarter == currentQuarter.Quarter &&
                                   x.Year == currentQuarter.Year))
                quarters.Add(currentQuarter);

            return new CampaignQuartersDto
            {
                DefaultQuarter = currentQuarter,
                Quarters = quarters.OrderByDescending(x => x.Year).ThenByDescending(x => x.Quarter).ToList()
            };
        }

        /// <inheritdoc />
        public List<LookupDto> GetStatuses(int? quarter, int? year)
        {
            QuarterDto quarterDto = null;

            if (quarter.HasValue && year.HasValue)
            {
                quarterDto = new QuarterDto
                {
                    Quarter = quarter.Value,
                    Year = year.Value
                };
            }

            var dateRange = _GetQuarterDateRange(quarterDto);

            var statuses = _CampaignRepository.GetCampaignsPlanStatuses(dateRange.Start, dateRange.End);

            return statuses.Select(x => new LookupDto { Id = (int)x, Display = x.Description() }).OrderBy(x => x.Id).ToList(); ;
        }

        private DateRange _GetQuarterDateRange(QuarterDto quarterDto)
        {
            if (quarterDto == null)
                return new DateRange(null, null);
            var quarterDetail = _QuarterCalculationEngine.GetQuarterDetail(quarterDto.Quarter, quarterDto.Year);
            return new DateRange(quarterDetail.StartDate, quarterDetail.EndDate);
        }

        /// <inheritdoc />
        public string TriggerCampaignAggregationJob(int campaignId, string queuedBy)
        {
            return _CampaignAggregationJobTrigger.TriggerJob(campaignId, queuedBy);
        }

        /// <summary>
        /// Processes the campaign aggregation.
        /// </summary>
        /// <remarks>
        /// Called by an external process.
        /// Let the exceptions bubble out so they are recorded by the external processor.
        /// </remarks>
        /// <param name="campaignId">The campaign identifier.</param>
        public void ProcessCampaignAggregation(int campaignId)
        {
            try
            {
                var summary = _CampaignAggregator.Aggregate(campaignId);
                summary.ProcessingStatus = CampaignAggregationProcessingStatusEnum.Completed;
                summary.LastAggregated = DateTime.Now;
                summary.ProcessingErrorMessage = null;
                _CampaignSummaryRepository.SaveSummary(summary);
            }
            catch (Exception e)
            {
                _CampaignSummaryRepository.SetSummaryProcessingStatusToError(campaignId, $"Exception caught during processing : {e.Message}");
                // re-throw so that the caller can track the failure.
                throw e;
            }
        }

        private List<DateRange> _ValidateDateRanges(List<DateRange> dateRanges)
        {
            var nonEmptyRanges = dateRanges.Where(x => !x.IsEmpty());
            var validStartDate = nonEmptyRanges.Where(x => x.Start != null);
            var hasEndDate = validStartDate.Where(x => x.End != null);
            var missingEndDate = validStartDate.Where(x => x.End == null);

            foreach (var dateRange in missingEndDate)
                dateRange.End = dateRange.Start;

            var allValidDateRanges = hasEndDate.Concat(missingEndDate).ToList();

            return allValidDateRanges;
        }

        private bool _IsFilterValid(CampaignFilterDto filter)
        {
            return filter != null && (filter.Quarter != null || filter.PlanStatus != null);
        }

        private CampaignFilterDto _GetDefaultFilter(DateTime currentDate)
        {
            var quarter = _QuarterCalculationEngine.GetQuarterRangeByDate(currentDate);

            return new CampaignFilterDto
            {
                Quarter = new QuarterDto
                {
                    Quarter = quarter.Quarter,
                    Year = quarter.Year
                },
                PlanStatus = null
            };
        }

        /// <inheritdoc />
        public CampaignDefaultsDto GetCampaignDefaults()
        {
            // return a blank object now, until requirements get updated
            var campaignDefaultValues = new CampaignDefaultsDto
            {
                Name = string.Empty,
                AgencyId = null,
                AdvertiserId = null,
                Notes = string.Empty
            };

            return campaignDefaultValues;
        }
    }
}

