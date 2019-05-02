using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class PostPrePostingFileSettings
    {
        public int Id;
        public string FileName;
        public bool Equivalized;
        public int PostingBookId;
        public ProposalEnums.ProposalPlaybackType PlaybackType;
        public List<LookupDto> DemoLookups;
        public List<int> Demos;
        public DateTime UploadDate;
        public DateTime ModifiedDate;

        public PostPrePostingFileSettings(int id, bool equivalized, int postingBookId, ProposalEnums.ProposalPlaybackType playbackType, ICollection<post_file_demos> demos, string fileName, DateTime uploadDate, DateTime modifiedDate)
        {
            Id = id;
            Equivalized = equivalized;
            PostingBookId = postingBookId;
            PlaybackType = playbackType;
            Demos = demos.Select(d => d.demo).ToList();
            FileName = fileName;
            UploadDate = uploadDate;
            ModifiedDate = modifiedDate;
        }
    }
}