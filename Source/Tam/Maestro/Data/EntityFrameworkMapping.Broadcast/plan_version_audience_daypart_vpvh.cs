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
    
    public partial class plan_version_audience_daypart_vpvh
    {
        public int id { get; set; }
        public int plan_version_id { get; set; }
        public int audience_id { get; set; }
        public int daypart_default_id { get; set; }
        public int vpvh_type { get; set; }
        public double vpvh_value { get; set; }
        public System.DateTime starting_point { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual daypart_defaults daypart_defaults { get; set; }
        public virtual plan_versions plan_versions { get; set; }
    }
}