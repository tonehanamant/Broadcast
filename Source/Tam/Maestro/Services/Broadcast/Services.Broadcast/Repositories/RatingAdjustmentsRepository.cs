using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IRatingAdjustmentsRepository : IDataRepository
    {
        void SaveRatingAdjustments(List<RatingAdjustmentsDto> ratingAdjustments);
        List<RatingAdjustmentsDto> GetRatingAdjustments();
        RatingAdjustmentsDto GetRatingAdjustment(int mediaMonthId);
    }

    public class RatingAdjustmentsRepository : BroadcastRepositoryBase, IRatingAdjustmentsRepository
    {

        public RatingAdjustmentsRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public void SaveRatingAdjustments(List<RatingAdjustmentsDto> ratingAdjustments)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    context.rating_adjustments.RemoveRange(context.rating_adjustments.ToList());

                    var efRatingsAdjustments = ratingAdjustments.Select(r => new rating_adjustments
                    {
                        media_month_id = r.MediaMonthId,
                        annual_adjustment = r.AnnualAdjustment,
                        nti_adjustment = r.NtiAdjustment
                    }).ToList();

                    context.rating_adjustments.AddRange(efRatingsAdjustments);
                    context.SaveChanges();
                });
        }

        public List<RatingAdjustmentsDto> GetRatingAdjustments()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var ratingAdjustments = context.rating_adjustments.Select(r => new RatingAdjustmentsDto
                    {
                        MediaMonthId = r.media_month_id,
                        AnnualAdjustment = r.annual_adjustment,
                        NtiAdjustment = r.nti_adjustment
                    }).ToList();

                    return ratingAdjustments;
                });
        }

        public RatingAdjustmentsDto GetRatingAdjustment(int mediaMonthId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var r = context.rating_adjustments.Find(mediaMonthId);
                    if (r == null)
                        return null;
                    return new RatingAdjustmentsDto
                    {
                        MediaMonthId = r.media_month_id,
                        AnnualAdjustment = r.annual_adjustment,
                        NtiAdjustment = r.nti_adjustment
                    };
                });
        }
    }
}
