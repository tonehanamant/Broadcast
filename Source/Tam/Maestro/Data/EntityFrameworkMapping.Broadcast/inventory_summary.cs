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
    
    public partial class inventory_summary
    {
        public inventory_summary()
        {
            this.inventory_summary_gaps = new HashSet<inventory_summary_gaps>();
        }
    
        public int id { get; set; }
        public int inventory_source_id { get; set; }
        public int first_quarter_number { get; set; }
        public int first_quarter_year { get; set; }
        public int last_quarter_number { get; set; }
        public int last_quarter_year { get; set; }
        public Nullable<System.DateTime> last_update_date { get; set; }
    
        public virtual inventory_sources inventory_sources { get; set; }
        public virtual ICollection<inventory_summary_gaps> inventory_summary_gaps { get; set; }
    }
}
