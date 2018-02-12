using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PostServiceIntegrationTests
    {
        [Ignore]
        [Test]
        public void GetPosts()
        {
            var x = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostService>();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(x.GetPosts()));
        }

        [Ignore]
        [Test]
        public void GetPost()
        {
            var x = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostService>();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(x.GetPost(158)));
        }
    }
}
