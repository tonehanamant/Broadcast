using Services.Broadcast.Entities.Enums.Inventory;

namespace Services.Broadcast.Entities.Inventory
{
	public class InventoryExportRequestDto
	{
		public InventoryExportGenreTypeEnum Genre { get; set; }
		public QuarterDetailDto Quarter { get; set; }
	}
}