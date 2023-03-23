using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        string GetBroadcastAppFolderPath();

        Task<string> TestNotifyFluidityPlanAsync(int planId, int planVersionId);
    }
    public class EnvironmentService: BroadcastBaseClass, IEnvironmentService
    {
        private readonly IRatingsRepository _RatingsRepo;
        private readonly IInventoryFileRepository _InventoryFileRepo;
        private readonly ICampaignServiceApiClient _CampaignServiceApiClient;

        public EnvironmentService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            ICampaignServiceApiClient campaignServiceApiClient,
            IFeatureToggleHelper featureToggleHelper,
            IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {
            _RatingsRepo = broadcastDataRepositoryFactory.GetDataRepository<IRatingsRepository>();
            _InventoryFileRepo = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
            _CampaignServiceApiClient = campaignServiceApiClient;
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

        public EnvironmentDto GetEnvironmentInfo()
        {
            var environmentInfo = new EnvironmentDto
            {
                Environment = new AppSettings().Environment.ToString(),
                HostName = Environment.MachineName,               
                // Keep These : these are referenced by the NavBar.cshtml           
                DisplayBuyingLink = IsFeatureToggleEnabledUserAnonymous(FeatureToggles.DISPLAY_BUYING_LINK)
            };
            
            return environmentInfo;
        }

        public string GetBroadcastAppFolderPath()
        { 
            return base._GetBroadcastAppFolder();
        }

        public async Task<string> TestNotifyFluidityPlanAsync(int planId, int planVersionId)
        {
            _LogInfo("Attempting to do _CampaignServiceApiClient.NotifyFluidityPlanAsync");
            await _CampaignServiceApiClient.NotifyFluidityPlanAsync(planId, planVersionId);
            _LogInfo("Successfully did _CampaignServiceApiClient.NotifyFluidityPlanAsync");
            return "success!";
        }
    }
}
