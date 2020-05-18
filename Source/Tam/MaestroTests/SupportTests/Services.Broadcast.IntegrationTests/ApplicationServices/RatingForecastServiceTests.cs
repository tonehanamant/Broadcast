using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class RatingForecastServiceTests
    {
        private readonly IRatingForecastService _Sut = IntegrationTestApplicationServiceFactory.GetApplicationService<IRatingForecastService>();

        [Test]
        [Category("long_running")]
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