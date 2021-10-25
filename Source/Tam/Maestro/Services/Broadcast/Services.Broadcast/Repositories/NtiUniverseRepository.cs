using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Nti;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface INtiUniverseRepository : IDataRepository
    {
        List<NtiUniverseAudienceMapping> GetNtiUniverseAudienceMappings();

        void SaveNtiUniverses(NtiUniverseHeader ntiUniverseHeader);

        NtiUniverseHeader GetLatestLoadedNsiUniverses();

        double GetLatestNtiUniverse(int audienceId);
    }

    public class NtiUniverseRepository : BroadcastRepositoryBase, INtiUniverseRepository
    {
        public NtiUniverseRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

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

        public double GetLatestNtiUniverse(int audienceId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return (from nti_universes in context.nti_universes
                        where nti_universes.audience_id == audienceId
                        orderby nti_universes.nti_universe_header_id descending
                        select nti_universes.universe).FirstOrDefault();
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
