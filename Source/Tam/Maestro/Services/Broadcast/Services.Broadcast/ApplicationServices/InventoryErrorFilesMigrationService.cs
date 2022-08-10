using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.IO;
using System.Linq;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryErrorFilesMigrationService : IApplicationService
	{
		MigrateFilesToAttachmentServiceResult MigrateFilesToAttachmentService(string username);
	}

	public class InventoryErrorFilesMigrationService : BroadcastBaseClass, IInventoryErrorFilesMigrationService
	{
		private readonly ISharedFolderService _SharedFolderService;
		private readonly IFileService _FileService;
		private readonly IInventoryFileRepository _InventoryFileRepository;
		private readonly IInventoryRepository _InventoryRepository;
		private readonly IDateTimeEngine _DateTimeEngine;

		public InventoryErrorFilesMigrationService(
			IDataRepositoryFactory broadcastDataRepositoryFactory,
			ISharedFolderService sharedFolderService,
			IFileService fileService,
			IDateTimeEngine dateTimeEngine,
			IFeatureToggleHelper featureToggleHelper, 
			IConfigurationSettingsHelper configurationSettingsHelper)
			: base(featureToggleHelper, configurationSettingsHelper)
        {
			_InventoryFileRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryFileRepository>();
			_InventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();

			_FileService = fileService;
			_SharedFolderService = sharedFolderService;

			_DateTimeEngine = dateTimeEngine;
		}

		protected string _GetFileSystemInventoryUploadErrorsFolder()
		{
			var inventoryUploadsFolder = Path.Combine(_GetBroadcastAppFolder(), BroadcastConstants.FolderNames.INVENTORY_UPLOAD);
			var inventoryUploadsErrorFolder = Path.Combine(inventoryUploadsFolder, BroadcastConstants.FolderNames.INVENTORY_UPLOAD_ERRORS);
			return inventoryUploadsErrorFolder;
		}

		public MigrateFilesToAttachmentServiceResult MigrateFilesToAttachmentService(string username)
        {
			_LogInfo("Beginning the process to migrate inventory error files to the attachment service.");

			var consolidationIsEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION);
			var migrationIsEnabled = _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.ENABLE_ATTACHMENT_MICRO_SERVICE);

			var result = new MigrateFilesToAttachmentServiceResult
			{
				ConsolidationEnabled = consolidationIsEnabled,
				MigrationEnabled = migrationIsEnabled
			};

			if (!consolidationIsEnabled || !migrationIsEnabled)
            {
				var errorMessage = $"Stopping because the feature is disabled. All toggles must be enabled :" +
					$"Toggle '{FeatureToggles.ENABLE_SHARED_FILE_SERVICE_CONSOLIDATION}' = '{consolidationIsEnabled}'; " +
					$"Toggle '{FeatureToggles.ENABLE_ATTACHMENT_MICRO_SERVICE}' = '{migrationIsEnabled}';";
				
				_LogError(errorMessage);
				return result;
            }

			var openMarketInventorySourceId = (int)InventorySourceEnum.OpenMarket;
			var allFiles = _InventoryRepository.GetInventoryUploadHistoryForInventorySource(openMarketInventorySourceId, startDate:null, endDate: null);
			var errorFiles = allFiles.Where(f => f.FileLoadStatus == FileStatusEnum.Failed).ToList();

			_LogInfo($"Beginning to work on {errorFiles.Count} error files of {allFiles.Count} total files.");

			result.TotalFileHistoryCount = allFiles.Count;
			result.TotalErrorFileHistoryCount = errorFiles.Count;

			foreach (var errorFileId in errorFiles.Select(s => s.FileId))
            {
				var inventoryFile = _InventoryFileRepository.GetInventoryFileById(errorFileId);

				if (inventoryFile.ErrorFileSharedFolderFileId.HasValue)
                {
					try
                    {
						var sharedFolderFile = _SharedFolderService.GetFileInfo(inventoryFile.ErrorFileSharedFolderFileId.Value);
						if (sharedFolderFile.AttachmentId.HasValue)
                        {
							_LogWarning($"File '{sharedFolderFile.FileNameWithExtension}' with FileId '{inventoryFile.Id}' has already been migrated " +
                                $"and exists as AttachmentId '{sharedFolderFile.AttachmentId}'.  Skipping migration of this file.");
							result.AlreadyMigratedFileNames.Add(inventoryFile.FileName);
							continue;
                        }
					}
					catch (Exception ex)
                    {
						_LogWarning($"Error caught retrieving file info from the SharedFoldersService.  Unable to migrate file.  " +
                            $"FileName: '{inventoryFile.FileName}'; FileId: '{inventoryFile.Id}'; SharedFolderFileId: '{inventoryFile.ErrorFileSharedFolderFileId.Value}'; " +
                            $"Error Message : '{ex.Message}';");

						result.FileNotFoundFileNames.Add(inventoryFile.FileName);
						continue;
                    }
                }

				var folderPath = _GetFileSystemInventoryUploadErrorsFolder();
				var fileName = $"{inventoryFile.Id}_{inventoryFile.FileName}.txt";
				var filePath = Path.Combine(folderPath, fileName);
				Stream fileContent = null;
				try
				{
					var fileStream = _FileService.GetFileStream(filePath);
					fileContent = fileStream;
				}
				catch (Exception ex)
				{
					_LogWarning($"Error caught retrieving file from network location using the FileService.  Unable to migrate file.  " +
						$"FileName: '{inventoryFile.FileName}'; FileId: '{inventoryFile.Id}'; " +
						$"Error Message : '{ex.Message}';");

					result.FileNotFoundFileNames.Add(inventoryFile.FileName);
					continue;
				}

				_LogInfo($"Saving file to the attachment service. FileName: '{inventoryFile.FileName}'; FileId: '{inventoryFile.Id}'; ");

				// we will just move the files and create a new shared_folder_files record, orphaning the old.
				var newFile = new SharedFolderFile
				{
					FolderPath = folderPath,
					FileNameWithExtension = fileName,
					FileMediaType = "application/xml",
					FileUsage = SharedFolderFileUsage.InventoryErrorFile,
					FileContent = fileContent,
					CreatedDate = _DateTimeEngine.GetCurrentMoment(),
					CreatedBy = $"InventoryErrorFileMigration : {username}"
				};

				try
                {
					var newSharedFolderFileId = _SharedFolderService.SaveFile(newFile);
					_InventoryFileRepository.SaveErrorFileId(inventoryFile.Id, newSharedFolderFileId);
					result.MigratedFileNames.Add(fileName);

					_LogInfo($"Finished saving file to the attachment service. FileName: '{inventoryFile.FileName}'; FileId: '{inventoryFile.Id}'; ");
				}
				catch (Exception ex)
                {
					_LogError($"Error caught attempting to save a file to the attachment service.  " +
                        $"FileName: '{inventoryFile.FileName}'; FileId: '{inventoryFile.Id}';", ex);

					result.FailedToSaveToAttachmentService.Add(fileName);
				}
            }

			_LogInfo($"Completed the process to migrate inventory error files to the attachment service. Results : {result.ResultsMessage};");

			return result;
        }
	}
}