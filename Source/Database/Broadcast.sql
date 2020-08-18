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

/*************************************** START - BP-90 ****************************************************/
BEGIN TRAN
BEGIN TRY

IF OBJECT_ID('plan_version_pricing_api_result_spot_frequencies') IS NULL
BEGIN 
	CREATE TABLE [plan_version_pricing_api_result_spot_frequencies]
	(
		[id] INT IDENTITY(1,1) NOT NULL,
		[plan_version_pricing_api_result_spot_id] INT NOT NULL,
		[spot_length_id] INT NOT NULL,
		[cost] MONEY NOT NULL,
		[spots] INT NOT NULL

		CONSTRAINT [PK_plan_version_pricing_api_result_spot_frequencies] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)

	ALTER TABLE [dbo].[plan_version_pricing_api_result_spot_frequencies]
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_api_result_spot_frequencies_plan_version_pricing_api_result_spots] 
	FOREIGN KEY([plan_version_pricing_api_result_spot_id])
	REFERENCES [dbo].[plan_version_pricing_api_result_spots] ([id])

	ALTER TABLE [dbo].[plan_version_pricing_api_result_spot_frequencies] 
	CHECK CONSTRAINT [FK_plan_version_pricing_api_result_spot_frequencies_plan_version_pricing_api_result_spots]


	ALTER TABLE [dbo].[plan_version_pricing_api_result_spot_frequencies]
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_api_result_spot_frequencies_spot_lengths]
	FOREIGN KEY([spot_length_id])
	REFERENCES [dbo].[spot_lengths] ([id])

	ALTER TABLE [dbo].[plan_version_pricing_api_result_spot_frequencies]
	CHECK CONSTRAINT [FK_plan_version_pricing_api_result_spot_frequencies_spot_lengths]

	EXEC('
		INSERT INTO plan_version_pricing_api_result_spot_frequencies(
			plan_version_pricing_api_result_spot_id, 
			spot_length_id, 
			cost, 
			spots)
		select s.id as plan_version_pricing_api_result_spot_id, 
			   pvcl.spot_length_id,
			   s.cost,
			   s.spots
		from plan_version_pricing_api_result_spots as s
		join plan_version_creative_lengths as pvcl
		on pvcl.id = (select top 1 cl.id
					  from plan_version_pricing_api_result_spots as rs
					  join plan_version_pricing_api_results as r on rs.plan_version_pricing_api_results_id = r.id
					  join plan_version_pricing_job as j on j.id = r.plan_version_pricing_job_id
					  join plan_versions as v on j.plan_version_id = v.id
					  join plan_version_creative_lengths as cl on cl.plan_version_id = v.id
					  where rs.id = s.id)
	')
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_result_spots') AND name = 'cost')
BEGIN
	EXEC('ALTER TABLE plan_version_pricing_api_result_spots DROP COLUMN cost')
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_result_spots') AND name = 'spots')
BEGIN
	EXEC('ALTER TABLE plan_version_pricing_api_result_spots DROP COLUMN spots')
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_result_spots') AND name = 'impressions')
BEGIN
	EXEC sp_rename 'dbo.plan_version_pricing_api_result_spots.impressions', 'impressions30sec', 'COLUMN';  
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_results') AND name = 'pricing_version')
BEGIN
	ALTER TABLE plan_version_pricing_api_results ADD pricing_version varchar(10) NULL
	EXEC('update plan_version_pricing_api_results set pricing_version = ''2''')
	ALTER TABLE plan_version_pricing_api_results ALTER COLUMN pricing_version varchar(10) NOT NULL
END

COMMIT TRAN
END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0)
	BEGIN
	  ROLLBACK TRAN;
	END;
	THROW
END CATCH

/*************************************** END - BP-90 ****************************************************/

/*************************************** START BP-836 *****************************************************/
-- DROP Foreign Keys 
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_plan_version_pricing_api_results_plan_versions' 
				AND parent_object_id = OBJECT_ID('plan_version_pricing_api_results'))
BEGIN
	ALTER TABLE [plan_version_pricing_api_results] DROP [FK_plan_version_pricing_api_results_plan_versions]
