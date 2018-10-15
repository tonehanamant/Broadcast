using Common.Services.WebComponents;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/SpotTracker")]
    public class SpotTrackerController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public SpotTrackerController(
           IWebLogger logger,
           BroadcastApplicationServiceFactory applicationServiceFactory)
           : base(logger, new ControllerNameRetriever(typeof(SpotTrackerController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }
        
        [HttpPost]
        [Route("UploadExtendedSigmaFile")]
        public BaseResponse<List<string>> UploadSigmaFile(HttpRequestMessage saveRequest)
        {
            if (saveRequest == null)
            {
                throw new Exception("No Sigma file data received.");
            }

            FileSaveRequest request = JsonConvert.DeserializeObject<FileSaveRequest>(saveRequest.Content.ReadAsStringAsync().Result);
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotTrackerService>().SaveSigmaFile(request, Identity.Name));
        }

        [HttpGet]
        [Route("SpotTrackerReport/{proposalId}")]
        public HttpResponseMessage GenerateSpotTrackerReport(int proposalId)
        {
            var report = _ApplicationServiceFactory
                    .GetApplicationService<ISpotTrackerService>()
                    .GenerateSpotTrackerReport(proposalId);

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(report.Stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = report.Filename
            };

            return result;
        }
    }
}