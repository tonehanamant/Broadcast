using Common.Services.Repositories;
using ConfigurationService.Client;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;

namespace Services.Broadcast.Repositories
{
    public interface INsiStationRepository : IDataRepository
    {
        /// <summary>
        /// Gets the NSI Station list by media month from the Broadcast Forecast database.
        /// </summary>
        /// <param name="mediaMonthId">The media month identifier.</param>
        /// <returns>A list of stations</returns>
        List<NsiStationDto> GetNsiStationListByMediaMonth(int mediaMonthId);

        /// <summary>
        /// Gets the latest media month identifier from station list.
        /// </summary>
        /// <returns>The media month id</returns>
        int GetLatestMediaMonthIdFromStationList();
    }

    public class NsiStationRepository : BroadcastForecastRepositoryBase, INsiStationRepository
    {
        public NsiStationRepository(
            IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper,
            IConfigurationWebApiClient configurationWebApiClient
            , IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        public List<NsiStationDto> GetNsiStationListByMediaMonth(int mediaMonthId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.stations
                        .Where(station => station.media_month_id == mediaMonthId)
                        .ToList().Select(station => _MapToDto(station)).ToList();
                });
        }

        public int GetLatestMediaMonthIdFromStationList()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.stations
                    .OrderByDescending(station => station.media_month_id)
                    .First().media_month_id;
                });
        }

        private NsiStationDto _MapToDto(stations station)
        {
            return new NsiStationDto
            {
                MediaMonthId = station.media_month_id,
                DistributorCode = station.distributor_code,
                MarketCode = station.market_code,
                MarketOfOriginCode = station.market_of_origin_code,
                CallLetters = station.call_letters,
                LegacyCallLetters = station.legacy_call_letters,
                StartDatetimeOfSurvey = station.start_datetime_of_survey,
                EndDatetimeOfSurvey = station.end_datetime_of_survey,
                ParentPlusIndicator = station.parent_plus_indicator,
                CableLongName = station.cable_long_name,
                BroadcastChannelNumber = station.broadcast_channel_number,
                DistributionSourceType = station.distribution_source_type,
                PrimaryAffiliation = station.primary_affiliation,
                SecondaryAffiliation = station.secondary_affiliation,
                TertiaryAffiliation = station.tertiary_affiliation,
                DistributorGroup = station.distributor_group,
                ParentIndicator = station.parent_indicator,
                SatelliteIndicatior = station.satellite_indicator,
                StationTypeCode = station.station_type_code,
                ReportabilityStatus = station.reportability_status,
            };
        }
    }
}
