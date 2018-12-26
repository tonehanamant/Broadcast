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

/*************************************** START BCOP-3936 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[station_inventory_spot_snapshots]'))
BEGIN
	CREATE TABLE [dbo].[station_inventory_spot_snapshots](
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[proposal_version_detail_quarter_week_id] [int] NULL,
		[media_week_id] [int] NOT NULL,
		[spot_length_id] [int] NOT NULL,
		[program_name] [varchar](255) NULL,
		[daypart_id] [int] NOT NULL,
		[station_code] [smallint] NOT NULL,
		[station_call_letters] [varchar](15) NOT NULL,
		[station_market_code] [smallint] NOT NULL,
		[station_market_rank] [int] NOT NULL,
		[spot_impressions] [float] NULL,
		[spot_cost] [money] NULL,
		[audience_id] [int] NOT NULL

		CONSTRAINT [PK_station_inventory_spot_snapshots] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
	
		ALTER TABLE [dbo].[station_inventory_spot_snapshots] WITH CHECK ADD CONSTRAINT [FK_station_inventory_spot_snapshots_proposal_version_detail_quarter_weeks] FOREIGN KEY([proposal_version_detail_quarter_week_id])
		REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id])
		ON DELETE SET NULL
		ALTER TABLE [dbo].[station_inventory_spot_snapshots] CHECK CONSTRAINT [FK_station_inventory_spot_snapshots_proposal_version_detail_quarter_weeks]

		ALTER TABLE [dbo].[station_inventory_spot_snapshots] WITH CHECK ADD CONSTRAINT [FK_station_inventory_spot_snapshots_spot_lengths] FOREIGN KEY([spot_length_id])
		REFERENCES [dbo].[spot_lengths] ([id])
		ALTER TABLE [dbo].[station_inventory_spot_snapshots] CHECK CONSTRAINT [FK_station_inventory_spot_snapshots_spot_lengths]

		ALTER TABLE [dbo].[station_inventory_spot_snapshots] WITH CHECK ADD CONSTRAINT [FK_station_inventory_spot_snapshots_dayparts] FOREIGN KEY([daypart_id])
		REFERENCES [dbo].[dayparts] ([id])
		ALTER TABLE [dbo].[station_inventory_spot_snapshots] CHECK CONSTRAINT [FK_station_inventory_spot_snapshots_dayparts]

		ALTER TABLE [dbo].[station_inventory_spot_snapshots] WITH CHECK ADD CONSTRAINT [FK_station_inventory_spot_snapshots_stations] FOREIGN KEY([station_code])
		REFERENCES [dbo].[stations] ([station_code])
		ALTER TABLE [dbo].[station_inventory_spot_snapshots] CHECK CONSTRAINT [FK_station_inventory_spot_snapshots_stations]

		ALTER TABLE [dbo].[station_inventory_spot_snapshots] WITH CHECK ADD CONSTRAINT [FK_station_inventory_spot_snapshots_markets] FOREIGN KEY([station_market_code])
		REFERENCES [dbo].[markets] ([market_code])
		ALTER TABLE [dbo].[station_inventory_spot_snapshots] CHECK CONSTRAINT [FK_station_inventory_spot_snapshots_markets]

		ALTER TABLE [dbo].[station_inventory_spot_snapshots]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_spot_snapshots_audiences] FOREIGN KEY([audience_id])
		REFERENCES [dbo].[audiences] ([id])
		ALTER TABLE [dbo].[station_inventory_spot_snapshots] CHECK CONSTRAINT [FK_station_inventory_spot_snapshots_audiences]

		CREATE INDEX IX_station_inventory_spot_snapshots_proposal_version_detail_quarter_week_id ON [station_inventory_spot_snapshots] ([proposal_version_detail_quarter_week_id])
		CREATE INDEX IX_station_inventory_spot_snapshots_spot_length_id ON [station_inventory_spot_snapshots] ([spot_length_id])
		CREATE INDEX IX_station_inventory_spot_snapshots_daypart_id ON [station_inventory_spot_snapshots] ([daypart_id])
		CREATE INDEX IX_station_inventory_spot_snapshots_station_code ON [station_inventory_spot_snapshots] ([station_code])
		CREATE INDEX IX_station_inventory_spot_snapshots_station_market_code ON [station_inventory_spot_snapshots] ([station_market_code])
		CREATE INDEX IX_station_inventory_spot_snapshots_audience_id ON [station_inventory_spot_snapshots] ([audience_id])
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'snapshot_date' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_versions]'))
BEGIN
	ALTER TABLE [dbo].[proposal_versions]  ADD [snapshot_date] datetime NULL
END
/*************************************** END BCOP-3936 *****************************************************/

