using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System;

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
        void Validate(SaveCampaignDto campaign);
    }

    /// <summary>
    /// Validates the fields of a Campaign.
    /// </summary>
    public class CampaignValidator : ICampaignValidator
    {
        public const string InvalidAdvertiserErrorMessage = "The advertiser id is invalid, please provide a valid and active id";
        public const string InvalidAgencyErrorMessage = "The agency id is invalid, please provide a valid and active id";
        public const string InvalidCampaignNameErrorMessage = "The campaign name is invalid, please provide a valid name";
        public const string InvalidCampaignNameLengthErrorMessage = "Campaign name cannot be longer than 255 characters.";
        public const string InvalidCampaignNotesErrorMessage = "Campaign notes cannot be longer than 1024 characters";

        private readonly IAabEngine _AabEngine;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignValidator"/> class.
        /// </summary>
        /// <param name="aabEngine">The aab engine.</param>
        /// <param name="featureToggleHelper">The feature toggle helper.</param>
        public CampaignValidator(IAabEngine aabEngine, 
            IFeatureToggleHelper featureToggleHelper)
        {
            _AabEngine = aabEngine;
            _FeatureToggleHelper = featureToggleHelper;
        }

        /// <inheritdoc />
        public void Validate(SaveCampaignDto campaign)
        {
            var isAabEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_AAB_NAVIGATION);
            _ValidateCampaignName(campaign);
            _ValidateAgency(campaign, isAabEnabled);
            _ValidateAdvertiser(campaign, isAabEnabled);
            _ValidateNotes(campaign);
        }

        private void _ValidateCampaignName(SaveCampaignDto campaign)
        {
            if (string.IsNullOrWhiteSpace(campaign.Name))
            {
                throw new InvalidOperationException(InvalidCampaignNameErrorMessage);
            }

            const int nameMaxLength = 255;
            if (campaign.Name.Length > nameMaxLength)
            {
                throw new InvalidOperationException(InvalidCampaignNameLengthErrorMessage);
            }
        }

        private void _ValidateAdvertiser(SaveCampaignDto campaign, bool isAabEnabled)
        {
            try
            {
                if (isAabEnabled)
                {
                    _AabEngine.GetAdvertiser(campaign.AdvertiserMasterId.Value);
                }
                else
                {
                    _AabEngine.GetAdvertiser(campaign.AdvertiserId.Value);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(InvalidAdvertiserErrorMessage, ex);
            }
        }

        private void _ValidateAgency(SaveCampaignDto campaign, bool isAabEnabled)
        {
            try
            {
                if (isAabEnabled)
                {
                    _AabEngine.GetAgency(campaign.AgencyMasterId.Value);
                }
                else
                {
                    _AabEngine.GetAgency(campaign.AgencyId.Value);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(InvalidAgencyErrorMessage, ex);
            }
        }

        private void _ValidateNotes(SaveCampaignDto campaign)
        {
            const int notesMaxLength = 1024;
            if ((campaign.Notes?.Length ?? 0) > notesMaxLength)
            {
                throw new InvalidOperationException(InvalidCampaignNotesErrorMessage);
            }
        }
    }
}