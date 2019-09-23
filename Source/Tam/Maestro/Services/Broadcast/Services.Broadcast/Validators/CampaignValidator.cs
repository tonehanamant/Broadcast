﻿using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using System;
using System.Linq;
using Services.Broadcast.Cache;

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

        private readonly ITrafficApiClient _TrafficApiClient;
        private readonly IAgencyCache _AgencyCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignValidator"/> class.
        /// </summary>
        /// <param name="trafficApiClient">The traffic API client.</param>
        /// <param name="agencyCache">The agency cache.</param>
        public CampaignValidator(ITrafficApiClient trafficApiClient, IAgencyCache agencyCache)
        {
            _TrafficApiClient = trafficApiClient;
            _AgencyCache = agencyCache;
        }

        /// <inheritdoc />
        public void Validate(SaveCampaignDto campaign)
        {
            _ValidateCampaignName(campaign);
            _ValidateAgency(campaign);
            _ValidateAdvertiser(campaign);
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

        private void _ValidateAdvertiser(SaveCampaignDto campaign)
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

        private void _ValidateAgency(SaveCampaignDto campaign)
        {
            try
            {
                _AgencyCache.GetAgency(campaign.AgencyId);
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