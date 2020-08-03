using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Web.Common;

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

            var broadcastEmployee = new BroadcastEmployeeDto(employee.Employee);
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
        /// Is the feature enabled for the logged in user?
        /// The user making the call is used for this query.
        /// </summary>
        /// <remarks>
        ///     For Testing only!!!
        /// </remarks>
        /// <param name="key">The feature toggle key.</param>
        /// <returns>The value of the flag for the user.</returns>
        [Route("TestIsFeatureToggleEnabled")]
        public BaseResponse<bool> GetTestIsFeatureToggleEnabled(string key)
        {
            var ssid = HttpContext.Current.Request.LogonUserIdentity.User.Value;
            var employee = SMSClient.Handler.GetEmployee(ssid, false);
            var loggedInUsername = employee.Employee.Email;

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IEnvironmentService>().IsFeatureToggleEnabled(key, loggedInUsername));
        }

        /// <summary>
        /// Is the feature enabled for the logged an anonymous user?
        /// </summary>
        /// <remarks>
        ///     For Testing only!!!
        /// </remarks>
        /// <param name="key">The feature toggle key.</param>
        /// <returns>The value of the flag for an anonymous user.</returns>
        [Route("TestIsFeatureToggleEnabledUserAnonymous")]
        public BaseResponse<bool> GetTestIsFeatureToggleEnabledUserAnonymous(string key)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IEnvironmentService>().IsFeatureToggleEnabledUserAnonymous(key));
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