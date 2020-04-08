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

/*************************************** START PRI-25505 *****************************************************/

GO

IF EXISTS(SELECT * FROM inventory_file_proprietary_header
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE inventory_file_proprietary_header
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM inventory_summary_quarter_details
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE inventory_summary_quarter_details
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM plan_version_dayparts
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE plan_version_dayparts
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM scx_generation_job_files
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE scx_generation_job_files
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM scx_generation_jobs
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE scx_generation_jobs
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM station_inventory_manifest_dayparts
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE station_inventory_manifest_dayparts
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM nti_to_nsi_conversion_rates
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	DELETE FROM nti_to_nsi_conversion_rates
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT id FROM daypart_defaults  WHERE code = 'ROSN')
BEGIN
	DELETE FROM daypart_defaults
	WHERE id IN (SELECT id FROM daypart_defaults  WHERE code = 'ROSN')
END

GO

/*************************************** END PRI-25505 *******************************************************/

/*************************************** START PRI-25196 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'goal_fulfilled_by_proprietary' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_results'))
BEGIN
	ALTER TABLE [plan_version_pricing_results] ADD [goal_fulfilled_by_proprietary] [BIT] NULL
	
	EXEC('UPDATE [plan_version_pricing_results]
		  SET [goal_fulfilled_by_proprietary] = 0')
		  
    ALTER TABLE [plan_version_pricing_results]
	ALTER COLUMN [goal_fulfilled_by_proprietary] [BIT] NOT NULL
END
/*************************************** END PRI-25196 *****************************************************/

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