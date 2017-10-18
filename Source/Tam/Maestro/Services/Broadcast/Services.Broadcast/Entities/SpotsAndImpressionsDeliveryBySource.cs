using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class SpotsAndImpressionsDeliveryBySource
    {
        public int Spots { get; set; }
        private Dictionary<int, double> _AudienceImpressions;
        public InventoryFile.InventorySource Source { get; set; }

        public Dictionary<int, double> AudienceImpressions
        {
            get
            {
                if (_AudienceImpressions == null)
                {
                    _AudienceImpressions = new Dictionary<int, double>();
                }
                return _AudienceImpressions;
            }
            set { _AudienceImpressions = value; }
        }
    }
}
