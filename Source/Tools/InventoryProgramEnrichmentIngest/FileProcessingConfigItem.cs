namespace InventoryProgramEnrichmentIngest
{
    public class FileProcessingConfigItem
    {
        public string Name { get; set; }
        public string TargetUrl { get; set; }
        public string SourceDirectoryPath { get; set; }
        public bool IsEnabled { get; set; }
    }
}