using Newtonsoft.Json;
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

        // TODO: Plug this in when David fixes it.
        [JsonIgnore]
        public int Length { get; set; }

        // TODO: Plug this in when David fixes it.
        [JsonIgnore]
        public List<string> Advertiser { get; set; }
    }
}