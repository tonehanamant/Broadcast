using System;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary>The WeekStartDate and the WeekEndDate</summary>
    public class OutOfSpecPlanAdvertisersRequestDto
    {
        /// <summary>Gets or sets the week start date.</summary>
        /// <value>The week start date.</value>
        public DateTime WeekStartDate { get; set; }

        /// <summary>Gets or sets the week end date.</summary>
        /// <value>The week end date.</value>
        public DateTime WeekEndDate { get; set; }
    }

    /// <summary>The MasterId and the Name of the advertiser</summary>
    public class MasterIdName
    {
        /// <summary>Gets the name of the advertiser.</summary>
        /// <value>The name of the advertiser</value>
        public string Name { get; set; }

        /// <summary>Gets the master id.</summary>
        /// <value>The master id</value>
        public Guid? MasterId { get; set; }
    }
}

