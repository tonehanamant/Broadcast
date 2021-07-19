using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.ReelRosterIscis
{
    /// <summary>
    /// The entity returned from the api.
    /// </summary>
    public class ReelRosterIsciEntity
    {
        public string ISCI_Name { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public int Length { get; set; }
        public List<string> Advertiser { get; set; }
    }
}