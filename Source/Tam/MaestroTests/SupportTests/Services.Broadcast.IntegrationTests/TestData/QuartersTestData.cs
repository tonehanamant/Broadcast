using Services.Broadcast.Entities;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.TestData
{
    public static class QuartersTestData
    {
        public static List<QuarterDetailDto> GetAllQuartersBetweenDates(DateTime start, DateTime end)
        {
            var allQuarters = GetBroadcastQuarters();
            var result = allQuarters.Where(q => end >= q.StartDate && start <= q.EndDate).ToList();
            return result;
        }

        public static QuarterDetailDto GetQuarterRangeByDate(DateTime candidate)
        {
            var allQuarters = GetBroadcastQuarters();
            var quarter = allQuarters.First(q => candidate.IsBetween(q.StartDate, q.EndDate));
            return quarter;
        }

        public static DateRange GetQuarterDateRange(int quarterNumber, int year)
        {
            var quarter = GetQuarterDetail(quarterNumber, year);
            var result = new DateRange(quarter.StartDate, quarter.EndDate);
            return result;
        }

        public static QuarterDetailDto GetQuarterDetail(int quarterNumber, int year)
        {
            var allQuarters = GetBroadcastQuarters();
            var quarter = allQuarters.First(q => q.Quarter == quarterNumber && q.Year == year);
            return quarter;
        }

        public static List<QuarterDetailDto> GetBroadcastQuarters()
        {
            #region BigList

            var quarters = new List<QuarterDetailDto>
            {
                new QuarterDetailDto { Quarter = 1, Year = 2017, StartDate = DateTime.Parse("2016-12-26"), EndDate = DateTime.Parse("2017-03-26")},
                new QuarterDetailDto { Quarter = 2, Year = 2017, StartDate = DateTime.Parse("2017-03-27"), EndDate = DateTime.Parse("2017-06-25")},
                new QuarterDetailDto { Quarter = 3, Year = 2017, StartDate = DateTime.Parse("2017-06-26"), EndDate = DateTime.Parse("2017-09-24")},
                new QuarterDetailDto { Quarter = 4, Year = 2017, StartDate = DateTime.Parse("2017-09-25"), EndDate = DateTime.Parse("2017-12-31")},
                new QuarterDetailDto { Quarter = 1, Year = 2018, StartDate = DateTime.Parse("2018-01-01"), EndDate = DateTime.Parse("2018-03-25")},
                new QuarterDetailDto { Quarter = 2, Year = 2018, StartDate = DateTime.Parse("2018-03-26"), EndDate = DateTime.Parse("2018-06-24")},
                new QuarterDetailDto { Quarter = 3, Year = 2018, StartDate = DateTime.Parse("2018-06-25"), EndDate = DateTime.Parse("2018-09-30")},
                new QuarterDetailDto { Quarter = 4, Year = 2018, StartDate = DateTime.Parse("2018-10-01"), EndDate = DateTime.Parse("2018-12-30")},
                new QuarterDetailDto { Quarter = 1, Year = 2019, StartDate = DateTime.Parse("2018-12-31"), EndDate = DateTime.Parse("2019-03-31")},
                new QuarterDetailDto { Quarter = 2, Year = 2019, StartDate = DateTime.Parse("2019-04-01"), EndDate = DateTime.Parse("2019-06-30")},
                new QuarterDetailDto { Quarter = 3, Year = 2019, StartDate = DateTime.Parse("2019-07-01"), EndDate = DateTime.Parse("2019-09-29")},
                new QuarterDetailDto { Quarter = 4, Year = 2019, StartDate = DateTime.Parse("2019-09-30"), EndDate = DateTime.Parse("2019-12-29")},
                new QuarterDetailDto { Quarter = 1, Year = 2020, StartDate = DateTime.Parse("2019-12-30"), EndDate = DateTime.Parse("2020-03-29")},
                new QuarterDetailDto { Quarter = 2, Year = 2020, StartDate = DateTime.Parse("2020-03-30"), EndDate = DateTime.Parse("2020-06-28")},
                new QuarterDetailDto { Quarter = 3, Year = 2020, StartDate = DateTime.Parse("2020-06-29"), EndDate = DateTime.Parse("2020-09-27")},
                new QuarterDetailDto { Quarter = 4, Year = 2020, StartDate = DateTime.Parse("2020-09-28"), EndDate = DateTime.Parse("2020-12-27")},
                new QuarterDetailDto { Quarter = 1, Year = 2021, StartDate = DateTime.Parse("2020-12-28"), EndDate = DateTime.Parse("2021-03-28")},
                new QuarterDetailDto { Quarter = 2, Year = 2021, StartDate = DateTime.Parse("2021-03-29"), EndDate = DateTime.Parse("2021-06-27")},
                new QuarterDetailDto { Quarter = 3, Year = 2021, StartDate = DateTime.Parse("2021-06-28"), EndDate = DateTime.Parse("2021-09-26")},
                new QuarterDetailDto { Quarter = 4, Year = 2021, StartDate = DateTime.Parse("2021-09-27"), EndDate = DateTime.Parse("2021-12-26")},
            };

            #endregion // #region BigList

            return quarters;
        }
    }
}