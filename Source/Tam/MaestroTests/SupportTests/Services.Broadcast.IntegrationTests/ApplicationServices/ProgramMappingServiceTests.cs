using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Common.Services;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Common.DataLayer;

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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void GetUnmappedProgramsTest()
        {
            var result = _ProgramMappingService.GetUnmappedPrograms();

            // Take 100 programs only otherwise the result file is too big and the file comparer
            // is not able to compare the results.
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result.Take(100)));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ProgramMappingsTestNoError()
        {
            using (new TransactionScopeWrapper())
            {
                var sharedFolderServiceFake = IntegrationTestApplicationServiceFactory.GetApplicationService<ISharedFolderService>();
                var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappings.xlsx", FileMode.Open);
                var sharedFolderFile = new SharedFolderFile
                {
                    FolderPath = Path.GetTempPath(),
                    FileNameWithExtension = "ProgramMappings.xlsx",
                    FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileUsage = SharedFolderFileUsage.ProgramLineup,
                    CreatedDate = new DateTime(2020, 8, 28),
                    CreatedBy = "IntegrationTestUser",
                    FileContent = fileStream
                };
                var fileGuid = sharedFolderServiceFake.SaveFile(sharedFolderFile);
                _ProgramMappingService.RunProgramMappingsProcessingJob(fileGuid, "IntegrationTestUser", new DateTime(2020, 8, 28));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ProgramMappingsTestProgramNotInMasterList()
        {
            using (new TransactionScopeWrapper())
            {
                var sharedFolderServiceFake = IntegrationTestApplicationServiceFactory.GetApplicationService<ISharedFolderService>();
                var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsNotInMasterList.xlsx", FileMode.Open);
                var sharedFolderFile = new SharedFolderFile
                {
                    FolderPath = Path.GetTempPath(),
                    FileNameWithExtension = "ProgramMapNotInMasterList.xlsx",
                    FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileUsage = SharedFolderFileUsage.ProgramLineup,
                    CreatedDate = new DateTime(2020, 8, 28),
                    CreatedBy = "IntegrationTestUser",
                    FileContent = fileStream
                };
                var fileGuid = sharedFolderServiceFake.SaveFile(sharedFolderFile);
                
                var exception = Assert.Throws<Exception>(() => _ProgramMappingService.RunProgramMappingsProcessingJob(fileGuid, "IntegrationTestUser", new DateTime(2020, 8, 28)));
                Assert.That(exception.Message, Is.EqualTo("Error parsing program ABBABAB: Program not found in master list or exception list\r\n"));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ProgramMappingsTestInvalidGenre()
        {
            using (new TransactionScopeWrapper())
            {
                var sharedFolderServiceFake = IntegrationTestApplicationServiceFactory.GetApplicationService<ISharedFolderService>();
                var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsInvalidGenre.xlsx", FileMode.Open);
                var sharedFolderFile = new SharedFolderFile
                {
                    FolderPath = Path.GetTempPath(),
                    FileNameWithExtension = "ProgramMappingsInvalidGenre.xlsx",
                    FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileUsage = SharedFolderFileUsage.ProgramLineup,
                    CreatedDate = new DateTime(2020, 8, 28),
                    CreatedBy = "IntegrationTestUser",
                    FileContent = fileStream
                };
                var fileGuid = sharedFolderServiceFake.SaveFile(sharedFolderFile);

                var exception = Assert.Throws<Exception>(() => _ProgramMappingService.RunProgramMappingsProcessingJob(fileGuid, "IntegrationTestUser", new DateTime(2020, 8, 28)));
                Assert.That(exception.Message, Is.EqualTo("Error parsing program America Undercover: Genre not found: Dramatic\r\n"));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ProgramMappingTestProgramInExceptionList()
        {
            using (new TransactionScopeWrapper())
            {
                var sharedFolderServiceFake = IntegrationTestApplicationServiceFactory.GetApplicationService<ISharedFolderService>();
                var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsException.xlsx", FileMode.Open);
                var sharedFolderFile = new SharedFolderFile
                {
                    FolderPath = Path.GetTempPath(),
                    FileNameWithExtension = "ProgramMappingsException.xlsx",
                    FileMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileUsage = SharedFolderFileUsage.ProgramLineup,
                    CreatedDate = new DateTime(2020, 8, 28),
                    CreatedBy = "IntegrationTestUser",
                    FileContent = fileStream
                };
                var fileGuid = sharedFolderServiceFake.SaveFile(sharedFolderFile);

                _ProgramMappingService.RunProgramMappingsProcessingJob(fileGuid, "IntegrationTestUser", new DateTime(2020, 8, 28));
            }
        }
    }
}