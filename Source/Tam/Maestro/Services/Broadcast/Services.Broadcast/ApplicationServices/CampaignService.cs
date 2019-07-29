using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.ApplicationServices.Campaigns;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Tam.Maestro.Services.Clients;

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
        List<CampaignDto> GetAllCampaigns();

        /// <summary>
        /// Gets the campaign.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        CampaignDto GetCampaignById(int campaignId);

        /// <summary>
        /// Creates the campaign.
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="createdDate">The created date.</param>
        void CreateCampaign(CampaignDto campaign, string userName, DateTime createdDate);

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
    }

    /// <summary>
    /// Operations related to the Campaign domain.
    /// </summary>
    /// <seealso cref="Services.Broadcast.ApplicationServices.ICampaignService" />
    public class CampaignService : ICampaignService
    {
        #region Fields

        private readonly ICampaignServiceData _CampaignData;
        private readonly ICampaignValidator _CampaignValidator;

        #endregion // #region Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignService"/> class.
        /// </summary>
        /// <param name="dataRepositoryFactory">The data repository factory.</param>
        /// <param name="smsClient">The SMS client.</param>
        public CampaignService(IDataRepositoryFactory dataRepositoryFactory, ISMSClient smsClient)
        {
            var campaignRepository = dataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _CampaignData = new CampaignServiceData(campaignRepository, smsClient);
            _CampaignValidator = new CampaignValidator(_CampaignData);
        }

        #endregion // #region Constructor

        #region Operations

        /// <inheritdoc />
        public List<CampaignDto> GetAllCampaigns()
        {
            var data = GetCampaignServiceData();
            var campaigns = data.GetAllCampaigns();
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
        public void CreateCampaign(CampaignDto campaign, string createdBy, DateTime createdDate)
        {
            var validator = GetCampaignValidator();
            validator.Validate(campaign);

            campaign.ModifiedBy = createdBy;
            campaign.ModifiedDate = createdDate;

            var data = GetCampaignServiceData();
            data.CreateCampaign(campaign, createdBy, createdDate);
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

        #endregion // #region Helpers
    }
}
