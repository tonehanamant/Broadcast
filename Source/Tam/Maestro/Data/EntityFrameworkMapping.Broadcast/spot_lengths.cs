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
    
    public partial class spot_lengths
    {
        public spot_lengths()
        {
            this.bvs_file_details = new HashSet<bvs_file_details>();
            this.post_file_details = new HashSet<post_file_details>();
            this.proposal_version_spot_length = new HashSet<proposal_version_spot_length>();
            this.schedule_details = new HashSet<schedule_details>();
            this.spot_length_cost_multipliers = new HashSet<spot_length_cost_multipliers>();
            this.station_inventory_manifest_rates = new HashSet<station_inventory_manifest_rates>();
            this.station_inventory_manifest = new HashSet<station_inventory_manifest>();
            this.proposal_version_details = new HashSet<proposal_version_details>();
        }
    
        public int id { get; set; }
        public int length { get; set; }
        public double delivery_multiplier { get; set; }
        public int order_by { get; set; }
        public bool is_default { get; set; }
    
        public virtual ICollection<bvs_file_details> bvs_file_details { get; set; }
        public virtual ICollection<post_file_details> post_file_details { get; set; }
        public virtual ICollection<proposal_version_spot_length> proposal_version_spot_length { get; set; }
        public virtual ICollection<schedule_details> schedule_details { get; set; }
        public virtual ICollection<spot_length_cost_multipliers> spot_length_cost_multipliers { get; set; }
        public virtual ICollection<station_inventory_manifest_rates> station_inventory_manifest_rates { get; set; }
        public virtual ICollection<station_inventory_manifest> station_inventory_manifest { get; set; }
        public virtual ICollection<proposal_version_details> proposal_version_details { get; set; }
    }
}
