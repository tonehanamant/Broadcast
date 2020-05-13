using System;
using System.Diagnostics;
using System.IO;

namespace InventoryProgramEnrichmentIngest
{
    public interface IProcessEngine
    {
        void PerformFileProcessing(ProcessConfiguration processConfig);
    }

    public class ProcessEngine : ProcessingBaseClass, IProcessEngine
    {
        public void PerformFileProcessing(ProcessConfiguration processConfig)
        {
            foreach (var configItem in processConfig.FileProcessingConfigItems)
            {
                if (configItem.IsEnabled == false)
                {
                    _LogInfo($"Skipping disabled configItem '{configItem.Name}'");
                    continue;
                }

                _PerformFileProcessing(configItem, processConfig);
            }
        }

        private void _PerformFileProcessing(FileProcessingConfigItem configItem, ProcessConfiguration processConfig)
        {
            var fileProcessingEngine = new FileProcessingEngine();
            var localPath = Path.Combine(processConfig.LocalPathRoot, configItem.Name);

            // resolve the source path
            var sourceDirectoryPath = _ResolveSourcePath(configItem.SourceDirectoryPath, processConfig.ProcessingDayOffset);

            _LogInfo("");
            _LogInfo("*** Environment Processing Start ***");
            _LogInfo($"Beginning to process source directory '{sourceDirectoryPath}'");

            try
            {
                // 1) copy from share to local (clean first)
                fileProcessingEngine.CleanupLocal(localPath);
                fileProcessingEngine.BringFilesLocal(sourceDirectoryPath, localPath, processConfig.FileSearchPattern);

                // 2) process from local
                var files = fileProcessingEngine.GetFilesToProcess(localPath, processConfig.FileSearchPattern);
                _LogInfo($"Beginning to process {files.Count} files.");
                var fileIndex = 0;
                foreach (var filePath in files)
                {
                    fileIndex++;
                    _LogInfo($"Processing file {fileIndex} of {files.Count}");
                    fileProcessingEngine.ProcessFile(filePath, configItem.TargetUrl, processConfig.FileUploadTimeoutMinutes);
                    _LogInfo("File processed");
                }
                // 3) delete from local
                fileProcessingEngine.CleanupLocal(localPath);
            }
            catch (Exception ex)
            {
                _LogError($"Error caught processing directory '{sourceDirectoryPath}'", ex);
            }
            _LogInfo("*** Environment Processing End ***");
        }

        private string _ResolveSourcePath(string originalSourcePath, int dayOffset)
        {
            const string dateStampToken = "[yyyyMMdd]";
            var result = originalSourcePath;

            if (result.Contains(dateStampToken))
            {
                var dateStampString = DateTime.Now.AddDays(dayOffset).ToString("yyyyMMdd");
                var temp = result.Replace(dateStampToken, dateStampString);
                result = temp;
            }

            return result;
        }
    }
}