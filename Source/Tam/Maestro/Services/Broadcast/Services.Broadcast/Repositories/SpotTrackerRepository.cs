using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface ISpotTrackerRepository : IDataRepository
    {
        int GetSigmaFileIdByHash(string hash);

        /// <summary>
        /// Saves a SpotTracker file object
        /// </summary>
        /// <param name="file">SpotTracker object</param>
        /// <returns>Newly created id</returns>
        int SaveSpotTrackerFile(TrackerFile<SpotTrackerFileDetail> file);

        /// <summary>
        /// Checks existing details comparing with the DB
        /// </summary>
        /// <param name="newDetails">Details to check</param>
        /// <returns>FileDetailFilterResult object</returns>
        FileDetailFilterResult<SpotTrackerFileDetail> FilterOutExistingDetails(List<SpotTrackerFileDetail> newDetails);

        /// <summary>
        /// Returns a list of SpotTrackerFileDetails filtered by estimate ids
        /// </summary>
        /// <param name="estimateIds">List of estimate ids</param>
        /// <returns>List of SpotTrackerFileDetails</returns>
        List<SpotTrackerFileDetail> GetSpotTrackerFileDetailsByEstimateIds(IEnumerable<int> estimateIds);
    }

    public class SpotTrackerRepository : BroadcastRepositoryBase, ISpotTrackerRepository
    {
        public SpotTrackerRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public int GetSigmaFileIdByHash(string hash)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from x in context.spot_tracker_files
                            where x.file_hash == hash
                            select x.id).FirstOrDefault();
                });
        }

        /// <summary>
        /// Saves a SpotTracker file object
        /// </summary>
        /// <param name="file">SpotTracker object</param>
        /// <returns>Newly created id</returns>
        public int SaveSpotTrackerFile(TrackerFile<SpotTrackerFileDetail> file)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    context.spot_tracker_files.Add(_MapToSpotTrackerFile(file));
                    context.SaveChanges();
                    return file.Id;
                });
            
        }

        /// <summary>
        /// Checks existing details comparing with the DB
        /// </summary>
        /// <param name="newDetails">Details to check</param>
        /// <returns>FileDetailFilterResult object</returns>
        public FileDetailFilterResult<SpotTrackerFileDetail> FilterOutExistingDetails(List<SpotTrackerFileDetail> newDetails)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var iscis = newDetails.Select(bfd => bfd.Isci).ToList();

                var relevantFileDetails = context.spot_tracker_file_details.Where(fd => iscis.Contains(fd.isci)).ToList();
                var groupJoin = from newDetail in _MapToSpotTrackerFileDetail(newDetails)
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

                var result = new FileDetailFilterResult<SpotTrackerFileDetail>();
                foreach (var pair in groupJoin)
                {
                    foreach (var existingDetail in pair.existingDetail.DefaultIfEmpty())
                    {
                        if (existingDetail == null)
                        {
                            result.New.Add(_MapFromSpotTrackerFileDetail(pair.newDetail));
                        }
                        else if (existingDetail.program_name == pair.newDetail.program_name)
                        {
                            result.Ignored.Add(_MapFromSpotTrackerFileDetail(pair.newDetail));
                        }
                        else if (existingDetail.program_name != pair.newDetail.program_name)
                        {
                            existingDetail.program_name = pair.newDetail.program_name;
                            result.Updated.Add(_MapFromSpotTrackerFileDetail(pair.newDetail));
                        }
                        context.SaveChanges();
                    }
                }

                context.SaveChanges();

                return result;
            });
        }

        private spot_tracker_files _MapToSpotTrackerFile(TrackerFile<SpotTrackerFileDetail> file)
        {
            return new spot_tracker_files
            {
                spot_tracker_file_details = _MapToSpotTrackerFileDetail(file.FileDetails),
                created_by = file.CreatedBy,
                created_date = file.CreatedDate,
                end_date = file.EndDate,
                file_hash = file.FileHash,
                id = file.Id,
                file_name = file.Name,
                start_date = file.StartDate
            };
        }
        
        private List<spot_tracker_file_details> _MapToSpotTrackerFileDetail(List<SpotTrackerFileDetail> newDetails)
        {
            return newDetails.Select(x => new spot_tracker_file_details
            {
                advertiser = x.Advertiser,
                affiliate = x.Affiliate,
                date_aired = x.DateAired,
                estimate_id = x.EstimateId,
                id = x.Id,
                isci = x.Isci,
                program_name = x.ProgramName,
                rank = x.Rank,
                spot_length = x.SpotLength,
                spot_length_id = x.SpotLengthId,
                station = x.Station,
                time_aired = x.TimeAired,
                client = x.Client,
                client_name = x.ClientName,
                country = x.Country,
                daypart = x.Daypart,
                day_of_week = x.DayOfWeek,
                discid = x.Discid,
                encode_date = x.EncodeDate,
                encode_time = x.EncodeTime,
                identifier_2 = x.Identifier2,
                identifier_3 = x.Identifier3,
                market = x.Market,
                market_code = x.MarketCode,
                release_name = x.ReleaseName,
                rel_type = x.RelType,
                sid = x.Sid,
                station_name = x.StationName
            }).ToList();
        }

        private SpotTrackerFileDetail _MapFromSpotTrackerFileDetail(spot_tracker_file_details detail)
        {
            return new SpotTrackerFileDetail
            {
                Advertiser = detail.advertiser,
                Affiliate = detail.affiliate,
                DateAired = detail.date_aired,
                EstimateId = detail.estimate_id,
                Id = detail.id,
                Isci = detail.isci,
                Market = detail.market,
                ProgramName = detail.program_name,
                Rank = detail.rank.Value,
                SpotLength = detail.spot_length,
                SpotLengthId = detail.spot_length_id,
                Station = detail.station,
                TimeAired = detail.time_aired,
                Client = detail.client,
                StationName = detail.station_name,
                ClientName = detail.client_name,
                Country = detail.country,
                DayOfWeek = detail.day_of_week,
                Daypart = detail.daypart,
                Discid = detail.discid,
                EncodeDate = detail.encode_date,
                EncodeTime = detail.encode_time,
                Identifier2 = detail.identifier_2,
                Identifier3 = detail.identifier_3,
                MarketCode = detail.market_code,
                ReleaseName = detail.release_name,
                RelType = detail.rel_type,
                Sid = detail.sid
            };
        }

        public List<SpotTrackerFileDetail> GetSpotTrackerFileDetailsByEstimateIds(IEnumerable<int> estimateIds)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.spot_tracker_file_details
                        .Where(d => estimateIds.Contains(d.estimate_id))
                        .ToList()
                        .Select(_MapFromSpotTrackerFileDetail)
                        .ToList();
                });
        }
    }
}
