using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.Stubbs
{
    public class TrafficApiClientStub : ITrafficApiClient
    {
        public List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId)
        {
            return new List<AdvertiserDto>
            {
                new AdvertiserDto { Id = 1, Name = "Name1", AgencyId = 1 }
            };
        }

        public List<AgencyDto> GetAgencies()
        {
            return new List<AgencyDto>
            {
                new AgencyDto { Id = 1, Name = "Name1" }
            };
        }

        public ProductDto GetProduct(int productId)
        {
            // imitates the client behavior when not existing product is passed
            if (productId == 666)
            {
                throw new Exception($"Cannot fetch data of the product {productId}");
            }

            return new ProductDto
            {
                Id = productId,
                Name = "Product" + productId,
                AdvertiserId = 1
            };
        }

        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            throw new System.NotImplementedException();
        }
    }
}
