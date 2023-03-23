using Amazon.Runtime.Internal;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// The user defined daypart for the plan.
    /// </summary>
    public class PlanDaypartDto : IEquatable<PlanDaypartDto>
    {
        /// <summary>
        /// Gets or sets the daypart code identifier.
        /// </summary>
        /// <value>
        /// The daypart code identifier.
        /// </value>
        public int DaypartCodeId { get; set; }

        /// <summary>
        /// Gets or sets the daypart type identifier.
        /// </summary>
        /// <value>
        /// The daypart type identifier.
        /// </value>
        public DaypartTypeEnum DaypartTypeId { get; set; }

        /// <summary>
        /// Gets or sets the daypart organization identifier.
        /// </summary>
        /// <value>
        /// The daypart organization identifier.
        /// </value>
        public int? DaypartOrganizationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the daypart organization.
        /// </summary>
        /// <value>
        /// The name of the daypart organization.
        /// </value>
        public string DaypartOrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the name of the custom.
        /// </summary>
        /// <value>
        /// The name of the custom.
        /// </value>
        public string CustomName { get; set; }

        /// <summary>
        /// Gets or sets the start time seconds from midnight ET.
        /// </summary>
        /// <value>
        /// The start time seconds from midnight ET.
        /// </value>
        public int StartTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the default start time value was overridden by the user
        /// </summary>
        /// <value>
        /// <c>true</c> if the start time modified by the user; otherwise, <c>false</c>.
        /// </value>
        public bool IsStartTimeModified { get; set; }

        /// <summary>
        /// Gets or sets the end time seconds from midnight ET.
        /// </summary>
        /// <value>
        /// The end time seconds from midnight ET.
        /// </value>
        public int EndTimeSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the default end time value was overridden by the user
        /// </summary>
        /// <value>
        /// <c>true</c> if the end time modified by the user; otherwise, <c>false</c>.
        /// </value>
        public bool IsEndTimeModified { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Monday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Tuesday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Wednesday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Thursday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Friday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Saturday { get; set; }

        /// <summary>
        /// True when the daypart covers this day of the week.
        /// </summary>
        public bool Sunday { get; set; }

        /// <summary>
        /// Gets or sets the weighting goal percent.
        /// </summary>
        /// <value>
        /// The weighting goal percent.
        /// </value>
        public double? WeightingGoalPercent { get; set; }

        /// <summary>
        /// Gets or sets the weighting goal percent for weekdays. WeekdaysWeighting and WeekendWeighting should sum up to 100
        /// </summary>
        public double? WeekdaysWeighting { get; set; }

        /// <summary>
        /// Gets or sets the weighting goal percent for weekend. WeekdaysWeighting and WeekendWeighting should sum up to 100
        /// </summary>
        public double? WeekendWeighting { get; set; }

        /// <summary>
        /// Gets and set the PlanDaypartId.
        /// </summary>
        /// <value>
        /// The PlanDaypartId.
        /// </value>
        public int PlanDaypartId { get; set; }       

        public List<PlanDaypartVpvhForAudienceDto> VpvhForAudiences { get; set; } = new List<PlanDaypartVpvhForAudienceDto>();

        public RestrictionsDto Restrictions { get; set; } = new RestrictionsDto();
        public string DaypartUniquekey { get { return $"{DaypartCodeId}|{DaypartOrganizationId}|{CustomName?.ToLower()}"; } }

        public bool Equals(PlanDaypartDto other)
        {
            return DaypartCodeId == other.DaypartCodeId
                && DaypartTypeId == other.DaypartTypeId
                && IsStartTimeModified == other.IsStartTimeModified
                && IsEndTimeModified == other.IsEndTimeModified
                && WeightingGoalPercent == other.WeightingGoalPercent;
        }

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
    }

    public class PlanDaypart : PlanDaypartDto
    {
        /// <summary>
        /// The same days as on a plan at the moment
        /// There are some plans to extend dayparts functionality
        /// This property is needed to be able to implement sorting by days
        /// </summary>
        public List<int> FlightDays { get; set; }
    }
}