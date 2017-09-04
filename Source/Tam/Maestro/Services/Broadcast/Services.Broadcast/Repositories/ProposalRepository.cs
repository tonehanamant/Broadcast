using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using Services.Broadcast.Entities.OpenMarketInventory;
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
        void SaveProposalDetailOpenMarketInventoryTotals(int proposalDetailId, ProposalDetailSingleInventoryTotalsDto proposalDetailSingleInventoryTotalsDto);
        void SaveProposalDetailProprietaryInventoryTotals(int proposalDetailId, ProposalInventoryTotalsDto proposalDetailSingleInventoryTotalsDto);
        ProposalDetailSingleInventoryTotalsDto GetProposalDetailOpenMarketInventoryTotals(int proposalDetailId);
        ProposalDetailSingleInventoryTotalsDto GetProposalDetailProprietaryInventoryTotals(int proposalDetailId);
        void SaveProposalDetailOpenMarketWeekInventoryTotals(ProposalDetailOpenMarketInventoryDto proposalDetailProprietaryInventoryDto);
        void SaveProposalDetailProprietaryWeekInventoryTotals(int proposalDetailId, ProposalInventoryTotalsDto proposalDetailProprietaryInventoryDto);
        List<ProposalDetailSingleWeekTotalsDto> GetProposalDetailOpenMarketWeekInventoryTotals(int proposalDetailId);
        List<ProposalDetailSingleWeekTotalsDto> GetProposalDetailProprietaryWeekInventoryTotals(int proposalDetailId);
        void UpdateProposalDetailSweepsBooks(int proposalDetailId, int hutBook, int shareBook);
        void UpdateProposalDetailSweepsBook(int proposalDetailId, int book);
        List<ProposalDetailTotalsDto> GetAllProposalDetailsTotals(int proposalVersionId);
        void SaveProposalTotals(int proposalVersionId, ProposalHeaderTotalsDto proposalTotals);
    }

    public class ProposalRepository : BroadcastRepositoryBase, IProposalRepository
    {

        public ProposalRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory
            , ITransactionHelper pTransactionHelper)
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
                markets = (byte?)proposalDto.MarketGroupId,
                blackout_markets = (byte?)proposalDto.BlackoutMarketGroupId,
                created_by = userName,
                created_date = timestamp,
                modified_by = userName,
                modified_date = timestamp,
                target_budget = proposalDto.TargetBudget,
                target_impressions = proposalDto.TargetImpressions.HasValue ? (int?)(proposalDto.TargetImpressions * 1000) : null,
                target_cpm = proposalDto.TargetCPM.Value,
                margin = proposalDto.Margin.Value,
                target_units = proposalDto.TargetUnits,
                notes = proposalDto.Notes,
                post_type = (byte?)proposalDto.PostType,
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

        private static void _SaveProposalSpotLength(BroadcastContext context, int proposalVersionId, List<LookupDto> spotLengths)
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
            dbProposalVersion.markets = (byte?)proposalDto.MarketGroupId;
            dbProposalVersion.blackout_markets = (byte?)proposalDto.BlackoutMarketGroupId;
            dbProposalVersion.modified_by = userName;
            dbProposalVersion.modified_date = timestamp;
            dbProposalVersion.target_budget = proposalDto.TargetBudget;
            dbProposalVersion.target_impressions = proposalDto.TargetImpressions.HasValue ? (int?)(proposalDto.TargetImpressions * 1000) : null;
            dbProposalVersion.target_cpm = proposalDto.TargetCPM.Value;
            dbProposalVersion.margin = proposalDto.Margin.Value;
            dbProposalVersion.target_units = proposalDto.TargetUnits;
            dbProposalVersion.notes = proposalDto.Notes;
            dbProposalVersion.post_type = (byte?)proposalDto.PostType;
            dbProposalVersion.equivalized = proposalDto.Equivalized;
            dbProposalVersion.status = (byte)proposalDto.Status;

            context.SaveChanges();

            return dbProposalVersion.id;
        }

        private static void _SaveProposalVersionMarkets(BroadcastContext context, int proposalVersionId, IEnumerable<ProposalMarketDto> markets)
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

        private static void _SaveProposalVersionSecondaryDemos(BroadcastContext context, int proposalVersionId, List<int> secondaryDemos)
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

        private static void _SaveProposalVersionDetails(BroadcastContext context, int proposalVersionId, List<ProposalDetailDto> proposalDetails)
        {
            if (proposalDetails == null || !proposalDetails.Any()) return;

            context.proposal_version_details.AddRange(
                proposalDetails.Select(proposalDetail => new proposal_version_details
                {
                    cost_total = (decimal?)proposalDetail.TotalCost,
                    daypart_code = proposalDetail.DaypartCode,
                    proposal_version_id = proposalVersionId,
                    spot_length_id = proposalDetail.SpotLengthId,
                    units_total = proposalDetail.TotalUnits,
                    impressions_total = (int)(proposalDetail.TotalImpressions * 1000),
                    start_date = proposalDetail.FlightStartDate,
                    end_date = proposalDetail.FlightEndDate,
                    daypart_id = proposalDetail.DaypartId,
                    adu = proposalDetail.Adu,
                    single_posting_book_id = proposalDetail.SinglePostingBookId,
                    hut_posting_book_id = proposalDetail.HutPostingBookId,
                    share_posting_book_id = proposalDetail.SharePostingBookId,
                    playback_type = (byte?)proposalDetail.PlaybackType,
                    proposal_version_detail_quarters =
                        proposalDetail.Quarters.Select(quarter => new proposal_version_detail_quarters
                        {
                            cpm = quarter.Cpm,
                            impressions_goal = (int)(quarter.ImpressionGoal * 1000),
                            quarter = quarter.Quarter,
                            year = quarter.Year,
                            proposal_version_detail_quarter_weeks =
                                quarter.Weeks.Select(
                                    quarterWeek => new proposal_version_detail_quarter_weeks
                                    {
                                        is_hiatus = quarterWeek.IsHiatus,
                                        cost = quarterWeek.Cost,
                                        impressions = (int)(quarterWeek.Impressions * 1000),
                                        units = quarterWeek.Units,
                                        media_week_id = quarterWeek.MediaWeekId,
                                        end_date = quarterWeek.EndDate,
                                        start_date = quarterWeek.StartDate
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

        private static void _UpdateProposalVersionDetails(BroadcastContext context, int proposalVersionId, List<ProposalDetailDto> proposalDetails)
        {
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
                    updatedDetail.impressions_total = (int)(detail.TotalImpressions * 1000);
                    updatedDetail.start_date = detail.FlightStartDate;
                    updatedDetail.end_date = detail.FlightEndDate;
                    updatedDetail.daypart_id = detail.DaypartId;
                    updatedDetail.adu = detail.Adu;
                    updatedDetail.single_posting_book_id = detail.SinglePostingBookId;
                    updatedDetail.hut_posting_book_id = detail.HutPostingBookId;
                    updatedDetail.share_posting_book_id = detail.SharePostingBookId;
                    updatedDetail.playback_type = (byte?)detail.PlaybackType;

                    // deal with quarters that have been deleted 
                    // scenario where user maintain the detail but change completely the flight generating new quarters for this particular detail
                    var deletedQuartersIds =
                        updatedDetail.proposal_version_detail_quarters.Select(a => a.id)
                            .Except(detail.Quarters.Where(b => b.Id != null).Select(q => q.Id.Value))
                            .ToList();

                    context.proposal_version_detail_quarters.RemoveRange(
                        context.proposal_version_detail_quarters.Where(a => deletedQuartersIds.Any(b => b == a.id)));

                    // deal with new added detail quarters
                    var newDetailQuarters = _GetProposalVersionDetailQuarters(detail.Quarters.Where(b => b.Id == null).ToList());
                    newDetailQuarters.ForEach(n => updatedDetail.proposal_version_detail_quarters.Add(n));

                    // detail with detail quarters that have been updated
                    detail.Quarters.Where(b => b.Id != null).ForEach(detailQuarter =>
                    {
                        var updatedQuarter =
                            updatedDetail.proposal_version_detail_quarters.FirstOrDefault(q => q.id == detailQuarter.Id);

                        if (updatedQuarter != null)
                        {
                            updatedQuarter.cpm = detailQuarter.Cpm;
                            updatedQuarter.impressions_goal = (int)(detailQuarter.ImpressionGoal * 1000);
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
                            var newQuarterWeeks = _GetProposalVersionDetailQuarterWeeks(detailQuarter.Weeks.Where(z => z.Id == null).ToList());
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
                                    quarterWeek.impressions = (int)(detatilQuarterWeek.Impressions * 1000);
                                    quarterWeek.cost = detatilQuarterWeek.Cost;
                                }
                            });
                        }
                    });
                }
            });

            context.SaveChanges();
        }

        private static List<proposal_version_detail_quarters> _GetProposalVersionDetailQuarters(List<ProposalQuarterDto> proposalQuarterDtos)
        {
            var detailQuarters = new List<proposal_version_detail_quarters>();
            if (!proposalQuarterDtos.Any()) return detailQuarters;

            var newDetailQuarters =
                proposalQuarterDtos.Select(quarter => new proposal_version_detail_quarters
                {
                    cpm = quarter.Cpm,
                    impressions_goal = (int)(quarter.ImpressionGoal * 1000),
                    quarter = quarter.Quarter,
                    year = quarter.Year,
                    proposal_version_detail_quarter_weeks =
                        quarter.Weeks.Select(
                            quarterWeek => new proposal_version_detail_quarter_weeks
                            {
                                is_hiatus = quarterWeek.IsHiatus,
                                cost = quarterWeek.Cost,
                                impressions = (int)(quarterWeek.Impressions * 1000),
                                units = quarterWeek.Units,
                                media_week_id = quarterWeek.MediaWeekId,
                                end_date = quarterWeek.EndDate,
                                start_date = quarterWeek.StartDate
                            }).ToList()
                }).ToList();

            detailQuarters.AddRange(newDetailQuarters);
            return detailQuarters;
        }

        private static List<proposal_version_detail_quarter_weeks> _GetProposalVersionDetailQuarterWeeks(List<ProposalWeekDto> proposalWeekDtos)
        {
            var quarterWeeks = new List<proposal_version_detail_quarter_weeks>();
            if (!proposalWeekDtos.Any()) return quarterWeeks;

            var newQuarterWeeks = proposalWeekDtos.Select(
                quarterWeek => new proposal_version_detail_quarter_weeks
                {
                    is_hiatus = quarterWeek.IsHiatus,
                    cost = quarterWeek.Cost,
                    impressions = (int)(quarterWeek.Impressions * 1000),
                    units = quarterWeek.Units,
                    media_week_id = quarterWeek.MediaWeekId,
                    end_date = quarterWeek.EndDate,
                    start_date = quarterWeek.StartDate
                }).ToList();
            quarterWeeks.AddRange(newQuarterWeeks);

            return quarterWeeks;
        }

        private static void _SaveProposalVersionFlightWeeks(BroadcastContext context, int proposalVersionId, List<ProposalFlightWeek> flightWeeks)
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

        private static void _SetProposalPrimaryVersion(BroadcastContext context, int proposalId, int primaryVersionId, string userName)
        {
            var proposalToUpdate = context.proposals.Single(q => q.id == proposalId, string.Format("Cannot find proposal {0}.", proposalId));

            var proposalVersions = proposalToUpdate.proposal_versions.Where(pv => pv.status == (int)ProposalEnums.ProposalStatusType.AgencyOnHold || pv.status == (int)ProposalEnums.ProposalStatusType.Contracted).ToList();
            proposalToUpdate.primary_version_id = proposalVersions.Any() ? proposalVersions.First().id : primaryVersionId;

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
                context => (from p in context.proposals
                            join v in context.proposal_versions on p.id equals v.proposal_id
                            where p.primary_version_id == v.id
                                  && p.id == proposalId
                            select new { p, v })
                    .ToList().Select(pv => _MapToProposalDto(pv.p, pv.v))
                    .Single(string.Format("The Proposal information you have entered [{0}] does not exist. Please try again.", proposalId)));
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
                            .ToList().Select(pv => _MapToProposalDto(pv.p, pv.v))
                            .Single(string.Format("Cannot find version {0} for proposal {1}.", version, proposalId)));
        }

        private static ProposalDto _MapToProposalDto(proposal proposal, proposal_versions proposalVersion)
        {
            var proposalDto = new ProposalDto();

            proposalDto.Id = proposalVersion.proposal.id;
            proposalDto.ProposalName = proposalVersion.proposal.name;
            proposalDto.AdvertiserId = proposalVersion.proposal.advertiser_id;
            proposalDto.FlightEndDate = proposalVersion.end_date;
            proposalDto.FlightStartDate = proposalVersion.start_date;
            proposalDto.GuaranteedDemoId = proposalVersion.guaranteed_audience_id;
            proposalDto.MarketGroupId = (ProposalEnums.ProposalMarketGroups?)proposalVersion.markets;
            proposalDto.BlackoutMarketGroupId = (ProposalEnums.ProposalMarketGroups?)proposalVersion.blackout_markets;
            proposalDto.Markets = proposalVersion.proposal_version_markets.Where(q => q.proposal_version_id == proposalVersion.id).
                    Select(f => new ProposalMarketDto
                    {
                        Id = f.market_code,
                        Display = f.market.geography_name,
                        IsBlackout = f.is_blackout
                    }).ToList();
            proposalDto.Status = (ProposalEnums.ProposalStatusType)proposalVersion.status;
            proposalDto.TargetUnits = proposalVersion.target_units;
            proposalDto.TargetBudget = proposalVersion.target_budget;
            proposalDto.TargetImpressions = proposalVersion.target_impressions.HasValue ? (double?)proposalVersion.target_impressions / 1000 : null;
            proposalDto.TargetCPM = proposalVersion.target_cpm;
            proposalDto.TotalCost = proposalVersion.cost_total;
            proposalDto.TotalImpressions = proposalVersion.impressions_total;
            proposalDto.TotalCPM = GetCpm(proposalVersion.impressions_total, proposalVersion.cost_total);
            proposalDto.Margin = proposalVersion.margin;
            proposalDto.Notes = proposalVersion.notes;
            proposalDto.Version = proposalVersion.proposal_version;
            proposalDto.VersionId = proposalVersion.id;
            proposalDto.PrimaryVersionId = proposal.primary_version_id;
            proposalDto.FlightWeeks =
                proposalVersion.proposal_version_flight_weeks.Where(q => q.proposal_version_id == proposalVersion.id)
                    .Select(f => new ProposalFlightWeek
                    {
                        EndDate = f.end_date,
                        IsHiatus = !f.active,
                        StartDate = f.start_date,
                        MediaWeekId = f.media_week_id
                    }).ToList();
            proposalDto.Equivalized = proposalVersion.equivalized;
            proposalDto.SecondaryDemos =
                proposalVersion.proposal_version_audiences.OrderBy(r => r.rank).Select(a => a.audience_id).ToList();
            proposalDto.PostType = (SchedulePostType?)proposalVersion.post_type;
            proposalDto.SpotLengths =
                proposalVersion.proposal_version_spot_length.Select(a => new LookupDto { Id = a.spot_length_id })
                    .ToList();
            proposalDto.Details = proposalVersion.proposal_version_details.Select(version => new ProposalDetailDto
            {
                Id = version.id,
                DaypartCode = version.daypart_code,
                SpotLengthId = version.spot_length_id,
                TotalCost = version.cost_total.GetValueOrDefault(),
                TotalImpressions = (double)(version.impressions_total ?? 0) / 1000,
                TotalUnits = version.units_total.GetValueOrDefault(),
                FlightEndDate = version.end_date,
                FlightStartDate = version.start_date,
                DaypartId = version.daypart_id,
                Adu = version.adu,
                SinglePostingBookId = version.single_posting_book_id,
                SharePostingBookId = version.share_posting_book_id,
                HutPostingBookId = version.hut_posting_book_id,
                PlaybackType = (ProposalEnums.ProposalPlaybackType?)version.playback_type,
                Quarters = version.proposal_version_detail_quarters.Select(quarter => new ProposalQuarterDto
                {
                    Cpm = quarter.cpm.GetValueOrDefault(),
                    ImpressionGoal = (double)(quarter.impressions_goal ?? 0) / 1000,
                    Id = quarter.id,
                    Year = quarter.year,
                    Quarter = quarter.quarter,
                    QuarterText = string.Format("{0} Q{1}", quarter.year, quarter.quarter),
                    Weeks = quarter.proposal_version_detail_quarter_weeks.Select(week => new ProposalWeekDto
                    {
                        Id = week.id,
                        Cost = week.cost.GetValueOrDefault(),
                        EndDate = week.end_date,
                        StartDate = week.start_date,
                        Impressions = (double)(week.impressions ?? 0) / 1000,
                        IsHiatus = week.is_hiatus,
                        Units = week.units.GetValueOrDefault(),
                        MediaWeekId = week.media_week_id,
                        Week = week.start_date.ToShortDateString()
                    }).ToList()
                }).ToList()
            }).ToList();

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
            proposalVersion.Markets = (ProposalEnums.ProposalMarketGroups?)x.markets;
            proposalVersion.TargetUnits = x.target_units;
            proposalVersion.TargetImpressions = x.target_impressions.HasValue ? (double?)x.target_impressions / 1000 : null;
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
                            }).ToList();
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
                        impressions_goal = (int)(quarter.ImpressionGoal * 1000),
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
                        impressions = (int)(quarterWeek.Impressions * 1000),
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
                    var pv = context.proposal_version_details.Include(pvd => pvd.proposal_versions.proposal)
                                                             .Include(pvd => pvd.proposal_version_detail_quarters.Select(dq => dq.proposal_version_detail_quarter_weeks))
                                                             .Include(pvd => pvd.proposal_versions.proposal_version_flight_weeks)
                                                             .Include(pvd => pvd.proposal_version_detail_criteria_cpm)
                                                             .Include(pvd => pvd.proposal_version_detail_criteria_genres)
                                                             .Include(pvd => pvd.proposal_version_detail_criteria_programs)
                                                             .Single(d => d.id == proposalDetailId, string.Format("The proposal detail information you have entered [{0}] does not exist.", proposalDetailId));

                    var dto = new ProposalDetailOpenMarketInventoryDto();
                    dto.Margin = pv.proposal_versions.margin;
                    SetBaseFields(pv, dto);
                    dto.Criteria = new OpenMarketCriterion
                    {
                        CpmCriteria = pv.proposal_version_detail_criteria_cpm.Select(c => new CpmCriteria { Id = c.id, MinMax = (MinMaxEnum)c.min_max, Value = c.value }).ToList(),
                        GenreSearchCriteria = pv.proposal_version_detail_criteria_genres.Select(c => new GenreCriteria { Id = c.id, Contain = (ContainTypeEnum)c.contain_type, GenreId = c.genre_id }).ToList(),
                        ProgramNameSearchCriteria = pv.proposal_version_detail_criteria_programs.Select(c => new ProgramCriteria { Id = c.id, Contain = (ContainTypeEnum)c.contain_type, ProgramName = c.program_name }).ToList()
                    };

                    dto.Weeks = (from quarter in pv.proposal_version_detail_quarters
                                 from week in quarter.proposal_version_detail_quarter_weeks
                                 orderby week.start_date
                                 select new ProposalOpenMarketInventoryWeekDto
                                 {
                                     ProposalVersionDetailQuarterWeekId = week.id,
                                     ImpressionsGoal = (double)((week.impressions ?? 0) / 1000),
                                     Budget = week.cost ?? 0,
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
                var proposalDetail = context.proposal_version_details.
                                        Single(pvd => pvd.id == proposalDetailId,
                                            string.Format("The proposal detail information you have entered [{0}] does not exist.", proposalDetailId));

                var proposalDetailDto = new ProposalDetailDto
                {

                    Id = proposalDetail.id,
                    DaypartCode = proposalDetail.daypart_code,
                    SpotLengthId = proposalDetail.spot_length_id,
                    TotalCost = proposalDetail.cost_total.GetValueOrDefault(),
                    TotalImpressions = (double)(proposalDetail.impressions_total ?? 0) / 1000,
                    TotalUnits = proposalDetail.units_total.GetValueOrDefault(),
                    FlightEndDate = proposalDetail.end_date,
                    FlightStartDate = proposalDetail.start_date,
                    DaypartId = proposalDetail.daypart_id,
                    Adu = proposalDetail.adu,
                    SinglePostingBookId = proposalDetail.single_posting_book_id,
                    SharePostingBookId = proposalDetail.share_posting_book_id,
                    HutPostingBookId = proposalDetail.hut_posting_book_id,
                    PlaybackType = (ProposalEnums.ProposalPlaybackType?)proposalDetail.playback_type
                };

                return proposalDetailDto;
            });
        }

        public void SaveProposalDetailOpenMarketInventoryTotals(int proposalDetailId, ProposalDetailSingleInventoryTotalsDto proposalDetailSingleInventoryTotalsDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var proposalDetail = context.proposal_version_details.First(p => p.id == proposalDetailId);

                proposalDetail.open_market_impressions_total = (long)(proposalDetailSingleInventoryTotalsDto.TotalImpressions * 1000);
                proposalDetail.open_market_cost_total = proposalDetailSingleInventoryTotalsDto.TotalCost;

                context.SaveChanges();
            });
        }

        public void SaveProposalDetailProprietaryInventoryTotals(int proposalDetailId, ProposalInventoryTotalsDto proposalDetailSingleInventoryTotalsDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var proposalDetail = context.proposal_version_details.First(p => p.id == proposalDetailId);

                proposalDetail.proprietary_impressions_total = (long)(proposalDetailSingleInventoryTotalsDto.TotalImpressions * 1000);
                proposalDetail.proprietary_cost_total = proposalDetailSingleInventoryTotalsDto.TotalCost;

                context.SaveChanges();
            });
        }

        ProposalDetailSingleInventoryTotalsDto IProposalRepository.GetProposalDetailOpenMarketInventoryTotals(int proposalDetailId)
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
                    TotalImpressions = proposalDetail.open_market_impressions_total / 1000.0
                };
            });
        }

        ProposalDetailSingleInventoryTotalsDto IProposalRepository.GetProposalDetailProprietaryInventoryTotals(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                if (proposalDetailId == 0)
                    return new ProposalDetailSingleInventoryTotalsDto();

                var proposalDetail = context.proposal_version_details.First(p => p.id == proposalDetailId);

                return new ProposalDetailSingleInventoryTotalsDto
                {
                    TotalCost = proposalDetail.proprietary_cost_total,
                    TotalImpressions = proposalDetail.proprietary_impressions_total / 1000.0
                };
            });
        }

        public void SaveProposalDetailOpenMarketWeekInventoryTotals(ProposalDetailOpenMarketInventoryDto proposalDetailOpenMarketInventoryDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                foreach (var proposalWeekDto in proposalDetailOpenMarketInventoryDto.Weeks)
                {
                    var week = context.proposal_version_detail_quarter_weeks.First(p => p.id == proposalWeekDto.ProposalVersionDetailQuarterWeekId);

                    week.open_market_impressions_total = (long)(proposalWeekDto.ImpressionsTotal * 1000);
                    week.open_market_cost_total = proposalWeekDto.BudgetTotal;
                }

                context.SaveChanges();
            });
        }

        public void SaveProposalDetailProprietaryWeekInventoryTotals(int proposalDetailId, ProposalInventoryTotalsDto proposalDetailProprietaryInventoryDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                foreach (var proposalWeekDto in proposalDetailProprietaryInventoryDto.Weeks)
                {
                    var localProposalWeekDto = proposalWeekDto;

                    var week =
                        context.proposal_version_details.Include(
                            p => p.proposal_version_detail_quarters.Select(q => q.proposal_version_detail_quarter_weeks))
                            .Where(p => p.id == proposalDetailId)
                            .SelectMany(
                                p =>
                                    p.proposal_version_detail_quarters.SelectMany(
                                        q => q.proposal_version_detail_quarter_weeks))
                            .First(w => w.media_week_id == localProposalWeekDto.MediaWeekId);

                    week.proprietary_impressions_total = (long)(proposalWeekDto.Impressions * 1000);
                    week.proprietary_cost_total = proposalWeekDto.Budget;
                }

                context.SaveChanges();
            });
        }

        public List<ProposalDetailSingleWeekTotalsDto> GetProposalDetailOpenMarketWeekInventoryTotals(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var weeksTotals = new List<ProposalDetailSingleWeekTotalsDto>();

                if (proposalDetailId == 0)
                    return weeksTotals;

                var weeks =
                    context.proposal_version_details.Include(
                        p => p.proposal_version_detail_quarters.Select(q => q.proposal_version_detail_quarter_weeks))
                        .Where(p => p.id == proposalDetailId)
                        .SelectMany(
                            p =>
                                p.proposal_version_detail_quarters.SelectMany(
                                    q => q.proposal_version_detail_quarter_weeks));

                weeks.ForEach(w => weeksTotals.Add(new ProposalDetailSingleWeekTotalsDto
                {
                    MediaWeekId = w.media_week_id,
                    TotalCost = w.open_market_cost_total,
                    TotalImpressions = w.open_market_impressions_total / 1000.0
                }));

                return weeksTotals;
            });
        }

        public List<ProposalDetailSingleWeekTotalsDto> GetProposalDetailProprietaryWeekInventoryTotals(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var weeksTotals = new List<ProposalDetailSingleWeekTotalsDto>();

                if (proposalDetailId == 0)
                    return weeksTotals;

                var weeks =
                    context.proposal_version_details.Include(
                        p => p.proposal_version_detail_quarters.Select(q => q.proposal_version_detail_quarter_weeks))
                        .Where(p => p.id == proposalDetailId)
                        .SelectMany(
                            p =>
                                p.proposal_version_detail_quarters.SelectMany(
                                    q => q.proposal_version_detail_quarter_weeks));

                weeks.ForEach(w => weeksTotals.Add(new ProposalDetailSingleWeekTotalsDto
                {
                    MediaWeekId = w.media_week_id,
                    TotalCost = w.proprietary_cost_total,
                    TotalImpressions = w.proprietary_impressions_total / 1000.0
                }));

                return weeksTotals;
            });
        }

        public void UpdateProposalDetailSweepsBooks(int proposalDetailId, int hutBook, int shareBook)
        {
            _InReadUncommitedTransaction(c =>
            {
                var detail = c.proposal_version_details.Find(proposalDetailId);
                detail.hut_posting_book_id = hutBook;
                detail.share_posting_book_id = shareBook;
                c.SaveChanges();
            });
        }

        public void UpdateProposalDetailSweepsBook(int proposalDetailId, int book)
        {
            _InReadUncommitedTransaction(c =>
            {
                var detail = c.proposal_version_details.Find(proposalDetailId);
                detail.single_posting_book_id= book;
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

        public ProposalDetailProprietaryInventoryDto GetProprietaryProposalDetailInventory(int proposalDetailId)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var pv = context.proposal_version_details.Include(pvd => pvd.proposal_versions.proposal)
                                                             .Include(pvd => pvd.proposal_version_detail_quarters.Select(dq => dq.proposal_version_detail_quarter_weeks))
                                                             .Include(pvd => pvd.proposal_versions.proposal_version_flight_weeks)
                                                            .Single(pvd => pvd.id == proposalDetailId, string.Format("The proposal detail information you have entered [{0}] does not exist.", proposalDetailId));

                    var dto = new ProposalDetailProprietaryInventoryDto();
                    dto.Margin = pv.proposal_versions.margin;

                    SetBaseFields(pv, dto);
                    dto.Weeks = (from quarter in pv.proposal_version_detail_quarters
                                 from week in quarter.proposal_version_detail_quarter_weeks
                                 orderby week.start_date
                                 select new ProposalProprietaryInventoryWeekDto
                                 {
                                     QuarterText = string.Format("Q{0}", quarter.quarter),
                                     ImpressionsGoal = (double)(week.impressions ?? 0) / 1000,
                                     Budget = week.cost ?? 0,
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
                }).ToList();

            baseDto.DetailDaypartId = pvd.daypart_id;
            baseDto.DetailSpotLengthId = pvd.spot_length_id;
            baseDto.DetailTargetImpressions = pvd.impressions_total.HasValue ? (double)pvd.impressions_total / 1000 : 0;
            baseDto.DetailTargetBudget = pvd.cost_total;
            baseDto.DetailCpm = GetCpm(pvd.impressions_total, pvd.cost_total);
            baseDto.DetailFlightEndDate = pvd.end_date;
            baseDto.DetailFlightStartDate = pvd.start_date;
            baseDto.DetailFlightWeeks = pvd.proposal_version_detail_quarters.SelectMany(quarter => quarter.proposal_version_detail_quarter_weeks.Select(week =>
                new ProposalFlightWeek
                {
                    EndDate = week.end_date,
                    StartDate = week.start_date,
                    IsHiatus = week.is_hiatus,
                    MediaWeekId = week.media_week_id
                }).ToList()).ToList();
            baseDto.SinglePostingBookId = pvd.single_posting_book_id;
            baseDto.SharePostingBookId = pvd.share_posting_book_id;
            baseDto.HutPostingBookId = pvd.hut_posting_book_id;
            baseDto.PlaybackType = (ProposalEnums.ProposalPlaybackType?)pvd.playback_type;
        }

        private static decimal GetCpm(long? totalImpressions, decimal? totalCost)
        {
            var impressions = (decimal)(totalImpressions ?? 0) / 1000;
            var cost = totalCost ?? 0;

            return impressions == 0 ? 0 : Math.Round(cost / impressions, 2);
        }
    }
}
