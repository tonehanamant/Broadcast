using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Isci;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;


namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/plan-iscis")]
    public class PlanIsciApiController : BroadcastControllerBase
    {
        public PlanIsciApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(new ControllerNameRetriever(typeof(PlanIsciApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Endpoint for listing iscis based on search key in the system.
        /// </summary>
        /// <returns>List of iscis</returns>
        [HttpPost]
        [Route("available-isci")]
        public BaseResponse<List<IsciListItemDto>> GetAvailableIscis(IsciSearchDto isciSearch)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPlanIsciService>().GetAvailableIscis(isciSearch));
        }

        /// <summary>
        /// Gets media months
        /// </summary>
        /// <returns>List of MediaMonthDto object</returns>
        [HttpGet]
        [Route("media-months")]
        public BaseResponse<List<MediaMonthDto>> GetMediaMonths()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanIsciService>().GetMediaMonths());
        }

        /// <summary>
        /// Gets the available plans for Isci mapping
        /// </summary>
        /// <param name="isciPlanSearch">The object which contains search parameters</param>
        /// <returns>List of IsciPlanResultDto object</returns>
        [HttpPost]
        [Route("available-plans")]
        public BaseResponse<List<IsciPlanResultDto>> GetAvailableIsciPlans(IsciSearchDto isciPlanSearch)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanIsciService>().GetAvailableIsciPlans(isciPlanSearch));
        }

        /// <summary>
        /// Endpoint to save isci Mappings.
        /// </summary>
        /// <returns>True or False</returns>
        [HttpPost]
        [Route("plan-iscis")]
        [Authorize]
        public BaseResponse<bool> SaveIsciMappings(IsciPlanMappingsSaveRequestDto isciPlanProductsMapping)
        {
            var createdBy = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPlanIsciService>().SaveIsciMappings(isciPlanProductsMapping, createdBy));
        }

        [HttpGet]
        [Route("plan-iscis-details")]
        public BaseResponse<PlanIsciMappingsDetailsDto> GetPlanIsciMappingsDetails(int planId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPlanIsciService>().GetPlanIsciMappingsDetails(planId));
        }

        [HttpPost]
        [Route("isci-search")]
        public BaseResponse<SearchPlanIscisDto> SearchPlanIscis(SearchIsciRequestDto searchIsciRequestDto)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPlanIsciService>().SearchPlanIscisByName(searchIsciRequestDto));
        }

        [HttpGet]
        [Route("target-plans")]
        public BaseResponse<IsciTargetPlansDto> GetTargetPlans(int sourcePlanId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPlanIsciService>().GetTargetIsciPlans(sourcePlanId));
        }
    }
}
