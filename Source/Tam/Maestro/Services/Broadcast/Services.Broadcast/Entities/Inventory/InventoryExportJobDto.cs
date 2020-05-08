using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Enums.Inventory;
using System;

namespace Services.Broadcast.Entities.Inventory
{
    /// <summary>
    /// An Inventory Export job instance.
    /// </summary>
    public class InventoryExportJobDto
    {
        /// <summary>
        /// The identifier.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// The source for the export.
        /// </summary>
        public int InventorySourceId { get; set; }

        /// <summary>
        /// The quarter for the export.
        /// </summary>
        public QuarterDetailDto Quarter { get; set; }

        /// <summary>
        /// The type of genre for the export.
        /// </summary>
        public InventoryExportGenreTypeEnum ExportGenreType { get; set; }

        /// <summary>
        /// The job status.
        /// </summary>
        public BackgroundJobProcessingStatus Status { get; set; } = BackgroundJobProcessingStatus.Queued;

        /// <summary>
        /// The message explaining the status if one is needed.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// The name of the generated export file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The timestamp when the job completed.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// The timestamp when the job was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The username who created the job.
        /// </summary>
        public string CreatedBy { get; set; }
    }
}