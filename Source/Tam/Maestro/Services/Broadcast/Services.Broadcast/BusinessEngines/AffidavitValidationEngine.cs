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
        List<AffidavitValidationResult> ValidateAffidavitRecord(AffidavitSaveRequestDetail affidavitDetail);
    }

    public class AffidavitValidationEngine : IAffidavitValidationEngine
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;

        public AffidavitValidationEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
        }

        public List<AffidavitValidationResult> ValidateAffidavitRecord(AffidavitSaveRequestDetail affidavitDetail)
        {
            Dictionary<int, int> spotLengthDict = null;

            var validationResults = new List<AffidavitValidationResult>();

            if (string.IsNullOrWhiteSpace(affidavitDetail.ProgramName))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "ProgramName",
                    ErrorMessage = "'ProgramName' is required",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.Genre))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "Genre",
                    ErrorMessage = "'Genre' is required",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.LeadInProgramName))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "LeadInProgramName",
                    ErrorMessage = "'LeadInProgramName' is required",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.LeadInGenre))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "LeadInGenre",
                    ErrorMessage = "'LeadInGenre' is required",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.LeadOutProgramName))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "LeadOutProgramName",
                    ErrorMessage = "'LeadOutProgramName' is required",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.LeadOutGenre))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "LeadOutGenre",
                    ErrorMessage = "'LeadOutGenre' is required",
                });
            }

            if (affidavitDetail.AirTime == DateTime.MinValue)
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "AirTime",
                    ErrorMessage = "'AirTime' must be a valid date",
                });
            }

            if (affidavitDetail.InventorySource == (int) (InventorySourceEnum.Blank))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "InventorySource",
                    ErrorMessage = "'InventorySource' must be valid",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.Station))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "Station",
                    ErrorMessage = "'Station' is required",
                });
            }

            if (!_IsSpotLengthValid(affidavitDetail.SpotLength, ref spotLengthDict))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "SpotLength",
                    ErrorMessage = "'SpotLength' must be valid broadcast spot length: 15,30,60,120,180,300",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.Isci))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "Isci",
                    ErrorMessage = "'Isci' is required",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.Affiliate))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "Affiliate",
                    ErrorMessage = "'Affiliate' is required",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.ShowType))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "ShowType",
                    ErrorMessage = "'ShowType' is required",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.LeadInShowType))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "LeadInShowType",
                    ErrorMessage = "'LeadInShowType' is required",
                });
            }

            if (string.IsNullOrWhiteSpace(affidavitDetail.LeadOutShowType))
            {
                validationResults.Add(new AffidavitValidationResult()
                {
                    InvalidField = "LeadOutShowType",
                    ErrorMessage = "'LeadOutShowType' is required",
                });
            }

            return validationResults;
        }

        private bool _IsSpotLengthValid(int spotLength, ref Dictionary<int, int> spotLengthDict)
        {
            if (spotLengthDict == null)
                spotLengthDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>()
                    .GetSpotLengthAndIds();

            return spotLengthDict.ContainsKey(spotLength);
        }
    }
}