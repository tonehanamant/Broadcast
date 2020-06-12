using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class TrafficApiClientStub : ITrafficApiClient
    {
        public int GetAdvertiserCallCount { get; set; }
        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            GetAdvertiserCallCount++;

            // imitates the client behavior when not existing advertiser id is passed
            if (advertiserId == 666)
            {
                throw new Exception($"Cannot fetch data of the advertiser {advertiserId}");
            }

            return new AdvertiserDto { Id = advertiserId, Name = "Name" + advertiserId, AgencyId = 1 };
        }


        public int GetAdvertisersCalledCount { get; set; }
        public List<AdvertiserDto> GetAdvertisers()
        {
            GetAdvertisersCalledCount++;

            return new List<AdvertiserDto>
            {
                new AdvertiserDto { Id = 1, Name = "Name1", AgencyId = 1 },
                new AdvertiserDto { Id = 2, Name = "Name1", AgencyId = 1 },
                new AdvertiserDto { Id = 3, Name = "Name1", AgencyId = 2 },
                new AdvertiserDto { Id = 4, Name = "Name1", AgencyId = 2 },
                new AdvertiserDto { Id = 37444, Name = "Leagas Delaney", AgencyId = 3 }
            };
        }

        public int GetProductCallCount { get; set; }
        public ProductDto GetProduct(int productId)
        {
            GetProductCallCount++;
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

        public int GetProductsByAdvertiserIdCallCount { get; set; }
        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            GetProductsByAdvertiserIdCallCount++;
            if (advertiserId == 666)
            {
                throw new Exception($"Cannot fetch products data for advertiser {advertiserId}.");
            }

            return new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Product1", AdvertiserId = advertiserId},
                new ProductDto { Id = 2, Name = "Product2", AdvertiserId = advertiserId},
                new ProductDto { Id = 3, Name = "Product3", AdvertiserId = advertiserId},
            };
        }

        public int GetFilteredAgenciesCalledCount { get; set; }
        private int _agenciesListId = 0;
        public List<AgencyDto> GetFilteredAgencies(string filter)
        {
            GetFilteredAgenciesCalledCount++;

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

        public List<AgencyDto> GetAgencies()
        {
            return new List<AgencyDto>
            {
                new AgencyDto { Id = ++_agenciesListId, Name = $"Name_{_agenciesListId}"},
                new AgencyDto { Id = ++_agenciesListId, Name = $"Name_{_agenciesListId}"},
                new AgencyDto { Id = ++_agenciesListId, Name = $"Name_{_agenciesListId}"}
            };
        }
    }
}
