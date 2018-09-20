//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntityFrameworkMapping.Broadcast
{
    using System;
    using System.Collections.Generic;
    
    public partial class station
    {
        public station()
        {
            this.station_contacts = new HashSet<station_contacts>();
            this.station_inventory_manifest = new HashSet<station_inventory_manifest>();
            this.proposal_buy_file_details = new HashSet<proposal_buy_file_details>();
            this.open_market_pricing_guide = new HashSet<open_market_pricing_guide>();
        }
    
        public short station_code { get; set; }
        public string station_call_letters { get; set; }
        public string affiliation { get; set; }
        public short market_code { get; set; }
        public string legacy_call_letters { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_date { get; set; }
    
        public virtual market market { get; set; }
        public virtual ICollection<station_contacts> station_contacts { get; set; }
        public virtual ICollection<station_inventory_manifest> station_inventory_manifest { get; set; }
        public virtual ICollection<proposal_buy_file_details> proposal_buy_file_details { get; set; }
        public virtual ICollection<open_market_pricing_guide> open_market_pricing_guide { get; set; }
    }
}
