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
    
    public partial class genre
    {
        public genre()
        {
            this.genre_mappings = new HashSet<genre_mappings>();
            this.genre_mappings1 = new HashSet<genre_mappings>();
            this.plan_version_daypart_program_restrictions = new HashSet<plan_version_daypart_program_restrictions>();
            this.plan_version_daypart_genre_restrictions = new HashSet<plan_version_daypart_genre_restrictions>();
            this.proposal_version_detail_criteria_genres = new HashSet<proposal_version_detail_criteria_genres>();
            this.station_inventory_manifest_daypart_genres = new HashSet<station_inventory_manifest_daypart_genres>();
            this.station_inventory_manifest_daypart_programs = new HashSet<station_inventory_manifest_daypart_programs>();
            this.station_inventory_manifest_daypart_programs1 = new HashSet<station_inventory_manifest_daypart_programs>();
            this.station_inventory_manifest_genres = new HashSet<station_inventory_manifest_genres>();
            this.program_name_exceptions = new HashSet<program_name_exceptions>();
            this.program_name_mapping_keywords = new HashSet<program_name_mapping_keywords>();
            this.inventory_proprietary_daypart_programs = new HashSet<inventory_proprietary_daypart_programs>();
            this.programs = new HashSet<program>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_date { get; set; }
        public int program_source_id { get; set; }
    
        public virtual ICollection<genre_mappings> genre_mappings { get; set; }
        public virtual ICollection<genre_mappings> genre_mappings1 { get; set; }
        public virtual ICollection<plan_version_daypart_program_restrictions> plan_version_daypart_program_restrictions { get; set; }
        public virtual program_sources program_sources { get; set; }
        public virtual ICollection<plan_version_daypart_genre_restrictions> plan_version_daypart_genre_restrictions { get; set; }
        public virtual ICollection<proposal_version_detail_criteria_genres> proposal_version_detail_criteria_genres { get; set; }
        public virtual ICollection<station_inventory_manifest_daypart_genres> station_inventory_manifest_daypart_genres { get; set; }
        public virtual ICollection<station_inventory_manifest_daypart_programs> station_inventory_manifest_daypart_programs { get; set; }
        public virtual ICollection<station_inventory_manifest_daypart_programs> station_inventory_manifest_daypart_programs1 { get; set; }
        public virtual ICollection<station_inventory_manifest_genres> station_inventory_manifest_genres { get; set; }
        public virtual ICollection<program_name_exceptions> program_name_exceptions { get; set; }
        public virtual ICollection<program_name_mapping_keywords> program_name_mapping_keywords { get; set; }
        public virtual ICollection<inventory_proprietary_daypart_programs> inventory_proprietary_daypart_programs { get; set; }
        public virtual ICollection<program> programs { get; set; }
    }
}
