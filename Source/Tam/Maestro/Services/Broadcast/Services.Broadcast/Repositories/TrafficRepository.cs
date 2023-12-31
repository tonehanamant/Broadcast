﻿using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{

    public interface ITrafficRepository : IDataRepository
    {
        TrafficDisplayDto GetTrafficProposals(List<int> weekIds, ProposalEnums.ProposalStatusType? proposalStatus);
    }

    public class TrafficRepository : BroadcastRepositoryBase, ITrafficRepository
    {

        public TrafficRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

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
            //TODO: fix this
            return new Dictionary<int, List<TrafficProposalInventory>>();
            // retrieve all proposals that have mediaweeks in the quarter_week for either inventory_detail or station_program
            //var trafficWeeks = (from p in context.proposals
            //    join pv in context.proposal_versions on p.id equals pv.proposal_id
            //    join pd in context.proposal_version_details on pv.id equals pd.proposal_version_id
            //    join pq in context.proposal_version_detail_quarters on pd.id equals pq.proposal_version_detail_id
            //    join pw in context.proposal_version_detail_quarter_weeks on pq.id equals pw.proposal_version_quarter_id
            //    where pv.proposal_id == p.id &&
            //          pv.id == p.primary_version_id &&
            //          weekIds.Contains(pw.media_week_id) &&
            //          (!proposalStatus.HasValue || (proposalStatus.HasValue && pv.status == (byte)proposalStatus.Value))
            //    select new
            //    {
            //        MediaWeekId = pw.media_week_id,
            //        ProposalId = p.id,
            //        ProposalName = p.name,
            //        ProposalAdvertiserId = p.advertiser_id,
            //        ProposalFlightStartDate = pv.start_date,
            //        ProposalFlightEndDate = pv.end_date,
            //        //OpenMarketISCI = OpenMarketUnassignedISCI.Count(z2 => string.IsNullOrEmpty(z2.isci))
            //    });

            //// group proposals to sum up the values of openmarket or proprietary
            //var trafficProposals = (from t in trafficWeeks
            //    where t.OpenMarketISCI > 0
            //    group t by
            //        new
            //        {
            //            t.MediaWeekId,
            //            t.ProposalId,
            //            t.ProposalName,
            //            t.ProposalAdvertiserId,
            //            t.ProposalFlightStartDate,
            //            t.ProposalFlightEndDate
            //        }
            //    into grp
            //    select new
            //    {
            //        grp.Key.MediaWeekId,
            //        TrafficProposal = new TrafficProposalInventory()
            //        {
            //            Id = grp.Key.ProposalId,
            //            AdvertiserId = grp.Key.ProposalAdvertiserId,
            //            Name = grp.Key.ProposalName,
            //            StartDate = grp.Key.ProposalFlightStartDate,
            //            EndDate = grp.Key.ProposalFlightEndDate,
            //            OpenMarketUnassignedISCI = grp.Sum(a => a.OpenMarketISCI)
            //        }
            //    }).OrderBy(c => c.MediaWeekId).ThenBy(d => d.TrafficProposal.Id).ToList();
            //// group them by mediaweek/proposal
            //return trafficProposals.GroupBy(a => a.MediaWeekId)
            //    .ToDictionary(b => b.Key, c => c.Select(r => r.TrafficProposal).ToList());
        }
    }
}
