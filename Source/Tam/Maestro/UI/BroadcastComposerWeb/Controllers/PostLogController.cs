using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;


namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/PostLog")]
    public class PostLogController : BroadcastControllerBase
    {
        public PostLogController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(PostLogController).Name), applicationServiceFactory)
        {
        }

        [Route("")]
        public BaseResponse<PostedContractedProposalsDto> GetPostLogList()
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPostLogService>().GetPostLogs(DateTime.Now));
        }

        [HttpPost]
        [Route("ClientScrubbingProposal/{proposalId}")]
        public BaseResponse<ClientPostScrubbingProposalDto> GetClientScrubbingForProposal(int proposalId, ProposalScrubbingRequest proposalScrubbingRequest)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPostLogService>().GetClientScrubbingForProposal(proposalId, proposalScrubbingRequest));
        }

        [HttpPut]
        [Route("OverrideStatus")]
        public BaseResponse<ClientPostScrubbingProposalDto> OverrideScrubStatus(ScrubStatusOverrideRequest request)
        {
            return
            _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IPostLogService>().OverrideScrubbingStatus(request));
        }

        [HttpPut]
        [Route("UndoOverrideStatus")]
        public BaseResponse<ClientPostScrubbingProposalDto> UndoOverrideScrubStatus(ScrubStatusOverrideRequest request)
        {
            return
            _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IPostLogService>().UndoOverrideScrubbingStatus(request));
        }

        [HttpGet]
        [Route("UnlinkedIscis")]
        public BaseResponse<List<UnlinkedIscisDto>> GetUnlinkedIscis()
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPostLogService>().GetUnlinkedIscis());
        }

        [HttpGet]
        [Route("ArchivedIscis")]
        public BaseResponse<List<ArchivedIscisDto>> GetArchivedIscis()
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPostLogService>().GetArchivedIscis());
        }

        [HttpPost]
        [Route("ScrubUnlinkedIsci")]
        [Authorize]
        public BaseResponse<bool> ScrubUnlinkedIsci(ScrubIsciRequest request)
        {
            var fullName = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(() =>
                    _ApplicationServiceFactory.GetApplicationService<IPostLogService>().ScrubUnlinkedPostLogDetailsByIsci(request.Isci, DateTime.Now, fullName));
        }

        [HttpPost]
        [Route("ArchiveUnlinkedIsci")]
        [Authorize]
        public BaseResponse<bool> ArchiveUnlinkedIsci(List<string> iscis)
        {
            var fullName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() =>
            _ApplicationServiceFactory.GetApplicationService<IPostLogService>().ArchiveUnlinkedIsci(iscis, fullName));
        }

        [HttpPost]
        [Route("UndoArchiveIsci")]
        [Authorize]
        public BaseResponse<bool> UndoArchiveUnlinkedIsci(List<long> FileDetailsIds)
        {
            var fullName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() =>
            _ApplicationServiceFactory.GetApplicationService<IPostLogService>().UndoArchiveUnlinkedIsci(FileDetailsIds, DateTime.Now, fullName));
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
            _ApplicationServiceFactory.GetApplicationService<IPostLogService>().MapIsci(mapIsciDto, DateTime.Now, fullName));
        }

        [HttpPost]
        [Route("SwapProposalDetail")]
        [Authorize]
        public BaseResponse<bool> SwapProposalDetail(SwapProposalDetailRequest requestData)
        {
            var fullName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IPostLogService>().SwapProposalDetails(requestData, DateTime.Now, fullName));
        }
    }
}