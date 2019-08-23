using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices.Campaigns;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.ApplicationServices.CampaignService;

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
        /// Gets the advertisers.
        /// </summary>
        /// <returns></returns>
        List<AdvertiserDto> GetAdvertisers();

        /// <summary>
        /// Gets the agencies.
        /// </summary>
        /// <returns></returns>
        List<AgencyDto> GetAgencies();

        /// <summary>
        /// Gets the quarters.
        /// </summary>
        /// <param name="currentDate">The date for the default quarter.</param>
        /// <returns></returns>
        CampaignQuartersDto GetQuarters(DateTime currentDate);
    }

    /// <summary>
    /// Operations related to the Campaign domain.
    /// </summary>
    /// <seealso cref="ICampaignService" />
    public class CampaignService : ICampaignService
    {
        #region Fields

        private readonly ICampaignServiceData _CampaignData;
        private readonly ICampaignValidator _CampaignValidator;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;

        #endregion // #region Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignService"/> class.
        /// </summary>
        /// <param name="dataRepositoryFactory">The data repository factory.</param>
        /// <param name="smsClient">The SMS client.</param>
        /// <param name="mediaMonthAndWeekAggregateCache">The media month and week aggregate cache.</param>
        /// <param name="quarterCalculationEngine">The quarter calculation engine</param>
        public CampaignService(IDataRepositoryFactory dataRepositoryFactory, 
                               ISMSClient smsClient, 
                               IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
                               IQuarterCalculationEngine quarterCalculationEngine)
        {
            var campaignRepository = dataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _CampaignData = new CampaignServiceData(campaignRepository, smsClient);
            _CampaignValidator = new CampaignValidator(_CampaignData);
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _QuarterCalculationEngine = quarterCalculationEngine;
        }

        #endregion // #region Constructor

        #region Operations

        /// <inheritdoc />
        public List<CampaignDto> GetCampaigns(CampaignFilterDto filter, DateTime currentDate)
        {
            if (!_IsFilterValid(filter))
                filter = _GetDefaultFilter(currentDate);
            var data = GetCampaignServiceData();
            var quarterDetail = _QuarterCalculationEngine.GetQuarterDetail(filter.Quarter.Quarter, filter.Quarter.Year);
            var campaigns = data.GetCampaigns(quarterDetail);
            return campaigns;
        }

        /// <inheritdoc />
        public CampaignDto GetCampaignById(int campaignId)
        {
            var data = GetCampaignServiceData();
            var campaign = data.GetCampaign(campaignId);
            return campaign;
        }

        /// <inheritdoc />
        public List<AdvertiserDto> GetAdvertisers()
        {
            var data = GetCampaignServiceData();
            var items = data.GetAdvertisers();
            return items;
        }

        /// <inheritdoc />
        public List<AgencyDto> GetAgencies()
        {
            var data = GetCampaignServiceData();
            var items = data.GetAgencies();
            return items;
        }

        /// <inheritdoc />
        public int SaveCampaign(CampaignDto campaign, string createdBy, DateTime createdDate)
        {
            var validator = GetCampaignValidator();
            validator.Validate(campaign);

            campaign.ModifiedBy = createdBy;
            campaign.ModifiedDate = createdDate;

            var data = GetCampaignServiceData();
            return data.SaveCampaign(campaign, createdBy, createdDate);
        }

        /// <inheritdoc />
        public CampaignQuartersDto GetQuarters(DateTime currentDate)
        {
            var data = GetCampaignServiceData();
            var dates = data.GetCampaignsDateRanges();
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

        #endregion // #region Operations

        #region Helpers

        /// <summary>
        /// Gets the campaign data.
        /// </summary>
        /// <returns></returns>
        protected virtual ICampaignServiceData GetCampaignServiceData()
        {
            return _CampaignData;
        }

        /// <summary>
        /// Gets the campaign validator.
        /// </summary>
        /// <returns></returns>
        protected virtual ICampaignValidator GetCampaignValidator()
        {
            return _CampaignValidator;
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
            return filter != null && filter.Quarter != null;
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
                }
            };
        }

        #endregion // #region Helpers
    }
}

