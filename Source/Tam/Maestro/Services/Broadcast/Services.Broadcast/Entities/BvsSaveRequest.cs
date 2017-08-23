using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.Entities
{
    [Serializable]
    public class BvsSaveRequest
    {
        public string UserName { get; set; }
        public List<BvsFile> BvsFiles { get; set; }

        public BvsSaveRequest()
        {
            BvsFiles = new List<BvsFile>();
        }
    }

    public class BvsFile
    {
        private Stream _BvsStream;
        public string BvsFileName { get; set; }
        public string RawData { get; set; }
        public Stream BvsStream
        {
            get
            {
                if (_BvsStream != null)
                {
                    return _BvsStream;
                }
                if (RawData == null) return null;
                byte[] rawBytes = Convert.FromBase64String(RawData);
                MemoryStream ms = new MemoryStream(rawBytes, 0, rawBytes.Length);
                return ms;
            }
            set { _BvsStream = value; }
        }
    }
}
