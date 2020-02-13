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
                //windows with NTFS allows only 255 chars limit on filename, so we're going to truncate the name
                //excel can open a file that has the entire filePath less than 218 chars
                //to have a margin for long path on people computers, we're going to truncate the filename at 64 chars
                FileName = Path.GetFileNameWithoutExtension(value).Substring(0, 64);
                FileExtension = Path.GetExtension(value);
            }
        }
    }
}
