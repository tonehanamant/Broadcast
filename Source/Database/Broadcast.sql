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

/*************************************** START PRI-10290 *****************************************************/

-- If the table doesn't have the new column, recreate it and its dependencies.
IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE NAME = 'inventory_source_id' AND
              OBJECT_ID = OBJECT_ID('scx_generation_jobs'))
BEGIN
	IF OBJECT_ID('dbo.scx_generation_job_files', 'U') IS NOT NULL 
		DROP TABLE dbo.scx_generation_job_files; 
	IF OBJECT_ID('dbo.scx_generation_job_units', 'U') IS NOT NULL 
		DROP TABLE dbo.scx_generation_job_units;
	IF OBJECT_ID('dbo.scx_generation_jobs', 'U') IS NOT NULL 
		DROP TABLE dbo.scx_generation_jobs;
END

IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND 
				      TABLE_NAME = 'scx_generation_job_request_units'))
BEGIN
	DROP TABLE scx_generation_job_request_units
END

IF (EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND 
				      TABLE_NAME = 'scx_generation_job_requests'))
BEGIN
	IF OBJECT_ID('dbo.scx_generation_job_files', 'U') IS NOT NULL 
		DROP TABLE dbo.scx_generation_job_files; 
	IF OBJECT_ID('dbo.scx_generation_job_units', 'U') IS NOT NULL 
		DROP TABLE dbo.scx_generation_job_units;
	IF OBJECT_ID('dbo.scx_generation_jobs', 'U') IS NOT NULL 
		DROP TABLE dbo.scx_generation_jobs;
	DROP TABLE scx_generation_job_requests
END

-- Renaming incorrectly named PK.
IF EXISTS (SELECT * 
		   FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
		   WHERE CONSTRAINT_NAME ='PK_scx_generation_files')
BEGIN
	EXEC sp_rename @objname = N'[dbo].[scx_generation_job_files].[PK_scx_generation_files]', @newname = N'PK_scx_generation_job_files'
END

IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND 
				      TABLE_NAME = 'scx_generation_jobs'))
BEGIN
	CREATE TABLE [dbo].[scx_generation_jobs](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[inventory_source_id] [int] NOT NULL,
		[daypart_code_id] [int] NOT NULL,
		[start_date] [datetime] NOT NULL,
		[end_date] [datetime] NOT NULL,
		[status] [int] NOT NULL,
		[queued_at] [datetime] NOT NULL,
		[completed_at] [datetime] NULL,
		[requested_by] varchar(63) NOT NULL,
	 CONSTRAINT [PK_scx_generation_jobs] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[scx_generation_jobs] WITH CHECK ADD  CONSTRAINT [FK_scx_generation_jobs_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	
	ALTER TABLE [dbo].[scx_generation_jobs] CHECK CONSTRAINT [FK_scx_generation_jobs_inventory_sources]
	
	ALTER TABLE [dbo].[scx_generation_jobs] WITH CHECK ADD CONSTRAINT [FK_scx_generation_jobs_daypart_codes] FOREIGN KEY([daypart_code_id])
	REFERENCES [dbo].[daypart_codes] ([id])
	
	ALTER TABLE [dbo].[scx_generation_jobs] CHECK CONSTRAINT [FK_scx_generation_jobs_daypart_codes]
END

-- [scx_generation_job_request_units]
IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND 
				      TABLE_NAME = 'scx_generation_job_units'))
BEGIN
	CREATE TABLE [dbo].[scx_generation_job_units](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[scx_generation_job_id] [int] NOT NULL,
		[unit_name] varchar(50) NOT NULL
	 CONSTRAINT [PK_scx_generation_job_units] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[scx_generation_job_units]  WITH CHECK ADD  CONSTRAINT [FK_scx_generation_job_units_scx_generation_jobs] FOREIGN KEY([scx_generation_job_id])
	REFERENCES [dbo].[scx_generation_jobs] ([id])

	ALTER TABLE [dbo].[scx_generation_job_units] CHECK CONSTRAINT [FK_scx_generation_job_units_scx_generation_jobs]
END

--[scx_generation_job_files]
IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND 
				      TABLE_NAME = 'scx_generation_job_files'))
