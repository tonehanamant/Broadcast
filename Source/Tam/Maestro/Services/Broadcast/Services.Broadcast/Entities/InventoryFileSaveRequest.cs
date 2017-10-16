using System;
using System.IO;
using System.Security;

namespace Services.Broadcast.Entities
{
    [Serializable]
    public class InventoryFileSaveRequest
    {
        private Stream _ratesStream;

        public string UserName { get; set; }
        public string FileName { get; set; }
        public string RawData { get; set; }
        public string InventorySource { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DaypartDto Daypart { get; set; }
        public int RatingBook { get; set; }
        public ProposalEnums.ProposalPlaybackType PlaybackType { get; set; }
        public AudiencePricingDto AudiencePricing { get; set; }
        public decimal FixedPrice { get; set; }
        public Stream RatesStream
        {
            get
            {
                if (_ratesStream != null)
                {
                    return _ratesStream;
                }
                
                if (RawData == null) 
                    return null;
                
                var fromBase64String = Convert.FromBase64String(RawData);
                var memoryStream = new MemoryStream(fromBase64String, 0, fromBase64String.Length);
                
                return memoryStream;
            }

            set { _ratesStream = value; }
        }
    }

    public class AudiencePricingDto
    {
        public int AudienceId { get; set; }
        public decimal Price { get; set; }
    }
}
