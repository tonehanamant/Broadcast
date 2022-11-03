using Services.Broadcast.Entities.Enums.Inventory;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Scx
{
    /// <summary>
    /// Object used to pass the filters
    /// </summary>
    public class InventoryScxOpenMarketsDownloadRequest
    {
        /// <summary>
        /// Daypart code id used to filter data
        /// </summary>
        /// <value>12344</value>
        public int StandardDaypartId { get; set; }

        /// <summary>
        /// Start Date of the start quarter
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date of the end quarter
        /// </summary>
        public DateTime EndDate { get; set; }

        public int MarketCode { get; set; }
        public OpenMarketInventoryExportGenreTypeEnum GenreType { get; set; }
        public List<string> Affiliates { get; set; }
    }
}
