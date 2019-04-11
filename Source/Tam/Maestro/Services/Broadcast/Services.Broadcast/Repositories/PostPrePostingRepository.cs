using System;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IPostPrePostingRepository : IDataRepository
    {
        List<PostPrePostingFile> GetAllPostsList();
        List<PostPrePostingFile> GetAllPosts();
        int SavePost(post_files file);
        PostPrePostingFile GetPost(int id);
        bool DeletePost(int id);
        void SavePostImpressions(List<post_file_detail_impressions> impressions);
        void DeletePostImpressions(int id);
        post_files GetPostEF(int id);
        bool PostExist(string fileName);
    }

    public class PostPrePostingRepository : BroadcastRepositoryBase, IPostPrePostingRepository
    {
        public PostPrePostingRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper) { }

        public List<PostPrePostingFile> GetAllPostsList()
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    var files = c.post_files.ToList();
                    return files.Select(f => f.ConvertShallow()).ToList();
                });
        }


        public List<PostPrePostingFile> GetAllPosts()
        {
            return _InReadUncommitedTransaction(
                c =>
                {
                    return c.post_files.Include(f => f.post_file_details)
                        .Include(f => f.post_file_demos)
                        .Include(f => f.post_file_details.Select(fd => fd.post_file_detail_impressions))
                        .ToList()
                        .Select(x => x.Convert()).ToList();
                });
        }

        public PostPrePostingFile GetPost(int id)
        {
            return _InReadUncommitedTransaction(c =>
            {
                return c.post_files.Include(f => f.post_file_details)
                                    .Include(f => f.post_file_details.Select(fd => fd.post_file_detail_impressions))
                                    .Single(f => f.id == id).Convert();
            });
        }

        public int SavePost(post_files file)
        {
            if (file.id > 0)
            {
                 _InReadUncommitedTransaction(
                context =>
                {
                    var dbPost = context.post_files.Find(file.id);
                    dbPost.equivalized = file.equivalized;
                    dbPost.posting_book_id = file.posting_book_id;

                    var oldPostFileDemos = context.post_file_demos.Where(pfd => pfd.post_file_id == file.id);
                    context.post_file_demos.RemoveRange(oldPostFileDemos);

                    dbPost.post_file_demos = file.post_file_demos
                        .Select(pfd => new post_file_demos {demo = pfd.demo}).ToList();
                    dbPost.playback_type = file.playback_type;
                    dbPost.modified_date = file.modified_date;

                    context.SaveChanges();
                });
                return file.id;
            }
            else
            {
                return DoLockAndRetry(() =>
                {
                    var fileDetails = new List<post_file_details>();
                    _InReadUncommitedTransaction(
                    context =>
                    {
                        context.Configuration.AutoDetectChangesEnabled = false;
                        // PRI-6647: Clear the file details so that the add doesn't try to insert them all at once
                        fileDetails.AddRange(file.post_file_details);
                        file.post_file_details.Clear();
                        // Add file here to get id
                        context.post_files.Add(file);
                        context.SaveChanges();
                        
                        // Set the details' file ids to the returned id
                        fileDetails.ForEach(detail =>
                        {
                            detail.post_file_id = file.id;
                        });
                        SetPostFileDetailIds(context, fileDetails);

                        try
                        {
                            BulkInsert(context, fileDetails);
                        }
                        catch (SqlException ex) when (ex.Number == 547)
                        {
                            // Exception occurs from adding duplicate ids, which means more details were added since the table was read last
                            // Reset the ids of the details we're inserting
                            SetPostFileDetailIds(context, fileDetails);
                            BulkInsert(context, fileDetails);
                        }
                    });
                    file.post_file_details = fileDetails;

                    return file.id;
                });
            }
        }

        public bool DeletePost(int id)
        {
            _InReadUncommitedTransaction(c =>
            {
                var upload = c.post_files.Single(f => f.id == id);

                c.post_files.Remove(upload);

                c.SaveChanges();
            });
            return true;
        }

        public void SavePostImpressions(List<post_file_detail_impressions> impressions)
        {
            _InReadUncommitedTransaction(c =>
            {
                BulkInsert(c,impressions);
            });
        }

        public void DeletePostImpressions(int id)
        {
                _InReadUncommitedTransaction(c =>
                {
                    c.post_file_detail_impressions.RemoveRange(c.post_file_details.Where(d => d.post_file_id == id).SelectMany(d => d.post_file_detail_impressions));
                    c.SaveChanges();
                });
        }

        public post_files GetPostEF(int id)
        {
            return _InReadUncommitedTransaction(c =>
            {
                return c.post_files.Include(f => f.post_file_details)
                    .Include(f => f.post_file_demos)
                    .Include(f => f.post_file_details.Select(fd => fd.post_file_detail_impressions))
                    .Single(f => f.id == id);
            });
        }

        public bool PostExist(string fileName)
        {
            return _InReadUncommitedTransaction(c => { return c.post_files.Any(f => f.file_name == fileName); });
        }
        
        // Manually set detail ids based on the next available ids in the database
        private void SetPostFileDetailIds(QueryHintBroadcastContext context, List<post_file_details> fileDetails)
        {
            int nextSequence = context.post_file_details.Max(detail => detail.id) + 1;
            fileDetails.ForEach(detail =>
            {
                // manually set the id to the next available
                detail.id = nextSequence;
                nextSequence++;
            });
        }
    }

    public static class Extensions
    {
        public static PostPrePostingFile ConvertShallow(this post_files x)
        {
            var postUpload = new PostPrePostingFile(x.id, x.equivalized, x.posting_book_id, (ProposalEnums.ProposalPlaybackType)x.playback_type, x.post_file_demos, x.file_name, x.upload_date, x.modified_date);
            return postUpload;
        }
        public static PostPrePostingFile Convert(this post_files x)
        {
            var postUpload = new PostPrePostingFile(x.id, x.equivalized, x.posting_book_id, (ProposalEnums.ProposalPlaybackType)x.playback_type, x.post_file_demos, x.file_name, x.upload_date, x.modified_date, x.post_file_details.Select(d => d.Convert()).ToList());
            return postUpload;
        }

        public static PostFileDetail Convert(this post_file_details x)
        {
            var postUpload = new PostFileDetail(x.id, x.post_file_id, x.rank, x.market, x.station, x.affiliate, x.weekstart, x.day, x.date, x.time_aired, x.program_name, x.spot_length, x.spot_length_id, x.house_isci, x.client_isci, x.advertiser, x.inventory_source, x.inventory_source_daypart, x.advertiser_out_of_spec_reason, x.inventory_out_of_spec_reason, x.estimate_id, x.detected_via, x.spot, x.post_file_detail_impressions.Select(i => i.Convert()).ToList());
            return postUpload;
        }

        public static PostFileDetailImpression Convert(this post_file_detail_impressions x)
        {
            return new PostFileDetailImpression(x.demo, x.impression, x.post_file_detail_id);
        }
    }
}