Usage :
$ cd 'C:\Program Files\Cadent\Broadcast\InventoryProgramEnrichmentIngest'
$ InventoryProgramEnrichmentIngest.exe

A log will be created : 
	C:\Program Files\Cadent\Broadcast\InventoryProgramEnrichmentIngest\log.txt

Install :
	Copy the files from the bin directory into :
		C:\Program Files\Cadent\Broadcast\InventoryProgramEnrichmentIngest

	Enable \ Disable the FileProcessingConfig items appropriately in file 
		FileProcessingConfig.json

Configuration : 
	App.Config :
		- LocalPathRoot : The local working directory where files are copied to for processing.
		- FileSearchPattern : Process the files in the source directory that match this pattern
		- ProcessingConfigFileName : The file in the exe directory that has the configurations (a.k.a : environments) to execute on.
		- ProcessingDayOffset : The day offset for processing.  Exe : -1 means process files for yesterday.
		- FileUploadTimeoutMinutes : The timeout for the file upload operation.
		- ShouldPauseOnDone : Should the tool pause when it completes?
			- true for debug so the F5 keeps the window open
			- false when deployed

	ProcessingConfigFile : These are the config items the tool will iterate over
		- Name : Name for the config item.  
		- SourceDirectoryPath : The path to the source files to process
		- TargetUrl : The processing Api endpoint url
		- IsEnabled : When true this config item will be processed.
