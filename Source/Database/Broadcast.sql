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


/*************************************** START - BP-51 ****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_result_spot_frequencies') AND name = 'impressions')
BEGIN
	ALTER TABLE plan_version_pricing_api_result_spot_frequencies
	ADD impressions float NULL

	EXEC('UPDATE f
	      SET f.impressions = s.impressions30sec
	      FROM plan_version_pricing_api_result_spot_frequencies f
	      JOIN plan_version_pricing_api_result_spots as s on f.plan_version_pricing_api_result_spot_id = s.id
	      JOIN plan_version_pricing_api_results as r on s.plan_version_pricing_api_results_id = r.id
	      WHERE r.pricing_version = ''2''')

	EXEC('UPDATE f
	      SET f.impressions = (s.impressions30sec * 
		case pv.equivalized
			when 0 then 1
			when 1 then sl.delivery_multiplier
		end)
	      FROM plan_version_pricing_api_result_spot_frequencies as f
		  JOIN plan_version_pricing_api_result_spots as s on f.plan_version_pricing_api_result_spot_id = s.id
		  JOIN plan_version_pricing_api_results as r on s.plan_version_pricing_api_results_id = r.id
		  JOIN plan_version_pricing_job as j on r.plan_version_pricing_job_id = j.id
		  JOIN plan_versions as pv on j.plan_version_id = pv.id
		  JOIN spot_lengths as sl on f.spot_length_id = sl.id
		  WHERE r.pricing_version = ''3''')

	ALTER TABLE plan_version_pricing_api_result_spot_frequencies
	ALTER COLUMN impressions float NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_api_result_spot_frequencies') AND name = 'impressions')
BEGIN
	ALTER TABLE plan_version_buying_api_result_spot_frequencies
	ADD impressions float NULL

	EXEC('UPDATE f
		  SET f.impressions = s.impressions30sec
		  FROM plan_version_buying_api_result_spot_frequencies f
		  JOIN plan_version_buying_api_result_spots as s on f.plan_version_buying_api_result_spot_id = s.id
		  JOIN plan_version_buying_api_results as r on s.plan_version_buying_api_results_id = r.id
		  WHERE r.buying_version = ''2''')

	EXEC('UPDATE f
	      SET f.impressions = (s.impressions30sec * 
		case pv.equivalized
			when 0 then 1
			when 1 then sl.delivery_multiplier
		end)
	      FROM plan_version_buying_api_result_spot_frequencies as f
		  JOIN plan_version_buying_api_result_spots as s on f.plan_version_buying_api_result_spot_id = s.id
		  JOIN plan_version_buying_api_results as r on s.plan_version_buying_api_results_id = r.id
		  JOIN plan_version_buying_job as j on r.plan_version_buying_job_id = j.id
		  JOIN plan_versions as pv on j.plan_version_id = pv.id
		  JOIN spot_lengths as sl on f.spot_length_id = sl.id
		  WHERE r.buying_version = ''3''')

	ALTER TABLE plan_version_buying_api_result_spot_frequencies
	ALTER COLUMN impressions float NOT NULL
END
/*************************************** END - BP-51 ****************************************************/
/*************************************** START BP-894 *******************************************************/

--add total market coverage percentage 
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_results') 
				AND name = 'total_market_coverage_percent')
BEGIN
	ALTER TABLE [plan_version_buying_results] ADD [total_market_coverage_percent] FLOAT NULL

	EXEC('UPDATE t1
	SET t1.total_market_coverage_percent = t2.total_coverage_percent
	FROM [plan_version_buying_results] AS t1
	INNER JOIN [plan_version_buying_markets] AS t2 ON t1.plan_version_buying_job_id = t2.plan_version_buying_job_id')

	ALTER TABLE [plan_version_buying_results] ALTER COLUMN [total_market_coverage_percent] FLOAT NOT NULL
END

--add plan_version_buying_result_id column and all related changes
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_market_details') 
		AND name = 'plan_version_buying_result_id')
BEGIN
	ALTER TABLE [plan_version_buying_market_details] ADD [plan_version_buying_result_id] INT NULL

	EXEC('UPDATE t1 
			SET  t1.plan_version_buying_result_id = t3.id
			FROM plan_version_buying_market_details AS t1
			INNER JOIN plan_version_buying_markets AS t2 ON t1.plan_version_buying_market_id = t2.id
			INNER JOIN [plan_version_buying_results] AS t3 ON t2.plan_version_buying_job_id = t3.plan_version_buying_job_id')

	ALTER TABLE [plan_version_buying_market_details] ALTER COLUMN [plan_version_buying_result_id] INT NOT NULL

	--drop FK constraint on old column [plan_version_buying_market_id]
	IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_buying_market_details')
					AND name = 'FK_plan_version_buying_market_details_buying_market')
	BEGIN
		ALTER TABLE [plan_version_buying_market_details] DROP [FK_plan_version_buying_market_details_buying_market]
	END

	--drop old column plan_version_buying_market_id
	IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_market_details') 
		AND name = 'plan_version_buying_market_id')
	BEGIN
		ALTER TABLE [plan_version_buying_market_details] DROP COLUMN [plan_version_buying_market_id]
	END

	--add FK constraint on the new column plan_version_buying_result_id
	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID('plan_version_buying_market_details')
					AND name = 'FK_plan_version_buying_market_details_plan_version_buying_results')
	BEGIN
		ALTER TABLE [dbo].[plan_version_buying_market_details]
		ADD CONSTRAINT [FK_plan_version_buying_market_details_plan_version_buying_results] 
		FOREIGN KEY([plan_version_buying_result_id])
		REFERENCES [dbo].[plan_version_buying_results] ([id])
	END

	--drop table plan_version_buying_markets
	IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_markets'))
	BEGIN
		DROP TABLE [dbo].[plan_version_buying_markets]
	END
