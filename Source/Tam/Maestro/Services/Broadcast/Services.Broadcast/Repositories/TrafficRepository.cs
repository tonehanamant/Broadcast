using System.Data.Entity;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Microsoft.PowerShell.Commands;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Broadcast.Entities.spotcableXML;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{

    public interface ITrafficRepository : IDataRepository
    {
        TrafficDisplayDto GetTrafficProposals(List<int> weekIds, ProposalEnums.ProposalStatusType? proposalStatus);
    }

    public class TrafficRepository : BroadcastRepositoryBase, ITrafficRepository
    {

        public TrafficRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory
            , ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public TrafficDisplayDto GetTrafficProposals(List<int> weekIds, ProposalEnums.ProposalStatusType? proposalStatus)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    // get a dictionary containing a list of proprietary and openmarket that does not have an isci against it for the current quarter
                    var trafficWeekProposals = _GetTrafficWeeks(context, weekIds, proposalStatus);

                    return new TrafficDisplayDto()
                    {
                        Weeks = trafficWeekProposals.Select(w => new TrafficWeek()
                        {
                            WeekId = w.Key,
                            TrafficProposalInventories = w.Value
                        }).ToList()
                    };
                });
        }

        private Dictionary<int, List<TrafficProposalInventory>> _GetTrafficWeeks(QueryHintBroadcastContext context,
            List<int> weekIds, ProposalEnums.ProposalStatusType? proposalStatus)
        {
            // retrieve all proposals that have mediaweeks in the quarter_week for either inventory_detail or station_program
            var trafficWeeks = (from p in context.proposals
                join pv in context.proposal_versions on p.id equals pv.proposal_id
                join pd in context.proposal_version_details on pv.id equals pd.proposal_version_id
                join pq in context.proposal_version_detail_quarters on pd.id equals pq.proposal_version_detail_id
                join pw in context.proposal_version_detail_quarter_weeks on pq.id equals pw.proposal_version_quarter_id
                join inv in context.inventory_detail_slot_proposal on pw.id equals
                    inv.proprosal_version_detail_quarter_week_id into ProprietaryUnassignedISCI
                join pro in context.station_program_flight_proposal on pw.id equals
                    pro.proprosal_version_detail_quarter_week_id into OpenMarketUnassignedISCI
                where pv.proposal_id == p.id &&
                      pv.id == p.primary_version_id &&
                      weekIds.Contains(pw.media_week_id) &&
                      (!proposalStatus.HasValue || (proposalStatus.HasValue && pv.status == (byte)proposalStatus.Value))
                select new
                {
                    MediaWeekId = pw.media_week_id,
                    ProposalId = p.id,
                    ProposalName = p.name,
                    ProposalAdvertiserId = p.advertiser_id,
                    ProposalFlightStartDate = pv.start_date,
                    ProposalFlightEndDate = pv.end_date,
                    ProprietaryISCI = ProprietaryUnassignedISCI.Count(z1 => string.IsNullOrEmpty(z1.isci)),
                    OpenMarketISCI = OpenMarketUnassignedISCI.Count(z2 => string.IsNullOrEmpty(z2.isci))
                });

            // group proposals to sum up the values of openmarket or proprietary
            var trafficProposals = (from t in trafficWeeks
                where t.ProprietaryISCI > 0 || t.OpenMarketISCI > 0
                group t by
                    new
                    {
                        t.MediaWeekId,
                        t.ProposalId,
                        t.ProposalName,
                        t.ProposalAdvertiserId,
                        t.ProposalFlightStartDate,
                        t.ProposalFlightEndDate
                    }
                into grp
                select new
                {
                    grp.Key.MediaWeekId,
                    TrafficProposal = new TrafficProposalInventory()
                    {
                        Id = grp.Key.ProposalId,
                        AdvertiserId = grp.Key.ProposalAdvertiserId,
                        Name = grp.Key.ProposalName,
                        StartDate = grp.Key.ProposalFlightStartDate,
                        EndDate = grp.Key.ProposalFlightEndDate,
                        OpenMarketUnassignedISCI = grp.Sum(a => a.OpenMarketISCI),
                        ProprietaryUnassignedISCI = grp.Sum(b => b.ProprietaryISCI)
                    }
                }).OrderBy(c => c.MediaWeekId).ThenBy(d => d.TrafficProposal.Id).ToList();

            // group them by mediaweek/proposal
            return trafficProposals.GroupBy(a => a.MediaWeekId)
                .ToDictionary(b => b.Key, c => c.Select(r => r.TrafficProposal).ToList());
        }
    }
}
