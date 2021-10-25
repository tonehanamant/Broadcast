using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Helpers;
using System;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories.Inventory
{
    public interface IInventoryExportJobRepository : IDataRepository
    {
        int CreateJob(InventoryExportJobDto jobDto, string userName);

        void UpdateJob(InventoryExportJobDto jobDto);

        InventoryExportJobDto GetJob(int jobId);
    }

    public class InventoryExportJobRepository : BroadcastRepositoryBase, IInventoryExportJobRepository
    {
        public InventoryExportJobRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        {
        }

        public int CreateJob(InventoryExportJobDto jobDto, string userName)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = new inventory_export_jobs
                {
                    inventory_source_id = jobDto.InventorySourceId,
                    quarter_year = jobDto.Quarter.Year,
                    quarter_number = jobDto.Quarter.Quarter,
                    export_genre_type_id = (int)jobDto.ExportGenreType,
                    status = (int)jobDto.Status,
                    created_at = DateTime.Now,
                    created_by = userName
                };

                context.inventory_export_jobs.Add(entity);
                context.SaveChanges();

                return entity.id;
            });
        }

        public void UpdateJob(InventoryExportJobDto jobDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                if (jobDto.Id.HasValue == false)
                {
                    throw new InvalidOperationException("Id required for operation.");
                }

                var entity = context.inventory_export_jobs
                    .Single(j => j.id.Equals(jobDto.Id.Value), $"Job with id {jobDto.Id.Value} not found");

                entity.status = (int)jobDto.Status;
                entity.status_message = jobDto.StatusMessage;
                entity.file_name = jobDto.FileName;
                entity.completed_at = jobDto.CompletedAt;

                context.SaveChanges();
            });
        }

        public InventoryExportJobDto GetJob(int jobId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var entity = context.inventory_export_jobs
                    .Single(j => j.id.Equals(jobId), $"Job with id {jobId} not found");

                var dto = new InventoryExportJobDto
                {
                    Id = entity.id,
                    InventorySourceId = entity.inventory_source_id,
                    Quarter = new QuarterDetailDto { Year = entity.quarter_year, Quarter = entity.quarter_number },
                    ExportGenreType = EnumHelper.GetEnum<InventoryExportGenreTypeEnum>(entity.export_genre_type_id),
                    Status = EnumHelper.GetEnum<BackgroundJobProcessingStatus>(entity.status),
                    StatusMessage = entity.status_message,
                    FileName = entity.file_name,
                    CompletedAt = entity.completed_at,
                    CreatedAt = entity.created_at,
                    CreatedBy = entity.created_by
                };

                return dto;
            });
        }
    }
}