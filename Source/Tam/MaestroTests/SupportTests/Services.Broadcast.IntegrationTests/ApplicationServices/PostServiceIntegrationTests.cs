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
        [Test]
        [Ignore]
        public void GetPostsTest()
        {
            var postService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostService>();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(postService.GetPosts()));
        }
    }
}
