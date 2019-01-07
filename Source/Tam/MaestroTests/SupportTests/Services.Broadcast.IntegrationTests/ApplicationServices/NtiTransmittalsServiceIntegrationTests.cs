using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Nti;
using System;
using System.IO;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class NtiTransmittalsServiceIntegrationTests
    {
        private readonly INtiTransmittalsService _NtiTransmittalsService = IntegrationTestApplicationServiceFactory.GetApplicationService<INtiTransmittalsService>();

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SendPdfDocumentToNielson()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _NtiTransmittalsService.SendPdfDocumentToNielson(new FileRequest
                {
                    FileName = "TLA1217 P3 TRANSMITTALS.PDF",
                    RawData = Convert.ToBase64String(File.ReadAllBytes(@".\Files\TLA1217 P3 TRANSMITTALS.PDF"))
                });

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadNtiTransmittalsFile()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _NtiTransmittalsService.UploadNtiTransmittalsFile(new FileRequest
                {
                    FileName = "TLA1217 P3 TRANSMITTALS.PDF",
                    RawData = Convert.ToBase64String(File.ReadAllBytes(@".\Files\TLA1217 P3 TRANSMITTALS.PDF"))
                }, "integration test user");

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadNtiTransmittalsFile_ProcessFile()
        {
            using (new TransactionScopeWrapper())
            {
                var nielsenDocument = _NtiTransmittalsService.SendPdfDocumentToNielson(new FileRequest
                {
                    FileName = "TLA1217 P3 TRANSMITTALS.PDF",
                    RawData = Convert.ToBase64String(File.ReadAllBytes(@".\Files\TLA1217 P3 TRANSMITTALS.PDF"))
                });
                NtiFile ntiFile = new NtiFile
                {
                    CreatedDate = DateTime.Now,
                    CreatedBy = "integration test user",
                    FileName = "TLA1217 P3 TRANSMITTALS.PDF"
                };
                _NtiTransmittalsService.ProcessFileContent(ntiFile, nielsenDocument);
                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(NtiFile), "CreatedDate");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(ntiFile, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadNtiTransmittalsFile_InvalidFile()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _NtiTransmittalsService.UploadNtiTransmittalsFile(new FileRequest
                {
                    FileName = "Test Adv NAV3 30 05-30-16.txt",
                    RawData = Convert.ToBase64String(File.ReadAllBytes(@".\Files\Test Adv NAV3 30 05-30-16.txt"))
                }, "integration test user");

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }
    }
}
