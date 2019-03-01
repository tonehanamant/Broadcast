using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.OpenMarketInventory;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Common.Services.Extensions;
using System;
using Tam.Maestro.Common;
using static Services.Broadcast.Entities.OpenMarketInventory.ProposalVersionSnapshot.ProposalVersionDetail.ProposalVersionDetailQuarter.ProposalVersionDetailQuarterWeek;
using static Services.Broadcast.Entities.OpenMarketInventory.ProposalVersionSnapshot.ProposalVersionDetail.ProposalVersionDetailQuarter;
using static Services.Broadcast.Entities.OpenMarketInventory.ProposalVersionSnapshot.ProposalVersionDetail;
using static Services.Broadcast.Entities.OpenMarketInventory.ProposalVersionSnapshot;

namespace Services.Broadcast.Repositories
{
    public interface IProposalOpenMarketInventoryRepository : IDataRepository
    {
        List<OpenMarketInventoryAllocation> GetProposalDetailAllocations(int proposalVersionDetailId);
        void RemoveAllocations(List<int> programIds, int proposalDetailId);
        void SaveAllocations(AllocationsChangeRequest allocationsChangeRequest, int guaranteedAudienceId);
        Dictionary<int, List<station_inventory_manifest>> GetStationManifestFromQuarterWeeks(List<int> quarterWeekIds);
        List<StationInventorySpotSnapshot> GetStationInventorySpotsSnapshotData(int proposalId, List<int> proposalDetailIds);
        ProposalVersionSnapshot SaveProposalVersionSnapshot(int proposalId, List<int> proposalDetailIds, List<StationInventorySpotSnapshot> stationInventorySpotSnapshotData);
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

        public void SaveAllocations(AllocationsChangeRequest allocationsChangeRequest, int guaranteedAudienceId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    foreach (var allocation in allocationsChangeRequest.AllocationsToAdd)
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

                    foreach (var allocation in allocationsChangeRequest.AllocationsToRemove)
                    {
                        var programAllocation =
                            context.station_inventory_spots.First(f => f.id == allocation.Id);

                        context.station_inventory_spots.Remove(programAllocation);
                    }

                    context.SaveChanges();
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

        public List<StationInventorySpotSnapshot> GetStationInventorySpotsSnapshotData(int proposalId, List<int> proposalDetailIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var stationInventorySpots = context.station_inventory_spots
                    .Include(x => x.station_inventory_manifest)
                    .Include(x => x.station_inventory_manifest.station_inventory_manifest_dayparts)
                    .Include(x => x.station_inventory_manifest.station)
                    .Include(x => x.station_inventory_manifest.station.market)
                    .Include(x => x.station_inventory_manifest.station.market.market_coverages)
                    .Include(x => x.station_inventory_spot_audiences)
                    .Include(x => x.proposal_version_detail_quarter_weeks)
                    .Include(x => x.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters)
                    .Include(x => x.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details)
                    .Where(x => x.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.proposal_versions.proposal_id == proposalId &&
                                proposalDetailIds.Contains(x.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details.id))
                    .ToList();

                return stationInventorySpots
                    .SelectMany(x => x.station_inventory_spot_audiences, (source, audience) => new { spot = source, audience })
                    .SelectMany(x => x.spot.station_inventory_manifest.station_inventory_manifest_dayparts, (source, daypart) => new
                    {
                        source.spot,
                        manifest = source.spot.station_inventory_manifest,
                        source.spot.station_inventory_manifest.station,
                        source.audience,
                        daypart,
                        detail = source.spot.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details
                    })
                    .Select(x => new StationInventorySpotSnapshot
                    {
                        ProposalVersionDetailQuarterWeekId = x.spot.proposal_version_detail_quarter_week_id,
                        MediaWeekId = x.spot.media_week_id,
                        SpotLengthId = x.manifest.spot_length_id,
                        ProgramName = x.daypart.program_name,
                        DaypartId = x.daypart.daypart_id,
                        StationCode = x.manifest.station_code,
                        StationCallLetters = x.station.legacy_call_letters,
                        StationMarketCode = x.station.market_code.Value,
                        SpotImpressions = x.audience.calculated_impressions,
                        SpotCost = x.audience.calculated_rate,
                        AudienceId = x.audience.audience_id,
                        SingleProjectionBookId = x.detail.single_projection_book_id,
                        ShareProjectionBookId = x.detail.share_projection_book_id
                    })
                    .ToList();
            });
        }

