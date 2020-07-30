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
    }
    public class EnvironmentService: IEnvironmentService
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

        public EnvironmentDto GetEnvironmentInfo()
        {
            // This should be the logged in user, but they're not logged in yet.
            // This is ok for now.
            // As soon as FE implements LaunchDarkly hooks we can remove returning the toggles from here.
            const string broadcastUserString = "broadcast_user";
            const string toggleKeyEnableAabNavigation = "broadcast-enable-aab-navigation";

            return new EnvironmentDto
            {
                Environment = new AppSettings().Environment.ToString(),
                DisplayCampaignLink = BroadcastServiceSystemParameter.DisplayCampaignLink,
                DisplayBuyingLink = BroadcastServiceSystemParameter.DisplayBuyingLink,
                AllowMultipleCreativeLengths = BroadcastServiceSystemParameter.AllowMultipleCreativeLengths,
                EnablePricingInEdit = BroadcastFeaturesSystemParameter.EnablePricingInEdit,
                EnableExportPreBuy = BroadcastFeaturesSystemParameter.EnableExportPreBuy,
                EnableAabNavigation = _FeatureToggleHelper.IsToggleEnabled(toggleKeyEnableAabNavigation, broadcastUserString)
            };
        }
    }
}
