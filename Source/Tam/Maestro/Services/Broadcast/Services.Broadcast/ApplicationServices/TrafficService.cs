using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using System.Transactions;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.BusinessEngines;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface ITrafficService : IApplicationService
    {
        TrafficDisplayDto GetTrafficProposals(DateTime currentDateTime, ProposalEnums.ProposalStatusType? proposalStatus);
    }

    public class TrafficService :  ITrafficService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IQuarterCalculationEngine _quarterCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly ITrafficRepository _TrafficRepository;
        private readonly ISMSClient _SmsClient;

        public TrafficService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IQuarterCalculationEngine quarterCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            ISMSClient smsClient)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _quarterCalculationEngine = quarterCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _TrafficRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ITrafficRepository>();
            _SmsClient = smsClient;
        }

        public TrafficDisplayDto GetTrafficProposals(DateTime currentDateTime, ProposalEnums.ProposalStatusType? proposalStatus)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _GetTrafficProposalsByProposalStatus(currentDateTime, proposalStatus);
            }            
        }

        private TrafficDisplayDto _GetTrafficProposalsByProposalStatus(DateTime currentDateTime, ProposalEnums.ProposalStatusType? proposalStatus)
        {
            var quarterDates = _quarterCalculationEngine.GetQuarterRangeByDate(currentDateTime, 0);
            var mediaWeeks = _MediaMonthAndWeekAggregateCache.GetMediaWeeksInRange(quarterDates.StartDate,
                quarterDates.EndDate);

            var trafficProposals =
                _TrafficRepository.GetTrafficProposals(mediaWeeks.Select(m => m.Id).ToList(), proposalStatus);

            // set weekname and advertiser
            foreach (var week in trafficProposals.Weeks)
            {
                var mediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(week.WeekId);
                week.Week = mediaMonth.StartDate.ToShortDateString();
                week.TrafficProposalInventories.ForEach(t => t.Advertiser = _SmsClient.FindAdvertiserById(t.AdvertiserId).Display);
            }

            return trafficProposals;
        }

    }

}
