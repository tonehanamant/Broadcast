using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using IsolationLevel = System.Transactions.IsolationLevel;
using proposal = EntityFrameworkMapping.Broadcast.proposal;

namespace Services.Broadcast.Repositories
{
    public interface IProposalRepository : IDataRepository
    {
        List<DisplayProposal> GetAllProposals();
        ProposalDto GetProposalById(int proposalId);
        int GetPrimaryProposalVersionNumber(int proposalId);
        short GetLatestProposalVersion(int proposalId);
        List<FlightWeekDto> GetProposalVersionFlightWeeks(int proposalVersionId);
        List<ProposalVersion> GetProposalVersions(int proposalId);
        ProposalDto GetProposalByIdAndVersion(int proposalId, int version);
        void UpdateProposal(ProposalDto proposalDto, string userName);
        void CreateProposal(ProposalDto proposalDto, string userName);
        int GetProposalVersionId(int proposalId, short versionId);
        ProposalDetailProprietaryInventoryDto GetProprietaryProposalDetailInventory(int proposalDetailId);
        List<int> GetProposalDetailQuarterWeekIdsByProposalVersionId(int proposalVersionId);
        ProposalDetailOpenMarketInventoryDto GetOpenMarketProposalDetailInventory(int proposalDetailId);
        ProposalDetailDto GetProposalDetail(int proposalDetailId);
        int GetProposalDetailSpotLengthId(int proposalDetailId);

        void SaveProposalDetailOpenMarketInventoryTotals(int proposalDetailId,
            ProposalDetailSingleInventoryTotalsDto proposalDetailSingleInventoryTotalsDto);

        void SaveProposalDetailProprietaryInventoryTotals(int proposalDetailId,
            ProposalInventoryTotalsDto proposalDetailSingleInventoryTotalsDto);

        ProposalDetailSingleInventoryTotalsDto GetProposalDetailOpenMarketInventoryTotals(int proposalDetailId);
        ProposalDetailSingleInventoryTotalsDto GetProposalDetailProprietaryInventoryTotals(int proposalDetailId);

        void SaveProposalDetailOpenMarketWeekInventoryTotals(
            ProposalDetailOpenMarketInventoryDto proposalDetailProprietaryInventoryDto);

        void SaveProposalDetailProprietaryWeekInventoryTotals(int proposalDetailId,
            ProposalInventoryTotalsDto proposalDetailProprietaryInventoryDto);

        List<ProposalDetailSingleWeekTotalsDto> GetProposalDetailOpenMarketWeekInventoryTotals(int proposalDetailId);
        List<ProposalDetailSingleWeekTotalsDto> GetProposalDetailProprietaryWeekInventoryTotals(int proposalDetailId);
        void UpdateProposalDetailSweepsBooks(int proposalDetailId, int hutBook, int shareBook);
        void UpdateProposalDetailSweepsBook(int proposalDetailId, int book);
        List<ProposalDetailTotalsDto> GetAllProposalDetailsTotals(int proposalVersionId);
        int GetProposalDetailGuaranteedAudienceId(int proposalDetailId);
        void SaveProposalTotals(int proposalVersionId, ProposalHeaderTotalsDto proposalTotals);
        void ResetAllTotals(int proposalId, int proposalVersion);
        void DeleteProposal(int proposalId);
        Dictionary<int, ProposalDto> GetProposalsByQuarterWeeks(List<int> quarterWeekIds);
        List<AffidavitMatchingProposalWeek> GetAffidavitMatchingProposalWeeksByHouseIsci(string isci);
        ProposalDto MapToProposalDto(proposal proposal, proposal_versions proposalVersion);
        void UpdateProposalDetailPostingBooks(List<ProposalDetailPostingData> list);
    }

    public class ProposalRepository : BroadcastRepositoryBase, IProposalRepository
    {

        public ProposalRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public void CreateProposal(ProposalDto proposalDto, string userName)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    // if no id exists it means a new proposal (returns a new proposal id), otherwise a new proposal version will be created
                    if (!proposalDto.Id.HasValue)
                        proposalDto.Id = _CreateProposalHeader(context, proposalDto, userName);

                    // create new proposal version
                    var proposalVersionId = _CreateProposalVersion(context, proposalDto, userName);

                    // set primary version
                    _SetProposalPrimaryVersion(context, proposalDto.Id.Value, proposalVersionId, userName);
                    proposalDto.PrimaryVersionId = proposalVersionId;
                    proposalDto.VersionId = proposalVersionId;

                    // deal with proposal version spot length
                    _SaveProposalSpotLength(context, proposalVersionId, proposalDto.SpotLengths);

                    // deal with flights
                    _SaveProposalVersionFlightWeeks(context, proposalVersionId, proposalDto.FlightWeeks);

                    // deal with secondary demos
                    _SaveProposalVersionSecondaryDemos(context, proposalVersionId, proposalDto.SecondaryDemos);

                    // deal with proposal details
                    _SaveProposalVersionDetails(context, proposalVersionId, proposalDto.Details);

                    // deal with proposal markets
                    _SaveProposalVersionMarkets(context, proposalVersionId, proposalDto.Markets);

