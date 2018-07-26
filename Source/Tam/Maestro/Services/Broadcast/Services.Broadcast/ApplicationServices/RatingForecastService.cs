using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Transactions;
using Common.Services;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.ApplicationServices
{
    public interface IRatingForecastService : IApplicationService
    {
        RatingForecastResponse ForecastRatings(RatingForecastRequest forecastRequest);
        List<MediaMonthCrunchStatus> GetMediaMonthCrunchStatuses();
        void CrunchMediaMonth(short mediaMonthId);
    }

    public class RatingForecastService : IRatingForecastService
    {
        private readonly IRatingForecastRepository _RatingForecastRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IBroadcastAudienceRepository _AudienceRepository;
        internal static string IdenticalHutAndShareMessage = "Hut and Share Media Month {0} cannot be identical";
        internal static string HutMediaMonthMustBeEarlierLessThanThanShareMediaMonth = "Hut Media Month {0} must be earlier (less than) than Share Media Month {1}";
        internal static string NoSelectedDaysMessage = "The following programs have dayparts with no selected days:\n {0}";
        internal static string DuplicateProgramsMessage = "The following programs are duplicates:\n {0} ";

        public RatingForecastService(IDataRepositoryFactory dataRepositoryFactory, IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _RatingForecastRepository = dataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            _AudienceRepository = dataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
        }

        public void CrunchMediaMonth(short mediaMonthId)
        {
            var mm = _MediaMonthAndWeekAggregateCache.GetMediaMonthById(mediaMonthId);
            _RatingForecastRepository.CrunchMonth(mediaMonthId, mm.StartDate, mm.EndDate);
        }

        public List<MediaMonthCrunchStatus> GetMediaMonthCrunchStatuses()
        {
            return MediaMonthCrunchCache.Instance.GetMediaMonthCrunchStatuses();
        }

        public RatingForecastResponse ForecastRatings(RatingForecastRequest forecastRequest)
        {
            ValidateRequest(forecastRequest);
            forecastRequest.SetNielsenRatingAudienceIds(_AudienceRepository.GetRatingsAudiencesByMaestroAudience(new List<int> { forecastRequest.MaestroAudienceId }).Select(d => d.rating_audience_id));
            var ratings = GetRatings(forecastRequest);
            return new RatingForecastResponse(forecastRequest, ratings);
        }

        private static void ValidateRequest(RatingForecastRequest forecastRequest)
        {
            if (forecastRequest.ShareMediaMonthId == forecastRequest.HutMediaMonthId)
                throw new IdenticalHutAndShareException(string.Format(IdenticalHutAndShareMessage, forecastRequest.HutMediaMonthId));

            if (forecastRequest.HutMediaMonthId > forecastRequest.ShareMediaMonthId)
                throw new HutGreaterThanShareException(string.Format(HutMediaMonthMustBeEarlierLessThanThanShareMediaMonth,
                    forecastRequest.HutMediaMonthId, forecastRequest.ShareMediaMonthId));

            if (forecastRequest.Programs.Count == 0)
                throw new NoProgramsException();

            var emptyDayparts = forecastRequest.Programs.Where(program => program.DisplayDaypart.ActiveDays == 0).ToList();
            if (emptyDayparts.Any())
                throw new NoSelectedDaysException(string.Format(NoSelectedDaysMessage, string.Join("\n", emptyDayparts)));

            var distinct = new HashSet<Program>();
            var duplicatePrograms = forecastRequest.Programs.Where(program => !distinct.Add(program)).ToList();
            if (duplicatePrograms.Any())
                throw new DuplicateProgramsException(string.Format(DuplicateProgramsMessage, string.Join(";", duplicatePrograms)));
        }

        private List<ProgramRating> GetRatings(RatingForecastRequest forecastRequest)
        {
            var tasks = new List<Task<RatingForecastResponse>>();

            var numThreads = CalculateThreadCounts(forecastRequest.Programs.Count);
            var programs = forecastRequest.Programs.Split(numThreads).ToList();
            for (var i = 0; i < numThreads; i++)
                tasks.Add(GetRatingForecastResponse(new RatingForecastRequest(forecastRequest, programs[i].ToList())));

            Task.WaitAll(tasks.ToArray());

            return tasks.SelectMany(a => a.Result.ProgramRatings).ToList();
        }

        internal static int CalculateThreadCounts(int totalRequests)
        {
            const double minRequestPerThread = 5000.0;
            const int maxThreads = 3;

            if (totalRequests / minRequestPerThread > maxThreads)
                return maxThreads;

            return (int)Math.Ceiling(totalRequests / minRequestPerThread);
        }

        private async Task<RatingForecastResponse> GetRatingForecastResponse(RatingForecastRequest forecastRequest)
        {
            var task = new Task<RatingForecastResponse>(() =>
            {
                var results = _RatingForecastRepository.ForecastRatings(forecastRequest.HutMediaMonthId, forecastRequest.ShareMediaMonthId, forecastRequest.NielsenRatingAudienceIds, forecastRequest.MinPlaybackType, forecastRequest.Programs, BroadcastComposerWebSystemParameter.UseDayByDayImpressions);
                var programRatings = GetProgramRatings(forecastRequest, results).ToList();
                return new RatingForecastResponse(forecastRequest, programRatings);
            });

            task.Start();
            return await task.ConfigureAwait(false);
        }

        private static IEnumerable<ProgramRating> GetProgramRatings(RatingForecastRequest forecastRequest, IEnumerable<RatingsResult> results)
        {
            var dict = results.ToDictionary(r => new Program(r.station_code, new DisplayDaypart(0, r.start_time, r.end_time, r.mon, r.tue, r.wed, r.thu, r.fri, r.sat, r.sun)).ToString(), r => r.rating);
            foreach (var program in forecastRequest.Programs)
            {
                double? calculatedRating;
                dict.TryGetValue(program.ToString(), out calculatedRating);

                yield return new ProgramRating(program.StationCode, program.DisplayDaypart, calculatedRating);
            }
        }

    }
}
