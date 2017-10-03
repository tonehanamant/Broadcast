using Common.Services;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.Converters.RateImport
{
    public interface IRateFileImporterFactory
    {
        RateFileImporterBase GetFileImporterInstance(RatesFile.RateSourceType rateSource);
    }

    public class RateFileImporterFactory : IRateFileImporterFactory
    {
        private BroadcastDataDataRepositoryFactory _broadcastDataDataRepositoryFactory;
        private readonly IDaypartCache _daypartCache;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;

        public RateFileImporterFactory(BroadcastDataDataRepositoryFactory broadcastDataFactory,
            IDaypartCache daypartCache, MediaMonthAndWeekAggregateCache mediaWeekCache, IBroadcastAudiencesCache audiencesCache)
        {
            _broadcastDataDataRepositoryFactory = broadcastDataFactory;
            _daypartCache = daypartCache;
            _MediaMonthAndWeekAggregateCache = mediaWeekCache;
            _AudiencesCache = audiencesCache;
        }

        public RateFileImporterBase GetFileImporterInstance(RatesFile.RateSourceType rateSource)
        {
            RateFileImporterBase fileImporter;
            switch (rateSource)
            {
                case RatesFile.RateSourceType.CNN:
                    fileImporter = new CNNFileImporter();
                    break;
                case RatesFile.RateSourceType.TTNW:
                    fileImporter = new TTNWFileImporter();
                    break;
                default:
                    fileImporter = new OpenMarketFileImporter();
                    break;
            }

            fileImporter.BroadcastDataDataRepository = _broadcastDataDataRepositoryFactory;
            fileImporter.DaypartCache = _daypartCache;
            fileImporter.MediaMonthAndWeekAggregateCache = _MediaMonthAndWeekAggregateCache;
            fileImporter.AudiencesCache = _AudiencesCache;

            return fileImporter;
        }
    }
}
