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
using Services.Broadcast.Extensions;
using System.Data;
using System.Data.SqlClient;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.Repositories
{
    public interface IProgramMappingRepository : IDataRepository
    {
        List<ProgramMappingsDto> GetProgramMappingsByOriginalProgramNames(IEnumerable<string> originalProgramNames);

        /// <summary>
        /// Get all the program mappings.
        /// </summary>
        /// <returns></returns>
        List<ProgramMappingsDto> GetProgramMappings();

        void CreateProgramMappings(IEnumerable<ProgramMappingsDto> newProgramMappings, string createdBy, DateTime createdAt);

        void UpdateProgramMappings(IEnumerable<ProgramMappingsDto> programMappings, string updatedBy, DateTime updatedAt);
    }

    public class ProgramMappingRepository : BroadcastRepositoryBase, IProgramMappingRepository
    {
        public ProgramMappingRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) 
            : base(pBroadcastContextFactory, pTransactionHelper, configurationWebApiClient, featureToggleHelper, configurationSettingsHelper)
        {
        }

        public List<ProgramMappingsDto> GetProgramMappingsByOriginalProgramNames(IEnumerable<string> originalProgramNames)
        {
            var chunks = originalProgramNames.GetChunks(BroadcastConstants.DefaultDatabaseQueryChunkSize);

            var result = chunks
                .AsParallel()
                .SelectMany(chunk =>
                {
                    return _InReadUncommitedTransaction(context =>
                    {
                        return context.program_name_mappings
                            .Include(x => x.genre)
                            .Include(x => x.show_types)
                            .Where(x => chunk.Contains(x.inventory_program_name))
                            .ToList()
                            .Select(_MapToDto)
                            .ToList();
                    });
                })
                .ToList();

            return result;
        }

        /// <inheritdoc />
        public void CreateProgramMappings(IEnumerable<ProgramMappingsDto> newProgramMappings, string createdBy, DateTime createdAt)
        {
            var chunks = newProgramMappings.GetChunks(BroadcastConstants.DefaultDatabaseQueryChunkSize);

            foreach (var chunk in chunks)
            {
                _InReadUncommitedTransaction(context =>
                {
                    var recordsToInsert = chunk
                        .Select(x =>
                        {
                            var newProgramMapping = new program_name_mappings();
                            _MapFromDto(x, newProgramMapping);
                            newProgramMapping.created_at = createdAt;
                            newProgramMapping.created_by = createdBy;
                            return newProgramMapping;
                        })
                        .ToList();

                    BulkInsert(context, recordsToInsert, propertiesToIgnore: new List<string> { "id" });
                });
            }
        }

        public void UpdateProgramMappings(IEnumerable<ProgramMappingsDto> programMappings, string updatedBy, DateTime updatedAt)
        {
            var chunks = programMappings.GetChunks(BroadcastConstants.DefaultDatabaseQueryChunkSize);

            foreach (var chunk in chunks)
            {
                _InReadUncommitedTransaction(c =>
                {
                    var modified_at_param = new SqlParameter("modified_at", SqlDbType.DateTime) { Value = updatedAt };
                    var modified_by_param = new SqlParameter("modified_by", SqlDbType.VarChar) { Value = updatedBy };

                    var update_requests = new DataTable();
                    update_requests.Columns.Add("program_name_mapping_id");
                    update_requests.Columns.Add("official_program_name");
                    update_requests.Columns.Add("genre_id");
                    update_requests.Columns.Add("show_type_id");

                    chunk.ForEach(x => update_requests.Rows.Add(
                        x.Id,
                        x.OfficialProgramName,
                        x.OfficialGenre.Id,
                        x.OfficialShowType.Id));

                    var update_requests_param = new SqlParameter("update_requests", SqlDbType.Structured) { Value = update_requests, TypeName = "ProgramMappingUpdateRequests" };

                    var storedProcedureName = "[dbo].[usp_UpdateProgramNameMappings]";

                    var command = c.Database.Connection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = storedProcedureName;
                    command.Parameters.Add(modified_at_param);
                    command.Parameters.Add(modified_by_param);
                    command.Parameters.Add(update_requests_param);
                    command.CommandTimeout = 0; // This makes it infinite

                    command.ExecuteNonQuery();
                });


                // Plan B
                /*
                var mappingById = chunk.ToDictionary(x => x.Id, x => x);
                var mappingIds = chunk.Select(x => x.Id).ToList();

                _InReadUncommitedTransaction(context =>
                {
                    var dbMappings = context.program_name_mappings.Where(x => mappingIds.Contains(x.id)).ToList();

                    foreach (var dbMapping in dbMappings)
                    {
                        var programMapping = mappingById[dbMapping.id];

                        dbMapping.official_program_name = programMapping.OfficialProgramName;
                        dbMapping.genre_id = programMapping.OfficialGenre.Id;
                        dbMapping.show_type_id = programMapping.OfficialShowType.Id;

                        dbMapping.modified_by = updatedBy;
                        dbMapping.modified_at = updatedAt;
                    }

                    context.SaveChanges();
                });
                */
            }
        }

        /// <inheritdoc />
        public List<ProgramMappingsDto> GetProgramMappings()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var mappingEntities = context.program_name_mappings
                    .Include(x => x.genre)
                    .Include(x => x.show_types)
                    .ToList();

                var programMappings = mappingEntities.Select(_MapToDto).ToList();
                return programMappings;
            });
        }

        private ProgramMappingsDto _MapToDto(program_name_mappings program_name_mappings)
        {
            if(program_name_mappings == null)
            {
                return null;
            }
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
                    Name = program_name_mappings.show_types.name,
                    ShowTypeSource = (ProgramSourceEnum)program_name_mappings.show_types.program_source_id
                },
                CreatedBy = program_name_mappings.created_by,
                CreatedAt = program_name_mappings.created_at,
                ModifiedBy = program_name_mappings.modified_by,
                ModifiedAt = program_name_mappings.modified_at
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
