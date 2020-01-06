using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{

    public class DetectionLoadDto
    {
        public List<Quarter> Quarters { get; set; }
        public Quarter CurrentQuarter { get; set; }
        public List<LookupDto> Advertisers { get; set; }
        public List<LookupDto> PostingBooks { get; set; }
        public List<LookupDto> InventorySources { get; set; }
        public List<LookupDto> SchedulePostTypes { get; set; }
        public List<LookupDto> Markets { get; set; }
        public List<LookupDto> Audiences { get; set; }

        public DetectionLoadDto()
        {
            Quarters = new List<Quarter>();
            CurrentQuarter = new Quarter();
            Advertisers = new List<LookupDto>();
            PostingBooks = new List<LookupDto>();
            InventorySources = new List<LookupDto>();
            SchedulePostTypes = new List<LookupDto>();
            Markets = new List<LookupDto>();
            Audiences = new List<LookupDto>(); 
        }
    }

    public class LoadSchedulesDto
    {
        public List<DisplaySchedule> Schedules { get; set; }
        public List<LookupDto> Advertisers { get; set; }
        public List<LookupDto> PostingBooks { get; set; }
        public LoadSchedulesDto()
        {
            Schedules = new List<DisplaySchedule>();
            Advertisers = new List<LookupDto>();
            PostingBooks = new List<LookupDto>();
        }
    }

    
    public class Quarter
    {
        public int Id { get; set; }
        public string Display { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