END

--add plan_version_buying_result_id column and all related changes
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_station_details') 
		AND name = 'plan_version_buying_result_id')
BEGIN
	ALTER TABLE [plan_version_buying_station_details] ADD [plan_version_buying_result_id] INT NULL

	EXEC('UPDATE t1 
			SET  t1.plan_version_buying_result_id = t3.id
			FROM plan_version_buying_station_details AS t1
			INNER JOIN plan_version_buying_stations AS t2 ON t1.plan_version_buying_station_id = t2.id
			INNER JOIN [plan_version_buying_results] AS t3 ON t2.plan_version_buying_job_id = t3.plan_version_buying_job_id')

	ALTER TABLE [plan_version_buying_station_details] ALTER COLUMN [plan_version_buying_result_id] INT NOT NULL

	--drop FK constraint on old column [plan_version_buying_station_id]
	IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_buying_station_details')
					AND name = 'FK_plan_version_buying_stations_plan_version_buying_station_details')
	BEGIN
		ALTER TABLE [plan_version_buying_station_details] DROP [FK_plan_version_buying_stations_plan_version_buying_station_details]
	END

	--drop old column plan_version_buying_station_id
	IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_station_details') 
		AND name = 'plan_version_buying_station_id')
	BEGIN
		ALTER TABLE [plan_version_buying_station_details] DROP COLUMN [plan_version_buying_station_id]
	END

	--add FK constraint on the new column plan_version_buying_result_id
	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID('plan_version_buying_station_details')
					AND name = 'FK_plan_version_buying_station_details_plan_version_buying_results')
	BEGIN
		ALTER TABLE [dbo].[plan_version_buying_station_details]
		ADD CONSTRAINT [FK_plan_version_buying_station_details_plan_version_buying_results] 
		FOREIGN KEY([plan_version_buying_result_id])
		REFERENCES [dbo].[plan_version_buying_results] ([id])
	END

	--drop table plan_version_buying_stations
	IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_stations'))
	BEGIN
		DROP TABLE [dbo].[plan_version_buying_stations]
	END
END

--add plan_version_buying_result_id column and all related changes
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_band_details') 
		AND name = 'plan_version_buying_result_id')
BEGIN
	ALTER TABLE [plan_version_buying_band_details] ADD [plan_version_buying_result_id] INT NULL

	EXEC('UPDATE t1 
			SET  t1.plan_version_buying_result_id = t3.id
			FROM plan_version_buying_band_details AS t1
			INNER JOIN plan_version_buying_bands AS t2 ON t1.plan_version_buying_band_id = t2.id
			INNER JOIN [plan_version_buying_results] AS t3 ON t2.plan_version_buying_job_id = t3.plan_version_buying_job_id')

	ALTER TABLE [plan_version_buying_band_details] ALTER COLUMN [plan_version_buying_result_id] INT NOT NULL

	--drop FK constraint on old column [plan_version_buying_band_id]
	IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_buying_band_details')
					AND name = 'FK_plan_version_buying_band_details_plan_version_buying_bands')
	BEGIN
		ALTER TABLE [plan_version_buying_band_details] DROP [FK_plan_version_buying_band_details_plan_version_buying_bands]
	END

	--drop old column plan_version_buying_band_id
	IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_band_details') 
		AND name = 'plan_version_buying_band_id')
	BEGIN
		ALTER TABLE [plan_version_buying_band_details] DROP COLUMN [plan_version_buying_band_id]
	END

	--add FK constraint on the new column plan_version_buying_result_id
	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID('plan_version_buying_band_details')
					AND name = 'FK_plan_version_buying_band_details_plan_version_buying_results')
	BEGIN
		ALTER TABLE [dbo].[plan_version_buying_band_details]
		ADD CONSTRAINT [FK_plan_version_buying_band_details_plan_version_buying_results] 
		FOREIGN KEY([plan_version_buying_result_id])
		REFERENCES [dbo].[plan_version_buying_results] ([id])
	END

	--drop table plan_version_buying_bands
	IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_bands'))
	BEGIN
		DROP TABLE [dbo].[plan_version_buying_bands]
	END
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_ownership_group_details'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_ownership_group_details](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_result_id] [int] NOT NULL,	
		[ownership_group_name] [NVARCHAR] (100) NOT NULL,
		[markets] [int] NOT NULL,
		[stations] [int] NOT NULL,
		[spots] [int] NOT NULL,
		[impressions] [float] NOT NULL,
		[cpm] [money] NOT NULL,
		[budget] [money] NOT NULL,
		[impressions_percentage] [float] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_ownership_group_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_ownership_group_details] ADD CONSTRAINT [FK_plan_version_buying_ownership_group_details_plan_version_buying_results] FOREIGN KEY([plan_version_buying_result_id])
	REFERENCES [dbo].[plan_version_buying_results] ([id])
