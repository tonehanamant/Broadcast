using System;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [Category("short_running")]
    public class ProjectionBooksService
    {
        private readonly IProjectionBooksService _projectionBooksService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProjectionBooksService>();

        [Test]
        public void CanReturnDefaultPostingBooks()
        {
            var dateTime = new DateTime(2016, 02, 1);
            var defaultPostingBook = _projectionBooksService.GetDefaultProjectionBooks(dateTime);

            Assert.AreEqual(413, defaultPostingBook.DefaultShareBook.PostingBookId);
            Assert.AreEqual(401, defaultPostingBook.DefaultHutBook.PostingBookId);
            Assert.IsFalse(defaultPostingBook.DefaultShareBook.HasWarning);
            Assert.IsFalse(defaultPostingBook.DefaultHutBook.HasWarning);
        }

        [Test]
        [Ignore]
        public void DefaultShareBookNotAvailable()
        {
            var dateTime = new DateTime(2005, 01, 1);
            var defaultPostingBook = _projectionBooksService.GetDefaultProjectionBooks(dateTime);

            Assert.IsNull(defaultPostingBook.DefaultShareBook.PostingBookId);
            Assert.IsNull(defaultPostingBook.DefaultHutBook.PostingBookId);
            Assert.IsTrue(defaultPostingBook.DefaultShareBook.HasWarning);
            Assert.IsTrue(defaultPostingBook.DefaultHutBook.HasWarning);
            Assert.AreEqual(Broadcast.ApplicationServices.ProjectionBooksService.ShareBookNotFound, defaultPostingBook.DefaultShareBook.WarningMessage);
        }

        [Test]
        public void DefaultHutBookNotAvailable()
        {
            var dateTime = new DateTime(2015, 05, 1);
            var defaultPostingBook = _projectionBooksService.GetDefaultProjectionBooks(dateTime);

            Assert.AreEqual(404, defaultPostingBook.DefaultShareBook.PostingBookId);
            Assert.AreEqual(401, defaultPostingBook.DefaultHutBook.PostingBookId);
            Assert.IsFalse(defaultPostingBook.DefaultShareBook.HasWarning);
            Assert.IsTrue(defaultPostingBook.DefaultHutBook.HasWarning);
            Assert.AreEqual(Broadcast.ApplicationServices.ProjectionBooksService.HutBookForLastYearNotAvailable, defaultPostingBook.DefaultHutBook.WarningMessage);
        }

        [Test]
        public void UseShareOnlyWhenInThirdQuarterOfYear()
        {
            var dateTime = new DateTime(2015, 07, 1);
            var defaultPostingBook = _projectionBooksService.GetDefaultProjectionBooks(dateTime);

            Assert.AreEqual(406, defaultPostingBook.DefaultShareBook.PostingBookId);
            Assert.IsNull(defaultPostingBook.DefaultHutBook.PostingBookId);
            Assert.IsFalse(defaultPostingBook.DefaultShareBook.HasWarning);
            Assert.IsFalse(defaultPostingBook.DefaultHutBook.HasWarning);
        }

        [Test]
        public void NoPostingBooksAvailable()
        {
            var dateTime = new DateTime(2014, 01, 01);
            var defaultPostingBook = _projectionBooksService.GetDefaultProjectionBooks(dateTime);

            Assert.IsNull(defaultPostingBook.DefaultShareBook.PostingBookId);
            Assert.IsNull(defaultPostingBook.DefaultHutBook.PostingBookId);
            Assert.IsTrue(defaultPostingBook.DefaultShareBook.HasWarning);
            Assert.IsTrue(defaultPostingBook.DefaultHutBook.HasWarning);
            Assert.AreEqual(Broadcast.ApplicationServices.ProjectionBooksService.ShareBookNotFound, defaultPostingBook.DefaultShareBook.WarningMessage);
            Assert.AreEqual(Broadcast.ApplicationServices.ProjectionBooksService.HutBookNotFound, defaultPostingBook.DefaultHutBook.WarningMessage);
        }
    }
}
