using NUnit.Framework;
using ApprovalTests.Reporters;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;
using ApprovalTests;
using IntegrationTests.Common;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class SpotTrackerServiceIntegrationTests
    {

        private readonly ISpotTrackerService _ISpotTrackerService;

        public SpotTrackerServiceIntegrationTests()
        {
            _ISpotTrackerService = IntegrationTestApplicationServiceFactory.GetApplicationService<ISpotTrackerService>();
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveSigmaExtendedFile()
        {
            using (new TransactionScopeWrapper())
            {
                var fileSaveRequest = new FileSaveRequest()
                {
                    Files = new List<FileRequest>() {
                        new FileRequest()
                        {
                            FileName = "ExtendedSigmaImport_ValidFile.csv",
                            StreamData = new FileStream(@".\Files\ExtendedSigmaImport_ValidFile.csv", FileMode.Open, FileAccess.Read)
                        }
                    }
                };

                var userName = "Test_ExtendedSigmaFile";

                var messages = _ISpotTrackerService.SaveSigmaFile(fileSaveRequest, userName);
                if (messages.Count == 0)
                {
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson("File imported with no messages"));
                }
                else
                {
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(messages));
                }
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveSigmaExtendedFile_DuplicateMessages()
        {
            using (new TransactionScopeWrapper())
            {
                var fileSaveRequest = new FileSaveRequest()
                {
                    Files = new List<FileRequest>() {
                        new FileRequest()
                        {
                            FileName = "ExtendedSigmaImport_ValidFile.csv",
                            StreamData = new FileStream(@".\Files\ExtendedSigmaImport_ValidFile.csv", FileMode.Open, FileAccess.Read)
                        }
                    }
                };
                var fileSaveRequestDuplicate = new FileSaveRequest()
                {
                    Files = new List<FileRequest>() {
                        new FileRequest()
                        {
                            FileName = "ExtendedSigmaImport_ValidFile_DuplicatedMessages.csv",
                            StreamData = new FileStream(@".\Files\ExtendedSigmaImport_ValidFile_DuplicatedMessages.csv", FileMode.Open, FileAccess.Read)
                        }
                    }
                };

                var userName = "Test_ExtendedSigmaFile";

                _ISpotTrackerService.SaveSigmaFile(fileSaveRequest, userName);

                var messages = _ISpotTrackerService.SaveSigmaFile(fileSaveRequestDuplicate, userName);
                if (messages.Count == 0)
                {
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson("File imported with no messages"));
                }
                else
                {
                    Approvals.Verify(IntegrationTestHelper.ConvertToJson(messages));
                }
            }
        }
    }
}
