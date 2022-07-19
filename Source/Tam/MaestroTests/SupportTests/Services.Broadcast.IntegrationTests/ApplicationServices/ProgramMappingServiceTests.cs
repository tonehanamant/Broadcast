using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Unity;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProgramMappingServiceTests
    {
        private IProgramMappingService _ProgramMappingService;

        [SetUp]
        public void SetUp()
        {
            // Master list is too big and makes the tests take too long to run
            var masterListImporterMock = new Mock<IMasterProgramListImporter>();
            masterListImporterMock.Setup(m => m.ImportMasterProgramList()).Returns(new List<ProgramMappingsDto>
            {
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Comedy" },
                     OfficialShowType = new ShowTypeDto{ Name = "Comedy"},
                      OfficialProgramName = "Family Guy"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "News" },
                     OfficialShowType = new ShowTypeDto{ Name = "News"},
                      OfficialProgramName = "OTHER NEWS"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Documentary" },
                     OfficialShowType = new ShowTypeDto{ Name = "Documentary"},
                      OfficialProgramName = "An Inconvenient Truth"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "News" },
                     OfficialShowType = new ShowTypeDto{ Name = "News"},
                      OfficialProgramName = "NEWS"
                },
                new ProgramMappingsDto
                {
                     OfficialGenre = new Genre{ Name = "Movie" },
                     OfficialShowType = new ShowTypeDto{ Name = "Movie"},
                      OfficialProgramName = "Avengers"
                },
                new ProgramMappingsDto
                {
                    OfficialProgramName = "2 broke girls",
                    OfficialGenre = new Genre{ Name = "Series"},
                    OfficialShowType = new ShowTypeDto{ Name = "Series"},
                },
                new ProgramMappingsDto
                {
                    OfficialProgramName = "Breaking Bad",
                    OfficialGenre = new Genre{ Name = "Series"},
                    OfficialShowType = new ShowTypeDto{ Name = "Series"},
                },
                new ProgramMappingsDto
                {
                    OfficialProgramName = "Champions League",
                    OfficialGenre = new Genre{ Name = "Sports"},
                    OfficialShowType = new ShowTypeDto{ Name = "Sports"},
                },
                new ProgramMappingsDto
                {
                    OfficialProgramName = "America Undercover",
                    OfficialGenre = new Genre{ Name = "Drama"},
                    OfficialShowType = new ShowTypeDto{ Name = "Series"},
                },
                new ProgramMappingsDto
                {
                    OfficialProgramName = "An American in Canada",
                    OfficialGenre = new Genre{ Name = "Drama"},
                    OfficialShowType = new ShowTypeDto{ Name = "Series"},
                },
                new ProgramMappingsDto
                {
                    OfficialProgramName = "The Simpsons",
                    OfficialGenre = new Genre{ Name = "Comedy"},
                    OfficialShowType = new ShowTypeDto{ Name = "Series"},
                }

            });
            IntegrationTestApplicationServiceFactory.Instance.RegisterInstance<IMasterProgramListImporter>(masterListImporterMock.Object);

            _ProgramMappingService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProgramMappingService>();
            var attachmentMicroServiceApiClientMock = new Mock<IAttachmentMicroServiceApiClient>();
            attachmentMicroServiceApiClientMock.Setup(m => m.RegisterAttachment("", "", ""))
                .Returns(
                new RegisterResponseDto
                {
                    AttachmentId = new Guid(),
                    Success = true,
                    Message = "No Error"
                });
        }

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
        public void ExportUnmappedPrograms()
        {
            // the content format, etc is tested via unit tests.
            // here we will just test that it runs end to end 
            // and the export stream is not empty
            const string expectedFileName = "Unmapped_";
            const string fileExtension = ".zip";
            var exportedFile = _ProgramMappingService.ExportUnmappedPrograms();

            // verify it's named well
            Assert.IsTrue(exportedFile.Filename.StartsWith(expectedFileName));
            Assert.IsTrue(exportedFile.Filename.EndsWith(fileExtension));

            // Uncomment this to write to file for a visual...
            //var filePath = $@"c:\temp\{exportedFile.Filename}";
            //using (var fileStream = File.Create(filePath))
            //{
            //    exportedFile.Stream.Seek(0, SeekOrigin.Begin);
            //    exportedFile.Stream.CopyTo(fileStream);
            //    exportedFile.Stream.Seek(0, SeekOrigin.Begin);
            //}

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
            Approvals.Verify(IntegrationTestHelper.ConvertToJsonMoreRounding(result.Take(100)));
        }

        [Test]
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

                var exception = Assert.Throws<InvalidOperationException>(() => _ProgramMappingService.RunProgramMappingsProcessingJob(fileGuid, "IntegrationTestUser", new DateTime(2020, 8, 28)));
                Assert.That(exception.Message, Is.EqualTo("Error parsing program 'ABBABAB': Mapping Program not found in master list or exception list.; MetaData=ABBABAB|ABBABAB|Drama;\r\n"));
            }
        }

        [Test]
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

                var exception = Assert.Throws<InvalidOperationException>(() => _ProgramMappingService.RunProgramMappingsProcessingJob(fileGuid, "IntegrationTestUser", new DateTime(2020, 8, 28)));
                Assert.That(exception.Message, Is.EqualTo("Error parsing program 'America Undercover': Mapping Genre not found: Dramatic; MetaData=America Undercover|America Undercover|Dramatic;\r\n"));
            }
        }

        [Test]
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        [Category("long_running")]
        public void ProgramMappingTestMappedGenre()
        {
            using (new TransactionScopeWrapper())
            {
                var sharedFolderServiceFake = IntegrationTestApplicationServiceFactory.GetApplicationService<ISharedFolderService>();
                var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsMappedGenre.xlsx", FileMode.Open);
                var sharedFolderFile = new SharedFolderFile
                {
                    FolderPath = Path.GetTempPath(),
                    FileNameWithExtension = "ProgramMappingsMappedGenre.xlsx",
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
        [Category("long_running")]
        public void ProgramMappingTestInvertPrepositions()
        {
            using (new TransactionScopeWrapper())
            {
                var sharedFolderServiceFake = IntegrationTestApplicationServiceFactory.GetApplicationService<ISharedFolderService>();
                var fileStream = File.Open(@".\Files\Program Mapping\ProgramMappingsInvertPreposition.xlsx", FileMode.Open);
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