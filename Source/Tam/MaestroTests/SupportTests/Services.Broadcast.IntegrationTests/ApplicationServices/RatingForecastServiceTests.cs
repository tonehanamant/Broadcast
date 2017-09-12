using IntegrationTests.Common;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class RatingForecastServiceTests
    {
        private readonly IRatingForecastService _Sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IRatingForecastService>();

        private readonly List<int> _Audiences = new List<int>();
        private readonly List<int> _StartTimes = new List<int>();
        private readonly List<short> _MediaMonthIds = new List<short> { 401, 404, 406, 413, 416};
        private readonly List<bool> _Bools = new List<bool> { true, false };
        private readonly List<PlaybackType> _PlaybackTypes = new List<PlaybackType> { PlaybackType.O, PlaybackType.S, PlaybackType.One, PlaybackType.Three, PlaybackType.Seven };
        private readonly List<DisplayDaypart> _Dayparts = new List<DisplayDaypart>();
        private readonly List<short> _Programs = new List<short>();

        [SetUp]
        public void SetUp()
        {
            using (var fs = new StreamReader(@"$(ProjectDir)\..\..\..\ApplicationServices\audiences.csv"))
            {
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    _Audiences.Add(Convert.ToInt32(line));
                }
            }

            using (var fs = new StreamReader(@"$(ProjectDir)\..\..\..\ApplicationServices\programs.csv"))
            {
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    _Programs.Add(Convert.ToInt16(line));
                }
            }

            for (var i = 0; i < 86400; i += 1800)
                _StartTimes.Add(i);

            for (var i = 0; i < 800; i++)
                GenerateRandomDaypart();

            for (var i = 0; i < 100; i++)
                GenerateRandomDaypart(25200, 25200 + 7199);

            for (var i = 0; i < 100; i++)
                GenerateRandomDaypart(61200, 61200 + 10799);
        }

        private void GenerateRandomDaypart(int? startTime = null, int? endTime = null)
        {
            bool mon;
            bool tue;
            bool wed;
            bool thu;
            bool fri;
            bool sat;
            bool sun;
            do
            {
                mon = _Bools.Random();
                tue = _Bools.Random();
                wed = _Bools.Random();
                thu = _Bools.Random();
                fri = _Bools.Random();
                sat = _Bools.Random();
                sun = _Bools.Random();
            } while (!mon && !tue && !wed && !thu && !fri && !sat && !sun);

            if (startTime == null)
                startTime = _StartTimes.Random();
            if (endTime == null)
                endTime = startTime + 3599;

            var daypart = new DisplayDaypart(0, startTime.Value, endTime.Value, mon, tue, wed, thu, fri, sat, sun);
            _Dayparts.Add(daypart);
        }

        [Test]
        [Ignore("Inconsistently failing on dev, will look into tomorrow, most likely something with the months being off")]
        [Category(TestCategories.LongRunning)]
        public void MultipleRatings()
        {
            const int totalRequests = 16000;
            short hut;

            short share;
            do
            {
                hut = _MediaMonthIds.Random();
                share = _MediaMonthIds.Random();
            } while (hut == share || hut > share);

            var playbackType = _PlaybackTypes.Random();
            var audience = _Audiences.Random();
            var request = new RatingForecastRequest(hut, share, audience, playbackType);

            var distinct = new HashSet<Program>();
            while (distinct.Count < totalRequests)
            {
                var daypart = _Dayparts.Random();
                var p = _Programs.Random();
                var program = new Program(p, daypart);
                if (distinct.Add(program))
                    request.Programs.Add(program);
            }

            var sw = new Stopwatch();
            sw.Start();
            var responses = _Sut.ForecastRatings(request);
            Console.WriteLine("{0} R/S requested {1}, {2} nulls and {3} ratings, {4} total in {5} seconds",
                                    Math.Round(responses.ProgramRatings.Count / sw.Elapsed.TotalSeconds, 0),
                                    totalRequests,
                                    responses.ProgramRatings.Count(r => r.Rating == null),
                                    responses.ProgramRatings.Count(r => r.Rating != null),
                                    responses.ProgramRatings.Count,
                                    sw.Elapsed.TotalSeconds);
            sw.Stop();

            var badRatings = responses.ProgramRatings.Where(r => r.Rating != null && 0 <= r.Rating && r.Rating >= 1).ToList();
            if (badRatings.Any())
                badRatings.ForEach(r => PrintProgramRating(responses, r));
            Assert.IsEmpty(badRatings);

            if (responses.ProgramRatings.All(r => r.Rating == null))
            {
                responses.ProgramRatings.Take(5).ForEach(r => PrintProgramRating(responses, r));
                Assert.Fail();
            }
        }

        private static void PrintProgramRating(RatingForecastResponse forecastResponses, ProgramRating r)
        {
            Console.WriteLine(forecastResponses.HutMediaMonthId + " " + forecastResponses.ShareMediaMonthId + " " + forecastResponses.MaestroAudienceId + " " + r.StationCode + " " + r.DisplayDaypart + " " + r.DisplayDaypart.StartTime + " " + r.DisplayDaypart.EndTime + " " + forecastResponses.PlaybackType + " " + " " + r.Rating);
        }

        [TestCase(4999, 1)]
        [TestCase(5000, 1)]
        [TestCase(5001, 2)]
        [TestCase(10000, 2)]
        [TestCase(10001, 3)]
        [TestCase(20000, 3)]
        public void CalculateThreadCounts(int requestCount, int threadCount)
        {
            Assert.AreEqual(RatingForecastService.CalculateThreadCounts(requestCount), threadCount);
        }

        [Test]
        public void GetRatings_Fails_WhenContainingDuplicatePrograms()
        {
            var request = new RatingForecastRequest(0, 1, 0, PlaybackType.O);
            var daypart1 = _Dayparts.Random();
            request.Programs.Add(new Program(10776, daypart1));
            request.Programs.Add(new Program(10776, daypart1));

            var daypart2 = _Dayparts.Random();
            request.Programs.Add(new Program(10777, daypart2));
            request.Programs.Add(new Program(10777, daypart2));

            var duplicates = new List<DisplayDaypart> { daypart1, daypart2 };
            Assert.Throws<DuplicateProgramsException>(() => _Sut.ForecastRatings(request), string.Format(RatingForecastService.DuplicateProgramsMessage, string.Join(";", duplicates)));
        }

        [Test]
        public void GetRatings_Fails_WhenContainingNoPrograms()
        {
            var request = new RatingForecastRequest(0, 1, 0, PlaybackType.O);

            Assert.Throws<NoProgramsException>(() => _Sut.ForecastRatings(request));
        }

        [Test]
        public void GetRatings_Fails_WhenHutMediaMonthGreaterThanShareMediaMonth()
        {
            var request = new RatingForecastRequest(1, 0, 0, PlaybackType.O);

            Assert.Throws<HutGreaterThanShareException>(() => _Sut.ForecastRatings(request), string.Format(RatingForecastService.HutMediaMonthMustBeEarlierLessThanThanShareMediaMonth, request.HutMediaMonthId, request.ShareMediaMonthId));
        }

        [Test]
        public void GetRatings_Fails_WhenSameHutAndShareMediaMonth()
        {
            var request = new RatingForecastRequest(0, 0, 0, PlaybackType.O);
            request.Programs.Add(new Program(10776, _Dayparts.Random()));
            Assert.Throws<IdenticalHutAndShareException>(() => _Sut.ForecastRatings(request), string.Format(RatingForecastService.IdenticalHutAndShareMessage, request.HutMediaMonthId));
        }

        [Test]
        public void GetRatings_Fails_WhenDaypartHasNoSelectedDays()
        {
            var request = new RatingForecastRequest(0, 1, 0, PlaybackType.O);
            var displayDaypart = new DisplayDaypart(0, 0, 10000, false, false, false, false, false, false, false);
            request.Programs.Add(new Program(10776, displayDaypart));

            var displayDaypart2 = new DisplayDaypart(0, 0, 10001, false, false, false, false, false, false, false);
            request.Programs.Add(new Program(10776, displayDaypart));

            var noDaysDayparts = new List<DisplayDaypart> { displayDaypart, displayDaypart2 };

            Assert.Throws<NoSelectedDaysException>(() => _Sut.ForecastRatings(request), string.Format(RatingForecastService.NoSelectedDaysMessage, string.Join("\n", noDaysDayparts)));
        }

        [Test]
        public void GetRatingsCrunchStatus_Works()
        {
            var x = _Sut.GetMediaMonthCrunchStatuses();
            x.ForEach(m => Console.WriteLine(m.ToString()));
        }
    }
    public static class EnumerableExtension
    {
        private static readonly Random _Random = new Random();

        public static T Random<T>(this IList<T> source)
        {
            return source[_Random.Next(0, source.Count)];
        }
    }
}