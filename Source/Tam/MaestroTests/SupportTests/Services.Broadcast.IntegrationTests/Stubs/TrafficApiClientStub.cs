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

            return new AdvertiserDto
            {
                Id = advertiserId, MasterId = new Guid("3476883D-0F76-483B-BAB0-17A55E662B29"), 
                Name = "Name" + advertiserId, 
                AgencyId = 1,
                AgencyMasterId = new Guid("2813CF6F-7530-42E0-9F06-3DEE97F7A670")
            };
        }


        public int GetAdvertisersCalledCount { get; set; }
        public List<AdvertiserDto> GetAdvertisers()
        {
            GetAdvertisersCalledCount++;

            return new List<AdvertiserDto>
            {
                new AdvertiserDto { Id = 1, MasterId = new Guid("004003C9-7F8C-4494-B922-9FA8144CE5C6"), Name = "Name1", AgencyId = 1, AgencyMasterId = new Guid("A5408700-1699-494D-9D84-01D12D4FAA2C")},
                new AdvertiserDto { Id = 2, MasterId = new Guid("0147106A-657A-40B7-95D1-862DBFC1E40A"), Name = "Name1", AgencyId = 1, AgencyMasterId = new Guid("A5408700-1699-494D-9D84-01D12D4FAA2C")},
                new AdvertiserDto { Id = 3, MasterId = new Guid("B244A609-AF71-4A76-B865-52B5C7D43565"), Name = "Name1", AgencyId = 2, AgencyMasterId = new Guid("7DF35DCC-1DB5-4E41-9D0F-A9BD7AE5A4FC") },
                new AdvertiserDto { Id = 4, MasterId = new Guid("530F1EEA-8DDF-4AF3-9711-BACE39275F12"), Name = "Name1", AgencyId = 2, AgencyMasterId = new Guid("7DF35DCC-1DB5-4E41-9D0F-A9BD7AE5A4FC") },
                new AdvertiserDto { Id = 37444, MasterId = new Guid("A6714BDF-CC94-41B4-9C36-6CA87AA75B4B"), Name = "Leagas Delaney", AgencyId = 3, AgencyMasterId = new Guid("5B4E5010-710B-47A3-91FA-8007B7DA7320")}
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
                MasterId = new Guid("7E41B3F5-0375-4E1A-B06F-4C6069CDE16C"),
                Name = "Product" + productId,
                AdvertiserId = 1,
                AdvertiserMasterId = new Guid("134A6780-734E-4AC0-9F83-091F984F66E4")
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
                new ProductDto { Id = 1, MasterId = new Guid("4C61CA64-DB36-4C49-9B85-8FBAD1DB79BA"),Name = "Product1", AdvertiserId = advertiserId, AdvertiserMasterId = new Guid("1C299D09-FAE5-4433-94BF-7C294D168A48")},
                new ProductDto { Id = 2, MasterId = new Guid("34DCD457-A069-427A-A9B7-9B637E49758E"), Name = "Product2", AdvertiserId = advertiserId, AdvertiserMasterId = new Guid("1C299D09-FAE5-4433-94BF-7C294D168A48")},
                new ProductDto { Id = 3, MasterId = new Guid("B282E6DD-2D34-4BBF-903B-3EBD2501DC37"), Name = "Product3", AdvertiserId = advertiserId, AdvertiserMasterId = new Guid("1C299D09-FAE5-4433-94BF-7C294D168A48")},
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
                new AgencyDto { Id = ++_agenciesListId, MasterId = new Guid("A5408700-1699-494D-9D84-01D12D4FAA2C"), Name = $"{filter}_Name_{_agenciesListId}"},
                new AgencyDto { Id = ++_agenciesListId, MasterId = new Guid("A5408700-1699-494D-9D84-01D12D4FAA2C"), Name = $"{filter}_Name_{_agenciesListId}"},
                new AgencyDto { Id = ++_agenciesListId, MasterId = new Guid("A5408700-1699-494D-9D84-01D12D4FAA2C"), Name = $"{filter}_Name_{_agenciesListId}"}
            };

            return agencies;
        }

        public int GetAgenciesCalledCount { get; set; } = 0;
        public List<AgencyDto> GetAgencies()
        {
            GetAgenciesCalledCount++;
            return new List<AgencyDto>
            {
                new AgencyDto { Id = ++_agenciesListId, MasterId = new Guid("A5408700-1699-494D-9D84-01D12D4FAA2C"), Name = $"Name_{_agenciesListId}"},
                new AgencyDto { Id = ++_agenciesListId, MasterId = new Guid("A5408700-1699-494D-9D84-01D12D4FAA2C"), Name = $"Name_{_agenciesListId}"},
                new AgencyDto { Id = ++_agenciesListId, MasterId = new Guid("A5408700-1699-494D-9D84-01D12D4FAA2C"), Name = $"Name_{_agenciesListId}"}
            };
        }
    }
}
