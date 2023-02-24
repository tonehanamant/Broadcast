using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.RecommendedPlans
{
    public class SpotExceptionsRecommendedPlanSpotsToDoDto
    {
        public SpotExceptionsRecommendedPlanSpotsToDoDto()
        {
            SpotLength = new SpotLengthDto();
            SpotExceptionsRecommendedPlanDetailsToDo = new List<SpotExceptionsRecommendedPlanDetailsToDoDto>();
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// The recommended plan type
        /// </summary>
        public string recommendedPlanType = "ToDo";

        /// <summary>
        /// Gets or sets the spot unique hash external.
        /// </summary>
        /// <value>The spot unique hash external.</value>
        public string SpotUniqueHashExternal { get; set; }

        /// <summary>
        /// Gets or sets the ambiguity code.
        /// </summary>
        /// <value>The ambiguity code.</value>
        public int AmbiguityCode { get; set; }

        /// <summary>
        /// Gets or sets the execution identifier external.
        /// </summary>
        /// <value>The execution identifier external.</value>
        public string ExecutionIdExternal { get; set; }

        /// <summary>
        /// Gets or sets the estimate identifier.
        /// </summary>
        /// <value>The estimate identifier.</value>
        public int? EstimateId { get; set; }

        /// <summary>
        /// Gets or sets the inventory source.
        /// </summary>
        /// <value>The inventory source.</value>
        public string InventorySource { get; set; }

        /// <summary>
        /// Gets or sets the house isci.
        /// </summary>
        /// <value>The house isci.</value>
        public string HouseIsci { get; set; }

        /// <summary>
        /// Gets or sets the client isci.
        /// </summary>
        /// <value>The client isci.</value>
        public string ClientIsci { get; set; }

        /// <summary>
        /// Gets or sets the spot length identifier.
        /// </summary>
        /// <value>The spot length identifier.</value>
        public int? SpotLengthId { get; set; }

        /// <summary>
        /// Gets or sets the program air time.
        /// </summary>
        /// <value>The program air time.</value>
        public DateTime ProgramAirTime { get; set; }

        /// <summary>
        /// Gets or sets the station legacy call letters.
        /// </summary>
        /// <value>The station legacy call letters.</value>
        public string StationLegacyCallLetters { get; set; }

        /// <summary>
        /// Gets or sets the affiliate.
        /// </summary>
        /// <value>The affiliate.</value>
        public string Affiliate { get; set; }

        /// <summary>
        /// Gets or sets the market code.
        /// </summary>
        /// <value>The market code.</value>
        public int? MarketCode { get; set; }

        /// <summary>
        /// Gets or sets the market rank.
        /// </summary>
        /// <value>The market rank.</value>
        public int? MarketRank { get; set; }

        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the program genre.
        /// </summary>
        /// <value>The program genre.</value>
        public string ProgramGenre { get; set; }

        /// <summary>
        /// Gets or sets the ingested by.
        /// </summary>
        /// <value>The ingested by.</value>
        public string IngestedBy { get; set; }

        /// <summary>
        /// Gets or sets the ingested at.
        /// </summary>
        /// <value>The ingested at.</value>
        public DateTime IngestedAt { get; set; }

        /// <summary>
        /// Gets or sets the ingested media week identifier.
        /// </summary>
        /// <value>The ingested media week identifier.</value>
        public int IngestedMediaWeekId { get; set; }

        /// <summary>
        /// Gets or sets the length of the spot.
        /// </summary>
        /// <value>The length of the spot.</value>
        public SpotLengthDto SpotLength { get; set; }

        /// <summary>
        /// Gets or sets the spot exceptions recommended plan details.
        /// </summary>
        /// <value>The spot exceptions recommended plan details.</value>
        public List<SpotExceptionsRecommendedPlanDetailsToDoDto> SpotExceptionsRecommendedPlanDetailsToDo { get; set; }
    }
}

