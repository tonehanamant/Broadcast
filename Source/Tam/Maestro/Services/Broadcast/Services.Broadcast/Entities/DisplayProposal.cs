using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class DisplayProposal
    {
        public int Id { get; set; }
        public string ProposalName { get; set; }
        public LookupDto Advertiser { get; set; }
        public ProposalEnums.ProposalStatusType Status {get; set; }
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public List<FlightWeekDto> Flights { get; set; }
        public string Owner {get; set; }
        public DateTime LastModified {get; set;}        
    }
}
