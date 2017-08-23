using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class ProposalOpenMarketDisplayFilter
    {
        public ProposalOpenMarketDisplayFilter()
        {
            ProgramNames = new List<string>();
            Genres = new List<LookupDto>();
            Affiliations = new List<string>();
            Markets = new List<LookupDto>();
        }
        public List<string> ProgramNames { get; set; }
        public List<LookupDto> Genres { get; set; }
        public List<string> Affiliations { get; set; }
        public List<LookupDto> Markets { get; set; }
    }
}
