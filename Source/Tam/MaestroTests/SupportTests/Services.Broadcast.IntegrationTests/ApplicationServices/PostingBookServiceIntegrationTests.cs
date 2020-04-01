using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using System;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [Category("short_running")]
    public class PostingBookServiceIntegrationTests
    {
        private readonly IPostingBookService _PostingBookService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostingBookService>();

        [Test]
        public void GetShareBooks()
        {
            var shareBookId = _PostingBookService.GetDefaultShareBookId(new DateTime(2019,12,31));

            Assert.AreEqual(437, shareBookId);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetHUTBooks()
        {
            var shareBooks = _PostingBookService.GetHUTBooks(437);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(shareBooks));
        }
    }
}
