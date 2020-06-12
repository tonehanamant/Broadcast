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

        /// <summary>
        /// Gets the advertisers.
        /// </summary>
        /// <returns>List of AdvertiserDto objects</returns>
        List<AdvertiserDto> GetAdvertisers();

        List<ProductDto> GetProductsByAdvertiserId(int advertiserId);
        ProductDto GetProduct(int productId);

        /// <summary>
        /// Gets the agencies.
        /// </summary>
        /// <returns>List of AgencyDto objects</returns>
        List<AgencyDto> GetAgencies();
    }

    public class TrafficApiClient : ITrafficApiClient
    {
        private readonly string _AABApiUrl;
        private readonly HttpClient _HttpClient;

        public TrafficApiClient()
        {
            _AABApiUrl = $"{BroadcastServiceSystemParameter.AgencyAdvertiserBrandApiUrl}";
            _HttpClient = new HttpClient();
        }

        public List<AgencyDto> GetFilteredAgencies(string filter)
        {
            var url = $"{_AABApiUrl}/agency?filter={Uri.EscapeDataString(filter)}";

            try
            {
                return _HttpClient.Get<List<AgencyDto>>(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch agencies data with filter '{filter}'.", ex);
            }
        }

        /// <inheritdoc/>
        public List<AgencyDto> GetAgencies()
        {
            var url = $"{_AABApiUrl}/agency/nolimit";

            try
            {
                return _HttpClient.Get<List<AgencyDto>>(url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch agencies data.", ex);
            }
        }

        /// <inheritdoc/>
        public List<AdvertiserDto> GetAdvertisers()
        {
            var url = $"{_AABApiUrl}/advertiser/nolimit";

            try
            {
                var advertisers = _HttpClient.Get<List<AdvertiserDto>>(url);

                return advertisers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch advertisers data.", ex);
            }
        }

        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            var url = $"{_AABApiUrl}/advertiser/{advertiserId}/products";

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
            var url = $"{_AABApiUrl}/product/{productId}";

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
            var url = $"{_AABApiUrl}/advertiser/{advertiserId}";

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
