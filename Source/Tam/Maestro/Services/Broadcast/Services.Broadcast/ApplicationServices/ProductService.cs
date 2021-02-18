﻿using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProductService : IApplicationService
    {
        /// <summary>
        /// Returns products for a specific advertiser
        /// </summary>
        List<ProductDto> GetProductsByAdvertiserId(int advertiserId);

        /// <summary>
        /// Gets the advertiser products.
        /// </summary>
        /// <param name="advertiserMasterId">The advertiser master identifier.</param>
        /// <returns></returns>
        List<ProductDto> GetAdvertiserProducts(Guid advertiserMasterId);
    }

    public class ProductService : IProductService
    {
        private readonly IAabEngine _AabEngine;

        public ProductService(IAabEngine aabEngine)
        {
            _AabEngine = aabEngine;
        }

        /// <inheritdoc />
        public List<ProductDto> GetProductsByAdvertiserId(int advertiserId)
        {
            return _AabEngine.GetAdvertiserProducts(advertiserId);
        }

        /// <inheritdoc />
        public List<ProductDto> GetAdvertiserProducts(Guid advertiserMasterId)
        {
            return _AabEngine.GetAdvertiserProducts(advertiserMasterId);
        }
    }
}
