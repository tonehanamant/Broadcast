using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalWeekDto
    {
        public ProposalWeekDto()
        {
            Iscis = new List<ProposalWeekIsciDto>();
        }

        public int? Id { get; set; }
        public int MediaWeekId { get; set; }
        public string Week { get; set; }
        public bool IsHiatus { get; set; }
        public int Units { get; set; }
        public double Impressions { get; set; }
        public decimal Cost { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public List<ProposalWeekIsciDto> Iscis { get; set; }
        public string MyEventsReportName { get; set; }
    }
}