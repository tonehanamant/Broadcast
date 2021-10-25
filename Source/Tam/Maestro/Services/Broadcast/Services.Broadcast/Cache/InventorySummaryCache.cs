using Newtonsoft.Json;
using Services.Broadcast.Entities.InventorySummary;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Services.Broadcast.Cache
{
    public interface IInventorySummaryCache
    {
        List<InventorySummaryDto> GetOrCreate(InventorySummaryFilterDto keyObject, Func<List<InventorySummaryDto>> createItemForKeyFunc);
        long GetItemCount(bool reset);
    }

    public class InventorySummaryCache : BaseMemoryCache<List<InventorySummaryDto>>, IInventorySummaryCache
    {
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;

        public InventorySummaryCache(IConfigurationSettingsHelper configurationSettingsHelper) : base("InventorySummaryCache") 
        {
            _ConfigurationSettingsHelper = configurationSettingsHelper;
        }

        public List<InventorySummaryDto> GetOrCreate(InventorySummaryFilterDto keyObject, Func<List<InventorySummaryDto>> createItemForKeyFunc)
        {
            var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var keyJsonString = JsonConvert.SerializeObject(keyObject, Formatting.None, jsonSettings);
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(_ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.InventorySummaryCacheAbsoluteExpirationSeconds, 3600)) };

            var item = GetOrCreate(keyJsonString, createItemForKeyFunc, policy);

            return item;
        }

    }
}
