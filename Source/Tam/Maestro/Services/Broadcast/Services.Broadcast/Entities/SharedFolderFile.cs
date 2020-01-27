using Services.Broadcast.Entities.Enums;
using System;
using System.IO;

namespace Services.Broadcast.Entities
{
    public class SharedFolderFile
    {
        public Guid Id { get; set; }

        public string FolderPath { get; set; }

        public string FileName { get; set; }

        public string FileExtension { get; set; }

        public string FileMediaType { get; set; }

        public SharedFolderFileUsage FileUsage { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public Stream FileContent { get; set; }

        public string FileNameWithExtension
        {
            get { return FileName + FileExtension; }
            set
            {
                FileName = Path.GetFileNameWithoutExtension(value);
                FileExtension = Path.GetExtension(value);
            }
        }
    }
}
