using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class DownloadAndProcessWWTVFilesResponse
    {
        public List<string> FilesFoundToProcess { get; set; } = new List<string>();
        public List<string> FailedDownloads { get; set; } = new List<string>();
        public Dictionary<string, List<WWTVInboundFileValidationResult>> ValidationErrors { get; set; } = new Dictionary<string, List<WWTVInboundFileValidationResult>>();
        public List<WWTVSaveResult> SaveResults { get; set; } = new List<WWTVSaveResult>();
    }
}
