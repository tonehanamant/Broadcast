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
    
    public partial class inventory_summary_quarters
    {
        public inventory_summary_quarters()
        {
            this.inventory_summary_gaps = new HashSet<inventory_summary_gaps>();
            this.inventory_summary_quarter_details = new HashSet<inventory_summary_quarter_details>();
        }
    
        public int id { get; set; }
        public int inventory_source_id { get; set; }
        public int quarter_number { get; set; }
        public int quarter_year { get; set; }
        public Nullable<int> share_book_id { get; set; }
        public Nullable<int> hut_book_id { get; set; }
        public int total_markets { get; set; }
        public int total_stations { get; set; }
        public Nullable<int> total_programs { get; set; }
        public Nullable<int> total_daypart_codes { get; set; }
        public Nullable<int> total_units { get; set; }
        public Nullable<double> total_projected_impressions { get; set; }
        public Nullable<decimal> cpm { get; set; }
    
        public virtual inventory_sources inventory_sources { get; set; }
        public virtual ICollection<inventory_summary_gaps> inventory_summary_gaps { get; set; }
        public virtual ICollection<inventory_summary_quarter_details> inventory_summary_quarter_details { get; set; }
    }
}