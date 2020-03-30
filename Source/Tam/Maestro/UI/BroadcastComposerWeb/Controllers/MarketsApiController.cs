using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System.Collections.Generic;
using System.Web.Http;
using Services.Broadcast.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    /// <summary>
    /// Operations for Markets.
    /// </summary>
    /// <seealso cref="BroadcastComposerWeb.Controllers.BroadcastControllerBase" />
    [RoutePrefix("api/v1/Markets")]
    public class MarketsApiController : BroadcastControllerBase
    {
        public MarketsApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(MarketsApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets the latest created market coverages.
        /// </summary>
        /// <returns>List of <see cref="MarketCoverage"/></returns>
        [HttpGet]
        [Route("LatestCreatedMarketCoverages")]
        public BaseResponse<List<MarketCoverage>> GetLatestCreatedMarketCoverages()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IMarketService>().GetMarketsWithLatestCoverage());
        }
    }
}