using OfficeOpenXml;

namespace Services.Broadcast.Entities.Inventory
{
    public class InventoryExportGenerationResult
    {
        public int InventoryTabLineCount { get; set; }
        public ExcelPackage ExportExcelPackage { get; set; }
    }
}