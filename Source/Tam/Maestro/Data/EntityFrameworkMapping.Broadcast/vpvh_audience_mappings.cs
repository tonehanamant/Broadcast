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
    
    public partial class vpvh_audience_mappings
    {
        public int id { get; set; }
        public int audience_id { get; set; }
        public int compose_audience_id { get; set; }
        public int operation { get; set; }
    
        public virtual audience compose_audience { get; set; }
        public virtual audience audience { get; set; }
    }
}