using Common.Services.Extensions;
using Common.Services.Repositories;
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
    public interface IStandardDaypartRepository : IDataRepository
    {
        bool StandardDaypartExists(string daypartCode);
        StandardDaypartDto GetStandardDaypartByCode(string daypartCode);
        List<StandardDaypartDto> GetStandardDaypartsByInventorySource(int inventorySourceId);
        List<StandardDaypartDto> GetAllStandardDayparts();
        StandardDaypartDto GetStandardDaypartById(int standardDaypartId);

        /// <summary>
        /// Gets the standard daypart with all data by identifier.
        /// </summary>
        /// <param name="standardDaypartId">The standard daypart identifier.</param>
        /// <returns></returns>
        StandardDaypartFullDto GetStandardDaypartWithAllDataById(int standardDaypartId);

        /// <summary>
        /// Gets all standard daypart fs with all data.
        /// </summary>
        /// <returns></returns>
        List<StandardDaypartFullDto> GetAllStandardDaypartsWithAllData();

        /// <summary>
        /// Get DaypartIds List by standard daypart ids
        /// </summary>
        /// <param name="standardDaypartIds"></param>
        /// <returns></returns>
        List<int> GetDayPartIds(List<int> standardDaypartIds);

        /// <summary>
        /// Get  standard_dayparts Ids  List by DaypartIds
        /// </summary>
        /// <param name="daypartIds"></param>
        /// <returns></returns>
        List<int> GetStandardDaypartIds(List<int> daypartIds);

        /// <summary>
        /// For all the Standard Dayparts get a dictionary where the Key is
        /// the StandardDaypart.Id and the Value is related the Daypart.Id.
        /// </summary>
        Dictionary<int, int> GetStandardDaypartIdDaypartIds();

        /// <summary>
        /// Gets the distinct daypart ids related to the daypart defaults.
        /// </summary>
        /// <param name="standardDaypartIds">The ids for the daypart_default records.</param>
        /// <returns>A distinct list of daypart ids covered by the given parameters.</returns>
        List<int> GetDayIdsFromStandardDayparts(List<int> standardDaypartIds);
    }

    public class StandardDaypartRepository : BroadcastRepositoryBase, IStandardDaypartRepository
    {
        private const string StandardDaypartNotFoundMessage = "Unable to find standard daypart";

        public StandardDaypartRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public bool StandardDaypartExists(string daypartCode)
        {
            return _InReadUncommitedTransaction(context => context.standard_dayparts.Any(x => x.code == daypartCode));
        }

        public List<StandardDaypartDto> GetStandardDaypartsByInventorySource(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(context =>
            {

                var query = (from week in context.station_inventory_manifest_weeks
                             join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                             join inventoryFile in context.inventory_files on manifest.file_id equals inventoryFile.id
                             join inventoryFileHeader in context.inventory_file_proprietary_header on inventoryFile.id equals inventoryFileHeader.inventory_file_id
                             join standardDaypart in context.standard_dayparts on inventoryFileHeader.standard_daypart_id equals standardDaypart.id
                             join ratingProcessingJob in context.inventory_file_ratings_jobs on inventoryFile.id equals ratingProcessingJob.inventory_file_id
                             where manifest.inventory_source_id == inventorySourceId &&
                                   week.spots > 0 &&
                                   ratingProcessingJob.status == (int)BackgroundJobProcessingStatus.Succeeded
                             group standardDaypart by standardDaypart.id into daypartCodeGroup
                             select daypartCodeGroup.FirstOrDefault());

                return query.Select(_MapToStandardDaypartDto).ToList();
            });
        }

        public StandardDaypartDto GetStandardDaypartByCode(string daypartCode)
        {
            return _InReadUncommitedTransaction(context =>
                    _MapToStandardDaypartDto(context.standard_dayparts.Single(x => x.code == daypartCode, StandardDaypartNotFoundMessage)));
        }

        public StandardDaypartDto GetStandardDaypartById(int standardDaypartId)
        {
            return _InReadUncommitedTransaction(context =>
                _MapToStandardDaypartDto(context.standard_dayparts.Single(x => x.id == standardDaypartId, StandardDaypartNotFoundMessage)));
        }

        ///<inheritdoc/>
        public StandardDaypartFullDto GetStandardDaypartWithAllDataById(int standardDaypartId)
        {
            return _InReadUncommitedTransaction(context => 
                _MapToStandardDaypartFullDto(context.standard_dayparts.Include(d => d.daypart).Include(d => d.daypart.timespan)
                                .Single(x => x.id == standardDaypartId, StandardDaypartNotFoundMessage)));
        }
        ///<inheritdoc/>
		public List<int> GetStandardDaypartIds(List<int> daypartIds)
        {
            return _InReadUncommitedTransaction(context => (context.standard_dayparts
                .Where(d => daypartIds.Contains(d.daypart_id))
                .Select(d => d.id).ToList()));
        }

        ///<inheritdoc/>
        public Dictionary<int, int> GetStandardDaypartIdDaypartIds()
        {
            return _InReadUncommitedTransaction(context => context.standard_dayparts
                .ToDictionary(d => d.id, d => d.daypart_id));
        }

        public List<int> GetDayPartIds(List<int> standardDaypartIds)
        {
            return _InReadUncommitedTransaction(context => (context.standard_dayparts
                .Where(d => standardDaypartIds.Contains(d.id)))
                .Select(d => d.daypart_id).ToList());
        }

        public List<StandardDaypartDto> GetAllStandardDayparts()
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.standard_dayparts
                    .Select(_MapToStandardDaypartDto)
                    .OrderBy(x => x.Code)
                    .ToList();
            });
        }

        ///<inheritdoc/>
        public List<StandardDaypartFullDto> GetAllStandardDaypartsWithAllData()
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.standard_dayparts
                    .Include(d => d.daypart)
                    .Include(d => d.daypart.timespan)
                    .Select(_MapToStandardDaypartFullDto)
                    .OrderBy(x => x.Code)
                    .ToList();
            });
        }

        ///<inheritdoc/>
        public List<int> GetDayIdsFromStandardDayparts(List<int> standardDaypartIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var dayIds = context.standard_dayparts
                    .Include(d => d.daypart)
                    .Include(d => d.daypart.days)
                    .Where(d => standardDaypartIds.Contains(d.id))
                    .SelectMany(d => d.daypart.days.Select(s => s.id))
                    .Distinct()
                    .ToList();

                return dayIds;
            });
        }

        private StandardDaypartDto _MapToStandardDaypartDto(standard_dayparts standardDaypart)
        {
            if (standardDaypart == null)
                return null;

            return new StandardDaypartDto
            {
                Id = standardDaypart.id,
                Code = standardDaypart.code,
                FullName = standardDaypart.name,
                VpvhCalculationSourceType = (VpvhCalculationSourceTypeEnum)standardDaypart.vpvh_calculation_source_type
            };
        }

        private StandardDaypartFullDto _MapToStandardDaypartFullDto(standard_dayparts standard_daypart)
        {
            if (standard_daypart == null)
                return null;

            return new StandardDaypartFullDto
            {
                Id = standard_daypart.id,
                Code = standard_daypart.code,
                FullName = standard_daypart.name,
                DaypartType = (DaypartTypeEnum)standard_daypart.daypart_type,
                DefaultStartTimeSeconds = standard_daypart.daypart.timespan.start_time,
                DefaultEndTimeSeconds = standard_daypart.daypart.timespan.end_time
            };
        }
    }
}
