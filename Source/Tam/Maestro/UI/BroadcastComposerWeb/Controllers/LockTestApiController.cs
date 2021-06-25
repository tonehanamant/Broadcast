using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/LockTestApi")]
    public class LockTestApiController : BroadcastControllerBase
    {
        public LockTestApiController(BroadcastApplicationServiceFactory applicationServiceFactory) : 
            base(new ControllerNameRetriever(typeof(LockTestApiController).Name), applicationServiceFactory)
        {
        }

        [HttpGet]
        [Route("Stations/{stationCode}/Lock")]
        [Authorize]
        public BaseResponse<BroadcastLockResponse> LockStation(int stationCode)
        {
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().LockStation(stationCode));
        }

        [HttpGet]
        [Route("Stations/{stationCode}/UnLock")]
        [Authorize]
        public BaseResponse<BroadcastReleaseLockResponse> UnlockStation(int stationCode)
        {
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().UnlockStation(stationCode));
        }
    }
}
