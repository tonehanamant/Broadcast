<#

This script expects this source directory structure:
<root>
<root>\DEV
<root>\QA
<root>\UAT
<root>\PROD

This script expects to be in the <root> directory.

dateString : The date formated yyyyMMdd used to derive the target directories.

If the dateString is not provided then it will use yesterday's date.
#>

Param(
	[string] $dateString = $null
)

$sourceRoot = '.\'
$targetRoot = '\\cadfs11.crossmw.com\Inventory Management UI\ProgramGuide\ExportsToProcess'
$processedDirectory = "Processed"

if ($dateString -eq $null)
{
    $dateString = $([DateTime]::Now).AddDays(-1).ToString("yyyyMMdd")
}

$ErrorActionPreference = "Stop"

$startingLoc = Get-Location
Set-Location $PSScriptRoot

function Write-OutputWithTime
{
    param
    (
        [Parameter(Mandatory=$true, Position=1, ValueFromPipeline=$true)]
        [string]$msg,
        [Parameter(Position=2)]
        [string]$file = $null,
		[Parameter(Position=3)]
		[switch]$IsError
    )
    $t = Get-Date -format "yyyy-MM-dd HH:mm:ss.fff"

	if ($IsError.IsPresent)
	{
		Write-Error "$t : $msg" 
	}
	else 
	{
		Write-Host "$t : $msg" -ForegroundColor Yellow
	}
    
    if ([string]::IsNullOrEmpty($file) -and -not ([string]::IsNullOrEmpty($LogFileName)))
    {
        $file = $LogFileName
    }

    if (-not ([string]::IsNullOrEmpty($file)))
    {
        Add-Content $file "$t : $msg"
    }
}

function Create-DirectoryItem  
{
    param
    (
        [string]$name
    )

    $source = Join-Path $sourceRoot $name
    $target = [IO.Path]::Combine($targetRoot, $dateString, $name, $processedDirectory)

    $props = @{ name=$name;source=$source;target=$target }
    New-Object PSObject -Property $props
}

function Create-FileToMoveItem 
{
    param
    (
        [string]$fileName,
        $directoryItem
    )

    $sourcePath = Join-Path $directoryItem.source $fileName
    $targetPath = Join-Path $directoryItem.Target $fileName

    $props = @{ fileName=$fileName;sourcePath=$sourcePath;targetPath=$targetPath }
    New-Object PSObject -Property $props
}

function Move-File
{
    param
    (
        $fileToMoveItem
    )

    $fileName = $fileToMoveItem.fileName
    $sourcePath = $fileToMoveItem.sourcePath
    $targetPath = $fileToMoveItem.targetPath

    Write-OutputWithTime " "
    Write-OutputWithTime "Starting to copy file"
    Write-OutputWithTime "Name : '$fileName'"
    Write-OutputWithTime "From : '$sourcePath'"
    Write-OutputWithTime "To : '$targetPath'"

    Copy-Item -Path $sourcePath -Destination $targetPath

    Write-OutputWithTime "Copy file complete."    
}

Write-OutputWithTime "CopyProcessedFilesToShare starting..."


$directories = @(
    $(Create-DirectoryItem -name "DEV"),
    $(Create-DirectoryItem -name "QA"),
    $(Create-DirectoryItem -name "UAT"),
    $(Create-DirectoryItem -name "PROD")
)

$filePathsToMove = @()
foreach ($directoryItem in $directories)
{
    $foundFileNames = $(Get-ChildItem $directoryItem.source -Filter *.csv | Sort-Object LastWriteTime -Descending).Name
    $foundFiles = $foundFileNames | foreach { Create-FileToMoveItem -fileName $_ -directoryItem $directoryItem }
    $filePathsToMove += $foundFiles
}

$fileCount = $filePathsToMove.Count
Write-OutputWithTime "Found $fileCount files to move."

$filePathsToMove | foreach { Move-File $_ }

Write-OutputWithTime " "
Write-OutputWithTime "All done."
