using Amazon.Runtime.Internal;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.Plan
{
    public class PlanCustomDaypartDto 
    {      
        public int Id { get; set; }
        public int CustomDaypartOrganizationId { get; set; }
        public string CustomDaypartOrganizationName { get; set; }
        public string CustomDaypartName { get; set; }
        public DaypartTypeEnum DaypartTypeId { get; set; }      
        public int StartTimeSeconds { get; set; }
        public bool IsStartTimeModified { get; set; }
        public int EndTimeSeconds { get; set; }
        public bool IsEndTimeModified { get; set; }
        public double? WeightingGoalPercent { get; set; }     
        public double? WeekdaysWeighting { get; set; }
        public double? WeekendWeighting { get; set; }
        public List<PlanDaypartVpvhForAudienceDto> VpvhForAudiences { get; set; } = new List<PlanDaypartVpvhForAudienceDto>();
        public RestrictionsDto Restrictions { get; set; } = new RestrictionsDto();
        
        public class RestrictionsDto
        {
            public ShowTypeRestrictionsDto ShowTypeRestrictions { get; set; } = new ShowTypeRestrictionsDto();
            public GenreRestrictionsDto GenreRestrictions { get; set; } = new GenreRestrictionsDto();
            public ProgramRestrictionDto ProgramRestrictions { get; set; } = new ProgramRestrictionDto();
            public AffiliateRestrictionsDto AffiliateRestrictions { get; set; } = new AffiliateRestrictionsDto();

            public class ShowTypeRestrictionsDto
            {
                public ContainTypeEnum ContainType { get; set; } = ContainTypeEnum.Exclude;
                public List<LookupDto> ShowTypes { get; set; } = new List<LookupDto>();
            }

            public class GenreRestrictionsDto
            {
                public ContainTypeEnum ContainType { get; set; } = ContainTypeEnum.Exclude;
                public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
            }

            public class ProgramRestrictionDto
            {
                public ContainTypeEnum ContainType { get; set; } = ContainTypeEnum.Exclude;
                public List<ProgramDto> Programs { get; set; } = new AutoConstructedList<ProgramDto>();
            }

            public class AffiliateRestrictionsDto
            {
                public ContainTypeEnum ContainType { get; set; } = ContainTypeEnum.Exclude;
                public List<LookupDto> Affiliates { get; set; } = new List<LookupDto>();
            }
        }
        public class PlanCustomDaypart : PlanCustomDaypartDto
        {         
            public List<int> FlightDays { get; set; }
        }
    }
}
    

