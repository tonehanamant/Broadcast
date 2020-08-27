using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface IEnvironmentService : IApplicationService
    {
        Dictionary<string, string> GetDbInfo();
        EnvironmentDto GetEnvironmentInfo();

        bool IsFeatureToggleEnabled(string key, string username);

        bool IsFeatureToggleEnabledUserAnonymous(string key);

        /// <summary>
        /// Authenticates the given username against the Launch Darkly application.
        /// Returns a client hash for the client to then use as the authenticated token when communicating with the Launch Darkly application directly.
        /// </summary>
        /// <param name="username">The username to authenticate.</param>
        /// <returns>A client hash for the client to then use as the authenticated token when communicating with the Launch Darkly application directly.</returns>
        string AuthenticateUserAgainstLaunchDarkly(string username);
    }
    public class EnvironmentService: BroadcastBaseClass, IEnvironmentService
    {
        private readonly IRatingsRepository _RatingsRepo;
        private readonly IInventoryFileRepository _InventoryFileRepo;
        private readonly IFeatureToggleHelper _FeatureToggleHelper;

        public EnvironmentService(IDataRepositoryFactory broadcastDataRepositoryFactory, IFeatureToggleHelper featureToggleHelper)
        {
            _RatingsRepo = broadcastDataRepositoryFactory.GetDataRepository<IRatingsRepository>();
            _InventoryFileRepo = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _FeatureToggleHelper = featureToggleHelper;
        }

        public Dictionary<string, string> GetDbInfo()
        {
            var result = new Dictionary<string, string>();
            result.Add("broadcast", _InventoryFileRepo.GetDbInfo());
            result.Add("broadcastforecast", _RatingsRepo.GetDbInfo());
            return result;
        }

        /// <inheritdoc />
        public string AuthenticateUserAgainstLaunchDarkly(string username)
        {
            try
            {
                var clientHash = _FeatureToggleHelper.Authenticate(username);
                return clientHash;
            }
            catch (Exception ex)
            {
                _LogError($"Error authenticating user '{username}' against the Launch Darkly application.", ex);
                return null;
            }
        }

        public bool IsFeatureToggleEnabled(string key, string username)
        {
            return _FeatureToggleHelper.IsToggleEnabled(key, username);
        }

        public bool IsFeatureToggleEnabledUserAnonymous(string key)
        {
            return _FeatureToggleHelper.IsToggleEnabledUserAnonymous(key);
        }

        // TODO : Tech-Debt to remove these and have the FE got to LaunchDarkly directly
        private const string FEATURE_TOGGLE_DISPLAY_CAMPAIGN_LINK = "broadcast-display-campaign-link";
        private const string FEATURE_TOGGLE_DISPLAY_BUYING_LINK = "broadcast-display-buying-link";
        private const string FEATURE_TOGGLE_ALLOW_MULTIPLE_CREATIVE_LENGTHS = "broadcast-allow-multiple-creative-lengths";
        private const string FEATURE_TOGGLE_ENABLE_PRICING_IN_EDIT = "broadcast-enable-pricing-in-edit";
        private const string FEATURE_TOGGLE_ENABLE_EXPORT_PRE_BUY = "broadcast-enable-export-pre-buy";
        private const string FEATURE_TOGGLE_RUN_PRICING_AUTOMATICALLY = "broadcast-enable-run-pricing-automatically";

        public EnvironmentDto GetEnvironmentInfo()
        {
            return new EnvironmentDto
            {
                Environment = new AppSettings().Environment.ToString(),
                DisplayCampaignLink = IsFeatureToggleEnabledUserAnonymous(FEATURE_TOGGLE_DISPLAY_CAMPAIGN_LINK),
                DisplayBuyingLink = IsFeatureToggleEnabledUserAnonymous(FEATURE_TOGGLE_DISPLAY_BUYING_LINK),
                AllowMultipleCreativeLengths = IsFeatureToggleEnabledUserAnonymous(FEATURE_TOGGLE_ALLOW_MULTIPLE_CREATIVE_LENGTHS),
                EnablePricingInEdit = IsFeatureToggleEnabledUserAnonymous(FEATURE_TOGGLE_ENABLE_PRICING_IN_EDIT),
                EnableExportPreBuy = IsFeatureToggleEnabledUserAnonymous(FEATURE_TOGGLE_ENABLE_EXPORT_PRE_BUY),
                EnableRunPricingAutomaticaly = IsFeatureToggleEnabledUserAnonymous(FEATURE_TOGGLE_RUN_PRICING_AUTOMATICALLY)
            };
        }
    }
}
