using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.IntegrationTests
{
    [TestFixture]
    public class EmailerTests
    {
        private IAffidavitPreprocessingService _AffidavitPreprocessingService;

        public EmailerTests()
        {
            var i = IntegrationTestApplicationServiceFactory.Instance;
        }

        [Ignore]
        [Test]
        public void Email_Hello_Test()
        {
            var from = new MailAddress(BroadcastServiceSystemParameter.EmailFrom);
            Emailer.QuickSend(true, "<b>Hello, world.</b>", "Test Hello", MailPriority.Normal, from, new List<MailAddress>() {new MailAddress("test@crossmw.com")});
        }
    }
}
