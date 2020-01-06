using EntityFrameworkMapping.Broadcast;
using NUnit.Framework;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using Services.Broadcast.Entities;
using Tam.Maestro.Common.DataLayer;
using System;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class BvsRepositoryIntegrationTests
    {
        private readonly IDetectionRepository BvsRepository;

        public BvsRepositoryIntegrationTests()
        {
            BvsRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IDetectionRepository>();
        }

        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_Station()
        {
            var existingDetail = BvsRepository.GetDetectionFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = _DuplicateBvsFileDetail(existingDetail);
            newDetail.Station += "DIFFERENT";

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates = BvsRepository.FilterOutExistingDetails(new List<DetectionFileDetail> { existingDetail, newDetail });

                Assert.True(nonDuplicates.Ignored.Single().Equals(existingDetail));
                Assert.True(nonDuplicates.New.Single().Equals(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }
        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_DateAired()
        {
            var existingDetail = BvsRepository.GetDetectionFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = _DuplicateBvsFileDetail(existingDetail);
            newDetail.DateAired = existingDetail.DateAired.AddSeconds(5);

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates = BvsRepository.FilterOutExistingDetails(new List<DetectionFileDetail> { existingDetail, newDetail });

                Assert.True(nonDuplicates.Ignored.Single().Equals(existingDetail));
                Assert.True(nonDuplicates.New.Single().Equals(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }
        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_ISCI()
        {
            var existingDetail = BvsRepository.GetDetectionFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = _DuplicateBvsFileDetail(existingDetail);
            newDetail.Isci += "12";

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates = BvsRepository.FilterOutExistingDetails(new List<DetectionFileDetail> { existingDetail, newDetail });

                Assert.True(nonDuplicates.Ignored.Single().Equals(existingDetail));
                Assert.True(nonDuplicates.New.Single().Equals(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }
        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_SpotLength()
        {
            var existingDetail = BvsRepository.GetDetectionFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = _DuplicateBvsFileDetail(existingDetail);
            newDetail.SpotLengthId = 0;

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates = BvsRepository.FilterOutExistingDetails(new List<DetectionFileDetail> { existingDetail, newDetail });

                Assert.True(nonDuplicates.Ignored.Single().Equals(existingDetail));
                Assert.True(nonDuplicates.New.Single().Equals(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }
        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_Estimate()
        {
            var existingDetail = BvsRepository.GetDetectionFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = _DuplicateBvsFileDetail(existingDetail);
            newDetail.EstimateId += 1;

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates = BvsRepository.FilterOutExistingDetails(new List<DetectionFileDetail> { existingDetail, newDetail });

                Assert.True(nonDuplicates.Ignored.Single().Equals(existingDetail));
                Assert.True(nonDuplicates.New.Single().Equals(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }

        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_Advertiser()
        {
            var existingDetail = BvsRepository.GetDetectionFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = _DuplicateBvsFileDetail(existingDetail);
            newDetail.Advertiser += "NOTMATCH";

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates = BvsRepository.FilterOutExistingDetails(new List<DetectionFileDetail> { existingDetail, newDetail });
                
                Assert.True(nonDuplicates.Ignored.Single().Equals(existingDetail));
                Assert.True(nonDuplicates.New.Single().Equals(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }

        [Test]
        public void FilterOutExistingDetails_Returns_Nothing_When_All_Exist()
        {
            var existingDetail = BvsRepository.GetDetectionFileDetailsByIds(new List<int> { 10525 }).Single();
            var duplicateDetail = _DuplicateBvsFileDetail(existingDetail);

            using (new TransactionScopeWrapper())
            {
                var input = new List<DetectionFileDetail> { existingDetail, duplicateDetail };
                var result = BvsRepository.FilterOutExistingDetails(input);
                
                Assert.That(result.New, Is.Empty);
                Assert.That(result.Updated, Is.Empty);
            }
        }

        [Test]
        public void FilterOutExistingDetails_Updates_ProgramName_When_ProgramName_Different()
        {
            var existingDetail = BvsRepository.GetDetectionFileDetailsByIds(new List<int> { 10525 }).Single();
            var duplicateNewName = _DuplicateBvsFileDetail(existingDetail);
            duplicateNewName.ProgramName += "test";
            
            using (new TransactionScopeWrapper())
            {
                var result = BvsRepository.FilterOutExistingDetails(new List<DetectionFileDetail> { existingDetail, duplicateNewName });

                Assert.True(result.Ignored.Single().Equals(existingDetail));
                Assert.That(result.New, Is.Empty);
                Assert.True(result.Updated.Single().Equals(duplicateNewName));

                var detail = BvsRepository.GetDetectionFileDetailsByIds(new List<int> { 10525 }).Single();

                Assert.AreEqual(duplicateNewName.ProgramName, detail.ProgramName);
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBvsFileSummaries()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(DetectionFileSummary), "Id");

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var bvsFileSummaries = IntegrationTestHelper.ConvertToJson(BvsRepository.GetDetectionFileSummaries(), jsonSettings);
            Approvals.Verify(bvsFileSummaries);
        }

        private DetectionFileDetail _DuplicateBvsFileDetail(DetectionFileDetail existingDetail)
        {
            return new DetectionFileDetail
            {
                Station = existingDetail.Station,
                DateAired = existingDetail.DateAired,
                Isci = existingDetail.Isci,
                SpotLengthId = existingDetail.SpotLengthId,
                EstimateId = existingDetail.EstimateId,
                Advertiser = existingDetail.Advertiser,
                ProgramName = existingDetail.ProgramName
            };
        }
    }
}
