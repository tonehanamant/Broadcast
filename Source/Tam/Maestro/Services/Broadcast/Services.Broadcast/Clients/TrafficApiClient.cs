using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    public interface ITrafficApiClient
    {
        List<AgencyDto> GetFilteredAgencies(string filter);
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

        public List<AgencyDto> GetFilteredAgencies(string filter)
        {
            var url = $"{_BaseTrafficCompanyURL}/agency?filter={Uri.EscapeDataString(filter)}";

            try
            {
                return _HttpClient.Get<List<AgencyDto>>(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch agencies data with filter '{filter}'.", ex);
            }
        }

        public List<AdvertiserDto> GetAdvertisersByAgencyId(int agencyId)
        {
            var url = $"{_BaseTrafficCompanyURL}/agency/{agencyId}/advertisers";

            try
            {
                var advertisers = _HttpClient.Get<List<AdvertiserDto>>(url);

                return advertisers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch advertisers data for agency {agencyId}.", ex);
            }
        }

        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            var url = $"{_BaseTrafficCompanyURL}/advertiser/{advertiserId}/products";

            try
            {
                var products = _HttpClient.Get<List<ProductDto>>(url);

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch products data for advertiser {advertiserId}.", ex);
            }
        }

        public ProductDto GetProduct(int productId)
        {
            var url = $"{_BaseTrafficCompanyURL}/product/{productId}";

            try
            {
                var product = _HttpClient.Get<ProductDto>(url);

                return product;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch data of the product {productId}", ex);
            }
        }

        public AdvertiserDto GetAdvertiser(int advertiserId)
        {
            var url = $"{_BaseTrafficCompanyURL}/advertiser/{advertiserId}";

            try
            {
                var advertiser = _HttpClient.Get<AdvertiserDto>(url);

                return advertiser;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch data of the advertiser {advertiserId}", ex);
            }
        }
    }
}
