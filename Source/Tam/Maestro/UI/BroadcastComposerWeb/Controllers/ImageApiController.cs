using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;
using WebApi.OutputCache.V2;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/Images")]
    public class ImageApiController : BroadcastControllerBase
    {
        public ImageApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(ImageApiController).Name), applicationServiceFactory)
        {
        }

        [HttpGet]
        [Route("logo_png")]
        [CacheOutput(ClientTimeSpan = BroadcastConstants.LogoCachingDurationInSeconds, 
                     ServerTimeSpan = BroadcastConstants.LogoCachingDurationInSeconds)]
        public HttpResponseMessage GetLogo()
        {           
            var logoService = _ApplicationServiceFactory.GetApplicationService<ILogoService>();
            var appDataPath = _GetAppDataPath();
            var logo = logoService.GetLogoAsByteArray(appDataPath);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(logo.LogoAsByteArray))
            };
            if (logo.logoFlag)
            {
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = BroadcastConstants.LOGO_FILENAME
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/svg+xml");

                return response;
            }
            else
            {
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = BroadcastConstants.LOGO_PNG_FILENAME
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return response;
            }
            
        }

        [HttpGet]
        [Route("logo")]
        [CacheOutput(ClientTimeSpan = BroadcastConstants.LogoCachingDurationInSeconds,
                     ServerTimeSpan = BroadcastConstants.LogoCachingDurationInSeconds)]
        public HttpResponseMessage GetInventoryLogo(int inventorySourceId)
        {
            var service = _ApplicationServiceFactory.GetApplicationService<ILogoService>();
            var logo = service.GetInventoryLogo(inventorySourceId);

            if (logo == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new MemoryStream(logo.FileContent))
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = logo.FileName
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

            return response;
        }

        [HttpPost]
        [Route("logo")]
        [InvalidateCacheOutput("GetInventoryLogo")]
        [Authorize]
        public BaseResponse UploadInventoryLogo([FromUri] int inventorySourceId, [FromBody] FileRequest saveRequest)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<ILogoService>();
                var fullName = _GetCurrentUserFullName();
                service.SaveInventoryLogo(inventorySourceId, saveRequest, fullName, DateTime.Now);

                return new BaseResponse
                {
                    Success = true
                };
            }
            catch (Exception e)
            {
                return new BaseResponse
                {
                    Success = false,
                    Message = e.Message
                };
            }
        }
    }
}
