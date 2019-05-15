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

/*************************************** END UPDATE SCRIPT **************************************************/

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