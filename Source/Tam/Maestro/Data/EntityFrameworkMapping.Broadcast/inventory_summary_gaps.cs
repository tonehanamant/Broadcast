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
    
    public partial class inventory_summary_gaps
    {
        public inventory_summary_gaps()
        {
            this.inventory_summary_gap_ranges = new HashSet<inventory_summary_gap_ranges>();
        }
    
        public int id { get; set; }
        public int inventory_summary_id { get; set; }
        public int quarter_number { get; set; }
        public int quarter_year { get; set; }
        public bool all_quarter_missing { get; set; }
    
        public virtual ICollection<inventory_summary_gap_ranges> inventory_summary_gap_ranges { get; set; }
        public virtual inventory_summary_quarters inventory_summary_quarters { get; set; }
    }
}
