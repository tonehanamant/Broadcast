﻿using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.EntityFrameworkMapping;
using media_months = EntityFrameworkMapping.Broadcast.media_months;
using media_weeks = EntityFrameworkMapping.Broadcast.media_weeks;

namespace Services.Broadcast.Repositories
{
    public interface IMediaMonthAndWeekAggregateRepository : IDataRepository
    {
        MediaMonthAndWeekAggregate GetMediaMonthAggregate();
    }

    public class MediaMonthAndWeekAggregateAndWeekAggregateRepository : BroadcastRepositoryBase, IMediaMonthAndWeekAggregateRepository
    {
        public MediaMonthAndWeekAggregateAndWeekAggregateRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public MediaMonthAndWeekAggregate GetMediaMonthAggregate()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var mediaMonths = (from x in context.media_months
                                   select x).ToList();

                var mediaMonthEntities = mediaMonths.Select(_ToMediaMonth).ToList();

                var weeks = (from x in context.media_weeks
                             select x).ToList();
                var mediaWeeks = weeks.Select(_ToMediaWeek).ToList();

                var result = new MediaMonthAndWeekAggregate(mediaMonthEntities, mediaWeeks);
                return result;
            });
        }

        private static MediaMonth _ToMediaMonth(media_months mediaMonth)
        {
            return new MediaMonth
            {
                Id = mediaMonth.id,
                StartDate = mediaMonth.start_date,
                EndDate = mediaMonth.end_date,
                Year = mediaMonth.year,
                Month = mediaMonth.month,
                MediaMonthX = mediaMonth.media_month
            };
        }

        private static MediaWeek _ToMediaWeek(media_weeks mediaWeek)
        {
            return new MediaWeek
            {
                Id = mediaWeek.id,
                MediaMonthId = mediaWeek.media_month_id,
                WeekNumber = mediaWeek.week_number,
                StartDate = mediaWeek.start_date,
                EndDate = mediaWeek.end_date
            };
        }
    }
}
