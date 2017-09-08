using System;
using System.Collections.Generic;
using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public class BaseProposalInventoryService
    {
        internal static string MissingBooksErrorMessage = "Unable to get proprietary inventory information due. Proposal must have both Share and Hut Book or Sweeps Book defined.";
        protected readonly IDataRepositoryFactory BroadcastDataRepositoryFactory;
        protected readonly IDaypartCache DaypartCache;
        protected readonly IProposalMarketsCalculationEngine ProposalMarketsCalculationEngine;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly IProposalTotalsCalculationEngine _proposalTotalsCalculationEngine;

        public BaseProposalInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache, IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IImpressionAdjustmentEngine impressionAdjustmentEngine,
            IProposalTotalsCalculationEngine proposalTotalsCalculationEngine)
        {
            BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            DaypartCache = daypartCache;
            ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
            _proposalTotalsCalculationEngine = proposalTotalsCalculationEngine;
        }

        protected IEnumerable<StationImpressions> GetImpressions(ProposalDetailInventoryBase proposalDetailInventory, List<int> ratingAudiences, IEnumerable<StationDetailDaypart> impressionRequests)
        {
            List<StationImpressions> impressions = null;

            if (proposalDetailInventory.SharePostingBookId.HasValue && proposalDetailInventory.HutPostingBookId.HasValue)
            {
                impressions = BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>().GetImpressionsDaypart((short)proposalDetailInventory.HutPostingBookId.Value, (short)proposalDetailInventory.SharePostingBookId.Value, ratingAudiences, impressionRequests, proposalDetailInventory.PlaybackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);
            }
            else if (proposalDetailInventory.SinglePostingBookId.HasValue)
            {
                impressions = new List<StationImpressions>(BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>().GetImpressionsDaypart(proposalDetailInventory.SinglePostingBookId.Value, ratingAudiences, impressionRequests, proposalDetailInventory.PlaybackType, BroadcastComposerWebSystemParameter.UseDayByDayImpressions));
            }

            if (impressions != null)
            {
                var ratingAdjustmentMonth = GetRatingAdjustmentMonth(proposalDetailInventory);

                impressions.ForEach(i => i.impressions = _ImpressionAdjustmentEngine.AdjustImpression(i.impressions, proposalDetailInventory.Equivalized, proposalDetailInventory.DetailSpotLength, proposalDetailInventory.PostType, ratingAdjustmentMonth, false));

                return impressions;
            }

            throw new ApplicationException(MissingBooksErrorMessage);
        }

        internal static int GetRatingAdjustmentMonth(ProposalDetailInventoryBase proposalDetailInventory)
        {
            int ratingAdjustmentMonth;
            if (proposalDetailInventory.HutPostingBookId.HasValue && proposalDetailInventory.SharePostingBookId.HasValue)
                ratingAdjustmentMonth = proposalDetailInventory.HutPostingBookId.Value;
            else
                ratingAdjustmentMonth = proposalDetailInventory.SinglePostingBookId.Value;
            return ratingAdjustmentMonth;
        }

        protected void _SetProposalInventoryDetailDaypart(ProposalDetailInventoryBase proposalInventory)
        {
            if (proposalInventory == null || !proposalInventory.DetailDaypartId.HasValue) return;
            proposalInventory.DetailDaypart = DaypartDto.ConvertDisplayDaypart(DaypartCache.GetDisplayDaypart(proposalInventory.DetailDaypartId.Value));
        }

        protected void _SetProposalInventoryDetailSpotLength(ProposalDetailInventoryBase proposalInventory)
        {
            if (proposalInventory == null) return;
            proposalInventory.DetailSpotLength =
                BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthById(proposalInventory.DetailSpotLengthId);
        }

        protected void _UpdateProposalTotals(int proposalVersionId)
        {
            var allProposalDetailsTotals = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                        .GetAllProposalDetailsTotals(proposalVersionId);
            var proposalTotals = _proposalTotalsCalculationEngine.SumAllDetailsTotals(allProposalDetailsTotals);
            BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                .SaveProposalTotals(proposalVersionId, proposalTotals);
        }
    }
}