/*************************************** START BCOP-3769 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[pricing_guide_distributions]'))
BEGIN
	CREATE TABLE [dbo].[pricing_guide_distributions](
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[proposal_version_detail_id] [int] NOT NULL,
		[adjustment_margin] [float] NULL,
		[adjustment_rate] [float] NULL,
		[adjustment_inflation] [float] NULL,
		[goal_impression] [float] NULL,
		[goal_budget] [MONEY] NULL,
		[open_market_cpm_min] [MONEY] NULL,
		[open_market_cpm_max] [MONEY] NULL,
		[open_market_unit_cap_per_station] [int] NULL,
		[open_market_cpm_target] [tinyint] NULL,
		[total_open_market_cpm] [FLOAT] NOT NULL,
		[total_open_market_cost] [MONEY] NOT NULL,
		[total_open_market_impressions] [FLOAT] NOT NULL,
		[total_open_market_coverage] [FLOAT] NOT NULL,
		[total_proprietary_cpm] [FLOAT] NOT NULL,
		[total_proprietary_cost] [MONEY] NOT NULL,
		[total_proprietary_impressions] [FLOAT] NOT NULL,
		[created_date] [DATETIME] NOT NULL,
		[created_by] [VARCHAR](63) NOT NULL,
		 CONSTRAINT [PK_pricing_guide_distributions] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
	
		ALTER TABLE [dbo].[pricing_guide_distributions]  WITH CHECK ADD  CONSTRAINT [FK_pricing_guide_distributions_proposal_versions] FOREIGN KEY([proposal_version_detail_id])
		REFERENCES [dbo].[proposal_version_details] ([id])
		ON DELETE CASCADE
		ALTER TABLE [dbo].[pricing_guide_distributions] CHECK CONSTRAINT [FK_pricing_guide_distributions_proposal_versions]
		CREATE INDEX IX_pricing_guide_distributions_proposal_version_detail_id ON [pricing_guide_distributions] ([proposal_version_detail_id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[pricing_guide_distribution_proprietary_inventory]'))
BEGIN
	CREATE TABLE [dbo].[pricing_guide_distribution_proprietary_inventory](
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[pricing_guide_distribution_id] [int] NOT NULL,
		[inventory_source] [tinyint] NOT NULL,
		[impressions_balance_percent] [float] NOT NULL,
		[cpm] [money] NOT NULL,
		 CONSTRAINT [PK_pricing_guide_distribution_proprietary_inventory] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY] 
	
		ALTER TABLE [dbo].[pricing_guide_distribution_proprietary_inventory]  WITH CHECK ADD  CONSTRAINT [FK_pricing_guide_distribution_proprietary_inventory_pricing_guide_distributions] FOREIGN KEY([pricing_guide_distribution_id])
		REFERENCES [dbo].[pricing_guide_distributions] ([id])
		ON DELETE CASCADE
		ALTER TABLE [dbo].[pricing_guide_distribution_proprietary_inventory] CHECK CONSTRAINT [FK_pricing_guide_distribution_proprietary_inventory_pricing_guide_distributions]
		CREATE INDEX IX_pricing_guide_distribution_proprietary_inventory_pricing_guide_distribution_id ON [pricing_guide_distribution_proprietary_inventory] ([pricing_guide_distribution_id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[pricing_guide_distribution_open_market_inventory]'))
BEGIN
	CREATE TABLE [dbo].[pricing_guide_distribution_open_market_inventory](
	[id] [INT] IDENTITY(1,1) NOT NULL,
	[pricing_guide_distribution_id] [int] NOT NULL,
	[manifest_id] [int] NOT NULL,
	[market_code] [smallint] NOT NULL,
	[station_code] [smallint] NOT NULL,
	[station_inventory_manifest_dayparts_id] [int] NOT NULL,
	[daypart_id] INT NOT NULL,
	[program_name] VARCHAR(255),
	[blended_cpm] [money] NOT NULL,
	[spots] [int] NOT NULL,
	[forecasted_impressions_per_spot] [float] NOT NULL,
	[station_impressions_per_spot] [float] NOT NULL,
	[cost_per_spot] [money] NOT NULL,	
	 CONSTRAINT [PK_pricing_guide_distribution_open_market_inventory] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory]  WITH CHECK ADD  CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_pricing_guide_distribution] FOREIGN KEY([pricing_guide_distribution_id])
	REFERENCES [dbo].[pricing_guide_distributions] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory] CHECK CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_pricing_guide_distribution]
	CREATE INDEX IX_pricing_guide_distribution_open_market_inventory_pricing_guide_distribution_id ON [pricing_guide_distribution_open_market_inventory] ([pricing_guide_distribution_id])

	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory]  WITH CHECK ADD  CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_markets] FOREIGN KEY([market_code])
	REFERENCES [dbo].[markets] ([market_code])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory] CHECK CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_markets]
	CREATE INDEX IX_pricing_guide_distribution_open_market_inventory_market_code ON [pricing_guide_distribution_open_market_inventory] ([market_code])

	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory]  WITH CHECK ADD  CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_stations] FOREIGN KEY([station_code])
	REFERENCES [dbo].[stations] ([station_code])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory] CHECK CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_stations]
	CREATE INDEX IX_pricing_guide_distribution_open_market_inventory_station_code ON [pricing_guide_distribution_open_market_inventory] ([station_code])

	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory]  WITH CHECK ADD CONSTRAINT [FK_pricing_guide_distribution_open_market_station_inventory_manifest] FOREIGN KEY([manifest_id])
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory] CHECK CONSTRAINT [FK_pricing_guide_distribution_open_market_station_inventory_manifest]
	CREATE INDEX IX_pricing_guide_distribution_open_market_inventory_manifest_id ON [pricing_guide_distribution_open_market_inventory] ([manifest_id])
	
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory]  WITH CHECK ADD  CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_dayparts] FOREIGN KEY([daypart_id])
	REFERENCES [dbo].dayparts ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory] CHECK CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_dayparts]
	CREATE INDEX IX_pricing_guide_distribution_open_market_inventory_daypart_id ON [pricing_guide_distribution_open_market_inventory] ([daypart_id])
END

IF EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_detail_proprietary_pricing]'))
BEGIN
	DROP TABLE [dbo].[proposal_version_detail_proprietary_pricing]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'adjustment_margin' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [dbo].[proposal_version_details]  DROP COLUMN [adjustment_margin]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'adjustment_rate' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [dbo].[proposal_version_details]  DROP COLUMN [adjustment_rate]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'adjustment_inflation' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [dbo].[proposal_version_details]  DROP COLUMN [adjustment_inflation]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'goal_impression' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [dbo].[proposal_version_details]  DROP COLUMN [goal_impression]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'goal_budget' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [dbo].[proposal_version_details]  DROP COLUMN [goal_budget]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'open_market_cpm_min' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [dbo].[proposal_version_details]  DROP COLUMN [open_market_cpm_min]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'open_market_cpm_max' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [dbo].[proposal_version_details]  DROP COLUMN [open_market_cpm_max]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'open_market_unit_cap_per_station' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [dbo].[proposal_version_details]  DROP COLUMN [open_market_unit_cap_per_station]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'open_market_cpm_target' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [dbo].[proposal_version_details]  DROP COLUMN [open_market_cpm_target]
END
/*************************************** END BCOP-3769 *****************************************************/

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

