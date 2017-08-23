using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroadcastTest.Model
{
    public class StationRateCard
    {

        public StationRateCard()
        {
            Rates = new List<RatesData>();
            Contacts = new List<ContactData>();
        }


        public string Station { get; set; }
        public string Affiliate { get; set; }
        public string Market { get; set; }
        public string RateDataThrough { get; set; }
        public string LastUpdate { get; set; }

        public List<RatesData> Rates { get; set; }
        public List<ContactData> Contacts { get; set; }
    
    
        public void PrintStationData()
        {
            Console.WriteLine("Station: " + this.Station.ToString());
            Console.WriteLine("Affiliate: " + this.Affiliate.ToString());
            Console.WriteLine("Market: " + this.Market.ToString());
            Console.WriteLine("Rate Data Through: " + this.RateDataThrough.ToString());
            Console.WriteLine("LastUpdate: " + this.LastUpdate.ToString());
        }

    }
}
