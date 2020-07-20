using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Mail;

namespace Services.Broadcast.IntegrationTests
{
    [TestFixture]
    [Category("short_running")]
    public class EmailerServiceTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Email_Hello_Test()
        {
            IntegrationTestApplicationServiceFactory.Instance.RegisterType<IEmailerService, EmailerServiceStub>();

            var emailerService = IntegrationTestApplicationServiceFactory.GetApplicationService<IEmailerService>() ;

            (emailerService as EmailerServiceStub).QuickSend(true, "<b>Hello, world.</b>", "Test Hello", MailPriority.Normal, new List<MailAddress>() {new MailAddress("test@crossmw.com")});

            var response = EmailerServiceStub.LastMailMessageGenerated;

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