END
/*************************************** END BP-894 *******************************************************/

GO

/*************************************** START BP-898 *******************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_rep_firm_details'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_rep_firm_details](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_result_id] [int] NOT NULL,	
		[rep_firm_name] [NVARCHAR] (100) NOT NULL,
		[markets] [int] NOT NULL,
		[stations] [int] NOT NULL,
		[spots] [int] NOT NULL,
		[impressions] [float] NOT NULL,
		[cpm] [money] NOT NULL,
		[budget] [money] NOT NULL,
		[impressions_percentage] [float] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_rep_firm_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
	ALTER TABLE [dbo].[plan_version_buying_rep_firm_details] ADD CONSTRAINT [FK_plan_version_buying_rep_firm_details_plan_version_buying_results] FOREIGN KEY([plan_version_buying_result_id])
	REFERENCES [dbo].[plan_version_buying_results] ([id])
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('stations') AND name = 'sales_group_name')
BEGIN
	EXEC sp_rename [dbo.stations.sales_group_name], [rep_firm_name], 'COLUMN'
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_market_details') AND name = 'market_name')
BEGIN
	ALTER TABLE plan_version_buying_market_details
	ADD market_name varchar(31) NULL
	EXEC('UPDATE d
		  SET d.market_name = m.geography_name
		  FROM plan_version_buying_market_details d
		  JOIN market_coverages mc on mc.percentage_of_us = d.market_coverage_percent and mc.rank = d.rank
		  JOIN markets m on mc.market_code = m.market_code')
	EXEC('UPDATE plan_version_buying_market_details
		  SET market_name = ''Unknown Market''
		  WHERE market_name is NULL')
    ALTER TABLE plan_version_buying_market_details
	ALTER COLUMN market_name varchar(31) NOT NULL
END
/*************************************** END BP-898 *******************************************************/

GO

/******************************** START BP-1088-2 *********************************************************/

-- Drop existing table inventory_proprietary_program_names as we are splitting it into two tables
IF EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('inventory_proprietary_program_names'))
BEGIN
	DROP TABLE dbo.inventory_proprietary_program_names
END
  
