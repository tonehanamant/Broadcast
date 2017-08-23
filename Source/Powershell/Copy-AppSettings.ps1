Param
  (
    [Parameter(Mandatory=$true)]
    [string]$sourceFile,
    [Parameter(Mandatory=$true)]
    [string]$targetFile,
    [Parameter(Mandatory=$true)]
    [string]$workingDirectory
  ) 
function Copy-AppSettings
{
  
  Write-Verbose "Executing Copy-AppSettings for $sourceFile to $targetFile in $workingDirectory" -Verbose
  Set-ExecutionPolicy Unrestricted
  Copy-Item -Path "$workingDirectory\$sourceFile" -Destination "$workingDirectory\$targetFile" -Force -Verbose
}

Copy-AppSettings