                    context.SaveChanges();
                });
        }

        private static int _CreateProposalHeader(BroadcastContext context, ProposalDto proposalDto, string userName)
        {
            var timestamp = DateTime.Now;
            var dbProposal = new proposal
            {
                name = proposalDto.ProposalName,
                advertiser_id = proposalDto.AdvertiserId,
                modified_by = userName,
                modified_date = timestamp,
                created_by = userName,
                created_date = timestamp,
                primary_version_id = 1
            };
            context.proposals.Add(dbProposal);
            context.SaveChanges();

            return dbProposal.id;
        }

        private static int _CreateProposalVersion(BroadcastContext context, ProposalDto proposalDto, string userName)
        {
            var timestamp = DateTime.Now;
            var maxProposalVersion = context.proposal_versions.Where(q => q.proposal_id == proposalDto.Id.Value)
                .Select(v => v.proposal_version)
                .DefaultIfEmpty()
                .Max();

            var dbProposalVersion = new proposal_versions
            {
                proposal_id = proposalDto.Id.Value,
                proposal_version = ++maxProposalVersion,
                start_date = proposalDto.FlightStartDate,
                end_date = proposalDto.FlightEndDate,
                guaranteed_audience_id = proposalDto.GuaranteedDemoId,
                markets = (byte)proposalDto.MarketGroupId,
                blackout_markets = (byte?)proposalDto.BlackoutMarketGroupId,
                created_by = userName,
                created_date = timestamp,
                modified_by = userName,
                modified_date = timestamp,
                target_budget = proposalDto.TargetBudget,
                target_impressions = proposalDto.TargetImpressions,
                target_cpm = proposalDto.TargetCPM.Value,
                margin = proposalDto.Margin.Value,
                target_units = proposalDto.TargetUnits,
                notes = proposalDto.Notes,
                post_type = (byte)proposalDto.PostType,
                equivalized = proposalDto.Equivalized,
                status = (byte)proposalDto.Status
            };

            context.proposal_versions.Add(dbProposalVersion);

            context.SaveChanges();

            return dbProposalVersion.id;
        }

        public void UpdateProposal(ProposalDto proposalDto, string userName)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    // deal with proposal headers
                    _UpdateProposalHeader(context, proposalDto, userName);

                    // deal with proposal version fields
                    var proposalVersionId = _UpdateProposalVersion(context, proposalDto, userName);

                    // set primary version
                    _SetProposalPrimaryVersion(context, proposalDto.Id.Value, proposalVersionId, userName);
                    proposalDto.PrimaryVersionId = proposalVersionId;

                    // deal with proposal version spot length
                    _SaveProposalSpotLength(context, proposalVersionId, proposalDto.SpotLengths);

                    // deal with flights
                    _SaveProposalVersionFlightWeeks(context, proposalVersionId, proposalDto.FlightWeeks);

                    // deal with secondary demos
                    _SaveProposalVersionSecondaryDemos(context, proposalVersionId, proposalDto.SecondaryDemos);

                    // deal with proposal details
                    _UpdateProposalVersionDetails(context, proposalVersionId, proposalDto.Details);

                    // deal with proposal markets
                    _SaveProposalVersionMarkets(context, proposalVersionId, proposalDto.Markets);

                    context.SaveChanges();
                });
        }

        private static void _SaveProposalSpotLength(BroadcastContext context, int proposalVersionId,
            List<LookupDto> spotLengths)
        {
            context.proposal_version_spot_length.RemoveRange(
                context.proposal_version_spot_length.Where(q => q.proposal_version_id == proposalVersionId));

            if (spotLengths != null)
                context.proposal_version_spot_length.AddRange(
                    spotLengths.Select((s) => new proposal_version_spot_length
                    {
                        proposal_version_id = proposalVersionId,
                        spot_length_id = s.Id
                    }).ToList());

            context.SaveChanges();
        }

        private static void _UpdateProposalHeader(BroadcastContext context, ProposalDto proposalDto, string userName)
        {
            var timestamp = DateTime.Now;
            var dbProposal = context.proposals.Single(p => p.id == proposalDto.Id.Value,
                string.Format("Cannot find proposal {0}", proposalDto.Id.Value));
            dbProposal.name = proposalDto.ProposalName;
            dbProposal.advertiser_id = proposalDto.AdvertiserId;
            dbProposal.modified_by = userName;
            dbProposal.modified_date = timestamp;

            context.SaveChanges();
        }

        private static int _UpdateProposalVersion(BroadcastContext context, ProposalDto proposalDto, string userName)
        {
            var timestamp = DateTime.Now;

            var dbProposalVersion =
                context.proposal_versions.Single(
                    pv => pv.proposal_id == proposalDto.Id.Value && pv.proposal_version == proposalDto.Version,
                    string.Format("Cannot find proposal version {0}", proposalDto.Version));

            dbProposalVersion.start_date = proposalDto.FlightStartDate;
            dbProposalVersion.end_date = proposalDto.FlightEndDate;
            dbProposalVersion.guaranteed_audience_id = proposalDto.GuaranteedDemoId;
            dbProposalVersion.markets = (byte)proposalDto.MarketGroupId;
            dbProposalVersion.blackout_markets = (byte?)proposalDto.BlackoutMarketGroupId;
            dbProposalVersion.modified_by = userName;
            dbProposalVersion.modified_date = timestamp;
            dbProposalVersion.target_budget = proposalDto.TargetBudget;
            dbProposalVersion.target_impressions = proposalDto.TargetImpressions;
            dbProposalVersion.target_cpm = proposalDto.TargetCPM.Value;
            dbProposalVersion.margin = proposalDto.Margin.Value;
            dbProposalVersion.target_units = proposalDto.TargetUnits;
            dbProposalVersion.notes = proposalDto.Notes;
            dbProposalVersion.post_type = (byte)proposalDto.PostType;
            dbProposalVersion.equivalized = proposalDto.Equivalized;
            dbProposalVersion.status = (byte)proposalDto.Status;

            context.SaveChanges();

            return dbProposalVersion.id;
        }

        private static void _SaveProposalVersionMarkets(BroadcastContext context, int proposalVersionId,
            IEnumerable<ProposalMarketDto> markets)
        {
            context.proposal_version_markets.RemoveRange(
                context.proposal_version_markets.Where(q => q.proposal_version_id == proposalVersionId));

            if (markets != null)
                context.proposal_version_markets.AddRange(
                    markets.Select(market =>
                        new proposal_version_markets
                        {
                            proposal_version_id = proposalVersionId,
                            market_code = market.Id,
                            is_blackout = market.IsBlackout
                        }).ToList());

            context.SaveChanges();
        }

        private static void _SaveProposalVersionSecondaryDemos(BroadcastContext context, int proposalVersionId,
            List<int> secondaryDemos)
        {
            context.proposal_version_audiences.RemoveRange(
                context.proposal_version_audiences.Where(q => q.proposal_version_id == proposalVersionId));

            // user can update secondary demos, including saving none
            if (secondaryDemos != null)
                context.proposal_version_audiences.AddRange(
                    secondaryDemos.Select(
                        (f, index) =>
                            new proposal_version_audiences
                            {
                                audience_id = f,
                                proposal_version_id = proposalVersionId,
                                rank = (byte)index
                            }).ToList());

            context.SaveChanges();
        }

        private static void _SaveProposalVersionDetails(BroadcastContext context, int proposalVersionId,
            List<ProposalDetailDto> proposalDetails)
        {
            if (proposalDetails == null || !proposalDetails.Any()) return;

            context.proposal_version_details.AddRange(
                proposalDetails.Select(proposalDetail => new proposal_version_details
                {
                    sequence = proposalDetail.Sequence,
                    cost_total = proposalDetail.TotalCost,
                    daypart_code = proposalDetail.DaypartCode,
                    proposal_version_id = proposalVersionId,
                    spot_length_id = proposalDetail.SpotLengthId,
                    units_total = proposalDetail.TotalUnits,
                    impressions_total = (proposalDetail.TotalImpressions),
                    start_date = proposalDetail.FlightStartDate,
                    end_date = proposalDetail.FlightEndDate,
                    daypart_id = proposalDetail.DaypartId,
                    adu = proposalDetail.Adu,
                    single_projection_book_id = proposalDetail.SingleProjectionBookId,
                    hut_projection_book_id = proposalDetail.HutProjectionBookId,
                    share_projection_book_id = proposalDetail.ShareProjectionBookId,
                    projection_playback_type = (byte)proposalDetail.ProjectionPlaybackType,
                    posting_book_id = proposalDetail.PostingBookId,
                    posting_playback_type = (byte?)proposalDetail.PostingPlaybackType,
                    proposal_version_detail_criteria_genres = proposalDetail.GenreCriteria.Select(g => new proposal_version_detail_criteria_genres()
                    {
                        genre_id = g.Genre.Id,
                        contain_type = (byte)g.Contain
                    }).ToList(),
                    proposal_version_detail_criteria_show_types = proposalDetail.ShowTypeCriteria.Select(st => new proposal_version_detail_criteria_show_types {
                        contain_type = (byte)st.Contain,
                        show_type_id = st.ShowType.Id
                    }).ToList(),
                    proposal_version_detail_criteria_programs = proposalDetail.ProgramCriteria.Select(p => new proposal_version_detail_criteria_programs()
                    {
                        program_name = p.Program.Display,
                        program_name_id = p.Program.Id,
                        contain_type = (byte)p.Contain
                    }).ToList(),
                    proposal_version_detail_quarters =
                        proposalDetail.Quarters.Select(quarter => new proposal_version_detail_quarters
                        {
                            cpm = quarter.Cpm,
                            impressions_goal = (quarter.ImpressionGoal),
                            quarter = quarter.Quarter,
                            year = quarter.Year,
                            proposal_version_detail_quarter_weeks =
                                quarter.Weeks.Select(
                                    quarterWeek => new proposal_version_detail_quarter_weeks
                                    {
                                        is_hiatus = quarterWeek.IsHiatus,
                                        cost = quarterWeek.Cost,
                                        impressions_goal = quarterWeek.Impressions,
                                        units = quarterWeek.Units,
                                        media_week_id = quarterWeek.MediaWeekId,
                                        end_date = quarterWeek.EndDate,
                                        start_date = quarterWeek.StartDate,
                                        myevents_report_name = quarterWeek.MyEventsReportName,
                                        proposal_version_detail_quarter_week_iscis = quarterWeek.Iscis.Select(isic =>
                                            new proposal_version_detail_quarter_week_iscis
                                            {
                                                client_isci = isic.ClientIsci,
                                                house_isci = isic.HouseIsci,
                                                brand = isic.Brand,
                                                married_house_iscii = isic.MarriedHouseIsci,
                                                monday = isic.Monday,
                                                tuesday = isic.Tuesday,
                                                wednesday = isic.Wednesday,
                                                thursday = isic.Thursday,
                                                friday = isic.Friday,
                                                saturday = isic.Saturday,
                                                sunday = isic.Sunday
                                            }).ToList()
                                    }).ToList()
                        }).ToList()
                }).ToList());

            context.SaveChanges();
        }

        public List<int> GetProposalDetailQuarterWeekIdsByProposalVersionId(int proposalVersionId)
        {
            return _InReadUncommitedTransaction(
                context => (from p in context.proposal_version_details
                            join q in context.proposal_version_detail_quarters on p.id equals q.proposal_version_detail_id
                            join w in context.proposal_version_detail_quarter_weeks on q.id equals
                                w.proposal_version_quarter_id
                            where p.proposal_version_id == proposalVersionId
                            select w.id).ToList());
        }

        private static void _UpdateProposalVersionDetails(BroadcastContext context, int proposalVersionId,
            List<ProposalDetailDto> proposalDetails)
        {
            //for debugging only:
            //context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
            // remove only details that the user has deleted in the grid
            var proposalVersionDetails =
                context.proposal_version_details.Where(a => a.proposal_version_id == proposalVersionId)
                    .Include(b => b.proposal_version_detail_quarters);

            var excludedDetailsIds = proposalVersionDetails.Select(b => b.id)
                .Except(proposalDetails.Where(a => a.Id.HasValue).Select(b => b.Id.Value).ToList())
                .ToList();

            // remove details only if user has actually deleted from grid
            context.proposal_version_details.RemoveRange(
                context.proposal_version_details.Where(a => a.proposal_version_id == proposalVersionId &&
                                                            excludedDetailsIds.Any(b => b == a.id)));

            // new details have been added
            _SaveProposalVersionDetails(context, proposalVersionId, proposalDetails.Where(a => a.Id == null).ToList());

            // deal with details that have been updated
            proposalDetails.Where(a => a.Id != null).ForEach(detail =>
            {
                var updatedDetail = proposalVersionDetails.FirstOrDefault(b => b.id == detail.Id);
                if (updatedDetail != null)
                {
                    updatedDetail.cost_total = detail.TotalCost;
                    updatedDetail.daypart_code = detail.DaypartCode;
                    updatedDetail.spot_length_id = detail.SpotLengthId;
                    updatedDetail.units_total = detail.TotalUnits;
                    updatedDetail.impressions_total = (detail.TotalImpressions);
                    updatedDetail.start_date = detail.FlightStartDate;
                    updatedDetail.end_date = detail.FlightEndDate;
                    updatedDetail.daypart_id = detail.DaypartId;
                    updatedDetail.adu = detail.Adu;
                    updatedDetail.single_projection_book_id = detail.SingleProjectionBookId;
                    updatedDetail.hut_projection_book_id = detail.HutProjectionBookId;
                    updatedDetail.share_projection_book_id = detail.ShareProjectionBookId;
                    updatedDetail.projection_playback_type = (byte)detail.ProjectionPlaybackType;
                    updatedDetail.posting_book_id = detail.PostingBookId;
                    updatedDetail.posting_playback_type = (byte?)detail.PostingPlaybackType;
                    updatedDetail.sequence = detail.Sequence;

                    //update proposal detail genre criteria
                    context.proposal_version_detail_criteria_genres.RemoveRange(
                        context.proposal_version_detail_criteria_genres.Where(g => g.proposal_version_detail_id == detail.Id));
                    if (detail.GenreCriteria != null && detail.GenreCriteria.Count > 0)
                        context.proposal_version_detail_criteria_genres.AddRange(
                            detail.GenreCriteria.Select(
                                g => new proposal_version_detail_criteria_genres()
                                {
                                    genre_id = g.Genre.Id,
                                    contain_type = (byte)g.Contain,
                                    proposal_version_detail_id = detail.Id.Value
                                }));

                    //update proposal detail show type criteria
                    context.proposal_version_detail_criteria_show_types.RemoveRange(
                        context.proposal_version_detail_criteria_show_types.Where(g => g.proposal_version_detail_id == detail.Id));
                    if (detail.ShowTypeCriteria != null && detail.ShowTypeCriteria?.Count > 0)
                        context.proposal_version_detail_criteria_show_types.AddRange(
                            detail.ShowTypeCriteria.Select(
                                g => new proposal_version_detail_criteria_show_types()
                                {
                                    show_type_id = g.ShowType.Id,
                                    contain_type = (byte)g.Contain,
                                    proposal_version_detail_id = detail.Id.Value
                                }));

                    //update proposal detail program name criteria
                    context.proposal_version_detail_criteria_programs.RemoveRange(
                        context.proposal_version_detail_criteria_programs.Where(g => g.proposal_version_detail_id == detail.Id));
                    if (detail.ProgramCriteria != null && detail.ProgramCriteria?.Count > 0)
                        context.proposal_version_detail_criteria_programs.AddRange(
                            detail.ProgramCriteria.Select(
                                p => new proposal_version_detail_criteria_programs()
                                {
                                    program_name = p.Program.Display,
                                    program_name_id = p.Program.Id,
                                    contain_type = (byte)p.Contain,
                                    proposal_version_detail_id = detail.Id.Value
                                }));
                    
                    // deal with quarters that have been deleted 
                    // scenario where user maintain the detail but change completely the flight generating new quarters for this particular detail
                    var deletedQuartersIds =
                        updatedDetail.proposal_version_detail_quarters.Select(a => a.id)
                            .Except(detail.Quarters.Where(b => b.Id != null).Select(q => q.Id.Value))
                            .ToList();

                    context.proposal_version_detail_quarters.RemoveRange(
                        context.proposal_version_detail_quarters.Where(a => deletedQuartersIds.Any(b => b == a.id)));

                    // deal with new added detail quarters
                    var newDetailQuarters =
                        _GetProposalVersionDetailQuarters(detail.Quarters.Where(b => b.Id == null).ToList());
                    newDetailQuarters.ForEach(n => updatedDetail.proposal_version_detail_quarters.Add(n));

                    // detail with detail quarters that have been updated
                    detail.Quarters.Where(b => b.Id != null).ForEach(detailQuarter =>
                    {
                        var updatedQuarter =
                            updatedDetail.proposal_version_detail_quarters.FirstOrDefault(q =>
                                q.id == detailQuarter.Id);

                        if (updatedQuarter != null)
                        {
                            updatedQuarter.cpm = detailQuarter.Cpm;
                            updatedQuarter.impressions_goal = (detailQuarter.ImpressionGoal);
                            updatedQuarter.quarter = detailQuarter.Quarter;
                            updatedQuarter.year = detailQuarter.Year;

                            var excludedMediaWeeks =
                                updatedQuarter.proposal_version_detail_quarter_weeks.Select(a => a.id)
                                    .Except(detailQuarter.Weeks.Where(a => a.Id != null).Select(v => v.Id.Value))
                                    .ToList();

                            // delete mediaweeks
                            context.proposal_version_detail_quarter_weeks.RemoveRange(
                                context.proposal_version_detail_quarter_weeks.Where(
                                    a =>
                                        a.proposal_version_quarter_id == detailQuarter.Id &&
                                        excludedMediaWeeks.Any(b => b == a.id)));

                            // add new weeks
                            var newQuarterWeeks =
                                _GetProposalVersionDetailQuarterWeeks(detailQuarter.Weeks.Where(z => z.Id == null)
                                    .ToList());
                            newQuarterWeeks.ForEach(q => updatedQuarter.proposal_version_detail_quarter_weeks.Add(q));

                            // update matching weeks
                            updatedQuarter.proposal_version_detail_quarter_weeks.ForEach(quarterWeek =>
                            {
                                var detatilQuarterWeek =
                                    detailQuarter.Weeks.Find(w => w.MediaWeekId == quarterWeek.media_week_id);

                                if (detatilQuarterWeek != null)
                                {
                                    quarterWeek.start_date = detatilQuarterWeek.StartDate;
                                    quarterWeek.end_date = detatilQuarterWeek.EndDate;
                                    quarterWeek.is_hiatus = detatilQuarterWeek.IsHiatus;
                                    quarterWeek.units = detatilQuarterWeek.Units;
                                    quarterWeek.impressions_goal = detatilQuarterWeek.Impressions;
                                    quarterWeek.cost = detatilQuarterWeek.Cost;
                                    quarterWeek.myevents_report_name = detatilQuarterWeek.MyEventsReportName;

                                    _UpdateProposalWeekIscis(context, detatilQuarterWeek, quarterWeek);
                                }
                            });
                        }
                    });
                }
            });

            context.SaveChanges();
        }

        private static void _UpdateProposalWeekIscis(BroadcastContext context, ProposalWeekDto proposalWeek,
            proposal_version_detail_quarter_weeks quarterWeek)
        {
            context.proposal_version_detail_quarter_week_iscis.RemoveRange(
                quarterWeek.proposal_version_detail_quarter_week_iscis);

            var newIscis = _GetProposalDetailQuarterWeekIscis(proposalWeek.Iscis).ToList();

            newIscis.ForEach(q => quarterWeek.proposal_version_detail_quarter_week_iscis.Add(q));
        }

        private static IEnumerable<proposal_version_detail_quarter_week_iscis> _GetProposalDetailQuarterWeekIscis(
            List<ProposalWeekIsciDto> proposalWeekIsciDtos)
        {
            var weekIscis = new List<proposal_version_detail_quarter_week_iscis>();

            if (!proposalWeekIsciDtos.Any())
                return weekIscis;

            var newWeekIscis = proposalWeekIsciDtos.Select(
                isci => new proposal_version_detail_quarter_week_iscis
                {
                    brand = isci.Brand,
                    client_isci = isci.ClientIsci,
                    house_isci = isci.HouseIsci,
                    married_house_iscii = isci.MarriedHouseIsci,
                    monday = isci.Monday,
                    tuesday = isci.Tuesday,
                    wednesday = isci.Wednesday,
                    thursday = isci.Thursday,
                    friday = isci.Friday,
                    saturday = isci.Saturday,
                    sunday = isci.Sunday
                }).ToList();

            weekIscis.AddRange(newWeekIscis);

            return weekIscis;
        }

        private static List<proposal_version_detail_quarters> _GetProposalVersionDetailQuarters(
            List<ProposalQuarterDto> proposalQuarterDtos)
        {
            var detailQuarters = new List<proposal_version_detail_quarters>();
            if (!proposalQuarterDtos.Any()) return detailQuarters;

            var newDetailQuarters =
                proposalQuarterDtos.Select(quarter => new proposal_version_detail_quarters
                {
                    cpm = quarter.Cpm,
                    impressions_goal = (quarter.ImpressionGoal),
                    quarter = quarter.Quarter,
                    year = quarter.Year,
                    proposal_version_detail_quarter_weeks =
                        quarter.Weeks.Select(
                            quarterWeek => new proposal_version_detail_quarter_weeks
                            {
                                is_hiatus = quarterWeek.IsHiatus,
                                cost = quarterWeek.Cost,
                                impressions_goal = (quarterWeek.Impressions),
                                units = quarterWeek.Units,
                                media_week_id = quarterWeek.MediaWeekId,
                                end_date = quarterWeek.EndDate,
                                start_date = quarterWeek.StartDate
                            }).ToList()
                }).ToList();

            detailQuarters.AddRange(newDetailQuarters);
            return detailQuarters;
        }

        private static List<proposal_version_detail_quarter_weeks> _GetProposalVersionDetailQuarterWeeks(
            List<ProposalWeekDto> proposalWeekDtos)
        {
            var quarterWeeks = new List<proposal_version_detail_quarter_weeks>();
            if (!proposalWeekDtos.Any()) return quarterWeeks;

            var newQuarterWeeks = proposalWeekDtos.Select(
                quarterWeek => new proposal_version_detail_quarter_weeks
                {
                    is_hiatus = quarterWeek.IsHiatus,
                    cost = quarterWeek.Cost,
                    impressions_goal = (quarterWeek.Impressions),
                    units = quarterWeek.Units,
                    media_week_id = quarterWeek.MediaWeekId,
                    end_date = quarterWeek.EndDate,
                    start_date = quarterWeek.StartDate
                }).ToList();
            quarterWeeks.AddRange(newQuarterWeeks);

            return quarterWeeks;
        }

        private static void _SaveProposalVersionFlightWeeks(BroadcastContext context, int proposalVersionId,
            List<ProposalFlightWeek> flightWeeks)
        {
            context.proposal_version_flight_weeks.RemoveRange(
                context.proposal_version_flight_weeks.Where(q => q.proposal_version_id == proposalVersionId));

            // user might have removed details and no flight weeks is saved
            if (flightWeeks != null)
                context.proposal_version_flight_weeks.AddRange(
                    flightWeeks.Select(f => new proposal_version_flight_weeks
                    {
                        proposal_version_id = proposalVersionId,
                        active = !f.IsHiatus,
                        start_date = f.StartDate,
                        end_date = f.EndDate,
                        media_week_id = f.MediaWeekId
                    }));

            context.SaveChanges();
        }

        private static void _SetProposalPrimaryVersion(BroadcastContext context, int proposalId, int primaryVersionId,
            string userName)
        {
            var proposalToUpdate = context.proposals.Single(q => q.id == proposalId,
                string.Format("Cannot find proposal {0}.", proposalId));

            var proposalVersions = proposalToUpdate.proposal_versions.Where(pv =>
                pv.status == (int)ProposalEnums.ProposalStatusType.AgencyOnHold ||
                pv.status == (int)ProposalEnums.ProposalStatusType.Contracted).ToList();
            proposalToUpdate.primary_version_id =
                proposalVersions.Any() ? proposalVersions.First().id : primaryVersionId;

            //set primary version id to whichever version is agency on hold or contracted
            proposalToUpdate.modified_by = userName;
            proposalToUpdate.modified_date = DateTime.Now;

            context.SaveChanges();
        }

        public List<DisplayProposal> GetAllProposals()
        {
            return _InReadUncommitedTransaction(
                context => (from p in context.proposals
                            join v in context.proposal_versions on p.id equals v.proposal_id
                            where p.primary_version_id == v.id
                            select new DisplayProposal
                            {
                                Id = p.id,
                                ProposalName = p.name,
                                Advertiser = new LookupDto { Id = p.advertiser_id },
                                FlightEndDate = v.end_date,
                                FlightStartDate = v.start_date,
                                LastModified = v.modified_date,
                                Owner = v.created_by,
                                Status = (ProposalEnums.ProposalStatusType)v.status,
                                Flights = context.proposal_version_flight_weeks.Where(q => q.proposal_version_id == v.id)
                                    .Select(f => new FlightWeekDto
                                    {
                                        Id = f.id,
                                        StartDate = f.start_date,
                                        EndDate = f.end_date,
                                        IsHiatus = !f.active
                                    }).ToList()
                            }).ToList());
        }

        public int GetProposalVersionId(int proposalId, short versionId)
        {
            return _InReadUncommitedTransaction(
                context =>
                    context.proposal_versions.Single(
                        q => q.proposal_id == proposalId && q.proposal_version == versionId,
                        string.Format("Cannot find proposal version {0}-{1}.", proposalId, versionId)).id);
        }

        public ProposalDto GetProposalById(int proposalId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from p in context.proposals
                            join v in context.proposal_versions on p.id equals v.proposal_id
                            where p.primary_version_id == v.id
                                  && p.id == proposalId
                            select new { p, v })
                        .ToList().Select(pv => MapToProposalDto(pv.p, pv.v))
                        .Single(string.Format(
                            "The Proposal information you have entered [{0}] does not exist. Please try again.",
                            proposalId));
                });
        }

        public int GetPrimaryProposalVersionNumber(int proposalId)
        {
            return _InReadUncommitedTransaction(
                context => (from p in context.proposals
                            join v in context.proposal_versions on p.id equals v.proposal_id
                            where p.primary_version_id == v.id
                                  && p.id == proposalId
                            select v.proposal_version)
                    .Single(string.Format("Cannot find primary version number for proposal {0}.", proposalId)));
        }

        public ProposalDto GetProposalByIdAndVersion(int proposalId, int version)
        {
            return _InReadUncommitedTransaction(
                context => (from p in context.proposals
                            join v in context.proposal_versions on p.id equals v.proposal_id
                            where p.id == proposalId && v.proposal_version == version
                            select new { p, v })
                    .ToList().Select(pv => MapToProposalDto(pv.p, pv.v))
                    .Single(string.Format("Cannot find version {0} for proposal {1}.", version, proposalId)));
        }

        public ProposalDto MapToProposalDto(proposal proposal, proposal_versions proposalVersion)
        {
            var proposalDto = new ProposalDto
            {
                Id = proposalVersion.proposal.id,
                ProposalName = proposalVersion.proposal.name,
                AdvertiserId = proposalVersion.proposal.advertiser_id,
                FlightEndDate = proposalVersion.end_date,
                FlightStartDate = proposalVersion.start_date,
                GuaranteedDemoId = proposalVersion.guaranteed_audience_id,
                MarketGroupId = (ProposalEnums.ProposalMarketGroups)proposalVersion.markets,
                BlackoutMarketGroupId = (ProposalEnums.ProposalMarketGroups?)proposalVersion.blackout_markets,
                Markets = proposalVersion.proposal_version_markets
                .Where(q => q.proposal_version_id == proposalVersion.id).Select(f => new ProposalMarketDto
                {
                    Id = f.market_code,
                    Display = f.market.geography_name,
                    IsBlackout = f.is_blackout
                }).ToList(),
                Status = (ProposalEnums.ProposalStatusType)proposalVersion.status,
                TargetUnits = proposalVersion.target_units,
                TargetBudget = proposalVersion.target_budget,
                TargetImpressions = proposalVersion.target_impressions,
                TargetCPM = proposalVersion.target_cpm,
                TotalCost = proposalVersion.cost_total,
                TotalImpressions = proposalVersion.impressions_total,
                TotalCPM =
                ProposalMath.CalculateCpm(proposalVersion.cost_total, proposalVersion.impressions_total),
                Margin = proposalVersion.margin,
                Notes = proposalVersion.notes,
                Version = proposalVersion.proposal_version,
                VersionId = proposalVersion.id,
                PrimaryVersionId = proposal.primary_version_id,
                FlightWeeks =
                proposalVersion.proposal_version_flight_weeks.Where(q => q.proposal_version_id == proposalVersion.id)
                    .Select(f => new ProposalFlightWeek
                    {
                        EndDate = f.end_date,
                        IsHiatus = !f.active,
                        StartDate = f.start_date,
                        MediaWeekId = f.media_week_id
                    }).OrderBy(w => w.StartDate).ToList(),
                Equivalized = proposalVersion.equivalized,
                SecondaryDemos =
                proposalVersion.proposal_version_audiences.OrderBy(r => r.rank).Select(a => a.audience_id).ToList(),
                PostType = (SchedulePostType)proposalVersion.post_type,
                SpotLengths =
                proposalVersion.proposal_version_spot_length.Select(a => new LookupDto { Id = a.spot_length_id })
                    .ToList(),
                Details = proposalVersion.proposal_version_details.Select(version => new ProposalDetailDto
                {
                    Id = version.id,
                    Sequence = version.sequence,
                    DaypartCode = version.daypart_code,
                    SpotLengthId = version.spot_length_id,
                    TotalCost = version.cost_total.GetValueOrDefault(),
                    TotalImpressions = version.impressions_total,
                    TotalUnits = version.units_total.GetValueOrDefault(),
                    FlightEndDate = version.end_date,
                    FlightStartDate = version.start_date,
                    DaypartId = version.daypart_id,
                    Adu = version.adu,
                    SingleProjectionBookId = version.single_projection_book_id,
                    ShareProjectionBookId = version.share_projection_book_id,
                    HutProjectionBookId = version.hut_projection_book_id,
                    ProjectionPlaybackType = (ProposalEnums.ProposalPlaybackType)version.projection_playback_type,
                    PostingBookId = version.posting_book_id,
                    PostingPlaybackType = (ProposalEnums.ProposalPlaybackType?)version.posting_playback_type,
                    GenreCriteria = version.proposal_version_detail_criteria_genres.Select(c => new GenreCriteria()
                    {
                        Id = c.id,
                        Contain = (ContainTypeEnum)c.contain_type,
                        Genre = new LookupDto { Id = c.genre.id, Display = c.genre.name }
                    }).ToList(),
                    ShowTypeCriteria = version.proposal_version_detail_criteria_show_types.Select(st => new ShowTypeCriteria {
                        Id = st.id,
                        Contain = (ContainTypeEnum)st.contain_type,
                        ShowType = new LookupDto { Id = st.show_types.id, Display = st.show_types.name }
                    }).ToList(),
                    ProgramCriteria = version.proposal_version_detail_criteria_programs.Select(p => new ProgramCriteria()
                    {
                        Id = p.id,
                        Contain = (ContainTypeEnum)p.contain_type,
                        Program = new LookupDto
                        {
                            Id = p.program_name_id,
                            Display = p.program_name
                        }
                    }).ToList(),
                    Quarters = version.proposal_version_detail_quarters.Select(quarter => new ProposalQuarterDto
                    {
                        Cpm = quarter.cpm,
                        ImpressionGoal = quarter.impressions_goal,
                        Id = quarter.id,
                        Year = quarter.year,
                        Quarter = quarter.quarter,
                        QuarterText = string.Format("{0} Q{1}", quarter.year, quarter.quarter),
                        Weeks = quarter.proposal_version_detail_quarter_weeks.Select(week => new ProposalWeekDto
                        {
                            Id = week.id,
                            Cost = week.cost,
                            EndDate = week.end_date,
                            StartDate = week.start_date,
                            Impressions = week.impressions_goal,
                            IsHiatus = week.is_hiatus,
                            Units = week.units,
                            MediaWeekId = week.media_week_id,
                            Week = week.start_date.ToShortDateString(),
                            MyEventsReportName = week.myevents_report_name,
                            Iscis = week.proposal_version_detail_quarter_week_iscis.Select(isci => new ProposalWeekIsciDto
                            {
                                Id = isci.id,
                                Brand = isci.brand,
                                ClientIsci = isci.client_isci,
                                HouseIsci = isci.house_isci,
                                MarriedHouseIsci = isci.married_house_iscii,
                                Monday = isci.monday == null ? false : isci.monday.Value,
                                Tuesday = isci.tuesday == null ? false : isci.tuesday.Value,
                                Wednesday = isci.wednesday == null ? false : isci.wednesday.Value,
                                Thursday = isci.thursday == null ? false : isci.thursday.Value,
                                Friday = isci.friday == null ? false : isci.friday.Value,
                                Saturday = isci.saturday == null ? false : isci.saturday.Value,
                                Sunday = isci.sunday == null ? false : isci.sunday.Value
                            }).ToList()
                        }).OrderBy(w => w.StartDate).ToList()
                        }).OrderBy(q => q.Year).ThenBy(q => q.Quarter).ToList()
                        }).OrderBy(d => d.Sequence).ToList()
                };

            return proposalDto;
        }

        public short GetLatestProposalVersion(int proposalId)
        {
            return _InReadUncommitedTransaction(
                context => (from x in context.proposal_versions
                            where x.proposal_id == proposalId
                            select x).Max(q => q.proposal_version));
        }

        private static ProposalVersion _MapToProposalVersion(proposal_versions x)
        {
            var proposalVersion = new ProposalVersion();

            proposalVersion.Id = x.id;
            proposalVersion.VersionNumber = x.proposal_version;
            proposalVersion.ProposalId = x.proposal_id;
            proposalVersion.AdvertiserId = x.proposal.advertiser_id;
            proposalVersion.Budget = x.target_budget;
            proposalVersion.StartDate = x.start_date;
            proposalVersion.EndDate = x.end_date;
            proposalVersion.GuaranteedAudienceId = x.guaranteed_audience_id;
            proposalVersion.LastModifiedBy = x.modified_by;
            proposalVersion.LastModifiedDate = x.modified_date;
            proposalVersion.Markets = (ProposalEnums.ProposalMarketGroups)x.markets;
            proposalVersion.TargetUnits = x.target_units;
            proposalVersion.TargetImpressions = x.target_impressions;
            proposalVersion.Notes = x.notes;
            proposalVersion.Primary = x.id == x.proposal.primary_version_id;
            proposalVersion.Status = (ProposalEnums.ProposalStatusType)x.status;

            return proposalVersion;
        }

        public List<FlightWeekDto> GetProposalVersionFlightWeeks(int proposalVersionId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return (from pf in context.proposal_version_flight_weeks
                            where pf.proposal_version_id == proposalVersionId
                            select new FlightWeekDto
                            {
                                Id = pf.media_week_id,
                                IsHiatus = !pf.active,
                                StartDate = pf.start_date,
                                EndDate = pf.end_date
                            }).OrderBy(w => w.StartDate).ToList();
                });
        }

        public List<ProposalVersion> GetProposalVersions(int proposalId)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context => (from x in context.proposal_versions
                                where x.proposal_id == proposalId
                                select x).ToList().Select(_MapToProposalVersion).ToList());
            }
        }

        public int SaveProposalDetailQuarter(int proposalDetailId, ProposalQuarterDto quarter)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var proposalDetailQuarter = new proposal_version_detail_quarters
                    {
                        proposal_version_detail_id = proposalDetailId,
                        cpm = quarter.Cpm,
                        impressions_goal = quarter.ImpressionGoal,
                        quarter = quarter.Quarter,
                        year = quarter.Year
                    };

                    context.proposal_version_detail_quarters.Add(proposalDetailQuarter);
                    context.SaveChanges();

                    return proposalDetailQuarter.id;
                });
        }

        public void SaveProposalDetailQuarterWeek(int quarterId, ProposalWeekDto quarterWeek)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var proposalDetailQuarterWeek = new proposal_version_detail_quarter_weeks
                    {
                        proposal_version_quarter_id = quarterId,
                        is_hiatus = quarterWeek.IsHiatus,
                        cost = quarterWeek.Cost,
                        impressions_goal = quarterWeek.Impressions,
                        units = quarterWeek.Units,
                        media_week_id = quarterWeek.MediaWeekId,
                        end_date = quarterWeek.EndDate,
                        start_date = quarterWeek.StartDate
                    };

                    context.proposal_version_detail_quarter_weeks.Add(proposalDetailQuarterWeek);
                    context.SaveChanges();
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

                var dto = new ProposalDetailOpenMarketInventoryDto();
                dto.Margin = pv.proposal_versions.margin;
                SetBaseFields(pv, dto);
                dto.Criteria = new OpenMarketCriterion
                {
                    CpmCriteria = pv.proposal_version_detail_criteria_cpm.Select(c =>
                        new CpmCriteria { Id = c.id, MinMax = (MinMaxEnum)c.min_max, Value = c.value }).ToList(),
                    GenreSearchCriteria = pv.proposal_version_detail_criteria_genres.Select(c =>
                            new GenreCriteria()
                            {
                                Id = c.id,
                                Contain = (ContainTypeEnum)c.contain_type,
                                Genre = new LookupDto(c.genre_id, "")
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

        public ProposalDetailDto GetProposalDetail(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var proposalDetail = context.proposal_version_details
                .Include(pvd => pvd.proposal_version_detail_criteria_genres)
                .Include(pvd => pvd.proposal_version_detail_criteria_programs)
                .Single(pvd => pvd.id == proposalDetailId,
                    string.Format("The proposal detail information you have entered [{0}] does not exist.",
                        proposalDetailId));

                var proposalDetailDto = new ProposalDetailDto
                {

                    Id = proposalDetail.id,
                    DaypartCode = proposalDetail.daypart_code,
                    SpotLengthId = proposalDetail.spot_length_id,
                    TotalCost = proposalDetail.cost_total.GetValueOrDefault(),
                    TotalImpressions = proposalDetail.impressions_total,
                    TotalUnits = proposalDetail.units_total.GetValueOrDefault(),
                    FlightEndDate = proposalDetail.end_date,
                    FlightStartDate = proposalDetail.start_date,
                    DaypartId = proposalDetail.daypart_id,
                    Adu = proposalDetail.adu,
                    SingleProjectionBookId = proposalDetail.single_projection_book_id,
                    ShareProjectionBookId = proposalDetail.share_projection_book_id,
                    HutProjectionBookId = proposalDetail.hut_projection_book_id,
                    ProjectionPlaybackType = (ProposalEnums.ProposalPlaybackType)proposalDetail.projection_playback_type,
                    GenreCriteria = proposalDetail.proposal_version_detail_criteria_genres.Select(c => new GenreCriteria()
                    {
                        Id = c.id,
                        Contain = (ContainTypeEnum)c.contain_type,
                        Genre = new LookupDto { Id = c.genre_id }
                    }).ToList(),
                    ShowTypeCriteria = proposalDetail.proposal_version_detail_criteria_show_types.Select(st => new ShowTypeCriteria
                    {
                        Id = st.id,
                        Contain = (ContainTypeEnum)st.contain_type,
                        ShowType = new LookupDto { Id = st.show_types.id, Display = st.show_types.name }
                    }).ToList(),
                    ProgramCriteria = proposalDetail.proposal_version_detail_criteria_programs.Select(p => new ProgramCriteria()
                    {
                        Id = p.id,
                        Contain = (ContainTypeEnum)p.contain_type,
                        Program = new LookupDto
                        {
                            Id = p.program_name_id,
                            Display = p.program_name
                        }
                    }).ToList()
                };

                return proposalDetailDto;
            });
        }

        public int GetProposalDetailSpotLengthId(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var proposalDetail = context.proposal_version_details.Single(p => p.id == proposalDetailId,
                    "Proposal detail not found");
                return proposalDetail.spot_length_id;
            });
        }

        public void SaveProposalDetailOpenMarketInventoryTotals(int proposalDetailId,
            ProposalDetailSingleInventoryTotalsDto proposalDetailSingleInventoryTotalsDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var proposalDetail = context.proposal_version_details.First(p => p.id == proposalDetailId);

                proposalDetail.open_market_impressions_total = proposalDetailSingleInventoryTotalsDto.TotalImpressions;
                proposalDetail.open_market_cost_total = proposalDetailSingleInventoryTotalsDto.TotalCost;

                context.SaveChanges();
            });
        }

        public void SaveProposalDetailProprietaryInventoryTotals(int proposalDetailId,
            ProposalInventoryTotalsDto proposalDetailSingleInventoryTotalsDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var proposalDetail = context.proposal_version_details.First(p => p.id == proposalDetailId);

                proposalDetail.proprietary_impressions_total = proposalDetailSingleInventoryTotalsDto.TotalImpressions;
                proposalDetail.proprietary_cost_total = proposalDetailSingleInventoryTotalsDto.TotalCost;

                context.SaveChanges();
            });
        }

        ProposalDetailSingleInventoryTotalsDto IProposalRepository.GetProposalDetailOpenMarketInventoryTotals(
            int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                if (proposalDetailId == 0)
                    return new ProposalDetailSingleInventoryTotalsDto();

                var proposalDetail = context.proposal_version_details.Include(pvd => pvd.proposal_versions)
                    .First(p => p.id == proposalDetailId);

                return new ProposalDetailSingleInventoryTotalsDto
                {
                    Margin = proposalDetail.proposal_versions.margin,
                    TotalCost = proposalDetail.open_market_cost_total,
                    TotalImpressions = proposalDetail.open_market_impressions_total
                };
            });
        }

        ProposalDetailSingleInventoryTotalsDto IProposalRepository.GetProposalDetailProprietaryInventoryTotals(
            int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                if (proposalDetailId == 0)
                    return new ProposalDetailSingleInventoryTotalsDto();

                var proposalDetail = context.proposal_version_details.First(p => p.id == proposalDetailId);

                return new ProposalDetailSingleInventoryTotalsDto
                {
                    TotalCost = proposalDetail.proprietary_cost_total,
                    TotalImpressions = proposalDetail.proprietary_impressions_total
                };
            });
        }

        public void SaveProposalDetailOpenMarketWeekInventoryTotals(
            ProposalDetailOpenMarketInventoryDto proposalDetailOpenMarketInventoryDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                foreach (var proposalWeekDto in proposalDetailOpenMarketInventoryDto.Weeks)
                {
                    var week = context.proposal_version_detail_quarter_weeks.First(p =>
                        p.id == proposalWeekDto.ProposalVersionDetailQuarterWeekId);

                    week.open_market_impressions_total = (proposalWeekDto.ImpressionsTotal);
                    week.open_market_cost_total = proposalWeekDto.BudgetTotal;
                }

                context.SaveChanges();
            });
        }

        public void SaveProposalDetailProprietaryWeekInventoryTotals(int proposalDetailId,
            ProposalInventoryTotalsDto proposalDetailProprietaryInventoryDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                foreach (var proposalWeekDto in proposalDetailProprietaryInventoryDto.Weeks)
                {
                    var localProposalWeekDto = proposalWeekDto;

                    var week =
                        context.proposal_version_details.Include(
                                p => p.proposal_version_detail_quarters.Select(q =>
                                    q.proposal_version_detail_quarter_weeks))
                            .Where(p => p.id == proposalDetailId)
                            .SelectMany(
                                p =>
                                    p.proposal_version_detail_quarters.SelectMany(
                                        q => q.proposal_version_detail_quarter_weeks))
                            .First(w => w.media_week_id == localProposalWeekDto.MediaWeekId);

                    week.proprietary_impressions_total = (proposalWeekDto.Impressions);
                    week.proprietary_cost_total = proposalWeekDto.Budget;
                }

                context.SaveChanges();
            });
        }

        public List<ProposalDetailSingleWeekTotalsDto> GetProposalDetailOpenMarketWeekInventoryTotals(
            int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var weeksTotals = new List<ProposalDetailSingleWeekTotalsDto>();

                if (proposalDetailId == 0)
                    return weeksTotals;

                var weeks =
                    context.proposal_version_details.Include(
                            p => p.proposal_version_detail_quarters.Select(q =>
                                q.proposal_version_detail_quarter_weeks))
                        .Where(p => p.id == proposalDetailId)
                        .SelectMany(
                            p =>
                                p.proposal_version_detail_quarters.SelectMany(
                                    q => q.proposal_version_detail_quarter_weeks));

                weeks.ForEach(w => weeksTotals.Add(new ProposalDetailSingleWeekTotalsDto
                {
                    MediaWeekId = w.media_week_id,
                    TotalCost = w.open_market_cost_total,
                    TotalImpressions = w.open_market_impressions_total
                }));

                return weeksTotals;
            });
        }

        public List<ProposalDetailSingleWeekTotalsDto> GetProposalDetailProprietaryWeekInventoryTotals(
            int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var weeksTotals = new List<ProposalDetailSingleWeekTotalsDto>();

                if (proposalDetailId == 0)
                    return weeksTotals;

                var weeks =
                    context.proposal_version_details.Include(
                            p => p.proposal_version_detail_quarters.Select(q =>
                                q.proposal_version_detail_quarter_weeks))
                        .Where(p => p.id == proposalDetailId)
                        .SelectMany(
                            p =>
                                p.proposal_version_detail_quarters.SelectMany(
                                    q => q.proposal_version_detail_quarter_weeks));

                weeks.ForEach(w => weeksTotals.Add(new ProposalDetailSingleWeekTotalsDto
                {
                    MediaWeekId = w.media_week_id,
                    TotalCost = w.proprietary_cost_total,
                    TotalImpressions = w.proprietary_impressions_total
                }));

                return weeksTotals;
            });
        }

        public void UpdateProposalDetailSweepsBooks(int proposalDetailId, int hutBook, int shareBook)
        {
            _InReadUncommitedTransaction(c =>
            {
                var detail = c.proposal_version_details.Find(proposalDetailId);
                detail.hut_projection_book_id = hutBook;
                detail.share_projection_book_id = shareBook;
                c.SaveChanges();
            });
        }

        public void UpdateProposalDetailSweepsBook(int proposalDetailId, int book)
        {
            _InReadUncommitedTransaction(c =>
            {
                var detail = c.proposal_version_details.Find(proposalDetailId);
                detail.single_projection_book_id = book;
                c.SaveChanges();
            });
        }

        public List<ProposalDetailTotalsDto> GetAllProposalDetailsTotals(int proposalVersionId)
        {
            return _InReadUncommitedTransaction(
                c =>
                    c.proposal_version_details.Where(p => p.proposal_version_id == proposalVersionId)
                        .Select(p => new ProposalDetailTotalsDto
                        {
                            OpenMarketImpressionsTotal = p.open_market_impressions_total,
                            OpenMarketCostTotal = p.open_market_cost_total,
                            ProprietaryImpressionsTotal = p.proprietary_impressions_total,
                            ProprietaryCostTotal = p.proprietary_cost_total
                        }).ToList());
        }

        public int GetProposalDetailGuaranteedAudienceId(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var proposalVersion =
                    context.proposal_version_details.Include(pvd => pvd.proposal_versions)
                        .Single(x => x.id == proposalDetailId, "Unable to find proposal detail");

                return proposalVersion.proposal_versions.guaranteed_audience_id;
            });
        }

        public void SaveProposalTotals(int proposalVersionId, ProposalHeaderTotalsDto proposalTotals)
        {
            _InReadUncommitedTransaction(c =>
            {
                var proposalVersion = c.proposal_versions.Find(proposalVersionId);
                proposalVersion.impressions_total = proposalTotals.ImpressionsTotal;
                proposalVersion.cost_total = proposalTotals.CostTotal;
                c.SaveChanges();
            });
        }

        public void ResetAllTotals(int proposalId, int proposalVersion)
        {
            _InReadUncommitedTransaction(c =>
            {
                var proposal =
                    c.proposal_versions.Single(
                        p => p.proposal_id == proposalId && p.proposal_version == proposalVersion);
                proposal.cost_total = 0;
                proposal.impressions_total = 0;
                proposal.proposal_version_details.ForEach(pvd =>
                {
                    pvd.open_market_cost_total = 0;
                    pvd.open_market_impressions_total = 0;
                    pvd.proprietary_cost_total = 0;
                    pvd.proprietary_impressions_total = 0;
                    pvd.proposal_version_detail_quarters.ForEach(pvdq =>
                        pvdq.proposal_version_detail_quarter_weeks.ForEach(
                            pvdqw =>
                            {
                                pvdqw.open_market_cost_total = 0;
                                pvdqw.open_market_impressions_total = 0;
                                pvdqw.proprietary_cost_total = 0;
                                pvdqw.proprietary_impressions_total = 0;
                            }));
                });
                c.SaveChanges();
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
                dto.Margin = pv.proposal_versions.margin;

                SetBaseFields(pv, dto);
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

        private static void SetBaseFields(proposal_version_details pvd, ProposalDetailInventoryBase baseDto)
        {
            var pv = pvd.proposal_versions;
            baseDto.ProposalVersionId = pv.id;
            baseDto.DetailId = pvd.id;
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
            baseDto.SingleProjectionBookId = pvd.share_projection_book_id;
            baseDto.HutProjectionBookId = pvd.hut_projection_book_id;
            baseDto.PlaybackType = (ProposalEnums.ProposalPlaybackType?)pvd.projection_playback_type;
        }

        public void DeleteProposal(int proposalId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var proposal = context.proposals.Single(a => a.id == proposalId,
                    string.Format("The Proposal information you have entered [{0}] does not exist. Please try again.",
                        proposalId));

                context.proposals.Remove(proposal);

                context.SaveChanges();
            });
        }

        public Dictionary<int, ProposalDto> GetProposalsByQuarterWeeks(List<int> quarterWeekIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var proposals = (from p in context.proposals
                                 join v in context.proposal_versions on p.id equals v.proposal_id
                                 join d in context.proposal_version_details on v.id equals d.proposal_version_id
                                 join q in context.proposal_version_detail_quarters on d.id equals q.proposal_version_detail_id
                                 join qw in context.proposal_version_detail_quarter_weeks on q.id equals qw
                                     .proposal_version_quarter_id
                                 where quarterWeekIds.Contains(qw.id)
                                 select new { p, v, pd = d, quarterWeekId = qw.id })
                    .GroupBy(g => g.quarterWeekId).ToList()
                    .ToDictionary(k => k.Key, val => val.Select(v => MapToProposalDto(v.p, v.v)).Single());

                return proposals;
            });
        }

        public List<AffidavitMatchingProposalWeek> GetAffidavitMatchingProposalWeeksByHouseIsci(string isci)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var weeks = context.proposal_version_detail_quarter_week_iscis.Where(
                        i => i.house_isci.Equals(isci, StringComparison.InvariantCultureIgnoreCase))
                        .Where(i => i.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_details
                            .proposal_versions.status == (int)ProposalEnums.ProposalStatusType.Contracted)
                        .Include(i => i.proposal_version_detail_quarter_weeks
                                .proposal_version_detail_quarters.proposal_version_details)
                        .Select(
                            i => new AffidavitMatchingProposalWeek()
                            {
                                ProposalVersionId = i.proposal_version_detail_quarter_weeks
                                                        .proposal_version_detail_quarters
                                                        .proposal_version_details
                                                        .proposal_version_id,
                                ProposalVersionDetailId = i.proposal_version_detail_quarter_weeks
                                                        .proposal_version_detail_quarters
                                                        .proposal_version_detail_id,
                                ProposalVersionDetailPostingBookId = i.proposal_version_detail_quarter_weeks
                                                        .proposal_version_detail_quarters
                                                        .proposal_version_details.posting_book_id,
                                ProposalVersionDetailPostingPlaybackType = (ProposalEnums.ProposalPlaybackType?)i.proposal_version_detail_quarter_weeks
                                                        .proposal_version_detail_quarters
                                                        .proposal_version_details.posting_playback_type,
                                ProposalVersionDetailWeekStart = i.proposal_version_detail_quarter_weeks.start_date,
                                ProposalVersionDetailWeekEnd = i.proposal_version_detail_quarter_weeks.end_date,
                                Spots = i.proposal_version_detail_quarter_weeks.units,
                                ProposalVersionDetailDaypartId = i.proposal_version_detail_quarter_weeks
                                                        .proposal_version_detail_quarters
                                                        .proposal_version_details.daypart_id,
                                ProposalVersionDetailQuarterWeekId = i.proposal_version_detail_quarter_week_id,
                                ClientIsci = i.client_isci,
                                HouseIsci = i.house_isci,
                                MarriedHouseIsci = i.married_house_iscii,
                                Brand = i.brand
                            }).ToList();
                    return weeks;
                });
        }

        public void UpdateProposalDetailPostingBooks(List<ProposalDetailPostingData> detailPostingData)
        {
            _InReadUncommitedTransaction(c =>
            {
                foreach(var detailData in detailPostingData)
                {
                    var detail = c.proposal_version_details.Find(detailData.ProposalVersionDetailId);
                    if(detail.posting_book_id == null)
                    {
                        detail.posting_book_id = detailData.PostingBookId;
                        detail.posting_playback_type = (byte)detailData.PostingPlaybackType.Value;
                        c.SaveChanges();
                    }                    
                }                
            });
        }
    }
}
