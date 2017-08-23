using System.Security.Principal;
using System.ServiceModel;

namespace Common.Services
{
    public interface IServiceSecurityContextWrapper
    {
        WindowsIdentity GetCurrentWindowsIdentity();
    }

    public class ServiceSecurityContextWrapper : IServiceSecurityContextWrapper
    {
        public WindowsIdentity GetCurrentWindowsIdentity()
        {
            return ServiceSecurityContext.Current.WindowsIdentity;
        }
    }
}
