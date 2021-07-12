using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.Repositories
{
    public interface IDetectionRepository : IDataRepository
    {
        int GetDetectionFileIdByHash(string hash);
        int SaveDetectionFile(TrackerFile<DetectionFileDetail> file);
        List<DetectionPostDetail> GetDetectionPostDetailsByEstimateId(int estimateId);
        List<DetectionPostDetailAudience> GetDetectionPostDetailAudienceByEstimateId(int estimateId);
        List<DetectionTrackingDetail> GetDetectionTrackingDetailsByEstimateId(int estimateId);
        List<DetectionTrackingDetail> GetDetectionTrackingDetailsByDetailIds(List<int> detectionDetailIds);
        DetectionTrackingDetail GetDetectionTrackingDetailById(int detectionDetailId);
        List<DetectionFileDetail> GetDetectionFileDetailsByIds(List<int> detectionFileIds);
        void PersistDetectionDetails(List<DetectionTrackingDetail> detectionItems);
        void ClearTrackingDetailsByEstimateId(int scheduleEstimateId);
        List<int> GetEstimateIdsWithSchedulesByFileIds(List<int> fileIds);

        List<int> GetEstimateIdsByIscis(List<string> iscis);
        FileDetailFilterResult<DetectionFileDetail> FilterOutExistingDetails(List<DetectionFileDetail> newDetails);
        List<DetectionFileSummary> GetDetectionFileSummaries();
        void DeleteById(int detectionFileId);
    }

    public class DetectionRepository : BroadcastRepositoryBase, IDetectionRepository
    {
        public DetectionRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper)
        {
        }

        public int GetDetectionFileIdByHash(string hash)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from x in context.detection_files
                            where x.file_hash == hash
                            select x.id).FirstOrDefault();
                });
        }

        public int SaveDetectionFile(TrackerFile<DetectionFileDetail> file)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    context.detection_files.Add(_MapToDetectionFile(file));
                    context.SaveChanges();
                });
            return file.Id;
        }

        private detection_files _MapToDetectionFile(TrackerFile<DetectionFileDetail> file)
        {
            var result =  new detection_files
            {
                detection_file_details = _MapToDetectionFileDetail(file.FileDetails),
                created_by = file.CreatedBy,
                created_date = file.CreatedDate,
                end_date = file.EndDate,
                file_hash = file.FileHash,
                id = file.Id,
                name = file.Name,
                start_date = file.StartDate
            };
            return result;
        }

        public List<DetectionPostDetail> GetDetectionPostDetailsByEstimateId(int estimateId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from x in context.detection_file_details
                            where x.estimate_id == estimateId
                            select new DetectionPostDetail
                            {
                                DetectionDetailId = x.id,
                                NsiDate = x.nsi_date,
                                TimeAired = x.time_aired,
                                Station = x.station
                            }).ToList();
                });
        }

        public List<DetectionPostDetailAudience> GetDetectionPostDetailAudienceByEstimateId(int estimateId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var pds = (from fd in context.detection_file_details
                               join pd in context.detection_post_details on fd.id equals pd.detection_file_detail_id
                               where fd.estimate_id == estimateId
                               select pd);
                    var pda = new List<DetectionPostDetailAudience>();
                    foreach (var pd in pds)
                        pda.Add(new DetectionPostDetailAudience(pd.detection_file_detail_id, pd.audience_rank ?? 1, pd.audience_id, pd.delivery));

                    return pda;
                });
        }
        public List<DetectionTrackingDetail> GetDetectionTrackingDetailsByEstimateId(int estimateId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from bfd in context.detection_file_details
                            where bfd.estimate_id == estimateId
                            orderby bfd.id
                            select new DetectionTrackingDetail
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
                                Impressions = (from x in context.detection_post_details
                                               where x.detection_file_detail_id == bfd.id
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

        public List<DetectionTrackingDetail> GetDetectionTrackingDetailsByDetailIds(List<int> detectionDetailIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from bfd in context.detection_file_details
                            where detectionDetailIds.Contains(bfd.id)
                            select new DetectionTrackingDetail
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
                                Impressions = (from x in context.detection_post_details
                                               where x.detection_file_detail_id == bfd.id
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

        public DetectionTrackingDetail GetDetectionTrackingDetailById(int detectionFileDetailId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from bfd in context.detection_file_details
                            where bfd.id == detectionFileDetailId
                            select new DetectionTrackingDetail
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
                                Impressions = (from x in context.detection_post_details
                                               where x.detection_file_detail_id == bfd.id
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

        public List<DetectionFileDetail> GetDetectionFileDetailsByIds(List<int> detectionFileIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var bvsFileDetails = context.detection_file_details.Where(f => detectionFileIds.Contains(f.id)).ToList();
                return bvsFileDetails.Select(x => _MapFromDetectionFileDetail(x)).ToList();
            });
        }

        public void PersistDetectionDetails(List<DetectionTrackingDetail> detectionItems)
        {
            var dict = detectionItems.ToDictionary(i => i.Id);
            var ids = dict.Keys.ToList();
            _InReadUncommitedTransaction(
                context =>
                {
                    var applicableObs = context.detection_file_details.Where(d => ids.Contains(d.id));
                    foreach (var detectionFileDetail in applicableObs)
                    {
                        var detectionFileDetails = dict[detectionFileDetail.id];
                        detectionFileDetail.match_airtime = detectionFileDetails.MatchAirtime;
                        detectionFileDetail.match_isci = detectionFileDetails.MatchIsci;
                        detectionFileDetail.match_program = detectionFileDetails.MatchProgram;
                        detectionFileDetail.match_station = detectionFileDetails.MatchStation;
                        detectionFileDetail.match_spot_length = detectionFileDetails.MatchSpotLength;
                        detectionFileDetail.status = (int)detectionFileDetails.Status;
                        detectionFileDetail.schedule_detail_week_id = detectionFileDetails.ScheduleDetailWeekId;
                        detectionFileDetail.has_lead_in_schedule_matches = detectionFileDetails.HasLeadInScheduleMatches;
                        detectionFileDetail.linked_to_leadin = detectionFileDetails.LinkedToLeadin;
                        detectionFileDetail.linked_to_block = detectionFileDetails.LinkedToBlock;
                    }

                    context.SaveChanges();
                });
        }

        public void ClearTrackingDetailsByEstimateId(int estimateId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var detectionTrackingDetails = context.detection_file_details.Where(bfd => bfd.estimate_id == estimateId);
                    foreach (var detectionFileDetail in detectionTrackingDetails)
                    {
                        detectionFileDetail.ClearStatus();
                        detectionFileDetail.schedule_detail_week_id = null;
                        detectionFileDetail.linked_to_block = false;
                        detectionFileDetail.linked_to_leadin = false;
                    }

                    context.SaveChanges();
                });
        }

        public List<int> GetEstimateIdsWithSchedulesByFileIds(List<int> fileIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from x in context.detection_file_details
                            join s in context.schedules on x.estimate_id equals s.estimate_id
                            where fileIds.Contains(x.detection_file_id)
                            select x.estimate_id).Distinct().ToList();
                });
        }

        public List<int> GetEstimateIdsByIscis(List<string> iscis)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from bvs in context.detection_file_details
                            join si in context.schedule_iscis on bvs.isci equals si.house_isci
                            where iscis.Contains(si.house_isci)
                            select bvs.estimate_id).Distinct().ToList();
                });
        }

        public FileDetailFilterResult<DetectionFileDetail> FilterOutExistingDetails(List<DetectionFileDetail> newDetails)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var iscis = newDetails.Select(bfd => bfd.Isci).ToList();

                var relevantFileDetails = context.detection_file_details.Where(fd => iscis.Contains(fd.isci)).ToList();
                var groupJoin = from newDetail in _MapToDetectionFileDetail(newDetails)
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

                var result = new FileDetailFilterResult<DetectionFileDetail>();
                foreach (var pair in groupJoin)
                {
                    foreach (var existingDetail in pair.existingDetail.DefaultIfEmpty())
                    {
                        if (existingDetail == null)
                        {
                            result.New.Add(_MapFromDetectionFileDetail(pair.newDetail));
                        }
                        else if (existingDetail.program_name == pair.newDetail.program_name)
                        {
                            result.Ignored.Add(_MapFromDetectionFileDetail(pair.newDetail));
                        }
                        else if (existingDetail.program_name != pair.newDetail.program_name)
                        {
                            existingDetail.program_name = pair.newDetail.program_name;
                            result.Updated.Add(_MapFromDetectionFileDetail(pair.newDetail));
                        }
                    }
                }

                context.SaveChanges();

                return result;
            });
        }

        private List<detection_file_details> _MapToDetectionFileDetail(List<DetectionFileDetail> newDetails)
        {
            return newDetails.Select(x => new detection_file_details
            {
                advertiser = x.Advertiser,
                affiliate = x.Affiliate,
                date_aired = x.DateAired,
                estimate_id = x.EstimateId,
                id = x.Id,
                isci = x.Isci,
                linked_to_block = x.LinkedToBlock,
                has_lead_in_schedule_matches = x.HasLeadInScheduleMatches,
                market = x.Market,
                linked_to_leadin = x.LinkedToLeadin,
                match_airtime = x.MatchAirtime,
                match_isci = x.MatchIsci,
                match_program = x.MatchProgram,
                match_spot_length = x.MatchSpotLength,
                match_station = x.MatchStation,
                nsi_date = x.NsiDate,
                nti_date = x.NtiDate,
                program_name = x.ProgramName,
                rank = x.Rank,
                schedule_detail_week_id = x.ScheduleDetailWeekId,
                spot_length = x.SpotLength,
                spot_length_id = x.SpotLengthId,
                station = x.Station,
                status = x.Status,
                time_aired = x.TimeAired
            }).ToList();
        }

        private DetectionFileDetail _MapFromDetectionFileDetail(detection_file_details detail)
        {
            return new DetectionFileDetail
            {
                Advertiser = detail.advertiser,
                Affiliate = detail.affiliate,
                DateAired = detail.date_aired,
                EstimateId = detail.estimate_id,
                Id = detail.id,
                Isci = detail.isci,
                LinkedToBlock = detail.linked_to_block,
                HasLeadInScheduleMatches = detail.has_lead_in_schedule_matches,
                DetectionPostDetails = detail.detection_post_details.Select(y => new DetectionPostDetail
                {
                    AudienceId = y.audience_id,
                    AudienceRank = y.audience_rank,
                    DetectionDetailId = y.detection_file_detail_id,
                    Delivery = y.delivery
                }).ToList(),
                Market = detail.market,
                LinkedToLeadin = detail.linked_to_leadin,
                MatchAirtime = detail.match_airtime,
                MatchIsci = detail.match_isci,
                MatchProgram = detail.match_program,
                MatchSpotLength = detail.match_spot_length,
                MatchStation = detail.match_station,
                NsiDate = detail.nsi_date,
                NtiDate = detail.nti_date,
                ProgramName = detail.program_name,
                Rank = detail.rank,
                ScheduleDetailWeekId = detail.schedule_detail_week_id,
                SpotLength = detail.spot_length,
                SpotLengthId = detail.spot_length_id,
                Station = detail.station,
                Status = detail.status,
                TimeAired = detail.time_aired
            };
        }

        public List<DetectionFileSummary> GetDetectionFileSummaries()
        {
            return _InReadUncommitedTransaction(c => (from bvs in c.detection_files
                                                      join bfd in c.detection_file_details on bvs.id equals bfd.detection_file_id
                                                      group bfd.id by bvs into gb
                                                      select new DetectionFileSummary
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
                    var bvsFile = context.detection_files.Single(x => x.id == bvsFileId);
                    context.detection_files.Remove(bvsFile);
                    context.SaveChanges();
                });
        }
    }
}
