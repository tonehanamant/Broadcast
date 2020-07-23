using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class WhosWatchingTvServiceTests
    {
        private IWhosWatchingTvService _WhosWatchingTvService =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IWhosWatchingTvService>();

        [Test]
        [Ignore("Not certain why we are ignoring this...")]
        [UseReporter(typeof(DiffReporter))]
        public void FindSomething()
        {
            var result = _WhosWatchingTvService.FindPrograms("half");

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));

        }
    }
}
