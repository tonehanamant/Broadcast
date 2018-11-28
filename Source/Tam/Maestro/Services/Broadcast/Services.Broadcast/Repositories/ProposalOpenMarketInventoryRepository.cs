﻿using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.OpenMarketInventory;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Services.Broadcast.Entities.DTO.PricingGuide;
using Services.Broadcast.Entities;
using Common.Services.Extensions;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Repositories
{
    public interface IProposalOpenMarketInventoryRepository : IDataRepository
    {
        List<OpenMarketInventoryAllocation> GetProposalDetailAllocations(int proposalVersionDetailId);
        void RemoveAllocations(List<OpenMarketInventoryAllocation> allocationToRemove);
        void AddAllocations(List<OpenMarketInventoryAllocation> allocationToAdd, int guaranteedAudienceId);
        void RemoveAllocations(List<int> programIds, int proposalDetailId);
        Dictionary<int, List<station_inventory_manifest>> GetStationManifestFromQuarterWeeks(List<int> quarterWeekIds);
    }

    public class ProposalOpenMarketInventoryRepository : BroadcastRepositoryBase, IProposalOpenMarketInventoryRepository
    {
        public ProposalOpenMarketInventoryRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public List<OpenMarketInventoryAllocation> GetProposalDetailAllocations(int proposalVersionDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return (from a in context.station_inventory_spots
                        join w in context.proposal_version_detail_quarter_weeks on a.proposal_version_detail_quarter_week_id equals w.id
                        join q in context.proposal_version_detail_quarters on w.proposal_version_quarter_id equals q.id
                        where q.proposal_version_detail_id == proposalVersionDetailId
                        select new OpenMarketInventoryAllocation
                        {
                            Id = a.id,
                            ProposalVersionDetailId = proposalVersionDetailId,
                            ManifestId = a.station_inventory_manifest_id,
                            MediaWeekId = a.media_week_id,
                            ProposalVersionDetailQuarterWeekId = a.proposal_version_detail_quarter_week_id,
                        }).ToList();
            });
        }

        public void RemoveAllocations(List<OpenMarketInventoryAllocation> allocations)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var allocation in allocations)
                    {
                        var programAllocation =
                            context.station_inventory_spots.First(f => f.id == allocation.Id);

                        context.station_inventory_spots.Remove(programAllocation);
                    }

                    context.SaveChanges();
                });
        }

        public void AddAllocations(List<OpenMarketInventoryAllocation> allocations, int guaranteedAudienceId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var allocation in allocations)
                    {
                        var proposalQuarterWeekId =
                            context.proposal_version_detail_quarter_weeks.Where(
                                w =>
                                    w.proposal_version_detail_quarters.proposal_version_detail_id ==
                                    allocation.ProposalVersionDetailId && w.media_week_id == allocation.MediaWeekId)
                                .Select(w => w.id)
                                .Single();

                        for (var index = 0; index < allocation.Spots; index++)
                        {
                            var programAllocation = new station_inventory_spots
                            {
                                media_week_id = allocation.MediaWeekId,
                                proposal_version_detail_quarter_week_id = proposalQuarterWeekId,
                                station_inventory_manifest_id = allocation.ManifestId
                            };

                            programAllocation.station_inventory_spot_audiences.Add(new station_inventory_spot_audiences
                            {
                                audience_id = guaranteedAudienceId,
                                calculated_impressions = allocation.UnitImpressions,
                                calculated_rate = allocation.Rate
                            });

                            context.station_inventory_spots.Add(programAllocation);
                        }
                    }
                    context.SaveChanges();
                });
        }

        public void RemoveAllocations(List<int> programIds, int proposalDetailId)
        {
            _InReadUncommitedTransaction(c =>
            {
                var proposalVersionDetailQuarterWeekIds =
                    c.proposal_version_details.Where(pvd => pvd.id == proposalDetailId)
                        .SelectMany(
                            pvd =>
                                pvd.proposal_version_detail_quarters.SelectMany(
                                    q => q.proposal_version_detail_quarter_weeks.Select(qw => qw.id)));

                var matchingAllocations =
                    c.station_inventory_spots.Where(
                        spfp =>
                            proposalVersionDetailQuarterWeekIds.Contains(spfp.proposal_version_detail_quarter_week_id ?? 0) &&
                            programIds.Contains(spfp.station_inventory_manifest_id));

                c.station_inventory_spots.RemoveRange(matchingAllocations);

                c.SaveChanges();
            });
        }

        public Dictionary<int, List<station_inventory_manifest>> GetStationManifestFromQuarterWeeks(List<int> quarterWeekIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var list = context.station_inventory_spots
                    .Include("station_inventory_manifest.station")
                    .Where(sis => sis.proposal_version_detail_quarter_week_id.HasValue &&
                                  quarterWeekIds.Contains(sis.proposal_version_detail_quarter_week_id.Value)).ToList();

                return list.GroupBy(sis => sis.proposal_version_detail_quarter_week_id.Value)
                            .ToDictionary(k => k.Key, v => v.Select(m => m.station_inventory_manifest).ToList());
            });
        }
    }
}
