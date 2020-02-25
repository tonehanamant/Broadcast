using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class StationMappingsDto
    {
        /// <summary>
        /// Gets or sets the station identifier.
        /// </summary>
        /// <value>
        /// The station identifier.
        /// </value>
        public int StationId { get; set; }

        /// <summary>
        /// Gets or sets the map set.
        /// </summary>
        /// <value>
        /// The map set.
        /// </value>
        public StationMapSetNamesEnum MapSet { get; set; }

        /// <summary>
        /// Gets or sets the map value.
        /// </summary>
        /// <value>
        /// The map value.
        /// </value>
        public string MapValue { get; set; }
    }
}
