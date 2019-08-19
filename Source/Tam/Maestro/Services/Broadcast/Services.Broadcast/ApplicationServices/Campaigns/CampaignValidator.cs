using Services.Broadcast.Entities;
using System;
using System.Linq;

namespace Services.Broadcast.ApplicationServices.Campaigns
{
    /// <summary>
    /// Validates the fields of a Campaign.
    /// </summary>
    public interface ICampaignValidator
    {
        /// <summary>
        /// Validates the specified campaign.
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        void Validate(CampaignDto campaign);
    }

    /// <summary>
    /// Validates the fields of a Campaign.
    /// </summary>
    public class CampaignValidator : ICampaignValidator
    {
        #region Constants

        public const string InvalidAdvertiserErrorMessage = "The advertiser id is invalid, please provide a valid and active id";
        public const string InvalidAgencyErrorMessage = "The agency id is invalid, please provide a valid and active id";
        public const string InvalidCampaignNameErrorMessage = "The campaign name is invalid, please provide a valid name";
        public const string InvalidCampaignNotesErrorMessage = "The campaign notes are invalid";

        #endregion // #region Constants

        #region Fields

        private readonly ICampaignServiceData _CampaignServiceData;

        #endregion // #region Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignValidator"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public CampaignValidator(ICampaignServiceData data)
        {
            _CampaignServiceData = data;
        }

        #endregion // #region Constructor

        #region Operations

        /// <inheritdoc />
        public void Validate(CampaignDto campaign)
        {
            _ValidateCampaignName(campaign);
            _ValidateAdvertiser(campaign);
            _ValidateAgency(campaign);
            _ValidateNotes(campaign);
        }

        #endregion // #region Operations

        #region Helpers

        /// <summary>
        /// Gets the campaign service data.
        /// </summary>
        /// <returns></returns>
        protected ICampaignServiceData GetCampaignServiceData()
        {
            return _CampaignServiceData;
        }

        private void _ValidateCampaignName(CampaignDto campaign)
        {
            if (string.IsNullOrWhiteSpace(campaign.Name))
            {
                throw new InvalidOperationException(InvalidCampaignNameErrorMessage);
            }

            const int nameMaxLength = 255;
            if (campaign.Name.Length > nameMaxLength)
            {
                throw new InvalidOperationException(InvalidCampaignNameErrorMessage);
            }
        }
        
        private void _ValidateAdvertiser(CampaignDto campaign)
        {
            var data = GetCampaignServiceData();
            var advertisers = data.GetAdvertisers();
            var found = advertisers.Any(a => a.Id.Equals(campaign.AdvertiserId));
            if (found == false)
            {
                throw new InvalidOperationException(InvalidAdvertiserErrorMessage);
            }
        }

        private void _ValidateAgency(CampaignDto campaign)
        {
            var data = GetCampaignServiceData();
            var agencies = data.GetAgencies();
            var found = agencies.Any(a => a.Id.Equals(campaign.AgencyId));
            if (found == false)
            {
                throw new InvalidOperationException(InvalidAgencyErrorMessage);
            }
        }

        private void _ValidateNotes(CampaignDto campaign)
        {
            const int notesMaxLength = 1024;
            if ((campaign.Notes?.Length ?? 0) > notesMaxLength)
            {
                throw new InvalidOperationException(InvalidCampaignNotesErrorMessage);
            }
        }

        #endregion // #region Helpers
    }
}