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


/*************************************** START PRI-17866 *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'job_group_id'
          AND Object_ID = Object_ID(N'inventory_programs_by_source_jobs')) 
BEGIN 
	ALTER TABLE inventory_programs_by_source_jobs
		ADD job_group_id UNIQUEIDENTIFIER NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'status_message'
          AND Object_ID = Object_ID(N'inventory_programs_by_source_jobs')) 
BEGIN 
	ALTER TABLE inventory_programs_by_source_jobs
		ADD status_message varchar(200) NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'status_message'
          AND Object_ID = Object_ID(N'inventory_programs_by_file_jobs')) 
BEGIN 
	ALTER TABLE inventory_programs_by_file_jobs
		ADD status_message varchar(200) NULL
END

GO

/*************************************** END PRI-17866 *****************************************************/

/*************************************** START PRI-21024 *****************************************************/
IF OBJECT_ID('station_mappings') IS NULL
BEGIN
	CREATE TABLE [dbo].[station_mappings](
		[id] int IDENTITY(1,1) NOT NULL,
		[station_id] int NOT NULL,
		[mapped_call_letters] [varchar](15) NOT NULL,
		[map_set] int NOT NULL,
		[created_date] datetime2(7) NOT NULL,
		[created_by] varchar(63) NOT NULL,
	 CONSTRAINT [PK_station_mappings] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[station_mappings]  WITH CHECK ADD CONSTRAINT [FK_station_mappings_stations] FOREIGN KEY([station_id])
	REFERENCES [dbo].[stations] ([id])
END

IF OBJECT_ID('station_month_details') IS NULL
BEGIN
	CREATE TABLE [dbo].[station_month_details](
		[station_id] [int] NOT NULL,
		[media_month_id] [int] NOT NULL,
		[affiliation] [varchar](7) NULL,
		[market_code] [smallint] NULL,
		[distributor_code] [int] NULL,
	CONSTRAINT [PK_station_month_details] PRIMARY KEY CLUSTERED 
	(
		[station_id] ASC, [media_month_id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[station_month_details]  WITH CHECK ADD CONSTRAINT [FK_station_month_details_stations] FOREIGN KEY([station_id])
	REFERENCES [dbo].[stations] ([id])

	ALTER TABLE [dbo].[station_month_details]  WITH CHECK ADD CONSTRAINT [FK_station_month_details_media_months] FOREIGN KEY([media_month_id])
	REFERENCES [dbo].[media_months] ([id])

	ALTER TABLE [dbo].[station_month_details]  WITH CHECK ADD CONSTRAINT [FK_station_month_details_markets] FOREIGN KEY([market_code])
	REFERENCES [dbo].[markets] ([market_code])

	ALTER TABLE [dbo].[station_month_details]
	ADD CONSTRAINT [UQ_station_month_details_station_id_station_month_details_media_month_id] 
	UNIQUE ([station_id], [media_month_id])
END
/*************************************** END PRI-21024 *****************************************************/

/*************************************** START PRI-20833 *****************************************************/
IF OBJECT_ID('plan_version_pricing_job_inventory_source_estimates') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_pricing_job_inventory_source_estimates](
		[id] int IDENTITY(1,1) NOT NULL,
		[media_week_id] int NOT NULL,
		[inventory_source_id] int NOT NULL,
		[plan_version_pricing_job_id] int NOT NULL,
		[impressions] float NOT NULL,
		[cost] money NOT NULL
	 CONSTRAINT [PK_plan_version_pricing_job_inventory_source_estimates] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_pricing_job_inventory_source_estimates]
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_job_inventory_source_estimates_media_weeks] 
	FOREIGN KEY([media_week_id])
	REFERENCES [dbo].[media_weeks] ([id])

	ALTER TABLE [dbo].[plan_version_pricing_job_inventory_source_estimates] CHECK CONSTRAINT [FK_plan_version_pricing_job_inventory_source_estimates_media_weeks]


	ALTER TABLE [dbo].[plan_version_pricing_job_inventory_source_estimates]
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_job_inventory_source_estimates_inventory_sources] 
	FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])

	ALTER TABLE [dbo].[plan_version_pricing_job_inventory_source_estimates] CHECK CONSTRAINT [FK_plan_version_pricing_job_inventory_source_estimates_inventory_sources]

	ALTER TABLE [dbo].[plan_version_pricing_job_inventory_source_estimates]
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_job_inventory_source_estimates_plan_version_pricing_job] 
	FOREIGN KEY([plan_version_pricing_job_id])
	REFERENCES [dbo].[plan_version_pricing_job] ([id])

	ALTER TABLE [dbo].[plan_version_pricing_job_inventory_source_estimates] CHECK CONSTRAINT [FK_plan_version_pricing_job_inventory_source_estimates_plan_version_pricing_job]
