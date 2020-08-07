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

        public List<PlanDaypartVpvhForAudienceDto> VpvhForAudiences { get; set; } = new List<PlanDaypartVpvhForAudienceDto>();

        public RestrictionsDto Restrictions { get; set; } = new RestrictionsDto();

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
            public ShowTypeRestrictionsDto ShowTypeRestrictions { get; set; }

            public GenreRestrictionsDto GenreRestrictions { get; set; }

            public ProgramRestrictionDto ProgramRestrictions { get; set; }

            public AffiliateRestrictionsDto AffiliateRestrictions { get; set; }

            public class ShowTypeRestrictionsDto
            {
                public ContainTypeEnum ContainType { get; set; }
                public List<LookupDto> ShowTypes { get; set; }
            }

            public class GenreRestrictionsDto
            {
                public ContainTypeEnum ContainType { get; set; }
                public List<LookupDto> Genres { get; set; }
            }

            public class ProgramRestrictionDto
            {
                public ContainTypeEnum ContainType { get; set; }
                public List<ProgramDto> Programs { get; set; }
            }

            public class AffiliateRestrictionsDto
            {
                public ContainTypeEnum ContainType { get; set; }
                public List<LookupDto> Affiliates { get; set; }
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