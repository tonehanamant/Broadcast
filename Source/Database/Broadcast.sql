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

/*************************************** START PRI-20790 *****************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id =OBJECT_ID('plan_version_creative_lengths'))
BEGIN
    CREATE TABLE [plan_version_creative_lengths](
        [id] INT IDENTITY(1,1) NOT NULL,
        [plan_version_id] INT NOT NULL,    
        [spot_length_id] INT NOT NULL,
		[weight] INT NULL
         CONSTRAINT [PK_plan_version_creative_lengths] PRIMARY KEY CLUSTERED
    (
        [id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
    ) ON [PRIMARY]
 
    ALTER TABLE [dbo].[plan_version_creative_lengths] WITH CHECK ADD CONSTRAINT [FK_plan_version_creative_lengths_plan_versions] FOREIGN KEY ([plan_version_id])
    REFERENCES [dbo].[plan_versions] (id)
     
    CREATE NONCLUSTERED INDEX [IX_plan_version_creative_lengths_plan_version_id] ON [dbo].[plan_version_creative_lengths] ([plan_version_id])
 
    ALTER TABLE [dbo].[plan_version_creative_lengths] WITH CHECK ADD CONSTRAINT [FK_plan_version_creative_lengths_spot_lengths] FOREIGN KEY ([spot_length_id])
    REFERENCES [dbo].[spot_lengths] (id)
     
    EXEC('INSERT INTO [plan_version_creative_lengths]([plan_version_id], [spot_length_id], [weight])
        SELECT id, spot_length_id, NULL
        FROM [plan_versions]')
 
    --remove FK from plan_versions
    IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_versions') AND name = 'FK_plan_versions_spot_lengths')
    BEGIN
        ALTER TABLE [plan_versions] DROP CONSTRAINT [FK_plan_versions_spot_lengths]
    END
    --remove spot_length_id from plan_versions
    IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'spot_length_id' and object_id = OBJECT_ID('plan_versions'))
    BEGIN
        ALTER TABLE [plan_versions] DROP COLUMN [spot_length_id]
    END
END
IF EXISTS(SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_creative_lengths') AND name = 'FK_plan_version_creative_lengths_plan_versions')
BEGIN
	ALTER TABLE [plan_version_creative_lengths] DROP CONSTRAINT [FK_plan_version_creative_lengths_plan_versions]
	ALTER TABLE [dbo].[plan_version_creative_lengths] WITH CHECK ADD CONSTRAINT [FK_plan_version_creative_lengths_plan_versions] FOREIGN KEY ([plan_version_id])
    REFERENCES [dbo].[plan_versions] (id) ON DELETE CASCADE
END
/*************************************** END PRI-20790 *****************************************************/

/*************************************** START PRI-25974 *********************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'budget_adjusted' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_parameters'))
BEGIN
	ALTER TABLE plan_version_pricing_parameters
	ADD budget_adjusted MONEY NULL

	EXEC('UPDATE plan_version_pricing_parameters
	SET budget_adjusted = budget_goal * (1 - (margin / 100.0))')

	ALTER TABLE plan_version_pricing_parameters
	ALTER COLUMN budget_adjusted MONEY NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'cpm_adjusted' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_parameters'))
BEGIN
	ALTER TABLE plan_version_pricing_parameters
	ADD cpm_adjusted MONEY NULL

	EXEC('UPDATE plan_version_pricing_parameters
	SET cpm_adjusted =  (budget_goal * (1 - (margin / 100.0)))/ impressions_goal')

	ALTER TABLE plan_version_pricing_parameters
	ALTER COLUMN cpm_adjusted MONEY NOT NULL
END

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id =OBJECT_ID('plan_version_pricing_inventory_source_percentages'))
BEGIN
	EXEC('DROP TABLE plan_version_pricing_inventory_source_percentages')
END

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id =OBJECT_ID('plan_version_pricing_inventory_source_type_percentages'))
BEGIN
	EXEC('DROP TABLE plan_version_pricing_inventory_source_type_percentages')
END

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id =OBJECT_ID('plan_version_pricing_execution_markets'))
BEGIN
	EXEC('DROP TABLE plan_version_pricing_execution_markets')
END

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id =OBJECT_ID('plan_version_pricing_executions'))
BEGIN
	EXEC('DROP TABLE plan_version_pricing_executions')
END
/*************************************** END PRI-25974 *********************************************************/


