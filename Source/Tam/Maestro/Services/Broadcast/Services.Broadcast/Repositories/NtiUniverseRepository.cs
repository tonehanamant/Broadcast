using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Nti;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using System.Data.Entity;
using System.Linq;
using Services.Broadcast.Entities;
using System;
using System.Transactions;
using System.Data;

namespace Services.Broadcast.Repositories
{
    public interface INtiUniverseRepository : IDataRepository
    {
        List<NtiUniverseAudienceMapping> GetNtiUniverseAudienceMappings();

        void SaveNtiUniverses(NtiUniverseHeader ntiUniverseHeader);

        NtiUniverseHeader GetLatestLoadedNsiUniverses();

        double? GetLatestNtiUniverseByYear(int audienceId, int year);
    }

    public class NtiUniverseRepository : BroadcastRepositoryBase, INtiUniverseRepository
    {
        public NtiUniverseRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper,
            IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public NtiUniverseHeader GetLatestLoadedNsiUniverses()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var header = context.nti_universe_headers
                                    .Include(x => x.nti_universes)
                                    .Include(x => x.nti_universes.Select(u => u.audience))
                                    .Include(x => x.nti_universe_details)
                                    .OrderByDescending(x => x.created_date)
                                    .FirstOrDefault();

                if (header == null)
                    throw new Exception("Nti universe data has not been loaded");

                return new NtiUniverseHeader
                {
                    Id = header.id,
                    CreatedDate = header.created_date,
                    CreatedBy = header.created_by,
                    Year = header.year,
                    Month = header.month,
                    WeekNumber = header.week_number,
                    NtiUniverseDetails = header.nti_universe_details.Select(x => new NtiUniverseDetail
                    {
                        Id = x.id,
                        NtiAudienceId = x.nti_audience_id,
                        NtiAudienceCode = x.nti_audience_code,
                        Universe = x.universe
                    }).ToList(),
                    NtiUniverses = header.nti_universes.Select(_MapToNtiUniverse).ToList()
                };
            });
        }

        public List<NtiUniverseAudienceMapping> GetNtiUniverseAudienceMappings()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var mappings = context.nti_universe_audience_mappings
                    .Include(x => x.audience)
                    .ToList();

                return mappings.Select(x => new NtiUniverseAudienceMapping
                {
                    Id = x.id,
                    NtiAudienceCode = x.nti_audience_code,
                    Audience = _MapToBroadcastAudience(x.audience)
                }).ToList();
            });
        }

        public double? GetLatestNtiUniverseByYear(int audienceId, int year)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, System.Transactions.IsolationLevel.ReadUncommitted))
            {
                var sql = @"SELECT universe
                            FROM nti_universes
                            WHERE audience_id = @audienceId 
	                            AND nti_universe_header_id = (SELECT TOP 1 id FROM nti_universe_headers
			                                                  WHERE year = @year ORDER BY month DESC)";

                return _InReadUncommitedTransaction(context =>
                {
                    var mediaMonthParam = new System.Data.SqlClient.SqlParameter("year", SqlDbType.Int) { Value = year };
                    var audienceParam = new System.Data.SqlClient.SqlParameter("audienceId", SqlDbType.Int) { Value = audienceId };

                    return context.Database.SqlQuery<double?>(sql, mediaMonthParam, audienceParam).SingleOrDefault();
                });
            }
        }

        public void SaveNtiUniverses(NtiUniverseHeader header)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.nti_universe_headers.Add(new nti_universe_headers
                {
                    created_date = header.CreatedDate,
                    created_by = header.CreatedBy,
                    year = header.Year,
                    month = header.Month,
                    week_number = header.WeekNumber,
                    nti_universe_details = header.NtiUniverseDetails.Select(x => new nti_universe_details
                    {
                        nti_audience_id = x.NtiAudienceId,
                        nti_audience_code = x.NtiAudienceCode,
                        universe = x.Universe
                    }).ToList(),
                    nti_universes = header.NtiUniverses.Select(x => new nti_universes
                    {
                        audience_id = x.Audience.Id,
                        universe = x.Universe
                    }).ToList()
                });

                context.SaveChanges();
            });
        }

        private BroadcastAudience _MapToBroadcastAudience(audience audience)
        {
            return new BroadcastAudience
            {
                Id = audience.id,
                CategoryCode = (EBroadcastAudienceCategoryCode)audience.category_code,
                SubCategoryCode = audience.sub_category_code,
                RangeStart = audience.range_start,
                RangeEnd = audience.range_end,
                Custom = audience.custom,
                Code = audience.code,
                Name = audience.name,
            };
        }

        private NtiUniverse _MapToNtiUniverse(nti_universes ntiUniveses)
        {
            return new NtiUniverse
            {
                Universe = ntiUniveses.universe,
                Id = ntiUniveses.id,
                Audience = _MapToBroadcastAudience(ntiUniveses.audience)
            };
        }
    }
}
