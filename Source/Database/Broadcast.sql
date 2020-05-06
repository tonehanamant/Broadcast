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