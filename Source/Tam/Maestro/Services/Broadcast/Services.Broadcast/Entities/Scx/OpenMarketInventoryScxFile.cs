﻿using Services.Broadcast.Entities.Enums.Inventory;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.Entities.Scx
{
    public class OpenMarketInventoryScxFile
    {
        public MemoryStream ScxStream { get; set; }
        public int DaypartCodeId { get; set; }
        public InventorySource InventorySource { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? SharedFolderFileId { get; set; }
        public int MarketCode { get; set; }
        public OpenMarketInventoryExportGenreTypeEnum GenreType { get; set; }
        public string Affiliate { get; set; }

        public string FileName
        {
            get
            {
                return string.Concat(string.Join("_",
                    new string[] { InventorySource.Name,
                        MarketCode.ToString(),
                        GenreType.ToString(),
                        Affiliate,
                        StartDate.ToString("yyyyMMdd"),
                        EndDate.ToString("yyyyMMdd"), }), ".scx");
            }
        }
    }
}
