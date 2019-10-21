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

/*************************************** START PRI-15991 BE_Calculate Contract Details for Secondary Audiences *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_secondary_audiences') AND name = 'delivery_rating_points')
BEGIN
	ALTER TABLE [plan_secondary_audiences] ADD [delivery_rating_points] [FLOAT] NULL

	EXEC('UPDATE plan_secondary_audiences
	SET delivery_rating_points = 0
	WHERE delivery_rating_points IS NULL')

	ALTER TABLE [plan_secondary_audiences]
	ALTER COLUMN [delivery_rating_points] [FLOAT] NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_secondary_audiences') AND name = 'delivery_impressions')
BEGIN
	ALTER TABLE [plan_secondary_audiences] ADD [delivery_impressions] [FLOAT] NULL

	EXEC('UPDATE plan_secondary_audiences
	SET delivery_impressions = 0
	WHERE delivery_impressions IS NULL')

	ALTER TABLE [plan_secondary_audiences]
	ALTER COLUMN [delivery_impressions] [FLOAT] NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_secondary_audiences') AND name = 'cpm')
BEGIN
	ALTER TABLE [plan_secondary_audiences] ADD [cpm] [MONEY] NULL

	EXEC('UPDATE plan_secondary_audiences
	SET cpm = 0
	WHERE cpm IS NULL')

	ALTER TABLE [plan_secondary_audiences]
	ALTER COLUMN [cpm] [MONEY] NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_secondary_audiences') AND name = 'cpp')
BEGIN
	ALTER TABLE [plan_secondary_audiences] ADD [cpp] [FLOAT] NULL
	
	EXEC('UPDATE plan_secondary_audiences
	SET cpp = 0
	WHERE cpp IS NULL')

	ALTER TABLE [plan_secondary_audiences]
	ALTER COLUMN [cpp] [FLOAT] NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_secondary_audiences') AND name = 'universe')
BEGIN
	ALTER TABLE [plan_secondary_audiences] ADD [universe] [FLOAT] NULL

	EXEC('UPDATE plan_secondary_audiences
	SET universe = 0
	WHERE universe IS NULL')

	ALTER TABLE [plan_secondary_audiences]
	ALTER COLUMN [universe] FLOAT NOT NULL
END

/*************************************** END PRI-15991 BE_Calculate Contract Details for Secondary Audiences *******************************************************/


/*************************************** START - PRI-15494 ****************************************************/
UPDATE plan_weeks
SET active_days_label = 'M-Su'
WHERE number_active_days = 7
/**************************************** END - PRI-15494 *****************************************************/

/*************************************** START - PRI-16652 ****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.COLUMNS WHERE object_id = OBJECT_ID('inventory_files') AND name = 'effective_date')
BEGIN
	ALTER TABLE inventory_files ADD effective_date datetime2 NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.COLUMNS WHERE object_id = OBJECT_ID('inventory_files') AND name = 'end_date')
BEGIN
	ALTER TABLE inventory_files ADD end_date datetime2 NULL
END
/**************************************** END - PRI-16652 *****************************************************/

/*************************************** START PRI 16342 : Program API - send request update out the api *****************************************************/