BEGIN
	CREATE TABLE [dbo].[scx_generation_job_files](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[scx_generation_job_id] [int] NOT NULL,
		[file_name] varchar(255) NOT NULL,
		[inventory_source_id] [int] NOT NULL,
		[daypart_code_id] [int] NOT NULL,
		[start_date] [datetime] NOT NULL,
		[end_date] [datetime] NOT NULL,
		[unit_name] varchar(50) NOT NULL
	 CONSTRAINT [PK_scx_generation_job_files] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[scx_generation_job_files]  WITH CHECK ADD  CONSTRAINT [FK_scx_generation_job_files_scx_generation_job] FOREIGN KEY([scx_generation_job_id])
	REFERENCES [dbo].[scx_generation_jobs] ([id])
	
	ALTER TABLE [dbo].[scx_generation_job_files] CHECK CONSTRAINT [FK_scx_generation_job_files_scx_generation_job]
	
	ALTER TABLE [dbo].[scx_generation_job_files]  WITH CHECK ADD  CONSTRAINT [FK_scx_generation_job_files_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	
	ALTER TABLE [dbo].[scx_generation_job_files] CHECK CONSTRAINT [FK_scx_generation_job_files_inventory_sources]
	
	ALTER TABLE [dbo].[scx_generation_job_files]  WITH CHECK ADD  CONSTRAINT [FK_scx_generation_job_files_daypart_codes] FOREIGN KEY([daypart_code_id])
	REFERENCES [dbo].[daypart_codes] ([id])
	
	ALTER TABLE [dbo].[scx_generation_job_files] CHECK CONSTRAINT [FK_scx_generation_job_files_daypart_codes]
END

/*************************************** END PRI-10290 *****************************************************/

/*************************************** START PRI-10832 *****************************************************/
IF OBJECT_ID('inventory_summary', 'U') IS NULL
BEGIN	
	CREATE TABLE [dbo].[inventory_summary](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[inventory_source_id] [INT] NOT NULL,
		[first_quarter_number] [INT] NOT NULL,
		[first_quarter_year] [INT] NOT NULL,
		[last_quarter_number] [INT] NOT NULL,
		[last_quarter_year] [INT] NOT NULL,
		[last_update_date] [DATETIME] NULL
	 CONSTRAINT [PK_inventory_summary] PRIMARY KEY CLUSTERED 
	(
		[id] ASC,
		[inventory_source_id]
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_summary] WITH CHECK ADD CONSTRAINT [FK_inventory_summary_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	ALTER TABLE [dbo].[inventory_summary] CHECK CONSTRAINT [FK_inventory_summary_inventory_sources]
	CREATE NONCLUSTERED INDEX [IX_inventory_summary_inventory_source_id]  ON [dbo].[inventory_summary] ([inventory_source_id] ASC)
	INCLUDE ([id]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

IF OBJECT_ID('inventory_summary_quarters', 'U') IS NULL
BEGIN	
	CREATE TABLE [dbo].[inventory_summary_quarters](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[inventory_source_id] [INT] NOT NULL,
		[quarter_number] [INT] NOT NULL,
		[quarter_year] [INT] NOT NULL,
		[share_book_id] [INT] NULL,
		[hut_book_id] [INT] NULL,
		[total_markets] [INT] NOT NULL,
		[total_stations] [INT] NOT NULL,
		[total_programs] [INT] NULL,
		[total_daypart_codes] [INT] NULL,
		[total_units] [INT] NULL,
		[total_projected_impressions] [FLOAT] NULL,
		[cpm] [MONEY] NULL
	 CONSTRAINT [PK_inventory_summary_quarters] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_summary_quarters] WITH CHECK ADD CONSTRAINT [FK_inventory_summary_quarters_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	ALTER TABLE [dbo].[inventory_summary_quarters] CHECK CONSTRAINT [FK_inventory_summary_quarters_inventory_sources]
	CREATE NONCLUSTERED INDEX [IX_inventory_summary_quarters_inventory_source_id]  ON [dbo].[inventory_summary_quarters] ([inventory_source_id] ASC)
	INCLUDE ([id], [quarter_number], [quarter_year]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

IF OBJECT_ID('inventory_summary_quarter_details', 'U') IS NULL
BEGIN	
	CREATE TABLE [dbo].[inventory_summary_quarter_details](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[inventory_summary_quarter_id] [INT] NOT NULL,
		[daypart] [VARCHAR](64),
		[total_markets] [INT] NOT NULL,
		[total_coverage] [FLOAT] NOT NULL,
		[total_units] [INT] NULL,
		[total_programs] [INT] NULL,
		[total_projected_impressions] [FLOAT] NULL,
		[cpm] [MONEY] NULL,
		[min_spots_per_week] [INT] NULL,
		[max_spots_per_week] [INT] NULL,
		[share_book_id] [INT] NULL,
		[hut_book_id] [INT] NULL
	 CONSTRAINT [PK_inventory_summary_quarter_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC,
		[inventory_summary_quarter_id]
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_summary_quarter_details] WITH CHECK ADD CONSTRAINT [FK_inventory_summary_quarter_details_inventory_summary_quarters] FOREIGN KEY([inventory_summary_quarter_id])
	REFERENCES [dbo].[inventory_summary_quarters] ([id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[inventory_summary_quarter_details] CHECK CONSTRAINT [FK_inventory_summary_quarter_details_inventory_summary_quarters]
	CREATE NONCLUSTERED INDEX [IX_inventory_summary_quarter_details_inventory_summary_quarter_id]  ON [dbo].[inventory_summary_quarter_details] ([inventory_summary_quarter_id] ASC)
	INCLUDE ([id]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

IF OBJECT_ID('inventory_summary_gaps', 'U') IS NULL
BEGIN	
	CREATE TABLE [dbo].[inventory_summary_gaps](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[inventory_summary_id] [INT] NOT NULL,
		[quarter_number] [INT] NOT NULL,
		[quarter_year] [INT] NOT NULL,
		[all_quarter_missing] [BIT] NOT NULL
	 CONSTRAINT [PK_inventory_summary_gaps] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_summary_gaps] WITH CHECK ADD CONSTRAINT [FK_inventory_summary_gaps_inventory_summary] FOREIGN KEY([inventory_summary_id])
	REFERENCES [dbo].[inventory_summary_quarters] ([id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[inventory_summary_gaps] CHECK CONSTRAINT [FK_inventory_summary_gaps_inventory_summary]
	CREATE NONCLUSTERED INDEX [IX_inventory_summary_gaps_inventory_summary_id]  ON [dbo].[inventory_summary_gaps] ([inventory_summary_id] ASC)
	INCLUDE ([id]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

IF OBJECT_ID('inventory_summary_gap_ranges', 'U') IS NULL
BEGIN	
	CREATE TABLE [dbo].[inventory_summary_gap_ranges](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[inventory_summary_gaps_id] [INT] NOT NULL,
		[start_date] [DATETIME] NOT NULL,
		[end_date] [DATETIME] NOT NULL
	 CONSTRAINT [PK_inventory_summary_gap_ranges] PRIMARY KEY CLUSTERED 
	(
		[id] ASC,
		[inventory_summary_gaps_id]
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_summary_gap_ranges] WITH CHECK ADD CONSTRAINT [FK_inventory_summary_gap_ranges_inventory_summary_gaps] FOREIGN KEY([inventory_summary_gaps_id])
	REFERENCES [dbo].[inventory_summary_gaps] ([id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[inventory_summary_gap_ranges] CHECK CONSTRAINT [FK_inventory_summary_gap_ranges_inventory_summary_gaps]
	CREATE NONCLUSTERED INDEX [IX_inventory_summary_gap_rages_inventory_summary_gaps_id]  ON [dbo].[inventory_summary_gap_ranges] ([inventory_summary_gaps_id] ASC)
	INCLUDE ([id]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
/*************************************** END PRI-10832 *****************************************************/

/*************************************** START PRI-7402 ***************************************************/

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'campaigns' AND COLUMN_NAME = 'start_date')
BEGIN
	ALTER TABLE Campaigns
		DROP COLUMN [start_date]
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'campaigns' AND COLUMN_NAME = 'end_date')
BEGIN
	ALTER TABLE Campaigns
		DROP COLUMN [end_date]
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'campaigns' AND COLUMN_NAME = 'budget')
BEGIN
	ALTER TABLE Campaigns
		DROP COLUMN budget
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'campaigns' AND COLUMN_NAME = 'notes')
BEGIN
	ALTER TABLE Campaigns
		ADD notes VARCHAR(1024) NULL
END

IF (1 = (SELECT is_nullable FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID('campaigns') AND [Name] = 'modified_date'))
BEGIN
	ALTER TABLE campaigns
		ALTER COLUMN modified_date DATETIME NOT NULL
END

IF (1 = (SELECT is_nullable FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID('campaigns') AND [Name] = 'modified_by'))
BEGIN
	ALTER TABLE campaigns
		ALTER COLUMN modified_by varchar(63) NOT NULL
END

/*************************************** END PRI-7402 *****************************************************/

/*************************************** START PRI-10832 Part 2 *****************************************************/
IF NOT EXISTS(SELECT 1 from sys.columns where name = 'daypart_code_id' AND object_id = OBJECT_id('inventory_summary_quarter_details'))
BEGIN
	DECLARE @Sql VARCHAR(1024)

	--add the daypart_code_id column
	ALTER TABLE [inventory_summary_quarter_details] ADD [daypart_code_id] INT NULL

	--assign value to daypart_code_id based on daypart column
	SET @Sql = 'UPDATE t1
				SET t1.daypart_code_id = t2.id
				FROM [inventory_summary_quarter_details] AS t1
				INNER JOIN [daypart_codes] AS t2 on T2.code = t1.daypart'
	EXEC (@Sql)

	--remove the nullable property from daypart_code_id
	EXEC('ALTER TABLE inventory_summary_quarter_details ALTER COLUMN [daypart_code_id] INT NOT NULL')
	
	--add FK pointing to daypart_codes
	ALTER TABLE [dbo].[inventory_summary_quarter_details] WITH CHECK ADD CONSTRAINT [FK_inventory_summary_quarter_details_daypart_codes] FOREIGN KEY([daypart_code_id])
	REFERENCES [dbo].[daypart_codes] ([id])
	ALTER TABLE [dbo].[inventory_summary_quarter_details] CHECK CONSTRAINT [FK_inventory_summary_quarter_details_daypart_codes]
	
	--add index on the FK
	SET @Sql =  'CREATE NONCLUSTERED INDEX [IX_inventory_summary_quarter_details_daypart_code_id]  ON [dbo].[inventory_summary_quarter_details] ([daypart_code_id] ASC)
	INCLUDE ([id]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]'	
	EXEC (@Sql)	
END

IF EXISTS(SELECT 1 from sys.columns where name = 'daypart' AND object_id = OBJECT_id('inventory_summary_quarter_details'))
BEGIN	
	--drop the column not used
	ALTER TABLE [inventory_summary_quarter_details] DROP COLUMN [daypart]
END
/*************************************** END PRI-10832 *****************************************************/

/*************************************** START PRI-7452 *****************************************************/
IF OBJECT_ID('plans') IS NULL
BEGIN
	CREATE TABLE [dbo].[plans](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[campaign_id] [int] NOT NULL,
		[name] [NVARCHAR](265),
		[product_id] [INT] NOT NULL,
		[spot_length_id] [INT] NOT NULL,
		[equivalized] [BIT] NOT NULL,
		[status] [INT] NOT NULL,
		[created_by] [VARCHAR](63) NOT NULL,
		[created_date] [DATETIME] NOT NULL
	 CONSTRAINT [PK_plans] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plans] WITH CHECK ADD  CONSTRAINT [FK_plans_campaigns] FOREIGN KEY([campaign_id])
	REFERENCES [dbo].[campaigns] ([id])	
	ALTER TABLE [dbo].[plans] CHECK CONSTRAINT [FK_plans_campaigns]
	CREATE NONCLUSTERED INDEX [IX_plans_campaign_id]  ON [dbo].[plans] ([campaign_id] ASC)
	INCLUDE ([id]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	
	ALTER TABLE [dbo].[plans] WITH CHECK ADD  CONSTRAINT [FK_plans_spot_lengths] FOREIGN KEY([spot_length_id])
	REFERENCES [dbo].[spot_lengths] ([id])	
	ALTER TABLE [dbo].[plans] CHECK CONSTRAINT [FK_plans_campaigns]
	CREATE NONCLUSTERED INDEX [IX_plans_spot_length_id]  ON [dbo].[plans] ([spot_length_id] ASC)
	INCLUDE ([id], [campaign_id]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

IF NOT EXISTS(SELECT 1 FROM spot_lengths WHERE length = 10)
BEGIN
	INSERT INTO spot_lengths(length, delivery_multiplier, order_by, is_default)
	VALUES(10,0.3,8,0)
	INSERT INTO spot_length_cost_multipliers(spot_length_id, cost_multiplier)
	VALUES((SELECT id FROM spot_lengths WHERE length = 10), 0.3)
END
IF NOT EXISTS(SELECT 1 FROM spot_lengths WHERE length = 150)
BEGIN
	INSERT INTO spot_lengths(length, delivery_multiplier, order_by, is_default)
	VALUES(150,5,9,0)
	INSERT INTO spot_length_cost_multipliers(spot_length_id, cost_multiplier)
	VALUES((SELECT id FROM spot_lengths WHERE length = 150), 5)
END
/*************************************** END PRI-7452 *****************************************************/

/*************************************** START PRI-7456 *****************************************************/

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'flight_start_date' AND Object_ID = OBJECT_ID('plans'))
BEGIN 
	ALTER TABLE plans
		ADD flight_start_date DATETIME NULL
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'flight_end_date' AND Object_ID = OBJECT_ID('plans'))
BEGIN 
	ALTER TABLE plans
		ADD flight_end_date DATETIME NULL
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'flight_notes' AND Object_ID = OBJECT_ID('plans'))
BEGIN 
	ALTER TABLE plans
		ADD flight_notes NVARCHAR(1024) NULL
END

IF OBJECT_ID('plan_flight_hiatus') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_flight_hiatus]
	(
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[plan_id] [INT] NOT NULL,
		[hiatus_day] [DATETIME] NOT NULL,		
		CONSTRAINT [PK_plan_flight_hiatus] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_flight_hiatus] WITH CHECK ADD CONSTRAINT [FK_plan_flight_hiatus_plans] FOREIGN KEY ([plan_id])
		REFERENCES [dbo].[plans] (id)
		ON DELETE CASCADE

	CREATE NONCLUSTERED INDEX [IX_plan_flight_hiatus_plan_id] ON [dbo].[plan_flight_hiatus] ([plan_id] ASC)
		INCLUDE ([id])
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

/*************************************** END PRI-7456 *****************************************************/

/*************************************** START PRI-12160 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'share_playback_type' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_audiences'))
BEGIN
    ALTER TABLE station_inventory_manifest_audiences ADD share_playback_type int NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'hut_playback_type' AND OBJECT_ID = OBJECT_ID(N'station_inventory_manifest_audiences'))
BEGIN
    ALTER TABLE station_inventory_manifest_audiences ADD hut_playback_type int NULL
END
/*************************************** END PRI-12160 *****************************************************/

/*************************************** START PRI-7460 *****************************************************/
	--add audience_type
IF NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('plans') AND name = 'audience_type')
BEGIN
	DECLARE @UpdateAudienceType VARCHAR(1024)
	ALTER TABLE [plans] ADD [audience_type] [INT] NULL
	SET @UpdateAudienceType = 'UPDATE [plans] SET [audience_type] = 1'		--Nielsen
	EXEC (@UpdateAudienceType)
	ALTER TABLE [plans] ALTER COLUMN [audience_type] [INT] NOT NULL
END

	--add posting_type
IF NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('plans') AND name = 'posting_type')
BEGIN
	DECLARE @UpdatePostingType VARCHAR(1024)
	ALTER TABLE [plans] ADD [posting_type] [INT] NULL
	SET @UpdatePostingType = 'UPDATE [plans] SET [posting_type] = 2'		--NTI
	EXEC (@UpdatePostingType)
	ALTER TABLE [plans] ALTER COLUMN [posting_type] [INT] NOT NULL
END

	--add primary_audience_id
IF NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('plans') AND name = 'audience_id')
BEGIN
	DECLARE @UpdateAudience VARCHAR(1024)
	ALTER TABLE [plans] ADD [audience_id] [INT] NULL
	SET @UpdateAudience = 'UPDATE [plans] SET [audience_id] = 31'		--HH
	EXEC (@UpdateAudience)
	ALTER TABLE [plans] ALTER COLUMN [audience_id] [INT] NOT NULL
	ALTER TABLE [dbo].[plans] ADD CONSTRAINT [FK_plans_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id]) ON DELETE CASCADE
END

	--add share_book_id
IF NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('plans') AND name = 'share_book_id')
BEGIN
	DECLARE @UpdateShareBook VARCHAR(1024)
	ALTER TABLE [plans] ADD [share_book_id] [INT] NULL
	SET @UpdateShareBook = 'UPDATE [plans] SET [share_book_id] = 437'		--Feb 2018
	EXEC (@UpdateShareBook)
	ALTER TABLE [plans] ALTER COLUMN [share_book_id] [INT] NOT NULL
	ALTER TABLE [dbo].[plans] ADD CONSTRAINT [FK_plans_media_months] FOREIGN KEY([share_book_id])
	REFERENCES [dbo].[media_months] ([id]) ON DELETE CASCADE
END

	--add hut_book_id
IF NOT EXISTS(SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('plans') AND name = 'hut_book_id')
BEGIN
	ALTER TABLE [plans] ADD [hut_book_id] [INT] NULL
END
/*************************************** END PRI-7460 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.09.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.08.1' -- Previous release version
		OR [version] = '19.09.1') -- Current release version
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