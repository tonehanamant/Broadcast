using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/Markets")]
    public class MarketController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public MarketController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(ImageApiController).Name))
        {
            _Logger = logger;
            _ApplicationServiceFactory = applicationServiceFactory;
        }
        
        [HttpGet]
        [Route("LoadCoverages")]
        public HttpResponseMessage LoadCoverages()
        {
            var relativePathToCoveragesFile = WebConfigurationManager.AppSettings["relativePathToMarketCoveragesFile"];

            try
            {
                var serverPathToCoveragesFile = System.Web.Hosting.HostingEnvironment.MapPath(relativePathToCoveragesFile);
                var marketService = _ApplicationServiceFactory.GetApplicationService<IMarketService>();
                marketService.LoadCoverages(serverPathToCoveragesFile);
            }
            catch(Exception e)
            {
                _Logger.LogException("Loading market coverages", e, "MarketService");
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}