IF OBJECT_ID('inventory_file_program_names_jobs') IS NULL
BEGIN 
	CREATE TABLE [inventory_file_program_names_jobs]
	(
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[inventory_file_id] [INT] NOT NULL,
		[status] [INT] NOT NULL,
		[error_message] [NVARCHAR](2000) NULL,
		[queued_at] [datetime] NOT NULL,
		[queued_by] [VARCHAR](50) NOT NULL,
		[completed_at] [datetime] NULL
		CONSTRAINT [PK_inventory_file_program_names_jobs] PRIMARY KEY CLUSTERED
		(
			[id] ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_file_program_names_jobs] WITH CHECK ADD CONSTRAINT [FK_inventory_file_program_names_jobs_inventory_file] FOREIGN KEY ([inventory_file_id])
			REFERENCES [dbo].[inventory_files] (id)
			ON DELETE CASCADE

	CREATE NONCLUSTERED INDEX [IX_inventory_file_program_names_jobs_inventory_file_id] ON [dbo].[inventory_file_program_names_jobs] ([inventory_file_id] ASC)
		INCLUDE ([id])
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

/*************************************** END PRI 16342 : Program API - send request update out the api *****************************************************/

/*************************************** START - PRI-16262 Plan Details - Daypart display Part 2- Additional Details ****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_dayparts') AND name = 'is_start_time_modified')
BEGIN
	ALTER TABLE [plan_dayparts] ADD [is_start_time_modified] [BIT] NULL

	EXEC('UPDATE t2
			SET t2.is_start_time_modified = (CASE WHEN t1.default_start_time_seconds - 1 = t2.start_time_seconds THEN 0 ELSE 1 END)
			FROM daypart_codes AS t1
			INNER JOIN plan_dayparts AS t2 ON t1.id = t2.daypart_code_id
			WHERE t2.is_start_time_modified IS NULL')

	ALTER TABLE [plan_dayparts]
	ALTER COLUMN [is_start_time_modified] [BIT] NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_dayparts') AND name = 'is_end_time_modified')
BEGIN
	ALTER TABLE [plan_dayparts] ADD [is_end_time_modified] [BIT] NULL

	EXEC('UPDATE t2
			SET t2.is_end_time_modified = (CASE WHEN t1.default_end_time_seconds - 1 = t2.end_time_seconds THEN 0 ELSE 1 END)
			FROM daypart_codes AS t1
			INNER JOIN plan_dayparts AS t2 ON t1.id = t2.daypart_code_id
			WHERE t2.is_end_time_modified IS NULL')

	ALTER TABLE [plan_dayparts]
	ALTER COLUMN [is_end_time_modified] [BIT] NOT NULL
END
/**************************************** END - PRI-16262 Plan Details - Daypart display Part 2- Additional Details *****************************************************/

/*************************************** START - PRI-16044 ****************************************************/
-- genre_sources
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('genre_sources'))
BEGIN

	CREATE TABLE genre_sources (
		id INT IDENTITY PRIMARY KEY,
		name VARCHAR(50) NOT NULL
	)

	INSERT INTO genre_sources
	VALUES ('Maestro'), ('Dativa')

END

