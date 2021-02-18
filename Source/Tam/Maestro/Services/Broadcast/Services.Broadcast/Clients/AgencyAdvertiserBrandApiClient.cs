using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Aab;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.Clients
{
    /// <summary>
    /// A client for retrieving the data.
    /// </summary>
    public interface IAgencyAdvertiserBrandApiClient
    {
        /// <summary>
        /// Gets the agencies.
        /// </summary>
        /// <returns></returns>
        List<AgencyDto> GetAgencies();

        /// <summary>
        /// Gets the advertisers.
        /// </summary>
        /// <returns></returns>
        List<AdvertiserDto> GetAdvertisers();

        /// <summary>
        /// Gets the advertiser products.
        /// </summary>
        /// <param name="masterId">The master identifier.</param>
        List<ProductDto> GetAdvertiserProducts(Guid masterId);
    }

    /// <summary>
    /// A client for retrieving the data.
    /// </summary>
    public class AgencyAdvertiserBrandApiClient : IAgencyAdvertiserBrandApiClient
    {
        private readonly string _AABApiUrl;
        private readonly HttpClient _HttpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyAdvertiserBrandApiClient"/> class.
        /// </summary>
        public AgencyAdvertiserBrandApiClient()
        {
            _AABApiUrl = $"{BroadcastServiceSystemParameter.AgencyAdvertiserBrandApiUrl}";
            _HttpClient = new HttpClient();
        }

        /// <inheritdoc/>
        public List<AgencyDto> GetAgencies()
        {
            var url = $"{_AABApiUrl}/agencies";

            try
            {
                var apiResult = _HttpClient.Get<List<aab_agency>>(url);
                var result = apiResult.Select(i => new AgencyDto
                {
                    Id = i.id,
                    MasterId = i.company_id,
                    Name = i.name
                }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch agencies data.", ex);
            }
        }

        /// <inheritdoc/>
        public List<AdvertiserDto> GetAdvertisers()
        {
            var url = $"{_AABApiUrl}/advertisers";

            try
            {
                var apiResult = _HttpClient.Get<List<aab_advertiser>>(url);
                var result = apiResult.Select(i => new AdvertiserDto()
                {
                    Id = i.id,
                    MasterId = i.company_id,
                    Name = i.name
                }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch advertisers data.", ex);
            }
        }

        /// <inheritdoc />
        public List<ProductDto> GetAdvertiserProducts(Guid advertiserMasterId)
        {
            var url = $"{_AABApiUrl}/advertisers/getadvertiserbyid/{advertiserMasterId}";

            try
            {
                var advertiserFullInfo = _HttpClient.Get<aab_advertiser>(url);
                var products = advertiserFullInfo.products.Select(i => new ProductDto
                {
                    Id = i.id,
                    MasterId = i.product_id,
                    Name = i.name,
                    AdvertiserId = advertiserFullInfo.id,
                    AdvertiserMasterId = advertiserFullInfo.company_id
                }).ToList();

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot fetch products data for advertiser {advertiserMasterId}.", ex);
            }
        }
    }
}