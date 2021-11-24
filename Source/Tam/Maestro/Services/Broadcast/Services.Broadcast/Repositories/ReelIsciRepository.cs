using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Isci;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IReelIsciRepository : IDataRepository
    {
        /// <summary>
        /// Deletes reel iscis which are active between startdate and enddate
        /// </summary>
        /// <param name="startDate">The startdate from when reel iscis to be deleted</param>
        /// <param name="endDate">The enddate till when reel iscis to be deleted</param>
        /// <returns>Total number of deleted reel iscis</returns>
        int DeleteReelIscisBetweenRange(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Adds reel iscis
        /// </summary>
        /// <param name="reelIscis">The list of reel iscis to be inserted</param>
        /// <returns>Total number of inserted reel iscis</returns>
        int AddReelIscis(List<ReelIsciDto> reelIscis);

        /// <summary>
        /// Gets the reel iscis.
        /// </summary>
        /// <returns></returns>
        List<ReelIsciDto> GetReelIscis();

        /// <summary>
        /// Gets the reel isci details for the given iscis.
        /// </summary>
        /// <param name="iscis">Given list of iscis.</param>
        List<ReelIsciDto> GetReelIscis(List<string> iscis);
    }

    public class ReelIsciRepository : BroadcastRepositoryBase, IReelIsciRepository
    {
        public ReelIsciRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }
        
        /// <inheritdoc />
        public int DeleteReelIscisBetweenRange(DateTime startDate, DateTime endDate)
        {
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddMinutes(-1);

            return _InReadUncommitedTransaction(context =>
            {
                var sql = $@"DELETE FROM reel_iscis WHERE (active_start_date <= @startDate AND active_end_date >= @endDate)
                                OR (active_start_date >= @startDate AND active_start_date <= @endDate)
                                OR (active_end_date >= @startDate AND active_end_date <= @endDate)";
                var startDateParameter = new SqlParameter("@startDate", startDate);
                var endDateParameter = new SqlParameter("@endDate", endDate);
                var deletedCount = context.Database.ExecuteSqlCommand(sql, startDateParameter, endDateParameter);
                return deletedCount;
            });
        }

        /// <inheritdoc />
        public int AddReelIscis(List<ReelIsciDto> reelIscis)
        {
            return _InReadUncommitedTransaction(context => 
            {
                var reelIscisToAdd = reelIscis.Select(reelIsci => new reel_iscis() 
                {
                    isci = reelIsci.Isci,
                    spot_length_id = reelIsci.SpotLengthId,
                    active_start_date = reelIsci.ActiveStartDate,
                    active_end_date = reelIsci.ActiveEndDate,
                    reel_isci_advertiser_name_references = reelIsci.ReelIsciAdvertiserNameReferences.Select(x => new reel_isci_advertiser_name_references()
                    {
                        advertiser_name_reference = x.AdvertiserNameReference
                    }).ToList(),
                    ingested_at = reelIsci.IngestedAt
                }).ToList();

                if (reelIscisToAdd.Any())
                {
                    var propertiesToIgnore = new List<string>() { "id" };
                    BulkInsert(context, reelIscisToAdd, propertiesToIgnore);

                    var ingestedAt = reelIscisToAdd.First().ingested_at;
                    var addedReelIscis = context.reel_iscis.Where(x => x.ingested_at == ingestedAt).ToList();
                    var reelIsciEntities = (from reelIsci in reelIscisToAdd
                                            join addedReelIsci in addedReelIscis on new
                                            {
                                                Isci = reelIsci.isci,
                                                SpotLengthId = reelIsci.spot_length_id,
                                                ActiveStartDate = reelIsci.active_start_date,
                                                ActiveEndDate = reelIsci.active_end_date,
                                            } equals new
                                            {
                                                Isci = addedReelIsci.isci,
                                                SpotLengthId = addedReelIsci.spot_length_id,
                                                ActiveStartDate = addedReelIsci.active_start_date,
                                                ActiveEndDate = addedReelIsci.active_end_date,
                                            }
                                            select new
                                            {
                                                ReelIscisToAdd = reelIsci,
                                                AddedReelIscis = addedReelIsci
                                            }).ToList();

                    var reelIsciAdvertiserNameReferencesToAdd = reelIsciEntities.SelectMany(x => x.ReelIscisToAdd.reel_isci_advertiser_name_references.Select(reelIsciAdvertiserNameReference => new reel_isci_advertiser_name_references()
                    {
                        reel_isci_id = x.AddedReelIscis.id,
                        advertiser_name_reference = reelIsciAdvertiserNameReference.advertiser_name_reference
                    })).ToList();
                    if (reelIsciAdvertiserNameReferencesToAdd.Any())
                    {
                        var propertiesToIgnoreReelIsciAdvertiserNameReferences = new List<string>() { "id" };
                        BulkInsert(context, reelIsciAdvertiserNameReferencesToAdd, propertiesToIgnoreReelIsciAdvertiserNameReferences);
                    }
                }
                var addedCount = reelIscisToAdd.Count();
                return addedCount;
            });
        }

        /// <inheritdoc />
        public List<ReelIsciDto> GetReelIscis()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.reel_iscis
                    .Select( s => new ReelIsciDto
                    {
                        Id = s.id,
                        Isci = s.isci,
                        SpotLengthId = s.spot_length_id,
                        ActiveStartDate = s.active_start_date,
                        ActiveEndDate = s.active_end_date,
                        IngestedAt = s.ingested_at,
                        ReelIsciAdvertiserNameReferences = s.reel_isci_advertiser_name_references
                            .Select(a => new ReelIsciAdvertiserNameReferenceDto
                        {
                            Id = a.id,
                            ReelIsciId = a.reel_isci_id,
                            AdvertiserNameReference = a.advertiser_name_reference
                        }).ToList()
                    })
                    .ToList();

                return result;
            });
        }

        /// <inheritdoc />
        public List<ReelIsciDto> GetReelIscis(List<string> iscis)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.reel_iscis
                .Where(s => iscis.Contains(s.isci))
                .Select(s => new ReelIsciDto
                    {
                        Id = s.id,
                        Isci = s.isci,
                        SpotLengthId = s.spot_length_id,
                        ActiveStartDate = s.active_start_date,
                        ActiveEndDate = s.active_end_date,
                        IngestedAt = s.ingested_at,
                        ReelIsciAdvertiserNameReferences = s.reel_isci_advertiser_name_references
                           .Select(a => new ReelIsciAdvertiserNameReferenceDto
                           {
                               Id = a.id,
                               ReelIsciId = a.reel_isci_id,
                               AdvertiserNameReference = a.advertiser_name_reference
                           }).ToList()
                    })                    
                    .ToList();

                return result;
            });
        }
    }
}