using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    /// <summary></summary>
    public class SpotExceptionsRecommendedPlanDetailsResultDto
    {
        /// <summary>Initializes a new instance of the <see cref="SpotExceptionsRecommendedPlanDetailsResultDto" /> class.</summary>
        public SpotExceptionsRecommendedPlanDetailsResultDto()
        {
            Plans = new List<RecommendedPlanDetailResultDto>();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the estimate identifier.
        /// </summary>
        /// <value>The estimate identifier.</value>
        public int? EstimateId { get; set; }

        /// <summary>
        /// Gets or sets the spot length string.
        /// </summary>
        /// <value>The spot length string.</value>
        public string SpotLengthString { get; set; }

        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        /// <value>The product.</value>
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the flight start date.
        /// </summary>
        /// <value>The flight start date.</value>
        public string FlightStartDate { get; set; }

        /// <summary>
        /// Gets or sets the flight end date.
        /// </summary>
        /// <value>The flight end date.</value>
        public string FlightEndDate { get; set; }

        /// <summary>
        /// Gets or sets the flight date string.
        /// </summary>
        /// <value>The flight date string.</value>
        public string FlightDateString { get; set; }

        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the program air date.
        /// </summary>
        /// <value>The program air date.</value>
        public string ProgramAirDate { get; set; }

        /// <summary>
        /// Gets or sets the program air time.
        /// </summary>
        /// <value>The program air time.</value>
        public string ProgramAirTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the inventory source.
        /// </summary>
        /// <value>The name of the inventory source.</value>
        public string InventorySourceName { get; set; }

        /// <summary>
        /// Gets or sets the plans.
        /// </summary>
        /// <value>The plans.</value>
        public List<RecommendedPlanDetailResultDto> Plans { get; set; }
    }

    /// <summary></summary>
    public class RecommendedPlanDetailResultDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the spot length string.
        /// </summary>
        /// <value>The spot length string.</value>
        public string SpotLengthString { get; set; }

        /// <summary>
        /// Gets or sets the flight start date.
        /// </summary>
        /// <value>The flight start date.</value>
        public string FlightStartDate { get; set; }

        /// <summary>
        /// Gets or sets the flight end date.
        /// </summary>
        /// <value>The flight end date.</value>
        public string FlightEndDate { get; set; }

        /// <summary>
        /// Gets or sets the flight date string.
        /// </summary>
        /// <value>The flight date string.</value>
        public string FlightDateString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is recommended plan.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is recommended plan; otherwise, <c>false</c>.</value>
        public bool IsRecommendedPlan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the pacing.
        /// </summary>
        /// <value>The pacing.</value>
        public string Pacing { get; set; }

        /// <summary>
        /// Gets or sets the recommended plan identifier.
        /// </summary>
        /// <value>The recommended plan identifier.</value>
        public int RecommendedPlanId { get; set; }

        /// <summary>
        /// Gets or sets the daypart code.
        /// </summary>
        /// <value>The daypart code.</value>
        public string DaypartCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the audience.
        /// </summary>
        /// <value>The name of the audience.</value>
        public string AudienceName { get; set; }

        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        /// <value>The product.</value>
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the impressions.
        /// </summary>
        /// <value>The impressions.</value>
        public double? Impressions { get; set; }

        /// <summary>
        /// Gets or sets the total contracted impressions.
        /// </summary>
        /// <value>The total contracted impressions.</value>
        public double? TotalContractedImpressions { get; set; }

        /// <summary>
        /// Gets or sets the total delivered impressions selected.
        /// </summary>
        /// <value>The total delivered impressions selected.</value>
        public double? TotalDeliveredImpressionsSelected { get; set; }

        /// <summary>
        /// Gets or sets the total pacing selected.
        /// </summary>
        /// <value>The total pacing selected.</value>
        public double? TotalPacingSelected { get; set; }

        /// <summary>
        /// Gets or sets the total delivered impressions unselected.
        /// </summary>
        /// <value>The total delivered impressions unselected.</value>
        public double? TotalDeliveredImpressionsUnselected { get; set; }

        /// <summary>
        /// Gets or sets the total pacing unselected.
        /// </summary>
        /// <value>The total pacing unselected.</value>
        public double? TotalPacingUnselected { get; set; }

        /// <summary>
        /// Gets or sets the weekly contracted impressions.
        /// </summary>
        /// <value>The weekly contracted impressions.</value>
        public double? WeeklyContractedImpressions { get; set; }

        /// <summary>
        /// Gets or sets the weekly delivered impressions selected.
        /// </summary>
        /// <value>The weekly delivered impressions selected.</value>
        public double? WeeklyDeliveredImpressionsSelected { get; set; }

        /// <summary>
        /// Gets or sets the weekly pacing selected.
        /// </summary>
        /// <value>The weekly pacing selected.</value>
        public double? WeeklyPacingSelected { get; set; }

        /// <summary>
        /// Gets or sets the weekly delivered impressions unselected.
        /// </summary>
        /// <value>The weekly delivered impressions unselected.</value>
        public double? WeeklyDeliveredImpressionsUnselected { get; set; }

        /// <summary>
        /// Gets or sets the weekly pacing unselected.
        /// </summary>
        /// <value>The weekly pacing unselected.</value>
        public double? WeeklyPacingUnselected { get; set; }
    }
}
