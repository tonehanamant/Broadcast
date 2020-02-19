using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public class InventoryProgramsProcessingJobBySourceDiagnostics : InventoryProgramsProcessingJobDiagnostics
    {
        public int InventorySourceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<int> MediaWeekIds { get; set; } = new List<int>();

        public InventoryProgramsProcessingJobBySourceDiagnostics(OnMessageUpdatedDelegate onMessageUpdated)
            : base(onMessageUpdated)
        {
        }

        public void RecordRequestParameters(int sourceId, DateTime startDate, DateTime endDate)
        {
            InventorySourceId = sourceId;
            StartDate = startDate;
            EndDate = endDate;

            _ReportToConsoleAndJobNotes($"SourceId : '{InventorySourceId}'");
            _ReportToConsoleAndJobNotes($"StartDate : '{StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}'");
            _ReportToConsoleAndJobNotes($"EndDate : '{EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}'");
        }

        public void RecordMediaWeekIds(List<int> mediaWeekIds)
        {
            MediaWeekIds.AddRange(mediaWeekIds);

            _ReportToConsoleAndJobNotes($"Processing MediaWeekIds : {string.Join(",", mediaWeekIds)}");
        }

        protected override string OnToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"SourceId : {InventorySourceId}");
            sb.AppendLine($"StartDate : {StartDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}");
            sb.AppendLine($"EndDate : {EndDate.ToString(BroadcastConstants.DATE_FORMAT_STANDARD)}");
            sb.AppendLine();
            sb.AppendLine($"Media Week Ids : {string.Join(",", MediaWeekIds)}");
            return sb.ToString();
        }
    }
}