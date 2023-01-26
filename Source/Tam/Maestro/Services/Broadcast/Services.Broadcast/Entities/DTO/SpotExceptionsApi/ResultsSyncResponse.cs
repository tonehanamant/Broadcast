namespace Services.Broadcast.Entities.DTO.SpotExceptionsApi
{
    public class ResultsSyncResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ResultsSyncResponse" /> is result.
        /// </summary>
        /// <value>
        ///   <c>true</c> if result; otherwise, <c>false</c>.</value>
        public bool Result { get; set; }

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
}

