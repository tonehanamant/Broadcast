using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Entities;
using Common.Services.Extensions;
using Services.Broadcast.Entities.Enums;
using System;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.Repositories
{
    public interface IShowTypeRepository : IDataRepository
    {
        /// <summary>
        /// Finds a Maestro show type based on the input string
        /// </summary>
        /// <param name="showTypeSearchString">Parameter to filter by</param>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> FindMaestroShowType(string showTypeSearchString);
        List<LookupDto> GetMaestroShowTypesLookupDto();
        /// <summary>
        /// Gets the Maestro show type by name.
        /// </summary>
        /// <param name="showTypeName">Name of the show type.</param>
        /// <returns>ShowTypeDto</returns>
        ShowTypeDto GetMaestroShowTypeByName(string showTypeName);

        List<LookupDto> GetMasterShowTypesLookupDto();

        Dictionary<ShowTypeDto, ShowTypeDto> GetShowTypeMappings();

        List<ShowTypeDto> GetMaestroShowTypes();

        List<ShowTypeDto> GetMasterShowTypes();

        ShowTypeDto GetMaestroShowType(int showTypeId);
    }

    public class ShowTypeRepository : BroadcastRepositoryBase, IShowTypeRepository
    {

        public ShowTypeRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        /// <summary>
        /// Finds a show type based on the input string
        /// </summary>
        /// <param name="showTypeSearchString">Parameter to filter by</param>
        /// <returns>List of LookupDto objects</returns>
        public List<LookupDto> FindMaestroShowType(string showTypeSearchString)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.show_types
                        .Where(g => g.program_source_id == (int)ProgramSourceEnum.Maestro)
                        .Where(g => g.name.ToLower().Contains(showTypeSearchString.ToLower()))
                        .ToList()
                        .Select(_MapToLookupDto)
                        .ToList();
                });
        }

        /// <inheritdoc />
        public ShowTypeDto GetMaestroShowTypeByName(string showTypeName)
        {
            return _InReadUncommitedTransaction(context =>
            {
                return _MapToDto(context.show_types
                    .Where(g => g.program_source_id == (int)ProgramSourceEnum.Maestro)
                    .Single(item => item.name.ToUpper() == showTypeName.ToUpper(), $"No show type was found by name : {showTypeName}"));
            });
        }

        public List<LookupDto> GetMaestroShowTypesLookupDto()
        {
            return _InReadUncommitedTransaction(context => 
            {
                return context.show_types
                    .Where(s => s.program_source_id == (int)ProgramSourceEnum.Maestro)
                    .ToList()
                    .Select(_MapToLookupDto)
                    .OrderBy(x => x.Display)
                    .ToList();
            });
        }

        public List<LookupDto> GetMasterShowTypesLookupDto()
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.show_types
                    .Where(s => s.program_source_id == (int)ProgramSourceEnum.Master)
                    .ToList()
                    .Select(_MapToLookupDto)
                    .OrderBy(x => x.Display)
                    .ToList();
            });
        }

        public List<ShowTypeDto> GetMaestroShowTypes()
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.show_types
                    .Where(s => s.program_source_id == (int)ProgramSourceEnum.Maestro)
                    .ToList()
                    .Select(_MapToDto)
                    .OrderBy(x => x.Name)
                    .ToList();
            });
        }

        public List<ShowTypeDto> GetMasterShowTypes()
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.show_types
                    .Where(s => s.program_source_id == (int)ProgramSourceEnum.Master)
                    .ToList()
                    .Select(_MapToDto)
                    .OrderBy(x => x.Name)
                    .ToList();
            });
        }

        public Dictionary<ShowTypeDto, ShowTypeDto> GetShowTypeMappings()
        {
            return _InReadUncommitedTransaction(context =>
            {
                return context.show_type_mappings
                       .Select(x => new
                       {
                           MaestroShowType = new ShowTypeDto
                           {
                               Id = x.maestro_show_type_id,
                               Name = x.maestro_show_types.name,
                               ShowTypeSource = (ProgramSourceEnum)x.maestro_show_types.program_source_id
                           },
                           MasterShowType = new ShowTypeDto
                           {
                               Id = x.mapped_show_type_id,
                               Name = x.mapped_show_types.name,
                               ShowTypeSource = (ProgramSourceEnum)x.mapped_show_types.program_source_id
                           }
                       })
                       .ToDictionary(x => x.MaestroShowType, x => x.MasterShowType);
            });
        }

        private ShowTypeDto _MapToDto(show_types showType)
        {
            return new ShowTypeDto
            {
                Id = showType.id,
                Name = showType.name,
                ShowTypeSource = (ProgramSourceEnum)showType.program_source_id
            };
        }

        private LookupDto _MapToLookupDto(show_types showType)
        {
            return new LookupDto()
            {
                Id = showType.id,
                Display = showType.name
            };
        }

        public ShowTypeDto GetMaestroShowType(int showTypeId)
        {   
            return _InReadUncommitedTransaction(context =>
            {
                return _MapToDto(context.show_types
                    .Where(g => g.program_source_id == (int)ProgramSourceEnum.Maestro)
                    .Single(item => item.id == showTypeId, $"No show type was found by id : {showTypeId}"));
            });
        }
    }    
}
