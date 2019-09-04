using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.Entities.Enums;
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
        List<CampaignDto> GetCampaigns(CampaignFilterDto filter, DateTime currentDate);

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

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignService"/> class.
        /// </summary>
        /// <param name="dataRepositoryFactory">The data repository factory.</param>
        /// <param name="campaignValidator">The class which contains validation logic for campaigns</param>
        /// <param name="mediaMonthAndWeekAggregateCache">The class which contains cached in memory media monts and media weeks</param>
        /// <param name="quarterCalculationEngine">The engine that does quarter calculations</param>
        public CampaignService(
            IDataRepositoryFactory dataRepositoryFactory,
            ICampaignValidator campaignValidator,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IQuarterCalculationEngine quarterCalculationEngine,
            ITrafficApiClient trafficApiClient,
            ILockingManagerApplicationService lockingManagerApplicationService)
        {
            _CampaignRepository = dataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _CampaignValidator = campaignValidator;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _TrafficApiClient = trafficApiClient;
            _LockingManagerApplicationService = lockingManagerApplicationService;
        }

        /// <inheritdoc />
        public List<CampaignDto> GetCampaigns(CampaignFilterDto filter, DateTime currentDate)
        {
            if (!_IsFilterValid(filter))
                filter = _GetDefaultFilter(currentDate);
            var quarterDetail = _QuarterCalculationEngine.GetQuarterDetail(filter.Quarter.Quarter, filter.Quarter.Year);
            var campaigns = _CampaignRepository.GetCampaigns(quarterDetail.StartDate, quarterDetail.EndDate, filter.PlanStatus);

            _SetAgencies(campaigns);
            _SetAdvertisers(campaigns);

            return campaigns;
        }

        private void _SetAgencies(List<CampaignDto> campaigns)
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

        private void _SetAdvertisers(List<CampaignDto> campaigns)
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

            campaign.Agency = _TrafficApiClient.GetAgency(campaign.Agency.Id);
            campaign.Advertiser = _TrafficApiClient.GetAdvertiser(campaign.Advertiser.Id);

            if (campaign.HasPlans)
            {
                _SetCampaignStubData(campaign);
                _SetPlansStubData(campaign);
            }

            return campaign;
        }

        private void _SetCampaignStubData(CampaignDto campaign)
        {
            campaign.FlightStartDate = new DateTime(2019, 4, 15);
            campaign.FlightEndDate = new DateTime(2019, 6, 2);
            campaign.FlightHiatusDays = 5;
            campaign.FlightActiveDays = 44;
            campaign.HasHiatus = true;
            campaign.Budget = 150;
            campaign.CPM = 11;
            campaign.Impressions = 13637;
            campaign.Rating = 11.46;
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

