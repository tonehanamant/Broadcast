using ApprovalTests;
using ApprovalTests.Reporters;
using EntityFrameworkMapping.Broadcast;
using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PostEngineIntegrationTests
    {
        [Ignore]
        [Test]
        public void Post_Works_With_DuplicateDetails()
        {
            using (new TransactionScopeWrapper())
            {
                var postUploadRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.LivePlus7;
                files.posting_book_id = 410;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                files.post_file_details.Add(GetParsedDetail());
                var detail2 = GetParsedDetail();
                detail2.date = detail2.date.AddDays(1);
                files.post_file_details.Add(detail2);

                var fileId = postUploadRepository.SavePost(files);

                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostEngine>();
                sut.Post(files);

                var file = postUploadRepository.GetPost(fileId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(file));
            }
        }

        [Ignore]
        [Test]
        public void Post()
        {
            using (new TransactionScopeWrapper())
            {
                var postUploadRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.LivePlus7;
                files.posting_book_id = 413;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                files.post_file_details.Add(GetParsedDetail());

                var fileId = postUploadRepository.SavePost(files);

                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostEngine>();
                sut.Post(files);

                var file = postUploadRepository.GetPost(fileId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(file));
            }
        }

        [Ignore]
        [Test]
        public void PostWithUnknownStation()
        {
            using (new TransactionScopeWrapper())
            {
                var postUploadRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.LivePlus7;
                files.posting_book_id = 413;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                var firstDetail = GetParsedDetail();
                files.post_file_details.Add(firstDetail);
                var secondDetail = GetParsedDetail();
                secondDetail.station = "NRQE";
                files.post_file_details.Add(secondDetail);

                var fileId = postUploadRepository.SavePost(files);

                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostEngine>();
                sut.Post(files);

                var file = postUploadRepository.GetPost(fileId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(file));
            }
        }

        [Ignore]
        [Test]
        public void Post_Friday()
        {
            using (new TransactionScopeWrapper())
            {
                var postUploadRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.LivePlus7;
                files.posting_book_id = 413;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                var detail = GetParsedDetail();
                detail.date = detail.date.AddDays(1);
                files.post_file_details.Add(detail);

                var fileId = postUploadRepository.SavePost(files);

                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostEngine>();
                sut.Post(files);

                var file = postUploadRepository.GetPost(fileId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(file));
            }
        }

        [Ignore]
        [Test]
        public void Post_Sunday()
        {
            using (new TransactionScopeWrapper())
            {
                var postUploadRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();
                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.LivePlus7;
                files.posting_book_id = 413;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                var detail = GetParsedDetail();
                detail.date = detail.date.AddDays(3);
                files.post_file_details.Add(detail);

                var fileId = postUploadRepository.SavePost(files);

                var sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostEngine>();
                sut.Post(files);

                var file = postUploadRepository.GetPost(fileId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(file));
            }
        }

        internal static post_file_details GetParsedDetail()
        {
            var detail = new post_file_details();
            detail.rank = 93;
            detail.market = "BATON ROUGE";
            detail.station = "WAFB";
            detail.affiliate = "CBS";
            detail.weekstart = DateTime.Parse("2/20/2017");
            detail.day = "THU";
            detail.date = DateTime.Parse("2/23/2017");
            detail.time_aired = 17768;
            detail.program_name = "WAFB 9 NEWS THIS MORNING: EARLY EDIT";
            detail.spot_length = 15;
            detail.spot_length_id = 3;
            detail.house_isci = "";
            detail.client_isci = "NNVA0045000";
            detail.advertiser = "BEIERSDORF";
            detail.inventory_source = "ASSEMBLY";
            detail.inventory_source_daypart = "EMN";
            detail.inventory_out_of_spec_reason = "Inventory In Spec";
            detail.advertiser_out_of_spec_reason = "Advertiser In Spec";
            detail.estimate_id = 7196;
            detail.detected_via = "BVS Cadent";
            detail.spot = 1;
            return detail;
        }
    }
}
