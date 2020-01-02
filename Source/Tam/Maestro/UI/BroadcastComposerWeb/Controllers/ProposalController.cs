using Common.Services.ApplicationServices;
using Common.Services.WebComponents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
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
    public class ProposalController : BroadcastControllerBase
    {
        private readonly IWebLogger _Logger;

        public ProposalController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(ProposalController).Name), applicationServiceFactory)
        {
            _Logger = logger;
        }

        [HttpGet]
        [Route("GetProposals")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<List<DisplayProposal>> GetAllProposals()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IProposalService>().GetAllProposals());
        }

        [HttpGet]
        [Route("InitialData")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<ProposalLoadDto> GetInitialProposalData()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                        .GetInitialProposalData(DateTime.Now));
        }

        [HttpPost]
        [Route("SaveProposal")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<ProposalDto> SaveProposal(ProposalDto proposal)
        {
            var fullName = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                            .SaveProposal(proposal, fullName, DateTime.Now));
        }

        [HttpDelete]
        [Route("DeleteProposal/{proposalId}")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
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
            var fullName = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                            .UnorderProposal(proposalId, fullName));
        }

        [HttpGet]
        [Route("GenerateScxArchive/{proposalId}")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public HttpResponseMessage GenerateScxArchive(int proposalId)
        {
            throw new HttpException(404, "Feature not supported");

            //var archive = _ApplicationServiceFactory.GetApplicationService<IProposalService>()
            //    .GenerateScxFileArchive(proposalId);

            //var result = Request.CreateResponse(HttpStatusCode.OK);
            //result.Content = new StreamContent(archive.Item2);
            //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            //result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            //{
            //    FileName = archive.Item1
            //};

            //return result;
        }

        [HttpPut]
        [Route("RerunScrubbing/{proposalId}/{proposalDetailId}")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<bool> RerunScrubbing(int proposalId, int proposalDetailId)
        {
            RescrubProposalDetailRequest request = new RescrubProposalDetailRequest()
            {
                ProposalId = proposalId,
                ProposalDetailId = proposalDetailId
            };

            var fullName = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(() =>
                    _ApplicationServiceFactory.GetApplicationService<IAffidavitService>()
                        .RescrubProposalDetail(request, fullName, DateTime.Now));
        }

        [HttpPost]
        [Route("GetProposalDetail")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<ProposalDetailDto> GetProposalDetail(ProposalDetailRequestDto proposalDetailRequestDto)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                            .GetProposalDetail(proposalDetailRequestDto));
        }

        [HttpPost]
        [Route("CalculateProposalChanges")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<ProposalDto> CalculateProposalChanges(ProposalChangeRequest changeRequest)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                            .CalculateProposalChanges(changeRequest));
        }

        [HttpGet]
        [Route("Proposal/{proposalId}")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<ProposalDto> GetProposal(int proposalId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                        .GetProposalById(proposalId));
        }

        [HttpGet]
        [Route("Proposal/{proposalId}/Versions/{version}")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<ProposalDto> GetProposal(int proposalId, int version)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                        .GetProposalByIdWithVersion(proposalId, version));
        }

        [HttpGet]
        [Route("Proposal/{proposalId}/Versions")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<List<DisplayProposalVersion>> GetProposalVersions(int proposalId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                        .GetProposalVersionsByProposalId(proposalId));
        }

        [HttpGet]
        [Route("Proposal/{proposalId}/Lock")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<LockResponse> LockProposal(int proposalId)
        {
            var key = KeyHelper.GetProposalLockingKey(proposalId);
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IBroadcastLockingManagerApplicationService>()
                    .LockObject(key));
        }

        [HttpGet]
        [Route("Proposal/{proposalId}/UnLock")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<ReleaseLockResponse> UnlockProposal(int proposalId)
        {
            var key = KeyHelper.GetProposalLockingKey(proposalId);
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IBroadcastLockingManagerApplicationService>()
                    .ReleaseObject(key));
        }

        [HttpGet]
        [Route("FindGenres/{genreSearchString}")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<List<LookupDto>> FindGenres(string genreSearchString)
        {

            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                    .FindGenres(genreSearchString));
        }

        [HttpGet]
        [Route("FindShowType/{showTypeSearchString}")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<List<LookupDto>> FindShowType(string showTypeSearchString)
        {

            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                    .FindShowType(showTypeSearchString));
        }

        [HttpPost]
        [Route("FindPrograms")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<List<LookupDto>> FindPrograms(ProgramSearchRequest request)
        {

            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                    .FindPrograms(request, Request.RequestUri.ToString()));
        }

        [HttpPost]
        [Route("FindProgramsExternalApi")]
        public BaseResponse<List<LookupDto>> FindProgramsExternalApi(ProgramSearchRequest request)
        {

            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IProposalService>()
                    .FindProgramsExternalApi(request));
        }

        [HttpPost]
        [Route("UploadProposalDetailBuy")]
        [Authorize]
        public BaseResponse<List<string>> UploadProposalDetailBuy(HttpRequestMessage proposalBuy)
        {
            var response = _ConvertToBaseResponse(() =>
            {
                if (proposalBuy == null)
                {
                    throw new Exception("No proposal buy file data received.");
                }
                ProposalBuySaveRequestDto proposalBuyRequest = JsonConvert.DeserializeObject<ProposalBuySaveRequestDto>(proposalBuy.Content.ReadAsStringAsync().Result, new IsoDateTimeConverter { DateTimeFormat = "MM-dd-yyyy" });
                proposalBuyRequest.Username = _GetCurrentUserFullName();
                var errors = _ApplicationServiceFactory.GetApplicationService<IProposalService>().SaveProposalBuy(proposalBuyRequest);
                return errors;
            });

            if (response.Data != null && response.Data.Any()) //check if any errors returned
            {
                response.Success = false;
            }

            return response;
        }

        [HttpGet]
        [Route("GenerateScxDetail/{proposalDetailId}")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public HttpResponseMessage GgenerateScxDetail(int proposalDetailId)
        {
            throw new HttpException(404, "Feature not supported");

            //var archive = _ApplicationServiceFactory.GetApplicationService<IProposalService>()
            //    .GenerateScxFileDetail(proposalDetailId);

            //var result = Request.CreateResponse(HttpStatusCode.OK);
            //result.Content = new StreamContent(archive.Item2);
            //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            //result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            //{
            //    FileName = archive.Item1
            //};

            //return result;
        }
    }
}