/*************************************** START BP1-4 *****************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id =OBJECT_ID('program_name_mappings'))
BEGIN
    CREATE TABLE [program_name_mappings]
	(
		[id] INT IDENTITY(1,1),
		[inventory_program_name] NVARCHAR(500),
		[official_program_name] NVARCHAR(500),
		[genre_id] INT NOT NULL,
		[show_type_id] INT NOT NULL,
		CONSTRAINT [PK_program_name_mappings] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	CREATE UNIQUE INDEX [UX_program_name_mappings_inventory_program_name] ON program_name_mappings
	(
		[inventory_program_name] ASC
	)
	INCLUDE([id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

	ALTER TABLE [dbo].[program_name_mappings] WITH CHECK ADD CONSTRAINT [FK_program_name_mappings_genres] FOREIGN KEY ([genre_id])
    REFERENCES [dbo].[genres] (id)

	ALTER TABLE [dbo].[program_name_mappings] WITH CHECK ADD CONSTRAINT [FK_program_name_mappings_show_types] FOREIGN KEY ([show_type_id])
    REFERENCES [dbo].[show_types] (id)
END
/*************************************** END BP1-4 *****************************************************/

/*************************************** START BP1-55 *****************************************************/
-- rename table plan_version_weeks to plan_version_weekly_breakdown
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('plan_version_weeks'))
BEGIN
	EXEC sp_rename 'dbo.plan_version_weeks', 'plan_version_weekly_breakdown';
END

-- add column spot_length_id
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'spot_length_id' AND OBJECT_ID = OBJECT_ID(N'plan_version_weekly_breakdown'))
BEGIN
	EXEC('
		ALTER TABLE [plan_version_weekly_breakdown] ADD [spot_length_id] int NULL

		ALTER TABLE [plan_version_weekly_breakdown] WITH CHECK ADD CONSTRAINT [FK_plan_version_weekly_breakdown_spot_lengths] FOREIGN KEY([spot_length_id])
		REFERENCES [spot_lengths] ([id])
		ALTER TABLE [plan_version_weekly_breakdown] CHECK CONSTRAINT [FK_plan_version_weekly_breakdown_spot_lengths]
	')
END

-- add column daypart_default_id
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'daypart_default_id' AND OBJECT_ID = OBJECT_ID(N'plan_version_weekly_breakdown'))
BEGIN
	EXEC('
		ALTER TABLE [plan_version_weekly_breakdown] ADD [daypart_default_id] int NULL

		ALTER TABLE [plan_version_weekly_breakdown] WITH CHECK ADD CONSTRAINT [FK_plan_version_weekly_breakdown_daypart_defaults] FOREIGN KEY([daypart_default_id])
		REFERENCES [daypart_defaults] ([id])
		ALTER TABLE [plan_version_weekly_breakdown] CHECK CONSTRAINT [FK_plan_version_weekly_breakdown_daypart_defaults]
	')
END

-- add column percentage_of_week
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'percentage_of_week' AND OBJECT_ID = OBJECT_ID(N'plan_version_weekly_breakdown'))
BEGIN
    EXEC('ALTER TABLE [plan_version_weekly_breakdown] ADD [percentage_of_week] float NULL')
END

-- rename column weekly_impressions to impressions
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'weekly_impressions' AND OBJECT_ID = OBJECT_ID(N'plan_version_weekly_breakdown'))
BEGIN
	EXEC sp_rename 'dbo.plan_version_weekly_breakdown.weekly_impressions', 'impressions', 'COLUMN';
END

-- rename column weekly_impressions_percentage to impressions_percentage
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'weekly_impressions_percentage' AND OBJECT_ID = OBJECT_ID(N'plan_version_weekly_breakdown'))
BEGIN
	EXEC sp_rename 'dbo.plan_version_weekly_breakdown.weekly_impressions_percentage', 'impressions_percentage', 'COLUMN';
END

