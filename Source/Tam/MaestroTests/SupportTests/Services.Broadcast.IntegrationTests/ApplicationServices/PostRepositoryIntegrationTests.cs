using EntityFrameworkMapping.Broadcast;
using NUnit.Framework;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PostRepositoryIntegrationTests
    {
        [Test]
        public void Delete_Works()
        {
            using (new TransactionScopeWrapper())
            {
                var postUploadRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();

                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.Live;
                files.posting_book_id = 410;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                var detail = PostEngineIntegrationTests.GetParsedDetail();
                files.post_file_details.Add(detail);

                files.post_file_details.First().post_file_detail_impressions.Add(new post_file_detail_impressions { demo = 33, impression = 123.123 });

                var fileId = postUploadRepository.SavePost(files);

                postUploadRepository.DeletePost(fileId);

                try
                {
                    postUploadRepository.GetPost(fileId);
                    Assert.Fail();
                }
                catch (Exception e)
                {
                    Assert.That(e.Message, Contains.Substring("Sequence contains no elements"));
                }
            }
        }

        [Test]
        public void DeleteImpressions_Only_Deletes_Impressions()
        {
            using (new TransactionScopeWrapper())
            {
                var postUploadRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();

                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.Live;
                files.posting_book_id = 410;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                var detail = PostEngineIntegrationTests.GetParsedDetail();
                files.post_file_details.Add(detail);

                files.post_file_details.First().post_file_detail_impressions.Add(new post_file_detail_impressions { demo = 33, impression = 123.123 });

                var fileId = postUploadRepository.SavePost(files);

                postUploadRepository.DeletePostImpressions(fileId);

                var upload = postUploadRepository.GetPost(fileId);

                postUploadRepository.DeletePost(fileId);

                Assert.That(upload.FileDetails.SelectMany(fd => fd.Impressions), Is.Empty);
            }
        }

        [Test]
        public void Save_DoesNotGenerate_Duplicate()
        {
            using (new TransactionScopeWrapper())
            {
                var postUploadRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();

                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.Live;
                files.posting_book_id = 410;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                var detail = PostEngineIntegrationTests.GetParsedDetail();
                files.post_file_details.Add(detail);

                files.post_file_details.First().post_file_detail_impressions.Add(new post_file_detail_impressions { demo = 33, impression = 123.123 });

                var fileId = postUploadRepository.SavePost(files);

                var file = postUploadRepository.GetPostEF(fileId);

                var fileId2 = postUploadRepository.SavePost(file);

                postUploadRepository.DeletePost(fileId);
                try
                {
                    postUploadRepository.DeletePost(fileId2);
                }
                catch (Exception)
                {
                    Assert.That(fileId, Is.EqualTo(fileId2));
                }
                Assert.That(fileId, Is.EqualTo(fileId2));
            }
        }

        [Test]
        public void Save_Persists_Info()
        {
            using (new TransactionScopeWrapper())
            {
                var postUploadRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IPostRepository>();

                var files = new post_files();
                files.equivalized = false;
                files.file_name = "abctest";
                files.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 33 }, new post_file_demos { demo = 34 } };
                files.playback_type = (byte)ProposalEnums.ProposalPlaybackType.Live;
                files.posting_book_id = 410;
                files.modified_date = DateTime.Now;
                files.upload_date = DateTime.Now;

                var detail = PostEngineIntegrationTests.GetParsedDetail();
                files.post_file_details.Add(detail);

                files.post_file_details.First().post_file_detail_impressions.Add(new post_file_detail_impressions { demo = 33, impression = 123.123 });

                var fileId = postUploadRepository.SavePost(files);

                var file = postUploadRepository.GetPostEF(fileId);
                file.equivalized = true;
                file.posting_book_id = 123;
                file.post_file_demos = new List<post_file_demos> { new post_file_demos { demo = 1 } };
                file.playback_type = (byte)ProposalEnums.ProposalPlaybackType.Live;
                file.modified_date = DateTime.Today;

                var fileId2 = postUploadRepository.SavePost(file);

                var modifiedFile = postUploadRepository.GetPostEF(fileId2);

                postUploadRepository.DeletePost(fileId);
                try
                {
                    postUploadRepository.DeletePost(fileId2);
                }
                catch (Exception) { }

                Assert.That(modifiedFile.equivalized, Is.EqualTo(file.equivalized));
                Assert.That(modifiedFile.posting_book_id, Is.EqualTo(file.posting_book_id));
                Assert.That(modifiedFile.post_file_demos.Count, Is.EqualTo(file.post_file_demos.Count));
                Assert.That(modifiedFile.post_file_demos.Select(d => d.demo), Is.EquivalentTo(file.post_file_demos.Select(d => d.demo)));
                Assert.That(modifiedFile.playback_type, Is.EqualTo(file.playback_type));
                Assert.That(modifiedFile.modified_date, Is.EqualTo(file.modified_date));
            }
        }
    }
}
