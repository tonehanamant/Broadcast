//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast
{
    using System;
    using System.Collections.Generic;
    
    public partial class stations
    {
        public short media_month_id { get; set; }
        public int distributor_code { get; set; }
        public int market_code { get; set; }
        public int market_of_origin_code { get; set; }
        public string call_letters { get; set; }
        public string legacy_call_letters { get; set; }
        public System.DateTime start_datetime_of_survey { get; set; }
        public System.DateTime end_datetime_of_survey { get; set; }
        public bool parent_plus_indicator { get; set; }
        public string cable_long_name { get; set; }
        public string broadcast_channel_number { get; set; }
        public string distribution_source_type { get; set; }
        public string primary_affiliation { get; set; }
        public string secondary_affiliation { get; set; }
        public string tertiary_affiliation { get; set; }
        public string distributor_group { get; set; }
        public bool parent_indicator { get; set; }
        public bool satellite_indicator { get; set; }
        public string station_type_code { get; set; }
        public string reportability_status { get; set; }
    }
}
