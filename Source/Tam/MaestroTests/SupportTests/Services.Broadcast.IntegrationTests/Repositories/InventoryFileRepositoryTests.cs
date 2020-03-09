using NUnit.Framework;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    public class InventoryFileRepositoryTests
    {
        [Test]
        public void GetLatestInventoryFileIdByName()
        {
            // there are 4 of these
            const string duplicateFileName = "Syndication_ValidFile1.xlsx";
            const int expectedResult = 57;

            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();

            var result = -1;
            using (new TransactionScopeWrapper())
            {
                result = repo.GetLatestInventoryFileIdByName(duplicateFileName);
            }
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void GetLatestInventoryFileIdByName_FileNotFound()
        {
            // there are 4 of these
            const string fileDoesNotExistName = "fileDoesNotExist.xlsx";
            const int expectedResult = 0;

            var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();

            var result = -1;
            using (new TransactionScopeWrapper())
            {
                result = repo.GetLatestInventoryFileIdByName(fileDoesNotExistName);
            }

            Assert.AreEqual(expectedResult, result);
        }
    }
}