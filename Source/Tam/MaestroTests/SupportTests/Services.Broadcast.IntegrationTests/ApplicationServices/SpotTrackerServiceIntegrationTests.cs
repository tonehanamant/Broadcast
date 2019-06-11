using NUnit.Framework;
using ApprovalTests.Reporters;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.IO;
using Tam.Maestro.Common.DataLayer;
using ApprovalTests;
using IntegrationTests.Common;
using Newtonsoft.Json;
using System;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [Ignore]
    public class SpotTrackerServiceIntegrationTests
    {
        private readonly ISpotTrackerService _SpotTrackerService;
        private readonly IProposalService _ProposalService;

        public SpotTrackerServiceIntegrationTests()
        {
            _SpotTrackerService = IntegrationTestApplicationServiceFactory.GetApplicationService<ISpotTrackerService>();
            _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
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

                var messages = _SpotTrackerService.SaveSigmaFile(fileSaveRequest, userName);
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

                _SpotTrackerService.SaveSigmaFile(fileSaveRequest, userName);

                var messages = _SpotTrackerService.SaveSigmaFile(fileSaveRequestDuplicate, userName);
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
        [ExpectedException(typeof(Exception), ExpectedMessage = "File 'ExtendedSigmaImport_ValidFile.csv' failed validation for Sigma import", MatchType = MessageMatch.Contains)]
        public void SaveSigmaExtendedFile_BCOP3634()
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

                _SpotTrackerService.SaveSigmaFile(fileSaveRequest, userName);
                _SpotTrackerService.SaveSigmaFile(fileSaveRequest, userName);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SpotTracker_GenerateEmptyReport_WithoutBuys()
        {
            using (new TransactionScopeWrapper())
            {
                var newProposalDto = ProposalServiceIntegrationTests.SetupProposalDto();
                var newProposalDetailDto = ProposalServiceIntegrationTests.SetupProposalDetailDto();
                newProposalDto.Details.Add(newProposalDetailDto);
                var newProposal = _ProposalService.SaveProposal(newProposalDto, "IntegrationTestUser", DateTime.Now);

                var spotTrackerReportData = _SpotTrackerService.GetSpotTrackerReportDataForProposal(newProposal.Id.Value);
                ApproveResults(spotTrackerReportData);
            }
        }
        
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SpotTracker_GenerateReport_WithoutErrors_WithValidData()
        {
            const int proposalId = 32474;
            
            var spotTrackerReportData = _SpotTrackerService.GetSpotTrackerReportDataForProposal(proposalId);
            ApproveResults(spotTrackerReportData);
        }

        private void ApproveResults(SpotTrackerReport spotTrackerReportData)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(SpotTrackerReport), "Id");
            jsonResolver.Ignore(typeof(SpotTrackerReport.Detail), "Id");
            jsonResolver.Ignore(typeof(SpotTrackerReport.Detail), "ProposalBuyFile");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            var spotTrackerReportDataJson = IntegrationTestHelper.ConvertToJson(spotTrackerReportData, jsonSettings);
            Approvals.Verify(spotTrackerReportDataJson);
        }
    }
}
