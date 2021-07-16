using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.ReelRosterIscis
{
    /// <summary>
    /// An Isci Dto ingested from the Reel Roster Ingest.
    /// </summary>
    public class ReelRosterIsciDto
    {
        public string Isci { get; set; }
        public int SpotLengthDuration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> AdvertiserNames { get; set; }
    }
}