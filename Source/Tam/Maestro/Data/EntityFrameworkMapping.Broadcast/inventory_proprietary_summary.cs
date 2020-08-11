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
    
    public partial class inventory_proprietary_summary
    {
        public inventory_proprietary_summary()
        {
            this.inventory_proprietary_summary_audiences = new HashSet<inventory_proprietary_summary_audiences>();
            this.inventory_proprietary_summary_markets = new HashSet<inventory_proprietary_summary_markets>();
        }
    
        public int id { get; set; }
        public int inventory_source_id { get; set; }
        public int daypart_default_id { get; set; }
        public int quarter_number { get; set; }
        public int quarter_year { get; set; }
        public int unit { get; set; }
        public Nullable<decimal> cpm { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_at { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_at { get; set; }
    
        public virtual daypart_defaults daypart_defaults { get; set; }
        public virtual ICollection<inventory_proprietary_summary_audiences> inventory_proprietary_summary_audiences { get; set; }
        public virtual inventory_sources inventory_sources { get; set; }
        public virtual ICollection<inventory_proprietary_summary_markets> inventory_proprietary_summary_markets { get; set; }
    }
}
