using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.DTO;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Products")]
    public class ProductApiController : BroadcastControllerBase
    {
        public ProductApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(ProductApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Returns products for a specific advertiser
        /// </summary>
        /// <param name="advertiserId">Advertiser id</param>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<ProductDto>> GetProductsByAdvertiserId(int advertiserId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProductService>().GetProductsByAdvertiserId(advertiserId));
        }
    }
}
