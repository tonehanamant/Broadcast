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
    
    public partial class detection_maps
    {
        public int detection_map_type_id { get; set; }
        public string detection_value { get; set; }
        public string schedule_value { get; set; }
    
        public virtual detection_map_types detection_map_types { get; set; }
    }
}