-- rename column weekly_rating_points to rating_points
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'weekly_rating_points' AND OBJECT_ID = OBJECT_ID(N'plan_version_weekly_breakdown'))
BEGIN
	EXEC sp_rename 'dbo.plan_version_weekly_breakdown.weekly_rating_points', 'rating_points', 'COLUMN';
END

-- rename column weekly_budget to budget
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'weekly_budget' AND OBJECT_ID = OBJECT_ID(N'plan_version_weekly_breakdown'))
BEGIN
	EXEC sp_rename 'dbo.plan_version_weekly_breakdown.weekly_budget', 'budget', 'COLUMN';
END

-- rename weekly_adu to adu
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'weekly_adu' AND OBJECT_ID = OBJECT_ID(N'plan_version_weekly_breakdown'))
BEGIN
	EXEC sp_rename 'dbo.plan_version_weekly_breakdown.weekly_adu', 'adu', 'COLUMN';
END

-- rename primary key PK_plan_version_weeks to PK_plan_version_weekly_breakdown
IF EXISTS (
	SELECT 1
	FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
	WHERE CONSTRAINT_TYPE='PRIMARY KEY' AND TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'plan_version_weekly_breakdown' and CONSTRAINT_NAME = 'PK_plan_version_weeks')
BEGIN
	EXEC sp_rename 'dbo.plan_version_weekly_breakdown.PK_plan_version_weeks', 'PK_plan_version_weekly_breakdown'
END

-- rename foreign key FK_plan_version_weeks_plan_versions to FK_plan_version_weekly_breakdown_plan_versions
IF EXISTS (
  SELECT 1 
  FROM sys.foreign_keys 
  WHERE object_id = OBJECT_ID(N'dbo.FK_plan_version_weeks_plan_versions')
  AND parent_object_id = OBJECT_ID(N'dbo.plan_version_weekly_breakdown')
  )
BEGIN
	EXEC sp_rename 'dbo.FK_plan_version_weeks_plan_versions', 'FK_plan_version_weekly_breakdown_plan_versions';
END

-- rename foreign key FK_plan_weeks_media_weeks to FK_plan_version_weekly_breakdown_media_weeks
IF EXISTS (
  SELECT 1 
  FROM sys.foreign_keys 
  WHERE object_id = OBJECT_ID(N'dbo.FK_plan_weeks_media_weeks')
  AND parent_object_id = OBJECT_ID(N'dbo.plan_version_weekly_breakdown')
  )
BEGIN
	EXEC sp_rename 'dbo.FK_plan_weeks_media_weeks', 'FK_plan_version_weekly_breakdown_media_weeks';
END

-- rename index PK_plan_version_weeks to PK_plan_version_weekly_breakdown
IF EXISTS (
	SELECT 1 
	FROM sys.indexes 
	WHERE name='PK_plan_version_weeks' AND object_id = OBJECT_ID('dbo.plan_version_weekly_breakdown')
	)
BEGIN
	EXEC sp_rename 'dbo.plan_version_weekly_breakdown.PK_plan_version_weeks', 'PK_plan_version_weekly_breakdown' , 'INDEX';
END

-- rename index FK_plan_version_weeks_media_weeks to IX_plan_version_weekly_breakdown_media_week_id
IF EXISTS (
	SELECT 1 
	FROM sys.indexes 
	WHERE name='FK_plan_version_weeks_media_weeks' AND object_id = OBJECT_ID('dbo.plan_version_weekly_breakdown')
	)
BEGIN
	EXEC sp_rename 'dbo.plan_version_weekly_breakdown.FK_plan_version_weeks_media_weeks', 'IX_plan_version_weekly_breakdown_media_week_id' , 'INDEX';
END

-- rename index IX_plan_version_weeks_plan_version_id to IX_plan_version_weekly_breakdown_plan_version_id
IF EXISTS (
	SELECT 1 
	FROM sys.indexes 
	WHERE name='IX_plan_version_weeks_plan_version_id' AND object_id = OBJECT_ID('dbo.plan_version_weekly_breakdown')
	)
BEGIN
	EXEC sp_rename 'dbo.plan_version_weekly_breakdown.IX_plan_version_weeks_plan_version_id', 'IX_plan_version_weekly_breakdown_plan_version_id' , 'INDEX';
