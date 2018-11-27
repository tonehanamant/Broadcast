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


/*************************************** START UPDATE SCRIPT *****************************************************/

/*************************************** START BCOP-3534 *****************************************************/
IF OBJECT_ID('station_inventory_manifest_staging', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.station_inventory_manifest_staging
	(  
		[id] int IDENTITY(1,1) NOT NULL,
		[file_id] int NULL,
		[manifest_id] varchar(63) NOT NULL,
		[inventory_source_id] int NOT NULL,
		[station] varchar(63) NOT NULL,
		[spots_per_week] int NULL,
		[spots_per_day] int NULL,
		[manifest_spot_length_id] int NOT NULL,
		[effective_date] date NOT NULL,
		[end_date] date NULL,
		[audience_id] int NOT NULL,
		[impressions] float NULL,
		[rating] float NULL,
		[audience_rate] money NOT NULL,
		[is_reference] bit NOT NULL,
		[daypart_id] int NOT NULL,
		[program_name] varchar(255) NULL,
		[rate] money NOT NULL,
		[rate_spot_length_id] int NOT NULL

		CONSTRAINT [PK_station_inventory_manifest_staging] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)

	ALTER TABLE [dbo].[station_inventory_manifest_staging]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_staging_spot_lengths] FOREIGN KEY([manifest_spot_length_id])
	REFERENCES [dbo].[spot_lengths] ([id])
	ALTER TABLE [dbo].[station_inventory_manifest_staging] CHECK CONSTRAINT [FK_station_inventory_manifest_staging_spot_lengths]

	ALTER TABLE [dbo].[station_inventory_manifest_staging]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_staging_inventory_files] FOREIGN KEY([file_id])
	REFERENCES [dbo].[inventory_files] ([id])
	ALTER TABLE [dbo].[station_inventory_manifest_staging] CHECK CONSTRAINT [FK_station_inventory_manifest_staging_inventory_files]

	ALTER TABLE [dbo].[station_inventory_manifest_staging]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_staging_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	ALTER TABLE [dbo].[station_inventory_manifest_staging] CHECK CONSTRAINT [FK_station_inventory_manifest_staging_inventory_sources]

	ALTER TABLE [dbo].[station_inventory_manifest_staging] ADD  CONSTRAINT [DF_station_inventory_manifest_staging_is_reference]  DEFAULT ((0)) FOR [is_reference]

	ALTER TABLE [dbo].[station_inventory_manifest_staging]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_staging_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])
	ALTER TABLE [dbo].[station_inventory_manifest_staging] CHECK CONSTRAINT [FK_station_inventory_manifest_staging_audiences]

	ALTER TABLE [dbo].[station_inventory_manifest_staging]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_staging_dayparts] FOREIGN KEY([daypart_id])
	REFERENCES [dbo].[dayparts] ([id])
	ALTER TABLE [dbo].[station_inventory_manifest_staging] CHECK CONSTRAINT [FK_station_inventory_manifest_staging_dayparts]

	ALTER TABLE [dbo].[station_inventory_manifest_staging]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_staging_rate_spot_lengths] FOREIGN KEY([rate_spot_length_id])
	REFERENCES [dbo].[spot_lengths] ([id])
	ALTER TABLE [dbo].[station_inventory_manifest_staging] CHECK CONSTRAINT [FK_station_inventory_manifest_staging_rate_spot_lengths]

	CREATE INDEX IX_station_inventory_manifest_staging_manifest_spot_length_id ON [station_inventory_manifest_staging] ([manifest_spot_length_id])
	CREATE INDEX IX_station_inventory_manifest_staging_file_id ON [station_inventory_manifest_staging] ([file_id])
	CREATE INDEX IX_station_inventory_manifest_staging_inventory_source_id ON [station_inventory_manifest_staging] ([inventory_source_id])
	CREATE INDEX IX_station_inventory_manifest_staging_audience_id ON [station_inventory_manifest_staging] ([audience_id])
	CREATE INDEX IX_station_inventory_manifest_staging_daypart_id ON [station_inventory_manifest_staging] ([daypart_id])
	CREATE INDEX IX_station_inventory_manifest_staging_rate_spot_length_id ON [station_inventory_manifest_staging] ([rate_spot_length_id])
END
/*************************************** END BCOP-3534 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.01.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '18.12.1' -- Previous release version
		OR [version] = '19.01.1') -- Current release version
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