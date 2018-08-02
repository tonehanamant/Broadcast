using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Common.Services;
using NUnit.Framework;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    [TestFixture]

    class FtpServiceTester
    {
        [Ignore]
        [Test]
        public void TestFileList()
        {
            FtpService srv = new FtpService();

            var userName = "tamguest";
            var password = "visitor@tam";

            NetworkCredential creds = new NetworkCredential(userName, password);
            var site = "ftp://ftp.cadentnetwork.com";
            var list = srv.GetFileList(creds, site);
        }
    }
}
