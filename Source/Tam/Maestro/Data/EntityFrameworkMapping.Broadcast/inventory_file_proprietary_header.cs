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
    
    public partial class inventory_file_proprietary_header
    {
        public int id { get; set; }
        public int inventory_file_id { get; set; }
        public System.DateTime effective_date { get; set; }
        public System.DateTime end_date { get; set; }
        public Nullable<decimal> cpm { get; set; }
        public Nullable<int> audience_id { get; set; }
        public Nullable<int> contracted_daypart_id { get; set; }
        public Nullable<int> share_projection_book_id { get; set; }
        public Nullable<int> hut_projection_book_id { get; set; }
        public Nullable<int> playback_type { get; set; }
        public Nullable<decimal> nti_to_nsi_increase { get; set; }
        public Nullable<int> standard_daypart_id { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual daypart daypart { get; set; }
        public virtual inventory_files inventory_files { get; set; }
        public virtual media_months media_months { get; set; }
        public virtual media_months media_months1 { get; set; }
        public virtual standard_dayparts standard_dayparts { get; set; }
    }
}
