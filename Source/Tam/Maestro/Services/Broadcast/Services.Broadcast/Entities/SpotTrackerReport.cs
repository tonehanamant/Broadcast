using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class SpotTrackerReport
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Detail> Details { get; set; }

        public class Detail
        {
            public int Id { get; set; }

            public ProposalBuyFile ProposalBuyFile { get; set; }

            public IEnumerable<Week> Weeks { get; set; }

            public class Week
            {
                public int MediaWeekId { get; set; }

                public DateTime StartDate { get; set; }

                public IEnumerable<StationSpotsValue> StationSpotsValues { get; set; }
                
                public class StationSpotsValue
                {
                    public string Station { get; set; }

                    public string Market { get; set; }

                    public string Affiliate { get; set; }

                    public int SpotsOrdered { get; set; }

                    public int SpotsDelivered { get; set; }
                }
            }
        } 
    }
}