END
/*************************************** END PRI-20833 *****************************************************/

/*************************************** START - PRI-22728 *****************************************************/
GO

IF ((SELECT CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE 
     TABLE_NAME = 'plan_version_pricing_job' AND 
     COLUMN_NAME = 'error_message') = 2000)
BEGIN
	ALTER TABLE plan_version_pricing_job
	ALTER COLUMN error_message nvarchar(max) NULL
END

GO
/*************************************** END - PRI-22728 *****************************************************/

/*************************************** START - PRI-20833 PART 2 ****************************************************/
-- make inventory_source_id to be nullable
IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_job_inventory_source_estimates') AND name = 'inventory_source_id')
BEGIN
	ALTER TABLE plan_version_pricing_job_inventory_source_estimates ALTER COLUMN inventory_source_id INT NULL
END

-- add nullable inventory_source_type
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_job_inventory_source_estimates') AND name = 'inventory_source_type')
BEGIN
	ALTER TABLE plan_version_pricing_job_inventory_source_estimates ADD inventory_source_type INT NULL
END

/*************************************** END - PRI-20833 PART 2 ****************************************************/

/*************************************** START PRI-20083 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'primary_program_id' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_dayparts'))
BEGIN
    ALTER TABLE station_inventory_manifest_dayparts ADD primary_program_id int NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'genre_id' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_daypart_programs'))
BEGIN
    EXEC sp_rename 'dbo.station_inventory_manifest_daypart_programs.genre_id', 'source_genre_id', 'COLUMN';
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'genre_source_id' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_daypart_programs'))
BEGIN
	-- add a nullable column
    ALTER TABLE station_inventory_manifest_daypart_programs ADD genre_source_id int NULL

	-- add a FK constraint
	ALTER TABLE station_inventory_manifest_daypart_programs WITH CHECK ADD CONSTRAINT FK_station_inventory_manifest_daypart_programs_genre_sources
	FOREIGN KEY(genre_source_id)
	REFERENCES genre_sources(id)
	ALTER TABLE station_inventory_manifest_daypart_programs CHECK CONSTRAINT FK_station_inventory_manifest_daypart_programs_genre_sources

	-- set Dativa genre source id to all records
	EXEC('update station_inventory_manifest_daypart_programs set genre_source_id = 2')

	-- update the column to be not nullable
	ALTER TABLE station_inventory_manifest_daypart_programs ALTER COLUMN genre_source_id int NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'maestro_genre_id' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_daypart_programs'))
BEGIN
	-- add a nullable column
    ALTER TABLE station_inventory_manifest_daypart_programs ADD maestro_genre_id int NULL

	-- add a FK constraint
	ALTER TABLE station_inventory_manifest_daypart_programs WITH CHECK ADD CONSTRAINT FK_station_inventory_manifest_daypart_programs_genres_maestro
	FOREIGN KEY(maestro_genre_id)
	REFERENCES genres(id)
	ALTER TABLE station_inventory_manifest_daypart_programs CHECK CONSTRAINT FK_station_inventory_manifest_daypart_programs_genres_maestro

	-- set values based on the source_genre_id
	EXEC('
	update station_inventory_manifest_daypart_programs
	set maestro_genre_id = (select top 1 gm.maestro_genre_id
							from genres as g
							join genre_mappings as gm on g.id = gm.mapped_genre_id
							where g.source_id = 2 and gm.mapped_genre_id = source_genre_id)
	')

	-- update the column to be not nullable
	ALTER TABLE station_inventory_manifest_daypart_programs ALTER COLUMN maestro_genre_id int NOT NULL
END
/*************************************** END PRI-20083 *****************************************************/

