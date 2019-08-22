using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IDaypartCodeRepository : IDataRepository
    {
        bool ActiveDaypartCodeExists(string daypartCode);
        DaypartCodeDto GetDaypartCodeByCode(string daypartCode);
        List<DaypartCodeDto> GetDaypartCodesByInventorySource(int inventorySourceId);
        List<DaypartCodeDto> GetAllActiveDaypartCodes();
        DaypartCodeDto GetDaypartCodeById(int daypartCodeId);

        /// <summary>
        /// Gets the daypart code defaults.
        /// </summary>
        /// <returns>List of <see cref="DaypartCodeDefaultDto"/></returns>
        List<DaypartCodeDefaultDto> GetDaypartCodeDefaults();
    }

    public class DaypartCodeRepository : BroadcastRepositoryBase, IDaypartCodeRepository
    {
        private const string DaypartCodeNotFoundMessage = "Unable to find daypart code";

        public DaypartCodeRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public bool ActiveDaypartCodeExists(string daypartCode)
        {
            return _InReadUncommitedTransaction(context => context.daypart_codes.Any(x => x.is_active && x.code == daypartCode));
        }

        public List<DaypartCodeDto> GetDaypartCodesByInventorySource(int inventorySourceId)
        {
            return _InReadUncommitedTransaction(context => 
            {
                var query = (from week in context.station_inventory_manifest_weeks
                             join manifest in context.station_inventory_manifest on week.station_inventory_manifest_id equals manifest.id
                             join inventoryFile in context.inventory_files on manifest.file_id equals inventoryFile.id
                             join inventoryFileHeader in context.inventory_file_proprietary_header on inventoryFile.id equals inventoryFileHeader.inventory_file_id
                             join daypartCode in context.daypart_codes on inventoryFileHeader.daypart_code_id equals daypartCode.id
                             join ratingProcessingJob in context.inventory_file_ratings_jobs on inventoryFile.id equals ratingProcessingJob.inventory_file_id
                             where manifest.inventory_source_id == inventorySourceId &&
                                   week.spots > 0 &&
                                   ratingProcessingJob.status == (int)BackgroundJobProcessingStatus.Succeeded
                             group daypartCode by daypartCode.id into daypartCodeGroup
                             select daypartCodeGroup.FirstOrDefault());
                
                return query.Select(_MapToDaypartCode).ToList();
            });
        }

        public DaypartCodeDto GetDaypartCodeByCode(string daypartCode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return _MapToDaypartCode(context.daypart_codes.Single(x => x.is_active && x.code == daypartCode, DaypartCodeNotFoundMessage));
            });
        }

        public DaypartCodeDto GetDaypartCodeById(int daypartCodeId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return _MapToDaypartCode(context.daypart_codes.Single(x => x.is_active && x.id == daypartCodeId, DaypartCodeNotFoundMessage));
            });
        }

        public List<DaypartCodeDto> GetAllActiveDaypartCodes()
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.daypart_codes.Where(d => d.is_active).Select(_MapToDaypartCode).OrderBy(x=>x.Code).ToList();
            });
        }

        private DaypartCodeDto _MapToDaypartCode(daypart_codes daypartCode)
        {
            if (daypartCode == null)
                return null;

            return new DaypartCodeDto
            {
                Id = daypartCode.id,
                Code = daypartCode.code,
                FullName = daypartCode.full_name
            };
        }

        ///<inheritdoc/>
        public List<DaypartCodeDefaultDto> GetDaypartCodeDefaults()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var defaultDaypartCodes = context.daypart_codes
                    .Where(c => c.is_active)
                    .Select(c => new DaypartCodeDefaultDto
                    {
                        Id = c.id,
                        Code = c.code,
                        FullName = c.full_name,
                        DaypartType = (DaypartTypeEnum)c.daypart_type,
                        DefaultStartTimeSeconds = c.default_start_time_seconds,
                        DefaultEndTimeSeconds = c.default_end_time_seconds
                    })
                    .ToList();
                return defaultDaypartCodes;
            });
        }
    }
}
