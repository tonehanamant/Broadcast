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


/*************************************** START PRI-13777 add vpvh in plans *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.COLUMNS 
				WHERE object_id = OBJECT_ID('plans')
					AND name = 'vpvh')
BEGIN
	ALTER TABLE plans ADD vpvh FLOAT NULL

	EXEC('UPDATE plans
	SET vpvh = 0.001
	WHERE vpvh IS NULL')

	ALTER TABLE plans
	ALTER COLUMN vpvh FLOAT NOT NULL
END
GO

/*************************************** END PRI-13777 add vpvh in plans  *****************************************************/

/*************************************** START PRI-12842 *****************************************************/

IF OBJECT_ID('campaign_summaries') IS NULL
BEGIN
	CREATE TABLE [campaign_summaries]
	(
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[campaign_id] [INT] NOT NULL,
		[processing_status] [INT] NOT NULL,
		[processing_status_error_msg] [NVARCHAR](2000),
		[queued_at] [DATETIME] NOT NULL,
		[queued_by] [VARCHAR](50) NOT NULL,
		[flight_start_Date] [DATETIME],
		[flight_end_Date] [DATETIME],
		[flight_hiatus_days] [INT],
		[flight_active_days] [INT],
		[budget] [FLOAT],
		[cpm] [FLOAT],
		[impressions] [FLOAT],
		[rating] [FLOAT],
		[plan_status_count_working] [INT],
		[plan_status_count_reserved] [INT],
		[plan_status_count_client_approval] [INT],
		[plan_status_count_contracted] [INT],
		[plan_status_count_live] [INT],
		[plan_status_count_complete] [INT],
		[campaign_status] [INT],
		[components_modified] [DATETIME],
		[last_aggregated] [DATETIME]
		CONSTRAINT [PK_campaign_summaries] PRIMARY KEY CLUSTERED
		(
			[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[campaign_summaries] WITH CHECK ADD CONSTRAINT [FK_campaign_summaries_campaign] FOREIGN KEY ([campaign_id])
		REFERENCES [dbo].[campaigns] (id)
		ON DELETE CASCADE

	CREATE NONCLUSTERED INDEX [IX_campaign_summaries_campaign_id] ON [dbo].[campaign_summaries] ([campaign_id] ASC)
		INCLUDE ([id])
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

/*************************************** END PRI-12842  *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.10.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.09.1' -- Previous release version
		OR [version] = '19.10.1') -- Current release version
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