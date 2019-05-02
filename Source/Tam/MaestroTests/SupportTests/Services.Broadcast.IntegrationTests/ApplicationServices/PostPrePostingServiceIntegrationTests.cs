﻿using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PostPrePostingServiceIntegrationTests
    {
        [Ignore]
        [Test]
        public void GetPosts()
        {
            var x = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostPrePostingService>();
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(x.GetPosts()));
        }

        [Ignore]
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
