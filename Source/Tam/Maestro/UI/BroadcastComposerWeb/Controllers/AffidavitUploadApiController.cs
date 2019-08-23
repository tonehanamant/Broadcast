﻿using System;
using System.Web.Http;
using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Tam.Maestro.Services.Cable.Entities;
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
            new ControllerNameRetriever(typeof(AffidavitUploadApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
            _Logger = logger;
        }

        [HttpPost]
        [Route("SaveAffidavit")]
        [Authorize]
        public BaseResponse<WWTVSaveResult> SaveAffidavit(InboundFileSaveRequest saveRequest)
        {
            return
                _ConvertToBaseResponse(() =>
                    _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().SaveAffidavit(saveRequest, Identity.Name, DateTime.Now));
        }
    }
}