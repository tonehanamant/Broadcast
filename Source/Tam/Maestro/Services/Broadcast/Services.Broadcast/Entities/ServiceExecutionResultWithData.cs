namespace Services.Broadcast.Entities
{
    /// <summary>
    /// An execution result that contains data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    public class ServiceExecutionResultWithData<T> : ServiceExecutionResult
    {
        /// <summary>
        /// The execution result data.
        /// </summary>
        public T Data { get; set; }
    }
}