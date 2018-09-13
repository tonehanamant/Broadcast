using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Net.Http;
using System.Web.Http;
using Common.Services.WebComponents;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/AffidavitUpload")]
    public class AffidavitUploadApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public AffidavitUploadApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory) : base(logger,
            new ControllerNameRetriever("AffidavitUploadApiController"))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
            _Logger = logger;
        }

        [HttpPost]
        [Route("SaveAffidavit")]
        public BaseResponse<WWTVSaveResult> SaveAffidavit(InboundFileSaveRequest saveRequest)
        {
            return
                _ConvertToBaseResponse(() =>
                    _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().SaveAffidavit(saveRequest, Identity.Name, DateTime.Now));
        }
    }
}