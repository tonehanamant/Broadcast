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
    
    public partial class market
    {
        public market()
        {
            this.stations = new HashSet<station>();
            this.schedules = new HashSet<schedule>();
            this.proposal_version_markets = new HashSet<proposal_version_markets>();
        }
    
        public short market_code { get; set; }
        public string geography_name { get; set; }
    
        public virtual ICollection<station> stations { get; set; }
        public virtual ICollection<schedule> schedules { get; set; }
        public virtual ICollection<proposal_version_markets> proposal_version_markets { get; set; }
        public virtual market_dma_map market_dma_map { get; set; }
    }
}
