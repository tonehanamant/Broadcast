using Services.Broadcast.Extensions;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventoryMarkets
{
    public class InventoryMarketAffiliatesReportData
    {
        private const string FILENAME_FORMAT = "Open_Market_Affiliate_Availability_Per_Market.xlsx";

        /// <summary>
        /// Gets or sets the name of the market affiliates export file.
        /// </summary>
        /// <value>The name of the market affiliates export file.</value>
        public string MarketAffiliatesExportFileName { get; set; }

        /// <summary>
        /// Gets or sets the news market affiliates.
        /// </summary>
        /// <value>The news market affiliates.</value>
        public List<InventoryMarketAffiliatesData> NewsMarketAffiliates { get; set; } = new List<InventoryMarketAffiliatesData>();

        /// <summary>
        /// Gets or sets the non news market affiliates.
        /// </summary>
        /// <value>The non news market affiliates.</value>
        public List<InventoryMarketAffiliatesData> NonNewsMarketAffiliates { get; set; } = new List<InventoryMarketAffiliatesData>();

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="InventoryMarketAffiliatesReportData" /> class.
        /// </summary>
        public InventoryMarketAffiliatesReportData()
        {
            MarketAffiliatesExportFileName = _GetMarketAffiliatesExportFileName();
        }

        /// <summary>
        /// Gets the name of the market affiliates export file.
        /// </summary>
        /// <returns></returns>
        internal string _GetMarketAffiliatesExportFileName()
        {
            var rawFileName = string.Format(FILENAME_FORMAT);
            var fileName = rawFileName.PrepareForUsingInFileName();
            return fileName;
        }
    }

    public class InventoryMarketAffiliatesData
    {
        /// <summary>
        /// Gets or sets the name of the market.
        /// </summary>
        /// <value>The name of the market.</value>
        public string marketName { get; set; }

        /// <summary>
        /// Gets or sets the market rank.
        /// </summary>
        /// <value>The market rank.</value>
        public int marketRank { get; set; }

        /// <summary>
        /// Gets or sets the affiliates.
        /// </summary>
        /// <value>The affiliates.</value>
        public string affiliates { get; set; }

        /// <summary>
        /// Gets or sets the inventory.
        /// </summary>
        /// <value>The inventory.</value>
        public string inventory { get; set; }

        /// <summary>
        /// Gets or sets the aggregate.
        /// </summary>
        /// <value>The aggregate.</value>
        public string aggregate { get; set; }
    }
}
