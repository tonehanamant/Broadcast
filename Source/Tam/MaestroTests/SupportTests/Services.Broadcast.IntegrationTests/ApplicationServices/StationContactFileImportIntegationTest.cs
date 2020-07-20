using NUnit.Framework;
using Services.Broadcast.Converters;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Ignore]
    public class StationContactFileImportIntegationTest
    {
        private IStationContactMasterFileImporter _stationContactMasterFileImporter = IntegrationTestApplicationServiceFactory.GetApplicationService<IStationContactMasterFileImporter>();

        [Test]
        [Ignore]
        public void CanLoadStationContactsFromFile()
        {
            using (new TransactionScopeWrapper())
            {
                var filename = @".\Files\MASTER_CONTACT_LIST.csv";
                var masterScriptFile = @".\MasterContactScript.sql";
                var stationContactsStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var fileContacts = _stationContactMasterFileImporter.ExtractStationContactScript(stationContactsStream,
                    "Integration Test User");
                File.AppendAllText(masterScriptFile, fileContacts.ToString());
                Assert.IsTrue(File.Exists(masterScriptFile));
                File.Delete(masterScriptFile);
            }
        }
    }
}
