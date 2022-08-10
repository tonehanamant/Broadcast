using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class MigrateFilesToAttachmentServiceResult
	{
		public bool ConsolidationEnabled { get; set; }
		public bool MigrationEnabled { get; set; }
		public int TotalFileHistoryCount { get; set; }
		public int TotalErrorFileHistoryCount { get; set; }
		public List<string> MigratedFileNames { get; set; } = new List<string>();
		public List<string> FileNotFoundFileNames { get; set; } = new List<string>();
		public List<string> AlreadyMigratedFileNames { get; set; } = new List<string>();
		public List<string> FailedToSaveToAttachmentService { get; set; } = new List<string>();

		public string ResultsMessage
		{
			get
			{
				var message =
					$"ConsolidationEnabled: {ConsolidationEnabled}; " +
					$"MigrationEnabled: {MigrationEnabled}; " +
					$"TotalFileHistoryCount: {TotalFileHistoryCount}; " +
					$"TotalErrorFileHistoryCount: {TotalErrorFileHistoryCount}; " +
					$"MigratedFileNamesCount: {MigratedFileNames.Count}; " +
					$"FileNotFoundFileNamesCount: {FileNotFoundFileNames.Count}; " +
					$"AlreadyMigratedFileNamesCount: {AlreadyMigratedFileNames.Count}; " +
					$"FailedToSaveToAttachmentServiceCount: {FailedToSaveToAttachmentService.Count};";
				return message;
			}
		}
	}
}
