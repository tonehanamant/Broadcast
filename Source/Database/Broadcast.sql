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