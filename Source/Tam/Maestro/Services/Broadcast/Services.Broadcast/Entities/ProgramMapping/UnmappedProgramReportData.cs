using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping.Entities;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.ProgramMapping
{
    public class UnmappedProgramReportData
	{
		public UnmappedProgramReportData(string fileName, List<UnmappedProgram> programs)
		{
			UnmappedPrograms = programs;
			ExportFileName = fileName;
		}

		public List<UnmappedProgram> UnmappedPrograms { get; set; }

		public string ExportFileName { get; set; }
	}
}