using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.BusinessEngines
{
    public interface IAffidavitValidationEngine : IApplicationService
    {
        AffidavitValidationResult ValidateAffidavitRecord(AffidavitSaveRequestDetail affidavitDetail);
    }

    public class AffidavitValidationEngine : IAffidavitValidationEngine
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;

        public AffidavitValidationEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
        }

        public AffidavitValidationResult ValidateAffidavitRecord(AffidavitSaveRequestDetail affidavitDetail)
        {
            Dictionary<int, int> spotLengthDict = null;

            var affidavitValidationResult = new AffidavitValidationResult
            {
                IsValid = true
            };

            if (string.IsNullOrWhiteSpace(affidavitDetail.ProgramName))
            {
                affidavitValidationResult.InvalidField = "ProgramName";
                affidavitValidationResult.ErrorMessage = "'ProgramName' is required";
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.Genre))
            {
                affidavitValidationResult.InvalidField = "Genre";
                affidavitValidationResult.ErrorMessage = "'Genre' is required";
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.LeadInProgramName))
            {
                affidavitValidationResult.InvalidField = "LeadInProgramName";
                affidavitValidationResult.ErrorMessage = "'LeadInProgramName' is required";
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.LeadInGenre))
            {
                affidavitValidationResult.InvalidField = "LeadInGenre";
                affidavitValidationResult.ErrorMessage = "'LeadInGenre' is required";
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.LeadOutProgramName))
            {
                affidavitValidationResult.InvalidField = "LeadOutProgramName";
                affidavitValidationResult.ErrorMessage = "'LeadOutProgramName' is required";
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.LeadOutGenre))
            {
                affidavitValidationResult.InvalidField = "LeadOutGenre";
                affidavitValidationResult.ErrorMessage = "'LeadOutGenre' is required";
            }

            if (affidavitDetail.AirTime == DateTime.MinValue)
            {
                affidavitValidationResult.InvalidField = "AirTime";
                affidavitValidationResult.ErrorMessage = "'AirTime' must be a valid date";
            }

            if (affidavitDetail.InventorySource == (int)(InventorySourceEnum.Blank))
            {
                affidavitValidationResult.InvalidField = "InventorySource";
                affidavitValidationResult.ErrorMessage = "'InventorySource' must be valid";
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.Station))
            {
                affidavitValidationResult.InvalidField = "Station";
                affidavitValidationResult.ErrorMessage = "'Station' is required";
            }

            if (!_IsSpotLengthValid(affidavitDetail.SpotLength, ref spotLengthDict))
            {
                affidavitValidationResult.InvalidField = "SpotLength";
                affidavitValidationResult.ErrorMessage = "'SpotLength' must be valid broadcast spot length: 15,30,60,120,180,300";
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.Isci))
            {
                affidavitValidationResult.InvalidField = "Isci";
                affidavitValidationResult.ErrorMessage = "'Isci' is required";
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.Affiliate))
            {
                affidavitValidationResult.InvalidField = "Affiliate";
                affidavitValidationResult.ErrorMessage = "'Affiliate' is required";
            }

            if (affidavitValidationResult.InvalidField != null)
                affidavitValidationResult.IsValid = false;

            return affidavitValidationResult;
        }

        private bool _IsSpotLengthValid(int spotLength, ref Dictionary<int, int> spotLengthDict)
        {
            if (spotLengthDict == null)
                spotLengthDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();

            return spotLengthDict.ContainsKey(spotLength);
        }
    }
}
