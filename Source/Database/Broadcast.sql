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

/*************************************** START PRI-7486 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'full_name' AND OBJECT_ID = OBJECT_ID(N'daypart_codes'))
BEGIN
    ALTER TABLE daypart_codes
	ADD full_name [varchar](255) NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'name' AND OBJECT_ID = OBJECT_ID(N'daypart_codes'))
BEGIN
	EXEC sp_RENAME 'daypart_codes.name', 'code', 'COLUMN'
END

GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'full_name' AND OBJECT_ID = OBJECT_ID(N'daypart_codes'))
BEGIN
    update daypart_codes set full_name = 'Early Morning News' where code = 'EMN'
	update daypart_codes set full_name = 'Midday News' where code = 'MDN'
	update daypart_codes set full_name = 'Evening News' where code = 'EN'
	update daypart_codes set full_name = 'Late News' where code = 'LN'
	update daypart_codes set full_name = 'Evening News/Late News' where code = 'ENLN'
	update daypart_codes set full_name = 'Early Fringe' where code = 'EF'
	update daypart_codes set full_name = 'Prime Access' where code = 'PA'
	update daypart_codes set full_name = 'Prime' where code = 'PT'
	update daypart_codes set full_name = 'Late Fringe' where code = 'LF'
	update daypart_codes set full_name = 'Total Day Syndication' where code = 'SYN'
	update daypart_codes set full_name = 'Overnights' where code = 'OVN'
	update daypart_codes set full_name = 'Daytime' where code = 'DAY'
	update daypart_codes set full_name = 'Diginet' where code = 'DIGI'
END
/*************************************** END PRI-7486 *****************************************************/

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

/*************************************** START PRI-9375 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'daypart_code_id' AND OBJECT_ID = OBJECT_ID(N'inventory_file_proprietary_header'))
BEGIN
	ALTER TABLE inventory_file_proprietary_header ADD daypart_code_id INT
	
	-- Necessary to avoid errors with the parser due to missing column.
	EXEC('UPDATE inventory_file_proprietary_header SET daypart_code_id = (SELECT id FROM daypart_codes WHERE daypart_codes.code = daypart_code)')

	ALTER TABLE inventory_file_proprietary_header ALTER COLUMN daypart_code_id INT NOT NULL
	
	ALTER TABLE [dbo].[inventory_file_proprietary_header] WITH CHECK ADD CONSTRAINT [FK_inventory_file_proprietary_header_daypart_codes] FOREIGN KEY([daypart_code_id])
	REFERENCES [dbo].[daypart_codes] ([id])
	ALTER TABLE [dbo].[inventory_file_proprietary_header] CHECK CONSTRAINT [FK_inventory_file_proprietary_header_daypart_codes]
	CREATE NONCLUSTERED INDEX [IX_inventory_file_proprietary_header_daypart_code_id]  ON [dbo].[inventory_file_proprietary_header] ([daypart_code_id] ASC)
	INCLUDE ([id], [inventory_file_id]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	ALTER TABLE inventory_file_proprietary_header DROP COLUMN daypart_code
END

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name = N'IX_station_inventory_group_name' AND OBJECT_ID = OBJECT_ID(N'station_inventory_group'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_station_inventory_group_name]  ON [dbo].[station_inventory_group] ([name] ASC)
	INCLUDE ([id], [inventory_source_id]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
/*************************************** END PRI-9375 *****************************************************/

/*************************************** START PRI-7621 *****************************************************/
IF NOT EXISTS (SELECT 1 FROM system_component_parameters WHERE component_id = 'BroadcastService' and parameter_key = 'InventoryUploadErrorsFolder')
BEGIN
        INSERT system_component_parameters (component_id, parameter_key, parameter_value, parameter_type, description, last_modified_time)
        Values('BroadcastService', 'InventoryUploadErrorsFolder', 'D:\\temp', 'string', 'Folder used to store inventory files with validation errors', GETDATE())
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'rows_processed' AND OBJECT_ID = OBJECT_ID(N'inventory_files'))
BEGIN
	ALTER TABLE [inventory_files] ADD rows_processed INT NULL	
END
/*************************************** END PRI-7621 *****************************************************/

/*************************************** START PRI-9931 *****************************************************/
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'daypart_code' AND OBJECT_ID = OBJECT_ID(N'station_inventory_group'))
BEGIN
	ALTER TABLE [station_inventory_group] DROP COLUMN daypart_code
END
/*************************************** END PRI-9931 *****************************************************/

/*************************************** START PRI-7621 PART 2 *****************************************************/
IF EXISTS (SELECT 1 FROM system_component_parameters WHERE component_id = 'BroadcastService' and parameter_key = 'InventoryUploadErrorsFolder')
BEGIN
	UPDATE system_component_parameters SET parameter_value = '\\cadfs11\Inventory Management UI\Continuous Deployment'
	WHERE component_id = 'BroadcastService' AND parameter_key = 'InventoryUploadErrorsFolder'
END
/*************************************** END PRI-7621 *****************************************************/


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