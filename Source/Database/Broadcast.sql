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

/*************************************** START PRI-20829 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'hangfire_job_id' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_job'))
BEGIN
	EXEC('ALTER TABLE [plan_version_pricing_job] ADD [hangfire_job_id] VARCHAR(16) NULL')
END
/*************************************** END PRI-20829 *****************************************************/

/*************************************** START PRI-24475 *****************************************************/
IF EXISTS(SELECT 1 FROM plan_versions WHERE target_audience_id = 31 AND target_universe <> hh_universe)
BEGIN
	UPDATE t1
	SET t1.target_universe = t2.universe
	FROM plan_versions as t1
	INNER JOIN nti_universes as t2 ON t1.target_audience_id = t2.audience_id
END
/*************************************** END PRI-24475 *****************************************************/

/*************************************** START PRI-24475 *****************************************************/
IF EXISTS(SELECT 1 FROM plan_versions WHERE target_audience_id = 31 AND target_universe <> hh_universe)
BEGIN
	UPDATE t1
	SET t1.target_universe = t2.universe
	FROM plan_versions as t1
	INNER JOIN nti_universes as t2 ON t1.target_audience_id = t2.audience_id
END
/*************************************** END PRI-24475 *****************************************************/

/*************************************** START PRI-24475 *****************************************************/
IF EXISTS(SELECT 1 FROM plan_versions WHERE target_audience_id = 31 AND target_universe <> hh_universe)
BEGIN
	UPDATE t1
	SET t1.target_universe = t2.universe
	FROM plan_versions as t1
	INNER JOIN nti_universes as t2 ON t1.target_audience_id = t2.audience_id
END
/*************************************** END PRI-24475 *****************************************************/

/*************************************** START PRI-19661 *****************************************************/
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'diagnostic_result' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_job'))
BEGIN
	ALTER TABLE [plan_version_pricing_job] ALTER COLUMN [diagnostic_result] nvarchar(max) NULL
END
/*************************************** END PRI-19661 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.05.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.04.1' -- Previous release version
		OR [version] = '20.05.1') -- Current release version
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