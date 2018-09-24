using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPostLogService : IApplicationService
    {
        WWTVSaveResult SaveKeepingTrac(InboundFileSaveRequest saveRequest, string username, DateTime currentDateTime);

        WWTVSaveResult SaveKeepingTracValidationErrors(InboundFileSaveRequest saveRequest, string userName, List<WWTVInboundFileValidationResult> validationResults);
    }

    public class PostLogService : IPostLogService
    {
        private readonly IPostLogRepository _PostLogRepository;
        private readonly IPostLogEngine _PostLogEngine;

        public PostLogService(IDataRepositoryFactory dataRepositoryFactory
           , IPostLogEngine postLogEngine)
        {
            _PostLogRepository = dataRepositoryFactory.GetDataRepository<IPostLogRepository>();
            _PostLogEngine = postLogEngine;
        }

        public WWTVSaveResult SaveKeepingTrac(InboundFileSaveRequest saveRequest, string username, DateTime currentDateTime)
        {
            var postLogFile = _MapInboundFileSaveRequestToPostLogFile(saveRequest, currentDateTime);
            var result = _PostLogSaveResult(saveRequest, username, currentDateTime, postLogFile);

            return result;
        }

        public WWTVSaveResult SaveKeepingTracValidationErrors(InboundFileSaveRequest saveRequest, string userName, List<WWTVInboundFileValidationResult> validationResults)
        {
            var postLogFile = _MapInboundFileSaveRequestToPostLogFile(saveRequest, DateTime.Now);

            var problems = _MapValidationErrorToWWTVFileProblem((validationResults));
            postLogFile.FileProblems.AddRange(problems);
            var result = new WWTVSaveResult
            {
                ValidationResults = validationResults
            };
            postLogFile.Status = validationResults.Any() ? FileProcessingStatusEnum.Invalid : FileProcessingStatusEnum.Valid;

            result.Id = _PostLogRepository.SavePostLogFile(postLogFile);
            return result;
        }

        public List<WWTVFileProblem> _MapValidationErrorToWWTVFileProblem(List<WWTVInboundFileValidationResult> validationResults)
        {
            List<WWTVFileProblem> problems = new List<WWTVFileProblem>();

            validationResults.ForEach(v =>
            {
                WWTVFileProblem problem = new WWTVFileProblem();
                var description = v.ErrorMessage;
                if (!string.IsNullOrEmpty(v.InvalidField))
                {
                    description = string.Format("Record: {0}: Field: '{1}' is invalid\r\n{2}", v.InvalidLine, v.InvalidField, v.ErrorMessage);
                }
                problem.ProblemDescription = description;
                problems.Add(problem);
            });
            return problems;
        }

        private WWTVSaveResult _PostLogSaveResult(InboundFileSaveRequest saveRequest, string username,
            DateTime currentDateTime, PostLogFile postLogFile)
        {
            var result = new WWTVSaveResult();

            var lineNumber = 0;
            foreach (var saveRequestDetail in saveRequest.Details)
            {
                lineNumber++;
                var validationErrors = _PostLogEngine.ValidatePostLogRecord(saveRequestDetail);

                if (validationErrors.Any())
                {
                    validationErrors.ForEach(r => r.InvalidLine = lineNumber);
                    var problems = _MapValidationErrorToWWTVFileProblem(validationErrors);
                    postLogFile.FileProblems.AddRange(problems);

                    result.ValidationResults.AddRange(validationErrors);
                }
            }

            postLogFile.Status = result.ValidationResults.Any() ? FileProcessingStatusEnum.Invalid : FileProcessingStatusEnum.Valid;

            if (result.ValidationResults.Any())
            {   // save and get out
                result.Id = _PostLogRepository.SavePostLogFile(postLogFile);
                return result;
            }

            result.Id = _PostLogRepository.SavePostLogFile(postLogFile);

            return result;
        }

        private static PostLogFile _MapInboundFileSaveRequestToPostLogFile(InboundFileSaveRequest saveRequest, DateTime currentDateTime)
        {
            if (saveRequest == null)
            {
                throw new Exception("No affidavit data received.");
            }

            var postLogFile = new PostLogFile
            {
                CreatedDate = currentDateTime,
                FileHash = saveRequest.FileHash,
                FileName = saveRequest.FileName,
                SourceId = saveRequest.Source
            };
            return postLogFile;
        }
    }
}
