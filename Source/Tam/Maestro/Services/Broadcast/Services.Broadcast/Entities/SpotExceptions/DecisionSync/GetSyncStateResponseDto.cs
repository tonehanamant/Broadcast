using Services.Broadcast.Entities.DTO.SpotExceptionsApi;

namespace Services.Broadcast.Entities.SpotExceptions.DecisionSync
{
    public class GetSyncStateResponseDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ResultsSyncResponse" /> is result.
        /// </summary>
        /// <value>
        ///   <c>true</c> if result; otherwise, <c>false</c>.</value>
        public GetSyncStateResultDto Result { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ResultsSyncResponse" /> is success.
        /// </summary>
        /// <value>
        ///   <c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the severity.
        /// </summary>
        /// <value>The severity.</value>
        public string Severity { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>The transaction identifier.</value>
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [mask payload].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [mask payload]; otherwise, <c>false</c>.</value>
        public bool MaskPayload { get; set; }
    }

    public class GetSyncStateResultDto
    {
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public GetSyncStateResponseStateDto State { get; set; }

        /// <summary>
        /// Gets or sets the job identifier.
        /// </summary>
        /// <value>The job identifier.</value>
        public long JobId { get; set; }

        /// <summary>
        /// Gets or sets the run identifier.
        /// </summary>
        /// <value>The run identifier.</value>
        public long RunId { get; set; }

        /// <summary>
        /// Gets or sets the number in jobs.
        /// </summary>
        /// <value>The number in jobs.</value>
        public int NumberInJobs { get; set; }
    }

    public class GetSyncStateResponseStateDto
    {
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the state message.
        /// </summary>
        /// <value>The state message.</value>
        public string StateMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [user cancelled or timeout].
        /// </summary>
        /// <value><c>true</c> if [user cancelled or timeout]; otherwise, <c>false</c>.</value>
        public bool UserCancelledOrTimeout { get; set; }
    }
}
