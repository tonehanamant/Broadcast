using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.Entities
{
    public enum ScheduleFileType
    {
        Default = 1,
        Scx = 2,
        Csv = 3
    };

    [Serializable]
    public class ScheduleSaveRequest
    {
        public ScheduleDTO Schedule { get; set; }

        public ScheduleSaveRequest()
        {
        }
    }

    [Serializable]
    public class ScheduleDTO
    {
        private Stream _FileStream;
        public Stream FileStream
        {
            get
            {
                if (_FileStream != null)
                {
                    return _FileStream;
                }
                if (RawData == null) return null;
                byte[] rawBytes = Convert.FromBase64String(RawData);
                _FileStream = new MemoryStream(rawBytes, 0, rawBytes.Length);
                return _FileStream;
            }
            set { _FileStream = value; }
        }

        public string ScheduleName { get; set; }
        public string FileName { get; set; }
        public int? EstimateId { get; set; }
        public int PostingBookId { get; set; }
        public int AdvertiserId { get; set; }
        public string UserName { get; set; }
        public string RawData { get; set; }
        public int Id { get; set; }
        public List<DetectionTrackingDetail> BvsTrackingDetails { get; set; }
        public List<int> MarketRestrictions { get; set; }
        public DaypartDto DaypartRestriction { get; set; }
        public PostingTypeEnum PostType { get; set; }
        public InventorySourceEnum? InventorySource { get; set; }
        public bool Equivalized { get; set; }
        public List<DetectionTrackingAudience> Audiences { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsBlank { get; set; }
        public List<IsciDto> ISCIs { get; set; }

        public bool ContainsIsci(string isci)
        {
            return ISCIs != null && ISCIs.Any(i => i.House.ToUpper() == isci.ToUpper());
        }

        private string _GetHouseIsci(string isciString)
        {
            var clientIsciStart = isciString.IndexOf('(');
            if (clientIsciStart < 0)
            {
                return isciString;
            }
            else
            {
                return isciString.Substring(0, clientIsciStart);
            }
        }
    }
}
