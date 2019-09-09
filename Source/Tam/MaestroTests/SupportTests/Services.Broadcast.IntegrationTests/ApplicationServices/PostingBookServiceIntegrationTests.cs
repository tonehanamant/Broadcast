using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    public class PostingBookServiceIntegrationTests
    {
        private readonly IPostingBookService _PostingBookService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostingBookService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetShareBooks()
        {
            var shareBooks = _PostingBookService.GetShareBooks(new DateTime(2019,12,31));

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(shareBooks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetHUTBooksByFlightStartDate()
        {
            var shareBooks = _PostingBookService.GetHUTBooks(new DateTime(2019, 12, 31), null);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(shareBooks));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetHUTBooksByShareBookId()
        {
            var shareBooks = _PostingBookService.GetHUTBooks(null, 437);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(shareBooks));
        }
    }
}
