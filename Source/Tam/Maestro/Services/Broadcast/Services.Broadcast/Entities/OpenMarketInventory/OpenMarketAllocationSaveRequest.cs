using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class OpenMarketAllocationSaveRequest
    {
        public OpenMarketAllocationSaveRequest()
        {
            Filter = new ProposalOpenMarketFilter();
        }
        public string Username { get; set; }
        public int ProposalVersionDetailId { get; set; }
        public List<OpenMarketAllocationWeek> Weeks { get; set; }
        public ProposalOpenMarketFilter Filter { get; set; }

        public class OpenMarketAllocationWeek
        {
            public int MediaWeekId { get; set; }
            public List<OpenMarketAllocationWeekProgram> Programs { get; set; } 
        }

        public class OpenMarketAllocationWeekProgram
        {
            public int ProgramId { get; set; }
            public int Spots { get; set; }
            public double Impressions { get; set; }
        }
    }
}