END

/*************************************** END BP1-55 *****************************************************/

/*************************************** START BP1-19 *****************************************************/
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id =OBJECT_ID('inventory_export_jobs'))
BEGIN
    CREATE TABLE [inventory_export_jobs](
        [id] INT IDENTITY(1,1) NOT NULL,
		[inventory_source_id] INT NOT NULL , -- FK TODO
		[quarter_year] INT NOT NULL,
		[quarter_number] INT NOT NULL,
		[export_genre_type_id] INT NOT NULL,
		[status] INT NOT NULL,
		[status_message] VARCHAR(MAX) NULL,
		[file_name] VARCHAR(200) NULL,
		[completed_at] DATETIME NULL,
		[created_at] DATETIME NOT NULL,
		[created_by] VARCHAR(63) NOT NULL
         CONSTRAINT [PK_inventory_export_jobs] PRIMARY KEY CLUSTERED
    (
        [id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
    ) ON [PRIMARY]
 
    ALTER TABLE [dbo].[inventory_export_jobs] WITH CHECK ADD CONSTRAINT [FK_inventory_export_jobs_inventory_source] FOREIGN KEY ([inventory_source_id])
    REFERENCES [dbo].[inventory_sources] (id)     
END
GO
/*************************************** END BP1-19 *****************************************************/

/*************************************** START BP-33 *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'market_group' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_parameters'))
BEGIN
	ALTER TABLE plan_version_pricing_parameters
	ADD market_group INT NULL

	EXEC('UPDATE plan_version_pricing_parameters
	      SET market_group = 5')

	ALTER TABLE plan_version_pricing_parameters
	ALTER COLUMN market_group INT NOT NULL
END

/*************************************** END BP-33 *****************************************************/

/*************************************** Start Cleanup - GenreSourceRename *****************************************************/

GO

UPDATE genre_sources SET
	[name] = 'RedBee'
WHERE id = 2
AND [name] <> 'RedBee'

GO

/*************************************** End Cleanup - GenreSourceRename *****************************************************/

/*************************************** START BP1-25 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'budget' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_result_spots'))
BEGIN
	ALTER TABLE plan_version_pricing_result_spots
	ADD budget MONEY NULL

	EXEC('UPDATE plan_version_pricing_result_spots
	SET budget = 0
	WHERE budget IS NULL')

	ALTER TABLE plan_version_pricing_result_spots
	ALTER COLUMN budget MONEY NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'spots' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_result_spots'))
BEGIN
	ALTER TABLE plan_version_pricing_result_spots
	ADD spots INT NULL

	EXEC('UPDATE plan_version_pricing_result_spots
	SET spots = 0
	WHERE spots IS NULL')

	ALTER TABLE plan_version_pricing_result_spots
	ALTER COLUMN spots INT NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'total_spots' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_results'))
BEGIN
	ALTER TABLE plan_version_pricing_results
	ADD total_spots INT NULL

	EXEC('UPDATE plan_version_pricing_results
	SET total_spots = 0
	WHERE total_spots IS NULL')

	ALTER TABLE plan_version_pricing_results
	ALTER COLUMN total_spots INT NOT NULL
END
/*************************************** END BP1-25 *****************************************************/

/*************************************** END BP1-299 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'impressions' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_result_spots'))
BEGIN
	ALTER TABLE plan_version_pricing_result_spots
	ADD impressions FLOAT NULL

	EXEC('UPDATE plan_version_pricing_result_spots
SET impressions = plan_version_pricing_result_spots.percentage_of_buy * plan_version_pricing_results.total_impressions
FROM plan_version_pricing_result_spots
INNER JOIN plan_version_pricing_results
ON plan_version_pricing_result_spots.plan_version_pricing_result_id = plan_version_pricing_results.id
WHERE impressions IS NULL ')

	ALTER TABLE plan_version_pricing_result_spots
	ALTER COLUMN impressions FLOAT NOT NULL
END
/*************************************** END BP1-299 *****************************************************/


/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.06.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.05.1' -- Previous release version
		OR [version] = '20.06.1') -- Current release version
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