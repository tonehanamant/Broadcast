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
    
    public partial class vpvh_quarters
    {
        public int id { get; set; }
        public int audience_id { get; set; }
        public int year { get; set; }
        public int quarter { get; set; }
        public double pm_news { get; set; }
        public double am_news { get; set; }
        public double syn_all { get; set; }
        public double tdn { get; set; }
        public double tdns { get; set; }
    
        public virtual audience audience { get; set; }
    }
}
