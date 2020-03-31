using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;
using System.Web.Http;
using Common.Services.WebComponents;
using Services.Broadcast.Entities.DTO.PricingGuide;
using Services.Broadcast.ApplicationServices.Security;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/PricingGuide")]
    public class PricingGuideController : BroadcastControllerBase
    {
        public PricingGuideController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(PricingGuideController).Name), applicationServiceFactory)
        {
        }

        [HttpGet]
        [Route("{proposalDetailId}")]
        public BaseResponse<PricingGuideDto> GetPricingGuideForProposalDetail(int proposalDetailId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPricingGuideService>().GetPricingGuideForProposalDetail(proposalDetailId));
        }

        [HttpPost]
        [Route("Save")]
        [Authorize]
        public BaseResponse<bool> SavePricingGuideModel(ProposalDetailPricingGuideSaveRequestDto model)
        {
            var fullName = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPricingGuideService>().SaveDistribution(model, fullName));
        }

        [HttpPost]
        [Route("Distribution/AllocateSpots")]
        public BaseResponse<PricingGuideDto> SavePricingGuideAllocations(PricingGuideDto dto)
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPricingGuideService>().SaveAllocations(dto));
        }

        [HttpPost]
        [Route("Distribution")]
        public BaseResponse<PricingGuideDto> GetOpenMarketPricingGuide(PricingGuideOpenMarketInventoryRequestDto request)
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPricingGuideService>().GetOpenMarketInventory(request));
        }

        [HttpPost]
        [Route("Distribution/ApplyFilter")]
        public BaseResponse<PricingGuideDto> ApplyFilterOnOpenMarketPricingGuideGrid(PricingGuideDto dto)
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPricingGuideService>().ApplyFilterOnOpenMarketGrid(dto));
        }

        [HttpPost]
        [Route("Distribution/UpdateMarkets")]
        public BaseResponse<PricingGuideDto> UpdateOpenMarketPricingGuideMarkets(PricingGuideDto dto)
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPricingGuideService>().UpdateOpenMarketMarkets(dto));
        }

        [HttpPost]
        [Route("Distribution/UpdateProprietaryCpms")]
        public BaseResponse<PricingGuideDto> UpdateProprietaryCpms(PricingGuideDto dto)
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPricingGuideService>().UpdateProprietaryCpms(dto));
        }

        [HttpGet]
        [Route("CopyToBuy/{proposalDetailId}")]
        public BaseResponse<bool> CopyPricingModelToBuy(int proposalDetailId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPricingGuideService>()
                    .CopyPricingGuideAllocationsToOpenMarket(proposalDetailId));
        }

        [HttpGet]
        [Route("HasSpotsAllocated/{proposalDetailId}")]
        public BaseResponse<bool> HasSpotsAllocated(int proposalDetailId)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IPricingGuideService>()
                            .HasSpotsAllocated(proposalDetailId));
        }
    }
}