using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        [HttpGet]
        [Route("DownloadNSIPostReport/{proposalId}")]
        public HttpResponseMessage DownloadNSIPostReport(int proposalId)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>().GenerateNSIPostReport(proposalId);
            var result = Request.CreateResponse(HttpStatusCode.OK);

            result.Content = new StreamContent(report.Stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = report.Filename
            };

            return result;
        }
    }
}
