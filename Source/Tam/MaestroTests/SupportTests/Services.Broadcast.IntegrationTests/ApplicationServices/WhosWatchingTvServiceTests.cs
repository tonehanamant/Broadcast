using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class WhosWatchingTvServiceTests
    {
        private IWhosWatchingTvService _WhosWatchingTvService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IWhosWatchingTvService>();

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void FindSomething()
        {
            var result = _WhosWatchingTvService.FindPrograms("half");

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));

        }
    }
}
