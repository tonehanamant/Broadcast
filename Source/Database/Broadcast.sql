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

/*************************************** START PRI-15898 *****************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id =OBJECT_ID('plan_versions'))
BEGIN
	CREATE TABLE [plan_versions](
		[id] INT IDENTITY(1,1) NOT NULL,
		[plan_id] INT NOT NULL,
		[is_draft] BIT NOT NULL,
		[spot_length_id] [int] NOT NULL,
		[equivalized] [bit] NOT NULL,		
		[flight_start_date] [datetime] NOT NULL,
		[flight_end_date] [datetime] NOT NULL,
		[flight_notes] [nvarchar](1024) NULL,
		[audience_type] [int] NOT NULL,
		[posting_type] [int] NOT NULL,
		[target_audience_id] [int] NOT NULL,
		[share_book_id] [int] NOT NULL,
		[hut_book_id] [int] NULL,
		[budget] [money] NOT NULL,
		[target_impression] [float] NOT NULL,
		[target_cpm] [money] NOT NULL,
		[target_rating_points] [float] NOT NULL,
		[target_cpp] [money] NOT NULL,
		[target_universe] [float] NOT NULL,
		[hh_impressions] [float] NOT NULL,
		[hh_cpm] [money] NOT NULL,
		[hh_rating_points] [float] NOT NULL,
		[hh_cpp] [money] NOT NULL,		
		[hh_universe] [float] NOT NULL,
		[currency] [int] NOT NULL,
		[target_vpvh] [float] NOT NULL,
		[coverage_goal_percent] [float] NOT NULL,
		[goal_breakdown_type] [int] NOT NULL,		
		[status] [int] NOT NULL,
		[created_by] [varchar](63) NOT NULL,
		[created_date] [datetime] NOT NULL,
		[modified_by] [varchar](63) NULL,
		[modified_date] [datetime] NULL
		 CONSTRAINT [PK_plan_versions] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_versions] WITH CHECK ADD CONSTRAINT [FK_plan_versions_plans] FOREIGN KEY ([plan_id])
	REFERENCES [dbo].[plans] (id)
	ALTER TABLE [dbo].[plan_versions] CHECK CONSTRAINT [FK_plan_versions_plans]

	CREATE NONCLUSTERED INDEX [IX_plan_versions_plan_id] ON [dbo].[plan_versions] ([plan_id])

	ALTER TABLE [dbo].[plan_versions] WITH CHECK ADD CONSTRAINT [FK_plan_versions_spot_lengths] FOREIGN KEY ([spot_length_id])
	REFERENCES [dbo].[spot_lengths] (id)
	ALTER TABLE [dbo].[plan_versions] CHECK CONSTRAINT [FK_plan_versions_spot_lengths]

	ALTER TABLE [dbo].[plan_versions] WITH CHECK ADD CONSTRAINT [FK_plan_versions_audiences] FOREIGN KEY ([target_audience_id])
	REFERENCES [dbo].[audiences] (id)
	ALTER TABLE [dbo].[plan_versions] CHECK CONSTRAINT [FK_plan_versions_audiences]

	ALTER TABLE [dbo].[plan_versions] WITH CHECK ADD CONSTRAINT [FK_plan_versions_share_media_months] FOREIGN KEY ([share_book_id])
	REFERENCES [dbo].[media_months] (id)
	ALTER TABLE [dbo].[plan_versions] CHECK CONSTRAINT [FK_plan_versions_share_media_months]

	ALTER TABLE [dbo].[plan_versions] WITH CHECK ADD CONSTRAINT [FK_plan_versions_hut_media_months] FOREIGN KEY ([hut_book_id])
	REFERENCES [dbo].[media_months] (id)
	ALTER TABLE [dbo].[plan_versions] CHECK CONSTRAINT [FK_plan_versions_hut_media_months]
	
	EXEC('INSERT INTO [plan_versions]([plan_id], [is_draft], [spot_length_id], [equivalized], [flight_start_date]
			,[flight_end_date], [flight_notes], [audience_type], [posting_type], [target_audience_id], [share_book_id], [hut_book_id]
			,[budget], [target_impression], [target_cpm], [target_rating_points], [target_cpp], [target_universe]
			,[hh_impressions], [hh_cpm], [hh_rating_points], [hh_cpp], [hh_universe], [currency], [target_vpvh]
			,[coverage_goal_percent], [goal_breakdown_type], [status], [created_by], [created_date], [modified_by] ,[modified_date])
		SELECT id, 0 as [is_draft], spot_length_id, equivalized, flight_start_date
			,flight_end_date, flight_notes, audience_type, posting_type, audience_id, share_book_id, hut_book_id
			,budget, delivery_impressions, cpm, delivery_rating_points, cpp, universe
			,household_delivery_impressions, household_cpm, household_rating_points, household_cpp, household_universe, currency, vpvh
			,coverage_goal_percent, goal_breakdown_type, status, created_by, created_date, modified_by ,modified_date
		FROM [plans]')
END

--add latest version id to plans and populate with data
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id =OBJECT_ID('plans') AND name = 'latest_version_id')
BEGIN
	ALTER TABLE [plans] ADD [latest_version_id] INT NULL

	EXEC('UPDATE t1 
			SET t1.latest_version_id = t2.id
			FROM plans AS t1
			INNER JOIN plan_versions AS t2 ON t1.id = t2.plan_id')

	ALTER TABLE [plans] ALTER COLUMN [latest_version_id] INT NOT NULL
END

--rename columns
IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id =OBJECT_ID('plan_summary_quarters') AND name = 'plan_summary_id')
BEGIN
	EXEC sp_rename 'dbo.plan_summary_quarters.plan_summary_id', 'plan_version_summary_id', 'COLUMN';
END

--rename primary keys
IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name = 'PK_plan_dayparts' AND parent_object_id =OBJECT_ID('plan_dayparts'))
BEGIN	
	EXEC sp_rename 'dbo.plan_dayparts.PK_plan_dayparts', 'PK_plan_version_dayparts'
END
IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name = 'PK_plan_available_markets' AND parent_object_id =OBJECT_ID('plan_available_markets'))
BEGIN	
	EXEC sp_rename 'dbo.plan_available_markets.PK_plan_available_markets', 'PK_plan_version_available_markets'
END
IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name = 'PK_plan_blackout_markets' AND parent_object_id =OBJECT_ID('plan_blackout_markets'))
BEGIN	
	EXEC sp_rename 'dbo.plan_blackout_markets.PK_plan_blackout_markets', 'PK_plan_version_blackout_markets'
END
IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name = 'PK_plan_flight_hiatus' AND parent_object_id =OBJECT_ID('plan_flight_hiatus'))
BEGIN	
	EXEC sp_rename 'dbo.plan_flight_hiatus.PK_plan_flight_hiatus', 'PK_plan_version_flight_hiatus'
END
IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name = 'PK_plan_secondary_audiences' AND parent_object_id =OBJECT_ID('plan_secondary_audiences'))
BEGIN	
	EXEC sp_rename 'dbo.plan_secondary_audiences.PK_plan_secondary_audiences', 'PK_plan_version_secondary_audiences'
END
IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name = 'PK_plan_summaries' AND parent_object_id =OBJECT_ID('plan_summaries'))
BEGIN	
	EXEC sp_rename 'dbo.plan_summaries.PK_plan_summaries', 'PK_plan_version_summaries'
END
IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name = 'PK_plan_weeks' AND parent_object_id =OBJECT_ID('plan_weeks'))
BEGIN	
	EXEC sp_rename 'dbo.plan_weeks.PK_plan_weeks', 'PK_plan_version_weeks'
END
IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name = 'PK_plan_summary_quarters' AND parent_object_id =OBJECT_ID('plan_summary_quartes'))
BEGIN	
	EXEC sp_rename 'dbo.plan_summary_quartes.PK_plan_summary_quarters', 'PK_plan_version_summary_quarters'
END

--rename primary key indexes
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_plan_summary_quarters_plan_summary_id' AND object_id =OBJECT_ID('plan_summary_quartes'))
BEGIN	
	EXEC sp_rename 'dbo.plan_summary_quartes.IX_plan_summary_quarters_plan_summary_id', 'IX_plan_version_summary_quarters_plan_summary_id'
END

--rename foreign keys
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'FK_plan_dayparts_daypart_codes' AND object_id =OBJECT_ID('plan_dayparts'))
BEGIN	
	EXEC sp_rename 'dbo.plan_dayparts.FK_plan_dayparts_daypart_codes', 'FK_plan_version_dayparts_daypart_codes'
END
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'FK_plan_weeks_media_weeks' AND object_id =OBJECT_ID('plan_weeks'))
BEGIN	
	EXEC sp_rename 'dbo.plan_weeks.FK_plan_weeks_media_weeks', 'FK_plan_version_weeks_media_weeks'
END
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'FK_plan_summary_quarters_plan_summary' AND object_id =OBJECT_ID('plan_summary_quartes'))
BEGIN	
	EXEC sp_rename 'dbo.plan_summary_quartes.FK_plan_summary_quarters_plan_summary', 'FK_plan_version_summary_quarters_plan_summary'
END
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'FK_plan_available_markets_market_coverage_file' AND object_id =OBJECT_ID('plan_available_markets'))
BEGIN	
	EXEC sp_rename 'dbo.plan_available_markets.FK_plan_available_markets_market_coverage_file', 'FK_plan_version_available_markets_market_coverage_file'
END
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'FK_plan_available_markets_markets' AND object_id =OBJECT_ID('plan_available_markets'))
BEGIN	
	EXEC sp_rename 'dbo.plan_available_markets.FK_plan_available_markets_markets', 'FK_plan_version_available_markets_markets'
END
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'FK_plan_blackout_markets_market_coverage_file' AND object_id =OBJECT_ID('plan_blackout_markets'))
BEGIN	
	EXEC sp_rename 'dbo.plan_blackout_markets.FK_plan_blackout_markets_market_coverage_file', 'FK_plan_version_blackout_markets_market_coverage_file'
END
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'FK_plan_blackout_markets_markets' AND object_id =OBJECT_ID('plan_blackout_markets'))
BEGIN	
	EXEC sp_rename 'dbo.plan_blackout_markets.FK_plan_blackout_markets_markets', 'FK_plan_version_blackout_markets_markets'
END
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'FK_plan_secondary_audiences_audiences' AND object_id =OBJECT_ID('plan_secondary_audiences'))
BEGIN
	EXEC sp_rename 'dbo.plan_secondary_audiences.FK_plan_secondary_audiences_audiences', 'FK_plan_version_secondary_audiences_audiences'
END

--rename tables
IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('plan_dayparts'))
BEGIN	
	EXEC sp_rename 'dbo.plan_dayparts', 'plan_version_dayparts'
END
IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('plan_available_markets'))
BEGIN	
	EXEC sp_rename 'dbo.plan_available_markets', 'plan_version_available_markets'
END
IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('plan_blackout_markets'))
BEGIN	
	EXEC sp_rename 'dbo.plan_blackout_markets', 'plan_version_blackout_markets'
END
IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('plan_flight_hiatus'))
BEGIN	
	EXEC sp_rename 'dbo.plan_flight_hiatus', 'plan_version_flight_hiatus_days'
END
IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('plan_secondary_audiences'))
BEGIN	
	EXEC sp_rename 'dbo.plan_secondary_audiences', 'plan_version_secondary_audiences'
END
IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('plan_summaries'))
BEGIN	
	EXEC sp_rename 'dbo.plan_summaries', 'plan_version_summaries'
END
IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('plan_summary_quarters'))
BEGIN	
	EXEC sp_rename 'dbo.plan_summary_quarters', 'plan_version_summary_quarters'
END
IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('plan_weeks'))
BEGIN	
	EXEC sp_rename 'dbo.plan_weeks', 'plan_version_weeks'
END

--add plan_version_id columns to tables
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id' AND  object_id = OBJECT_ID('plan_version_dayparts'))
BEGIN
	ALTER TABLE [plan_version_dayparts] ADD [plan_version_id] INT NULL
	EXEC(' UPDATE t1 
			SET t1.plan_version_id = t2.id
			FROM plan_version_dayparts as t1
			INNER JOIN plan_versions as t2 ON t1.plan_id = t2.plan_id')
	ALTER TABLE [plan_version_dayparts] ALTER COLUMN [plan_version_id] INT NOT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id' AND  object_id = OBJECT_ID('plan_version_available_markets'))
BEGIN
	ALTER TABLE [plan_version_available_markets] ADD [plan_version_id] INT NULL
	EXEC(' UPDATE t1 
			SET t1.plan_version_id = t2.id
			FROM plan_version_available_markets as t1
			INNER JOIN plan_versions as t2 ON t1.plan_id = t2.plan_id')
	ALTER TABLE [plan_version_available_markets] ALTER COLUMN [plan_version_id] INT NOT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id' AND  object_id = OBJECT_ID('plan_version_blackout_markets'))
BEGIN
	ALTER TABLE [plan_version_blackout_markets] ADD [plan_version_id] INT NULL
	EXEC(' UPDATE t1 
			SET t1.plan_version_id = t2.id
			FROM plan_version_blackout_markets as t1
			INNER JOIN plan_versions as t2 ON t1.plan_id = t2.plan_id')
	ALTER TABLE [plan_version_blackout_markets] ALTER COLUMN [plan_version_id] INT NOT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id' AND  object_id = OBJECT_ID('plan_version_flight_hiatus_days'))
BEGIN
	ALTER TABLE [plan_version_flight_hiatus_days] ADD [plan_version_id] INT NULL
	EXEC(' UPDATE t1 
			SET t1.plan_version_id = t2.id
			FROM plan_version_flight_hiatus_days as t1
			INNER JOIN plan_versions as t2 ON t1.plan_id = t2.plan_id')
	ALTER TABLE [plan_version_flight_hiatus_days] ALTER COLUMN [plan_version_id] INT NOT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id' AND  object_id = OBJECT_ID('plan_version_secondary_audiences'))
BEGIN
	ALTER TABLE [plan_version_secondary_audiences] ADD [plan_version_id] INT NULL
	EXEC(' UPDATE t1 
			SET t1.plan_version_id = t2.id
			FROM plan_version_secondary_audiences as t1
			INNER JOIN plan_versions as t2 ON t1.plan_id = t2.plan_id')
	ALTER TABLE [plan_version_secondary_audiences] ALTER COLUMN [plan_version_id] INT NOT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id' AND  object_id = OBJECT_ID('plan_version_summaries'))
BEGIN
	ALTER TABLE [plan_version_summaries] ADD [plan_version_id] INT NULL
	EXEC(' UPDATE t1 
			SET t1.plan_version_id = t2.id
			FROM plan_version_summaries as t1
			INNER JOIN plan_versions as t2 ON t1.plan_id = t2.plan_id')
	ALTER TABLE [plan_version_summaries] ALTER COLUMN [plan_version_id] INT NOT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id' AND  object_id = OBJECT_ID('plan_version_weeks'))
BEGIN
	ALTER TABLE [plan_version_weeks] ADD [plan_version_id] INT NULL
	EXEC(' UPDATE t1 
			SET t1.plan_version_id = t2.id
			FROM plan_version_weeks as t1
			INNER JOIN plan_versions as t2 ON t1.plan_id = t2.plan_id')
	ALTER TABLE [plan_version_weeks] ALTER COLUMN [plan_version_id] INT NOT NULL
END

--add plan_version_id FKs to tables
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_dayparts') AND name = 'FK_plan_version_dayparts_plan_versions')
BEGIN
	ALTER TABLE [dbo].[plan_version_dayparts] WITH CHECK ADD CONSTRAINT [FK_plan_version_dayparts_plan_versions] FOREIGN KEY ([plan_version_id])
	REFERENCES [dbo].[plan_versions] (id)
	CREATE NONCLUSTERED INDEX [IX_plan_version_dayparts_plan_version_id] ON [dbo].[plan_version_dayparts] ([plan_version_id])
END
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_available_markets') AND name = 'FK_plan_version_available_markets_plan_versions')
BEGIN
	ALTER TABLE [dbo].[plan_version_available_markets] WITH CHECK ADD CONSTRAINT [FK_plan_version_available_markets_plan_versions] FOREIGN KEY ([plan_version_id])
	REFERENCES [dbo].[plan_versions] (id)
	CREATE NONCLUSTERED INDEX [IX_plan_version_available_markets_plan_version_id] ON [dbo].[plan_version_available_markets] ([plan_version_id])
END
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_blackout_markets') AND name = 'FK_plan_version_blackout_markets_plan_versions')
BEGIN
	ALTER TABLE [dbo].[plan_version_blackout_markets] WITH CHECK ADD CONSTRAINT [FK_plan_version_blackout_markets_plan_versions] FOREIGN KEY ([plan_version_id])
	REFERENCES [dbo].[plan_versions] (id)
	CREATE NONCLUSTERED INDEX [IX_plan_version_blackout_markets_plan_version_id] ON [dbo].[plan_version_blackout_markets] ([plan_version_id])
END
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_flight_hiatus_days') AND name = 'FK_plan_version_flight_hiatus_days_plan_versions')
BEGIN
	ALTER TABLE [dbo].[plan_version_flight_hiatus_days] WITH CHECK ADD CONSTRAINT [FK_plan_version_flight_hiatus_days_plan_versions] FOREIGN KEY ([plan_version_id])
	REFERENCES [dbo].[plan_versions] (id)
	CREATE NONCLUSTERED INDEX [IX_plan_version_flight_hiatus_days_plan_version_id] ON [dbo].[plan_version_flight_hiatus_days] ([plan_version_id])
END
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_secondary_audiences') AND name = 'FK_plan_version_secondary_audiences_plan_versions')
BEGIN
	ALTER TABLE [dbo].[plan_version_secondary_audiences] WITH CHECK ADD CONSTRAINT [FK_plan_version_secondary_audiences_plan_versions] FOREIGN KEY ([plan_version_id])
	REFERENCES [dbo].[plan_versions] (id)
	CREATE NONCLUSTERED INDEX [IX_plan_version_secondary_audiences_plan_version_id] ON [dbo].[plan_version_secondary_audiences] ([plan_version_id])
END
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_summaries') AND name = 'FK_plan_version_summaries_plan_versions')
BEGIN
	ALTER TABLE [dbo].[plan_version_summaries] WITH CHECK ADD CONSTRAINT [FK_plan_version_summaries_plan_versions] FOREIGN KEY ([plan_version_id])
	REFERENCES [dbo].[plan_versions] (id)
	CREATE NONCLUSTERED INDEX [IX_plan_version_summaries_plan_version_id] ON [dbo].[plan_version_summaries] ([plan_version_id])
END
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_weeks') AND name = 'FK_plan_version_weeks_plan_versions')
BEGIN
	ALTER TABLE [dbo].[plan_version_weeks] WITH CHECK ADD CONSTRAINT [FK_plan_version_weeks_plan_versions] FOREIGN KEY ([plan_version_id])
	REFERENCES [dbo].[plan_versions] (id)
	CREATE NONCLUSTERED INDEX [IX_plan_version_weeks_plan_version_id] ON [dbo].[plan_version_weeks] ([plan_version_id])
END

--if the insert of the data was successfull, we're removing the columns and the FKs from plans table
IF (SELECT COUNT(*) FROM [plans]) = (SELECT COUNT(*) FROM [plan_versions])
BEGIN
	--remove FKs	
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plans') AND name = 'FK_plans_audiences')
	BEGIN
		ALTER TABLE [plans] DROP CONSTRAINT [FK_plans_audiences]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plans') AND name = 'FK_plans_media_months')
	BEGIN
		ALTER TABLE [plans] DROP CONSTRAINT [FK_plans_media_months]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plans') AND name = 'FK_plans_media_months_hut')
	BEGIN
		ALTER TABLE [plans] DROP CONSTRAINT [FK_plans_media_months_hut]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plans') AND name = 'FK_plans_spot_lengths')
	BEGIN
		ALTER TABLE [plans] DROP CONSTRAINT [FK_plans_spot_lengths]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_dayparts') AND name = 'FK_plan_dayparts_plans')
	BEGIN
		ALTER TABLE [plan_version_dayparts] DROP CONSTRAINT [FK_plan_dayparts_plans]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_available_markets') AND name = 'FK_plan_available_markets_plans')
	BEGIN
		ALTER TABLE [plan_version_available_markets] DROP CONSTRAINT [FK_plan_available_markets_plans]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_blackout_markets') AND name = 'FK_plan_blackout_markets_plans')
	BEGIN
		ALTER TABLE [plan_version_blackout_markets] DROP CONSTRAINT [FK_plan_blackout_markets_plans]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_flight_hiatus_days') AND name = 'FK_plan_flight_hiatus_plans')
	BEGIN
		ALTER TABLE [plan_version_flight_hiatus_days] DROP CONSTRAINT [FK_plan_flight_hiatus_plans]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_secondary_audiences') AND name = 'FK_plan_secondary_audiences_plans')
	BEGIN
		ALTER TABLE [plan_version_secondary_audiences] DROP CONSTRAINT [FK_plan_secondary_audiences_plans]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_summaries') AND name = 'FK_plan_summary_plans')
	BEGIN
		ALTER TABLE [plan_version_summaries] DROP CONSTRAINT [FK_plan_summary_plans]
	END
	IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_weeks') AND name = 'FK_plan_weeks_plans')
	BEGIN
		ALTER TABLE [plan_version_weeks] DROP CONSTRAINT [FK_plan_weeks_plans]
	END
	
	--remove indexes
	IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('plans') AND name = 'IX_plans_spot_length_id')
	BEGIN		
		DROP INDEX [plans].[IX_plans_spot_length_id]
	END
	IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_plan_dayparts_plan_id' AND object_id =OBJECT_ID('plan_version_dayparts'))
	BEGIN	
		DROP INDEX [plan_version_dayparts].[IX_plan_dayparts_plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_plan_available_markets_plan_id' AND object_id =OBJECT_ID('plan_version_available_markets'))
	BEGIN	
		DROP INDEX [plan_version_available_markets].[IX_plan_available_markets_plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_plan_blackout_markets_plan_id' AND object_id =OBJECT_ID('plan_version_blackout_markets'))
	BEGIN	
		DROP INDEX [plan_version_blackout_markets].[IX_plan_blackout_markets_plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_plan_flight_hiatus_plan_id' AND object_id =OBJECT_ID('plan_version_flight_hiatus_days'))
	BEGIN	
		DROP INDEX [plan_version_flight_hiatus_days].[IX_plan_flight_hiatus_plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_plan_secondary_audiences_plan_id' AND object_id =OBJECT_ID('plan_version_secondary_audiences'))
	BEGIN	
		DROP INDEX [plan_version_secondary_audiences].[IX_plan_secondary_audiences_plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_plan_summary_plan_id' AND object_id =OBJECT_ID('plan_version_summaries'))
	BEGIN	
		DROP INDEX [plan_version_summaries].[IX_plan_summary_plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_plan_weeks_plan_id' AND object_id =OBJECT_ID('plan_version_weeks'))
	BEGIN	
		DROP INDEX [plan_version_weeks].[IX_plan_weeks_plan_id]
	END

	--remove columns moved to plan_versions table	
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'spot_length_id' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [spot_length_id]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'equivalized' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [equivalized]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'flight_start_date' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [flight_start_date]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'flight_end_date' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [flight_end_date]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'flight_notes' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [flight_notes]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'audience_type' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [audience_type]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'posting_type' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [posting_type]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'audience_id' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [audience_id]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'share_book_id' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [share_book_id]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'hut_book_id' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [hut_book_id]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'budget' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [budget]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'delivery_impressions' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [delivery_impressions]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'cpm' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [cpm]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'delivery_rating_points' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [delivery_rating_points]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'cpp' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [cpp]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'universe' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [universe]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'household_delivery_impressions' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [household_delivery_impressions]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'household_cpm' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [household_cpm]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'household_rating_points' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [household_rating_points]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'household_cpp' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [household_cpp]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'household_universe' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [household_universe]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'currency' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [currency]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'vpvh' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [vpvh]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'coverage_goal_percent' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [coverage_goal_percent]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'goal_breakdown_type' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [goal_breakdown_type]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'status' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [status]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'created_by' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [created_by]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'created_date' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [created_date]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'modified_by' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [modified_by]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'modified_date' and object_id = OBJECT_ID('plans'))
	BEGIN
		ALTER TABLE [plans] DROP COLUMN [modified_date]
	END	

	--remove plan_id column because it is not used anymore
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_id' and object_id = OBJECT_ID('plan_version_dayparts'))
	BEGIN
		ALTER TABLE [plan_version_dayparts] DROP COLUMN [plan_id]
	END	
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_id' and object_id = OBJECT_ID('plan_version_available_markets'))
	BEGIN
		ALTER TABLE [plan_version_available_markets] DROP COLUMN [plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_id' and object_id = OBJECT_ID('plan_version_blackout_markets'))
	BEGIN
		ALTER TABLE [plan_version_blackout_markets] DROP COLUMN [plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_id' and object_id = OBJECT_ID('plan_version_flight_hiatus_days'))
	BEGIN
		ALTER TABLE [plan_version_flight_hiatus_days] DROP COLUMN [plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_id' and object_id = OBJECT_ID('plan_version_secondary_audiences'))
	BEGIN
		ALTER TABLE [plan_version_secondary_audiences] DROP COLUMN [plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_id' and object_id = OBJECT_ID('plan_version_summaries'))
	BEGIN
		ALTER TABLE [plan_version_summaries] DROP COLUMN [plan_id]
	END
	IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_id' and object_id = OBJECT_ID('plan_version_weeks'))
	BEGIN
		ALTER TABLE [plan_version_weeks] DROP COLUMN [plan_id]
	END
END
/*************************************** END PRI-15898 *****************************************************/

