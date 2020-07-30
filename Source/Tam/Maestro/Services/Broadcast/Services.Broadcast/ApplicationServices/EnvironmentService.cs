using System;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface IEnvironmentService : IApplicationService
    {
        Dictionary<string, string> GetDbInfo();
        EnvironmentDto GetEnvironmentInfo();

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

        public EnvironmentDto GetEnvironmentInfo()
        {
            // As soon as FE implements LaunchDarkly hooks we can remove returning the toggles from here.
            const string toggleKeyEnableAabNavigation = "broadcast-enable-aab-navigation";

            return new EnvironmentDto
            {
                Environment = new AppSettings().Environment.ToString(),
                DisplayCampaignLink = BroadcastServiceSystemParameter.DisplayCampaignLink,
                DisplayBuyingLink = BroadcastServiceSystemParameter.DisplayBuyingLink,
                AllowMultipleCreativeLengths = BroadcastServiceSystemParameter.AllowMultipleCreativeLengths,
                EnablePricingInEdit = BroadcastFeaturesSystemParameter.EnablePricingInEdit,
                EnableExportPreBuy = BroadcastFeaturesSystemParameter.EnableExportPreBuy,
                EnableAabNavigation = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(toggleKeyEnableAabNavigation)
            };
        }
    }
}
