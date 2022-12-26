using Newtonsoft.Json;

namespace Services.Broadcast.Entities.Inventory
{
    /// <summary>
    /// Represents the Inventory File api response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InventoryFileApIItemResponseTyped<T>
    { /// <summary>
      /// Defines the api result
      /// </summary>
        [JsonProperty("result")]
        public T Result { get; set; }
    }
}
