using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.ReelRosterIscis;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Services.Broadcast;
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
            _LogInfo("Starting to resolve user info.");

            var ssid = HttpContext.Current.Request.LogonUserIdentity.User.Value;
            var ssidForLog = $"{ssid.Substring(0, 4)}...";
            _LogInfo($"Discovered ssid from http context : '{ssidForLog}'.  Reaching out to SMS...");

            var employee = SMSClient.Handler.GetEmployee(ssid, false);
            if (employee == null)
            {
                _LogError($"SMS did not return user info for ssid '{ssidForLog}'.", null);
                return null;
            }
            _LogInfo($"SMS returns user '{employee.Employee.FullName}' with email '{employee.Employee.Email}' for SSID '{ssidForLog}'.  Reaching out to Launch Darkly to resolve the client hash.");
            var broadcastEmployee = new BroadcastEmployeeDto(employee.Employee);
            broadcastEmployee.LaunchDarklyClientHash = _ResolveLaunchDarklyCredentials(broadcastEmployee.Email);

            var clientHashLength = broadcastEmployee.LaunchDarklyClientHash?.Length ?? -1;

            _LogInfo($"Received an LaunchDarklyClientHash of length '{clientHashLength}' for employee '{employee.Employee.FullName}'.");

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

        /// <summary>
        /// Tests the reel isci API client with the original test case.
        /// </summary>
        [HttpGet]
        [Route("test-reel-isci-api-client-orig")]
        public BaseResponse<List<ReelRosterIsciDto>> TestReelISciApiClientOrig()
        {
            var numDays = 6;
            var startDate = new DateTime(2021, 1, 11);

            var result = _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IReelIsciIngestService>().TestReelISciApiClient(startDate, numDays));
            return result;
        }

        /// <summary>
        /// Tests the reel isci API client dynamically with the start date being Today-numDays
        /// </summary>
        [HttpGet]
        [Route("test-reel-isci-api-client-numDays")]
        public BaseResponse<List<ReelRosterIsciDto>> TestReelISciApiClientDays(int numDays)
        {
            var startDate = DateTime.Now.AddDays(-1 * numDays);

            var result = _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IReelIsciIngestService>().TestReelISciApiClient(startDate, numDays));
            return result;
        }

        /// <summary>
        /// Tests the reel isci API client dynamically with the start date being Today-numDays
        /// </summary>
        [HttpGet]
        [Route("test-reel-isci-api-client-numDays-date")]
        public BaseResponse<List<ReelRosterIsciDto>> TestReelISciApiClientDays(int numDays, DateTime startDate)
        {
            //var startDate = DateTime.Now.AddDays(-1 * numDays);

            var result = _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IReelIsciIngestService>().TestReelISciApiClient(startDate, numDays));
            return result;
        }
    }
}