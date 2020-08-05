using System.Collections.Generic;
using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void ProgramNameCleanup()
        {
            var input = new List<string>()
            {
                "FAB MORN NWS",
                "*** SEINFELD (ENCORE)",
                "Super Chess Arena AT 09:00AM-10:00AM",
                "SUN The House of Poker SAT",
                "10:30AM Ugreen Sytem",
                "@ 09:00AM-10:00AM Chessmaster",
                "FT. POP OS",
                "GM TODAY",
                "GD MORNING",
                "PLAYING CHESS ON THE WKND",
                "PLAYING W/ YOU"
            };

            var result = _ProgramMappingService.GetCleanPrograms(input);

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));

        }
    }
}