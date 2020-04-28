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
/*************************************** END PRI-20790 *****************************************************/

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