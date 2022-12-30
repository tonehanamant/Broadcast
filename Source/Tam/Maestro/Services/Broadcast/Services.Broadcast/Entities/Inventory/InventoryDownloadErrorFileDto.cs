using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.Entities.Inventory
{
    public class InventoryDownloadErrorFileDto
    {
        /// <summary>
        /// version of the inventory record.
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// content of the inventory record.
        /// </summary>
        public Content content { get; set; }
        /// <summary>
        /// statusCode of the inventory record.
        /// </summary>
        public string statusCode { get; set; }
        /// <summary>
        /// reasonPhrase of the inventory record.
        /// </summary>
        public string reasonPhrase { get; set; }
        /// <summary>
        /// headers of the inventory record.
        /// </summary>
        public List<object> headers { get; set; }
        /// <summary>
        /// trailingHeaders of the inventory record.
        /// </summary>
        public List<object> trailingHeaders { get; set; }
        /// <summary>
        /// requestMessage of the inventory record.
        /// </summary>
        public object requestMessage { get; set; }
        /// <summary>
        /// content of the inventory record.
        /// </summary>
        public bool isSuccessStatusCode { get; set; }

    }
    public class Content
    {
        /// <summary>
        /// headers of the inventory record.
        /// </summary>
        public List<Header> headers { get; set; }
    }

    public class Header
    {
        /// <summary>
        /// headers key.
        /// </summary>
        public string key { get; set; }
        /// <summary>
        /// headers value.
        /// </summary>
        public List<string> value { get; set; }
    }

   
     
    
}
