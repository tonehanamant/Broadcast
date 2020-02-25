using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;

namespace Services.Broadcast.Repositories
{
    public interface IStationMappingRepository : IDataRepository
    {
        /// <summary>
        /// Saves the station mapping.
        /// </summary>
        /// <param name="stationMapping">The station mapping.</param>
        /// <param name="createdBy">The user that created the station mapping.</param>
        /// <param name="createdDate">The created date.</param>
        void SaveStationMapping(StationMappingsFileRequestDto stationMapping, string createdBy, DateTime createdDate);

        /// <summary>
        /// Saves the new station mappings.
        /// </summary>
        /// <param name="stationMappingList">The station mapping list.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        void SaveNewStationMappings(List<StationMappingsDto> stationMappingList, string createdBy, DateTime createdDate);

        /// <summary>
        /// Saves the new station mapping.
        /// </summary>
        /// <param name="stationMapping">The station mapping.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        void SaveNewStationMapping(StationMappingsDto stationMapping, string createdBy, DateTime createdDate);

        /// <summary>
        /// Gets the station mappings by cadent call letters.
        /// </summary>
        /// <param name="cadentCallLetters">The cadent call letters.</param>
        /// <returns>The list of station mappings</returns>
        List<StationMappingsDto> GetStationMappingsByCadentCallLetters(string cadentCallLetters);

        /// <summary>
        /// Gets the cadent station from mapped call letters.
        /// </summary>
        /// <param name="mappedCallLetters">The mapped call letters.</param>
        /// <param name="mapSet">The map set.</param>
        /// <returns></returns>
        DisplayBroadcastStation GetCadentStationFromMappedCallLetters(string mappedCallLetters,
            StationMapSetNamesEnum mapSet);

        /// <summary>
        /// Removes all mappings for cadent call letters.
        /// </summary>
        /// <param name="cadentCallLetters">The cadent call letters.</param>
        void RemoveAllMappingsForCadentCallLetters(string cadentCallLetters);
    }

    public class StationMappingRepository : BroadcastRepositoryBase, IStationMappingRepository
    {
        public StationMappingRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient) 
            : base(pBroadcastContextFactory, pTransactionHelper, configurationWebApiClient)
        { }

        /// <inheritdoc />
        public void SaveStationMapping(StationMappingsFileRequestDto stationMapping, string createdBy, DateTime createdDate)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    if (!string.IsNullOrEmpty(stationMapping.ExtendedCallLetters))
                    {
                        var newStationMapping = new station_mappings
                        {
                            map_set = (int)StationMapSetNamesEnum.Extended,
                            mapped_call_letters = stationMapping.ExtendedCallLetters,
                            created_by = createdBy,
                            created_date = createdDate,
                        };

                        context.station_mappings.Add(newStationMapping);
                    }

                    if (!string.IsNullOrEmpty(stationMapping.NSICallLetters))
                    {
                        var newStationMapping = new station_mappings
                        {
                            map_set = (int)StationMapSetNamesEnum.NSI,
                            mapped_call_letters = stationMapping.NSICallLetters,
                            created_by = createdBy,
                            created_date = createdDate,
                        };

                        context.station_mappings.Add(newStationMapping);
                    }

                    if (!string.IsNullOrEmpty(stationMapping.NSILegacyCallLetters))
                    {
                        var newStationMapping = new station_mappings
                        {
                            map_set = (int)StationMapSetNamesEnum.NSILegacy,
                            mapped_call_letters = stationMapping.NSILegacyCallLetters,
                            created_by = createdBy,
                            created_date = createdDate,
                        };

                        context.station_mappings.Add(newStationMapping);
                    }

                    if (!string.IsNullOrEmpty(stationMapping.SigmaCallLetters))
                    {
                        var newStationMapping = new station_mappings
                        {
                            map_set = (int)StationMapSetNamesEnum.Sigma,
                            mapped_call_letters = stationMapping.SigmaCallLetters,
                            created_by = createdBy,
                            created_date = createdDate,
                        };

                        context.station_mappings.Add(newStationMapping);
                    }

                    context.SaveChanges();
                });
        }

        /// <inheritdoc />
        public List<StationMappingsDto> GetStationMappingsByCadentCallLetters(string cadentCallLetters)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entities = context.station_mappings
                    .Include(x => x.station)
                    .Where(x => x.station.legacy_call_letters == cadentCallLetters)
                    .ToList();
                return _MapToDto(entities);
            });
        }

        /// <inheritdoc />
        public DisplayBroadcastStation GetCadentStationFromMappedCallLetters(string mappedCallLetters,
            StationMapSetNamesEnum mapSet)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var mapping = context.station_mappings
                    .Include(x => x.station)
                    .Single(x => x.map_set == (int) mapSet &&
                                 x.mapped_call_letters == mappedCallLetters,
                        $"Cadent station not found for mapped call letters '{mappedCallLetters}' and map set '{mapSet}'.");

                var station = mapping.station;

                return _MapToStationDto(station);
            });
        }

        private List<StationMappingsDto> _MapToDto(List<station_mappings> entities)
        {
            var stationMappingsList = new List<StationMappingsDto>();
            entities.ForEach(stationMapping =>
            {
                stationMappingsList.Add(new StationMappingsDto
                {
                    StationId = stationMapping.station_id,
                    MapSet = (StationMapSetNamesEnum)stationMapping.map_set,
                    MapValue = stationMapping.mapped_call_letters
                });
            });

            return stationMappingsList;
        }

        private DisplayBroadcastStation _MapToStationDto(station entity)
        {
            return new DisplayBroadcastStation
            {
                Id = entity.id,
                Code = entity.station_code,
                CallLetters = entity.station_call_letters,
                Affiliation = entity.affiliation,
                MarketCode = entity.market_code,
                LegacyCallLetters = entity.legacy_call_letters,
                ModifiedDate = entity.modified_date
            };
        }

        /// <inheritdoc />
        public void SaveNewStationMappings(List<StationMappingsDto> stationMappingList, string createdBy, DateTime createdDate)
        {
            _InReadUncommitedTransaction(context => 
            {
                stationMappingList.ForEach(stationMapping => 
                {
                    context.station_mappings.Add(new station_mappings
                    {
                        station_id = stationMapping.StationId,
                        map_set = (int)stationMapping.MapSet,
                        mapped_call_letters = stationMapping.MapValue,
                        created_by = createdBy,
                        created_date = createdDate
                    });
                });

                context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public void SaveNewStationMapping(StationMappingsDto stationMapping, string createdBy, DateTime createdDate)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.station_mappings.Add(new station_mappings
                {
                    station_id = stationMapping.StationId,
                    map_set = (int)stationMapping.MapSet,
                    mapped_call_letters = stationMapping.MapValue,
                    created_by = createdBy,
                    created_date = createdDate
                });

                context.SaveChanges();
            });
        }

        public void RemoveAllMappingsForCadentCallLetters(string cadentCallLetters)
        {
            _InReadUncommitedTransaction(context =>
            {
                var entities = context.station_mappings
                .Include(x => x.station)
                .Where(x => x.station.legacy_call_letters == cadentCallLetters).ToList();

                context.station_mappings
                .RemoveRange(entities);

                context.SaveChanges();
            });
        }
    }
}
