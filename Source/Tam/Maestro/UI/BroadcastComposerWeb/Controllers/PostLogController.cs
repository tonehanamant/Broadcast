using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
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
        private readonly IWebLogger _Logger;
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public PostLogController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever("PostLogController"))
        {
            _Logger = logger;
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        [Route("")]
        public BaseResponse<PostedContractedProposalsDto> GetPostLogList()
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPostLogService>().GetPostLogs());
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
        public BaseResponse<bool> ScrubUnlinkedIsci(ScrubIsciRequest request)
        {
            return
                _ConvertToBaseResponse(() =>
                    _ApplicationServiceFactory.GetApplicationService<IPostLogService>().ScrubUnlinkedPostLogDetailsByIsci(request.Isci, DateTime.Now, Identity.Name));
        }

        [HttpPost]
        [Route("ArchiveUnlinkedIsci")]
        public BaseResponse<bool> ArchiveUnlinkedIsci(List<string> iscis)
        {
            return _ConvertToBaseResponse(() =>
            _ApplicationServiceFactory.GetApplicationService<IPostLogService>().ArchiveUnlinkedIsci(iscis, Identity.Name));
        }

        [HttpPost]
        [Route("UndoArchiveIsci")]
        public BaseResponse<bool> UndoArchiveUnlinkedIsci(List<long> FileDetailsIds)
        {
            return _ConvertToBaseResponse(() =>
            _ApplicationServiceFactory.GetApplicationService<IPostLogService>().UndoArchiveUnlinkedIsci(FileDetailsIds, DateTime.Now, Identity.Name));
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
        public BaseResponse<bool> MapIsci(MapIsciDto mapIsciDto)
        {
            return _ConvertToBaseResponse(() =>
            _ApplicationServiceFactory.GetApplicationService<IPostLogService>().MapIsci(mapIsciDto, DateTime.Now, Identity.Name));
        }

        [HttpPost]
        [Route("SwapProposalDetail")]
        public BaseResponse<bool> SwapProposalDetail(SwapProposalDetailRequest requestData)
        {
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IPostLogService>().SwapProposalDetails(requestData, DateTime.Now, Identity.Name));
        }
    }
}