        public ProposalVersionSnapshot SaveProposalVersionSnapshot(int proposalId, List<int> proposalDetailIds, List<StationInventorySpotSnapshot> stationInventorySpotSnapshotData)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    // let`s turn off lazy loading since we want EF to load only tables which are specified in Include function
                    context.Configuration.LazyLoadingEnabled = false;

                    var proposalVersion = context.proposal_versions
                        .Include(x => x.proposal_version_spot_length)
                        .Include(x => x.proposal_version_audiences)
                        .Include(x => x.proposal_version_flight_weeks)
                        .Include(x => x.proposal_version_markets)
                        .Include(x => x.proposal_version_details)
                        .Include(x => x.proposal_version_details.Select(d => d.proposal_version_detail_criteria_cpm))
                        .Include(x => x.proposal_version_details.Select(d => d.proposal_version_detail_criteria_genres))
                        .Include(x => x.proposal_version_details.Select(d => d.proposal_version_detail_criteria_programs))
                        .Include(x => x.proposal_version_details.Select(d => d.proposal_version_detail_criteria_show_types))
                        .Include(x => x.proposal_version_details.Select(d => d.proposal_version_detail_quarters))
                        .Include(x => x.proposal_version_details.Select(d => d.proposal_version_detail_quarters.Select(q => q.proposal_version_detail_quarter_weeks)))
                        .Include(x => x.proposal_version_details.Select(d => d.proposal_version_detail_quarters.Select(q => q.proposal_version_detail_quarter_weeks.Select(qw => qw.proposal_version_detail_quarter_week_iscis))))
                        .Single(x => x.proposal_id == proposalId && x.id == x.proposal.primary_version_id && x.snapshot_date == null);

                    // let`s make EF think it creates a new proposal version
                    _UnbindProposalVersionFromDBAndSetSnapshotData(proposalVersion, proposalDetailIds, stationInventorySpotSnapshotData, context);

                    proposalVersion.snapshot_date = DateTime.Now;

                    context.SaveChanges();

