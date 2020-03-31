using Newtonsoft.Json;

namespace BroadcastLogging
{
    /// <summary>
    /// Supporting extensions classes.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts the serializable object to Json string.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToJson(this object data)
        {
            try
            {
                return JsonConvert.SerializeObject(data);
            }
            catch
            {
                //Digest exception and return empty string. This may happen when the object is not serializable.
            }
            return string.Empty;
        }
    }
}