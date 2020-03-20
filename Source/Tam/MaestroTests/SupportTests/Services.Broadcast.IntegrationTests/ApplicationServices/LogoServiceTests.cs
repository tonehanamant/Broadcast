using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using System;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Category("short_running")]
    public class LogoServiceTests
    {
        private readonly ILogoService _LogoService = IntegrationTestApplicationServiceFactory.GetApplicationService<ILogoService>();

        [Test]
        public void InventorySourceLogoTest()
        {
            const string fileName = @"CNN.jpg";
            const int inventorySourceId = 5;
            const string createdBy = "IntegrationTestUser";

            using (new TransactionScopeWrapper())
            {
                var now = new DateTime(2019, 02, 02);
                var request = new FileRequest
                {
                    RawData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read).ToBase64String(),
                    FileName = fileName
                };

                _LogoService.SaveInventoryLogo(inventorySourceId, request, createdBy, now);

                var inventoryLogo = _LogoService.GetInventoryLogo(inventorySourceId);
                var inventoryLogoContent = Convert.ToBase64String(inventoryLogo.FileContent);

                Assert.AreEqual(request.RawData, inventoryLogoContent);
                Assert.AreEqual(request.FileName, inventoryLogo.FileName);
                Assert.AreEqual(inventorySourceId, inventoryLogo.InventorySource.Id);
                Assert.AreEqual(createdBy, inventoryLogo.CreatedBy);
                Assert.AreEqual(now, inventoryLogo.CreatedDate);
            }
        }

        [Test]
        public void InventorySourceLogoTest_WrongExtension()
        {
            const string expectedMessage = "Invalid file format. Please provide a file with one of the following extensions: .png, .jpeg, .jpg, .gif";
            const string fileName = @"CNN.txt";
            const int inventorySourceId = 5;
            const string createdBy = "IntegrationTestUser";

            using (new TransactionScopeWrapper())
            {
                var now = new DateTime(2019, 02, 02);
                var request = new FileRequest
                {
                    RawData = new FileStream($@".\Files\{fileName}", FileMode.Open, FileAccess.Read).ToBase64String(),
                    FileName = fileName
                };

                var exception = Assert.Throws<Exception>(() => _LogoService.SaveInventoryLogo(inventorySourceId, request, createdBy, now));
                Assert.AreEqual(expectedMessage, exception.Message);
            }
        }

        [Test]
        public void InventorySourceLogoTest_NoFileContent()
        {
            const string expectedMessage = "Please send logo image as RawData within a request";
            const string fileName = @"CNN.png";
            const int inventorySourceId = 5;
            const string createdBy = "IntegrationTestUser";

            using (new TransactionScopeWrapper())
            {
                var now = new DateTime(2019, 02, 02);
                var request = new FileRequest
                {
                    FileName = fileName
                };

                var exception = Assert.Throws<Exception>(() => _LogoService.SaveInventoryLogo(inventorySourceId, request, createdBy, now));
                Assert.AreEqual(expectedMessage, exception.Message);
            }
        }
    }
}