-- Create table inventory_proprietary_lookup
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('inventory_proprietary_daypart_programs'))
BEGIN
	CREATE TABLE [dbo].[inventory_proprietary_daypart_programs]
	(
		[id] [int] IDENTITY(1,1) NOT NULL,
		[unit_type] [varchar](50) NOT NULL,
		[program_name] [varchar](150) NOT NULL,
		[created_by] [varchar](63) NOT NULL,
		[created_at] [datetime] NOT NULL,
		[modified_by] [varchar](63) NULL,
		[modified_at] [datetime] NULL,
		CONSTRAINT [PK_inventory_proprietary_daypart_programs] PRIMARY KEY CLUSTERED 
		(
		[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	 ) ON [PRIMARY]	
	 
	-- Seed data for inventory_proprietary_daypart_programs
	
	INSERT INTO [dbo].[inventory_proprietary_daypart_programs]([unit_type], [program_name], [created_by], [created_at], [modified_by], [modified_at])
	VALUES('AM', 'Early Morning News', 'Admin', GETDATE(), 'Admin', GETDATE())

	INSERT INTO [dbo].[inventory_proprietary_daypart_programs]([unit_type], [program_name], [created_by], [created_at], [modified_by], [modified_at])
	VALUES('PM', 'Evening/Late News', 'Admin', GETDATE(), 'Admin', GETDATE())

	INSERT INTO [dbo].[inventory_proprietary_daypart_programs]([unit_type], [program_name], [created_by], [created_at], [modified_by], [modified_at])
	VALUES('News', 'Early Morning News','Admin', GETDATE(), 'Admin', GETDATE())

	INSERT INTO [dbo].[inventory_proprietary_daypart_programs]([unit_type], [program_name], [created_by], [created_at], [modified_by], [modified_at])
	VALUES('Syndication', 'ROS Syndication', 'Admin', GETDATE(), 'Admin', GETDATE())
END

-- Create table inventory_proprietary_daypart_program_mappings
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('inventory_proprietary_daypart_program_mappings'))
BEGIN	
	CREATE TABLE [dbo].[inventory_proprietary_daypart_program_mappings]
	(
		[id] [int] IDENTITY(1,1) NOT NULL,
		[inventory_source_id] [int] NOT NULL,
		[daypart_default_id] [int] NOT NULL,
		[inventory_proprietary_daypart_programs_id] [int] NOT NULL,	       
		[created_by] [varchar](63) NOT NULL,
		[created_at] [datetime] NOT NULL,
		[modified_by] [varchar](63) NULL,
		[modified_at] [datetime] NULL,
		CONSTRAINT [PK_inventory_proprietary_daypart_program_mappings] PRIMARY KEY CLUSTERED 
		(
		[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	 ) ON [PRIMARY]	
	 
	ALTER TABLE [dbo].[inventory_proprietary_daypart_program_mappings] ADD CONSTRAINT [FK_inventory_proprietary_daypart_program_mappings_inventory_proprietary_daypart_programs] FOREIGN KEY([inventory_proprietary_daypart_programs_id])
	REFERENCES [dbo].[inventory_proprietary_daypart_programs] ([id])
	
	EXEC ('ALTER TABLE [dbo].[inventory_proprietary_daypart_program_mappings]  WITH CHECK ADD  CONSTRAINT [FK_inventory_proprietary_daypart_program_mappings_daypart_defaults] FOREIGN KEY([daypart_default_id])
	REFERENCES [dbo].[daypart_defaults] ([id])')
	
	ALTER TABLE [dbo].[inventory_proprietary_daypart_program_mappings]  WITH CHECK ADD  CONSTRAINT [FK_inventory_proprietary_daypart_program_mappings_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	
	-- Seed data for [inventory_proprietary_daypart_program_mappings]
	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (5,1,1,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (5,2,1,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (5,15,1,''Admin'',GETDATE(),''Admin'',GETDATE())''')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (5,3,2,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (5,4,2,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (5,5,2,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (5,16,2,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,1,3,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,2,3,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES(3,3,3,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,5,3,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,15,3,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,16,3,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,17,3,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,6,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,7,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES(3,8,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,9,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,10,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,11,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,12,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,14,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,19,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,20,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings]
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,21,4,''Admin'',GETDATE(),''Admin'',GETDATE())')

	EXEC ('INSERT INTO [dbo].[inventory_proprietary_daypart_program_mappings] 
	([inventory_source_id],[daypart_default_id],[inventory_proprietary_daypart_programs_id],[created_by],[created_at],[modified_by],[modified_at])
	VALUES (3,22,4,''Admin'',GETDATE(),''Admin'',GETDATE())')
 END
 
 -- Add mapping table id into inventory_proprietary_summary table
IF NOT EXISTS(SELECT 1 FROM sys.columns 
			  WHERE object_id = OBJECT_ID('inventory_proprietary_summary') AND 
			        name = 'inventory_proprietary_daypart_program_mappings_id')
BEGIN
	DELETE FROM [inventory_proprietary_summary_markets]
	DELETE FROM [inventory_proprietary_summary_audiences]
	DELETE FROM [inventory_proprietary_summary]
	
	ALTER TABLE [inventory_proprietary_summary] ADD [inventory_proprietary_daypart_program_mappings_id] INT NOT NULL	

	ALTER TABLE [inventory_proprietary_summary] WITH CHECK ADD  CONSTRAINT [FK_inventory_proprietary_summary_inventory_proprietary_daypart_program_mappings] FOREIGN KEY([inventory_proprietary_daypart_program_mappings_id])
	REFERENCES [dbo].inventory_proprietary_daypart_program_mappings ([id])
END
	
-- Add new column spot_cost into inventory_proprietary_summary table
IF NOT EXISTS(SELECT 1 FROM sys.columns 
		      WHERE object_id = OBJECT_ID('inventory_proprietary_summary') AND 
				    name = 'unit_cost')
BEGIN
	ALTER TABLE [inventory_proprietary_summary] ADD [unit_cost] money NOT NULL
END
	
-- Remove foreign key for default_daypart from inventory_proprietary_summary table as we are dropping that column
IF EXISTS (SELECT 1 FROM sys.foreign_keys 
		   WHERE parent_object_id = OBJECT_ID('inventory_proprietary_summary') AND 
		         name = 'FK_inventory_proprietary_summary_daypart_defaults')
BEGIN
	ALTER TABLE [inventory_proprietary_summary] DROP [FK_inventory_proprietary_summary_daypart_defaults]
END

IF EXISTS (SELECT 1 FROM sys.columns 
		   WHERE object_id = OBJECT_ID('inventory_proprietary_summary') AND 
			     name = 'daypart_default_id')
BEGIN
	EXEC ('ALTER TABLE [inventory_proprietary_summary] DROP COLUMN [daypart_default_id]')
END
	
IF EXISTS (SELECT 1 FROM sys.columns 
		   WHERE object_id = OBJECT_ID('inventory_proprietary_summary') AND 
		         name = 'cpm')
BEGIN
	ALTER TABLE [inventory_proprietary_summary] DROP COLUMN [cpm]
END

GO
/********************************END BP-1088-2********************************************************************/

/*************************************** START - BP-811 ****************************************************/

IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_pricing_job_inventory_source_estimates'))
BEGIN
	EXEC('drop table plan_version_pricing_job_inventory_source_estimates')
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_job_inventory_source_estimates'))
BEGIN
	EXEC('drop table plan_version_buying_job_inventory_source_estimates')
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_pricing_parameters_inventory_source_percentages'))
BEGIN
	EXEC('drop table plan_version_pricing_parameters_inventory_source_percentages')
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_parameters_inventory_source_percentages'))
BEGIN
	EXEC('drop table plan_version_buying_parameters_inventory_source_percentages')
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_pricing_parameters_inventory_source_type_percentages'))
BEGIN
	EXEC('drop table plan_version_pricing_parameters_inventory_source_type_percentages')
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_parameters_inventory_source_type_percentages'))
BEGIN
	EXEC('drop table plan_version_buying_parameters_inventory_source_type_percentages')
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_pricing_parameter_inventory_proprietary_summaries'))
BEGIN
	CREATE TABLE [dbo].[plan_version_pricing_parameter_inventory_proprietary_summaries](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_pricing_parameter_id] [int] NOT NULL,	
		[inventory_proprietary_summary_id] [int] NOT NULL,	
	 CONSTRAINT [PK_plan_version_pricing_parameter_inventory_proprietary_summaries] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_pricing_parameter_inventory_proprietary_summaries] 
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_parameter_inventory_proprietary_summaries_plan_version_pricing_parameters] 
	FOREIGN KEY([plan_version_pricing_parameter_id])
	REFERENCES [dbo].[plan_version_pricing_parameters] ([id])

	ALTER TABLE [dbo].[plan_version_pricing_parameter_inventory_proprietary_summaries] 
	CHECK CONSTRAINT [FK_plan_version_pricing_parameter_inventory_proprietary_summaries_plan_version_pricing_parameters]

	ALTER TABLE [dbo].[plan_version_pricing_parameter_inventory_proprietary_summaries] 
	ADD CONSTRAINT [FK_plan_version_pricing_parameter_inventory_proprietary_summaries_inventory_proprietary_summary] 
	FOREIGN KEY([inventory_proprietary_summary_id])
	REFERENCES [dbo].[inventory_proprietary_summary] ([id])

	ALTER TABLE [dbo].[plan_version_pricing_parameter_inventory_proprietary_summaries] 
	CHECK CONSTRAINT [FK_plan_version_pricing_parameter_inventory_proprietary_summaries_inventory_proprietary_summary]
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_parameter_inventory_proprietary_summaries'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_parameter_inventory_proprietary_summaries](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_parameter_id] [int] NOT NULL,	
		[inventory_proprietary_summary_id] [int] NOT NULL,	
	 CONSTRAINT [PK_plan_version_buying_parameter_inventory_proprietary_summaries] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_parameter_inventory_proprietary_summaries] 
	WITH CHECK ADD CONSTRAINT [FK_plan_version_buying_parameter_inventory_proprietary_summaries_plan_version_buying_parameters] 
	FOREIGN KEY([plan_version_buying_parameter_id])
	REFERENCES [dbo].[plan_version_buying_parameters] ([id])

	ALTER TABLE [dbo].[plan_version_buying_parameter_inventory_proprietary_summaries] 
	CHECK CONSTRAINT [FK_plan_version_buying_parameter_inventory_proprietary_summaries_plan_version_buying_parameters]

	ALTER TABLE [dbo].[plan_version_buying_parameter_inventory_proprietary_summaries] 
	ADD CONSTRAINT [FK_plan_version_buying_parameter_inventory_proprietary_summaries_inventory_proprietary_summary] 
	FOREIGN KEY([inventory_proprietary_summary_id])
	REFERENCES [dbo].[inventory_proprietary_summary] ([id])

	ALTER TABLE [dbo].[plan_version_buying_parameter_inventory_proprietary_summaries] 
	CHECK CONSTRAINT [FK_plan_version_buying_parameter_inventory_proprietary_summaries_inventory_proprietary_summary]
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('inventory_proprietary_summary_audience_markets'))
BEGIN
	CREATE TABLE [dbo].[inventory_proprietary_summary_audience_markets](
		[id] int IDENTITY(1,1) NOT NULL,	
		[inventory_proprietary_summary_id] int NOT NULL,
		[audience_id] int NOT NULL,
		[market_code] smallint NOT NULL,
		[impressions] float NOT NULL
	 CONSTRAINT [PK_inventory_proprietary_summary_audience_markets] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_proprietary_summary_audience_markets] 
	ADD CONSTRAINT [FK_inventory_proprietary_summary_audience_markets_inventory_proprietary_summary] 
	FOREIGN KEY([inventory_proprietary_summary_id])
	REFERENCES [dbo].[inventory_proprietary_summary] ([id])

	ALTER TABLE [dbo].[inventory_proprietary_summary_audience_markets] 
	CHECK CONSTRAINT [FK_inventory_proprietary_summary_audience_markets_inventory_proprietary_summary]

	ALTER TABLE [dbo].[inventory_proprietary_summary_audience_markets] 
	ADD CONSTRAINT [FK_inventory_proprietary_summary_audience_markets_audiences] 
	FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])

	ALTER TABLE [dbo].[inventory_proprietary_summary_audience_markets] 
	CHECK CONSTRAINT [FK_inventory_proprietary_summary_audience_markets_audiences]

	ALTER TABLE [dbo].[inventory_proprietary_summary_audience_markets] 
	ADD CONSTRAINT [FK_inventory_proprietary_summary_audience_markets_markets] 
	FOREIGN KEY([market_code])
	REFERENCES [dbo].[markets] ([market_code])

	ALTER TABLE [dbo].[inventory_proprietary_summary_audience_markets] 
	CHECK CONSTRAINT [FK_inventory_proprietary_summary_audience_markets_markets]
END

IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('inventory_proprietary_summary_markets'))
BEGIN
	EXEC('drop table inventory_proprietary_summary_markets')
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_proprietary_summary_audiences') AND name = 'created_by')
BEGIN
	EXEC('ALTER TABLE [inventory_proprietary_summary_audiences] DROP COLUMN created_by')
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_proprietary_summary_audiences') AND name = 'created_at')
BEGIN
	EXEC('ALTER TABLE [inventory_proprietary_summary_audiences] DROP COLUMN created_at')
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_proprietary_summary_audiences') AND name = 'modified_by')
BEGIN
	EXEC('ALTER TABLE [inventory_proprietary_summary_audiences] DROP COLUMN modified_by')
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_proprietary_summary_audiences') AND name = 'modified_at')
BEGIN
	EXEC('ALTER TABLE [inventory_proprietary_summary_audiences] DROP COLUMN modified_at')
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_proprietary_summary') AND name = 'modified_by')
BEGIN
	EXEC('ALTER TABLE [inventory_proprietary_summary] DROP COLUMN modified_by')
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_proprietary_summary') AND name = 'modified_at')
BEGIN
	EXEC('ALTER TABLE [inventory_proprietary_summary] DROP COLUMN modified_at')
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_proprietary_summary') AND name = 'is_active')
BEGIN
	ALTER TABLE inventory_proprietary_summary ADD is_active bit NULL

	EXEC('UPDATE inventory_proprietary_summary SET is_active = 0')

	ALTER TABLE inventory_proprietary_summary ALTER COLUMN is_active bit NOT NULL
END

/*************************************** END - BP-811 ****************************************************/

GO

/*************************************** START - BP-1341 ****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('inventory_proprietary_summary_station_audiences'))
BEGIN
	CREATE TABLE [dbo].[inventory_proprietary_summary_station_audiences](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[inventory_proprietary_summary_id] [int] NOT NULL,	
		[audience_id] int NOT NULL,
		[market_code] smallint NOT NULL,
		[station_id] int NOT NULL,
		[impressions] float NOT NULL
	 CONSTRAINT [PK_inventory_proprietary_summary_station_audiences] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[inventory_proprietary_summary_station_audiences] 
	ADD CONSTRAINT [FK_inventory_proprietary_summary_station_audiences_inventory_proprietary_summary] 
	FOREIGN KEY([inventory_proprietary_summary_id])
	REFERENCES [dbo].[inventory_proprietary_summary] ([id])
	
	ALTER TABLE [dbo].[inventory_proprietary_summary_station_audiences] 
	ADD CONSTRAINT [FK_inventory_proprietary_summary_station_audiences_stations] 
	FOREIGN KEY([station_id])
	REFERENCES [dbo].[stations] ([id])
	
	ALTER TABLE [dbo].[inventory_proprietary_summary_station_audiences] 
	ADD CONSTRAINT [FK_inventory_proprietary_summary_station_audiences_markets] 
	FOREIGN KEY([market_code])
	REFERENCES [dbo].[markets] ([market_code])
	
	ALTER TABLE [dbo].[inventory_proprietary_summary_station_audiences] 
	ADD CONSTRAINT [FK_inventory_proprietary_summary_station_audiences_audiences] 
	FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])
END

IF EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('inventory_proprietary_summary_audience_markets'))
BEGIN
	DROP TABLE [inventory_proprietary_summary_audience_markets]
END

IF EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('inventory_proprietary_summary_audiences'))
BEGIN
	DROP TABLE [inventory_proprietary_summary_audiences]
END

/*************************************** END - BP-1341 ****************************************************/

/*************************************** START - BP-1286 ****************************************************/

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_proprietary_summary_station_audiences') AND name = 'spots_per_week')
BEGIN
	delete from inventory_proprietary_summary_station_audiences
	EXEC('ALTER TABLE inventory_proprietary_summary_station_audiences ADD spots_per_week int NOT NULL')
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_proprietary_summary_station_audiences') AND name = 'cost_per_week')
BEGIN
	delete from inventory_proprietary_summary_station_audiences
	EXEC('ALTER TABLE inventory_proprietary_summary_station_audiences ADD cost_per_week money NOT NULL')
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_market_details') AND name = 'is_proprietary ')
BEGIN
	ALTER TABLE plan_version_pricing_market_details ADD is_proprietary bit NULL

	EXEC('UPDATE plan_version_pricing_market_details SET is_proprietary = 0')

	ALTER TABLE plan_version_pricing_market_details ALTER COLUMN is_proprietary bit NOT NULL
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_market_details') AND name = 'cpm ')
BEGIN
	EXEC('ALTER TABLE plan_version_pricing_market_details DROP COLUMN cpm')
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_market_details') AND name = 'stations_per_market ')
BEGIN
	ALTER TABLE plan_version_pricing_market_details ADD stations_per_market int NULL

	EXEC('UPDATE plan_version_pricing_market_details SET stations_per_market = stations')

	ALTER TABLE plan_version_pricing_market_details ALTER COLUMN stations_per_market int NOT NULL
END

/*************************************** END - BP-1286 ****************************************************/

GO

/*************************************** START - BP-1283 ****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
			  WHERE object_id = OBJECT_ID('inventory_proprietary_daypart_programs') AND 
			        name = 'genre_id')
BEGIN
	ALTER TABLE inventory_proprietary_daypart_programs
	ADD genre_id INT NULL

	EXEC('UPDATE inventory_proprietary_daypart_programs
     	  SET genre_id = (select id from genres where name = ''News'' and program_source_id = 1)
	      WHERE unit_type = ''AM''')

	EXEC('UPDATE inventory_proprietary_daypart_programs
          SET genre_id = (select id from genres where name = ''News'' and program_source_id = 1)
	      WHERE unit_type = ''PM''')

	EXEC('UPDATE inventory_proprietary_daypart_programs
          SET genre_id = (select id from genres where name = ''News'' and program_source_id = 1)
	      WHERE unit_type = ''News''')

	EXEC('UPDATE inventory_proprietary_daypart_programs
          SET genre_id = (select id from genres where name = ''Entertainment'' and program_source_id = 1)
	      WHERE unit_type = ''Syndication''')

	ALTER TABLE inventory_proprietary_daypart_programs
	ALTER COLUMN genre_id INT NOT NULL

	ALTER TABLE inventory_proprietary_daypart_programs 
	WITH CHECK ADD CONSTRAINT FK_inventory_proprietary_daypart_programs_genres FOREIGN KEY(genre_id)
	REFERENCES [dbo].genres ([id])
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
			  WHERE object_id = OBJECT_ID('inventory_proprietary_daypart_programs') AND 
			        name = 'show_type_id')
BEGIN
	ALTER TABLE inventory_proprietary_daypart_programs
	ADD show_type_id INT NULL

	EXEC('UPDATE inventory_proprietary_daypart_programs
     	  SET show_type_id = (select id from show_types where name = ''News'' and program_source_id = 1)
	      WHERE unit_type = ''AM''')

	EXEC('UPDATE inventory_proprietary_daypart_programs
          SET show_type_id = (select id from show_types where name = ''News'' and program_source_id = 1)
	      WHERE unit_type = ''PM''')

	EXEC('UPDATE inventory_proprietary_daypart_programs
          SET show_type_id = (select id from show_types where name = ''News'' and program_source_id = 1)
	      WHERE unit_type = ''News''')

	EXEC('UPDATE inventory_proprietary_daypart_programs
          SET show_type_id = (select id from show_types where name = ''Miscellaneous'' and program_source_id = 1)
	      WHERE unit_type = ''Syndication''')

	ALTER TABLE inventory_proprietary_daypart_programs
	ALTER COLUMN show_type_id INT NOT NULL

	ALTER TABLE inventory_proprietary_daypart_programs 
	WITH CHECK ADD CONSTRAINT FK_inventory_proprietary_daypart_programs_show_types FOREIGN KEY(show_type_id)
	REFERENCES [dbo].show_types ([id])
END

/*************************************** END - BP-1283 ****************************************************/

GO

/*************************************** START - BP-1090 ****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_parameter_inventory_proprietary_summaries') AND name = 'unit_number')
BEGIN
	ALTER TABLE plan_version_pricing_parameter_inventory_proprietary_summaries
	ADD unit_number FLOAT NULL

	EXEC('UPDATE plan_version_pricing_parameter_inventory_proprietary_summaries
	SET unit_number = 1
	WHERE unit_number IS NULL')

	ALTER TABLE plan_version_pricing_parameter_inventory_proprietary_summaries
	ALTER COLUMN unit_number FLOAT NOT NULL
END
/*************************************** END - BP-1090 ****************************************************/

GO

/*************************************** START - BP-812 ****************************************************/

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_band_details') AND name = 'is_proprietary ')
BEGIN
	ALTER TABLE plan_version_pricing_band_details ADD is_proprietary bit NULL

	EXEC('UPDATE plan_version_pricing_band_details SET is_proprietary = 0')

	ALTER TABLE plan_version_pricing_band_details ALTER COLUMN is_proprietary bit NOT NULL
END

/*************************************** END - BP-812 ****************************************************/

GO

/*************************************** START BP-1467 *******************************************************/
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('daypart_defaults'))
BEGIN
	EXEC sp_rename [dbo.daypart_defaults],  [standard_dayparts]

	--rename columns
	IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_file_proprietary_header') AND name = 'daypart_default_id')
	BEGIN
		EXEC sp_rename [dbo.inventory_file_proprietary_header.daypart_default_id], [standard_daypart_id], 'COLUMN'
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_proprietary_daypart_program_mappings') AND name = 'daypart_default_id')
	BEGIN
		EXEC sp_rename [dbo.inventory_proprietary_daypart_program_mappings.daypart_default_id], [standard_daypart_id], 'COLUMN'
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('inventory_summary_quarter_details') AND name = 'daypart_default_id')
	BEGIN
		EXEC sp_rename [dbo.inventory_summary_quarter_details.daypart_default_id], [standard_daypart_id], 'COLUMN'
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('nti_to_nsi_conversion_rates') AND name = 'daypart_default_id')
	BEGIN
		EXEC sp_rename [dbo.nti_to_nsi_conversion_rates.daypart_default_id], [standard_daypart_id], 'COLUMN'
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_audience_daypart_vpvh') AND name = 'daypart_default_id')
	BEGIN
		EXEC sp_rename [dbo.plan_version_audience_daypart_vpvh.daypart_default_id], [standard_daypart_id], 'COLUMN'
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_dayparts') AND name = 'daypart_default_id')
	BEGIN
		EXEC sp_rename [dbo.plan_version_dayparts.daypart_default_id], [standard_daypart_id], 'COLUMN'
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_weekly_breakdown') AND name = 'daypart_default_id')
	BEGIN
		EXEC sp_rename [dbo.plan_version_weekly_breakdown.daypart_default_id], [standard_daypart_id], 'COLUMN'
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('scx_generation_job_files') AND name = 'daypart_default_id')
	BEGIN
		EXEC sp_rename [dbo.scx_generation_job_files.daypart_default_id], [standard_daypart_id], 'COLUMN'
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('scx_generation_jobs') AND name = 'daypart_default_id')
	BEGIN
		EXEC sp_rename [dbo.scx_generation_jobs.daypart_default_id], [standard_daypart_id], 'COLUMN'
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('station_inventory_manifest_dayparts') AND name = 'daypart_default_id')
	BEGIN
		EXEC sp_rename [dbo.station_inventory_manifest_dayparts.daypart_default_id], [standard_daypart_id], 'COLUMN'
	END

	--rename FKs
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id  = OBJECT_ID('inventory_file_proprietary_header') 
		AND name = 'FK_inventory_file_proprietary_header_daypart_codes')
	BEGIN
		EXEC sp_rename [dbo.FK_inventory_file_proprietary_header_daypart_codes]
			, [FK_inventory_file_proprietary_header_standard_dayparts]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id  = OBJECT_ID('inventory_proprietary_daypart_program_mappings') 
		AND name = 'FK_inventory_proprietary_daypart_program_mappings_daypart_defaults')
	BEGIN
		EXEC sp_rename [dbo.FK_inventory_proprietary_daypart_program_mappings_daypart_defaults]
			, [FK_inventory_proprietary_daypart_program_mappings_standard_dayparts]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id  = OBJECT_ID('inventory_summary_quarter_details') 
		AND name = 'FK_inventory_summary_quarter_details_daypart_codes')
	BEGIN
		EXEC sp_rename [dbo.FK_inventory_summary_quarter_details_daypart_codes]
			, [FK_inventory_summary_quarter_details_standard_dayparts]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id  = OBJECT_ID('nti_to_nsi_conversion_rates') 
		AND name = 'FK_nti_to_nsi_conversion_rates_daypart_defaults')
	BEGIN
		EXEC sp_rename [dbo.FK_nti_to_nsi_conversion_rates_daypart_defaults]
			, [FK_nti_to_nsi_conversion_rates_standard_dayparts]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id  = OBJECT_ID('plan_version_dayparts') 
		AND name = 'FK_plan_dayparts_daypart_codes')
	BEGIN
		EXEC sp_rename [dbo.FK_plan_dayparts_daypart_codes]
			, [FK_plan_dayparts_standard_dayparts]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id  = OBJECT_ID('plan_version_audience_daypart_vpvh') 
		AND name = 'FK_plan_version_audience_daypart_vpvh_daypart_defaults')
	BEGIN
		EXEC sp_rename [dbo.FK_plan_version_audience_daypart_vpvh_daypart_defaults]
			, [FK_plan_version_audience_daypart_vpvh_standard_dayparts]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id  = OBJECT_ID('plan_version_weekly_breakdown') 
		AND name = 'FK_plan_version_weekly_breakdown_daypart_defaults')
	BEGIN
		EXEC sp_rename [dbo.FK_plan_version_weekly_breakdown_daypart_defaults]
			, [FK_plan_version_weekly_breakdown_standard_dayparts]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id  = OBJECT_ID('scx_generation_job_files') 
		AND name = 'FK_scx_generation_job_files_daypart_codes')
	BEGIN
		EXEC sp_rename [dbo.FK_scx_generation_job_files_daypart_codes]
			, [FK_scx_generation_job_files_standard_dayparts]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id  = OBJECT_ID('scx_generation_jobs') 
		AND name = 'FK_scx_generation_jobs_daypart_codes')
	BEGIN
		EXEC sp_rename [dbo.FK_scx_generation_jobs_daypart_codes]
			, [FK_scx_generation_jobs_standard_dayparts]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id  = OBJECT_ID('station_inventory_manifest_dayparts') 
		AND name = 'FK_station_inventory_manifest_dayparts_daypart_codes')
	BEGIN
		EXEC sp_rename [dbo.FK_station_inventory_manifest_dayparts_daypart_codes]
			, [FK_station_inventory_manifest_dayparts_standard_dayparts]
	END
END
/*************************************** END BP-1467 *******************************************************/

/*************************************** START - BP-1288 ****************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_station_details') AND name = 'is_proprietary ')
BEGIN
	ALTER TABLE plan_version_pricing_station_details ADD is_proprietary bit NULL
	EXEC('UPDATE plan_version_pricing_station_details SET is_proprietary = 0')
	ALTER TABLE plan_version_pricing_station_details ALTER COLUMN is_proprietary bit NOT NULL
END
/*************************************** END - BP-1288 ****************************************************/

/*************************************** START - BP-1287 ****************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_result_spots') AND name = 'is_proprietary ')
BEGIN
	ALTER TABLE plan_version_pricing_result_spots ADD is_proprietary bit NULL
	EXEC('UPDATE plan_version_pricing_result_spots SET is_proprietary = 0')
	ALTER TABLE plan_version_pricing_result_spots ALTER COLUMN is_proprietary bit NOT NULL
END
/*************************************** END - BP-1287 ****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.10.1', -- Current release version
description = 'Schema Version'
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.09.1' -- Previous release version
		OR [version] = '20.10.1') -- Current release version
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