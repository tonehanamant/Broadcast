﻿using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IBvsRepository : IDataRepository
    {
        int GetBvsFileIdByHash(string hash);
        int SaveBvsFile(bvs_files file);
        List<BvsPostDetail> GetBvsPostDetailsByEstimateId(int estimateId);
        List<BvsPostDetailAudience> GetBvsPostDetailAudienceByEstimateId(int estimateId);
        List<BvsTrackingDetail> GetBvsTrackingDetailsByEstimateId(int estimateId);
        List<BvsTrackingDetail> GetBvsTrackingDetailsByDetailIds(List<int> bvsDetailIds);
        BvsTrackingDetail GetBvsTrackingDetailById(int bvsDetailId);
        List<bvs_file_details> GetBvsFileDetailsByIds(List<int> bvsFileIds);
        void PersistBvsDetails(List<BvsTrackingDetail> bvsItems);
        void ClearTrackingDetailsByEstimateId(int scheduleEstimateId);
        List<int> GetEstimateIdsWithSchedulesByFileIds(List<int> fileIds);

        List<int> GetEstimateIdsByIscis(List<string> iscis);
        BvsFileDetailFilterResult FilterOutExistingDetails(List<bvs_file_details> newDetails);
        List<BvsFileSummary> GetBvsFileSummaries();
        void DeleteById(int bvsFileId);
    }

    public class BvsRepository : BroadcastRepositoryBase, IBvsRepository
    {
        public BvsRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public int GetBvsFileIdByHash(string hash)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from x in context.bvs_files
                            where x.file_hash == hash
                            select x.id).FirstOrDefault();
                });
        }

        public int SaveBvsFile(bvs_files file)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    context.bvs_files.Add(file);
                    context.SaveChanges();
                });
            return file.id;
        }

        public List<BvsPostDetail> GetBvsPostDetailsByEstimateId(int estimateId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from x in context.bvs_file_details
                            where x.estimate_id == estimateId
                            select new BvsPostDetail
                            {
                                BvsDetailId = x.id,
                                NsiDate = x.nsi_date,
                                TimeAired = x.time_aired,
                                Station = x.station
                            }).ToList();
                });
        }

        public List<BvsPostDetailAudience> GetBvsPostDetailAudienceByEstimateId(int estimateId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var pds = (from fd in context.bvs_file_details
                               join pd in context.bvs_post_details on fd.id equals pd.bvs_file_detail_id
                               where fd.estimate_id == estimateId
                               select pd);
                    var pda = new List<BvsPostDetailAudience>();
                    foreach (var pd in pds)
                        pda.Add(new BvsPostDetailAudience(pd.bvs_file_detail_id, pd.audience_rank ?? 1, pd.audience_id, pd.delivery));

                    return pda;
                });
        }
        public List<BvsTrackingDetail> GetBvsTrackingDetailsByEstimateId(int estimateId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from bfd in context.bvs_file_details
                            where bfd.estimate_id == estimateId
                            select new BvsTrackingDetail
                            {
                                NsiDate = bfd.nsi_date
                                ,
                                AirTime = bfd.time_aired
                                ,
                                Id = bfd.id
                                ,
                                Isci = bfd.isci
                                ,
                                Market = bfd.market
                                ,
                                Program = bfd.program_name
                                ,
                                Affiliate = bfd.affiliate
                                ,
                                Station = bfd.station
                                ,
                                SpotLength = bfd.spot_length
                                ,
                                SpotLengthId = bfd.spot_length_id
                                ,
                                Impressions = (from x in context.bvs_post_details
                                               where x.bvs_file_detail_id == bfd.id
                                                     && x.audience_rank == 1
                                               select x.delivery).Sum()
                                ,
                                Cost = (from sdw in context.schedule_detail_weeks
                                        join sd in context.schedule_details on sdw.schedule_detail_id equals sd.id
                                        where sdw.id == bfd.schedule_detail_week_id
                                        select (decimal?)sd.spot_cost).FirstOrDefault()
                                ,
                                Status = (TrackingStatus)bfd.status
                                ,
                                ScheduleDetailWeekId = bfd.schedule_detail_week_id
                                ,
                                MatchStation = bfd.match_station
                                ,
                                MatchProgram = bfd.match_program
                                ,
                                MatchAirtime = bfd.match_airtime
                                ,
                                MatchIsci = bfd.match_isci
                                ,
                                MatchSpotLength = bfd.match_spot_length
                                ,
                                EstimateId = bfd.estimate_id
                                ,
                                HasLeadInScheduleMatches = bfd.has_lead_in_schedule_matches
                                ,
                                LinkedToLeadin = bfd.linked_to_leadin
                                ,
                                LinkedToBlock = bfd.linked_to_block
                            }).ToList();
                });
        }

        public List<BvsTrackingDetail> GetBvsTrackingDetailsByDetailIds(List<int> bvsDetailIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from bfd in context.bvs_file_details
                            where bvsDetailIds.Contains(bfd.id)
                            select new BvsTrackingDetail
                            {
                                NsiDate = bfd.nsi_date
                                ,
                                AirTime = bfd.time_aired
                                ,
                                Id = bfd.id
                                ,
                                Isci = bfd.isci
                                ,
                                Market = bfd.market
                                ,
                                Program = bfd.program_name
                                ,
                                Affiliate = bfd.affiliate
                                ,
                                Station = bfd.station
                                ,
                                SpotLength = bfd.spot_length
                                ,
                                SpotLengthId = bfd.spot_length_id
                                ,
                                Impressions = (from x in context.bvs_post_details
                                               where x.bvs_file_detail_id == bfd.id
                                                     && x.audience_rank == 1
                                               select x.delivery).Sum()
                                ,
                                Cost = (from sdw in context.schedule_detail_weeks
                                        join sd in context.schedule_details on sdw.schedule_detail_id equals sd.id
                                        select (decimal?)sd.spot_cost).FirstOrDefault()
                                ,
                                Status = (TrackingStatus)bfd.status
                                ,
                                ScheduleDetailWeekId = bfd.schedule_detail_week_id
                                ,
                                MatchStation = bfd.match_station
                                ,
                                MatchProgram = bfd.match_program
                                ,
                                MatchAirtime = bfd.match_airtime
                                ,
                                MatchIsci = bfd.match_isci
                                ,
                                MatchSpotLength = bfd.match_spot_length
                                ,
                                EstimateId = bfd.estimate_id
                                ,
                                HasLeadInScheduleMatches = bfd.has_lead_in_schedule_matches
                                ,
                                LinkedToLeadin = bfd.linked_to_leadin
                                ,
                                LinkedToBlock = bfd.linked_to_block
                            }).ToList();
                });
        }

        public BvsTrackingDetail GetBvsTrackingDetailById(int bvsDetailId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from bfd in context.bvs_file_details
                            where bfd.id == bvsDetailId
                            select new BvsTrackingDetail
                            {
                                NsiDate = bfd.nsi_date
                                ,
                                AirTime = bfd.time_aired
                                ,
                                Id = bfd.id
                                ,
                                Isci = bfd.isci
                                ,
                                Market = bfd.market
                                ,
                                Program = bfd.program_name
                                ,
                                Affiliate = bfd.affiliate
                                ,
                                Station = bfd.station
                                ,
                                SpotLength = bfd.spot_length
                                ,
                                SpotLengthId = bfd.spot_length_id
                                ,
                                Impressions = (from x in context.bvs_post_details
                                               where x.bvs_file_detail_id == bfd.id
                                                     && x.audience_rank == 1
                                               select x.delivery).Sum()
                                ,
                                Cost = (from sdw in context.schedule_detail_weeks
                                        join sd in context.schedule_details on sdw.schedule_detail_id equals sd.id
                                        select (decimal?)sd.spot_cost).FirstOrDefault()
                                ,
                                Status = (TrackingStatus)bfd.status
                                ,
                                ScheduleDetailWeekId = bfd.schedule_detail_week_id
                                ,
                                MatchStation = bfd.match_station
                                ,
                                MatchProgram = bfd.match_program
                                ,
                                MatchAirtime = bfd.match_airtime
                                ,
                                MatchIsci = bfd.match_isci
                                ,
                                EstimateId = bfd.estimate_id
                                ,
                                HasLeadInScheduleMatches = bfd.has_lead_in_schedule_matches
                                ,
                                LinkedToLeadin = bfd.linked_to_leadin
                                ,
                                LinkedToBlock = bfd.linked_to_block
                                ,
                                MatchSpotLength = bfd.match_spot_length
                            }).Single();
                });
        }

        public List<bvs_file_details> GetBvsFileDetailsByIds(List<int> bvsFileIds)
        {
            return _InReadUncommitedTransaction(context => context.bvs_file_details.Where(f => bvsFileIds.Contains(f.id)).ToList());
        }

        public void PersistBvsDetails(List<BvsTrackingDetail> bvsItems)
        {
            var dict = bvsItems.ToDictionary(i => i.Id);
            var ids = dict.Keys.ToList();
            _InReadUncommitedTransaction(
                context =>
                {
                    var applicableObs = context.bvs_file_details.Where(d => ids.Contains(d.id));
                    foreach (var bvsFileDetail in applicableObs)
                    {
                        var bvsDetails = dict[bvsFileDetail.id];
                        bvsFileDetail.match_airtime = bvsDetails.MatchAirtime;
                        bvsFileDetail.match_isci = bvsDetails.MatchIsci;
                        bvsFileDetail.match_program = bvsDetails.MatchProgram;
                        bvsFileDetail.match_station = bvsDetails.MatchStation;
                        bvsFileDetail.match_spot_length = bvsDetails.MatchSpotLength;
                        bvsFileDetail.status = (int)bvsDetails.Status;
                        bvsFileDetail.schedule_detail_week_id = bvsDetails.ScheduleDetailWeekId;
                        bvsFileDetail.has_lead_in_schedule_matches = bvsDetails.HasLeadInScheduleMatches;
                        bvsFileDetail.linked_to_leadin = bvsDetails.LinkedToLeadin;
                        bvsFileDetail.linked_to_block = bvsDetails.LinkedToBlock;
                    }

                    context.SaveChanges();
                });
        }

        public void ClearTrackingDetailsByEstimateId(int estimateId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var bvsTrackingDetails = context.bvs_file_details.Where(bfd => bfd.estimate_id == estimateId);
                    foreach (var bvsDetail in bvsTrackingDetails)
                    {
                        bvsDetail.ClearStatus();
                        bvsDetail.schedule_detail_week_id = null;
                        bvsDetail.linked_to_block = false;
                        bvsDetail.linked_to_leadin = false;
                    }

                    context.SaveChanges();
                });
        }

        public List<int> GetEstimateIdsWithSchedulesByFileIds(List<int> fileIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from x in context.bvs_file_details
                            join s in context.schedules on x.estimate_id equals s.estimate_id
                            where fileIds.Contains(x.bvs_file_id)
                            select x.estimate_id).Distinct().ToList();
                });
        }

        public List<int> GetEstimateIdsByIscis(List<string> iscis)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from bvs in context.bvs_file_details
                            join si in context.schedule_iscis on bvs.isci equals si.house_isci
                            where iscis.Contains(si.house_isci)
                            select bvs.estimate_id).Distinct().ToList();
                });
        }

        public BvsFileDetailFilterResult FilterOutExistingDetails(List<bvs_file_details> newDetails)
        {
            return _InReadUncommitedTransaction(c =>
            {
                var iscis = newDetails.Select(bfd => bfd.isci).ToList();

                var relevantFileDetails = c.bvs_file_details.Where(fd => iscis.Contains(fd.isci)).ToList();
                var groupJoin = from newDetail in newDetails
                                join bfd in relevantFileDetails
                                on new
                                {
                                    newDetail.station,
                                    newDetail.date_aired,
                                    newDetail.isci,
                                    newDetail.spot_length_id,
                                    newDetail.estimate_id,
                                    newDetail.advertiser
                                } equals
                                new
                                {
                                    bfd.station,
                                    bfd.date_aired,
                                    bfd.isci,
                                    bfd.spot_length_id,
                                    bfd.estimate_id,
                                    bfd.advertiser
                                } into gb
                                select new
                                {
                                    newDetail,
                                    existingDetail = gb
                                };

                var ret = new BvsFileDetailFilterResult();
                foreach (var pair in groupJoin)
                {
                    foreach (var existingDetail in pair.existingDetail.DefaultIfEmpty())
                    {
                        if (existingDetail == null)
                        {
                            ret.New.Add(pair.newDetail);
                        }
                        else if (existingDetail.program_name == pair.newDetail.program_name)
                        {
                            ret.Ignored.Add(pair.newDetail);
                        }
                        else if (existingDetail.program_name != pair.newDetail.program_name)
                        {
                            existingDetail.program_name = pair.newDetail.program_name;
                            ret.Updated.Add(pair.newDetail);
                        }
                    }
                }

                c.SaveChanges();

                return ret;
            });
        }

        public List<BvsFileSummary> GetBvsFileSummaries()
        {
            return _InReadUncommitedTransaction(c => (from bvs in c.bvs_files
                                                      join bfd in c.bvs_file_details on bvs.id equals bfd.bvs_file_id
                                                      group bfd.id by bvs into gb
                                                      select new BvsFileSummary
                                                      {
                                                          Id = gb.Key.id,
                                                          FileName = gb.Key.name,
                                                          StartDate = gb.Key.start_date,
                                                          EndDate = gb.Key.end_date,
                                                          UploadDate = gb.Key.created_date,
                                                          RecordCount = gb.Count()
                                                      }).ToList());
        }

        public void DeleteById(int bvsFileId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var bvsFile = context.bvs_files.Single(x => x.id == bvsFileId);
                    context.bvs_files.Remove(bvsFile);
                    context.SaveChanges();
                });
        }
    }
}
