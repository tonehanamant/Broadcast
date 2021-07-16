using System.Collections.Generic;
using Services.Broadcast.Clients;

namespace Services.Broadcast.Entities.ReelRosterIscis
{
    /// <summary>
    /// The response from the ReelRosterApi.
    /// </summary>
    public class ReelRosterIsciResponseEntity
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ReelRosterIsciEntity> Data { get; set; }
    }
}