/*************************************** START BCOP-3958 *****************************************************/

if  exists (select 1 from sys.columns where name = 'adjustment_rate' and object_name(object_id) = 'pricing_guide_distributions') AND
	not exists (select 1 from sys.columns where name = 'adjustment_impression_loss' and object_name(object_id) = 'pricing_guide_distributions')
BEGIN
	EXEC sp_RENAME 'pricing_guide_distributions.adjustment_rate', 'adjustment_impression_loss', 'COLUMN';
END

/*************************************** END BCOP-3958 *****************************************************/


/*************************************** BEGIN BCOP-3974 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[nti_transmittals_files]'))
BEGIN
	CREATE TABLE [dbo].[nti_transmittals_files](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[file_name] [varchar](255) NOT NULL,
		[created_date] [datetime] NOT NULL,
		[created_by] [varchar](63) NOT NULL,
		[status] [int] NOT NULL,
	 CONSTRAINT [PK_nti_transmittals_files] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
END
IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[nti_transmittals_file_problems]'))
BEGIN
	CREATE TABLE [dbo].[nti_transmittals_file_problems](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[nti_transmittals_file_id] [int] NOT NULL,
		[problem_description] [nvarchar](max) NOT NULL
	 CONSTRAINT [PK_nti_transmittals_file_problems] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[nti_transmittals_file_problems]  WITH CHECK ADD  CONSTRAINT [FK_nti_transmittals_file_problems_nti_transmittals_files] FOREIGN KEY([nti_transmittals_file_id])
	REFERENCES [dbo].[nti_transmittals_files] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[nti_transmittals_file_problems] CHECK CONSTRAINT [FK_nti_transmittals_file_problems_nti_transmittals_files]
	CREATE INDEX IX_nti_transmittals_file_problems_nti_transmittals_file_id ON [nti_transmittals_file_problems] ([nti_transmittals_file_id])
END
IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[nti_transmittals_file_reports]'))
BEGIN
	CREATE TABLE [dbo].[nti_transmittals_file_reports](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[nti_transmittals_file_id] [int] NOT NULL,
		[date] [DATE] NOT NULL,
		[advertiser] [VARCHAR](63) NOT NULL,
		[report_name] [VARCHAR](255) NOT NULL,
		[program_id] [INT] NOT NULL,
		[stream] [VARCHAR](16) NOT NULL,
		[program_type] [VARCHAR](4) NOT NULL,
		[program_duration] [int] NOT NULL,
		[stations] [int] NOT NULL,
		[CVG] [INT] NOT NULL,
		[TbyC] [INT] NOT NULL,
		[TA] [FLOAT] NOT NULL,
		[week_ending] [DATE] NOT NULL
	 CONSTRAINT [PK_nti_transmittals_file_reports] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[nti_transmittals_file_reports]  WITH CHECK ADD  CONSTRAINT [FK_nti_transmittals_file_reports_nti_transmittals_files] FOREIGN KEY([nti_transmittals_file_id])
	REFERENCES [dbo].[nti_transmittals_files] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[nti_transmittals_file_reports] CHECK CONSTRAINT [FK_nti_transmittals_file_reports_nti_transmittals_files]
	CREATE INDEX IX_nti_transmittals_file_reports_nti_transmittals_file_id ON [nti_transmittals_file_reports] ([nti_transmittals_file_id])
END
IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[nti_transmittals_file_report_ratings]'))
BEGIN
	CREATE TABLE [dbo].[nti_transmittals_file_report_ratings](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[nti_transmittals_file_report_id] [INT] NOT NULL,
		[category_name] [VARCHAR](255) NOT NULL,
		[subcategory_name] [VARCHAR](255) NULL,
		[percent] [FLOAT] NOT NULL,
		[impressions] [FLOAT] NOT NULL,
		[VPVH] [INT] NULL
	 CONSTRAINT [PK_nti_transmittals_file_report_ratings] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[nti_transmittals_file_report_ratings]  WITH CHECK ADD  CONSTRAINT [FK_nti_transmittals_file_report_ratings_nti_transmittals_file_reports] FOREIGN KEY([nti_transmittals_file_report_id])
	REFERENCES [dbo].[nti_transmittals_file_reports] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[nti_transmittals_file_report_ratings] CHECK CONSTRAINT [FK_nti_transmittals_file_report_ratings_nti_transmittals_file_reports]
	CREATE INDEX IX_nti_transmittals_file_report_ratings_nti_transmittals_file_report_id ON [nti_transmittals_file_report_ratings] ([nti_transmittals_file_report_id])
END
/*************************************** END BCOP-3974 *****************************************************/

