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

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class BvsRepositoryIntegrationTests
    {
        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_Station()
        {
            var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var existingDetail = sut.GetBvsFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = new bvs_file_details { station = existingDetail.station + "DIFFERENT", date_aired = existingDetail.date_aired, isci = existingDetail.isci, spot_length_id = existingDetail.spot_length_id, estimate_id = existingDetail.estimate_id, advertiser = existingDetail.advertiser };

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates = sut.FilterOutExistingDetails(new List<bvs_file_details> { existingDetail, newDetail });

                Assert.That(nonDuplicates.Ignored.Single(), Is.EqualTo(existingDetail));
                Assert.That(nonDuplicates.New.Single(), Is.EqualTo(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }
        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_DateAired()
        {
            var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var existingDetail = sut.GetBvsFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = new bvs_file_details { station = existingDetail.station, date_aired = existingDetail.date_aired.AddSeconds(5), isci = existingDetail.isci, spot_length_id = existingDetail.spot_length_id, estimate_id = existingDetail.estimate_id, advertiser = existingDetail.advertiser };

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates = sut.FilterOutExistingDetails(new List<bvs_file_details> { existingDetail, newDetail });

                Assert.That(nonDuplicates.Ignored.Single(), Is.EqualTo(existingDetail));
                Assert.That(nonDuplicates.New.Single(), Is.EqualTo(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }
        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_ISCI()
        {
            var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var existingDetail = sut.GetBvsFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = new bvs_file_details { station = existingDetail.station, date_aired = existingDetail.date_aired, isci = existingDetail.isci + "12", spot_length_id = existingDetail.spot_length_id, estimate_id = existingDetail.estimate_id, advertiser = existingDetail.advertiser };

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates =
                    sut.FilterOutExistingDetails(new List<bvs_file_details> { existingDetail, newDetail });

                Assert.That(nonDuplicates.Ignored.Single(), Is.EqualTo(existingDetail));
                Assert.That(nonDuplicates.New.Single(), Is.EqualTo(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }
        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_SpotLength()
        {
            var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var existingDetail = sut.GetBvsFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = new bvs_file_details { station = existingDetail.station, date_aired = existingDetail.date_aired, isci = existingDetail.isci, spot_length_id = 0, estimate_id = existingDetail.estimate_id, advertiser = existingDetail.advertiser };

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates =
                    sut.FilterOutExistingDetails(new List<bvs_file_details> { existingDetail, newDetail });

                Assert.That(nonDuplicates.Ignored.Single(), Is.EqualTo(existingDetail));
                Assert.That(nonDuplicates.New.Single(), Is.EqualTo(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }
        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_Estimate()
        {
            var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var existingDetail = sut.GetBvsFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = new bvs_file_details { station = existingDetail.station, date_aired = existingDetail.date_aired, isci = existingDetail.isci, spot_length_id = existingDetail.spot_length_id, estimate_id = existingDetail.estimate_id + 1, advertiser = existingDetail.advertiser };

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates =
                    sut.FilterOutExistingDetails(new List<bvs_file_details> { existingDetail, newDetail });

                Assert.That(nonDuplicates.Ignored.Single(), Is.EqualTo(existingDetail));
                Assert.That(nonDuplicates.New.Single(), Is.EqualTo(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }

        [Test]
        public void FilterOutExistingDetails_Filters_Out_Different_Advertiser()
        {
            var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var existingDetail = sut.GetBvsFileDetailsByIds(new List<int> { 10525 }).Single();
            var newDetail = new bvs_file_details { station = existingDetail.station, date_aired = existingDetail.date_aired, isci = existingDetail.isci, spot_length_id = existingDetail.spot_length_id, estimate_id = existingDetail.estimate_id, advertiser = existingDetail.advertiser + "NOTMATCH" };

            using (new TransactionScopeWrapper())
            {
                var nonDuplicates = sut.FilterOutExistingDetails(new List<bvs_file_details> { existingDetail, newDetail });

                Assert.That(nonDuplicates.Ignored.Single(), Is.EqualTo(existingDetail));
                Assert.That(nonDuplicates.New.Single(), Is.EqualTo(newDetail));
                Assert.That(nonDuplicates.Updated, Is.Empty);
            }
        }

        [Test]
        public void FilterOutExistingDetails_Returns_Nothing_When_All_Exist()
        {
            var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var existingDetail = sut.GetBvsFileDetailsByIds(new List<int> { 10525 }).Single();
            var duplicateDetail = new bvs_file_details { station = existingDetail.station, date_aired = existingDetail.date_aired, isci = existingDetail.isci, spot_length_id = existingDetail.spot_length_id, estimate_id = existingDetail.estimate_id, advertiser = existingDetail.advertiser, program_name = existingDetail.program_name };

            using (new TransactionScopeWrapper())
            {
                var input = new List<bvs_file_details> { existingDetail, duplicateDetail };
                var result = sut.FilterOutExistingDetails(input);

                CollectionAssert.AreEquivalent(result.Ignored, input);
                Assert.That(result.New, Is.Empty);
                Assert.That(result.Updated, Is.Empty);
            }
        }

        [Test]
        public void FilterOutExistingDetails_Updates_ProgramName_When_ProgramName_Different()
        {
            var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();
            var existingDetail = sut.GetBvsFileDetailsByIds(new List<int> { 10525 }).Single();
            var duplicateNewName = new bvs_file_details { station = existingDetail.station, date_aired = existingDetail.date_aired, isci = existingDetail.isci, spot_length_id = existingDetail.spot_length_id, estimate_id = existingDetail.estimate_id, advertiser = existingDetail.advertiser, program_name = existingDetail.program_name + "test" };

            using (new TransactionScopeWrapper())
            {
                var result = sut.FilterOutExistingDetails(new List<bvs_file_details> { existingDetail, duplicateNewName });

                Assert.That(result.Ignored.Single(), Is.EqualTo(existingDetail));
                Assert.That(result.New, Is.Empty);
                Assert.That(result.Updated.Single(), Is.EqualTo(duplicateNewName));

                var detail = sut.GetBvsFileDetailsByIds(new List<int> { 10525 }).Single();

                Assert.AreEqual(duplicateNewName.program_name, detail.program_name);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetBvsFileSummaries()
        {
            var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IBvsRepository>();

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(BvsFileSummary), "Id");

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var bvsFileSummaries = IntegrationTestHelper.ConvertToJson(sut.GetBvsFileSummaries(), jsonSettings);
            Approvals.Verify(bvsFileSummaries);
        }
    }
}
