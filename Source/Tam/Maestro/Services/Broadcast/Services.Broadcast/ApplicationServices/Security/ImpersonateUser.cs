using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace Services.Broadcast.ApplicationServices.Security
{
    public interface IImpersonateUser
    {
        void Impersonate(string domainName, string userName, string userPassword, Action actionToExecute);
    }
    /// <summary>
    /// </summary>
    public class ImpersonateUser : IImpersonateUser
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
        int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Impersonate(string domainName, string userName, string userPassword, Action actionToExecute)
        {
            SafeTokenHandle safeTokenHandle;
            try
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userPassword))
                {   // missing user info, then execute action as normal.
                    actionToExecute();
                    return;
                }

                const int LOGON32_PROVIDER_DEFAULT = 0;
                //This parameter causes LogonUser to create a primary token.
                const int LOGON32_LOGON_INTERACTIVE = 2;

                // Call LogonUser to obtain a handle to an access token.
                bool returnValue = LogonUser(userName, domainName, userPassword,
                    LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                    out safeTokenHandle);
                //Log("LogonUser called.");

                if (returnValue == false)
                {
                    int ret = Marshal.GetLastWin32Error();
                    //Log($"LogonUser failed with error code : {ret}");

                    throw new System.ComponentModel.Win32Exception(ret);
                }

                using (safeTokenHandle)
                {
                    //Log($"Value of Windows NT token: {safeTokenHandle}");
                    //Log($"Before impersonation: {WindowsIdentity.GetCurrent().Name}");

                    // Use the token handle returned by LogonUser.
                    using (WindowsIdentity newId = new WindowsIdentity(safeTokenHandle.DangerousGetHandle()))
                    {
                        using (WindowsImpersonationContext impersonatedUser = newId.Impersonate())
                        {
                            //Log($"impersonation: {WindowsIdentity.GetCurrent().Name}");

                            actionToExecute();

                            //Log("Finished impersonation and executing an action");
                        }
                    }
                    //Log($"After closing the context: {WindowsIdentity.GetCurrent().Name}");
                }

            }
            catch 
            {
                //Log("Error");
                throw;
            }
        }
    }

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }
}
