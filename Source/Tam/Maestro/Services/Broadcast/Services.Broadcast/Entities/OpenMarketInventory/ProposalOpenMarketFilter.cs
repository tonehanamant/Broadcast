using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class ProposalOpenMarketFilter
    {
        public enum OpenMarketSpotFilter
        {
            AllPrograms = 1,
            ProgramWithSpots = 2,
            ProgramWithoutSpots = 3
        }


        public ProposalOpenMarketFilter()
        {
            InitFilters();
        }

        private void InitFilters()
        {
            ProgramNames = new List<string>();
            Genres = new List<int>();
            DayParts = new List<DaypartDto>();
            Affiliations = new List<string>();
            Markets = new List<int>();
            SpotFilter = OpenMarketSpotFilter.AllPrograms;
        }
        
        public List<string> ProgramNames { get; set; }
        public List<int> Genres { get; set; }
        public List<DaypartDto> DayParts { get; set; }
        public List<string> Affiliations { get; set; }
        public List<int> Markets { get; set; }
        public OpenMarketSpotFilter? SpotFilter { get; set; }

        public void Clear()
        {
            InitFilters();
        }
    }
}
