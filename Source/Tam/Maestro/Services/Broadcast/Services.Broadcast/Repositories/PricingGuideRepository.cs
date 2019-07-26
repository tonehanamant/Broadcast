using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.DTO.PricingGuide;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.OpenMarketInventory;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IPricingGuideRepository : IDataRepository
    {
        PricingGuideOpenMarketInventory GetProposalDetailPricingGuideInventory(int proposalDetailId);

        ProposalDetailOpenMarketInventoryDto GetOpenMarketProposalDetailInventory(int proposalDetailId);

        ProposalDetailProprietaryInventoryDto GetProprietaryProposalDetailInventory(int proposalDetailId);

        /// <summary>
        /// Gets the pricing guide data based on a proposal version and a proposal detail
        /// </summary>
        /// <param name="proposalDetailId">Proposal detail to get the pricing data for</param>
        /// <returns>ProposalDetailPricingGuideDto object</returns>
        PricingGuideDto GetPricingGuideForProposalDetail(int proposalDetailId);

        /// <summary>
        /// Gets the proprietary pricings for a specifiv proposal detail
        /// </summary>
        /// <param name="proposalVersionDetailId">Proposal version detail id to filter by</param>
        /// <returns>List of ProprietaryPricingDto objects</returns>
        List<ProprietaryPricingDto> GetProprietaryPricings(int proposalVersionDetailId);

        /// <summary>
        /// Saves the pricing guide data and distribution
        /// </summary>
        /// <param name="model">ProposalDetailPricingGuideSaveRequest object</param>
        /// <param name="username">User requesting the save</param>
        /// <returns>ProposalDetailDto object</returns>
        void SavePricingGuideDistribution(ProposalDetailPricingGuideSave model, string username);

        /// <summary>
        /// Gets the distribution programs
        /// </summary>
        /// <param name="distributionId">Distribution id to get the programs for</param>
        /// <returns>List of DistributionProgram objects</returns>
        List<DistributionProgram> GetDistributionMarkets(int distributionId);
    }
    public class PricingGuideRepository : BroadcastRepositoryBase, IPricingGuideRepository
    {
        public PricingGuideRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public PricingGuideOpenMarketInventory GetProposalDetailPricingGuideInventory(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var pv = context.proposal_version_details
                    .Include(pvd => pvd.proposal_versions.proposal)
                    .Include(pvd =>
                        pvd.proposal_version_detail_quarters.Select(dq => dq.proposal_version_detail_quarter_weeks))
                    .Include(pvd => pvd.proposal_versions.proposal_version_flight_weeks)
                    .Include(pvd => pvd.proposal_version_detail_criteria_cpm)
                    .Include(pvd => pvd.proposal_version_detail_criteria_genres)
                    .Include(pvd => pvd.proposal_version_detail_criteria_programs)
                    .Include(pvd => pvd.proposal_versions.proposal_version_markets)
                    .Single(d => d.id == proposalDetailId,
                        $"The proposal detail information you have entered [{proposalDetailId}] does not exist.");

                var dto = new PricingGuideOpenMarketInventory
                {
                    MarketCoverage = pv.proposal_versions.market_coverage ?? 1,
                    ProposalMarkets = pv.proposal_versions.proposal_version_markets.Select(x => new ProposalMarketDto
                    {
                        Id = x.market_code,
                        IsBlackout = x.is_blackout
                    }).ToList()
                };

                _PopulateProposalDetailInventoryBase(pv, dto);

                dto.Criteria = new OpenMarketCriterion
                {
                    CpmCriteria = pv.proposal_version_detail_criteria_cpm.Select(c =>
                        new CpmCriteria { Id = c.id, MinMax = (MinMaxEnum)c.min_max, Value = c.value }).ToList(),
                    GenreSearchCriteria = pv.proposal_version_detail_criteria_genres.Select(c =>
                            new GenreCriteria()
                            {
                                Id = c.id,
                                Contain = (ContainTypeEnum)c.contain_type,
                                Genre = new LookupDto(c.genre_id, c.genre.name)
                            })
                        .ToList(),
                    ProgramNameSearchCriteria = pv.proposal_version_detail_criteria_programs.Select(c =>
                        new ProgramCriteria
                        {
                            Id = c.id,
                            Contain = (ContainTypeEnum)c.contain_type,
                            Program = new LookupDto
                            {
                                Id = c.program_name_id,
                                Display = c.program_name
                            }
                        }).ToList()
                };

                return dto;
            });
        }

        public ProposalDetailOpenMarketInventoryDto GetOpenMarketProposalDetailInventory(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var pv = context.proposal_version_details
                    .Include(pvd => pvd.proposal_versions.proposal)
                    .Include(pvd =>
                        pvd.proposal_version_detail_quarters.Select(dq => dq.proposal_version_detail_quarter_weeks))
                    .Include(pvd => pvd.proposal_versions.proposal_version_flight_weeks)
                    .Include(pvd => pvd.proposal_version_detail_criteria_cpm)
                    .Include(pvd => pvd.proposal_version_detail_criteria_genres)
                    .Include(pvd => pvd.proposal_version_detail_criteria_programs)
                    .Single(d => d.id == proposalDetailId,
                        string.Format("The proposal detail information you have entered [{0}] does not exist.",
                            proposalDetailId));

                var dto = new ProposalDetailOpenMarketInventoryDto
                {
                    Margin = pv.proposal_versions.margin,
                    GuaranteedAudience = pv.proposal_versions.guaranteed_audience_id
                };
                _PopulateProposalDetailInventoryBase(pv, dto);
                dto.Criteria = new OpenMarketCriterion
                {
                    CpmCriteria = pv.proposal_version_detail_criteria_cpm.Select(c =>
                        new CpmCriteria { Id = c.id, MinMax = (MinMaxEnum)c.min_max, Value = c.value }).ToList(),
                    GenreSearchCriteria = pv.proposal_version_detail_criteria_genres.Select(c =>
                            new GenreCriteria()
                            {
                                Id = c.id,
                                Contain = (ContainTypeEnum)c.contain_type,
                                Genre = new LookupDto(c.genre_id, c.genre.name)
                            })
                        .ToList(),
                    ProgramNameSearchCriteria = pv.proposal_version_detail_criteria_programs.Select(c =>
                        new ProgramCriteria
                        {
                            Id = c.id,
                            Contain = (ContainTypeEnum)c.contain_type,
                            Program = new LookupDto
                            {
                                Id = c.program_name_id,
                                Display = c.program_name
                            }
                        }).ToList()
                };

                dto.Weeks = (from quarter in pv.proposal_version_detail_quarters
                             from week in quarter.proposal_version_detail_quarter_weeks
                             orderby week.start_date
                             select new ProposalOpenMarketInventoryWeekDto
                             {
                                 ProposalVersionDetailQuarterWeekId = week.id,
                                 ImpressionsGoal = week.impressions_goal,
                                 Budget = week.cost,
                                 QuarterText = string.Format("Q{0}", quarter.quarter),
                                 Week = week.start_date.ToShortDateString(),
                                 IsHiatus = week.is_hiatus,
                                 MediaWeekId = week.media_week_id
                             }).ToList();

                return dto;
            });
        }

        public ProposalDetailProprietaryInventoryDto GetProprietaryProposalDetailInventory(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var pv = context.proposal_version_details.Include(pvd => pvd.proposal_versions.proposal)
                    .Include(pvd =>
                        pvd.proposal_version_detail_quarters.Select(dq => dq.proposal_version_detail_quarter_weeks))
                    .Include(pvd => pvd.proposal_versions.proposal_version_flight_weeks)
                    .Single(pvd => pvd.id == proposalDetailId,
                        string.Format("The proposal detail information you have entered [{0}] does not exist.",
                            proposalDetailId));

                var dto = new ProposalDetailProprietaryInventoryDto();

                _PopulateProposalDetailInventoryBase(pv, dto);

                dto.Weeks = (from quarter in pv.proposal_version_detail_quarters
                             from week in quarter.proposal_version_detail_quarter_weeks
                             orderby week.start_date
                             select new ProposalProprietaryInventoryWeekDto
                             {
                                 QuarterText = string.Format("Q{0}", quarter.quarter),
                                 ImpressionsGoal = week.impressions_goal,
                                 Budget = week.cost,
                                 Week = week.start_date.ToShortDateString(),
                                 IsHiatus = week.is_hiatus,
                                 MediaWeekId = week.media_week_id,
                                 ProposalVersionDetailQuarterWeekId = week.id
                             }).ToList();
                return dto;
            });
        }

        private static void _PopulateProposalDetailInventoryBase(proposal_version_details pvd, ProposalDetailInventoryBase baseDto)
        {
            var pv = pvd.proposal_versions;
            baseDto.ProposalVersionId = pv.id;
            baseDto.Margin = pv.margin;
            baseDto.PostType = (SchedulePostType?)pv.post_type;
            baseDto.GuaranteedAudience = pv.guaranteed_audience_id;
            baseDto.Equivalized = pv.equivalized;
            baseDto.ProposalId = pv.proposal.id;
            baseDto.ProposalVersion = pv.proposal_version;
            baseDto.ProposalName = pv.proposal.name;
            baseDto.ProposalFlightEndDate = pv.end_date;
            baseDto.ProposalFlightStartDate = pv.start_date;
            baseDto.ProposalFlightWeeks = pv.proposal_version_flight_weeks.Where(q => q.proposal_version_id == pv.id)
                .Select(f => new ProposalFlightWeek
                {
                    EndDate = f.end_date,
                    IsHiatus = !f.active,
                    StartDate = f.start_date,
                    MediaWeekId = f.media_week_id
                }).OrderBy(w => w.StartDate).ToList();
            baseDto.DetailId = pvd.id;
            baseDto.DetailDaypartId = pvd.daypart_id;
            baseDto.DetailSpotLengthId = pvd.spot_length_id;
            baseDto.DetailTargetImpressions = pvd.impressions_total;
            baseDto.DetailTargetBudget = pvd.cost_total;
            baseDto.DetailCpm = ProposalMath.CalculateCpm(pvd.cost_total ?? 0, pvd.impressions_total);
            baseDto.DetailFlightEndDate = pvd.end_date;
            baseDto.DetailFlightStartDate = pvd.start_date;
            baseDto.DetailFlightWeeks = pvd.proposal_version_detail_quarters.SelectMany(quarter => quarter
                .proposal_version_detail_quarter_weeks.Select(week =>
                    new ProposalFlightWeek
                    {
                        EndDate = week.end_date,
                        StartDate = week.start_date,
                        IsHiatus = week.is_hiatus,
                        MediaWeekId = week.media_week_id
                    })).OrderBy(w => w.StartDate).ToList();
            baseDto.SingleProjectionBookId = pvd.single_projection_book_id;
            baseDto.ShareProjectionBookId = pvd.share_projection_book_id;
            baseDto.HutProjectionBookId = pvd.hut_projection_book_id;
            baseDto.PlaybackType = (ProposalEnums.ProposalPlaybackType?)pvd.projection_playback_type;
        }

        /// <summary>
        /// Gets the pricing guide data based on a proposal version and a proposal detail
        /// </summary>
        /// <param name="proposalDetailId">Proposal detail to get the pricing data for</param>
        /// <returns>ProposalDetailPricingGuideDto object</returns>
        public PricingGuideDto GetPricingGuideForProposalDetail(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var data = (from distribution in context.pricing_guide_distributions
                                where distribution.proposal_version_detail_id == proposalDetailId
                                select distribution).SingleOrDefault();
                    if (data == null)
                    {
                        return new PricingGuideDto
                        {
                            ProposalDetailId = proposalDetailId,
                            ProposalId = (from version in context.proposal_versions
                                          from detail in version.proposal_version_details
                                          where detail.id == proposalDetailId && version.snapshot_date == null
                                          select version.proposal_id).Single()
                        };
                    }
                    return new PricingGuideDto()
                    {
                        DistributionId = data.id,
                        ProposalId = data.proposal_version_details.proposal_versions.proposal_id,
                        ProposalDetailId = proposalDetailId,
                        MarketCoverageFileId = data.market_coverage_file_id,
                        BudgetGoal = data.goal_budget,
                        ImpressionGoal = data.goal_impression,
                        Margin = data.adjustment_margin,
                        Inflation = data.adjustment_inflation,
                        ImpressionLoss = data.adjustment_impression_loss,
                        OpenMarketPricing = new OpenMarketPricingGuideDto
                        {
                            CpmMin = data.open_market_cpm_min,
                            CpmMax = data.open_market_cpm_max,
                            UnitCapPerStation = data.open_market_unit_cap_per_station,
                            OpenMarketCpmTarget = (OpenMarketCpmTarget?)data.open_market_cpm_target
                        },
                        OpenMarketTotals = new OpenMarketTotalsDto
                        {
                            Cost = data.total_open_market_cost,
                            Coverage = data.total_open_market_coverage,
                            Cpm = data.total_open_market_cpm,
                            Impressions = data.total_open_market_impressions
                        },
                        ProprietaryTotals = new ProprietaryTotalsDto
                        {
                            Cost = data.total_proprietary_cost,
                            Impressions = data.total_proprietary_impressions,
                            Cpm = data.total_proprietary_cpm
                        },
                        ProprietaryPricing = _GetProprietaryPricingsForProposalDetail(proposalDetailId, context)
                    };
                });
        }

        /// <summary>
        /// Gets the proprietary pricings for a specifiv proposal detail
        /// </summary>
        /// <param name="proposalVersionDetailId">Proposal version detail id to filter by</param>
        /// <returns>List of ProprietaryPricingDto objects</returns>
        public List<ProprietaryPricingDto> GetProprietaryPricings(int proposalVersionDetailId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return _GetProprietaryPricingsForProposalDetail(proposalVersionDetailId, context);
                });
        }

        private static List<ProprietaryPricingDto> _GetProprietaryPricingsForProposalDetail(int proposalVersionDetailId, QueryHintBroadcastContext context)
        {
            return (from distribution in context.pricing_guide_distributions
                    from pricings in distribution.pricing_guide_distribution_proprietary_inventory
                    where distribution.proposal_version_detail_id == proposalVersionDetailId
                    select new ProprietaryPricingDto
                    {
                        InventorySource = (InventorySourceEnum)pricings.inventory_source,
                        ImpressionsBalance = pricings.impressions_balance_percent,
                        Cpm = pricings.cpm
                    }).ToList();
        }

        /// <summary>
        /// Saves the pricing guide data and distribution
        /// </summary>
        /// <param name="model">ProposalDetailPricingGuideSaveRequest object</param>
        /// <param name="username">User requesting the save</param>
        /// <returns>ProposalDetailDto object</returns>
        public void SavePricingGuideDistribution(ProposalDetailPricingGuideSave model, string username)
        {
            _InReadUncommitedTransaction(context =>
            {
                var existingDistribution = context.pricing_guide_distributions.Where(x => x.proposal_version_detail_id == model.ProposalDetailId).ToList();
                if (existingDistribution.Any())
                {
                    context.pricing_guide_distributions.RemoveRange(existingDistribution);
                }

                context.pricing_guide_distributions.Add(new pricing_guide_distributions
                {
                    adjustment_inflation = model.Inflation,
                    adjustment_margin = model.Margin,
                    adjustment_impression_loss = model.ImpressionLoss,
                    goal_budget = model.GoalBudget,
                    goal_impression = model.GoalImpression,
                    open_market_cpm_max = model.CpmMax,
                    open_market_cpm_min = model.CpmMin,
                    open_market_cpm_target = (byte?)model.OpenMarketCpmTarget,
                    open_market_unit_cap_per_station = model.UnitCapPerStation,
                    proposal_version_detail_id = model.ProposalDetailId,
                    total_open_market_cost = model.OpenMarketTotals.Cost,
                    total_open_market_coverage = model.OpenMarketTotals.Coverage,
                    total_open_market_cpm = model.OpenMarketTotals.Cpm,
                    total_open_market_impressions = model.OpenMarketTotals.Impressions,
                    total_proprietary_cost = model.ProprietaryTotals.Cost,
                    total_proprietary_cpm = model.ProprietaryTotals.Cpm,
                    total_proprietary_impressions = model.ProprietaryTotals.Impressions,
                    created_by = username,
                    created_date = DateTime.Now,
                    market_coverage_file_id = model.MarketCoverageFileId,
                    pricing_guide_distribution_proprietary_inventory = model.ProprietaryPricing.Select(x => new pricing_guide_distribution_proprietary_inventory
                    {
                        cpm = x.Cpm,
                        impressions_balance_percent = x.ImpressionsBalance,
                        inventory_source = (byte)x.InventorySource
                    }).ToList(),
                    pricing_guide_distribution_open_market_inventory = model.Markets.Select(x => new pricing_guide_distribution_open_market_inventory
                    {
                        manifest_id = x.ProgramId,
                        market_code = (short)x.MarketId,
                        blended_cpm = x.BlendedCpm,
                        cost_per_spot = x.CostPerSpot,
                        daypart_id = x.DaypartId,
                        forecasted_impressions_per_spot = x.ImpressionsPerSpot,
                        program_name = x.ProgramName,
                        spots = x.Spots,
                        station_id = x.StationId,
                        station_impressions_per_spot = x.StationImpressionsPerSpot,
                        station_inventory_manifest_dayparts_id = x.ManifestDaypartId
                    }).ToList()
                });
                context.SaveChanges();
            });
        }

        public List<DistributionProgram> GetDistributionMarkets(int distributionId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return (from distribution in context.pricing_guide_distributions
                        from program in distribution.pricing_guide_distribution_open_market_inventory
                        where distribution.id == distributionId
                        select new DistributionProgram
                        {
                            ProgramId = program.manifest_id,
                            BlendedCpm = program.blended_cpm,
                            CostPerSpot = program.cost_per_spot,
                            ImpressionsPerSpot = program.forecasted_impressions_per_spot,
                            Spots = program.spots,
                            StationImpressionsPerSpot = program.station_impressions_per_spot,
                            ManifestDaypart = new LookupDto
                            {
                                Display = program.program_name,
                                Id = program.station_inventory_manifest_dayparts_id
                            },
                            Market = new LookupDto
                            {
                                Display = program.market.geography_name,
                                Id = program.market_code
                            },
                            Station = new DisplayScheduleStation
                            {
                                StationCode = (short)program.station.station_code.Value,
                                LegacyCallLetters = program.station.legacy_call_letters,
                                Affiliation = program.station.affiliation,
                                CallLetters = program.station.station_call_letters
                            },
                            Daypart = new LookupDto
                            {
                                Display = program.daypart.daypart_text,
                                Id = program.daypart_id
                            },
                            Genres = program.station_inventory_manifest_dayparts.station_inventory_manifest_daypart_genres.Select(x=> new LookupDto {
                                Display = x.genre.name,
                                Id = x.genre_id
                            }).ToList()
                        }).ToList();
            });
        }
    }
}
