using Common.Services.ApplicationServices;
using Services.Broadcast.Clients;
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
        private readonly ITrafficApiClient _TrafficApiClient;

        public ProductService(ITrafficApiClient trafficApiClient)
        {
            _TrafficApiClient = trafficApiClient;
        }

        /// <inheritdoc />
        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            return _TrafficApiClient.GetProductsByAdvertiserId(advertiserId);
        }
    }
}
