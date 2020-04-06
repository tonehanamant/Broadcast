# This script combines all the csv files in the script directory.
# .\CombineCsvFiles.ps1
#
Param(
	[string] $outputFileName = "output.csv"
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

Write-OutputWithTime "CombineCsvFiles starting..."
Write-OutputWithTime "Output to file '$outputFileName'"

if ($(Test-Path $outputFileName))
{
    Write-OutputWithTime "Deleting found output file : '$outputFileName'"
    Remove-Item $outputFileName
}

$excluded = @($outputFileName)
$files = Get-ChildItem . -Filter *.csv -Name -Exclude $excluded
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
    Import-Csv $file | Export-Csv $outputFileName -NoTypeInformation -Append

    $currentLinesCopied = $($lineCount - 1)  # subtract 1 for the header
    $totalLinesCopiedCount += $currentLinesCopied

    Write-OutputWithTime "Copied $currentLinesCopied lines"
    Write-OutputWithTime "Total Copied $totalLinesCopiedCount lines"

    Write-OutputWithTime " "
}

$outputLineCount = $(Get-Content "$outputFileName" | Measure-Object -Line).Lines

Write-OutputWithTime "Finished processing $filesCount files."
Write-OutputWithTime "Outputfile contains $outputLineCount"

Write-OutputWithTime "All done."