/*************************************** Start PRI-17556 ***************************************************/

UPDATE Genres SET
	[name] = 'Science & Technology'
	, modified_date = SYSDATETIME()
	, modified_by = 'PRI-17556'
WHERE [name] = 'Science & Techonology'

/*************************************** END PRI-17556 *****************************************************/

/*************************************** START PRI-16343 *****************************************************/

IF OBJECT_ID ('dbo.station_inventory_manifest_daypart_programs') IS NULL
BEGIN
	CREATE TABLE [station_inventory_manifest_daypart_programs]
	(
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[station_inventory_manifest_daypart_id] [INT] NOT NULL,		
		[name] [VARCHAR](255) NOT NULL,
		[show_type] [VARCHAR](30) NOT NULL,
		[genre_id] [INT] NOT NULL,
		[start_date] [DATE] NOT NULL,
		[end_date] [DATE] NOT NULL,
		[start_time] [INT] NOT NULL,
		[end_time] [INT] NOT NULL,
		[created_date] [DATETIME] NOT NULL
		CONSTRAINT [PK_station_inventory_manifest_daypart_programs] PRIMARY KEY CLUSTERED
		(
			[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[station_inventory_manifest_daypart_programs] WITH CHECK ADD CONSTRAINT [FK_station_inventory_manifest_daypart_programs_station_inventory_manifest_dayparts] FOREIGN KEY ([station_inventory_manifest_daypart_id])
		REFERENCES [dbo].[station_inventory_manifest_dayparts] (id)
		ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_manifest_daypart_programs] CHECK CONSTRAINT [FK_station_inventory_manifest_daypart_programs_station_inventory_manifest_dayparts]

	ALTER TABLE [dbo].[station_inventory_manifest_daypart_programs] WITH CHECK ADD CONSTRAINT [FK_station_inventory_manifest_daypart_programs_genres] FOREIGN KEY ([genre_id])
		REFERENCES [dbo].[genres] (id)

	ALTER TABLE [dbo].[station_inventory_manifest_daypart_programs] CHECK CONSTRAINT [FK_station_inventory_manifest_daypart_programs_genres]

	CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_daypart_programs_dayparts] ON [dbo].[station_inventory_manifest_daypart_programs] ([station_inventory_manifest_daypart_id])

END

GO

IF OBJECT_ID('dbo.inventory_file_program_names_jobs') IS NOT NULL
BEGIN
	DROP TABLE inventory_file_program_names_jobs
END

GO

IF OBJECT_ID('dbo.inventory_file_program_enrichment_jobs') IS NULL
BEGIN
	CREATE TABLE [dbo].[inventory_file_program_enrichment_jobs](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[inventory_file_id] [int] NOT NULL,
		[status] [int] NOT NULL,
		[error_message] [nvarchar](2000) NULL,
		[queued_at] [datetime] NOT NULL,
		[queued_by] [varchar](50) NOT NULL,
		[completed_at] [datetime] NULL,
	 CONSTRAINT [PK_inventory_file_program_enrichment_jobs] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_file_program_enrichment_jobs]  WITH CHECK ADD  CONSTRAINT [FK_inventory_file_program_enrichment_jobs_inventory_file] FOREIGN KEY([inventory_file_id])
	REFERENCES [dbo].[inventory_files] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[inventory_file_program_enrichment_jobs] CHECK CONSTRAINT [FK_inventory_file_program_enrichment_jobs_inventory_file]

END

GO

/*************************************** END PRI-16343 *****************************************************/

/*************************************** START - PRI-16393 ****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_dayparts') AND name = 'show_type_restrictions_contain_type')
BEGIN
	ALTER TABLE [plan_version_dayparts] ADD [show_type_restrictions_contain_type] INT NULL
END

IF OBJECT_ID('plan_version_daypart_show_type_restrictions') IS NULL
BEGIN 
	CREATE TABLE [plan_version_daypart_show_type_restrictions]
	(
		[id] INT IDENTITY(1,1) NOT NULL,
		[plan_version_daypart_id] INT NOT NULL,
		[show_type_id] INT NOT NULL

		CONSTRAINT [PK_plan_version_daypart_show_type_restrictions] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)

	ALTER TABLE [dbo].[plan_version_daypart_show_type_restrictions] WITH CHECK ADD CONSTRAINT [FK_plan_version_daypart_show_type_restrictions_plan_version_dayparts] FOREIGN KEY([plan_version_daypart_id])
	REFERENCES [dbo].[plan_version_dayparts] ([id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[plan_version_daypart_show_type_restrictions] CHECK CONSTRAINT [FK_plan_version_daypart_show_type_restrictions_plan_version_dayparts]

	ALTER TABLE [dbo].[plan_version_daypart_show_type_restrictions] WITH CHECK ADD CONSTRAINT [FK_plan_version_daypart_show_type_restrictions_show_types] FOREIGN KEY([show_type_id])
	REFERENCES [dbo].[show_types] ([id])
	ALTER TABLE [dbo].[plan_version_daypart_show_type_restrictions] CHECK CONSTRAINT [FK_plan_version_daypart_show_type_restrictions_show_types]

	ALTER TABLE [dbo].[plan_version_daypart_show_type_restrictions] WITH CHECK ADD CONSTRAINT [UQ_plan_version_daypart_show_type_restrictions] UNIQUE ([plan_version_daypart_id], [show_type_id])
END

/*************************************** END - PRI-16393 ****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.12.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.11.1' -- Previous release version
		OR [version] = '19.12.1') -- Current release version
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