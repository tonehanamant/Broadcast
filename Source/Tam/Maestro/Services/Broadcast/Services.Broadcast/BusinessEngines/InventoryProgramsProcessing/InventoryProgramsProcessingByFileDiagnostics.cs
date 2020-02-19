namespace Services.Broadcast.BusinessEngines.InventoryProgramsProcessing
{
    public class InventoryProgramsProcessingJobByFileDiagnostics : InventoryProgramsProcessingJobDiagnostics
    {
        public int FileId { get; set; }

        public InventoryProgramsProcessingJobByFileDiagnostics(OnMessageUpdatedDelegate onMessageUpdated)
            : base(onMessageUpdated)
        {
        }

        public void RecordRequestParameters(int fileId)
        {
            FileId = fileId;

            _ReportToConsoleAndJobNotes($"FileId  : {FileId}.");
        }

        protected override string OnToString()
        {
            return $"FileId : {FileId}";
        }
    }
}