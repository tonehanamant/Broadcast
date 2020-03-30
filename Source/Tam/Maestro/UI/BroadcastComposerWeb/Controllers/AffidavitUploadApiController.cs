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
        public AffidavitUploadApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory) : base(new ControllerNameRetriever(typeof(AffidavitUploadApiController).Name), applicationServiceFactory)
        {
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