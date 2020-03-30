using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities.DTO.Program;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Programs")]
    public class ProgramApiController : BroadcastControllerBase
    {
        public ProgramApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(new ControllerNameRetriever(typeof(ProgramApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets the programs.
        /// </summary>
        /// <returns>A list of program objects.</returns>
        [HttpPost]
        [Route("")]
        public BaseResponse<List<ProgramDto>> GetPrograms(SearchRequestProgramDto searchRequest)
        {
            var fullName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProgramService>().GetPrograms(searchRequest, fullName));
        }
    }
}
