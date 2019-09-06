using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using System;
using System.Linq;

namespace Services.Broadcast.Validators
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

        private readonly ITrafficApiClient _TrafficApiClient;

        #endregion // #region Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignValidator"/> class.
        /// </summary>
        /// <param name="trafficApiClient">The client to access web API of the traffic project</param>
        public CampaignValidator(ITrafficApiClient trafficApiClient)
        {
            _TrafficApiClient = trafficApiClient;
        }

        #endregion // #region Constructor

        #region Operations

        /// <inheritdoc />
        public void Validate(CampaignDto campaign)
        {
            _ValidateCampaignName(campaign);
            _ValidateAgency(campaign);
            _ValidateAdvertiser(campaign);
            _ValidateNotes(campaign);
        }

        #endregion // #region Operations

        #region Helpers

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
            try
            {
                _TrafficApiClient.GetAdvertiser(campaign.AdvertiserId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(InvalidAdvertiserErrorMessage, ex);
            }
        }

        private void _ValidateAgency(CampaignDto campaign)
        {
            try
            {
                _TrafficApiClient.GetAgency(campaign.AgencyId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(InvalidAgencyErrorMessage, ex);
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