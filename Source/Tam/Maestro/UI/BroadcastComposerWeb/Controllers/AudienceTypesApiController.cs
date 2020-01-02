using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System.Web.Http;
using Tam.Maestro.Web.Common;
using Tam.Maestro.Services.Cable.Entities;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/AudienceTypes")]
    public class AudienceTypesApiController : BroadcastControllerBase
    {
        public AudienceTypesApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(AudienceTypesApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets the audience types.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<LookupDto>> GetAudienceTypes()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IAudienceService>().GetAudienceTypes());
        }        
    }
}