/*************************************** START PRI-23440 *****************************************************/

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'media_week_id' AND OBJECT_ID = OBJECT_ID(N'plan_version_pricing_api_result_spots'))
BEGIN
    EXEC sp_rename 'dbo.plan_version_pricing_api_result_spots.media_week_id', 'inventory_media_week_id', 'COLUMN';
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'contract_media_week_id' AND OBJECT_ID = OBJECT_ID(N'plan_version_pricing_api_result_spots'))
BEGIN
    EXEC('ALTER TABLE plan_version_pricing_api_result_spots ADD contract_media_week_id int NULL')

	EXEC('UPDATE plan_version_pricing_api_result_spots SET contract_media_week_id = inventory_media_week_id')

	EXEC('ALTER TABLE plan_version_pricing_api_result_spots ALTER COLUMN contract_media_week_id INT NOT NULL')

	EXEC('
	ALTER TABLE plan_version_pricing_api_result_spots
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_api_result_spots_media_weeks1] 
	FOREIGN KEY([contract_media_week_id])
	REFERENCES media_weeks ([id])')

	EXEC('
	ALTER TABLE plan_version_pricing_api_result_spots CHECK CONSTRAINT [FK_plan_version_pricing_api_result_spots_media_weeks1]')
END

/*************************************** END PRI-23440 *****************************************************/

/*************************************** START PRI-23251 *****************************************************/

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE 
	object_id = OBJECT_ID(N'dbo.FK_plan_version_pricing_api_result_spots_dayparts') AND 
	parent_object_id = OBJECT_ID(N'dbo.plan_version_pricing_api_result_spots')
)
BEGIN
  ALTER TABLE dbo.plan_version_pricing_api_result_spots DROP CONSTRAINT FK_plan_version_pricing_api_result_spots_dayparts
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'daypart_id' AND Object_ID = Object_ID(N'plan_version_pricing_api_result_spots')) 
BEGIN 
	ALTER TABLE plan_version_pricing_api_result_spots DROP COLUMN daypart_id
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'standard_daypart_id' AND Object_ID = Object_ID(N'plan_version_pricing_api_result_spots')) 
BEGIN 
	ALTER TABLE plan_version_pricing_api_result_spots ADD standard_daypart_id int NULL

	ALTER TABLE plan_version_pricing_api_result_spots WITH CHECK ADD CONSTRAINT FK_plan_version_pricing_api_result_spots_daypart_defaults
	FOREIGN KEY(standard_daypart_id)
	REFERENCES daypart_defaults(id)
	ALTER TABLE plan_version_pricing_api_result_spots CHECK CONSTRAINT FK_plan_version_pricing_api_result_spots_daypart_defaults

	-- set any id since this data is not really used at the moment and pricing can be rerun
	EXEC('UPDATE plan_version_pricing_api_result_spots SET standard_daypart_id = (SELECT TOP 1 id FROM daypart_defaults)')

	ALTER TABLE plan_version_pricing_api_result_spots ALTER COLUMN standard_daypart_id int NOT NULL
END

/*************************************** END PRI-23251 *****************************************************/

