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
    
    public partial class timespan
    {
        public timespan()
        {
            this.dayparts = new HashSet<daypart>();
        }
    
        public int id { get; set; }
        public int start_time { get; set; }
        public int end_time { get; set; }
    
        public virtual ICollection<daypart> dayparts { get; set; }
    }
}
