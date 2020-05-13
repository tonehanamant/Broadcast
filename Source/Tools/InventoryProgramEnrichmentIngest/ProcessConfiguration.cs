using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace InventoryProgramEnrichmentIngest
{
    public class ProcessConfiguration : ProcessingBaseClass
    {
        private const string CONFIG_KEY_LOCAL_PATH_ROOT = "LocalPathRoot";
        private const string CONFIG_KEY_TEST_FILE_SEARCH_PATTERN = "FileSearchPattern";
        private const string CONFIG_KEY_PROCESSING_CONFIG_FILE_NAME = "ProcessingConfigFileName";
        private const string CONFIG_KEY_SHOULD_PAUSE_ON_DONE = "ShouldPauseOnDone";
        private const string CONFIG_KEY_PROCESSING_DAY_OFFSET = "ProcessingDayOffset";
        private const string CONFIG_KEY_FILE_UPLOAD_TIMEOUT_MINUTES = "FileUploadTimeoutMinutes";

        public string LocalPathRoot { get; set; }
        public string FileSearchPattern { get; set; }
        public string ProcessingConfigFileName { get; set; }
        public bool ShouldPauseOnDone { get; set; }
        public int ProcessingDayOffset { get; set; }
        public int FileUploadTimeoutMinutes { get; set; }

        public List<FileProcessingConfigItem> FileProcessingConfigItems { get; set; }
        
        public void Load()
        {
            _LogInfo("Loading the configuration...");
            _LoadAppConfig();
            _LoadFileProcessingConfigItems();
        }

        private void _LoadAppConfig()
        {
            LocalPathRoot = ConfigurationManager.AppSettings[CONFIG_KEY_LOCAL_PATH_ROOT];
            FileSearchPattern = ConfigurationManager.AppSettings[CONFIG_KEY_TEST_FILE_SEARCH_PATTERN];
            ShouldPauseOnDone = ConfigurationManager.AppSettings[CONFIG_KEY_SHOULD_PAUSE_ON_DONE]?.ToUpper() == "TRUE";

            const int defaultProcessingDayOffset = -1;
            ProcessingDayOffset = _GetIntFromConfig(CONFIG_KEY_PROCESSING_DAY_OFFSET, defaultProcessingDayOffset);

            const int defaultFileUploadTimeoutMinutes = 120;
            FileUploadTimeoutMinutes = _GetIntFromConfig(CONFIG_KEY_FILE_UPLOAD_TIMEOUT_MINUTES, defaultFileUploadTimeoutMinutes);

            ProcessingConfigFileName = ConfigurationManager.AppSettings[CONFIG_KEY_PROCESSING_CONFIG_FILE_NAME];
        }

        private int _GetIntFromConfig(string key, int defaultValue)
        {
            if (ConfigurationManager.AppSettings[key] != null)
            {
                if (int.TryParse(ConfigurationManager.AppSettings[key], out var result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        private void _LoadFileProcessingConfigItems()
        {
            List<FileProcessingConfigItem> configItems;
            using (StreamReader reader = new StreamReader(ProcessingConfigFileName))
            {
                var json = reader.ReadToEnd();
                configItems = JsonConvert.DeserializeObject<List<FileProcessingConfigItem>>(json);
            }

            FileProcessingConfigItems = configItems;
        }
    }
}