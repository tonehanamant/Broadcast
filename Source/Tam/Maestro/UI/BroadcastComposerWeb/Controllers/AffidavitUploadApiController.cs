﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web.Http;
using Common.Services.WebComponents;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/AffidavitUpload")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class AffidavitUploadApiController : BroadcastControllerBase
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
        public BaseResponse<int> SaveAffidavit(AffidavitSaveRequest saveRequest)
        {
            return
                _ConvertToBaseResponse(() =>
                    _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().SaveAffidavit(saveRequest, Identity.Name, DateTime.Now));
        }
    }
}