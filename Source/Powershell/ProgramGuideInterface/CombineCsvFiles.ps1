﻿# This script combines all the csv files in the script directory.
# .\CombineCsvFiles.ps1
#
Param(
	[string] $outputFileName = "output.csv"
)

$ErrorActionPreference = "Stop"

$tempFileName = 'temp_' + $outputFileName

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

Write-OutputWithTime "CombineCsvFiles starting..."
Write-OutputWithTime "Output to file '$outputFileName'"

if ($(Test-Path $tempFileName))
{
    Write-OutputWithTime "Deleting found temp file : '$tempFileName'"
    Remove-Item $tempFileName
}

if ($(Test-Path $outputFileName))
{
    Write-OutputWithTime "Deleting found output file : '$outputFileName'"
    Remove-Item $outputFileName
}

$excluded = @($outputFileName)
$foundFiles = $(Get-ChildItem . -Filter *.csv | Sort-Object LastWriteTime -Descending).Name

$files = @()
$fileNameStubs = @()
$skippedFiles = @()
foreach ($file in $foundFiles)
{
    # compare up to the file id to attempting to weed out dups
    # since this is ordered by time descending we will keep the latest one.
    $fileNameStubLength = 33
    if ($file.StartsWith("ProgramGuideExport_FILE_")) 
    { 
        $fileNameStubLength = 29 
    } 

    $fileNameStub = $file.Substring(0, $fileNameStubLength)
    if ($fileNameStubs.Contains($fileNameStub))
    {
        Write-OutputWithTime "Skipping file '$file' as a duplicate."
        $skippedFiles += $file
        continue;
    }
    $files += $file
    $fileNameStubs += $fileNameStub
}

$filesCount = $files.Count

Write-OutputWithTime "Found $filesCount files to process."
Write-OutputWithTime " "

$fileIndex = 0
$totalLinesCopiedCount = 1 # add 1 for the header
foreach ($file in $files)
{    
    $fileIndex++
    Write-OutputWithTime "Starting on file $fileIndex of $filesCount "
    Write-OutputWithTime "'$file.Name'"
    $lineCount = $(Get-Content "$file" | Measure-Object -Line).Lines
    Write-OutputWithTime "Line Count : $lineCount"

	# do this in one line to keep the memory usage down.
    Import-Csv $file | Export-Csv $tempFileName -NoTypeInformation -Append

    $currentLinesCopied = $($lineCount - 1)  # subtract 1 for the header
    $totalLinesCopiedCount += $currentLinesCopied

    Write-OutputWithTime "Copied $currentLinesCopied lines"
    Write-OutputWithTime "Total Copied $totalLinesCopiedCount lines"

    Write-OutputWithTime " "
}

if ($filesCount -gt 1)
{    
    Write-OutputWithTime "Beginning de-duplication..."
    
    # Dedup for 
    # 1) duplicate space\time 
    # 2) duplicate inventory record
    # be sure to keep "the latest"
    Import-Csv $tempFileName | sort station_call_letters,affiliation,start_date,end_date,daypart_text -Unique | sort inventory_id,inventory_week_id,inventory_daypart_id -Unique | Export-Csv $outputFileName -NoTypeInformation -Append
    
    Remove-Item $tempFileName
    Write-OutputWithTime "De-deplication completed."
    Write-OutputWithTime " "
}
else 
{
    Rename-Item -Path $tempFileName -NewName $outputFileName
}

$outputLineCount = $(Get-Content "$outputFileName" | Measure-Object -Line).Lines

Write-OutputWithTime "Finished processing $filesCount files."
Write-OutputWithTime "Outputfile contains $outputLineCount lines"

Write-OutputWithTime "All done."
