using Services.Broadcast.Cache;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class TrafficApiCacheStub : ITrafficApiCache
    {
        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            return new AdvertiserDto { Id = advertiserId, AgencyId = 1, Name = "Stub Advertiser" };
        }

        public List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId)
        {
            return new List<AdvertiserDto> { new AdvertiserDto { Id = 1, AgencyId = agencyId, Name = "Stub Advertiser" } };
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
            throw new NotImplementedException();
        }

        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            throw new NotImplementedException();
        }
    }
}
