using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Pricing")]
    public class PricingApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public PricingApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(PricingApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        /// <summary>
        /// Gets the unit caps.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("UnitCaps")]
        public BaseResponse<List<LookupDto>> GetUnitCaps()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPricingService>().GetUnitCaps());
        }
    }
}
