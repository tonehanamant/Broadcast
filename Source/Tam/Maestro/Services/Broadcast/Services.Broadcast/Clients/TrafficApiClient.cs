using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface ITrafficApiClient
    {
        List<AgencyDto> GetAgencies();
        AgencyDto GetAgency(int agencyId);
        AdvertiserDto GetAdvertiser(int advertiserId);
        List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId);
        List<ProductDto> GetProductsByAdvertiserId(int advertiserId);
        ProductDto GetProduct(int productId);
    }

    public class TrafficApiClient : ITrafficApiClient
    {
        private readonly string _BaseTrafficCompanyURL;
        private readonly HttpClient _HttpClient;

        public TrafficApiClient()
        {
            _BaseTrafficCompanyURL = $"{TrafficComposerWebSystemParameter.BaseTrafficURL}/api/company";
            _HttpClient = new HttpClient();
        }

        public List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId)
        {
            var url = $"{_BaseTrafficCompanyURL}/agency/{agencyId}/advertisers";

            try
            {
                var advertisers = _Get<List<AdvertiserDto>>(url);

                return advertisers;
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot fetch advertisers data", ex);
            }
        }

        public List<AgencyDto> GetAgencies()
        {
            var url = $"{_BaseTrafficCompanyURL}/agency";

            try
            {
                var agencies = _Get<List<AgencyDto>>(url);

                return agencies;
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot fetch agencies data", ex);
            }
        }

        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            var url = $"{_BaseTrafficCompanyURL}/advertiser/{advertiserId}/products";

            try
            {
                var products = _Get<List<ProductDto>>(url);

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot fetch products data", ex);
            }
        }

        public ProductDto GetProduct(int productId)
        {
            var url = $"{_BaseTrafficCompanyURL}/product/{productId}";

            try
            {
                var product = _Get<ProductDto>(url);

                return product;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch data of the product {productId}", ex);
            }
        }

        public AgencyDto GetAgency(int agencyId)
        {
            var url = $"{_BaseTrafficCompanyURL}/agency/{agencyId}";

            try
            {
                var agency = _Get<AgencyDto>(url);

                return agency;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch data of the agency {agencyId}", ex);
            }
        }

        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            var url = $"{_BaseTrafficCompanyURL}/advertiser/{advertiserId}";

            try
            {
                var advertiser = _Get<AdvertiserDto>(url);

                return advertiser;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch data of the advertiser {advertiserId}", ex);
            }
        }

        private T _Get<T>(string url)
        {
            var response = _HttpClient.GetAsync(url).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);

            return response.Content.ReadAsAsync<T>().GetAwaiter().GetResult();
        }
    }
}
