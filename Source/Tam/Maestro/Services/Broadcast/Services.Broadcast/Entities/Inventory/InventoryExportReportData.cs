namespace Services.Broadcast.Entities.Inventory
{
    public class InventoryExportReportData
    {
        public string ExportFileName { get; set; }

        public string GeneratedTimestampValue { get; set; }

        public string ShareBookValue { get; set; }

        public object[][] ProvidedAudienceHeaders { get; set; }

        public object[][] WeeklyColumnHeaders { get; set; }

        public object[][] InventoryTableData { get; set; }
    }
}