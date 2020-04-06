# This script chops any csv file in the script file that crosses a threshold into many smaller files.
# Existing files that match the output file pattern will be overwritten
#
# This script assumes 
#   The first column is "inventory_id"
#   The inventory_ids should not be split over file boundaries.
#
# .\ChopCsvFiles.ps1
#
Param(	
    [string] $inputFileName = 'results.csv',
    [float] $thresholdGb = 0.25
)

$ErrorActionPreference = "Stop"

#region Include required files
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

function GetChunkFileName
{
    param
    (
        [int] $index,
        [string] $nameSeed
    )

    $qualifiedIndex = $index.ToString().PadLeft(2, '0')
    return $("{0}_{1}" -f $qualifiedIndex, $nameSeed)    
}

Write-OutputWithTime "ChopCsvFiles starting..."
Write-OutputWithTime "inputFileName = '$inputFileName'"
Write-OutputWithTime "thresholdGb = $thresholdGb"
Write-OutputWithTime " "

if ($(Test-Path $inputFileName) -eq $false)
{
    Write-OutputWithTime "Error : Input file not found '$inputFileName'"
    Write-OutputWithTime "Script ending"
    EXIT 1
}

$chunkNameSeed = "chopped_{0}" -f $inputFileName
$chunkIndex = 1;
$currentChunkFileName = GetChunkFileName -index $chunkIndex -nameSeed $chunkNameSeed

$existingResultFileCount = $(Get-ChildItem . -Filter $("*$chunkNameSeed")).Count
if ($existingResultFileCount -gt 0)
{
    Write-OutputWithTime "Cleaning up prior runs..."
    Write-OutputWithTime "Deleting $existingResultFileCount existing result files..."
    Get-ChildItem . -Filter $("*$chunkNameSeed") | Remove-Item
}

# not exact, but good enough
$constant_estimatedLinesPerGb = 5040000

$thresholdLines = $constant_estimatedLinesPerGb * $thresholdGb
Write-OutputWithTime "thresholdLines = $thresholdLines"

Write-OutputWithTime "Measuring input file..."
$lineCount = $(Get-Content "$inputFileName" | Measure-Object -Line).Lines
Write-OutputWithTime "Input file contains $lineCount lines"

if ($lineCount -le $thresholdLines)
{
    Write-OutputWithTime "Creating chunk 1 of 1."
    Copy-Item $inputFileName $currentChunkFileName
    Write-OutputWithTime "All done."
    EXIT 0
}

$chunkCount = [Math]::Ceiling($lineCount / $thresholdLines)
Write-OutputWithTime "Chopping into $chunkCount chunks"

Write-OutputWithTime "Beginning file chunking..."
$chunkFiles = @()
$startrow = 0
while ($startrow -lt $lineCount)
{
    Write-OutputWithTime "Starting on chunk $chunkIndex of $chunkCount"

    Import-CSV $inputFileName | select-object -skip $startrow -First $thresholdLines | Export-CSV $currentChunkFileName -NoTypeInformation -Append
    $chunkFiles += $currentChunkFileName
        
    $startrow += $thresholdLines
    $chunkIndex++
    $currentChunkFileName = GetChunkFileName -index $chunkIndex -nameSeed $chunkNameSeed
}

Write-OutputWithTime "Completed file chunking."
Write-OutputWithTime "Beginning inventory id unification..."

$lastChunkFile = $chunkFiles[$chunkFiles.Length - 1]

$nextChunkIndex = 0
foreach ($chunkFile in $chunkFiles)
{    
    if ($chunkfile -eq $lastChunkFile)
    {
        break
    }

    Write-OutputWithTime "Starting unification on file '$chunkFile'"
    
    # get the last ID of the file
    $lastLineOfFile = Get-Content -tail 1 $chunkfile
    $lastInventoryId = $($lastLineOfFile.Substring(0, $lastLineOfFile.IndexOf(",")) -replace '"', '')
    
    $nextChunkIndex++    
    $nextFile = $chunkFiles[$nextChunkIndex]    

    # we're looking for the line number we care about
    $lineBreak = 0
    Import-CSV $nextFile | ForEach-Object {
        $tempInventoryId = $_.inventory_id
        if ($tempInventoryId -eq $lastInventoryId)
        {
            $lineBreak++;
        }
        else 
        {
            return
        }
    }

    Write-OutputWithTime "Moveing $lineBreak lines from file '$nextFile' to '$chunkFile'."
    
    # now move the first lines over 
    if ($lineBreak -gt 0)
    {
        $tempFileName = "temp_" + $nextFile
        
        # consolidate to the current file
        Import-CSV $nextFile | select-object -First $lineBreak | Export-Csv $chunkFile -NoTypeInformation -Append
        # remove from the next file
        Import-CSV $nextFile | Select-Object -Skip $lineBreak | Export-Csv $tempFileName -NoTypeInformation

        Remove-Item $nextFile
        Move-Item $tempFileName $nextFile
    }        
}

Write-OutputWithTime "Completed file chunking."
Write-OutputWithTime "Cleaning up empty files...."

[array]::reverse($chunkFiles)
foreach ($chunkFile in $chunkFiles)
{
    $fileLength = $(Get-Item $chunkFile).length
    if ($fileLength -eq 0kb)
    {
        Write-OutputWithTime "Removing empty file '$chunkFile'"        
        Remove-Item $chunkFile
    }
}

Write-OutputWithTime "All done."