                    return _MapToProposalVersion(proposalVersion);
                });
        }

        private static void _UnbindProposalVersionFromDBAndSetSnapshotData(
            proposal_versions version,
            List<int> proposalDetailIds,
            List<StationInventorySpotSnapshot> stationInventorySpotSnapshotData,
            QueryHintBroadcastContext context)
        {
            context.Entry(version).State = EntityState.Added;
            version.id = 0;
            
            version.proposal_version_spot_length.ForEach(x =>
            {
                context.Entry(x).State = EntityState.Added;
                x.id = 0;
                x.proposal_version_id = 0;
            });
            
            version.proposal_version_audiences.ForEach(x =>
            {
                context.Entry(x).State = EntityState.Added;
                x.id = 0;
                x.proposal_version_id = 0;
            });
            
            version.proposal_version_flight_weeks.ForEach(x =>
            {
                context.Entry(x).State = EntityState.Added;
                x.id = 0;
                x.proposal_version_id = 0;
            });
            
            version.proposal_version_markets.ForEach(x =>
            {
                context.Entry(x).State = EntityState.Added;
                x.id = 0;
                x.proposal_version_id = 0;
            });

            var detailsToRemove = version.proposal_version_details.Where(x => !proposalDetailIds.Contains(x.id)).ToList();

            version.proposal_version_details.ForEach(x =>
            {
                context.Entry(x).State = EntityState.Added;
                x.id = 0;
                x.proposal_version_id = 0;
            });

            version.proposal_version_details
                .SelectMany(x => x.proposal_version_detail_criteria_cpm)
                .ForEach(x =>
                {
                    context.Entry(x).State = EntityState.Added;
                    x.id = 0;
                    x.proposal_version_detail_id = 0;
                });

            version.proposal_version_details
                .SelectMany(x => x.proposal_version_detail_criteria_genres)
                .ForEach(x =>
                {
                    context.Entry(x).State = EntityState.Added;
                    x.id = 0;
                    x.proposal_version_detail_id = 0;
                });

            version.proposal_version_details
                .SelectMany(x => x.proposal_version_detail_criteria_programs)
                .ForEach(x =>
                {
                    context.Entry(x).State = EntityState.Added;
                    x.id = 0;
                    x.proposal_version_detail_id = 0;
                });

            version.proposal_version_details
                .SelectMany(x => x.proposal_version_detail_criteria_show_types)
                .ForEach(x =>
                {
                    context.Entry(x).State = EntityState.Added;
                    x.id = 0;
                    x.proposal_version_detail_id = 0;
                });
            
            version.proposal_version_details
                .SelectMany(x => x.proposal_version_detail_quarters)
                .ForEach(x =>
                {
                    context.Entry(x).State = EntityState.Added;
                    x.id = 0;
                    x.proposal_version_detail_id = 0;
                });

            version.proposal_version_details
                .SelectMany(x => x.proposal_version_detail_quarters)
                .SelectMany(x => x.proposal_version_detail_quarter_weeks)
                .ForEach(week =>
                {
                    context.Entry(week).State = EntityState.Added;
                    week.proposal_version_quarter_id = 0;

                    stationInventorySpotSnapshotData
                        .Where(x => x.ProposalVersionDetailQuarterWeekId == week.id)
                        .Select(_MapToStationInventorySpotSnapshot)
                        .ForEach(week.station_inventory_spot_snapshots.Add);

                    week.id = 0;
                });

            version.proposal_version_details
                .SelectMany(x => x.proposal_version_detail_quarters)
                .SelectMany(x => x.proposal_version_detail_quarter_weeks)
                .SelectMany(x => x.proposal_version_detail_quarter_week_iscis)
                .ForEach(x =>
                {
                    context.Entry(x).State = EntityState.Added;
                    x.id = 0;
                    x.proposal_version_detail_quarter_week_id = 0;
                });

            // Removing not needed details. We don't want EF to generate a SQL script for them. 
            // Do not move it somewhere above. EF would remove existing data in this case
            detailsToRemove.ForEach(x => context.proposal_version_details.Remove(x));
        }

        #region ProposalVersionSnapshot Mappings

        private static ProposalVersionSnapshot _MapToProposalVersion(proposal_versions model)
        {
            return new ProposalVersionSnapshot
            {
                Id = model.id,
                Proposal_Version = model.proposal_version,
                StartDate = model.start_date,
                EndDate = model.end_date,
                GuaranteedAudienceId = model.guaranteed_audience_id,
                Markets = model.markets,
                CreatedBy = model.created_by,
                CreatedDate = model.created_date,
                ModifiedBy = model.modified_by,
                ModifiedDate = model.modified_date,
                TargetBudget = model.target_budget,
                TargetUnits = model.target_units,
                TargetImpressions = model.target_impressions,
                Notes = model.notes,
                PostType = model.post_type,
                Equivalized = model.equivalized,
                BlackoutMarkets = model.blackout_markets,
                Status = model.status,
                TargetCpm = model.target_cpm,
                Margin = model.margin,
                CostTotal = model.cost_total,
                ImpressionsTotal = model.impressions_total,
                MarketCoverage = model.market_coverage,
                SnapshotDate = model.snapshot_date,
                ProposalVersionAudiences = model.proposal_version_audiences.Select(_MapToProposalVersionAudience).ToList(),
                ProposalVersionFlightWeeks = model.proposal_version_flight_weeks.Select(_MapToProposalVersionFlightWeek).ToList(),
                ProposalVersionMarkets = model.proposal_version_markets.Select(_MapToProposalVersionMarket).ToList(),
                ProposalVersionDetails = model.proposal_version_details.Select(_MapToProposalVersionDetail).ToList(),
                ProposalVersionSpotLengths = model.proposal_version_spot_length.Select(_MapToProposalVersionSpotLength).ToList()
            };
        }

        private static ProposalVersionSpotLength _MapToProposalVersionSpotLength(proposal_version_spot_length model)
        {
            return new ProposalVersionSpotLength
            {
                Id = model.id,
                SpotLengthId = model.spot_length_id
            };
        }

        private static ProposalVersionAudience _MapToProposalVersionAudience(proposal_version_audiences model)
        {
            return new ProposalVersionAudience
            {
                Id = model.id,
                AudienceId = model.audience_id,
                Rank = model.rank
            };
        }

        private static ProposalVersionFlightWeek _MapToProposalVersionFlightWeek(proposal_version_flight_weeks model)
        {
            return new ProposalVersionFlightWeek
            {
                Id = model.id,
                MediaWeekId = model.media_week_id,
                StartDate = model.start_date,
                EndDate = model.end_date,
                Active = model.active
            };
        }

        private static ProposalVersionMarket _MapToProposalVersionMarket(proposal_version_markets model)
        {
            return new ProposalVersionMarket
            {
                Id = model.id,
                MarketCode = model.market_code,
                IsBlackout = model.is_blackout
            };
        }

        private static ProposalVersionDetail _MapToProposalVersionDetail(proposal_version_details model)
        {
            return new ProposalVersionDetail
            {
                Id = model.id,
                SpotLengthId = model.spot_length_id,
                DaypartId = model.daypart_id,
                DaypartCode = model.daypart_code,
                StartDate = model.start_date,
                EndDate = model.end_date,
                UnitsTotal = model.units_total,
                ImpressionsTotal = model.impressions_total,
                CostTotal = model.cost_total,
                Adu = model.adu,
                SingleProjectionBookId = model.single_projection_book_id,
                HutProjectionBookId = model.hut_projection_book_id,
                ShareProjectionBookId = model.share_projection_book_id,
                ProjectionPlaybackType = model.projection_playback_type,
                OpenMarketImpressionsTotal = model.open_market_impressions_total,
                OpenMarketCostTotal = model.open_market_cost_total,
                ProprietaryImpressionsTotal = model.proprietary_impressions_total,
                ProprietaryCostTotal = model.proprietary_cost_total,
                Sequence = model.sequence,
                PostingBookId = model.posting_book_id,
                PostingPlaybackType = model.posting_playback_type,
                NtiConversionFactor = model.nti_conversion_factor,
                ProposalVersionDetailCriteriaCpms = model.proposal_version_detail_criteria_cpm.Select(_MapToProposalVersionDetailCriteriaCpm).ToList(),
                ProposalVersionDetailCriteriaGenres = model.proposal_version_detail_criteria_genres.Select(_MapToProposalVersionDetailCriteriaGenre).ToList(),
                ProposalVersionDetailCriteriaPrograms = model.proposal_version_detail_criteria_programs.Select(_MapToProposalVersionDetailCriteriaProgram).ToList(),
                ProposalVersionDetailCriteriaShowTypes = model.proposal_version_detail_criteria_show_types.Select(_MapToProposalVersionDetailCriteriaShowType).ToList(),
                ProposalVersionDetailQuarters = model.proposal_version_detail_quarters.Select(_MapToProposalVersionDetailQuarter).ToList()
            };
        }

        private static ProposalVersionDetailCriteriaCpm _MapToProposalVersionDetailCriteriaCpm(proposal_version_detail_criteria_cpm model)
        {
            return new ProposalVersionDetailCriteriaCpm
            {
                Id = model.id,
                MinMax = model.min_max,
                Value = model.value
            };
        }

        private static ProposalVersionDetailCriteriaGenre _MapToProposalVersionDetailCriteriaGenre(proposal_version_detail_criteria_genres model)
        {
            return new ProposalVersionDetailCriteriaGenre
            {
                Id = model.id,
                ContainType = model.contain_type,
                GenreId = model.genre_id
            };
        }

        private static ProposalVersionDetailCriteriaProgram _MapToProposalVersionDetailCriteriaProgram(proposal_version_detail_criteria_programs model)
        {
            return new ProposalVersionDetailCriteriaProgram
            {
                Id = model.id,
                ContainType = model.contain_type,
                ProgramName = model.program_name,
                ProgramNameId = model.program_name_id
            };
        }

        private static ProposalVersionDetailCriteriaShowType _MapToProposalVersionDetailCriteriaShowType(proposal_version_detail_criteria_show_types model)
        {
            return new ProposalVersionDetailCriteriaShowType
            {
                Id = model.id,
                ContainType = model.contain_type,
                ShowTypeId = model.show_type_id
            };
        }

        private static ProposalVersionDetailQuarter _MapToProposalVersionDetailQuarter(proposal_version_detail_quarters model)
        {
            return new ProposalVersionDetailQuarter
            {
                Id = model.id,
                Quarter = model.quarter,
                Year = model.year,
                Cpm = model.cpm,
                ImpressionsGoal = model.impressions_goal,
                ProposalVersionDetailQuarterWeeks = model.proposal_version_detail_quarter_weeks.Select(_MapToProposalVersionDetailQuarterWeek).ToList()
            };
        }

        private static ProposalVersionDetailQuarterWeek _MapToProposalVersionDetailQuarterWeek(proposal_version_detail_quarter_weeks model)
        {
            return new ProposalVersionDetailQuarterWeek
            {
                Id = model.id,
                MediaWeekId = model.media_week_id,
                StartDate = model.start_date,
                EndDate = model.end_date,
                IsHiatus = model.is_hiatus,
                Units = model.units,
                ImpressionsGoal = model.impressions_goal,
                Cost = model.cost,
                OpenMarketImpressionsTotal = model.open_market_impressions_total,
                OpenMarketCostTotal = model.open_market_cost_total,
                ProprietaryImpressionsTotal = model.proprietary_impressions_total,
                ProprietaryCostTotal = model.proprietary_cost_total,
                MyEventsReportName = model.myevents_report_name,
                ProposalVersionDetailQuarterWeekIscis = model.proposal_version_detail_quarter_week_iscis.Select(_MapToProposalVersionDetailQuarterWeekIsci).ToList(),
                StationInventorySpotSnapshots = model.station_inventory_spot_snapshots.Select(_MapToStationInventorySpotSnapshot).ToList()
            };
        }

        private static ProposalVersionDetailQuarterWeekIsci _MapToProposalVersionDetailQuarterWeekIsci(proposal_version_detail_quarter_week_iscis model)
        {
            return new ProposalVersionDetailQuarterWeekIsci
            {
                Id = model.id,
                ClientIsci = model.client_isci,
                HouseIsci = model.house_isci,
                Brand = model.brand,
                MarriedHouseIsci = model.married_house_iscii,
                Monday = model.monday,
                Tuesday = model.tuesday,
                Wednesday = model.wednesday,
                Thursday = model.thursday,
                Friday = model.friday,
                Saturday = model.saturday,
                Sunday = model.sunday
            };
        }

        private static StationInventorySpotSnapshot _MapToStationInventorySpotSnapshot(station_inventory_spot_snapshots model)
        {
            return new StationInventorySpotSnapshot
            {
                Id = model.id,
                ProposalVersionDetailQuarterWeekId = model.proposal_version_detail_quarter_week_id,
                MediaWeekId = model.media_week_id,
                SpotLengthId = model.spot_length_id,
                ProgramName = model.program_name,
                DaypartId = model.daypart_id,
                StationCode = model.station_code,
                StationCallLetters = model.station_call_letters,
                StationMarketCode = model.station_market_code,
                StationMarketRank = model.station_market_rank,
                SpotImpressions = model.spot_impressions,
                SpotCost = model.spot_cost,
                AudienceId = model.audience_id
            };
        }

        private static station_inventory_spot_snapshots _MapToStationInventorySpotSnapshot(StationInventorySpotSnapshot model)
        {
            return new station_inventory_spot_snapshots
            {
                media_week_id = model.MediaWeekId,
                spot_length_id = model.SpotLengthId,
                program_name = model.ProgramName,
                daypart_id = model.DaypartId,
                station_code = model.StationCode,
                station_call_letters = model.StationCallLetters,
                station_market_code = model.StationMarketCode,
                station_market_rank = model.StationMarketRank,
                spot_impressions = model.SpotImpressions,
                spot_cost = model.SpotCost,
                audience_id = model.AudienceId
            };
        }

        #endregion
    }
}
