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
    
    public partial class show_types
    {
        public show_types()
        {
            this.proposal_version_detail_criteria_show_types = new HashSet<proposal_version_detail_criteria_show_types>();
            this.plan_version_daypart_show_type_restrictions = new HashSet<plan_version_daypart_show_type_restrictions>();
            this.program_name_exceptions = new HashSet<program_name_exceptions>();
            this.program_name_mapping_keywords = new HashSet<program_name_mapping_keywords>();
            this.show_type_mappings = new HashSet<show_type_mappings>();
            this.show_type_mappings1 = new HashSet<show_type_mappings>();
            this.inventory_proprietary_daypart_programs = new HashSet<inventory_proprietary_daypart_programs>();
            this.program_name_mappings = new HashSet<program_name_mappings>();
            this.programs = new HashSet<program>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_date { get; set; }
        public int program_source_id { get; set; }
    
        public virtual ICollection<proposal_version_detail_criteria_show_types> proposal_version_detail_criteria_show_types { get; set; }
        public virtual ICollection<plan_version_daypart_show_type_restrictions> plan_version_daypart_show_type_restrictions { get; set; }
        public virtual ICollection<program_name_exceptions> program_name_exceptions { get; set; }
        public virtual ICollection<program_name_mapping_keywords> program_name_mapping_keywords { get; set; }
        public virtual ICollection<show_type_mappings> show_type_mappings { get; set; }
        public virtual ICollection<show_type_mappings> show_type_mappings1 { get; set; }
        public virtual ICollection<inventory_proprietary_daypart_programs> inventory_proprietary_daypart_programs { get; set; }
        public virtual ICollection<program_name_mappings> program_name_mappings { get; set; }
        public virtual ICollection<program> programs { get; set; }
    }
}
