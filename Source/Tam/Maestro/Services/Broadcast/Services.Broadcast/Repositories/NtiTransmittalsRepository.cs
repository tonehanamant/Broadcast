using System.Linq;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
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
    }

    public class NtiTransmittalsRepository : BroadcastRepositoryBase, INtiTransmittalsRepository
    {
        public NtiTransmittalsRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
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
                    var nti_transmittals_file = _MapFromScrubbingFile(ntiFile);
                    context.nti_transmittals_files.Add(nti_transmittals_file);
                    context.SaveChanges();
                });
        }

        private nti_transmittals_files _MapFromScrubbingFile(NtiFile ntiFile)
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
                    }).ToList()
                }).ToList()
            };
        }
    }
}
