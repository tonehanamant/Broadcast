using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class TrafficApiCacheStub : ITrafficApiCache
    {
        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            if(advertiserId == -1)
            {
                throw new InvalidOperationException(CampaignValidator.InvalidAdvertiserErrorMessage);
            }
            return new AdvertiserDto { Id = advertiserId, AgencyId = 1, Name = "Stub Advertiser" };
        }

        public List<AdvertiserDto> GetAdvertisers()
        {
            return new List<AdvertiserDto> { new AdvertiserDto { Id = 1, AgencyId = 1, Name = "Stub Advertiser" } };
        }

        public List<AgencyDto> GetAgencies()
        {
            return new List<AgencyDto> { new AgencyDto { Id = 1, Name = "Stub Agency" } };
        }

        public AgencyDto GetAgency(int agencyId)
        {
            return new AgencyDto { Id = 1, Name = "Stub Agency" };
        }

        public ProductDto GetProduct(int productId)
        {
            return new ProductDto { AdvertiserId = 1, Id = 1, Name = "Product" };
        }

        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            return new List<ProductDto>
            {
                new ProductDto { AdvertiserId = 1, Id = 1, Name = "Product" }
            };
        }

        public void ClearAgenciesCache()
        {
            // simulate success by doing nothing...
        }

        public void ClearAdvertisersCache()
        {
            // simulate success by doing nothing...
        }

        public void ClearProductsCache()
        {
            // simulate success by doing nothing...
        }
    }
}
