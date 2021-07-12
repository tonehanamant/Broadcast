using Newtonsoft.Json;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Cache
{
    public interface IInventorySummaryCache
    {
        List<InventorySummaryDto> GetOrCreate(InventorySummaryFilterDto keyObject, Func<List<InventorySummaryDto>> createItemForKeyFunc);
        long GetItemCount(bool reset);
    }

    public class InventorySummaryCache : BaseMemoryCache<List<InventorySummaryDto>>, IInventorySummaryCache
    {
        private readonly IFeatureToggleHelper _FeatureToggleHelper;
        private readonly Lazy<bool> _IsPipelineVariablesEnabled;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;

        public InventorySummaryCache(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base("InventorySummaryCache") 
        {
            _FeatureToggleHelper = featureToggleHelper;
            _IsPipelineVariablesEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_PIPELINE_VARIABLES));
            _ConfigurationSettingsHelper = configurationSettingsHelper;
        }

        public List<InventorySummaryDto> GetOrCreate(InventorySummaryFilterDto keyObject, Func<List<InventorySummaryDto>> createItemForKeyFunc)
        {
            var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var keyJsonString = JsonConvert.SerializeObject(keyObject, Formatting.None, jsonSettings);
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(_IsPipelineVariablesEnabled.Value ? _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.InventorySummaryCacheAbsoluteExpirationSeconds, 3600) : BroadcastServiceSystemParameter.InventorySummaryCacheAbsoluteExpirationSeconds) };

            var item = GetOrCreate(keyJsonString, createItemForKeyFunc, policy);

            return item;
        }

    }
}
