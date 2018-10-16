using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.Entities
{
    [Serializable]
    public class PostRequest
    {
        public int? FileId { get; set; }
        public string UserName { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool Equivalized { get; set; }
        public int PostingBookId { get; set; }
        public ProposalEnums.ProposalPlaybackType PlaybackType { get; set; }
        public List<int> Audiences { get; set; }
        
        public string RawData { get; set; }
        private Stream _PostStream;
        public Stream PostStream
        {
            get
            {
                if (_PostStream != null)
                    return _PostStream;

                if (RawData == null)
                    return null;

                var rawBytes = Convert.FromBase64String(RawData);
                return new MemoryStream(rawBytes, 0, rawBytes.Length);
            }
            set { _PostStream = value; }
        }
    }
}
