using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPostLogValidationEngine : IApplicationService
    {
        /// <summary>
        /// Validates a post log record
        /// </summary>
        /// <param name="requestDetail">InboundFileSaveRequestDetail object to be validated</param>
        /// <returns>List of WWTVInboundFileValidationResult objects containing the validation results</returns>
        List<WWTVInboundFileValidationResult> ValidatePostLogRecord(InboundFileSaveRequestDetail requestDetail);
    }

    public class PostLogValidationEngine : IPostLogValidationEngine
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly Dictionary<int, int> _SpotLengthDict;

        public PostLogValidationEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _SpotLengthDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
        }

        /// <summary>
        /// Validates a post log record
        /// </summary>
        /// <param name="requestDetail">InboundFileSaveRequestDetail object to be validated</param>
        /// <returns>List of WWTVInboundFileValidationResult objects containing the validation results</returns>
        public List<WWTVInboundFileValidationResult> ValidatePostLogRecord(InboundFileSaveRequestDetail requestDetail)
        {
            var validationResults = new List<WWTVInboundFileValidationResult>();

            if (requestDetail.AirTime == DateTime.MinValue)
            {
                validationResults.Add(new WWTVInboundFileValidationResult()
                {
                    InvalidField = "AirTime",
                    ErrorMessage = "must be a valid date",
                });
            }

            if (requestDetail.InventorySource == (int)(InventorySourceEnum.Blank))
            {
                validationResults.Add(new WWTVInboundFileValidationResult()
                {
                    InvalidField = "InventorySource",
                    ErrorMessage = "must be valid",
                });
            }

            if (string.IsNullOrWhiteSpace(requestDetail.Station))
            {
                validationResults.Add(new WWTVInboundFileValidationResult()
                {
                    InvalidField = "Station",
                    ErrorMessage = "is required",
                });
            }

            if (!_SpotLengthDict.ContainsKey(requestDetail.SpotLength))
            {
                validationResults.Add(new WWTVInboundFileValidationResult()
                {
                    InvalidField = "SpotLength",
                    ErrorMessage = "must be valid broadcast spot length: 15,30,60,120,180,300",
                });
            }

            if (string.IsNullOrWhiteSpace(requestDetail.Isci))
            {
                validationResults.Add(new WWTVInboundFileValidationResult()
                {
                    InvalidField = "Isci",
                    ErrorMessage = "is required",
                });
            }

            return validationResults;
        }
    }
}