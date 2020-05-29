using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Entities;
using Common.Services.Extensions;
using System;

namespace Services.Broadcast.Repositories
{
    public interface IProgramMappingRepository : IDataRepository
    {
        /// <summary>
        /// Checks if a program name mapping exists for original program name.
        /// </summary>
        /// <param name="originalProgramName">Original program name.</param>
        /// <returns>Boolean value representing whether mapping exists program name</returns>
        bool MappingExistsForOriginalProgramName(string originalProgramName);

        /// <summary>
        /// Gets the program mapping by original program name.
        /// </summary>
        /// <param name="originalProgramName">Name of the original program.</param>
        /// <returns>The program mapping</returns>
        ProgramMappingsDto GetProgramMappingByOriginalProgramName(string originalProgramName);

        List<ProgramMappingsDto> GetProgramMappings();

        /// <summary>
        /// Creates a new program mapping.
        /// </summary>
        /// <param name="newProgramMapping">The new program mapping.</param>
        /// <returns>The program mapping</returns>
        int CreateProgramMapping(ProgramMappingsDto newProgramMapping);

        /// <summary>
        /// Updates the program mapping.
        /// </summary>
        /// <param name="programMapping">The program mapping.</param>
        void UpdateProgramMapping(ProgramMappingsDto programMapping);
    }

    public class ProgramMappingRepository : BroadcastRepositoryBase, IProgramMappingRepository
    {
        public ProgramMappingRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient) 
            : base(pBroadcastContextFactory, pTransactionHelper, configurationWebApiClient)
        {
        }

        public ProgramMappingsDto GetProgramMappingByOriginalProgramName(string originalProgramName)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return _MapToDto(context.program_name_mappings
                        .Include(x => x.genre)
                        .Include(x => x.show_types)
                        .Single(x => x.inventory_program_name == originalProgramName, $"No program mapping found for name: {originalProgramName}"));
                });
        }

        public List<ProgramMappingsDto> GetProgramMappings()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.program_name_mappings
                        .Include(x => x.genre)
                        .Include(x => x.show_types)
                        .ToList()
                        .Select(_MapToDto)
                        .ToList();
                });
        }

        /// <inheritdoc />
        public bool MappingExistsForOriginalProgramName(string originalProgramName)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.program_name_mappings
                        .Any(x => x.inventory_program_name == originalProgramName);
                });
        }

        /// <inheritdoc />
        public int CreateProgramMapping(ProgramMappingsDto programMappingDto)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var newProgramMapping = new program_name_mappings();
                _MapFromDto(programMappingDto, newProgramMapping);

                context.program_name_mappings.Add(newProgramMapping);
                context.SaveChanges();

                programMappingDto.Id = newProgramMapping.id;

                return newProgramMapping.id;
            });
        }

        public void UpdateProgramMapping(ProgramMappingsDto programMappingDto)
        {
            _InReadUncommitedTransaction(context =>
            {
                var programMapping = context.program_name_mappings
                    .Include(x => x.genre)
                    .Include(x => x.show_types)
                    .Single(x => x.id == programMappingDto.Id, $"Program mapping not found with id: {programMappingDto.Id}");

                programMapping.official_program_name = programMappingDto.OfficialProgramName;
                programMapping.genre_id = programMappingDto.OfficialGenre.Id;
                programMapping.show_type_id = programMappingDto.OfficialShowType.Id;
                
                context.SaveChanges();
            });
        }

        private ProgramMappingsDto _MapToDto(program_name_mappings program_name_mappings)
        {
            return new ProgramMappingsDto
            {
                Id = program_name_mappings.id,
                OriginalProgramName = program_name_mappings.inventory_program_name,
                OfficialProgramName = program_name_mappings.official_program_name,
                OfficialGenre = new Genre
                {
                    Id = program_name_mappings.genre.id,
                    Name = program_name_mappings.genre.name
                },
                OfficialShowType = new ShowTypeDto
                {
                    Id = program_name_mappings.show_types.id,
                    Name = program_name_mappings.show_types.name
                }
            };
        }

        private void _MapFromDto(ProgramMappingsDto programMappingDto, program_name_mappings programMapping)
        {
            programMapping.inventory_program_name = programMappingDto.OriginalProgramName;
            programMapping.official_program_name = programMappingDto.OfficialProgramName;
            programMapping.show_type_id = programMappingDto.OfficialShowType.Id;
            programMapping.genre_id = programMappingDto.OfficialGenre.Id;
        }
    }
}
