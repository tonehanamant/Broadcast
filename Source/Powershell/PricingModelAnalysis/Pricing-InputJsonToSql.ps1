# This script takes json input sent to the Pricing Model Allocations Endpoint,
# converts it to tables and saves it to file.  Load that sql file for easier 
# analysis of what was sent tothe model.
# 
# The input file should contain the json that is output from the BE Swagger Endpoint 
# POST /api/v1/PricingService/PricingApiRequestPrograms
# Capture that output and save it to file, then feed that file to this script.
#
# Usage :
#     # Example : The input file is named 'InventorySentToModel.json'.
#     .\Pricing-InputJsonToSql.ps1 -inputFileName InventorySentToModel.json
# 
#     # Output : InventorySentToModel.sql
#     # Next : Load the sql file into the target environment database.
#     #     It will create temp tables populated with the file data.
#     #     Join against the relevant tables for the full picture.
# 
Param(	
    [string] $inputFileName
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

function GetSql-Top
{
    $sql = @'
IF OBJECT_ID('tempdb..#contracts') IS NOT NULL
BEGIN 
	DROP TABLE #contracts
END 

IF OBJECT_ID('tempdb..#inventory') IS NOT NULL
BEGIN 
	DROP TABLE #inventory
END 

GO

CREATE TABLE #contracts
(
	week_id INT,
	impression_goal FLOAT,
	cpm_goal FLOAT,
	market_coverage_goal FLOAT	
)

CREATE TABLE #inventory
(
	id INT,
	week_id INT,
	daypart_id INT,
	impressions FLOAT,
	cost FLOAT,
	station_id INT,
	market_code INT,
	percentage_of_us FLOAT	
)

GO
'@

    return $sql

}

function GetSql-Contract($contract)
{
    $sql = 'INSERT INTO #contracts (week_id, impression_goal, cpm_goal, market_coverage_goal) VALUES ('
    $sql += $("{0}," -f $contract.week_id)
    $sql += $("{0}," -f $contract.impression_goal)
    $sql += $("{0}," -f $contract.cpm_goal)
    $sql += $("{0}" -f $contract.market_coverage_goal)
    $sql += ");"    
    return $sql
}

function GetSql-Inventory($inventory)
{
    $sql = 'INSERT INTO #inventory (id, week_id, daypart_id, impressions, cost, station_id, market_code, percentage_of_us) VALUES ('
    $sql += $("{0}," -f $inventory.id)
    $sql += $("{0}," -f $inventory.week_id)
    $sql += $("{0}," -f $inventory.daypart_id)
    $sql += $("{0}," -f $inventory.impressions)
    $sql += $("{0}," -f $inventory.cost)
    $sql += $("{0}," -f $inventory.station_id)
    $sql += $("{0}," -f $inventory.market_code)
    $sql += $("{0}" -f $inventory.percentage_of_us)    
    $sql += ");"    
    return $sql
}

Write-OutputWithTime "Starting with inputs :"
Write-OutputWithTime "- inputFileName : $inputFileName"
Write-OutputWithTime "--"

if ($inputFileName -eq "" -or $inputFileName -eq $null)
{
    Write-OutputWithTime "inputFileName is required.  Stopping"
    Exit 1
}

$inputFileNameBase = [io.path]::GetFileNameWithoutExtension($inputFileName)
$outputFileName = $inputFileNameBase + ".sql";

Write-OutputWithTime "- outputFileName : $outputFileName"
Write-OutputWithTime "--"

if ($(Test-Path $inputFileName) -eq $false)
{
    Write-OutputWithTime "Input file '$inputFileName' not found.  Stopping"
    Exit 1
}

if ($(Test-Path $outputFileName) -eq $true)
{
    Write-OutputWithTime "Existing output file found ('$outputFileName').  Removing..."
    remove-item $outputFileName -force
}

Write-OutputWithTime "Reading the input file..."

$content = $(Get-Content -Raw -Path $inputFileName | ConvertFrom-Json).Data
$contracts = $content.contract
$inventories = $content.inventory

Write-OutputWithTime $("contracts found : " + $contracts.Count)
Write-OutputWithTime $("inventories found : " + $inventories.Count)

Write-OutputWithTime "Writing the contracts to file..."

$sqlTop = GetSql-Top

Add-Content $outputFileName $sqlTop

$lineGoThreshold = 100
$lineIndex = 0
foreach ($contract in $contracts)
{
    
    $lineSql = GetSql-Contract $contract
    Add-Content $outputFileName $lineSql

    $lineIndex++
    if ($lineIndex -ge $lineGoThreshold)
    {
        Add-Content $outputFileName "GO"
        $lineIndex = 0
    }
}

Add-Content $outputFileName "GO"

Write-OutputWithTime "Writing the inventories to file..."

$lineGoThreshold = 100
$lineIndex = 0
foreach ($inventory in $inventories)
{    
    $lineSql = GetSql-Inventory $inventory
    Add-Content $outputFileName $lineSql

    $lineIndex++
    if ($lineIndex -ge $lineGoThreshold)
    {
        Add-Content $outputFileName "GO"
        $lineIndex = 0
    }
}

Add-Content $outputFileName "GO"


Write-OutputWithTime "All done."
