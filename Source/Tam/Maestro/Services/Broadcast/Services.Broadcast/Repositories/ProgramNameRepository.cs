using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.ProgramMapping;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IProgramNameRepository : IDataRepository
    {
        /// <summary>
        /// Get list of programs from program_names tables which matches search criteria.
        /// Return list of programs from program_names 
        /// </summary>
        /// <param name="programSearchString"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns>List of Programs </returns>
        List<LookupDto> FindPrograms(string programSearchString, int start, int limit);
        /// <summary>
        /// Return list of official_program_name from program_name_mappings 
        /// </summary>
        /// <param name="programSearchString"></param>
        /// <returns>List of Program Name mappings</returns>
        List<ProgramNameMappingDto> FindProgramFromMapping(string programSearchString);
        /// <summary>
        /// Return list of custom_program_name from program_name_exceptions 
        /// </summary>
        /// <param name="programSearchString"></param>
        /// <returns>List of Program Exceptions</returns>
        List<ProgramNameExceptionDto> FindProgramFromExceptions(string programSearchString);
    }
    public class ProgramNameRepository : BroadcastRepositoryBase, IProgramNameRepository
    {
        public ProgramNameRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        /// <inheritdoc />
        public List<LookupDto> FindPrograms(string programSearchString, int start, int limit)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.programs.Where(p => p.name.ToLower().Contains(programSearchString.ToLower()))
                    .OrderBy(p => p.name)
                    .Skip(start - 1).Take(limit)
                    .Select(
                        p => new LookupDto()
                        {
                            Display = p.name,
                            Id = p.id
                        }).ToList();
                });
        }

        /// <inheritdoc />
        public List<ProgramNameExceptionDto> FindProgramFromExceptions(string programSearchString)
        {
	        return _InReadUncommitedTransaction(
		        context =>
		        {
			        return context.program_name_exceptions
				        .Where(p => p.custom_program_name.ToLower().Contains(programSearchString.ToLower()))
				        .OrderBy(p => p.custom_program_name)
				        .Select(
					        p => new ProgramNameExceptionDto
					        {
						        CustomProgramName = p.custom_program_name,
						        Id = p.id,
						        GenreId = p.genre_id,
						        ShowTypeId = p.show_type_id
					        }).ToList();
		        });
        }

        /// <inheritdoc />
        public List<ProgramNameMappingDto> FindProgramFromMapping(string programSearchString)
        {
	        return _InReadUncommitedTransaction(
		        context =>
		        {
                    var result = (from p in context.program_name_mappings
                                  join pg in context.programs on p.official_program_name equals pg.name
                                  where p.official_program_name.ToLower().Contains(programSearchString.ToLower()                                 
                                  )                        
                                  select new ProgramNameMappingDto
                                  {
                                      OfficialProgramName = p.official_program_name,
                                      GenreId = pg.genre_id
                                  }).OrderBy(p=>p.OfficialProgramName).ToList();
                    return result;       
		        });
        }
    }
}
