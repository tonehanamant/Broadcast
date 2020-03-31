using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Affiliates")]
    public class AffiliateApiController : BroadcastControllerBase
    {
        public AffiliateApiController(BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(AffiliateApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Endpoint for listing the affiliates in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<LookupDto>> GetAffiliates()
        {
            return _ConvertToBaseResponse(() =>
                 _ApplicationServiceFactory.GetApplicationService<IAffiliateService>().GetAllAffiliates()
            );
        }
    }
}