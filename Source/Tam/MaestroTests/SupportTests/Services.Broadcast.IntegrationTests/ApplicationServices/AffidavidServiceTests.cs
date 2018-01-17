using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class AffidavidServiceTests
    {
        private readonly IAffidavitRepository _AffidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IAffidavitRepository>();

        private static AffidavitService _SetupAffidavitService()
        {
            var mockPostingBookService = new Mock<IPostingBooksService>();

            mockPostingBookService.Setup(x => x.GetDefaultPostingBooks()).Returns(new DefaultPostingBooksDto
            {
                DefaultShareBook = new PostingBookResultDto
                {
                    PostingBookId = 416
                }
            });

            var affidavitMatchingEngine =
                IntegrationTestApplicationServiceFactory.GetApplicationService<IAffidavitMatchingEngine>();

            var affidavitService =
                new AffidavitService(
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory,
                    affidavitMatchingEngine,
                    new BroadcastAudiencesCache(
                        IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory),
                    mockPostingBookService.Object);
            return affidavitService;
        }

        private static JsonSerializerSettings _SetupJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(AffidavitFile), "CreatedDate");
            jsonResolver.Ignore(typeof(AffidavitFile), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileDetail), "Id");
            jsonResolver.Ignore(typeof(AffidavitFileDetail), "AffidavitFileId");
            jsonResolver.Ignore(typeof(AffidavitFileDetail), "AffidavitFileDetailId");
            jsonResolver.Ignore(typeof(AffidavitFileDetailAudience), "AffidavitFileDetailId");
            jsonResolver.Ignore(typeof(AffidavitClientScrub), "Id");
            jsonResolver.Ignore(typeof(AffidavitClientScrub), "AffidavitFileDetailId");
            jsonResolver.Ignore(typeof(AffidavitClientScrub), "ModifiedDate");

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
            return jsonSettings;
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteService()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var affidavitService = _SetupAffidavitService();

                var affidavitSaveRequest = new AffidavitSaveRequest
                {
                    FileHash = "abc123",
                    Source = (int) AffidaviteFileSource.Strata,
                    FileName = "test.file"
                };

                var affidavitSaveRequestDetail = new AffidavitSaveRequestDetail
                {
                    AirTime = DateTime.Parse("12/29/2018 10:04AM"),
                    Isci = "AAAAAAAA",
                    ProgramName = "Programs R Us",
                    SpotLength = 30,
                    Station = "WNBC"
                };

                affidavitSaveRequest.Details.Add(affidavitSaveRequestDetail);

                var affidavitId = affidavitService.SaveAffidavit(affidavitSaveRequest, "testuser", DateTime.Now);

                var affidavitFile = _AffidavitRepository.GetAffidavit(affidavitId);

                var jsonSettings = _SetupJsonSettings();

                var json = IntegrationTestHelper.ConvertToJson(affidavitFile, jsonSettings);

                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteServiceInSpec()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var affidavitService = _SetupAffidavitService();

                var affidavitSaveRequest = new AffidavitSaveRequest
                {
                    FileHash = "abc123",
                    Source = (int)AffidaviteFileSource.Strata,
                    FileName = "test.file"
                };

                var affidavitSaveRequestDetail = new AffidavitSaveRequestDetail
                {
                    AirTime = DateTime.Parse("06/01/2016 08:58AM"),
                    Isci = "AAAAAAAA",
                    ProgramName = "Programs R Us",
                    SpotLength = 30,
                    Station = "WNBC"
                };

                affidavitSaveRequest.Details.Add(affidavitSaveRequestDetail);

                var affidavitId = affidavitService.SaveAffidavit(affidavitSaveRequest, "testuser", DateTime.Now);

                var affidavitFile = _AffidavitRepository.GetAffidavit(affidavitId);

                var jsonSettings = _SetupJsonSettings();

                var json = IntegrationTestHelper.ConvertToJson(affidavitFile, jsonSettings);

                Approvals.Verify(json);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SaveAffidaviteServiceThrowsException()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var affidavitService = _SetupAffidavitService();

                var affidavitSaveRequest = new AffidavitSaveRequest
                {
                    FileHash = "abc123",
                    Source = (int)AffidaviteFileSource.Strata,
                    FileName = "test.file"
                };

                var affidavitSaveRequestDetail = new AffidavitSaveRequestDetail
                {
                    AirTime = DateTime.Parse("12/29/2018 10:04AM"),
                    Isci = "ISCI_NOT_FOUND",
                    ProgramName = "Programs R Us",
                    SpotLength = 30,
                    Station = "WNBC"
                };

                affidavitSaveRequest.Details.Add(affidavitSaveRequestDetail);

                try
                {
                    affidavitService.SaveAffidavit(affidavitSaveRequest, "testuser", DateTime.Now);
                    
                    Assert.Fail("Should have thrown an exception due to unmatched affidavit isci.");
                }
                catch (BroadcastAffidavitException ex)
                {
                    var json = IntegrationTestHelper.ConvertToJson(ex.ErrorList);

                    Approvals.Verify(json);
                }                
            }
        }
    }
}

