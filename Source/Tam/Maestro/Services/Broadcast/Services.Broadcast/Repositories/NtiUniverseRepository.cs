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

namespace Services.Broadcast.Repositories
{
    public interface INtiUniverseRepository : IDataRepository
    {
        List<NtiUniverseAudienceMapping> GetNtiUniverseAudienceMappings();

        void SaveNtiUniverses(NtiUniverseHeader ntiUniverseHeader);

        NtiUniverseHeader GetLatestLoadedNsiUniverses();
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
                    NtiUniverses = header.nti_universes.Select(x => new NtiUniverse
                    {
                        Id = x.id,
                        Audience = _MapToBroadcastAudience(x.audience),
                        Universe = x.universe
                    }).ToList()
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

        private BroadcastAudience _MapToBroadcastAudience(audience a)
        {
            return new BroadcastAudience
            {
                Id = a.id,
                CategoryCode = (EBroadcastAudienceCategoryCode)a.category_code,
                SubCategoryCode = a.sub_category_code,
                RangeStart = a.range_start,
                RangeEnd = a.range_end,
                Custom = a.custom,
                Code = a.code,
                Name = a.name,
            };
        }
    }
}
