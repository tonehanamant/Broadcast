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
        public BaseResponse<List<PostDto>> GetPostList()
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>().GetPosts());
        }

        [HttpGet]
        [Route("ClientScrubbingProposal/{proposalId}")]
        public BaseResponse<ClientPostScrubbingProposalHeaderDto> GetClientPostScrubbingProposalHeader(int proposalId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>().GetClientPostScrubbingProposalHeader(proposalId));
        }

        [HttpGet]
        [Route("ClientScrubbingProposal/{proposalId}/Detail/{detailId}")]
        public BaseResponse<ClientPostScrubbingProposalDetailDto> GetClientPostScrubbingProposalDetail(int proposalId, int detailId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>().GetClientPostScrubbingProposalDetail(proposalId, detailId));
        }
    }
}
