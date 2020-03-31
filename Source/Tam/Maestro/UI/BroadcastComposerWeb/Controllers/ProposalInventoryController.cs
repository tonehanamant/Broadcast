using Common.Services.WebComponents;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Services.Broadcast.Entities.DTO;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.ApplicationServices.Security;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/Inventory")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class ProposalInventoryController : BroadcastControllerBase
    {
        public ProposalInventoryController(BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(ProposalInventoryController).Name), applicationServiceFactory)
        {
        }

        [HttpGet]
        [Route("Proprietary/Detail/{proposalDetailId}")]
        public BaseResponse<ProposalDetailProprietaryInventoryDto> GetProprietaryInventoryByDetailId(int proposalDetailId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>().GetInventory(proposalDetailId));
        }

        [HttpGet]
        [Route("OpenMarket/Detail/{proposalDetailId}")]
        public BaseResponse<ProposalDetailOpenMarketInventoryDto> GetOpenMarketInventoryByDetailId(int proposalDetailId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>().GetInventory(proposalDetailId));
        }

        [HttpPost]
        [Route("OpenMarket/UpdateTotals")]
        public BaseResponse<ProposalDetailOpenMarketInventoryDto> UpdateOpenMarketInventoryTotals(ProposalDetailOpenMarketInventoryDto proposalInventoryDto)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>().UpdateOpenMarketInventoryTotals(proposalInventoryDto));
        }

        [HttpPost]
        [Route("OpenMarket/ApplyFilter")]
        public BaseResponse<ProposalDetailOpenMarketInventoryDto> ApplyFilterOnOpenMarketInventory(ProposalDetailOpenMarketInventoryDto proposalInventoryDto)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>().ApplyFilterOnOpenMarketInventory(proposalInventoryDto));
        }


        [HttpPost]
        [Route("OpenMarket/Detail/{proposalDetailId}/Refine")]
        public BaseResponse<ProposalDetailOpenMarketInventoryDto> GetOpenMarketInventoryByDetailId(HttpRequestMessage request)
        {
            var actualRequest = JsonConvert.DeserializeObject<OpenMarketRefineProgramsRequest>(request.Content.ReadAsStringAsync().Result);
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>().RefinePrograms(actualRequest));
        }

        [HttpPost]
        [Route("OpenMarket")]
        public BaseResponse<ProposalDetailOpenMarketInventoryDto> SaveOpenMarketInventoryAllocations(OpenMarketAllocationSaveRequest request)
        {
            request.Username = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>()
                            .SaveInventoryAllocations(request));
        }

        [HttpPost]
        [Route("Proprietary")]
        public BaseResponse<List<ProprietaryInventoryAllocationConflict>> SaveProprietaryInventoryAllocations(HttpRequestMessage request)
        {
            var actualRequest = JsonConvert.DeserializeObject<ProprietaryInventoryAllocationRequest>(request.Content.ReadAsStringAsync().Result);

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>().SaveInventoryAllocations(actualRequest));
        }

        [HttpPost]
        [Route("Detail/Totals")]
        public BaseResponse<ProposalInventoryTotalsDto> CalculateProposalDetailInventoryTotals(ProposalInventoryTotalsRequestDto request)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>().GetInventoryTotals(request));
        }

        [HttpPost]
        [Route("OpenMarket/CheckForAllocatedSpots")]
        public BaseResponse<bool> CheckForAllocatedSpots(CheckForAllocatedSpotsRequestDto request)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IProposalOpenMarketInventoryService>().CheckForAllocatedSpots(request));
        }
    }
}
