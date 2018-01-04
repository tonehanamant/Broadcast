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
    [RoutePrefix("api/AffidavitUpload")]
    public class AffidavitUploadApiController : ControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public AffidavitUploadApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory) : base(logger, new ControllerNameRetriever("AffidavitUploadApiController"))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
            _Logger = logger;
        }

        [HttpPost]
        [Route("SaveAffidavit")]
        public BaseResponse<bool> SaveAffidavit(AffidavitSaveRequest saveRequest)
        {
            return
                _ConvertToBaseResponse(() =>
                    _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().SaveAffidavit(saveRequest));

        }
    }
}