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

/*************************************** Start BP-2005 *****************************************************/

DECLARE @populationSql VARCHAR(MAX) = '
UPDATE plan_version_available_markets SET
	is_user_share_of_voice_percent = CASE WHEN share_of_voice_percent IS NULL THEN 0 ELSE 1 END
WHERE is_user_share_of_voice_percent IS NULL
'

DECLARE @alterSql VARCHAR(MAX) = '
ALTER TABLE plan_version_available_markets 
	ALTER COLUMN is_user_share_of_voice_percent BIT NOT NULL
'

IF NOT EXISTS (SELECT 1 FROM sys.columns 
          WHERE Name = N'is_user_share_of_voice_percent'
          AND Object_ID = Object_ID(N'plan_version_available_markets'))
BEGIN	
	ALTER TABLE plan_version_available_markets
		ADD is_user_share_of_voice_percent BIT NULL
END

EXEC (@populationSql)
EXEC (@alterSql)

GO
/*************************************** End BP-2005 *****************************************************/

/*************************************** Start BP-1551 - settings *****************************************************/

DECLARE @envName VARCHAR(50)
DECLARE @trafficUrlBase VARCHAR(500) = 'http://[MachineName]/traffic/api/company'
DECLARE @urlBase VARCHAR(500) = 'https://[MachineName]/aabapi/api/v1'
DECLARE @envTrafficUrl VARCHAR(500)
DECLARE @envUrl VARCHAR(500)
DECLARE @IsStg BIT

SELECT @IsStg = CASE WHEN @@SERVERNAME = 'CADSQL-STG' THEN 1 ELSE 0 END 

SELECT @envName = parameter_value
FROM system_settings.dbo.system_component_parameters
WHERE component_id = 'MaestroEnvironment'
AND parameter_key = 'Environment'

SELECT @envUrl = CASE 
		WHEN @envName = 'PROD' AND @IsStg = 1 THEN REPLACE(@urlBase, '[MachineName]', 'stg.cadent.tv')
		WHEN @envName = 'PROD' THEN REPLACE(@urlBase, '[MachineName]', 'platform.cadent.tv')
		WHEN @envName = 'UAT' THEN REPLACE(@urlBase, '[MachineName]', 'cadapps-uat1.dev.crossmw.com')
		WHEN @envName = 'QA' THEN REPLACE(@urlBase, '[MachineName]', 'test.cadent.tv')
		ELSE REPLACE(@urlBase, '[MachineName]', 'cd.cadent.tv')
	END

SELECT @envTrafficUrl = CASE 
		WHEN @envName = 'PROD' AND @IsStg = 1 THEN REPLACE(@trafficUrlBase, '[MachineName]', 'cadapps-stg5')
		WHEN @envName = 'PROD' THEN REPLACE(@trafficUrlBase, '[MachineName]', 'cadapps-prod5')
		WHEN @envName = 'UAT' THEN REPLACE(@trafficUrlBase, '[MachineName]', 'cadapps-uat1.crossmw.com')
		WHEN @envName = 'QA' THEN REPLACE(@trafficUrlBase, '[MachineName]', 'devvmqa1.dev.crossmw.com')
		ELSE REPLACE(@trafficUrlBase, '[MachineName]', 'devvmqa2.dev.crossmw.com')
	END	

IF NOT EXISTS (SELECT 1 FROM system_settings.dbo.system_component_parameters
	WHERE component_id = 'BroadcastService'
	AND parameter_key = 'AgencyAdvertiserBrandTrafficApiUrl')
BEGIN
	INSERT INTO system_settings.dbo.system_component_parameters  (component_id, parameter_key, parameter_value, parameter_type, [description], last_modified_time)
		VALUES('BroadcastService', 'AgencyAdvertiserBrandTrafficApiUrl', @envTrafficUrl, 'string', 'The endpoint for the AAB traffic api.', SYSDATETIME())
END

UPDATE s SET
	parameter_value = @envTrafficUrl,
	last_modified_time = SYSDATETIME()
FROM system_settings.dbo.system_component_parameters s
WHERE component_id = 'BroadcastService'
AND parameter_key = 'AgencyAdvertiserBrandTrafficApiUrl'
AND parameter_value <> @envTrafficUrl

UPDATE s SET
	parameter_value = @envUrl,
	last_modified_time = SYSDATETIME()
FROM system_settings.dbo.system_component_parameters s
WHERE component_id = 'BroadcastService'
AND parameter_key = 'AgencyAdvertiserBrandApiUrl'
AND parameter_value <> @envUrl

GO
/*************************************** End BP-1551 - settings *****************************************************/

/*************************************** Start BP-1551 - schema *****************************************************/

-- These columns will exist.
ALTER TABLE campaigns
	ALTER COLUMN agency_id INT NULL

ALTER TABLE campaigns
	ALTER COLUMN advertiser_id INT NULL

ALTER TABLE plans
	ALTER COLUMN product_id INT NULL

-- these columns may not exist
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'agency_master_id'
          AND Object_ID = Object_ID(N'campaigns'))
BEGIN
    ALTER TABLE campaigns
		ADD agency_master_id UNIQUEIDENTIFIER NULL	
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'advertiser_master_id'
          AND Object_ID = Object_ID(N'campaigns'))
BEGIN
    ALTER TABLE campaigns
		ADD advertiser_master_id UNIQUEIDENTIFIER NULL	
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'product_master_id'
          AND Object_ID = Object_ID(N'plans'))
BEGIN
    ALTER TABLE plans
		ADD product_master_id UNIQUEIDENTIFIER NULL	
END

GO
/*************************************** End BP-1551 - schema *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '21.02.2' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.11.1' -- Previous release version
		OR [version] = '21.02.1' -- version skipped, but deployed to pre-prod
		OR [version] = '21.02.2') -- Current release version
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