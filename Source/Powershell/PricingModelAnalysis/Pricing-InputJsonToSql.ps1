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

# TODO: Add Frequency_cap, share_of_voice list and spot_lengths list and daypart_weighting list

function GetSql-Top
{
    $sql = @'
-- /*
USE broadcast 
GO
IF OBJECT_ID('tempdb..#contracts') IS NOT NULL
BEGIN 
	DROP TABLE #contracts
END 

IF OBJECT_ID('tempdb..#contract_share_of_voice') IS NOT NULL
BEGIN 
	DROP TABLE #contract_share_of_voice
END 

IF OBJECT_ID('tempdb..#contract_spot_lengths') IS NOT NULL
BEGIN 
	DROP TABLE #contract_spot_lengths
END 

IF OBJECT_ID('tempdb..#contract_daypart_weighting') IS NOT NULL
BEGIN 
	DROP TABLE #contract_daypart_weighting
END 

IF OBJECT_ID('tempdb..#inventory') IS NOT NULL
BEGIN 
	DROP TABLE #inventory
END 

IF OBJECT_ID('tempdb..#inventory_spot_cost') IS NOT NULL
BEGIN 
	DROP TABLE #inventory_spot_cost
END 

GO

CREATE TABLE #contracts
(
	week_id INT,
	impression_goal FLOAT,
	cpm_goal FLOAT,
	market_coverage_goal FLOAT,
    frequency_cap INT
)

CREATE TABLE #contract_share_of_voice
(
    week_id INT,
    market_code INT,
    market_goal FLOAT
)

CREATE TABLE #contract_spot_lengths
(
    week_id INT,
    spot_length_id INT,
    spot_length_goal FLOAT,
    spot_scale_factor FLOAT
)

CREATE TABLE #contract_daypart_weighting
(
    week_id INT,
    daypart_id INT,
    daypart_goal FLOAT
)

CREATE TABLE #inventory
(
	inventory_id INT,
	week_id INT,
	daypart_id INT,
	impressions FLOAT,    
	station_id INT,
	market_code INT,
	percentage_of_us FLOAT	
)

CREATE TABLE #inventory_spot_cost
(
    inventory_id INT,
    week_id INT,
	daypart_id INT,
    spot_length_id INT,
	spot_length_cost FLOAT,
)

GO
'@

    return $sql

}

function GetSql-BuildHelpfuleComponents
{
$sql = @'
USE broadcast 
GO

IF OBJECT_ID('tempdb..#components') IS NOT NULL
BEGIN 
	DROP TABLE #components
END 

IF OBJECT_ID('tempdb..#components_by_market') IS NOT NULL
BEGIN 
	DROP TABLE #components_by_market
END 

IF OBJECT_ID('tempdb..#market_sov_caps_hardcoded') IS NOT NULL
BEGIN 
	DROP TABLE #market_sov_caps_hardcoded
END 

CREATE TABLE #market_sov_caps_hardcoded
(
	market_rank_range_start INT,
	market_rank_range_end INT,
	sov_cap FLOAT
)

-- These are hardcoded into the DS Model.  It will be replaced with the SOV Caps Toggle Implementation
INSERT INTO #market_sov_caps_hardcoded (market_rank_range_start, market_rank_range_end, sov_cap)
	SELECT 1, 51, 100
	UNION
	SELECT 51, 100, 1
	UNION
	SELECT 101, 210, 0.5

SELECT ai.*		
	, ((ai.sum_cost * ac.spot_scale_factor) / ai.sum_impressions) * 1000 AS sum_cpm
	, ((ai.avg_cost * ac.spot_scale_factor) / ai.avg_impressions) * 1000  AS avg_cpm
	, ac.impression_goal
	, ac.cpm_goal
	, ac.spot_length_goal
INTO #components
FROM 
(
	SELECT i.week_id, i.daypart_id, c.spot_length_id
		, SUM(i.impressions) AS sum_impressions
		, AVG(i.impressions) AS avg_impressions
		, SUM(c.spot_length_cost) AS sum_cost 
		, AVG(c.Spot_length_cost) AS avg_cost 
	FROM #inventory i
	JOIN #inventory_spot_cost c 
		ON i.inventory_id = c.inventory_id
		AND i.week_id = c.week_id
		AND i.daypart_id = c.daypart_id
	GROUP BY i.week_id, i.daypart_id, c.spot_length_id
) ai
JOIN 
(
	SELECT c.*, d.daypart_id, d.daypart_goal
		, s.spot_length_id, spot_length_goal, spot_scale_factor
	FROM #contracts c
	JOIN #contract_daypart_weighting d
		ON d.week_id = c.week_id
	JOIN #contract_spot_lengths s
		ON s.week_id = c.week_id
) ac
	ON ai.week_id = ac.week_id
	AND ai.daypart_id = ac.daypart_id
	AND ai.spot_length_id = ac.spot_length_id

SELECT ai.*		
	, ((ai.sum_cost * ac.spot_scale_factor) / ai.sum_impressions) * 1000 AS sum_cpm
	, ((ai.avg_cost * ac.spot_scale_factor) / ai.avg_impressions) * 1000  AS avg_cpm
	, ac.impression_goal
	, ac.cpm_goal
	, ac.spot_length_goal
	, ac.market_goal
	INTO #components_by_market
FROM 
(
	-- inventory : week \ daypart \ market \ spot length
	SELECT i.week_id, i.daypart_id, i.market_code, c.spot_length_id
		, SUM(i.impressions) AS sum_impressions
		, AVG(i.impressions) AS avg_impressions
		, SUM(c.spot_length_cost) AS sum_cost 
		, AVG(c.Spot_length_cost) AS avg_cost 
	FROM #inventory i
	JOIN #inventory_spot_cost c 
		ON i.inventory_id = c.inventory_id
		AND i.week_id = c.week_id
		AND i.daypart_id = c.daypart_id	
	GROUP BY i.week_id, i.daypart_id, i.market_code, c.spot_length_id
) ai
JOIN 
(
	-- contract : week \ daypart \ market\ spot length
	SELECT c.*, d.daypart_id, d.daypart_goal
		, v.market_code, v.market_goal
		, s.spot_length_id, spot_length_goal, spot_scale_factor
	FROM #contracts c
	JOIN #contract_daypart_weighting d
		ON d.week_id = c.week_id
	JOIN #contract_spot_lengths s
		ON s.week_id = c.week_id
	JOIN #contract_share_of_voice v
		ON v.week_id = c.week_id
) ac
	ON ai.week_id = ac.week_id
		AND ai.daypart_id = ac.daypart_id
		AND ai.market_code = ac.market_code
		AND ai.spot_length_id = ac.spot_length_id

-- All tables
select * from #contracts
select * from #contract_share_of_voice
select * from #contract_spot_lengths
select * from #contract_daypart_weighting

select * from #inventory
select * from #inventory_spot_cost

'@

    return $sql
}

function GetSql-HelpfulQueries
{
    $sql = @'
USE broadcast 
GO

-- CPM Week\Daypart\SpotLength
SELECT b.week_id, b.daypart_id, b.spot_length_id
	, CASE WHEN b.sum_impressions >= b.impression_goal THEN 'Yes' ELSE 'No' END AS MetImpressionsGoal
	, CASE 
		WHEN b.sum_cpm > b.cpm_goal THEN CAST((b.sum_cpm - b.cpm_goal) AS VARCHAR(20)) + ' OVER the CPM goal' 
		WHEN b.sum_cpm < b.cpm_goal THEN CAST((b.cpm_goal - b.sum_cpm) AS VARCHAR(20)) + ' UNDER the CPM goal' 
		ELSE  'EQUALS the CPM goal'
	END AS cpm_delta_comparison
	, '|||' AS SEP
	, b.impression_goal, b.sum_impressions, b.avg_impressions
	, b.cpm_goal, b.sum_cpm, b.avg_cpm
	, b.sum_cost, b.avg_cost
FROM #components b

-- CPM Week\Daypart\Market\SpotLength
SELECT b.week_id, b.daypart_id, b.market_code, b.spot_length_id
	, CASE WHEN b.sum_impressions >= b.impression_goal THEN 'Yes' ELSE 'No' END AS MetImpressionsGoal
	, CASE 
		WHEN b.sum_cpm > b.cpm_goal THEN CAST((b.sum_cpm - b.cpm_goal) AS VARCHAR(20)) + ' OVER the CPM goal' 
		WHEN b.sum_cpm < b.cpm_goal THEN CAST((b.cpm_goal - b.sum_cpm) AS VARCHAR(20)) + ' UNDER the CPM goal' 
		ELSE  'EQUALS the CPM goal'
	END AS cpm_delta_comparison		
	, '|||' AS SEP
	, b.impression_goal, b.sum_impressions, b.avg_impressions
	, b.cpm_goal, b.sum_cpm, b.avg_cpm
	, b.sum_cost, b.avg_cost	
FROM #components_by_market b

-- market goals met?
SELECT b.*
	, CASE 
        WHEN b.market_impressions_percentage >= b.market_goal THEN 'Goal Met'
	    ELSE 'Goal not Met' 
    END AS met_market_impressions_goal
    , bm.geography_name
FROM 
(
	SELECT m.week_id, m.daypart_id, m.spot_length_id, m.market_code
		, m.sum_impressions / c.sum_impressions AS market_impressions_percentage
		, m.market_goal
	FROM #components_by_market m
	JOIN #components c
		ON m.week_id = c.week_id
		AND m.daypart_id = c.daypart_id
		AND m.spot_length_id = c.spot_length_id
) b
JOIN markets bm
	ON bm.market_code = b.market_code

-- Are we within the hardcoded Market Caps?
-- (These caps will be replaced with the SOV Caps Toggle Implementation)
SELECT bm.geography_name
	, m.market_code
	, mc.[rank] AS market_rank
	, m.market_goal
	, m.market_goal * 100 AS market_goal_percent
	, sc.sov_cap
	, CASE WHEN (m.market_goal * 100) > sc.sov_cap THEN 'Exceeds CAP' ELSE 'Within CAP' END AS CapEvaluation
FROM #components_by_market m
JOIN #components c
	ON m.week_id = c.week_id
	AND m.daypart_id = c.daypart_id
	AND m.spot_length_id = c.spot_length_id
JOIN market_coverages mc
	ON mc.market_code = m.market_code
	AND mc.market_coverage_file_id = 1 -- there is only 1, but make sure
JOIN markets bm 
	ON bm.market_code = m.market_code
LEFT OUTER JOIN #market_sov_caps_hardcoded sc
	ON mc.[rank] BETWEEN sc.market_rank_range_start AND sc.market_rank_range_end	
WHERE m.week_id = (select MIN(week_id) from #contracts)
ORDER BY market_rank

'@

    return $sql
}

function GetSql-Contract($contract)
{
    $sql = 'INSERT INTO #contracts (week_id, impression_goal, cpm_goal, market_coverage_goal, frequency_cap) VALUES ('
    $sql += $("{0}," -f $contract.week_id)
    $sql += $("{0}," -f $contract.impression_goal)
    $sql += $("{0}," -f $contract.cpm_goal)
    $sql += $("{0}," -f $contract.market_coverage_goal)
    $sql += $("{0}" -f $contract.frequency_cap)    
    $sql += ");"
    return $sql
}

function GetSql-Contract-Share-Of-Voice
{
    param
    (        
        $contract,        
        $contract_share_of_voice
    )
    $sql = 'INSERT INTO #contract_share_of_voice (week_id, market_code, market_goal) VALUES ('
    $sql += $("{0}," -f $contract.week_id)
    $sql += $("{0}," -f $contract_share_of_voice.market_code)
    $sql += $("{0}" -f $contract_share_of_voice.market_goal)
    $sql += ");"
    return $sql
}

function GetSql-Contract-Spot-Lengths
{
    param
    (
        $contract, 
        $contract_spot_lengths
    )

    $sql = 'INSERT INTO #contract_spot_lengths (week_id, spot_length_id, spot_length_goal, spot_scale_factor) VALUES ('
    $sql += $("{0}," -f $contract.week_id)
    $sql += $("{0}," -f $contract_spot_lengths.spot_length_id)
    $sql += $("{0}," -f $contract_spot_lengths.spot_length_goal)
    $sql += $("{0}" -f $contract_spot_lengths.spot_scale_factor)
    $sql += ");"
    return $sql
}

function GetSql-Contract-Daypart-Weighting
{
    param
    (
        $contract, 
        $contract_daypart_weighting
    )
    $sql = 'INSERT INTO #contract_daypart_weighting (week_id, daypart_id, daypart_goal) VALUES ('
    $sql += $("{0}," -f $contract.week_id)
    $sql += $("{0}," -f $contract_daypart_weighting.daypart_id)
    $sql += $("{0}" -f $contract_daypart_weighting.daypart_goal)
    $sql += ");"
    return $sql
}

function GetSql-Inventory($inventory)
{
    $sql = 'INSERT INTO #inventory (inventory_id, week_id, daypart_id, impressions, station_id, market_code, percentage_of_us) VALUES ('
    $sql += $("{0}," -f $inventory.id)
    $sql += $("{0}," -f $inventory.week_id)
    $sql += $("{0}," -f $inventory.daypart_id)
    $sql += $("{0}," -f $inventory.impressions)    
    $sql += $("{0}," -f $inventory.station_id)
    $sql += $("{0}," -f $inventory.market_code)
    $sql += $("{0}" -f $inventory.percentage_of_us)    
    $sql += ");"    
    return $sql
}

function GetSql-Inventory-Spot-Cost
{
    param
    (
        $inventory, 
        $inventory_spot_cost
    )
    $sql = 'INSERT INTO #inventory_spot_cost (inventory_id, week_id, daypart_id, spot_length_id, spot_length_cost) VALUES ('
    $sql += $("{0}," -f $inventory.id)
    $sql += $("{0}," -f $inventory.week_id)
    $sql += $("{0}," -f $inventory.daypart_id)

    $sql += $("{0}," -f $inventory_spot_cost.spot_length_id)    
    $sql += $("{0}" -f $inventory_spot_cost.spot_length_cost)
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

$content = $(Get-Content -Raw -Path $inputFileName | ConvertFrom-Json)
$contracts = $content.contract
$inventories = $content.inventory

Write-OutputWithTime $("contracts found : " + $contracts.Count)
Write-OutputWithTime $("inventories found : " + $inventories.Count)

Write-OutputWithTime "Beginning item transformation to sql..."

$sqlTop = GetSql-Top
Add-Content $outputFileName $sqlTop

$lineGoThreshold = 100
$lineIndex = 0

Write-OutputWithTime "Writing the contracts to file..."

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

Write-OutputWithTime "Writing the contract SOVs to file..."

$lineGoThreshold = 100
$lineIndex = 0
foreach ($contract in $contracts)
{
    foreach($contract_share_of_voice in $contract.share_of_voice)
    {
        $lineSql = GetSql-Contract-Share-Of-Voice -contract $contract -contract_share_of_voice $contract_share_of_voice
        Add-Content $outputFileName $lineSql

        $lineIndex++
        if ($lineIndex -ge $lineGoThreshold)
        {
            Add-Content $outputFileName "GO"
            $lineIndex = 0
        }
    }    
}

Add-Content $outputFileName "GO"

Write-OutputWithTime "Writing the contract SpotLengths to file..."

$lineGoThreshold = 100
$lineIndex = 0
foreach ($contract in $contracts)
{
    foreach($contract_spot_length in $contract.spot_lengths)
    {
        $lineSql = GetSql-Contract-Spot-Lengths -contract $contract -contract_spot_length $contract_spot_length
        Add-Content $outputFileName $lineSql

        $lineIndex++
        if ($lineIndex -ge $lineGoThreshold)
        {
            Add-Content $outputFileName "GO"
            $lineIndex = 0
        }
    }    
}

Add-Content $outputFileName "GO"

Write-OutputWithTime "Writing the contract Daypart Weightings to file..."

$lineGoThreshold = 100
$lineIndex = 0
foreach ($contract in $contracts)
{
    foreach($contract_daypart_weighting in $contract.daypart_weighting)
    {
        $lineSql = GetSql-Contract-Daypart-Weighting -contract $contract -contract_daypart_weighting $contract_daypart_weighting
        Add-Content $outputFileName $lineSql

        $lineIndex++
        if ($lineIndex -ge $lineGoThreshold)
        {
            Add-Content $outputFileName "GO"
            $lineIndex = 0
        }
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

Write-OutputWithTime "Writing the inventory spot_costs to file..."

$lineGoThreshold = 100
$lineIndex = 0
foreach ($inventory in $inventories)
{
    foreach($inventory_spot_cost in $inventory.spot_cost)
    {
        $lineSql = GetSql-Inventory-Spot-Cost -inventory $inventory -inventory_spot_cost $inventory_spot_cost
        Add-Content $outputFileName $lineSql

        $lineIndex++
        if ($lineIndex -ge $lineGoThreshold)
        {
            Add-Content $outputFileName "GO"
            $lineIndex = 0
        }
    }    
}

Add-Content $outputFileName "GO"

$sql = GetSql-BuildHelpfuleComponents
Add-Content $outputFileName $sql

Add-Content $outputFileName "--*/"

$sql = GetSql-HelpfulQueries
Add-Content $outputFileName $sql

Write-OutputWithTime "All done."
