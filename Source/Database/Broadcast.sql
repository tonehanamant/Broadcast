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

/*************************************** START UPDATE SCRIPT ************************************************/


/*************************************** START PRI-8007 *****************************************************/
--update the table name
IF EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('dbo.inventory_file_barter_header'))
BEGIN
	exec sp_rename 'dbo.inventory_file_barter_header', 'inventory_file_proprietary_header'
END

--update the primary key constraint name
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_TYPE = 'PRIMARY KEY' AND CONSTRAINT_NAME ='PK_inventory_file_barter_header')
BEGIN
	EXEC sp_rename N'dbo.PK_inventory_file_barter_header', N'PK_inventory_file_proprietary_header'
END

--update the constraints names
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_inventory_file_barter_header_audiences')
BEGIN
	EXEC sp_rename N'dbo.FK_inventory_file_barter_header_audiences', N'FK_inventory_file_proprietary_header_audiences', N'OBJECT'
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_inventory_file_barter_header_dayparts')
BEGIN
	EXEC sp_rename N'dbo.FK_inventory_file_barter_header_dayparts', N'FK_inventory_file_proprietary_header_dayparts', N'OBJECT'
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_inventory_file_barter_header_inventory_files')
BEGIN
	EXEC sp_rename N'dbo.FK_inventory_file_barter_header_inventory_files', N'FK_inventory_file_proprietary_header_inventory_files', N'OBJECT'
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_inventory_file_barter_header_media_months')
BEGIN
	EXEC sp_rename N'dbo.FK_inventory_file_barter_header_media_months', N'FK_inventory_file_proprietary_header_media_months', N'OBJECT'
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_inventory_file_barter_header_media_months_hut_book')
BEGIN
	EXEC sp_rename N'dbo.FK_inventory_file_barter_header_media_months_hut_book', N'FK_inventory_file_proprietary_header_media_months_hut_book', N'OBJECT'
END

--update the indexes names
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_inventory_file_barter_header_audience_id' AND object_id = OBJECT_ID('dbo.inventory_file_proprietary_header'))
BEGIN
	EXEC sp_rename N'dbo.inventory_file_proprietary_header.IX_inventory_file_barter_header_audience_id', N'IX_inventory_file_proprietary_header_audience_id', N'INDEX'
END

IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_inventory_file_barter_header_contracted_daypart_id' AND object_id = OBJECT_ID('dbo.inventory_file_proprietary_header'))
BEGIN
	EXEC sp_rename N'dbo.inventory_file_proprietary_header.IX_inventory_file_barter_header_contracted_daypart_id', N'IX_inventory_file_proprietary_header_contracted_daypart_id', N'INDEX'
END

IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_inventory_file_barter_header_hut_projection_book_id' AND object_id = OBJECT_ID('dbo.inventory_file_proprietary_header'))
BEGIN
	EXEC sp_rename N'dbo.inventory_file_proprietary_header.IX_inventory_file_barter_header_hut_projection_book_id', N'IX_inventory_file_proprietary_header_hut_projection_book_id', N'INDEX'
END

IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_inventory_file_barter_header_inventory_file_id' AND object_id = OBJECT_ID('dbo.inventory_file_proprietary_header'))
BEGIN
	EXEC sp_rename N'dbo.inventory_file_proprietary_header.IX_inventory_file_barter_header_inventory_file_id', N'IX_inventory_file_proprietary_header_inventory_file_id', N'INDEX'
END

IF EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_inventory_file_barter_header_share_projection_book_id' AND object_id = OBJECT_ID('dbo.inventory_file_proprietary_header'))
BEGIN
	EXEC sp_rename N'dbo.inventory_file_proprietary_header.IX_inventory_file_barter_header_share_projection_book_id', N'IX_inventory_file_proprietary_header_share_projection_book_id', N'INDEX'
END

/*************************************** END PRI-8007 *******************************************************/

/*************************************** START PRI-8453 *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE OBJECT_ID = OBJECT_ID('schedule_details') AND name = 'IX_schedule_details_schedule_id_total_spots')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_schedule_details_schedule_id_total_spots] ON [dbo].[schedule_details]
	(
		[schedule_id] ASC
	)
	INCLUDE ( 	[total_spots])
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE OBJECT_ID = OBJECT_ID('schedule_detail_audiences') AND name = 'IX_schedule_detail_audiences_schedule_detail_id_audience_rank_impressions')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_schedule_detail_audiences_schedule_detail_id_audience_rank_impressions] ON [dbo].[schedule_detail_audiences]
	(
		[schedule_detail_id] ASC,
		[audience_rank] ASC
	)
	INCLUDE ( 	[impressions])
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE OBJECT_ID = OBJECT_ID('bvs_file_details') AND name = 'IX_bvs_file_details_estimate_id_status_spot_length')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_bvs_file_details_estimate_id_status_spot_length] ON [dbo].[bvs_file_details]
	(
		[estimate_id] ASC,
		[status] ASC
	)
	INCLUDE ( 	[spot_length])
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE OBJECT_ID = OBJECT_ID('bvs_post_details') AND name = 'IX_bvs_post_details_bvs_file_detail_id_audience_rank_delivery')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_bvs_post_details_bvs_file_detail_id_audience_rank_delivery] ON [dbo].[bvs_post_details]
	(
		[bvs_file_detail_id] ASC,
		[audience_rank] ASC
	)
	INCLUDE ( 	[delivery])
END
GO

/*************************************** END PRI-8453 *******************************************************/

