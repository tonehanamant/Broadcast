using BroadcastComposerWeb.Attributes;
using Common.Services.WebComponents;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/Images")]
    public class ImageApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public ImageApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(ImageApiController).Name))
        {
            _Logger = logger;
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        [HttpGet]
        [Route("logo.png")]
        [CacheWebApi(Duration = BroadcastConstants.LogoCachingDurationInMinutes)]
        public HttpResponseMessage GetLogo()
        {
            var logoService = _ApplicationServiceFactory.GetApplicationService<ILogoService>();
            var logo = logoService.GetLogoAsByteArray();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(logo))
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "logo.png"
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

            return response;
        }
    }
}
