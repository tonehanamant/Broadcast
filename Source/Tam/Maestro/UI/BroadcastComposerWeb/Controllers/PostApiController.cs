using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System;
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
        public PostApiController(BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(PostApiController).Name), applicationServiceFactory)
        {
        }

        [Route("")]
        public BaseResponse<PostedContractedProposalsDto> GetPostList()
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().GetPosts());
        }

        [HttpPost]
        [Route("ClientScrubbingProposal/{proposalId}")]
        public BaseResponse<ClientPostScrubbingProposalDto> GetClientScrubbingForProposal(int proposalId, ProposalScrubbingRequest proposalScrubbingRequest)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().GetClientScrubbingForProposal(proposalId, proposalScrubbingRequest));
        }

        [HttpGet]
        [Route("DownloadNSIPostReport/{proposalId}")]
        public HttpResponseMessage DownloadNSIPostReport(int proposalId)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<IPostReportService>().GeneratePostReport(proposalId);
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
        [Route("DownloadNSIPostReportWithOvernight/{proposalId}")]
        public HttpResponseMessage DownloadNSIPostReportWithOvernight(int proposalId)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<IPostReportService>().GeneratePostReport(proposalId, withOvernightImpressions: true);
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
                _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().GetUnlinkedIscis());
        }

        [HttpPost]
        [Route("ScrubUnlinkedIsci")]
        [Authorize]
        public BaseResponse<bool> ScrubUnlinkedIsci(ScrubIsciRequest request)
        {
            var fullName = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(() =>
                    _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().ScrubUnlinkedAffidavitDetailsByIsci(request.Isci, DateTime.Now, fullName));
        }

        [HttpGet]
        [Route("ArchivedIscis")]
        public BaseResponse<List<ArchivedIscisDto>> GetArchivedIscis()
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().GetArchivedIscis());
        }

        [HttpPost]
        [Route("ArchiveUnlinkedIsci")]
        [Authorize]
        public BaseResponse<bool> ArchiveUnlinkedIsci(List<string> iscis)
        {
            var fullName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() =>
            _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().ArchiveUnlinkedIsci(iscis, fullName));
        }


        [HttpPost]
        [Route("UndoArchiveIsci")]
        [Authorize]
        public BaseResponse<bool> UndoArchiveUnlinkedIsci(List<long> FileDetailsIds)
        {
            var fullName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() =>
            _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().UndoArchiveUnlinkedIsci(FileDetailsIds, DateTime.Now, fullName));
        }

        [HttpGet]
        [Route("FindValidIscis/{isci}")]
        public BaseResponse<List<string>> FindValidIscis(string isci)
        {
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IIsciService>().FindValidIscis(isci));
        }

        [HttpPost]
        [Route("MapIsci")]
        [Authorize]
        public BaseResponse<bool> MapIsci(MapIsciDto mapIsciDto)
        {
            var fullName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() =>
            _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().MapIsci(mapIsciDto, DateTime.Now, fullName));
        }

        [HttpPut]
        [Route("OverrideStatus")]
        public BaseResponse<ClientPostScrubbingProposalDto> OverrideScrubStatus(ScrubStatusOverrideRequest request)
        {
            return
            _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().OverrideScrubbingStatus(request));
        }

        [HttpPut]
        [Route("UndoOverrideStatus")]
        public BaseResponse<ClientPostScrubbingProposalDto> UndoOverrideScrubStatus(ScrubStatusOverrideRequest request)
        {
            return
            _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().UndoOverrideScrubbingStatus(request));
        }

        [HttpPost]
        [Route("SwapProposalDetail")]
        [Authorize]
        public BaseResponse<bool> SwapProposalDetail(SwapProposalDetailRequest requestData)
        {
            var fullName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().SwapProposalDetails(requestData, DateTime.Now, fullName));
        }

        [HttpPost]
        [Route("UploadNtiTransmittals")]
        [Authorize]
        public BaseResponse UploadNtiTransmittals(FileRequest request)
        {
            var fullName = _GetCurrentUserFullName();
            return _ApplicationServiceFactory.GetApplicationService<INtiTransmittalsService>().UploadNtiTransmittalsFile(request, fullName);
        }

        [HttpGet]
        [Route("DownloadNTIReport/{proposalId}")]
        public HttpResponseMessage DownloadNTIReport(int proposalId)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<IPostReportService>().GeneratePostReport(proposalId, isNtiReport: true);
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
