using System;
using System.Collections.Generic;
using System.Web.Http;
using Common.Services.WebComponents;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/AffadavitUpload")]
    public class AffadavitUploadApiController : ControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public AffadavitUploadApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory) : base(logger, new ControllerNameRetriever("AffadavitUploadApiController"))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
            _Logger = logger;
        }

        [HttpPost]
        [Route("SaveAffadavit")]
        public BaseResponse UploadBvsFile(AffadavitSaveRequest saveRequest)
        {
            if (saveRequest == null)
            {
                throw new Exception("No affadavit data received.");
            }
            //return true until persist story is completed
            return new BaseResponse()
            {
                Success =  true
            };
        }
    }
}