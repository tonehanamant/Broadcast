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
    
    public partial class nti_universes
    {
        public int id { get; set; }
        public int audience_id { get; set; }
        public double universe { get; set; }
        public int nti_universe_header_id { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual nti_universe_headers nti_universe_headers { get; set; }
    }
}
