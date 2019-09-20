using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.Stubbs
{
    public class TrafficApiClientStub : ITrafficApiClient
    {
        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            // imitates the client behavior when not existing advertiser id is passed
            if (advertiserId == 666)
            {
                throw new Exception($"Cannot fetch data of the advertiser {advertiserId}");
            }

            return new AdvertiserDto { Id = advertiserId, Name = "Name" + advertiserId, AgencyId = 1 };
        }

        public List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId)
        {
            return new List<AdvertiserDto>
            {
                new AdvertiserDto { Id = 1, Name = "Name1", AgencyId = agencyId },
                new AdvertiserDto { Id = 2, Name = "Name1", AgencyId = agencyId },
                new AdvertiserDto { Id = 3, Name = "Name1", AgencyId = agencyId },
                new AdvertiserDto { Id = 4, Name = "Name1", AgencyId = agencyId },
                new AdvertiserDto { Id = 37444, Name = "Leagas Delaney", AgencyId = agencyId }
            };
        }

        public List<AgencyDto> GetAgencies()
        {
            return new List<AgencyDto>
            {
                new AgencyDto { Id = 1, Name = "Name1" }
            };
        }

        public AgencyDto GetAgency(int agencyId)
        {
            return new AgencyDto { Id = agencyId, Name = "Name" + agencyId };
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

        private int _agenciesListId = 0;

        public List<AgencyDto> GetFilteredAgencies(string filter)
        {
            // imitates the client behavior when not existing advertiser id is passed
            if (filter == "666")
            {
                throw new Exception($"Cannot fetch agencies data with filter '{filter}'.");
            }

            var agencies = new List<AgencyDto>
            {
                new AgencyDto { Id = ++_agenciesListId, Name = $"{filter}_Name_{_agenciesListId}"},
                new AgencyDto { Id = ++_agenciesListId, Name = $"{filter}_Name_{_agenciesListId}"},
                new AgencyDto { Id = ++_agenciesListId, Name = $"{filter}_Name_{_agenciesListId}"}
            };

            return agencies;
        }
    }
}
