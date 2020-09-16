using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IDaypartDefaultRepository : IDataRepository
    {
        bool DaypartDefaultExists(string daypartCode);
        DaypartDefaultDto GetDaypartDefaultByCode(string daypartCode);
        List<DaypartDefaultDto> GetDaypartDefaultsByInventorySource(int inventorySourceId);
        List<DaypartDefaultDto> GetAllDaypartDefaults();
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
        List<DaypartDefaultFullDto> GetAllDaypartDefaultsWithAllData();

        /// <summary>
        /// Get DaypartIds List by DefaultDaypartIds
        /// </summary>
        /// <param name="defaultDaypartIds"></param>
        /// <returns></returns>
        List<int> GetDayPartIds(List<int> dayPartDefaultIds);

        /// <summary>
        /// Get  daypart_defaults Ids  List by DaypartIds
        /// </summary>
        /// <param name="daypartIds"></param>
        /// <returns></returns>
        List<int> GetDaypartDefaultIds(List<int> daypartIds);

        /// <summary>
        /// Gets the distinct daypart ids related to the daypart defaults.
        /// </summary>
        /// <param name="daypartDefaultIds">The ids for the daypart_default records.</param>
        /// <returns>A distinct list of daypart ids covered by the given parameters.</returns>
        List<int> GetDayIdsFromDaypartDefaults(List<int> daypartDefaultIds);
    }

    public class DaypartDefaultRepository : BroadcastRepositoryBase, IDaypartDefaultRepository
    {
        private const string DaypartDefaultNotFoundMessage = "Unable to find daypart default";

        public DaypartDefaultRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public bool DaypartDefaultExists(string daypartCode)
        {
            return _InReadUncommitedTransaction(context => context.daypart_defaults.Any(x => x.code == daypartCode));
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

                return query.Select(_MapToDaypartDefaultDto).ToList();
            });
        }

        public DaypartDefaultDto GetDaypartDefaultByCode(string daypartCode)
        {
            return _InReadUncommitedTransaction(context => _MapToDaypartDefaultDto(context.daypart_defaults.Single(x => x.code == daypartCode, DaypartDefaultNotFoundMessage)));
        }

        public DaypartDefaultDto GetDaypartDefaultById(int daypartDefaulId)
        {
            return _InReadUncommitedTransaction(context => _MapToDaypartDefaultDto(context.daypart_defaults.Single(x => x.id == daypartDefaulId, DaypartDefaultNotFoundMessage)));
        }

        ///<inheritdoc/>
        public DaypartDefaultFullDto GetDaypartDefaultWithAllDataById(int daypartDefaultId)
        {
            return _InReadUncommitedTransaction(context => _MapToDaypartDefaultFullDto(context.daypart_defaults.Include(d => d.daypart).Include(d => d.daypart.timespan).Single(x => x.id == daypartDefaultId, DaypartDefaultNotFoundMessage)));
        }
        ///<inheritdoc/>
		public List<int> GetDaypartDefaultIds(List<int> daypartIds)
		{
			return _InReadUncommitedTransaction(context => (context.daypart_defaults
				.Where(d => daypartIds.Contains(d.daypart_id))
				.Select(d => d.id).ToList()));
		}

		public List<int> GetDayPartIds(List<int> dayPartDefaultIds)
        {
	        return _InReadUncommitedTransaction(context => (context.daypart_defaults
		        .Where(d => dayPartDefaultIds.Contains( d.id)))
		        .Select(d => d.daypart_id).ToList());
        }
        public List<DaypartDefaultDto> GetAllDaypartDefaults()
        {
            return _InReadUncommitedTransaction(context => {
                return context.daypart_defaults
                    .Select(_MapToDaypartDefaultDto)
                    .OrderBy(x => x.Code)
                    .ToList();
            });
        }

        ///<inheritdoc/>
        public List<DaypartDefaultFullDto> GetAllDaypartDefaultsWithAllData()
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

        ///<inheritdoc/>
        public List<int> GetDayIdsFromDaypartDefaults(List<int> daypartDefaultIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var dayIds = context.daypart_defaults
                    .Include(d => d.daypart)
                    .Include(d => d.daypart.days)
                    .Where(d => daypartDefaultIds.Contains(d.id))
                    .SelectMany(d => d.daypart.days.Select(s => s.id))
                    .Distinct()
                    .ToList();

                return dayIds;
            });
        }

        private DaypartDefaultDto _MapToDaypartDefaultDto(daypart_defaults daypartDefault)
        {
            if (daypartDefault == null)
                return null;

            return new DaypartDefaultDto
            {
                Id = daypartDefault.id,
                Code = daypartDefault.code,
                FullName = daypartDefault.name,
                VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)daypartDefault.vpvh_calculation_source_type
            };
        }

        private DaypartDefaultFullDto _MapToDaypartDefaultFullDto(daypart_defaults daypartDefault)
        {
            if (daypartDefault == null)
                return null;

            return new DaypartDefaultFullDto
            {
                Id = daypartDefault.id,
                Code = daypartDefault.code,
                FullName = daypartDefault.name,
                DaypartType = (DaypartTypeEnum)daypartDefault.daypart_type,
                DefaultStartTimeSeconds = daypartDefault.daypart.timespan.start_time,
                DefaultEndTimeSeconds = daypartDefault.daypart.timespan.end_time
            };
        }
    }
}
