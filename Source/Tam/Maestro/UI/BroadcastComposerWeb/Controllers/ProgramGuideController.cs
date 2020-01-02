using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.ProgramGuide;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    // TODO: Remove this during PRI-17014.  Reroute consumers to ProgramGuideApiClient.
    [RoutePrefix("api/v1/ProgramGuideTest")]
    public class ProgramGuideController : BroadcastControllerBase
    {
        public ProgramGuideController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(ProgramGuideController).Name), applicationServiceFactory)
        {
        }

        [HttpPost]
        public BaseResponse<List<GuideResponseElementDto>> GetProgramsForGuide(List<GuideRequestElementDto> guideRequestElements, bool simulate = true)
        {
            var fullName = "testUser";
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProgramGuideService>().GetProgramsForGuide(guideRequestElements, fullName, simulate));
        }
    }
}
