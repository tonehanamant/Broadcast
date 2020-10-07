namespace Services.Broadcast.Helpers.Json
{
    /// <summary>
    /// Operations to tell your contract resolver what properties to ignore.
    /// </summary>
    public interface IJsonIgnorignator
    {
        /// <summary>
        /// Setup the fields to ignore.
        /// </summary>
        /// <param name="resolver">A resolver.</param>
        void SetupIgnoreFields(IgnorableSerializerContractResolver resolver);
    }
}