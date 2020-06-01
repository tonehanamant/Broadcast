using System.Collections.Generic;

namespace Services.Broadcast.Entities.ProgramMapping
{
	public class UnMappedProgramNameReportData
	{
		public List<string> ProgramNames { get; set; }
		public string ExportFileName { get; set; }
	}
}