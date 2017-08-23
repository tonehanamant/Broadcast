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

        public BaseProposalInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory, IDaypartCache daypartCache, IProposalMarketsCalculationEngine proposalMarketsCalculationEngine)
        {
            BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            DaypartCache = daypartCache;
            ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
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
                var spotLengthIdsAndCostMultipliers = BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthMultiplierRepository>().GetSpotLengthIdsAndCostMultipliers();
                var equivalizedMultiplier = proposalDetailInventory.Equivalized == true ? spotLengthIdsAndCostMultipliers[proposalDetailInventory.DetailSpotLengthId] : 1;

                impressions.ForEach(i => i.impressions = equivalizedMultiplier * i.impressions / 1000);
                ApplyPostTypeConversion(impressions, proposalDetailInventory);
                return impressions;
            }

            throw new ApplicationException(MissingBooksErrorMessage);
        }

        internal void ApplyPostTypeConversion(List<StationImpressions> impressions, ProposalDetailInventoryBase proposalDetailInventory)
        {
            int? mediaMonthId = null;
            if (proposalDetailInventory.PostType == SchedulePostType.NTI)
            {
                if (proposalDetailInventory.HutPostingBookId.HasValue && proposalDetailInventory.SharePostingBookId.HasValue)
                {
                    mediaMonthId = proposalDetailInventory.HutPostingBookId.Value;
                }
                else if (proposalDetailInventory.SinglePostingBookId.HasValue)
                {
                    mediaMonthId = proposalDetailInventory.SinglePostingBookId.Value;
                }
            }

            if (mediaMonthId.HasValue)
            {
                var ratingAdjustmentsDto = BroadcastDataRepositoryFactory.GetDataRepository<IRatingAdjustmentsRepository>().GetRatingAdjustment(mediaMonthId.Value);
                if (ratingAdjustmentsDto != null)
                {
                    impressions.ForEach(si => si.impressions = si.impressions * (double)(1 - ratingAdjustmentsDto.NtiAdjustment / 100));
                }
            }
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
    }
}