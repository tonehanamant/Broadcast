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
    
    public partial class schedule_detail_audiences
    {
        public int id { get; set; }
        public int schedule_detail_id { get; set; }
        public int audience_id { get; set; }
        public int demo_rank { get; set; }
        public int demo_population { get; set; }
        public double impressions { get; set; }
    
        public virtual schedule_details schedule_details { get; set; }
    }
}
