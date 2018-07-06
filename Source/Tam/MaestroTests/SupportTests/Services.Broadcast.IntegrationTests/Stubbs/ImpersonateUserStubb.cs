using System;
using System.Collections.Generic;
using System.Net.Mail;
using Common.Services;
using Services.Broadcast.ApplicationServices.Security;


namespace Services.Broadcast.IntegrationTests
{
    public class ImpersonateUserStubb : IImpersonateUser
    {
        public void Impersonate(string domainName, string userName, string userPassword, Action actionToExecute)
        {
            actionToExecute.Invoke();
        }
    }

}