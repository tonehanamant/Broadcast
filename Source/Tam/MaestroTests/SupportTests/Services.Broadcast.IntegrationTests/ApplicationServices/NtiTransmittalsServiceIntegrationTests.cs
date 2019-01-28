using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Nti;
using System;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.Entities;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class NtiTransmittalsServiceIntegrationTests
    {
        private readonly INtiTransmittalsService _NtiTransmittalsService = IntegrationTestApplicationServiceFactory.GetApplicationService<INtiTransmittalsService>();
        private readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadNtiTransmittalsFile_ProcessFile()
        {
            using (new TransactionScopeWrapper())
            {
                var nielsenDocument = JsonConvert.DeserializeObject<BaseResponse<List<NtiRatingDocumentDto>>>((File.ReadAllText(@".\Files\NtiTransmittalsFileStub.txt")));
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

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadNtiTransmittalsFile_InvalidFile()
        {
            using (new TransactionScopeWrapper())
            {
                var nielsenDocument = new BaseResponse<List<NtiRatingDocumentDto>> { Message = "Invalid file", Success = false };
                NtiFile ntiFile = new NtiFile
                {
                    CreatedDate = DateTime.Now,
                    CreatedBy = "integration test user",
                    FileName = "TLA1217 P3 TRANSMITTALS.PDF"
                };
                var result = _NtiTransmittalsService.ProcessFileContent(ntiFile, nielsenDocument);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadNtiTransmittalsFile_BCOP4282()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(26010);
                proposal.Status = Entities.Enums.ProposalEnums.ProposalStatusType.AgencyOnHold;
                _ProposalService.SaveProposal(proposal, "nti transmittal test", DateTime.Now);

                var nielsenDocument = JsonConvert.DeserializeObject<BaseResponse<List<NtiRatingDocumentDto>>>((File.ReadAllText(@".\Files\NtiTransmittalsFileStub.txt")));
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
    }
}
