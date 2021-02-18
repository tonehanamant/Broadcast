using System;
using System.Collections.Generic;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class AgencyAdvertiserBrandApiClientStub : IAgencyAdvertiserBrandApiClient
    {
        public List<AgencyDto> GetAgencies()
        {
            var agencies = new List<AgencyDto>
            {
                new AgencyDto{ Id = 1, MasterId = new Guid("A8B10A69-FBD6-43FE-B143-156B7297B62D"), Name = "Agency1"},
                new AgencyDto{ Id = 2, MasterId = new Guid("D93EE3E1-EE03-4415-A0A9-BBDAD120B35A"), Name = "Agency2"},
                new AgencyDto{ Id = 3, MasterId = new Guid("39F32E95-700C-48CA-B079-B7398F18BAC2"), Name = "Agency3"},
                new AgencyDto{ Id = 4, MasterId = new Guid("CF40997F-F314-4FE7-BE6B-EBF027C14247"), Name = "Agency4"},
            };
            return agencies;
        }

        public List<AdvertiserDto> GetAdvertisers()
        {
            var agencies = GetAgencies();
            var advertisers = new List<AdvertiserDto>
            {
                new AdvertiserDto{ Id = 1, MasterId = new Guid("4CDA85D1-2F40-4B27-A4AD-72A012907E3C"), Name ="Advertiser1", AgencyId = agencies[0].Id, AgencyMasterId = agencies[0].MasterId},
                new AdvertiserDto{ Id = 2, MasterId = new Guid("CFFFE6C6-0A33-44C5-8E12-FC1C0563591B"), Name ="Advertiser2", AgencyId = agencies[0].Id, AgencyMasterId = agencies[0].MasterId},
                new AdvertiserDto{ Id = 3, MasterId = new Guid("E58028B0-03E8-4411-AE19-C0A51C75DFF5"), Name ="Advertiser3", AgencyId = agencies[1].Id, AgencyMasterId = agencies[1].MasterId},
                new AdvertiserDto{ Id = 4, MasterId = new Guid("221116A6-573E-4B10-BB4D-0A2F2913FA6F"), Name ="Advertiser4", AgencyId = agencies[1].Id, AgencyMasterId = agencies[1].MasterId},
            };
            return advertisers;
        }

        public List<ProductDto> GetAdvertiserProducts(Guid masterId)
        {
            var products = new List<ProductDto>
            {
                new ProductDto {Id = 1, MasterId = new Guid("C2771F6B-8579-486A-910C-FF3C84144DE7"), Name = "Product1", AdvertiserId = 2, AdvertiserMasterId = masterId},
                new ProductDto {Id = 2, MasterId = new Guid("BEA60F15-A681-42CE-B344-EC57AF0B4398"), Name = "Product2", AdvertiserId = 2, AdvertiserMasterId = masterId},
                new ProductDto {Id = 3, MasterId = new Guid("EFED91C9-1A6D-4ABE-86CD-1EC847105435"), Name = "Product3", AdvertiserId = 2, AdvertiserMasterId = masterId},
                new ProductDto {Id = 4, MasterId = new Guid("6BEF080E-01ED-4D42-BE54-927110457907"), Name = "Product4", AdvertiserId = 2, AdvertiserMasterId = masterId},
            };
            return products;
        }
    }
}