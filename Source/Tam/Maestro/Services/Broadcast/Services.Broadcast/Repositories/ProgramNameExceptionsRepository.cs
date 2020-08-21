using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.ProgramMapping;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
	public interface IProgramNameExceptionsRepository : IDataRepository
	{
		List<ProgramNameExceptionDto> GetProgramExceptions();
	}

	public class ProgramNameExceptionsRepository : BroadcastRepositoryBase, IProgramNameExceptionsRepository
	{
		public ProgramNameExceptionsRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
			ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient)
			: base(pBroadcastContextFactory, pTransactionHelper, configurationWebApiClient)
		{
		}

		/// <inheritdoc />
		public List<ProgramNameExceptionDto> GetProgramExceptions()
		{
			const ProgramSourceEnum programSource = ProgramSourceEnum.Maestro;

			return _InReadUncommitedTransaction(
				context =>
				{
					var programNameExceptions = context.program_name_exceptions
						.Include(p => p.genre).Where(p => p.genre.program_source_id.Equals((int)programSource))
						.Include(p => p.show_types)
						.OrderBy(p => p.custom_program_name)
						.Select(
							p => new ProgramNameExceptionDto
							{
								CustomProgramName = p.custom_program_name,
								Id = p.id,
								GenreId = p.genre_id,
								GenreName = p.genre.name,
								ShowTypeId = p.show_type_id,
								ShowTypeName = p.show_types.name
							}).ToList();
					return programNameExceptions;
				});
		}

	}
}