END
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_plan_version_pricing_bands_plan_versions' 
				AND parent_object_id = OBJECT_ID('plan_version_pricing_bands'))
BEGIN
	ALTER TABLE [plan_version_pricing_bands] DROP [FK_plan_version_pricing_bands_plan_versions]
END
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_plan_version_pricing_markets_plan_version' 
				AND parent_object_id = OBJECT_ID('plan_version_pricing_markets'))
BEGIN
	ALTER TABLE [plan_version_pricing_markets] DROP [FK_plan_version_pricing_markets_plan_version]
END
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_plan_version_pricing_results_plan_versions' 
				AND parent_object_id = OBJECT_ID('plan_version_pricing_results'))
BEGIN
	ALTER TABLE [plan_version_pricing_results] DROP [FK_plan_version_pricing_results_plan_versions]
END
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_plan_versions_plan_version_pricing_stations' 
				AND parent_object_id = OBJECT_ID('plan_version_pricing_stations'))
BEGIN
	ALTER TABLE [plan_version_pricing_stations] DROP [FK_plan_versions_plan_version_pricing_stations]
END

-- DROP plan_version_id column
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id'
				AND object_id = OBJECT_ID('plan_version_pricing_api_results'))
BEGIN
	ALTER TABLE [plan_version_pricing_api_results] DROP COLUMN [plan_version_id]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id'
				AND object_id = OBJECT_ID('plan_version_pricing_bands'))
BEGIN
	ALTER TABLE [plan_version_pricing_bands] DROP COLUMN [plan_version_id]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id'
				AND object_id = OBJECT_ID('plan_version_pricing_markets'))
BEGIN
	ALTER TABLE [plan_version_pricing_markets] DROP COLUMN [plan_version_id]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id'
				AND object_id = OBJECT_ID('plan_version_pricing_results'))
BEGIN
	ALTER TABLE [plan_version_pricing_results] DROP COLUMN [plan_version_id]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id'
				AND object_id = OBJECT_ID('plan_version_pricing_stations'))
BEGIN
	ALTER TABLE [plan_version_pricing_stations] DROP COLUMN [plan_version_id]
END
/*************************************** END BP-836 *****************************************************/
/*************************************** START BP-804 *****************************************************/
/**************************Start inventory_proprietary_summary *************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('inventory_proprietary_summary'))
BEGIN
	CREATE TABLE inventory_proprietary_summary (
	id INT NOT NULL IDENTITY,
	inventory_source_id INT NOT NULL,
	daypart_default_id INT NOT NULL,
	quarter_number INT NOT NULL,
	quarter_year INT NOT NULL, 
	unit int NOT NULL, 
	cpm money  NULL,
	[created_by] [varchar](63) NOT NULL,
	[created_at] [datetime] NOT NULL,
	[modified_by] [varchar](63) NULL,
	[modified_at] [datetime] NULL,
	CONSTRAINT PK_inventory_proprietary_summary PRIMARY KEY (id)
	)
	ALTER TABLE [dbo].inventory_proprietary_summary ADD CONSTRAINT [FK_inventory_proprietary_summary_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	ALTER TABLE [dbo].[inventory_proprietary_summary] ADD CONSTRAINT [FK_inventory_proprietary_summary_daypart_defaults] FOREIGN KEY([daypart_default_id])
	REFERENCES [dbo].[daypart_defaults] ([id])
END

/**************************End inventory_proprietary_summary *************************************************/
/**************************start inventory_proprietary_summary_audiences *************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('inventory_proprietary_summary_audiences'))
BEGIN
	CREATE TABLE inventory_proprietary_summary_audiences (
	id INT NOT NULL IDENTITY,
	inventory_proprietary_summary_id INT NOT NULL,
	audience_id INT NOT NULL,
	impressions float  NULL,
	[created_by] [varchar](63) NOT NULL,
	[created_at] [datetime] NOT NULL,
	[modified_by] [varchar](63) NULL,
	[modified_at] [datetime] NULL,
	CONSTRAINT PK_inventory_proprietary_summary_audiences PRIMARY KEY (id)
	)
	ALTER TABLE [dbo].[inventory_proprietary_summary_audiences]  WITH CHECK ADD  CONSTRAINT [FK_inventory_proprietary_summary_audiences_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])
	ALTER TABLE [dbo].[inventory_proprietary_summary_audiences] ADD  CONSTRAINT [FK_inventory_proprietary_summary_audiences_inventory_proprietary_summary] FOREIGN KEY(inventory_proprietary_summary_id)
	REFERENCES [dbo].[inventory_proprietary_summary] ([id])
END

/**************************end inventory_proprietary_summary_audiences *************************************************/
/**************************start inventory_proprietary_summary_markets *************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('inventory_proprietary_summary_markets'))
BEGIN
	CREATE TABLE inventory_proprietary_summary_markets (
	id INT NOT NULL IDENTITY,
	inventory_proprietary_summary_id INT NOT NULL,
	market_code smallint NOT NULL,
	market_coverage float  NULL,
	[created_by] [varchar](63) NOT NULL,
	[created_at] [datetime] NOT NULL,
	[modified_by] [varchar](63) NULL,
	[modified_at] [datetime] NULL,
	CONSTRAINT PK_inventory_proprietary_summary_markets PRIMARY KEY (id)
	)
	ALTER TABLE [dbo].[inventory_proprietary_summary_markets] ADD CONSTRAINT [FK_inventory_proprietary_summary_markets_markets] FOREIGN KEY([market_code])
	REFERENCES [dbo].[markets] ([market_code])
	ALTER TABLE [dbo].[inventory_proprietary_summary_markets] ADD CONSTRAINT [FK_inventory_proprietary_summary_markets_inventory_proprietary_summary] FOREIGN KEY(inventory_proprietary_summary_id)
	REFERENCES [dbo].[inventory_proprietary_summary] ([id])
END

/**************************end inventory_proprietary_summary_markets *************************************************/
/*************************************** END BP-804 *****************************************************/

