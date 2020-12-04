using Newtonsoft.Json;

namespace Services.Broadcast.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Creates a deep clone of an object using Json serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCloneUsingSerialization<T>(this T obj)
        {
            var serializedObj = JsonConvert.SerializeObject(obj);
            var clonedObject = JsonConvert.DeserializeObject<T>(serializedObj);
            return clonedObject;
        }
    }
}
