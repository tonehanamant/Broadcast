using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;
using System.Web.Http;
using Common.Services.WebComponents;
using Services.Broadcast.Entities.DTO.PricingGuide;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/PricingGuide")]
    public class PricingGuideController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public PricingGuideController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(PricingGuideController).Name))
        {
            _Logger = logger;
            _ApplicationServiceFactory = applicationServiceFactory;
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
        public BaseResponse<bool> SavePricingGuideModel(ProposalDetailPricingGuideSaveRequestDto model)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPricingGuideService>().SaveDistribution(model, Identity.Name));
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