/*************************************** START BCOP-3769 Code Review *****************************************************/
IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('[dbo].[pricing_guide_distributions]') 
			AND  name = 'total_proprietary_cpm'
			AND system_type_id = (SELECT system_type_id FROM sys.types WHERE name = 'float'))
BEGIN
	ALTER TABLE [dbo].[pricing_guide_distributions]  ALTER COLUMN [total_proprietary_cpm] [MONEY] NOT NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('[dbo].[pricing_guide_distributions]') 
			AND  name = 'total_open_market_cpm'
			AND system_type_id = (SELECT system_type_id FROM sys.types WHERE name = 'float'))
BEGIN
	ALTER TABLE [dbo].[pricing_guide_distributions]  ALTER COLUMN [total_open_market_cpm] [MONEY] NOT NULL
END
/*************************************** END BCOP-3769 Code Review *****************************************************/

/*************************************** START BCOP-4119 *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
			  WHERE NAME = N'manifest_id'
			  AND OBJECT_ID = OBJECT_ID(N'pricing_guide_distribution_open_market_inventory'))
BEGIN
    ALTER TABLE pricing_guide_distribution_open_market_inventory
	ADD manifest_id INT NOT NULL CONSTRAINT DF_pricing_guide_distribution_open_market_inventory_manifest_id DEFAULT 0
	
	DROP INDEX pricing_guide_distribution_open_market_inventory.IX_pricing_guide_distribution_open_market_inventory_station_inventory_manifest_dayparts_id
	
	ALTER TABLE [pricing_guide_distribution_open_market_inventory]
	DROP CONSTRAINT [FK_pricing_guide_distribution_open_market_inventory_[station_inventory_manifest_dayparts]

	EXEC('UPDATE pricing_guide_distribution_open_market_inventory
		  SET manifest_id = (SELECT station_inventory_manifest_id FROM station_inventory_manifest_dayparts simd
						     WHERE simd.id = pricing_guide_distribution_open_market_inventory.station_inventory_manifest_dayparts_id)')
	
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory]  WITH CHECK ADD CONSTRAINT [FK_pricing_guide_distribution_open_market_station_inventory_manifest] FOREIGN KEY([manifest_id])
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory] CHECK CONSTRAINT [FK_pricing_guide_distribution_open_market_station_inventory_manifest]
	CREATE INDEX IX_pricing_guide_distribution_open_market_inventory_manifest_id ON [pricing_guide_distribution_open_market_inventory] ([manifest_id])

    ALTER TABLE [pricing_guide_distribution_open_market_inventory]
	DROP CONSTRAINT DF_pricing_guide_distribution_open_market_inventory_manifest_id
END

/*************************************** END BCOP-4119 *****************************************************/


/*************************************** START BCOP-4192 *****************************************************/

IF (SELECT character_maximum_length
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE 
		TABLE_NAME = 'pricing_guide_distribution_open_market_inventory' AND 
		COLUMN_NAME = 'program_name') = 255
BEGIN
	ALTER TABLE pricing_guide_distribution_open_market_inventory
	ALTER COLUMN program_name VARCHAR(MAX) NULL
END

/*************************************** END BCOP-4192 *****************************************************/


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