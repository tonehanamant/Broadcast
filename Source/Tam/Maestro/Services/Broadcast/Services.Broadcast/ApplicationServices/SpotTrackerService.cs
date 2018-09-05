using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tam.Maestro.Common;

namespace Services.Broadcast.ApplicationServices
{
    public interface ISpotTrackerService : IApplicationService
    {
        /// <summary>
        /// Saves A to AA columns from a sigma file
        /// </summary>
        /// <param name="fileSaveRequest">FileSaveRequest object</param>
        /// <param name="username">User requesting the file load</param>
        /// <returns>List of duplicated messages</returns>
        List<string> SaveSigmaFile(FileSaveRequest fileSaveRequest, string username);
    }

    public class SpotTrackerService : ISpotTrackerService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly ISigmaConverter _SigmaConverter;

        public SpotTrackerService(IDataRepositoryFactory repositoryFactory, ISigmaConverter sigmaCOnverter)
        {
            _BroadcastDataRepositoryFactory = repositoryFactory;
            _SigmaConverter = sigmaCOnverter;
        }

        /// <summary>
        /// Saves A to AA columns from a sigma file
        /// </summary>
        /// <param name="fileSaveRequest">FileSaveRequest object</param>
        /// <param name="username">User requesting the file load</param>
        /// <returns>List of duplicated messages</returns>
        public List<string> SaveSigmaFile(FileSaveRequest fileSaveRequest, string username)
        {
            List<string> duplicateMessages = new List<string>();
            
            var spotTrackerRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotTrackerRepository>();
            StringBuilder errorMessages = new StringBuilder();
            var hasErrors = false;

            foreach (var requestFile in fileSaveRequest.Files)
            {
                try
                {
                    //compute file hash to check against duplicate files being loaded
                    var hash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(requestFile.StreamData));

                    //check if file has already been loaded
                    if (spotTrackerRepository.GetSigmaFileIdByHash(hash) > 0)
                    {
                        throw new ApplicationException("Unable to load spot tracker file. The selected file has already been loaded.");
                    }

                    //we made it this far, it must be a new file - persist the file
                    TrackerFile<SpotTrackerFileDetail> sigmaFile = new TrackerFile<SpotTrackerFileDetail>();
                    Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int> lineInfo = new Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int>();

                    sigmaFile = _SigmaConverter.ExtractSigmaDataExtended(requestFile.StreamData, hash, username, requestFile.FileName, out lineInfo);


                    string message = string.Empty;
                    if (!string.IsNullOrEmpty(message))
                    {
                        errorMessages.AppendLine(message);
                        hasErrors = true;
                    }

                    var filterResult = spotTrackerRepository.FilterOutExistingDetails(sigmaFile.FileDetails);
                    sigmaFile.FileDetails = filterResult.New;

                    duplicateMessages.AddRange(_CreateDuplicateMessages(lineInfo, filterResult.Ignored, "The following line(s) were previously imported and were ignored"));
                    duplicateMessages.AddRange(_CreateDuplicateMessages(lineInfo, filterResult.Updated, "The following line(s) were previously imported and were updated with new program name"));
                    
                    spotTrackerRepository.SaveSpotTrackerFile(sigmaFile);

                    errorMessages.AppendLine($"File '{requestFile.FileName}' uploaded successfully.<br />");
                }
                catch (Exception e)
                {
                    hasErrors = true;
                    errorMessages.AppendLine($"Error processing file '{requestFile.FileName}'<br />");
                    errorMessages.AppendLine($"Message: {e.Message}");
                }
            }

            if (hasErrors)
                throw new Exception(errorMessages.ToString());

            return duplicateMessages;
        }

        private IEnumerable<string> _CreateDuplicateMessages(Dictionary<TrackerFileDetailKey<SpotTrackerFileDetail>, int> lineInfo, List<SpotTrackerFileDetail> list, string title)
        {
            var duplicateMessages = new List<string>();
            if (list.Any())
            {
                duplicateMessages.Add($"<p>{title}:</p><ul>");
                duplicateMessages.AddRange(list
                    .Select(x => string.Format("<li>Line {0}: Station {1}, Date {2}, Time Aired {3}, ISCI {4}, Spot Length {5}, Campaign {6}, Advertiser {7}</li>",
                        lineInfo[new TrackerFileDetailKey<SpotTrackerFileDetail>(x)], x.Station, x.DateAired, x.TimeAired, x.Isci, x.SpotLength, x.EstimateId, x.Advertiser))
                    .ToList());
                duplicateMessages.Add("</ul>");
            }
            return duplicateMessages;
        }
    }
}
