using System.Web;
using Common.Services.WebComponents;
using System.Web.Http;
using Tam.Maestro.Common.Systems.DataTransferObjects;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Web.Common;
using Services.Broadcast.ApplicationServices;
using System.Collections.Generic;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    //[EnableCors(origins: "*", headers: "*", methods: "*")] //Enabled globally in global.asax
    public class EnvironmentController : ControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public EnvironmentController(IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(EnvironmentController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        // GET api/employee/
        [Route("employee")]
        public BaseResponse<EmployeeDto> GetEmployee()
        {
            var ssid = HttpContext.Current.Request.LogonUserIdentity.User.Value;
            var employee = SMSClient.Handler.GetEmployee(ssid, false);
            if (employee == null)
            {
                return null;
            }
            return _ConvertToBaseResponse(() => employee.Employee );
        }

        //GET api/environment
        [Route("environment")]
        public BaseResponse<string> GetSystemEnvironment()
        {
            return _ConvertToBaseResponse(() => new AppSettings().Environment.ToString());
        }

        [Route("dbinfo")]
        public BaseResponse<Dictionary<string,string>> GetBroadcastDbInfo()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IEnvironmentService>().GetDbInfo());
        }

    }
}