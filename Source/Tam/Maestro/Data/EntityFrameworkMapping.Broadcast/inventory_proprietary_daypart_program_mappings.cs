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
    
    public partial class inventory_proprietary_daypart_program_mappings
    {
        public inventory_proprietary_daypart_program_mappings()
        {
            this.inventory_proprietary_summary = new HashSet<inventory_proprietary_summary>();
        }
    
        public int id { get; set; }
        public int inventory_source_id { get; set; }
        public int inventory_proprietary_daypart_programs_id { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_at { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_at { get; set; }
        public int standard_daypart_id { get; set; }
    
        public virtual inventory_sources inventory_sources { get; set; }
        public virtual inventory_proprietary_daypart_programs inventory_proprietary_daypart_programs { get; set; }
        public virtual ICollection<inventory_proprietary_summary> inventory_proprietary_summary { get; set; }
        public virtual standard_dayparts standard_dayparts { get; set; }
    }
}
