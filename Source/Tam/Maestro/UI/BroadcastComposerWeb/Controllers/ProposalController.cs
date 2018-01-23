using Common.Services.ApplicationServices;
using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/Proposals")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class ProposalController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public ProposalController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(ProposalController).Name))
        {
            _Logger = logger;
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        [HttpGet]
        [Route("GetProposals")]
        public BaseResponse<List<DisplayProposal>> GetAllProposals()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IProposalService>().GetAllProposals());
        }

        [HttpGet]
        [Route("InitialData")]
        public BaseResponse<ProposalLoadDto> GetInitialProposalData()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IProposalService>().GetInitialProposalData(DateTime.Now));
        } 

        [HttpPost]
        [Route("SaveProposal")]
        public BaseResponse<ProposalDto> SaveProposal(ProposalDto proposal)
        {            
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                            .SaveProposal(proposal, Identity.Name, DateTime.Now));
        }

        [HttpDelete]
        [Route("DeleteProposal/{proposalId}")]
        public BaseResponse<ValidationWarningDto> DeleteProposal(int proposalId)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                            .DeleteProposal(proposalId));
        }

        [HttpPost]
        [Route("UnorderProposal")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Buyer)]
        public BaseResponse<ProposalDto> UnorderProposal(int proposalId)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                            .UnorderProposal(proposalId, Identity.Name));
        }
        [HttpGet]
        [Route("generate_scx_archive/{proposalId}")]
        public HttpResponseMessage GenerateScxArchive(int proposalId)
        {
            var archive = _ApplicationServiceFactory.GetApplicationService<IProposalService>().GenerateScxFileArchive(proposalId);

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(archive.Item2);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = archive.Item1
            };

            return result;
        }

        [HttpPost]
        [Route("GetProposalDetail")]
        public BaseResponse<ProposalDetailDto> GetProposalDetail(ProposalDetailRequestDto proposalDetailRequestDto)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                            .GetProposalDetail(proposalDetailRequestDto));
        }

        [HttpPost]
        [Route("UpdateProposal")]
        public BaseResponse<ProposalDto> UpdateProposal(List<ProposalDetailDto> proposalDetailDtos)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                            .UpdateProposal(proposalDetailDtos));
        }

        [HttpGet]
        [Route("Proposal/{proposalId}")]
        public BaseResponse<ProposalDto> GetProposal(int proposalId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IProposalService>().GetProposalById(proposalId));
        }

        [HttpGet]
        [Route("Proposal/{proposalId}/Versions/{version}")]
        public BaseResponse<ProposalDto> GetProposal(int proposalId, int version)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IProposalService>().GetProposalByIdWithVersion(proposalId, version));
        }

        [HttpGet]
        [Route("Proposal/{proposalId}/Versions")]
        public BaseResponse<List<DisplayProposalVersion>> GetProposalVersions(int proposalId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IProposalService>().GetProposalVersionsByProposalId(proposalId));
        }

        [HttpGet]
        [Route("Proposal/{proposalId}/Lock")]
        public BaseResponse<LockResponse> LockProposal(int proposalId)
        {
            var key = string.Format("broadcast_proposal : {0}", proposalId);
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<ILockingManagerApplicationService>().LockObject(key));
        }

        [HttpGet]
        [Route("Proposal/{proposalId}/UnLock")]
        public BaseResponse<ReleaseLockResponse> UnlockProposal(int proposalId)
        {
            var key = string.Format("broadcast_proposal : {0}", proposalId);
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<ILockingManagerApplicationService>().ReleaseObject(key));
        }

        [HttpGet]
        [Route("FindGenres/{genreSearchString}")]
        public BaseResponse<List<LookupDto>> FindGenres(string genreSearchString)
        {

            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IProposalService>().FindGenres(genreSearchString));
        }
    }
}