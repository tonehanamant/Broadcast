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
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    //[EnableCors(origins: "*", headers: "*", methods: "*")] //Enabled globally in global.asax
    public class EnvironmentController : ControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public EnvironmentController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(EnvironmentController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        // GET api/employee/
        [Route("employee")]
        public BaseResponse<BroadcastEmployeeDto> GetEmployee()
        {
            var ssid = HttpContext.Current.Request.LogonUserIdentity.User.Value;
            var employee = SMSClient.Handler.GetEmployee(ssid, false);
            if (employee == null)
            {
                return null;
            }

            var broadcastEmployee = (BroadcastEmployeeDto)employee.Employee;
            broadcastEmployee.LaunchDarklyClientHash = _ResolveLaunchDarklyCredentials(broadcastEmployee.Email);

            return _ConvertToBaseResponse(() => broadcastEmployee);
        }

        private string _ResolveLaunchDarklyCredentials(string username)
        {
            var clientHash = _ApplicationServiceFactory.GetApplicationService<IEnvironmentService>()
                .AuthenticateUserAgainstLaunchDarkly(username);
            return clientHash;
        }

        //GET api/environment
        [Route("environment")]
        public BaseResponse<string> GetSystemEnvironment()
        {
            return _ConvertToBaseResponse(() => new AppSettings().Environment.ToString());
        }

        /// <summary>
        /// Get environment information
        /// </summary>
        [Route("~/api/v1/environment")]
        public BaseResponse<EnvironmentDto> GetSystemEnvironment_v1()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IEnvironmentService>().GetEnvironmentInfo());
        }

        [Route("dbinfo")]
        public BaseResponse<Dictionary<string,string>> GetBroadcastDbInfo()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IEnvironmentService>().GetDbInfo());
        }

    }
}