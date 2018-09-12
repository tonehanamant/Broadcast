using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IPostLogRepository : IDataRepository
    {
        /// <summary>
        /// Saves processing validation results
        /// </summary>
        /// <param name="validationResults">List of FileValidationResult objects</param>
        void SavePreprocessingValidationResults(List<FileValidationResult> validationResults);

        /// <summary>
        /// Saves a post log file
        /// </summary>
        /// <param name="postLogFile">Post log file to be saved</param>
        /// <returns>The newly created id</returns>
        int SavePostLogFile(PostLogFile postLogFile);
    }
    public class PostLogRepository : BroadcastRepositoryBase, IPostLogRepository
    {
        public PostLogRepository(ISMSClient pSmsClient,
                                IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
                                ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        /// <summary>
        /// Saves processing validation results
        /// </summary>
        /// <param name="validationResults">List of FileValidationResult objects</param>
        public void SavePreprocessingValidationResults(List<FileValidationResult> validationResults)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.postlog_outbound_files.AddRange(validationResults.Select(item =>
                    new postlog_outbound_files()
                    {
                        created_date = item.CreatedDate,
                        file_hash = item.FileHash,
                        file_name = item.FileName,
                        source_id = (int) item.Source,
                        status = (int)item.Status,
                        created_by = item.CreatedBy,
                        postlog_outbound_file_problems = item.ErrorMessages.Select(y =>
                            new postlog_outbound_file_problems()
                            {
                                problem_description = y
                            }).ToList()
                    }).ToList());
                context.SaveChanges();
            });
        }

        /// <summary>
        /// Saves a post log file
        /// </summary>
        /// <param name="postLogFile">Post log file to be saved</param>
        /// <returns>The newly created id</returns>
        public int SavePostLogFile(PostLogFile postLogFile)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var postlog_file = _MapFromPostLogFile(postLogFile);
                    context.postlog_files.Add(postlog_file);
                    context.SaveChanges();
                    return postlog_file.id;
                });
        }

        private postlog_files _MapFromPostLogFile(PostLogFile affidavitFile)
        {
            var result = new postlog_files
            {
                created_date = affidavitFile.CreatedDate,
                file_hash = affidavitFile.FileHash,
                file_name = affidavitFile.FileName,
                source_id = affidavitFile.SourceId,
                status = (int)affidavitFile.Status,
                postlog_file_problems = affidavitFile.FileProblems.Select(p => new postlog_file_problems()
                {
                    id = p.Id,
                    postlog_file_id = p.FileId,
                    problem_description = p.ProblemDescription
                }).ToList(),
                postlog_file_details = affidavitFile.FileDetails.Select(d => new postlog_file_details
                {
                    air_time = d.AirTime,
                    original_air_date = d.OriginalAirDate,
                    isci = d.Isci,
                    program_name = d.ProgramName,
                    genre = d.Genre,
                    spot_length_id = d.SpotLengthId,
                    station = d.Station,
                    market = d.Market,
                    affiliate = d.Affiliate,
                    estimate_id = d.EstimateId,
                    inventory_source = d.InventorySource,
                    spot_cost = d.SpotCost,
                    leadin_genre = d.LeadinGenre,
                    leadout_genre = d.LeadoutGenre,
                    leadin_program_name = d.LeadinProgramName,
                    leadout_program_name = d.LeadoutProgramName,
                    leadin_end_time = d.LeadInEndTime,
                    leadout_start_time = d.LeadOutStartTime,
                    program_show_type = d.ShowType,
                    leadin_show_type = d.LeadInShowType,
                    leadout_show_type = d.LeadOutShowType,
                    adjusted_air_date = d.AdjustedAirDate,
                    archived = d.Archived,
                    postlog_file_detail_problems = _MapFromFileDetailProblems(d.FileDetailProblems),
                    postlog_file_detail_demographics = d.Demographics.Select(demo =>
                        new postlog_file_detail_demographics
                        {
                            audience_id = demo.AudienceId,
                            overnight_impressions = demo.OvernightImpressions,
                            overnight_rating = demo.OvernightRating
                        }).ToList()
                }).ToList()
            };

            return result;
        }

        private ICollection<postlog_file_detail_problems> _MapFromFileDetailProblems(List<FileDetailProblem> affidavitFileDetailProblems)
        {
            var result = affidavitFileDetailProblems.Select(p =>
                        new postlog_file_detail_problems
                        {
                            problem_description = p.Description,
                            problem_type = (int)p.Type
                        }).ToList();
            return result;
        }

    }
}
