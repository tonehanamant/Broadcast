using System.Collections.Generic;
using System.Linq;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Nti;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface INtiTransmittalsRepository : IDataRepository
    {
        /// <summary>
        /// Save a Scrubbing file to nti tables
        /// </summary>
        /// <param name="file">NtiFile object to save</param>
        void SaveFile(NtiFile file);

        /// <summary>
        /// Gets the proposal weeks by myevents report name
        /// </summary>
        /// <param name="reportName">Report name to filter by</param>
        /// <returns>List of NtiProposalVersionDetailWeek objects</returns>
        List<NtiProposalVersionDetailWeek> GetProposalWeeksByReportName(string reportName);
    }

    public class NtiTransmittalsRepository : BroadcastRepositoryBase, INtiTransmittalsRepository
    {
        public NtiTransmittalsRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        /// <summary>
        /// Gets the proposal weeks by myevents report name
        /// </summary>
        /// <param name="reportName">Report name to filter by</param>
        /// <returns>List of NtiProposalVersionDetailWeek objects</returns>
        public List<NtiProposalVersionDetailWeek> GetProposalWeeksByReportName(string reportName)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var data = (from week in context.proposal_version_detail_quarter_weeks
                                from affidavitClientScrub in week.affidavit_client_scrubs
                                where week.myevents_report_name == reportName 
                                && week.proposal_version_detail_quarters.proposal_version_details.proposal_versions.status == (int)ProposalEnums.ProposalStatusType.Contracted
                                select new { week.id, affidavitClientScrub.affidavit_client_scrub_audiences })
                                .GroupBy(x => x.id);
                    return data.Select(x => new
                    {
                        WeekId = x.Key,
                        Audiences = x.SelectMany(y => y.affidavit_client_scrub_audiences)
                    })
                    .AsEnumerable()
                    .Select(x => new NtiProposalVersionDetailWeek
                    {
                        WeekId = x.WeekId,
                        NsiImpressions = x.Audiences
                        .GroupBy(y => y.audience_id)
                        .ToDictionary(k => k.Key, k => k.Sum(w => w.impressions))
                    }).ToList();
                });
        }

        /// <summary>
        /// Save a nti file to nti tables
        /// </summary>
        /// <param name="file">NtiFile object to save</param>
        public void SaveFile(NtiFile ntiFile)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var nti_transmittals_file = _MapFromNtiFile(ntiFile);
                    var weekIds = nti_transmittals_file.nti_transmittals_file_reports.SelectMany(y => y.nti_transmittals_audiences.Select(w => w.proposal_version_detail_quarter_week_id)).Distinct().ToList();
                    var previouslyAudiences = context.nti_transmittals_audiences.Where(x => weekIds.Contains(x.proposal_version_detail_quarter_week_id)).ToList();

                    context.nti_transmittals_audiences.RemoveRange(previouslyAudiences);
                    context.nti_transmittals_files.Add(nti_transmittals_file);
                    context.SaveChanges();
                });
        }

        private nti_transmittals_files _MapFromNtiFile(NtiFile ntiFile)
        {
            return new nti_transmittals_files
            {
                created_by = ntiFile.CreatedBy,
                created_date = ntiFile.CreatedDate,
                file_name = ntiFile.FileName,
                status = (int)ntiFile.Status,
                nti_transmittals_file_problems = ntiFile.FileProblems.Select(x => new nti_transmittals_file_problems { problem_description = x.ProblemDescription }).ToList(),
                nti_transmittals_file_reports = ntiFile.Details.Select(x => new nti_transmittals_file_reports
                {
                    advertiser = x.Header.Advertiser,
                    CVG = int.Parse(x.Header.CVG),
                    date = x.Header.Date,
                    program_duration = int.Parse(x.Header.ProgramDuration),
                    program_id = int.Parse(x.Header.ProgramId),
                    program_type = x.Header.ProgramType,
                    report_name = x.Header.ReportName,
                    stations = int.Parse(x.Header.Stations),
                    stream = x.Header.Stream,
                    TA = double.Parse(x.Header.TA),
                    TbyC = int.Parse(x.Header.TbyC),
                    week_ending = x.Header.WeekEnding,
                    nti_transmittals_file_report_ratings = x.Ratings.Select(y => new nti_transmittals_file_report_ratings
                    {
                        category_name = y.Category,
                        subcategory_name = y.SubCategory,
                        impressions = y.Impressions,
                        percent = y.Percent,
                        VPVH = y.VPVH
                    }).ToList(),
                    nti_transmittals_audiences = x.ProposalWeeks.SelectMany(y => y.Audiences.Select(w => new nti_transmittals_audiences
                    {
                        audience_id = w.AudienceId,
                        impressions = w.Impressions,
                        proposal_version_detail_quarter_week_id = y.WeekId
                    }).ToList()).ToList()
                }).ToList()
            };
        }
    }
}