/*************************************** START BP-814 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('stations')
				 AND name = 'sales_group_name')
BEGIN
	ALTER TABLE [dbo].[stations] ADD [sales_group_name] VARCHAR(100) NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('stations')
				 AND name = 'owner_name')
BEGIN
	ALTER TABLE [dbo].[stations] ADD [owner_name] VARCHAR(100) NULL
END
/*************************************** END BP-814 *****************************************************/

/*************************************** END BP-897 *****************************************************/
IF ((SELECT COLUMNPROPERTY(OBJECT_ID('plan_version_pricing_job', 'U'), 'plan_version_id', 'AllowsNull')) = 0)
BEGIN
	ALTER TABLE plan_version_pricing_job
	ALTER COLUMN plan_version_id INT NULL
END

IF ((SELECT COLUMNPROPERTY(OBJECT_ID('plan_version_pricing_parameters', 'U'), 'plan_version_id', 'AllowsNull')) = 0)
BEGIN
	ALTER TABLE plan_version_pricing_parameters
	ALTER COLUMN plan_version_id INT NULL
END
/*************************************** END BP-897 *****************************************************/

/*************************************** START BP-1098 **************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_job'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_job](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_id] [int] NULL,
		[status] [int] NOT NULL,
		[queued_at] [datetime] NOT NULL,
		[completed_at] [datetime] NULL,
		[error_message] [nvarchar](max) NULL,
		[diagnostic_result] [nvarchar](max) NULL,
		[hangfire_job_id] [varchar](16) NULL,
	 CONSTRAINT [PK_plan_version_buying_job] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	
	ALTER TABLE [dbo].[plan_version_buying_job]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_job_plan_versions] FOREIGN KEY([plan_version_id])
	REFERENCES [dbo].[plan_versions] ([id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_job_inventory_source_estimates'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_job_inventory_source_estimates](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[media_week_id] [int] NOT NULL,
		[inventory_source_id] [int] NULL,
		[plan_version_buying_job_id] [int] NOT NULL,
		[impressions] [float] NOT NULL,
		[cost] [money] NOT NULL,
		[inventory_source_type] [int] NULL,
	 CONSTRAINT [PK_plan_version_buying_job_inventory_source_estimates] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_job_inventory_source_estimates]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_job_inventory_source_estimates_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])

	ALTER TABLE [dbo].[plan_version_buying_job_inventory_source_estimates]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_job_inventory_source_estimates_media_weeks] FOREIGN KEY([media_week_id])
	REFERENCES [dbo].[media_weeks] ([id])

	ALTER TABLE [dbo].[plan_version_buying_job_inventory_source_estimates]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_job_inventory_source_estimates_plan_version_buying_job] FOREIGN KEY([plan_version_buying_job_id])
	REFERENCES [dbo].[plan_version_buying_job] ([id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_bands'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_bands](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_job_id] [int] NULL,
		[total_spots] [int] NOT NULL,
		[total_impressions] [float] NOT NULL,
		[total_cpm] [money] NOT NULL,
		[total_budget] [money] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_bands] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[plan_version_buying_bands]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_bands_plan_version_buying_job] FOREIGN KEY([plan_version_buying_job_id])
	REFERENCES [dbo].[plan_version_buying_job] ([id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_band_details'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_band_details](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_band_id] [int] NOT NULL,
		[min_band] [money] NULL,
		[max_band] [money] NULL,
		[spots] [int] NOT NULL,
		[impressions] [float] NOT NULL,
		[cpm] [money] NOT NULL,
		[budget] [money] NOT NULL,
		[impressions_percentage] [float] NOT NULL,
		[available_inventory_percentage] [float] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_band_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_band_details]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_band_details_plan_version_buying_bands] FOREIGN KEY([plan_version_buying_band_id])
	REFERENCES [dbo].[plan_version_buying_bands] ([id])
	ON DELETE CASCADE
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_api_results'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_api_results](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[optimal_cpm] [money] NOT NULL,
		[plan_version_buying_job_id] [int] NULL,
		[buying_version] [varchar](10) NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_api_results] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_api_results]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_api_results_plan_version_buying_job] FOREIGN KEY([plan_version_buying_job_id])
	REFERENCES [dbo].[plan_version_buying_job] ([id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_api_result_spots'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_api_result_spots](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_api_results_id] [int] NOT NULL,
		[station_inventory_manifest_id] [int] NOT NULL,
		[inventory_media_week_id] [int] NOT NULL,
		[impressions30sec] [float] NOT NULL,
		[contract_media_week_id] [int] NOT NULL,
		[standard_daypart_id] [int] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_api_result_spots] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_api_result_spots]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_api_result_spots_daypart_defaults] FOREIGN KEY([standard_daypart_id])
	REFERENCES [dbo].[daypart_defaults] ([id])

	ALTER TABLE [dbo].[plan_version_buying_api_result_spots]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_api_result_spots_inventory_media_week] FOREIGN KEY([inventory_media_week_id])
	REFERENCES [dbo].[media_weeks] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[plan_version_buying_api_result_spots]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_api_result_spots_contract_media_week] FOREIGN KEY([contract_media_week_id])
	REFERENCES [dbo].[media_weeks] ([id])

	ALTER TABLE [dbo].[plan_version_buying_api_result_spots]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_api_result_spots_plan_version_buying_api_results] FOREIGN KEY([plan_version_buying_api_results_id])
	REFERENCES [dbo].[plan_version_buying_api_results] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[plan_version_buying_api_result_spots]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_api_result_spots_station_inventory_manifest] FOREIGN KEY([station_inventory_manifest_id])
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ON DELETE CASCADE
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_api_result_spot_frequencies'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_api_result_spot_frequencies](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_api_result_spot_id] [int] NOT NULL,
		[spot_length_id] [int] NOT NULL,
		[cost] [money] NOT NULL,
		[spots] [int] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_api_result_spot_frequencies] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[plan_version_buying_api_result_spot_frequencies]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_api_result_spot_frequencies_plan_version_buying_api_result_spots] FOREIGN KEY([plan_version_buying_api_result_spot_id])
	REFERENCES [dbo].[plan_version_buying_api_result_spots] ([id])
	
	ALTER TABLE [dbo].[plan_version_buying_api_result_spot_frequencies]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_api_result_spot_frequencies_spot_lengths] FOREIGN KEY([spot_length_id])
	REFERENCES [dbo].[spot_lengths] ([id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_markets'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_markets](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_job_id] [int] NULL,
		[total_markets] [int] NOT NULL,
		[total_coverage_percent] [float] NOT NULL,
		[total_stations] [int] NOT NULL,
		[total_spots] [int] NOT NULL,
		[total_impressions] [float] NOT NULL,
		[total_cpm] [float] NOT NULL,
		[total_budget] [float] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_markets] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_markets]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_markets_buying_job] FOREIGN KEY([plan_version_buying_job_id])
	REFERENCES [dbo].[plan_version_buying_job] ([id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_market_details'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_market_details](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_market_id] [int] NOT NULL,
		[rank] [int] NOT NULL,
		[market_coverage_percent] [float] NOT NULL,
		[stations] [int] NOT NULL,
		[spots] [int] NOT NULL,
		[impressions] [float] NOT NULL,
		[cpm] [float] NOT NULL,
		[budget] [float] NOT NULL,
		[impressions_percentage] [float] NOT NULL,
		[share_of_voice_goal_percentage] [float] NULL,
	 CONSTRAINT [PK_plan_version_buying_market_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_market_details]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_market_details_buying_market] FOREIGN KEY([plan_version_buying_market_id])
	REFERENCES [dbo].[plan_version_buying_markets] ([id])
	ON DELETE CASCADE
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_parameters'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_parameters](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_id] [int] NULL,
		[min_cpm] [money] NULL,
		[max_cpm] [money] NULL,
		[coverage_goal] [float] NOT NULL,
		[impressions_goal] [float] NOT NULL,
		[budget_goal] [money] NOT NULL,
		[cpm_goal] [money] NOT NULL,
		[proprietary_blend] [float] NOT NULL,
		[competition_factor] [float] NULL,
		[inflation_factor] [float] NULL,
		[unit_caps_type] [int] NOT NULL,
		[unit_caps] [int] NOT NULL,
		[cpp] [money] NOT NULL,
		[currency] [int] NOT NULL,
		[rating_points] [float] NOT NULL,
		[margin] [float] NULL,
		[plan_version_buying_job_id] [int] NULL,
		[budget_adjusted] [money] NOT NULL,
		[cpm_adjusted] [money] NOT NULL,
		[market_group] [int] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_parameters] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_parameters]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_parameters_plan_version_buying_job] FOREIGN KEY([plan_version_buying_job_id])
	REFERENCES [dbo].[plan_version_buying_job] ([id])

	ALTER TABLE [dbo].[plan_version_buying_parameters]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_parameters_plan_versions] FOREIGN KEY([plan_version_id])
	REFERENCES [dbo].[plan_versions] ([id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_parameters_inventory_source_percentages'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_parameters_inventory_source_percentages](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_parameter_id] [int] NOT NULL,
		[inventory_source_id] [int] NOT NULL,
		[percentage] [int] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_parameters_inventory_source_percentages] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_parameters_inventory_source_percentages]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_parameters_inventory_source_percentages_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])

	ALTER TABLE [dbo].[plan_version_buying_parameters_inventory_source_percentages]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_parameters_inventory_source_percentages_plan_version_buying_parameters] FOREIGN KEY([plan_version_buying_parameter_id])
	REFERENCES [dbo].[plan_version_buying_parameters] ([id])
	ON DELETE CASCADE
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_parameters_inventory_source_type_percentages'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_parameters_inventory_source_type_percentages](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_parameter_id] [int] NOT NULL,
		[inventory_source_type] [tinyint] NOT NULL,
		[percentage] [int] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_parameters_inventory_source_type_percentages] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY],
	 CONSTRAINT [UQ_plan_version_buying_parameters_inventory_source_type_percentages_plan_version_buying_parameter_id_inventory_source_type] UNIQUE NONCLUSTERED 
	(
		[plan_version_buying_parameter_id] ASC,
		[inventory_source_type] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_parameters_inventory_source_type_percentages]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_parameters_inventory_source_type_percentages_plan_version_buying_parameters] FOREIGN KEY([plan_version_buying_parameter_id])
	REFERENCES [dbo].[plan_version_buying_parameters] ([id])
	ON DELETE CASCADE
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_results'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_results](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[optimal_cpm] [money] NOT NULL,
		[total_market_count] [int] NOT NULL,
		[total_station_count] [int] NOT NULL,
		[total_avg_cpm] [money] NOT NULL,
		[total_avg_impressions] [float] NOT NULL,
		[goal_fulfilled_by_proprietary] [bit] NOT NULL,
		[total_impressions] [float] NOT NULL,
		[total_budget] [money] NOT NULL,
		[plan_version_buying_job_id] [int] NULL,
		[total_spots] [int] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_results] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_results]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_results_plan_version_buying_job] FOREIGN KEY([plan_version_buying_job_id])
	REFERENCES [dbo].[plan_version_buying_job] ([id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_result_spots'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_result_spots](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_result_id] [int] NOT NULL,
		[program_name] [varchar](255) NOT NULL,
		[genre] [varchar](500) NOT NULL,
		[avg_impressions] [float] NOT NULL,
		[avg_cpm] [money] NOT NULL,
		[station_count] [int] NOT NULL,
		[market_count] [int] NOT NULL,
		[percentage_of_buy] [float] NOT NULL,
		[budget] [money] NOT NULL,
		[spots] [int] NOT NULL,
		[impressions] [float] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_result_spots] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_result_spots]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_result_spots_plan_version_buying_results] FOREIGN KEY([plan_version_buying_result_id])
	REFERENCES [dbo].[plan_version_buying_results] ([id])
	ON DELETE CASCADE
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_stations'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_stations](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_job_id] [int] NULL,
		[total_spots] [int] NOT NULL,
		[total_impressions] [float] NOT NULL,
		[total_cpm] [money] NOT NULL,
		[total_budget] [money] NOT NULL,
		[total_stations] [int] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_stations] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_stations]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_job_plan_version_buying_stations] FOREIGN KEY([plan_version_buying_job_id])
	REFERENCES [dbo].[plan_version_buying_job] ([id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_station_details'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_station_details](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_station_id] [int] NOT NULL,
		[station] [varchar](15) NOT NULL,
		[market] [varchar](31) NOT NULL,
		[spots] [int] NOT NULL,
		[impressions] [float] NOT NULL,
		[cpm] [money] NOT NULL,
		[budget] [money] NOT NULL,
		[impressions_percentage] [float] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_station_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_station_details]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_buying_stations_plan_version_buying_station_details] FOREIGN KEY([plan_version_buying_station_id])
	REFERENCES [dbo].[plan_version_buying_stations] ([id])
	ON DELETE CASCADE
END
GO
/*************************************** END BP-1098 *****************************************************/

