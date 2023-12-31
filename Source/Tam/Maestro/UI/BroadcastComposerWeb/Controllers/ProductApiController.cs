﻿using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Products")]
    public class ProductApiController : BroadcastControllerBase
    {
        public ProductApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(new ControllerNameRetriever(typeof(ProductApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets the advertiser products.
        /// </summary>
        /// <param name="advertiserMasterId">The advertiser master identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("AdvertiserProducts")]
        public BaseResponse<List<ProductDto>> GetAdvertiserProducts(Guid advertiserMasterId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProductService>().GetAdvertiserProducts(advertiserMasterId));
        }
    }
}
