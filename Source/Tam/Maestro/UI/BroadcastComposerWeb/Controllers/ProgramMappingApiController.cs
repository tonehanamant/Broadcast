using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping.Entities;
using Services.Broadcast.Entities.DTO.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{ 

    [RoutePrefix("api/v1/ProgramMapping")]
    public class ProgramMappingApiController : BroadcastControllerBase
    {
        public ProgramMappingApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(new ControllerNameRetriever(typeof(ProgramMappingApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets the programs.
        /// </summary>
        /// <returns>A list of program objects.</returns>
        [HttpGet]
        [Route("UnmappedPrograms")]
        public BaseResponse<List<UnmappedProgram>> GetUnmappedPrograms()
        {
            var fullName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProgramMappingService>().GetUnmappedPrograms());
        }
    }
}