/*************************************** START PRI-6134 *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = N'vpvh' AND 
			  OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_audiences'))
BEGIN
    ALTER TABLE station_inventory_manifest_audiences
	ADD vpvh FLOAT NULL
END

/*************************************** END PRI-6134 *******************************************************/

/*************************************** START PRI-8858 *****************************************************/

IF (SELECT name FROM inventory_sources
    WHERE name = 'OpenMarket') IS NOT NULL
BEGIN
	UPDATE inventory_sources
	SET name = 'Open Market'
	WHERE name = 'OpenMarket'
END

/*************************************** END PRI-8858 *******************************************************/

/*************************************** START PRI-8030 *****************************************************/

IF NOT (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
				WHERE TABLE_SCHEMA = 'dbo' AND
				TABLE_NAME = 'campaigns'))
BEGIN
	CREATE TABLE campaigns
	(
		id INT IDENTITY NOT NULL,
		name VARCHAR(127) NOT NULL,
		advertiser_id INT NOT NULL,
		agency_id INT NOT NULL,
		start_date DATETIME NOT NULL,
		end_date DATETIME NOT NULL,
		budget MONEY,
		created_date DATETIME NOT NULL,
		created_by VARCHAR(63) NOT NULL,
		modified_date DATETIME,
		modified_by VARCHAR(63)
		CONSTRAINT PK_campaigns PRIMARY KEY CLUSTERED
		(
			id ASC
		)
	)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
			  WHERE Name = N'campaign_id'
			  AND Object_ID = Object_ID(N'proposals'))
BEGIN
    ALTER TABLE proposals
	ADD campaign_id INT

	ALTER TABLE proposals ADD CONSTRAINT FK_proposals_campaigns
	FOREIGN KEY (campaign_id) REFERENCES campaigns(id)
END


/*************************************** END PRI-8030 *******************************************************/

/*************************************** START PRI-8277 *****************************************************/

--Add start_date, end_date to the weeks table
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'start_date' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_weeks'))
BEGIN
    ALTER TABLE station_inventory_manifest_weeks ADD [start_date] date NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'end_date' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_weeks'))
BEGIN
    ALTER TABLE station_inventory_manifest_weeks ADD [end_date] date NULL
END

GO

--make start_date and effective_date nullable in manifest and group tables
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'effective_date' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest'))
BEGIN
    ALTER TABLE station_inventory_manifest ALTER COLUMN effective_date [date] NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'start_date' AND OBJECT_ID = OBJECT_ID(N'station_inventory_group'))
BEGIN
    ALTER TABLE station_inventory_group ALTER COLUMN [start_date] [date] NULL
END

GO

--populate start_date, end_date in station_inventory_manifest_weeks
SELECT *
INTO #ManifestWeeks
FROM (SELECT mw.id AS manifest_week_id,
	  m.effective_date AS manifest_effective_date,
	  m.end_date AS manifest_end_date,
	  w.[start_date] AS media_week_start_date,
	  w.end_date AS media_week_end_date
	  FROM station_inventory_manifest_weeks AS mw
	  JOIN station_inventory_manifest AS m ON m.id = mw.station_inventory_manifest_id
	  JOIN media_weeks AS w ON w.id = mw.media_week_id
	  WHERE inventory_source_id != 1 --skip OpenMarket, it will be processed in the OpenMarket expiration story
	  AND CAST(m.end_date as datetime) >= CAST(m.effective_date as datetime) --take only valid manifests
	  AND NOT CAST(m.end_date as datetime) < CAST(w.[start_date] as datetime) AND NOT CAST(m.effective_date as datetime) > CAST(w.end_date as datetime)--skip invalid cases
	  ) DATA

GO

