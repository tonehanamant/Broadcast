using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Hangfire;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
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
        int SaveCampaign(CampaignDto campaign, string userName, DateTime createdDate);

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
        /// <returns></returns>
        List<LookupDto> GetStatuses(QuarterDto quarter);

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
        void ProcessCampaignAggregation(int campaignId);
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
        private readonly ITrafficApiClient _TrafficApiClient;
        private readonly ILockingManagerApplicationService _LockingManagerApplicationService;
        private readonly ICampaignAggregator _CampaignAggregator;
        private readonly ICampaignSummaryRepository _CampaignSummaryRepository;
        private readonly ICampaignAggregationJobTrigger _CampaignAggregationJobTrigger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignService"/> class.
        /// </summary>
        /// <param name="dataRepositoryFactory">The data repository factory.</param>
        /// <param name="campaignValidator">The campaign validator.</param>
        /// <param name="mediaMonthAndWeekAggregateCache">The media month and week aggregate cache.</param>
        /// <param name="quarterCalculationEngine">The quarter calculation engine.</param>
        /// <param name="trafficApiClient">The traffic API client.</param>
        /// <param name="lockingManagerApplicationService">The locking manager application service.</param>
        /// <param name="campaignAggregator">The campaign aggregator.</param>
        /// <param name="campaignAggregationJobTrigger">The campaign aggregation job trigger.</param>
        public CampaignService(
            IDataRepositoryFactory dataRepositoryFactory,
            ICampaignValidator campaignValidator,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            ITrafficApiClient trafficApiClient,
            ILockingManagerApplicationService lockingManagerApplicationService,
            ICampaignAggregator campaignAggregator,
            ICampaignAggregationJobTrigger campaignAggregationJobTrigger)
        {
            _CampaignRepository = dataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _CampaignValidator = campaignValidator;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _TrafficApiClient = trafficApiClient;
            _LockingManagerApplicationService = lockingManagerApplicationService;
            _CampaignAggregator = campaignAggregator;
            _CampaignSummaryRepository = dataRepositoryFactory.GetDataRepository<ICampaignSummaryRepository>();
            _CampaignAggregationJobTrigger = campaignAggregationJobTrigger;
        }

        /// <inheritdoc />
        public List<CampaignListItemDto> GetCampaigns(CampaignFilterDto filter, DateTime currentDate)
        {
            if (!_IsFilterValid(filter))
                filter = _GetDefaultFilter(currentDate);
            var quarterDetail = _QuarterCalculationEngine.GetQuarterDetail(filter.Quarter.Quarter, filter.Quarter.Year);
            var campaigns = _CampaignRepository.GetCampaigns(quarterDetail.StartDate, quarterDetail.EndDate, filter.PlanStatus);

            _SetAgencies(campaigns);
            _SetAdvertisers(campaigns);

            return campaigns;
        }

        private void _SetAgencies(List<CampaignListItemDto> campaigns)
        {
            const int cachingDurationInSeconds = 300;

            var cache = new BaseMemoryCache<AgencyDto>("localAgenciesCache");
            
            foreach (var campaign in campaigns)
            {
                // Let`s cache agencies to reduce the number of requests to the traffic API
                campaign.Agency = cache.GetOrCreate(
                    campaign.Agency.Id.ToString(), 
                    () => _TrafficApiClient.GetAgency(campaign.Agency.Id),
                    new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(cachingDurationInSeconds) });
            }
        }

        private void _SetAdvertisers(List<CampaignListItemDto> campaigns)
        {
            const int cachingDurationInSeconds = 300;

            var cache = new BaseMemoryCache<List<AdvertiserDto>>("localAdvertisersCache");

            foreach (var campaign in campaigns)
            {
                // Let`s cache advertisers to reduce the number of requests to the traffic API
                var advertisers = cache.GetOrCreate(
                    campaign.Agency.Id.ToString(),
                    () => _TrafficApiClient.GetAdvertisersByAgencyId(campaign.Agency.Id),
                    new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(cachingDurationInSeconds) });

                campaign.Advertiser = advertisers.Single(
                    x => x.Id == campaign.Advertiser.Id, 
                    "Cannot find an advertiser with id: " + campaign.Advertiser.Id);
            }
        }

        /// <inheritdoc />
        public CampaignDto GetCampaignById(int campaignId)
        {
            var campaign = _CampaignRepository.GetCampaign(campaignId);
            var summary = _CampaignSummaryRepository.GetSummaryForCampaign(campaignId);

            _HydrateCampaignWithSummary(campaign, summary);

            _SetPlansStubData(campaign);

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
            campaign.CPM = summary.CPM;
            campaign.Impressions = summary.Impressions;
            campaign.Rating = summary.Rating;

            campaign.CampaignStatus = summary.CampaignStatus;
            campaign.PlanStatuses = _MapToPlanStatuses(summary);
        }

        private List<PlansStatusCountDto> _MapToPlanStatuses(CampaignSummaryDto summary)
        {
            var statuses = new List<PlansStatusCountDto>();
            EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Working, summary.PlanStatusCountWorking);
            EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Reserved, summary.PlanStatusCountReserved);
            EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.ClientApproval, summary.PlanStatusCountClientApproval);
            EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Contracted, summary.PlanStatusCountContracted);
            EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Live, summary.PlanStatusCountLive);
            EvaluateAndAddPlanStatus(statuses, PlanStatusEnum.Complete, summary.PlanStatusCountComplete);
            return statuses;
        }

        private void EvaluateAndAddPlanStatus(List<PlansStatusCountDto> planStatuses, PlanStatusEnum status, int? candidate)
        {
            if (candidate > 0)
            {
                planStatuses.Add(new PlansStatusCountDto { PlanStatus = status, Count = candidate.Value });
            }
        }

        private void _SetPlansStubData(CampaignDto campaign)
        {
            foreach (var plan in campaign.Plans)
            {
                plan.HHImpressions = 10543.78d;
                plan.HHCPM = 46.3m;
            }
        }

        /// <inheritdoc />
        public int SaveCampaign(CampaignDto campaign, string createdBy, DateTime createdDate)
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
        }

        /// <inheritdoc />
        public CampaignQuartersDto GetQuarters(PlanStatusEnum? planStatus, DateTime currentDate)
        {
            var dates = _CampaignRepository.GetCampaignsDateRanges(planStatus);
            var validDateRanges = _ValidateDateRanges(dates);
            var allMediaMonths = new List<MediaMonth>();

            foreach (var range in validDateRanges)
            {
                var mediaMonths = _MediaMonthAndWeekAggregateCache.GetMediaMonthsBetweenDatesInclusive(range.Start.Value, range.End.Value);
                allMediaMonths.AddRange(mediaMonths);
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

        public List<LookupDto> GetStatuses(QuarterDto quarter)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (quarter != null)
            {
                var quarterDetail = _QuarterCalculationEngine.GetQuarterDetail(quarter.Quarter, quarter.Year);
                startDate = quarterDetail.StartDate;
                endDate = quarterDetail.EndDate;
            }

            var statuses = _CampaignRepository.GetCampaignsPlanStatuses(startDate, endDate);

            return statuses.Select(x => new LookupDto { Id = (int)x, Display = x.Description() }).OrderBy(x => x.Id).ToList(); ;
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
    }
}

