using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PostFileDetail
    {
        public int Id;
        public int PostFileId;
        public int? Rank;
        public string Market;
        public string Station;
        public string Affiliate;
        public DateTime Weekstart;
        public string Day;
        public DateTime Date;
        public int TimeAired;
        public string ProgramName;
        public int SpotLength;
        public int SpotLengthID;
        public string HouseISCI;
        public string ClientISCI;
        public string Advertiser;
        public string InventorySource;
        public string InventorySourceDaypart;
        public string InventoryOutOfSpecReason;
        public int EstimateID;
        public string DetectedVia;
        public int Spot;
        public List<PostFileDetailImpression> Impressions;

        public PostFileDetail(int xID, int postFileId, int? rank, string market, string station, string affiliate, DateTime weekstart, string day, DateTime date, int timeAired, string programName, int spotLength, int spotLengthID, string houseISCI, string clientISCI, string advertiser, string inventorySource, string inventorySourceDaypart, string inventoryOutOfSpecReason, int estimateID, string detectedVia, int spot)
        {
            Id = xID;
            PostFileId = postFileId;
            Rank = rank;
            Market = market;
            Station = station;
            Affiliate = affiliate;
            Weekstart = weekstart;
            Day = day;
            Date = date;
            TimeAired = timeAired;
            ProgramName = programName;
            SpotLength = spotLength;
            SpotLengthID = spotLengthID;
            HouseISCI = houseISCI;
            ClientISCI = clientISCI;
            Advertiser = advertiser;
            InventorySource = inventorySource;
            InventorySourceDaypart = inventorySourceDaypart;
            InventoryOutOfSpecReason = inventoryOutOfSpecReason;
            EstimateID = estimateID;
            DetectedVia = detectedVia;
            Spot = spot;
        }

        public PostFileDetail(int xID, int postFileId, int? rank, string market, string station, string affiliate, DateTime weekstart, string day, DateTime date, int timeAired, string programName, int spotLength, int spotLengthID, string houseISCI, string clientISCI, string advertiser, string inventorySource, string inventorySourceDaypart, string inventoryOutOfSpecReason, int estimateID, string detectedVia, int spot, List<PostFileDetailImpression> impressions)
        {
            Id = xID;
            PostFileId = postFileId;
            Rank = rank;
            Market = market;
            Station = station;
            Affiliate = affiliate;
            Weekstart = weekstart;
            Day = day;
            Date = date;
            TimeAired = timeAired;
            ProgramName = programName;
            SpotLength = spotLength;
            SpotLengthID = spotLengthID;
            HouseISCI = houseISCI;
            ClientISCI = clientISCI;
            Advertiser = advertiser;
            InventorySource = inventorySource;
            InventorySourceDaypart = inventorySourceDaypart;
            InventoryOutOfSpecReason = inventoryOutOfSpecReason;
            EstimateID = estimateID;
            DetectedVia = detectedVia;
            Spot = spot;
            Impressions = impressions;
        }
    }
}