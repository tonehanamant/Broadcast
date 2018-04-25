using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IMyEventsReportDataRepository : IDataRepository
    {
        List<MyEventsReportData> GetMyEventsReportData(int proposalId);
    }

    public class MyEventsReportDataRepository : BroadcastRepositoryBase, IMyEventsReportDataRepository
    {
        private readonly ISMSClient _SmsClient;

        public MyEventsReportDataRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper) : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
            _SmsClient = pSmsClient;
        }

        public List<MyEventsReportData> GetMyEventsReportData(int proposalId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var myEventsReportData = new List<MyEventsReportData>();

                    var affidavitData = (from proposal in context.proposals
                                         from proposalVersion in proposal.proposal_versions
                                         from proposalVersionDetail in proposalVersion.proposal_version_details
                                         from proposalVersionQuarters in proposalVersionDetail.proposal_version_detail_quarters
                                         from proposalVersionWeeks in proposalVersionQuarters.proposal_version_detail_quarter_weeks
                                         from affidavitClientScrub in proposalVersionWeeks.affidavit_client_scrubs
                                         let affidavitFileDetail = affidavitClientScrub.affidavit_file_details
                                         where proposalVersionDetail.proposal_versions.proposal_id == proposalId && 
                                         affidavitClientScrub.status == (int)ScrubbingStatus.InSpec
                                         select new { affidavitFileDetail, affidavitClientScrub, proposal, proposalVersionDetail }).ToList();

                    var spotLengths = (from spotLength in context.spot_lengths select spotLength).ToList();

                    foreach (var affidavit in affidavitData)
                    {
                        var advertiser = _SmsClient.FindAdvertiserById(affidavit.proposal.advertiser_id);

                        var myEventsReportDataItem = new MyEventsReportData
                        {
                            CallLetter = affidavit.affidavitFileDetail.station,
                            LineupStartDate = affidavit.affidavitFileDetail.original_air_date,
                            LineupStartTime = new DateTime().AddSeconds(affidavit.affidavitFileDetail.air_time),
                            SpotLength = spotLengths.Single(y => y.id == affidavit.affidavitFileDetail.spot_length_id).length,
                            Advertiser = advertiser.Display,
                            DaypartCode = affidavit.proposalVersionDetail.daypart_code
                        };

                        myEventsReportData.Add(myEventsReportDataItem);
                    }

                    return myEventsReportData;
                });
        }

        private DateTime _GetWeekStartDate(DateTime date)
        {
            var differenceToMonday = (date.DayOfWeek - DayOfWeek.Monday);

            if (differenceToMonday < 0)
                differenceToMonday += 7;

            return date.AddDays(-differenceToMonday);
        }
    }
}
