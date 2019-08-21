using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices.Campaigns
{
    /// <summary>
    /// Data operations in the Campaign domain.
    /// </summary>
    public interface ICampaignServiceData
    {
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
        /// Gets all campaigns.
        /// </summary>
        /// <returns></returns>
        List<CampaignDto> GetAllCampaigns();

        /// <summary>
        /// Gets the campaign.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        CampaignDto GetCampaign(int campaignId);

        /// <summary>
        /// Creates the campaign.
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>Id of the new campaign</returns>
        int SaveCampaign(CampaignDto campaign, string createdBy, DateTime createdDate);
    }

    /// <summary>
    /// Data operations in the Campaign domain.
    /// </summary>
    /// <seealso cref="ICampaignServiceData" />
    public class CampaignServiceData : ICampaignServiceData
    {
        #region Fields

        private ISMSClient _SmsClient;
        private ICampaignRepository _CampaignRepository;

        #endregion // #region Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignServiceData"/> class.
        /// </summary>
        /// <param name="campaignRepository">The campaign repository.</param>
        /// <param name="smsClient">The SMS client.</param>
        public CampaignServiceData(ICampaignRepository campaignRepository, ISMSClient smsClient)
        {
            _CampaignRepository = campaignRepository;
            _SmsClient = smsClient;
        }

        #endregion // #region Constructor

        #region Operations

        /// <inheritdoc />
        public List<AdvertiserDto> GetAdvertisers()
        {
            // TODO : Sms Api Call tbd
            /* PRI 11436 : Update SMS contract to enable Broadcast
             * GetAllAdvertisers(string filter) <--- for type-ahead, if null/empty, return all
                returns List<{id, agencyId, name}>
             */
            var items = new List<AdvertiserDto>
            {
                new AdvertiserDto{Id = 1, Name = "Advertiser1"},
                new AdvertiserDto{Id = 2, Name = "Advertiser2"},
                new AdvertiserDto{Id = 3, Name = "Advertiser3"}
            };
            return items;
        }

        /// <inheritdoc />
        public List<AgencyDto> GetAgencies()
        {
            // TODO : Sms Api Call tbd
            /* PRI 11436 : Update SMS contract to enable Broadcast
             * GetAllAgencies(string filter) <--- for type-ahead, if null/empty, return all
                returns List<{id, name}>             
             */
            var items = new List<AgencyDto>
            {
                new AgencyDto{ Id = 1, Name = "Agency1"},
                new AgencyDto{ Id = 2, Name = "Agency2"},
                new AgencyDto{ Id = 3, Name = "Agency3"}
            };
            return items;
        }

        /// <inheritdoc />
        public List<CampaignDto> GetAllCampaigns()
        {
            var repo = GetCampaignRepository();
            List<CampaignDto> campaigns = repo.GetAllCampaigns();
            return campaigns;
        }

        /// <inheritdoc />
        public CampaignDto GetCampaign(int campaignId)
        {
            var repo = GetCampaignRepository();
            var campaign = repo.GetCampaign(campaignId);

            if (campaign.HasPlans)
            {
                _SetCampaignStubData(campaign);
            }

            return campaign;
        }

        /// <inheritdoc />
        public int SaveCampaign(CampaignDto campaign, string createdBy, DateTime createdDate)
        {
            var repo = GetCampaignRepository();
            if(campaign.Id == 0)
            {
                return repo.CreateCampaign(campaign, createdBy, createdDate);
            }
            else
            {
                return repo.UpdateCampaign(campaign);
            }            
        }

        #endregion // #region Operations

        #region Helpers

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

        /// <summary>
        /// Gets the SMS client.
        /// </summary>
        /// <returns></returns>
        protected ISMSClient GetSmsClient()
        {
            return _SmsClient;
        }

        /// <summary>
        /// Gets the campaign repository.
        /// </summary>
        /// <returns></returns>
        protected ICampaignRepository GetCampaignRepository()
        {
            return _CampaignRepository;
        }

        #endregion // #region Helpers
    }
}