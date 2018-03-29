using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/Post")]
    public class PostApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public PostApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever("PostApiController"))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        [Route("")]
        public BaseResponse<PostedContractedProposalsDto> GetPostList()
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>().GetPosts());
        }
        
        [HttpGet]
        [Route("ClientScrubbingProposal/{proposalId}")]
        public BaseResponse<ClientPostScrubbingProposalDto>  GetClientScrubbingForProposal(int proposalId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>().GetClientScrubbingForProposal(proposalId));
        }
    }
}
