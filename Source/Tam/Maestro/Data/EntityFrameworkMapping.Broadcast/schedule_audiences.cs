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
    
    public partial class schedule_audiences
    {
        public int id { get; set; }
        public int schedule_id { get; set; }
        public int audience_id { get; set; }
        public int population { get; set; }
        public int rank { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual schedule schedule { get; set; }
    }
}
