using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    class BulkInventoryFileImportTester
    {
        private IInventoryService _inventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IInventoryService>();

        [Test]
        [Ignore]
        public void RunBulkImport()
        {
            var sourcePath = @"C:\Users\mtrzcianowski\Documents\broadcast\SYN\";

            var files = Directory.GetFiles(sourcePath);

            System.Diagnostics.Debug.WriteLine(string.Format("Loading files from {0}", sourcePath));

            using (StreamWriter output = new StreamWriter(@"C:\Users\mtrzcianowski\Documents\broadcast\SYN.txt"))
            {

                foreach (var filePath in files)
                {
                    using (new TransactionScopeWrapper())
                    {
                        try
                        {
                            var request = new InventoryFileSaveRequest();

                            using (request.RatesStream = new FileStream(
                                filePath,
                                FileMode.Open,
                                FileAccess.Read))
                            {
                                request.UserName = "IntegrationTestUser";
                                _inventoryService.SaveInventoryFile(request);
                            }

                            var message = string.Format("{0} loaded successfully", filePath);
                            System.Diagnostics.Debug.WriteLine(message);
                            //output.WriteLine(message);
                            File.Move(filePath, sourcePath + @"loaded\" + Path.GetFileName(filePath));
                        }
                        catch (Exception e)
                        {
                            var message = string.Format("{0}: {1}", filePath, e.Message);
                            System.Diagnostics.Debug.WriteLine(message);
                            output.WriteLine(message);
                            File.Move(filePath, sourcePath + @"failed\" + Path.GetFileName(filePath));
                        }
                    }
                }

            }

        }
    }
}
