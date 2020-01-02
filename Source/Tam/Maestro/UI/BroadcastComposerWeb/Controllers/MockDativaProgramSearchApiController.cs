using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.DTO.Program;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/MockDativaProgramSearch")]
    public class MockDativaProgramSearchApiController : BroadcastControllerBase
    {
        public MockDativaProgramSearchApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(ProgramGuideController).Name), applicationServiceFactory)
        {
        }

        // TODO after switching to the real Dativa API: remove this
        /// <summary>
        /// This is a simulation of the Dativa api
        /// </summary>
        [HttpPost]
        [Route("")]
        public List<SearchProgramDativaResponseDto> GetPrograms(SearchProgramDativaRequestDto searchRequest)
        {
            return _ApplicationServiceFactory.GetApplicationService<IProgramService>().GetProgramsExternal(searchRequest);
        }
    }
}
