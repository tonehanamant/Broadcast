using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.DTO
{
    public class ProposalBuySaveRequestDto
    {
        public int EstimateId { get; set; }
        public string FileName { get; set; }
        public string RawData { get; set; }
        public string Username { get; set; }
        public int ProposalVersionDetailId { get; set; }

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
    }
}
