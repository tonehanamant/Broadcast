using System.ComponentModel;

namespace Services.Broadcast.IntegrationTests.UnitTests.Post
{
    public class BvsPostFileRow
    {
        public string Rank { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string Affiliate { get; set; }
        public string Date { get; set; }

        [DisplayName("Time Aired")]
        public string TimeAired { get; set; }
        [DisplayName("Program Name")]
        public string ProgramName { get; set; }
        public string Length { get; set; }
        [DisplayName("House ISCI")]
        public string HouseIsci { get; set; }
        [DisplayName("Client ISCI")]
        public string ClientIsci { get; set; }
        [DisplayName("Advertiser/Daypart")]
        public string AdvertiserAndDaypart { get; set; }
        [DisplayName("Estimate/Inventory Source")]
        public string EstimateAndInventorySource { get; set; }
        [DisplayName("Advertiser Out of Spec")]
        public string AdvertiserOutOfSpec { get; set; }
        [DisplayName("Inventory Out of Spec")]
        public string InventoryOutofSpecReason { get; set; }

        public BvsPostFileRow(string rank, string market, string station, string affiliate, string date,
            string timeAired, string programName, string length, string houseIsci, string clientIsci,
            string advertiserAndDaypart, string estimateAndInventorySource, string advertiserOutOfSpec,
            string inventoryOutofSpecReason)
        {
            Rank = rank;
            Market = market;
            Station = station;
            Affiliate = affiliate;
            Date = date;
            TimeAired = timeAired;
            ProgramName = programName;
            Length = length;
            HouseIsci = houseIsci;
            ClientIsci = clientIsci;
            AdvertiserAndDaypart = advertiserAndDaypart;
            EstimateAndInventorySource = estimateAndInventorySource;
            AdvertiserOutOfSpec = advertiserOutOfSpec;
            InventoryOutofSpecReason = inventoryOutofSpecReason;
        }
    }
}
