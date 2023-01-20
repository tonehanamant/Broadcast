using Services.Broadcast.Entities.Enums.Inventory;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.Entities.Scx
{
    public class OpenMarketInventoryScxFile
    {
        public MemoryStream ScxStream { get; set; }
        public List<int> DaypartIds { get; set; }
        public InventorySource InventorySource { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? SharedFolderFileId { get; set; }
        public string MarketRank { get; set; }
        public OpenMarketInventoryExportGenreTypeEnum GenreType { get; set; }
        public string Affiliate { get; set; }

        public string FileName
        {
            get
            {
                return string.Concat(string.Join("_",
                    new string[] { InventorySource.Name,
                        MarketRank.ToString(), }), ".scx");
            }
        }
    }
}
