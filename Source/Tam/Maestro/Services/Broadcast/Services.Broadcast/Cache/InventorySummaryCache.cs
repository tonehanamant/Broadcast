using Newtonsoft.Json;
using Services.Broadcast.Entities.InventorySummary;
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
        public InventorySummaryCache() : base("InventorySummaryCache") { }

        public List<InventorySummaryDto> GetOrCreate(InventorySummaryFilterDto keyObject, Func<List<InventorySummaryDto>> createItemForKeyFunc)
        {
            var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var keyJsonString = JsonConvert.SerializeObject(keyObject, Formatting.None, jsonSettings);
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddSeconds(BroadcastServiceSystemParameter.InventorySummaryCacheAbsoluteExpirationSeconds) };

            var item = GetOrCreate(keyJsonString, createItemForKeyFunc, policy);

            return item;
        }

    }
}