/*************************************** START - BP-797 ****************************************************/
IF NOT EXISTS (SELECT 1 
               FROM   sys.objects 
               WHERE  object_id = 
                      Object_id('program_name_mapping_keywords' 
                      )) 
BEGIN 
	CREATE TABLE program_name_mapping_keywords (
		id INT NOT NULL IDENTITY,
		keyword VARCHAR(100) NOT NULL,
		[program_name] VARCHAR(100) NOT NULL,
		genre_id INT NOT NULL,
		show_type_id INT NOT NULL,
		CONSTRAINT PK_program_name_mapping_keywords PRIMARY KEY (id)
	)

	ALTER TABLE [dbo].[program_name_mapping_keywords] WITH CHECK ADD CONSTRAINT [FK_program_name_mapping_keywords_genres] FOREIGN KEY([genre_id])
	REFERENCES [dbo].[genres] ([id])
	ALTER TABLE [dbo].[program_name_mapping_keywords] CHECK CONSTRAINT [FK_program_name_mapping_keywords_genres]
	CREATE INDEX IX_program_name_mapping_keywords_genre_id ON [program_name_mapping_keywords] ([genre_id])

	ALTER TABLE [dbo].[program_name_mapping_keywords] WITH CHECK ADD CONSTRAINT [FK_program_name_mapping_keywords_show_types] FOREIGN KEY([show_type_id])
	REFERENCES [dbo].[show_types] ([id])
	ALTER TABLE [dbo].[program_name_mapping_keywords] CHECK CONSTRAINT [FK_program_name_mapping_keywords_show_types]
	CREATE INDEX IX_program_name_mapping_keywords_show_type ON [program_name_mapping_keywords] ([show_type_id]) 

	EXEC('INSERT INTO program_name_mapping_keywords (keyword, [program_name], genre_id, show_type_id)
	VALUES (''golf'', ''Golf'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''Wells Fargo champ'', ''Golf'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''NFL'', ''NFL'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''NHL'', ''NHL'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''FUTBOL'', ''Soccer'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''HOCKEY'', ''NHL'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''US OPEN'', ''Golf'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''USA GYMNASTICS'', ''GYMNASTICS'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''US WOMENS OPEN'', ''Golf'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''US SENIOR OPEN'', ''Golf'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''TALLADEGA'', ''NASCAR'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''TENNIS'', ''Tennis'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''STANLEY CUP'', ''NHL'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''SENIOR OPEN'', ''Golf'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''SENIOR PGA'', ''Golf'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''PGA'', ''Golf'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''NBA'', ''NBA'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''YANKEES'', ''MLB'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''METS'', ''MLB'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''NASCAR'', ''NASCAR'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''LPGA'', ''Golf'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''KENTCKY DRBY'', ''Horse Racing'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''KENTUCKY DERBY'', ''Horse Racing'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''INDY 500'', ''NASCAR'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''INDIANAPOLIS 500'', ''NASCAR'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''HORSE RACING'', ''Horse Racing'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''FRENCH OPEN'', ''Tennis'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''AUGUSTA'', ''Golf'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports'')),
	(''BOXING'', ''Boxing'', (SELECT TOP 1 Id FROM genres WHERE name = ''Sports'' ORDER BY 1), (SELECT id FROM show_types WHERE name = ''Sports''))')