-- Genres
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('genres') AND name = 'source_id')
BEGIN
	ALTER TABLE genres
	ADD source_id INT 

	ALTER TABLE genres
	ADD FOREIGN KEY (source_id) REFERENCES genre_sources(id);

	EXEC('UPDATE genres 
	SET source_id = 1
	WHERE source_id IS NULL')

	ALTER TABLE genres
	ALTER COLUMN source_id INT NOT NULL

	-- Cadent genres
	EXEC('UPDATE genres SET name = ''Sports/Sports Talk'' WHERE name = ''Sports''')

	EXEC('INSERT INTO genres (name, created_by, created_date, modified_by, modified_date, source_id)
	VALUES (''Action/Adventure'', ''System'', GETDATE(), ''System'', GETDATE(), 1),
	(''Children'', ''System'', GETDATE(), ''System'', GETDATE(), 1),
	(''Educational'', ''System'', GETDATE(), ''System'', GETDATE(), 1),
	(''Lifestyle'', ''System'', GETDATE(), ''System'', GETDATE(), 1),
	(''Paid Program'', ''System'', GETDATE(), ''System'', GETDATE(), 1),
	(''Special'', ''System'', GETDATE(), ''System'', GETDATE(), 1);')

	-- WWTV genres
	EXEC('INSERT INTO genres (name, created_by, created_date, modified_by, modified_date, source_id)
	VALUES (''Family'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Comedy'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Crime'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Investigative'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Documentary'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Adult'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Anthology'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Drama'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Fantasy'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Mystery'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Romance'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Western'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''History'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Science & Techonology'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Entertainment'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Interview'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Musical'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Competition'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Game Show'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Informational'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Profile'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Auction'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Beauty & Fashion'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Do It Yourself'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Food'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Fitness & Exercise'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Health & Medicine'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Inspirational'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Instructional'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Music'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Travel'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Nature'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Business'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''News'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Politics'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Home Shopping'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Reality'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Religious'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Awards'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Holiday'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Parade'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Performance'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Telethon'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Sports'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Talk Show'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Science Fiction'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Action'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Adventure'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Horror'', ''System'', GETDATE(), ''System'', GETDATE(), 2),
	(''Thriller & Suspense'', ''System'', GETDATE(), ''System'', GETDATE(), 2)')

END

-- genre_mappings
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('genre_mappings'))
BEGIN
	CREATE TABLE genre_mappings (
		maestro_genre_id INT NOT NULL,
		mapped_genre_id INT NOT NULL,
		created_by VARCHAR(63) NOT NULL,
		created_date DATETIME NOT NULL,
		modified_by VARCHAR(63) NOT NULL,
		modified_date DATETIME NOT NULL,
		PRIMARY KEY (maestro_genre_id, mapped_genre_id),
		FOREIGN KEY (maestro_genre_id) REFERENCES genres(id),
		FOREIGN KEY (mapped_genre_id) REFERENCES genres(id),
	)

	EXEC('INSERT INTO genre_mappings
	VALUES ((SELECT id FROM genres WHERE source_id = 1 AND name = ''Children''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Family''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Comedy''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Comedy''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Crime''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Crime''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Crime''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Investigative''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Documentary''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Documentary''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Drama''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Adult''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Drama''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Anthology''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Drama''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Drama''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Drama''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Fantasy''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Drama''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Mystery''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Drama''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Romance''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Drama''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Western''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Educational''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''History''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Educational''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Science & Techonology''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Entertainment''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Entertainment''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Entertainment''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Interview''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Entertainment''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Musical''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Game Show''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Game Show''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Informational''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Informational''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Informational''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Profile''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Lifestyle''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Auction''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Lifestyle''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Beauty & Fashion''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Lifestyle''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Do It Yourself''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Lifestyle''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Food''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Lifestyle''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Fitness & Exercise''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Lifestyle''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Health & Medicine''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Lifestyle''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Inspirational''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Lifestyle''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Instructional''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Lifestyle''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Music''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Lifestyle''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Travel''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Nature''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Nature''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''News''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Business''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''News''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''News''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''News''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Politics''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Paid Program''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Home Shopping''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Reality''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Competition''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Reality''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Reality''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Religious''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Religious''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Special''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Awards''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Special''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Holiday''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Special''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Parade''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Special''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Performance''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Special''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Telethon''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Sports/Sports Talk''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Sports''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Talk Show''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Talk Show''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Science Fiction''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Science Fiction''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Action/Adventure''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Action''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Action/Adventure''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Adventure''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Horror''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Horror''), ''System'', GETDATE(), ''System'', GETDATE()),
	((SELECT id FROM genres WHERE source_id = 1 AND name = ''Horror''), (SELECT id FROM genres WHERE source_id = 2 AND name = ''Thriller & Suspense''), ''System'', GETDATE(), ''System'', GETDATE())')

END
/**************************************** END - PRI-16044 *****************************************************/

/*************************************** START PRI-17160 BE - Fix PlanStatuses and CampaignStatus *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('campaign_summaries') AND name = 'plan_status_count_scenario')
BEGIN
	ALTER TABLE [campaign_summaries] ADD [plan_status_count_scenario] [INT] NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('campaign_summaries') AND name = 'plan_status_count_canceled')
BEGIN
	ALTER TABLE [campaign_summaries] ADD [plan_status_count_canceled] [INT] NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('campaign_summaries') AND name = 'plan_status_count_rejected')
BEGIN
	ALTER TABLE [campaign_summaries] ADD [plan_status_count_rejected] [INT] NULL
END
/*************************************** END PRI-17160 BE - Fix PlanStatuses and CampaignStatus *****************************************************/

/**************************************** START - PRI-7457 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_summaries') AND name = 'available_market_with_sov_count')
BEGIN
	ALTER TABLE [plan_summaries] ADD [available_market_with_sov_count] [INT] NULL	
END
/**************************************** END - PRI-7457 *****************************************************/

/*************************************** START - PRI-15876 ****************************************************/
IF NOT EXISTS (SELECT * FROM sys.objects o WHERE o.object_id = object_id(N'[dbo].[FK_dayparts_timespans)]') AND OBJECTPROPERTY(o.object_id, N'IsForeignKey') = 1)
BEGIN
	ALTER TABLE [dbo].[dayparts]  WITH CHECK ADD  CONSTRAINT [FK_dayparts_timespans] FOREIGN KEY([timespan_id])
	REFERENCES [dbo].[timespans] ([id])
END
/*************************************** END - PRI-15876 ****************************************************/



/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.11.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.10.1' -- Previous release version
		OR [version] = '19.11.1') -- Current release version
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