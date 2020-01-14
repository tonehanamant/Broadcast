using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using System;
using System.Data.Entity;

namespace Services.Broadcast.Repositories
{
    public interface IDaypartDefaultRepository : IDataRepository
    {
        bool ActiveDaypartDefaultExists(string daypartCode);
        DaypartDefaultDto GetDaypartDefaultByCode(string daypartCode);
        List<DaypartDefaultDto> GetDaypartDefaultsByInventorySource(int inventorySourceId);
        List<DaypartDefaultDto> GetAllActiveDaypartDefaults();
        DaypartDefaultDto GetDaypartDefaultById(int daypartDefaultId);

        /// <summary>
        /// Gets the daypart defaults by id.
        /// </summary>
        /// <param name="daypartDefaultId">The daypart default identifier.</param>
        /// <returns></returns>
        DaypartDefaultFullDto GetDaypartDefaultWithAllDataById(int daypartDefaultId);

        /// <summary>
        /// Gets the daypart defaults.
        /// </summary>
        /// <returns>List of <see cref="DaypartDefaultFullDto"/></returns>
        List<DaypartDefaultFullDto> GetAllActiveDaypartDefaultsWithAllData();
    }

    public class DaypartDefaultRepository : BroadcastRepositoryBase, IDaypartDefaultRepository
    {
        private const string DaypartDefaultNotFoundMessage = "Unable to find daypart default";

        public DaypartDefaultRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public bool ActiveDaypartDefaultExists(string daypartCode)
        {
            return _InReadUncommitedTransaction(context => context.daypart_defaults.Any(x => x.daypart.code == daypartCode));
        }

        public List<DaypartDefaultDto> GetDaypartDefaultsByInventorySource(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(context =>
            {

                var query = (from week in context.station_inventory_manifest_weeks
                             join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                             join inventoryFile in context.inventory_files on manifest.file_id equals inventoryFile.id
                             join inventoryFileHeader in context.inventory_file_proprietary_header on inventoryFile.id equals inventoryFileHeader.inventory_file_id
                             join daypartDefault in context.daypart_defaults on inventoryFileHeader.daypart_default_id equals daypartDefault.id
                             join ratingProcessingJob in context.inventory_file_ratings_jobs on inventoryFile.id equals ratingProcessingJob.inventory_file_id
                             where manifest.inventory_source_id == inventorySourceId &&
                                   week.spots > 0 &&
                                   ratingProcessingJob.status == (int)BackgroundJobProcessingStatus.Succeeded
                             group daypartDefault by daypartDefault.id into daypartCodeGroup
                             select daypartCodeGroup.FirstOrDefault());

                return query.Include(d => d.daypart).Select(_MapToDaypartDefaultDto).ToList();
            });
        }

        public DaypartDefaultDto GetDaypartDefaultByCode(string daypartCode)
        {
            return _InReadUncommitedTransaction(context => _MapToDaypartDefaultDto(context.daypart_defaults.Include(d => d.daypart).Single(x => x.daypart.code == daypartCode, DaypartDefaultNotFoundMessage)));
        }

        public DaypartDefaultDto GetDaypartDefaultById(int daypartDefaulId)
        {
            return _InReadUncommitedTransaction(context => _MapToDaypartDefaultDto(context.daypart_defaults.Include(d => d.daypart).Single(x => x.id == daypartDefaulId, DaypartDefaultNotFoundMessage)));
        }

        ///<inheritdoc/>
        public DaypartDefaultFullDto GetDaypartDefaultWithAllDataById(int daypartDefaultId)
        {
            return _InReadUncommitedTransaction(context => _MapToDaypartDefaultFullDto(context.daypart_defaults.Include(d => d.daypart).Include(d => d.daypart.timespan).Single(x => x.id == daypartDefaultId, DaypartDefaultNotFoundMessage)));
        }

        public List<DaypartDefaultDto> GetAllActiveDaypartDefaults()
        {
            return _InReadUncommitedTransaction(context => {
                return context.daypart_defaults
                    .Include(d => d.daypart)
                    .Select(_MapToDaypartDefaultDto)
                    .OrderBy(x => x.Code)
                    .ToList();
            });
        }

        ///<inheritdoc/>
        public List<DaypartDefaultFullDto> GetAllActiveDaypartDefaultsWithAllData()
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.daypart_defaults
                    .Include(d => d.daypart)
                    .Include(d => d.daypart.timespan)
                    .Select(_MapToDaypartDefaultFullDto)
                    .OrderBy(x => x.Code)
                    .ToList();
            });
        }

        private DaypartDefaultDto _MapToDaypartDefaultDto(daypart_defaults daypartDefault)
        {
            if (daypartDefault == null)
                return null;

            return new DaypartDefaultDto
            {
                Id = daypartDefault.id,
                Code = daypartDefault.daypart.code,
                FullName = daypartDefault.daypart.name
            };
        }

        private DaypartDefaultFullDto _MapToDaypartDefaultFullDto(daypart_defaults daypartDefault)
        {
            if (daypartDefault == null)
                return null;

            return new DaypartDefaultFullDto
            {
                Id = daypartDefault.id,
                Code = daypartDefault.daypart.code,
                FullName = daypartDefault.daypart.name,
                DaypartType = (DaypartTypeEnum)daypartDefault.daypart_type,
                DefaultStartTimeSeconds = daypartDefault.daypart.timespan.start_time,
                DefaultEndTimeSeconds = daypartDefault.daypart.timespan.end_time,
            };
        }
    }
}
