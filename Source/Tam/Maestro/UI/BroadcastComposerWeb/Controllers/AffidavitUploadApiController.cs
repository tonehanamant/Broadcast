using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/AffidavitUpload")]
    public class AffidavitUploadApiController : BroadcastControllerBase
    {
        private readonly IWebLogger _Logger;

        public AffidavitUploadApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory) : base(logger,
            new ControllerNameRetriever(typeof(AffidavitUploadApiController).Name), applicationServiceFactory)
        {
            _Logger = logger;
        }

        [HttpPost]
        [Route("SaveAffidavit")]
        [Authorize]
        public BaseResponse<WWTVSaveResult> SaveAffidavit(InboundFileSaveRequest saveRequest)
        {
            var fullName = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(() =>
                    _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().SaveAffidavit(saveRequest, fullName, DateTime.Now));
        }
    }
}