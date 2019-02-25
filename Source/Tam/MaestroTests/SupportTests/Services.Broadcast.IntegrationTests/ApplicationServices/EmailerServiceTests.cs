using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using IntegrationTests.Common;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Mail;

namespace Services.Broadcast.IntegrationTests
{
    [TestFixture]
    public class EmailerServiceTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Email_Hello_Test()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStubb>();

            var from = new MailAddress("broadcastsmtp@crossmw.com");

            var emailerService = IntegrationTestApplicationServiceFactory.GetApplicationService<IEmailerService>() ;

            (emailerService as EmailerServiceStubb).QuickSend(true, "<b>Hello, world.</b>", "Test Hello", MailPriority.Normal, from, new List<MailAddress>() {new MailAddress("test@crossmw.com")});

            var response = EmailerServiceStubb.LastMailMessageGenerated;

            var jsonResolver = new IgnorableSerializerContractResolver();
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver,
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(response, jsonSettings));

        }
    }
}