/*************************************** START PRI-23934 *****************************************************/
BEGIN
    EXEC('
		DELETE FROM station_mappings
		WHERE station_id IN (
		  SELECT s.id FROM stations AS s WHERE s.legacy_call_letters IN (''KWAB'', ''KCWQ'', ''KFPH'', ''KFT'',
		   ''KFTU'', ''KFVE'', ''KNEP'', ''NNEP'', ''KAME'', ''OAME'', ''QOLO'', ''KALS'', ''KUV'', ''MWES'', ''NWES'', ''KWYB'', ''NBYW'', ''NFXF'',
		   ''WMYO'', ''EBND'', ''DBRC'', ''EBRC'', ''WYCN'', ''ECSC'', ''CW4'', ''EDFX'', ''GDJT'', ''HDJT'', ''KJWP'', ''EETM'', ''HETM'', ''HGBX'', ''WGX2'',
		   ''DHAM'', ''EHBQ'', ''GHLT'', ''GIAT'', ''EIS'', ''GIS'', ''HIS'', ''DLBT'', ''GLBT'', ''EMEB'', ''IMEB'', ''DMEU'', ''DPGA'', ''EPGA'', ''EPGX'',
		   ''DPGX'', ''QPNE'', ''GSAV'', ''GSBT'', ''WGSA'', ''HSCG'', ''ETHR'', ''GTHR'', ''DTOC'', ''ETOC'', ''ETVQ'', ''GTVQ'', ''WTW2'', ''WLHG'')
		  )
	')
END

GO

/*************************************** END PRI-23934 *****************************************************/

/*************************************** START PRI-23136 *****************************************************/

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_station_inventory_manifest_file_id' AND object_id = OBJECT_ID('[dbo].[station_inventory_manifest]'))
BEGIN
      CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_file_id] ON [dbo].[station_inventory_manifest]
	  (
		[file_id] ASC
      )
	  INCLUDE ([spot_length_id], [spots_per_week], [inventory_source_id], [station_inventory_group_id], [spots_per_day], [station_id], [comment]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]  
END

GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_station_inventory_manifest_weeks_media_week_id' AND object_id = OBJECT_ID('[dbo].[station_inventory_manifest_weeks]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_weeks_media_week_id] ON [dbo].[station_inventory_manifest_weeks]
	(
		[media_week_id] ASC
	) 
	INCLUDE ([station_inventory_manifest_id], [spots], [start_date], [end_date], [sys_start_date], [sys_end_date])WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_station_mappings_station_id' AND object_id = OBJECT_ID('[dbo].[station_mappings]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_station_mappings_station_id] ON [dbo].[station_mappings]
	(
		[station_id] ASC
	) 
	INCLUDE ([mapped_call_letters], [map_set], [created_date], [created_by])WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_dayparts_timespan_id' AND object_id = OBJECT_ID('[dbo].[dayparts]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_dayparts_timespan_id] ON [dbo].[dayparts]
	(
		[timespan_id] ASC
	) 
	INCLUDE ([code], [name], [tier], [daypart_text], [total_hours])WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_stations_market_code' AND object_id = OBJECT_ID('[dbo].[stations]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_stations_market_code] ON [dbo].[stations]
	(
		[market_code] ASC
	) 
	INCLUDE ([station_code], [station_call_letters], [affiliation], [legacy_call_letters], [modified_date])WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_station_inventory_manifest_weeks_start_date1' AND object_id = OBJECT_ID('[dbo].[station_inventory_manifest_weeks]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_weeks_start_date1] ON [dbo].[station_inventory_manifest_weeks]
	(
		[start_date] ASC, [end_date]ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_station_inventory_manifest_weeks_start_date2' AND object_id = OBJECT_ID('[dbo].[station_inventory_manifest_weeks]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_weeks_start_date2] ON [dbo].[station_inventory_manifest_weeks] ([start_date],[end_date])
	INCLUDE ([station_inventory_manifest_id])
END

GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_station_inventory_manifest_audiences_audience_id_ALL' AND object_id = OBJECT_ID('[dbo].[station_inventory_manifest_audiences]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_audiences_audience_id_ALL] ON [dbo].[station_inventory_manifest_audiences]
	(
		[station_inventory_manifest_id],[audience_id],[impressions],[cpm],[is_reference],[rating],[vpvh],[share_playback_type],[hut_playback_type] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_station_inventory_manifest_weeks_start_date3' AND object_id = OBJECT_ID('[dbo].[station_inventory_manifest_weeks]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_weeks_start_date3] ON [dbo].[station_inventory_manifest_weeks]
	( 
		[station_inventory_manifest_id] ASC, [start_date] ASC, [end_date] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_station_inventory_manifest_rates_station_inventory_manifest_id1' AND object_id = OBJECT_ID('[dbo].[station_inventory_manifest_rates]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_rates_station_inventory_manifest_id1] ON [dbo].[station_inventory_manifest_rates]
	(
		[station_inventory_manifest_id] ASC, [spot_length_id] ASC, [spot_cost]
	)
	INCLUDE (id) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

GO

/*************************************** END PRI-23136 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.04.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.03.1' -- Previous release version
		OR [version] = '20.04.1') -- Current release version
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