using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Vpvh;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.Repositories
{
    public interface IVpvhRepository : IDataRepository
    {
        bool HasFile(string fileHash);

        void SaveFile(VpvhFile vpvhFile);

        void SaveQuarter(VpvhQuarter quarter);

        void SaveNewQuarter(VpvhQuarter quarter);

        List<VpvhQuarter> GetQuarters(QuarterDto quarter);

        List<VpvhQuarter> GetQuarters(int year, int quarter, List<int> audienceIds);

        List<VpvhQuarter> GetQuartersByYears(IEnumerable<int> years);

        VpvhQuarter GetQuarter(int audienceId, int year, int quarter);

        List<VpvhAudienceMapping> GetVpvhMappings();

        List<VpvhQuarter> GetAllQuarters();

        List<QuarterDto> GetQuartersWithVpvhData();
    }

    public class VpvhRepository : BroadcastRepositoryBase, IVpvhRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignRepository"/> class.
        /// </summary>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="pConfigurationWebApiClient">The p configuration web API client.</param>
        public VpvhRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        public List<VpvhQuarter> GetQuarters(QuarterDto quarter)
        {
            return _InReadUncommitedTransaction(
                context => context.vpvh_quarters.Include(v => v.audience).Where(v => v.quarter == quarter.Quarter && v.year == quarter.Year).Select(_MapToDto).ToList());
        }

        public List<QuarterDto> GetQuartersWithVpvhData()
        {
            return _InReadUncommitedTransaction(context =>
                context.vpvh_quarters
                    .Select(x => new { x.quarter, x.year })
                    .Distinct()
                    .ToList()
                    .Select(x => new QuarterDto { Quarter = x.quarter, Year = x.year })
                    .ToList());
        }

        public List<VpvhQuarter> GetQuartersByYears(IEnumerable<int> years)
        {
            return _InReadUncommitedTransaction(
                context => context.vpvh_quarters.Include(v => v.audience).Where(v => years.Contains(v.year)).Select(_MapToDto).ToList());
        }

        public List<VpvhQuarter> GetQuarters(int year, int quarter, List<int> audienceIds)
        {
            return _InReadUncommitedTransaction(
                context => context.vpvh_quarters.Include(v => v.audience)
                .Where(v => v.quarter == quarter && v.year == year && audienceIds.Contains(v.audience_id) ).Select(_MapToDto).ToList());
        }

        public VpvhQuarter GetQuarter(int audienceId, int year, int quarter)
        {
            return _MapToDto(_InReadUncommitedTransaction(
                context => context.vpvh_quarters.Include(v => v.audience).Where(v => v.quarter == quarter && v.year == year && v.audience_id == audienceId).SingleOrDefault()));
        }

        public List<VpvhQuarter> GetAllQuarters()
        {
            return _InReadUncommitedTransaction(
                context => context.vpvh_quarters.Include(v => v.audience).Select(_MapToDto).ToList());
        }

        public List<VpvhAudienceMapping> GetVpvhMappings()
        {
            return _InReadUncommitedTransaction(context => 
                context.vpvh_audience_mappings
                .Include(v => v.audience)
                .Include(v => v.compose_audience)
                .Select(_MapToVpvhMappingAudienceDto).ToList()
            );
        }

        public bool HasFile(string fileHash) =>
            _InReadUncommitedTransaction(
                context => context.vpvh_files.Any(x => x.file_hash == fileHash));

        public void SaveFile(VpvhFile vpvhFile)
        {
            _InReadUncommitedTransaction(context =>
            {
                context.vpvh_files.Add(new vpvh_files
                {
                    created_date = vpvhFile.CreatedDate,
                    created_by = vpvhFile.CreatedBy,
                    error_message = vpvhFile.ErrorMessage,
                    file_hash = vpvhFile.FileHash,
                    file_name = vpvhFile.FileName,
                    success = vpvhFile.Success,
                    vpvhs = vpvhFile.Items.Select(v => new vpvh
                    {
                        am_news = v.AMNews,
                        audience_id = v.Audience.Id,
                        pm_news = v.PMNews,
                        quarter = v.Quarter,
                        syn_all = v.SynAll,
                        year = v.Year
                    }).ToList()
                });

                context.SaveChanges();
            });
        }

        public void SaveNewQuarter(VpvhQuarter quarterDto)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var vpvhQuarter = new vpvh_quarters();
                    _MapFromDto(vpvhQuarter, quarterDto);
                    context.vpvh_quarters.Add(vpvhQuarter);

                    context.SaveChanges();
                });
        }

        public void SaveQuarter(VpvhQuarter quarterDto)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var vpvhQuarter = context.vpvh_quarters
                    .Single(v => v.quarter == quarterDto.Quarter && v.year == quarterDto.Year && v.audience_id == quarterDto.Audience.Id);

                    _MapFromDto(vpvhQuarter, quarterDto);

                    context.SaveChanges();
                });
        }

        private void _MapFromDto(vpvh_quarters quarter, VpvhQuarter quarterDto)
        {
            quarter.am_news = quarterDto.AMNews;
            quarter.quarter = quarterDto.Quarter;
            quarter.audience_id = quarterDto.Audience.Id;
            quarter.pm_news = quarterDto.PMNews;
            quarter.syn_all = quarterDto.SynAll;
            quarter.tdn = quarterDto.Tdn;
            quarter.tdns = quarterDto.Tdns;
            quarter.year = quarterDto.Year;
        }

        private VpvhQuarter _MapToDto(vpvh_quarters quarter)
        {
            if (quarter == null)
                return null;

            return new VpvhQuarter
            {
                Id = quarter.id,
                AMNews = quarter.am_news,
                Audience = _MapToDisplayAudience(quarter.audience),
                PMNews = quarter.pm_news,
                Quarter = quarter.quarter,
                SynAll = quarter.syn_all,
                Tdn = quarter.tdn,
                Tdns = quarter.tdns,
                Year = quarter.year
            };
        }
            

        private DisplayAudience _MapToDisplayAudience(audience audience) =>
            new DisplayAudience(audience.id, audience.code);

        private VpvhAudienceMapping _MapToVpvhMappingAudienceDto(vpvh_audience_mappings mapping) =>
            new VpvhAudienceMapping
            {
                Id = mapping.id,
                Audience = _MapToDisplayAudience(mapping.audience),
                ComposeAudience = _MapToDisplayAudience(mapping.compose_audience),
                Operation = (VpvhOperationEnum)mapping.operation
            };
    }
}
