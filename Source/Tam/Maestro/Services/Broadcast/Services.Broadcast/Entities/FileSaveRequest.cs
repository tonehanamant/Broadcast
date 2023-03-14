using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.Entities
{
    [Serializable]
    public class FileSaveRequest
    {
        public List<FileRequest> Files { get; set; } = new List<FileRequest>();
    }

    public class FileRequest
    {
        private Stream _StreamData;
        public string FileName { get; set; }
        public string RawData { get; set; }
        public Stream StreamData
        {
            get
            {
                if (_StreamData != null)
                {
                    return _StreamData;
                }
                if (RawData == null) return null;
                byte[] rawBytes = Convert.FromBase64String(RawData);
                MemoryStream ms = new MemoryStream(rawBytes, 0, rawBytes.Length);
                return ms;
            }
            set { _StreamData = value; }
        }
    }
    public class UserInformation
    {
        /// <summary>
        /// The full name of the use who performed this action
        /// </summary>
        public string UserName { get; set; }
    }
}
