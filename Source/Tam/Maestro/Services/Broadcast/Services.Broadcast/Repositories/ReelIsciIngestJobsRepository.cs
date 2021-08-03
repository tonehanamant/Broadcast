using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IReelIsciIngestJobsRepository : IDataRepository
    {
        int AddReelIsciIngestJob(ReelIsciIngestJobDto reelIsciIngestJobObj);
        void UpdateReelIsciIngestJob(ReelIsciIngestJobDto reelIsciIngestJobObj);

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
    }

    public class ReelIsciIngestJobsRepository : BroadcastRepositoryBase, IReelIsciIngestJobsRepository
    {
        public ReelIsciIngestJobsRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        public int AddReelIsciIngestJob(ReelIsciIngestJobDto reelIsciIngestJobObj)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var reelIsciIngestJobDb = new reel_isci_ingest_jobs
                {
                    status = (int)reelIsciIngestJobObj.Status,
                    queued_at = reelIsciIngestJobObj.QueuedAt,
                    queued_by = reelIsciIngestJobObj.QueuedBy
                };

                context.reel_isci_ingest_jobs.Add(reelIsciIngestJobDb);

                context.SaveChanges();

                return reelIsciIngestJobDb.id;
            });
        }
        public void UpdateReelIsciIngestJob(ReelIsciIngestJobDto reelIsciIngestJobObj)
        {
            _InReadUncommitedTransaction(context =>
                {
                    var job = context.reel_isci_ingest_jobs.Single(x => x.id == reelIsciIngestJobObj.Id);

                    job.status = (int)reelIsciIngestJobObj.Status;
                    job.completed_at = reelIsciIngestJobObj.CompletedAt;
                    job.error_message = reelIsciIngestJobObj.ErrorMessage;

                    context.SaveChanges();
                }
            );
        }

        /// <inheritdoc />
        public int DeleteReelIscisBetweenRange(DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var reelIscisToDelete = context.reel_iscis
                                                .Where(reelIsci => reelIsci.active_start_date <= startDate && reelIsci.active_end_date >= endDate 
                                                                    || reelIsci.active_start_date >= startDate && reelIsci.active_start_date <= endDate 
                                                                    || reelIsci.active_end_date >= startDate && reelIsci.active_end_date <= endDate)
                                                .ToList();

                var deletedCount = context.reel_iscis.RemoveRange(reelIscisToDelete).Count();
                context.SaveChanges();
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
                var addedCount = context.reel_iscis.AddRange(reelIscisToAdd).Count();
                context.SaveChanges();
                return addedCount;
            });
        }
    }
}