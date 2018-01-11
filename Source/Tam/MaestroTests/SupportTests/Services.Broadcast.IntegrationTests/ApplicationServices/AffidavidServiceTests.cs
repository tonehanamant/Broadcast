using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
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

            var affidavitService =
                new AffidavitService(
                    IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory,
                    new BroadcastAudiencesCache(
                        IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory),
                    mockPostingBookService.Object);
            return affidavitService;
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
                    Isci = "ISCI",
                    ProgramName = "Programs R Us",
                    SpotLength = 30,
                    Station = "WNBC"
                };

                affidavitSaveRequest.Details.Add(affidavitSaveRequestDetail);

                var affidavitId = affidavitService.SaveAffidavit(affidavitSaveRequest);

                var affidavitFile = _AffidavitRepository.GetAffidavit(affidavitId);

                var jsonResolver = new IgnorableSerializerContractResolver();

                jsonResolver.Ignore(typeof(AffidavitFile), "CreatedDate");
                jsonResolver.Ignore(typeof(AffidavitFile), "Id");
                jsonResolver.Ignore(typeof(AffidavitFileDetail), "Id");
                jsonResolver.Ignore(typeof(AffidavitFileDetail), "AffidavitFileId");
                jsonResolver.Ignore(typeof(AffidavitFileDetail), "AffidavitFileDetailId");
                jsonResolver.Ignore(typeof(AffidavitFileDetailAudience), "AffidavitFileDetailId");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                var json = IntegrationTestHelper.ConvertToJson(affidavitFile, jsonSettings);

                Approvals.Verify(json);
            }
        }
    }
}

