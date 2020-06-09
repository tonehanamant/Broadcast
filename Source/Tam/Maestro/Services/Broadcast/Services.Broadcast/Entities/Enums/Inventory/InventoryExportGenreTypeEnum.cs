using System.ComponentModel;

namespace Services.Broadcast.Entities.Enums.Inventory
{
	public enum InventoryExportGenreTypeEnum
	{
		[Description("News")]
		News = 1,

		[Description("Non-News")]
		NonNews = 2,

        [Description("Not Enriched")]
		NotEnriched = 3
	}
}