using System.IO;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProgramMappingServiceTests
    {
        private readonly IProgramMappingService _ProgramMappingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProgramMappingService>();

        [Test]
        public void ExportProgramMappingsFile()
        {
            // the content format, etc is tested via unit tests.
            // here we will just test that it runs end to end 
            // and the export stream is not empty
            const string testUsername = "testUser";
            const string expectedFileName = "BroadcastMappedPrograms.xlsx";
            var exportedFile = _ProgramMappingService.ExportProgramMappingsFile(testUsername);

            // verify it's named well
            Assert.AreEqual(expectedFileName, exportedFile.Filename);

            // verify the stream has contents
            string fileContent;
            using (var reader = new StreamReader(exportedFile.Stream))
            {
                fileContent = reader.ReadToEnd();
            }
            var hasContent = fileContent.Length > 0;
            Assert.IsTrue(hasContent);
        }
    }
}