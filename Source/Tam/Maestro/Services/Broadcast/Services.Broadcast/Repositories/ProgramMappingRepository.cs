﻿using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

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
        /// <summary>
        /// Get all the program mappings.
        /// </summary>       
        List<ProgramMappingsDto> GetInventoryProgramMappings();

        /// <summary>
        /// Creates new program mappings.
        /// </summary>
        void CreateProgramMappings(IEnumerable<ProgramMappingsDto> newProgramMappings, string createdBy, DateTime createdAt);

        /// <summary>
        /// Updates existing program mappings.
        /// </summary>
        void UpdateProgramMappings(IEnumerable<ProgramMappingsDto> programMappings, string updatedBy, DateTime updatedAt);

        /// <summary>
        /// Gets the Master Programs From Data Table
        /// </summary>
        /// <returns>List Of Master Programs</returns>
        List<ProgramMappingsDto> GetProgramsAndGeneresFromDataTable();

        /// <summary>
        /// Uploads the master program mappings.
        /// </summary>
        void UploadMasterProgramMappings(IEnumerable<ProgramMappingsDto> newProgramMappings, string createdBy, DateTime createdAt);

        /// <summary>
        /// Updates existing the master program mappings.
        /// </summary>
        void UpdateMasterProgramMappings(IEnumerable<ProgramMappingsDto> masterPrograms, string createdBy, DateTime createdAt);

        /// <summary>
        /// Gets the master programs. (From the programs table)
        /// </summary>
        List<MasterProgramsDto> GetMasterPrograms();

        /// <summary>
        /// Deletes the program_name_mapping records that do not exist in the programs program_name_exceptions tables.
        /// </summary>
        /// <returns>The number of records deleted.</returns>
        int CleanupOrphanedProgramNameMappings();

        /// <summary>
        /// Deletes the programs.
        /// </summary>
        int DeletePrograms();
    }

    public class ProgramMappingRepository : BroadcastRepositoryBase, IProgramMappingRepository
    {
        public ProgramMappingRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper) 
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        {
        }

        /// <inheritdoc />
        public List<ProgramMappingsDto> GetProgramMappingsByOriginalProgramNames(IEnumerable<string> originalProgramNames)
        {
            var chunks = originalProgramNames.GetChunks(BroadcastConstants.DefaultDatabaseQueryChunkSize);

            var result = chunks
                .AsParallel()
                .SelectMany(chunk =>
                {
                    return _InReadUncommitedTransaction(context =>
                    {
                        var mappingEntities = (from m in context.program_name_mappings.Include(x => x.show_types)
                                               join p in context.programs.Include(x => x.genre)
                                                on m.official_program_name equals p.name
                                               where chunk.Contains(m.inventory_program_name)
                                               select new
                                               {
                                                   mapping = m,
                                                   program = p
                                               })
                            .ToList();
                        var programMappings = mappingEntities.Select(x => _MapToDto(x.mapping, x.program)).ToList();
                        return programMappings;
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

        /// <inheritdoc />
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
                    update_requests.Columns.Add("show_type_id");

                    chunk.ForEach(x => update_requests.Rows.Add(
                        x.Id,
                        x.OfficialProgramName,
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
            }
        }

        /// <inheritdoc />
        public List<ProgramMappingsDto> GetProgramMappings()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var mappingEntities = (from m in context.program_name_mappings.Include(x => x.show_types)
                                       join p in context.programs.Include(x => x.genre)
                                        on m.official_program_name equals p.name
                            select new
                            {
                                mapping = m,
                                program = p
                            })
                            .ToList();

                var programMappings = mappingEntities.Select(x=> _MapToDto(x.mapping, x.program)).ToList();
                return programMappings;
            });
        }

        private ProgramMappingsDto _MapToDto(program_name_mappings program_name_mappings, program program)
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
                OfficialShowType = new ShowTypeDto
                {
                    Id = program_name_mappings.show_types.id,
                    Name = program_name_mappings.show_types.name,
                    ShowTypeSource = (ProgramSourceEnum)program_name_mappings.show_types.program_source_id
                },
                OfficialGenre = new Genre
                {
                    Id = program.genre.id,
                    Name = program.genre.name,
                    ProgramSourceId = program.genre.program_source_id
                },
                CreatedBy = program_name_mappings.created_by,
                CreatedAt = program_name_mappings.created_at,
                ModifiedBy = program_name_mappings.modified_by,
                ModifiedAt = program_name_mappings.modified_at
            };
        }

        public List<ProgramMappingsDto> GetInventoryProgramMappings()
        {
            return _InReadUncommitedTransaction(context =>
            {                
                var mappingEntities = (from m in context.program_name_mappings
                                       join p in context.programs on m.official_program_name.ToLower() equals
                                       p.name.ToLower()
                                       join g in context.genres on p.genre_id equals g.id
                                       join st in context.show_types on m.show_type_id equals st.id
                                       select new ProgramMappingsDto
                                       {
                                           Id = m.id,
                                           OriginalProgramName = m.inventory_program_name,
                                           OfficialProgramName = m.official_program_name,
                                           OfficialShowType = new ShowTypeDto
                                           {
                                               Id = m.show_type_id,
                                               Name = m.show_types.name,
                                               ShowTypeSource = (ProgramSourceEnum)m.show_types.program_source_id
                                           },
                                           OfficialGenre = new Genre
                                           {
                                               Id = p.genre.id,
                                               Name = p.genre.name,
                                               ProgramSourceId = p.genre.program_source_id
                                           },
                                           CreatedBy = m.created_by,
                                           CreatedAt = m.created_at,
                                           ModifiedBy = m.modified_by,
                                           ModifiedAt = m.modified_at
                                       })
                                      .ToList();
                return mappingEntities;


            });
        }     

        private void _MapFromDto(ProgramMappingsDto programMappingDto, program_name_mappings programMapping)
        {
            programMapping.inventory_program_name = programMappingDto.OriginalProgramName;
            programMapping.official_program_name = programMappingDto.OfficialProgramName;
            programMapping.show_type_id = programMappingDto.OfficialShowType.Id;            
        }

        /// <inheritdoc/>
        public List<ProgramMappingsDto> GetProgramsAndGeneresFromDataTable()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var mappingEntities = context.programs                   
                    .Include(x => x.show_types)   
                    .Include(x=> x.genre)
                    .ToList();

                var programMappings = mappingEntities.Select(_MapToDataTableDto).ToList();
                return programMappings;
            });
        }

        private ProgramMappingsDto _MapToDataTableDto(program programMappings)
        {
            if (programMappings == null)
            {
                return null;
            }
            return new ProgramMappingsDto
            {
                Id = programMappings.id,
                OfficialProgramName = programMappings.name,               
                OfficialShowType = new ShowTypeDto
                {
                    Id = programMappings.show_types.id,
                    Name = programMappings.show_types.name,
                    ShowTypeSource = (ProgramSourceEnum)programMappings.show_types.program_source_id,                    
                },
                OfficialGenre= new Genre
                {
                    Id =programMappings.genre.id,
                    Name=programMappings.genre.name,
                    ProgramSourceId= programMappings.genre.program_source_id,
                }
            };
        }

        /// <inheritdoc />
        public void UploadMasterProgramMappings(IEnumerable<ProgramMappingsDto> masterPrograms, string createdBy, DateTime createdAt)
        {
            var chunks = masterPrograms.GetChunks(BroadcastConstants.DefaultDatabaseQueryChunkSize);

            foreach (var chunk in chunks)
            {
                _InReadUncommitedTransaction(context =>
                {
                    var recordsToInsert = chunk
                        .Select(x =>
                        {
                            var masterProgram = new programs();
                            _MapToProgramsDto(x, masterProgram);
                            return masterProgram;
                        })
                        .ToList();

                    BulkInsert(context, recordsToInsert, propertiesToIgnore: new List<string> { "id" });
                });
            }
        }

        /// <inheritdoc />
        public void UpdateMasterProgramMappings(IEnumerable<ProgramMappingsDto> masterPrograms, string createdBy, DateTime createdAt)
        {
            var chunks = masterPrograms.GetChunks(BroadcastConstants.DefaultDatabaseQueryChunkSize);
            foreach (var chunk in chunks)
            {
                var programIdDict = chunk.ToDictionary(s => s.Id);
                var ids = programIdDict.Keys.ToList();

                _InReadUncommitedTransaction(context =>
                {
                    var found = context.programs.Where(p => ids.Contains(p.id)).ToList();
                    found.ForEach(p =>
                    {
                        p.genre_id = programIdDict[p.id].OfficialGenre.Id;
                        p.show_type_id = programIdDict[p.id].OfficialShowType.Id;
                    });
                    context.SaveChanges();
                });
            }
        }

        /// <inheritdoc />
        public List<MasterProgramsDto> GetMasterPrograms()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var masterEntities = context.programs.ToList();

                var masterPrograms = masterEntities.Select(_MapToMasterProgramsDto).ToList();
                return masterPrograms;
            });
        }

        /// <inheritdoc />
        public int CleanupOrphanedProgramNameMappings()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var toDelete = (from m in context.program_name_mappings
                                   join pt in context.programs
                                    on m.official_program_name equals pt.name into ptm
                                   from ptmResult in ptm.DefaultIfEmpty()
                                   join et in context.program_name_exceptions
                                    on m.official_program_name equals et.custom_program_name into etm
                                   from etmResult in etm.DefaultIfEmpty()
                                   where ptmResult == null
                                    && etmResult == null
                                   select m
                                  ).ToList();

                var toDeleteCount = toDelete.Count;
                if(toDeleteCount > 0)
                {
                    context.program_name_mappings.RemoveRange(toDelete);
                    context.SaveChanges();
                }
                return toDeleteCount;
            });
        }

        /// <inheritdoc />
        public int DeletePrograms()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var toDelete = context.programs.ToList();
                var toDeleteCount = toDelete.Count;
                if (toDeleteCount > 0)
                {
                    context.programs.RemoveRange(toDelete);
                    context.SaveChanges();
                }

                return toDeleteCount;
            });
        }

        private MasterProgramsDto _MapToMasterProgramsDto(program masterProgram)
        {
            if (masterProgram == null)
            {
                return null;
            }
            return new MasterProgramsDto
            {
                Name = masterProgram.name,
                ShowTypeId = masterProgram.show_type_id,
                GenreId = masterProgram.genre_id
            };
        }
        private void _MapToProgramsDto(ProgramMappingsDto programMappingDto, programs masterProgram)
        {
            masterProgram.genre_id = programMappingDto.OfficialGenre.Id;
            masterProgram.name = programMappingDto.OfficialProgramName;
            masterProgram.show_type_id = programMappingDto.OfficialShowType.Id;
        }
    }
}
