using Services.Broadcast.ApplicationServices.Security;
using System;

namespace Services.Broadcast.IntegrationTests
{
    public class ImpersonateUserStub : IImpersonateUser
    {
        public void Impersonate(string domainName, string userName, string userPassword, Action actionToExecute)
        {
            actionToExecute.Invoke();
        }
    }

}