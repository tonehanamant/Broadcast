using System;

namespace Services.Broadcast.Entities
{
    public class RegisterRequestDto
    {
		public string SourceFileName { get; set; }
		
        public string UploadingUserId { get; set; }

        public bool AllowAccessAnyApp { get; set; }

        public string FileMetadata { get; set; }
    }
}
