using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroadcastTest.Model
{
    class BvsScheduleRecord
    {

        public BvsScheduleRecord()
        {
            BvsScheduleRecords = new List<StationRateCard>();
        }

        public String Schedule { get; set; }
        public String Advertiser { get; set; }
        public String Estimate { get; set; }
        public String StartDate { get; set; }
        public String EndDate { get; set; }
        public String SpotsBooked { get; set; }
        public String SpotsDelivered { get; set; }
        public String OutOfSpec { get; set; }
        public String PostingBook { get; set; }
        public String PrimaryDemoBookedImp { get; set; }
        public String PrimaryDemoDeliveredImp { get; set; }

        public List<StationRateCard> BvsScheduleRecords { get; set; }
    }
}