SELECT *
INTO #ManifestWeekDateRanges
FROM (SELECT w.id AS manifest_week_id,
		[start_date] = CASE
			WHEN manifest_effective_date <= media_week_start_date AND
				 manifest_end_date >= media_week_end_date THEN media_week_start_date

			WHEN manifest_effective_date <= media_week_start_date AND
				 manifest_end_date >= media_week_start_date AND
				 manifest_end_date <= media_week_end_date THEN media_week_start_date

			WHEN manifest_effective_date >= media_week_start_date AND
				 manifest_end_date <= media_week_end_date THEN manifest_effective_date

			WHEN manifest_effective_date >= media_week_start_date AND
				 manifest_effective_date <= media_week_end_date AND
				 manifest_end_date >= media_week_end_date THEN manifest_effective_date
		END,
		end_date = CASE
			WHEN manifest_effective_date <= media_week_start_date AND
				 manifest_end_date >= media_week_end_date THEN media_week_end_date

			WHEN manifest_effective_date <= media_week_start_date AND
				 manifest_end_date >= media_week_start_date AND
				 manifest_end_date <= media_week_end_date THEN manifest_end_date

			WHEN manifest_effective_date >= media_week_start_date AND
				 manifest_end_date <= media_week_end_date THEN manifest_end_date

			WHEN manifest_effective_date >= media_week_start_date AND
				 manifest_effective_date <= media_week_end_date AND
				 manifest_end_date >= media_week_end_date THEN media_week_end_date
		END
	FROM station_inventory_manifest_weeks AS w
	JOIN #ManifestWeeks AS join1 ON join1.manifest_week_id = w.id) DATA

GO

EXEC('UPDATE station_inventory_manifest_weeks
SET station_inventory_manifest_weeks.[start_date] = join1.[start_date],
	station_inventory_manifest_weeks.end_date = join1.end_date
FROM station_inventory_manifest_weeks
JOIN #ManifestWeekDateRanges AS join1 ON join1.manifest_week_id = station_inventory_manifest_weeks.id')

GO

DROP TABLE #ManifestWeekDateRanges
DROP TABLE #ManifestWeeks

GO

--make start_date and end_date not nullable in station_inventory_manifest_weeks
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'start_date' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_weeks'))
BEGIN
    ALTER TABLE station_inventory_manifest_weeks ALTER COLUMN [start_date] date NOT NULL
	
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'end_date' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_weeks'))
BEGIN
    ALTER TABLE station_inventory_manifest_weeks ALTER COLUMN [end_date] date NOT NULL	
END

--add indexes
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE OBJECT_ID = OBJECT_ID('station_inventory_manifest_weeks') AND name = 'IX_station_inventory_manifest_weeks_start_date')
BEGIN
	EXEC('CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_weeks_start_date] ON [dbo].[station_inventory_manifest_weeks] ([start_date])')
END

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE OBJECT_ID = OBJECT_ID('station_inventory_manifest_weeks') AND name = 'IX_station_inventory_manifest_weeks_end_date')
BEGIN
	EXEC('CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_weeks_end_date] ON [dbo].[station_inventory_manifest_weeks] ([end_date])')
END

----adding history table to station_inventory_manifest_weeks
IF EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('dbo.station_inventory_manifest_weeks_history'))
BEGIN
    SET NOEXEC ON;	
END

--Step 1 – Add start time and end time period columns in the table
EXEC('ALTER TABLE dbo.station_inventory_manifest_weeks ADD sys_start_date DATETIME2')
EXEC('ALTER TABLE dbo.station_inventory_manifest_weeks ADD sys_end_date DATETIME2')

GO

--Step 2 - set default min and max date value
EXEC('UPDATE dbo.station_inventory_manifest_weeks SET sys_start_date = ''19000101 00:00:00.0000000''')
EXEC('UPDATE dbo.station_inventory_manifest_weeks SET sys_end_date = ''99991231 23:59:59.9999999''')

GO

--Step 3 – Alter column to add NOT NULL constraint
EXEC('ALTER TABLE dbo.station_inventory_manifest_weeks ALTER COLUMN sys_start_date DATETIME2 NOT NULL')
EXEC('ALTER TABLE dbo.station_inventory_manifest_weeks ALTER COLUMN sys_end_date DATETIME2 NOT NULL')

GO

--Step 5 – Declare system period columns
EXEC('ALTER TABLE dbo.station_inventory_manifest_weeks ADD PERIOD FOR SYSTEM_TIME (sys_start_date, sys_end_date)')

GO

--Step 6 – Enable system versioning on the table
EXEC('ALTER TABLE dbo.station_inventory_manifest_weeks SET(SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.station_inventory_manifest_weeks_history, DATA_CONSISTENCY_CHECK = ON))')

SET NOEXEC OFF;

/*************************************** END PRI-8277 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.06.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.05.1' -- Previous release version
		OR [version] = '19.06.1') -- Current release version
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