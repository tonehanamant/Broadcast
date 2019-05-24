---------------------------------------------------------------------------------------------------
-- !!! If Executing from SQL Server Manager, please enable SQLCMD Mode!!! To enable option, select menu Query->Enable SQLCMD mode. --
---------------------------------------------------------------------------------------------------
-- All scripts should be written in a way that they can be run multiple times
-- All features/bugs should be wrapped in comments indicating the start/end of the scripts
---------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------
-- TFS Items:
---------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------
SET NOEXEC OFF;
SET NOCOUNT OFF;
GO

:on error exit --sqlcmd exit script on error
GO
:setvar __IsSqlCmdEnabled "True"
GO
IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'
	BEGIN
		PRINT N'SQLCMD mode must be enabled to successfully execute this script.To enable option, select menu Query->Enable SQLCMD mode';
		SET NOCOUNT ON;
		SET NOEXEC ON; -- this will not execute any queries. queries will be compiled only.
	END
GO

SET XACT_ABORT ON -- Rollback transaction incase of error
GO


BEGIN
	PRINT 'RUNNING SCRIPT IN LOCAL DATBASE'
END
GO

BEGIN TRANSACTION

CREATE TABLE #previous_version 
( 
	[version] VARCHAR(32) 
)
GO

-- Only run this script when the schema is in the correct previous version
INSERT INTO #previous_version
		SELECT parameter_value 
		FROM system_component_parameters 
		WHERE parameter_key = 'SchemaVersion' 
GO

/*************************************** START UPDATE SCRIPT *****************************************************/


/*************************************** START PRI-8714 *****************************************************/
--insert missing data into station_inventory_manifest_weeks for all the open market manifests
IF NOT EXISTS (SELECT TOP 1 1 FROM station_inventory_manifest_weeks
WHERE station_inventory_manifest_id in (SELECT id FROM station_inventory_manifest WHERE inventory_source_id =1))
BEGIN
	EXEC('INSERT INTO station_inventory_manifest_weeks(station_inventory_manifest_id, media_week_id, spots, start_date, end_date)
	SELECT t2.id AS station_inventory_manifest_id
			, t1.id media_week_id
			, 0 Spots
			, [start_date] = CASE
				WHEN t2.effective_date <= t1.start_date AND t2.end_date >= t1.end_date THEN t1.start_date
				WHEN t2.effective_date <= t1.start_date AND t2.end_date >= t1.start_date AND t2.end_date <= t1.end_date THEN t1.start_date
				WHEN t2.effective_date >= t1.start_date AND t2.end_date <= t1.end_date THEN t2.effective_date
				WHEN t2.effective_date >= t1.start_date AND t2.effective_date <= t1.end_date AND t2.end_date >= t1.end_date THEN t2.effective_date
			END
			, [end_date] = CASE
				WHEN t2.effective_date <= t1.start_date AND t2.end_date >= t1.end_date THEN t1.end_date
				WHEN t2.effective_date <= t1.start_date AND t2.end_date >= t1.start_date AND t2.end_date <= t1.end_date THEN t2.end_date
				WHEN t2.effective_date >= t1.start_date AND t2.end_date <= t1.end_date THEN t2.end_date
				WHEN t2.effective_date >= t1.start_date AND t2.effective_date <= t1.end_date AND t2.end_date >= t1.end_date THEN t1.end_date
			END
	FROM media_weeks AS t1 
	INNER JOIN station_inventory_manifest AS t2 ON t1.start_date between t2.effective_date and t2.end_date
	WHERE  t2.inventory_source_id = 1 --OpenMarket
	ORDER BY start_date ASC	')
END

--remove effective_date from station_inventory_manifest table
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'effective_date' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest'))
BEGIN
	DROP INDEX [IX_station_inventory_manifest_inventory_source_id] ON [station_inventory_manifest]
    ALTER TABLE station_inventory_manifest DROP COLUMN [effective_date]
	CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_inventory_source_id] ON [dbo].[station_inventory_manifest]
	(
		[inventory_source_id] ASC
	)
	INCLUDE ([id],
		[station_id],
		[spots_per_week],
		[station_inventory_group_id],
		[file_id],
		[spots_per_day]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

--remove end_date from station_inventory_manifest table
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'end_date' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest'))
BEGIN
    ALTER TABLE station_inventory_manifest DROP COLUMN [end_date]
END

--remove start_date and end_date from station_inventory_group
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'end_date' AND OBJECT_ID = OBJECT_ID(N'station_inventory_group'))
BEGIN
    ALTER TABLE [station_inventory_group] DROP COLUMN [end_date]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'start_date' AND OBJECT_ID = OBJECT_ID(N'station_inventory_group'))
BEGIN
    ALTER TABLE [station_inventory_group] DROP COLUMN [start_date]
END
/*************************************** END PRI-8714 *****************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.07.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.06.1' -- Previous release version
		OR [version] = '19.07.1') -- Current release version
	BEGIN
		PRINT 'Database Successfully Updated'
		COMMIT TRANSACTION
		DROP TABLE #previous_version
	END
	ELSE
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Incorrect Previous Database Version', 11, 1)
	END

END
GO

IF(XACT_STATE() = -1)
BEGIN
	ROLLBACK TRANSACTION
	RAISERROR('Database Update Failed. Transaction rolled back.', 11, 1)
END
GO