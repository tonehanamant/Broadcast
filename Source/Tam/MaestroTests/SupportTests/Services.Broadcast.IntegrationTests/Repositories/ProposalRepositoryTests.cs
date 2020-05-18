using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.Repositories
{
    [TestFixture]
    [Category("short_running")]
    public class ProposalRepositoryTests
    {
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProposalDtoWithSortedQuartersWeeks()
        {
            using (new TransactionScopeWrapper())
            {
                var sut = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
                var proposal = new EntityFrameworkMapping.Broadcast.proposal();
                var proposalVersion = new EntityFrameworkMapping.Broadcast.proposal_versions();
                proposalVersion.proposal = proposal;
                var proposalDetail = new EntityFrameworkMapping.Broadcast.proposal_version_details();
                proposalVersion.proposal_version_details = new List<EntityFrameworkMapping.Broadcast.proposal_version_details>()
                {
                    proposalDetail
                };
                proposalDetail.proposal_version_detail_quarters = new List<EntityFrameworkMapping.Broadcast.proposal_version_detail_quarters>()
                {
                    new EntityFrameworkMapping.Broadcast.proposal_version_detail_quarters()
                    {
                        year = 2018,
                        quarter = 2,
                        proposal_version_detail_quarter_weeks = new List<EntityFrameworkMapping.Broadcast.proposal_version_detail_quarter_weeks>()
                        {
                            new EntityFrameworkMapping.Broadcast.proposal_version_detail_quarter_weeks()
                            {
                                media_week_id = 746,
                                start_date = new DateTime(2018,4,9),
                                end_date = new DateTime(2018,4,15)
                            },
                            new EntityFrameworkMapping.Broadcast.proposal_version_detail_quarter_weeks()
                            {
                                media_week_id = 747,
                                start_date = new DateTime(2018,4,16),
                                end_date = new DateTime(2018,4,22)
                            },
                            new EntityFrameworkMapping.Broadcast.proposal_version_detail_quarter_weeks()
                            {
                                 media_week_id = 745,
                                 start_date = new DateTime(2018,4,2),
                                 end_date = new DateTime(2018,4,8)
                            }
                        }
                    },
                    new EntityFrameworkMapping.Broadcast.proposal_version_detail_quarters()
                    {
                        year = 2018,
                        quarter = 1,
                        proposal_version_detail_quarter_weeks = new List<EntityFrameworkMapping.Broadcast.proposal_version_detail_quarter_weeks>()
                        {
                            new EntityFrameworkMapping.Broadcast.proposal_version_detail_quarter_weeks()
                            {
                                 media_week_id = 732,
                                 start_date = new DateTime(2018,1,1),
                                 end_date = new DateTime(2018,1,7)
                            },
                            new EntityFrameworkMapping.Broadcast.proposal_version_detail_quarter_weeks()
                            {
                                media_week_id = 734,
                                start_date = new DateTime(2018,1,15),
                                end_date = new DateTime(2018,1,21)
                            },
                            new EntityFrameworkMapping.Broadcast.proposal_version_detail_quarter_weeks()
                            {
                                media_week_id = 733,
                                start_date = new DateTime(2018,1,8),
                                end_date = new DateTime(2018,1,14)
                            }
                        }
                    }
                };

                var result = sut.MapToProposalDto(proposal, proposalVersion);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }
    }
}
