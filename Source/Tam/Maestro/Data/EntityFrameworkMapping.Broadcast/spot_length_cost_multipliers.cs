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
    
    public partial class spot_length_cost_multipliers
    {
        public int id { get; set; }
        public int spot_length_id { get; set; }
        public double cost_multiplier { get; set; }
        public decimal inventory_cost_premium { get; set; }
    
        public virtual spot_lengths spot_lengths { get; set; }
    }
}
