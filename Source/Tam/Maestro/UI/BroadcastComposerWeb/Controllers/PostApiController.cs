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
        
        [HttpPost]
        [Route("ClientScrubbingProposal/{proposalId}")]
        public BaseResponse<ClientPostScrubbingProposalDto>  GetClientScrubbingForProposal(int proposalId, ProposalScrubbingRequest proposalScrubbingRequest)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>().GetClientScrubbingForProposal(proposalId, proposalScrubbingRequest));
        }

        [HttpGet]
        [Route("DownloadNSIPostReport/{proposalId}")]
        public HttpResponseMessage DownloadNSIPostReport(int proposalId)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<IPostReportService>().GenerateNSIPostReport(proposalId);
            var result = Request.CreateResponse(HttpStatusCode.OK);

            result.Content = new StreamContent(report.Stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = report.Filename
            };

            return result;
        }

        [HttpGet]
        [Route("DownloadMyEventsReport/{proposalId}")]
        public HttpResponseMessage DownloadMyEventsReport(int proposalId)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<IPostReportService>().GenerateMyEventsReport(proposalId);
            var result = Request.CreateResponse(HttpStatusCode.OK);

            result.Content = new ByteArrayContent(report.Stream.ToArray());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = report.Filename
            };

            return result;
        }

        [HttpGet]
        [Route("UnlinkedIscis")]
        public BaseResponse<List<UnlinkedIscisDto>> GetUnlinkedIscis()
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IAffidavitScrubbingService>().GetUnlinkedIscis());
        }

    }
}
