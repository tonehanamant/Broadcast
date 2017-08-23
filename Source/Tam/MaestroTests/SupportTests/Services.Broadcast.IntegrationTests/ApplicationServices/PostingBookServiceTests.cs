using System;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    public class PostingBookServiceTests
    {
        private readonly IPostingBooksService _postingBooksService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostingBooksService>();

        [Test]
        public void CanReturnDefaultPostingBooks()
        {
            var dateTime = new DateTime(2016, 02, 1);
            var defaultPostingBook = _postingBooksService.GetDefaultPostingBooks(dateTime);

            Assert.AreEqual(413, defaultPostingBook.DefaultShareBook.PostingBookId);
            Assert.AreEqual(401, defaultPostingBook.DefaultHutBook.PostingBookId);
            Assert.IsFalse(defaultPostingBook.DefaultShareBook.HasWarning);
            Assert.IsFalse(defaultPostingBook.DefaultHutBook.HasWarning);
        }

        [Test]
        public void DefaultShareBookNotAvailable()
        {
            var dateTime = new DateTime(2017, 01, 1);
            var defaultPostingBook = _postingBooksService.GetDefaultPostingBooks(dateTime);

            Assert.AreEqual(416, defaultPostingBook.DefaultShareBook.PostingBookId);
            Assert.AreEqual(413, defaultPostingBook.DefaultHutBook.PostingBookId);
            Assert.IsTrue(defaultPostingBook.DefaultShareBook.HasWarning);
            Assert.IsFalse(defaultPostingBook.DefaultHutBook.HasWarning);
            Assert.AreEqual(PostingBooksService.ShareBookForCurrentQuarterNotAvailable, defaultPostingBook.DefaultShareBook.WarningMessage);
        }

        [Test]
        public void DefaultHutBookNotAvailable()
        {
            var dateTime = new DateTime(2015, 05, 1);
            var defaultPostingBook = _postingBooksService.GetDefaultPostingBooks(dateTime);

            Assert.AreEqual(404, defaultPostingBook.DefaultShareBook.PostingBookId);
            Assert.AreEqual(401, defaultPostingBook.DefaultHutBook.PostingBookId);
            Assert.IsFalse(defaultPostingBook.DefaultShareBook.HasWarning);
            Assert.IsTrue(defaultPostingBook.DefaultHutBook.HasWarning);
            Assert.AreEqual(PostingBooksService.HutBookForLastYearNotAvailable, defaultPostingBook.DefaultHutBook.WarningMessage);
        }

        [Test]
        public void UseShareOnlyWhenInThirdQuarterOfYear()
        {
            var dateTime = new DateTime(2015, 07, 1);
            var defaultPostingBook = _postingBooksService.GetDefaultPostingBooks(dateTime);

            Assert.AreEqual(406, defaultPostingBook.DefaultShareBook.PostingBookId);
            Assert.AreEqual(ProposalConstants.UseShareBookOnlyId, defaultPostingBook.DefaultHutBook.PostingBookId);
            Assert.IsFalse(defaultPostingBook.DefaultShareBook.HasWarning);
            Assert.IsFalse(defaultPostingBook.DefaultHutBook.HasWarning);
        }

        [Test]
        public void NoPostingBooksAvailable()
        {
            var dateTime = new DateTime(2014, 01, 01);
            var defaultPostingBook = _postingBooksService.GetDefaultPostingBooks(dateTime);

            Assert.AreEqual(ProposalConstants.ShareBookNotFoundId, defaultPostingBook.DefaultShareBook.PostingBookId);
            Assert.AreEqual(ProposalConstants.UseShareBookOnlyId, defaultPostingBook.DefaultHutBook.PostingBookId);
            Assert.IsTrue(defaultPostingBook.DefaultShareBook.HasWarning);
            Assert.IsTrue(defaultPostingBook.DefaultHutBook.HasWarning);
            Assert.AreEqual(PostingBooksService.ShareBookNotFound, defaultPostingBook.DefaultShareBook.WarningMessage);
            Assert.AreEqual(PostingBooksService.HutBookNotFound, defaultPostingBook.DefaultHutBook.WarningMessage);
        }
    }
}
