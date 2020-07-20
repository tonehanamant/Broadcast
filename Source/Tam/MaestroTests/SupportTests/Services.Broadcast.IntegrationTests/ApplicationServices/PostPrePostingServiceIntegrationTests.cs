using ApprovalTests;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("long_running")] // marking as a long-running because we are currently not working in this area
    public class PostPrePostingServiceIntegrationTests
    {
        [Ignore("Not certain why we are ignoring this...")]
        [Test]
        public void GetPosts()
        {
            var x = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(x.GetPosts()));
        }

        [Ignore("Not certain why we are ignoring this...")]
        [Test]
        public void GetPost()
        {
            var x = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(x.GetPost(158)));
        }
        
        [Test]
        public void GetPostSettings()
        {
            var x = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();
            var settings = x.GetPostSettings(158);
            Assert.AreEqual(settings.Id, 158);
        }
    }
}
