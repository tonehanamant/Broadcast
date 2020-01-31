using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.ProgramGuide;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/ProgramGuideTest")]
    public class ProgramGuideController : BroadcastControllerBase
    {
        public ProgramGuideController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(ProgramGuideController).Name), applicationServiceFactory)
        {
        }

        [HttpPost]
        public BaseResponse<List<GuideResponseElementDto>> GetProgramsForGuide(List<GuideRequestElementDto> guideRequestElements)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProgramGuideService>().GetProgramsForGuide(guideRequestElements));
        }

        [HttpPost]
        [Route("Test")]
        public BaseResponse<List<GuideResponseElementDto>> PerformGetProgramsForGuideTest()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProgramGuideService>().PerformGetProgramsForGuideTest());
        }
    }
}
