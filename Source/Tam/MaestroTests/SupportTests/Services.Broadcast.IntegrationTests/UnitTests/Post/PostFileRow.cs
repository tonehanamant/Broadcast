using System.ComponentModel;

namespace Services.Broadcast.IntegrationTests.UnitTests.Post
{
    public class PostFileRow
    {
        public string Rank { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string Affiliate { get; set; }
        public string Weekstart { get; set; }
        public string Day { get; set; }
        public string Date { get; set; }

        [DisplayName("Time Aired")]
        public string TimeAired { get; set; }
        [DisplayName("Program Name")]
        public string ProgramName { get; set; }
        public string Length { get; set; }
        [DisplayName("House ISCI")]
        public string HouseISCI { get; set; }
        [DisplayName("Client ISCI")]
        public string ClientISCI { get; set; }
        public string Advertiser { get; set; }
        [DisplayName("Inventory Source")]
        public string InventorySource { get; set; }
        [DisplayName("Inventory Source Daypart")]
        public string InventorySourceDaypart { get; set; }
        [DisplayName("Inventory Out of Spec Reason")]
        public string InventoryOutofSpecReason { get; set; }
        [DisplayName("Advertiser Out of Spec Reason")]
        public string AdvertiserOutofSpecReason { get; set; }
        public string Estimate { get; set; }
        [DisplayName("Detected Via")]
        public string DetectedVia { get; set; }
        public string Spot { get; set; }

        public PostFileRow(string rank, string market, string station, string affiliate, string weekstart, string day, string date, string timeAired, string programName, string length, string houseISCI, string clientISCI, string advertiser, string inventorySource, string inventorySourceDaypart, string inventoryOutofSpecReason, string advertiserOutofSpecReason, string estimate, string detectedVia, string spot)
        {
            Rank = rank;
            Market = market;
            Station = station;
            Affiliate = affiliate;
            Weekstart = weekstart;
            Day = day;
            Date = date;
            TimeAired = timeAired;
            ProgramName = programName;
            Length = length;
            HouseISCI = houseISCI;
            ClientISCI = clientISCI;
            Advertiser = advertiser;
            InventorySource = inventorySource;
            InventorySourceDaypart = inventorySourceDaypart;
            InventoryOutofSpecReason = inventoryOutofSpecReason;
            AdvertiserOutofSpecReason = advertiserOutofSpecReason;
            Estimate = estimate;
            DetectedVia = detectedVia;
            Spot = spot;
        }
    }
}
