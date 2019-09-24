using Common.Services.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.DTO;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProductService : IApplicationService
    {
        /// <summary>
        /// Returns products for a specific advertiser
        /// </summary>
        List<ProductDto> GetProductsByAdvertiserId(int advertiserId);
    }

    public class ProductService : IProductService
    {
        private readonly ITrafficApiCache _TrafficApiCache;

        public ProductService(ITrafficApiCache trafficApiCache)
        {
            _TrafficApiCache = trafficApiCache;
        }

        /// <inheritdoc />
        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            return _TrafficApiCache.GetProductsByAdvertiserId(advertiserId);
        }
    }
}
