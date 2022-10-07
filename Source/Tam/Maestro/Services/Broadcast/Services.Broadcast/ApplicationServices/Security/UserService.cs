using Common.Services.ApplicationServices;
using System;
using System.Security.Principal;
using System.Web;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.Helpers;
namespace Services.Broadcast.ApplicationServices.Security
{
    public interface IUserService : IApplicationService
    {
        string GetCurrentUserFullName();
    }

    public class UserService : BroadcastBaseClass, IUserService
    {
        public UserService(IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
        {

        }
        public string GetCurrentUserFullName()
        {
            var ssid = _GetCurrentUserSsid();
            _LogInfo($"ssid: {ssid}");
            var employee = SMSClient.Handler.GetEmployee(ssid, false);        
            if (employee == null)
            {
                return null;
            }
            _LogInfo($"Employee: {employee.Employee.FullName}");
            return employee.Employee.FullName;
        }

        private string _GetCurrentUserSsid()
        {
            var ssid = _GetSsidFromHttpContext();
            if (string.IsNullOrEmpty(ssid))
                return _GetSsidFromWindowsIdentity();
            return ssid;
        }

        private string _GetSsidFromHttpContext()
        {
            try
            {
                return HttpContext.Current.Request.LogonUserIdentity.User.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string _GetSsidFromWindowsIdentity() =>
            WindowsIdentity.GetCurrent().User.Value;
    }
}