END
GO
/*************************************** END - BP-797 ****************************************************/

/*************************************** START - BP-875 ****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_market_details') AND name = 'market_name')
BEGIN
	ALTER TABLE plan_version_pricing_market_details
	ADD market_name varchar(31) NULL

	EXEC('UPDATE d
		  SET d.market_name = m.geography_name
		  FROM plan_version_pricing_market_details d
		  JOIN market_coverages mc on mc.percentage_of_us = d.market_coverage_percent and mc.rank = d.rank
		  JOIN markets m on mc.market_code = m.market_code')

	EXEC('UPDATE plan_version_pricing_market_details
		  SET market_name = ''Unknown Market''
		  WHERE market_name is NULL')

    ALTER TABLE plan_version_pricing_market_details
	ALTER COLUMN market_name varchar(31) NOT NULL
END
GO
/*************************************** END - BP-875 ****************************************************/

/*************************************** START BP-1076 *****************************************************/
-- Daypart for 'SA-SU 9AM-8PM'
-- This ID is validated to be the same in all environments.
-- It is an "older" daypart record, restored to all environments from prod.
DECLARE @daypart_id INT = 1546
	, @weekend_daypart_code VARCHAR(3) = 'WKD'
IF NOT EXISTS (SELECT 1 FROM daypart_defaults WHERE code = @weekend_daypart_code)
BEGIN		
	INSERT INTO daypart_defaults (daypart_type, daypart_id, code, [name], vpvh_calculation_source_type)
		VALUES(
		2 -- non-news
		, @daypart_id -- 'SA-SU 9AM-8PM'
		, @weekend_daypart_code
		, 'Weekend'
		, 3 -- SYN_All
	)
	INSERT INTO nti_to_nsi_conversion_rates (conversion_rate, daypart_default_id, media_month_id)
		SELECT 0.075, d.id, m.media_month_id
		FROM daypart_defaults d, 
		( SELECT DISTINCT media_month_id FROM nti_to_nsi_conversion_rates ) m
		WHERE d.code = @weekend_daypart_code
END 
GO
/*************************************** END BP-1076 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.09.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.08.1' -- Previous release version
		OR [version] = '20.09.1') -- Current release version
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