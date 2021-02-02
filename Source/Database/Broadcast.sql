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

/*************************************** Start BP-1925 *****************************************************/

-- We will do system_settings here since we no longer release with cable.

DECLARE @ComponentId VARCHAR(20) = 'BroadcastService'
DECLARE @Environment VARCHAR(20)

DECLARE @SettingsToModify TABLE
(
	component_id VARCHAR(50),
	parameter_key VARCHAR(50),
	parameter_value VARCHAR(500),
	parameter_type VARCHAR(50),
	[description] VARCHAR(500),
	already_exists BIT DEFAULT 0
)

SELECT @Environment = parameter_value 
	FROM system_settings.dbo.system_component_parameters 
	WHERE component_id = 'MaestroEnvironment'
	AND parameter_key = 'Environment'

INSERT INTO @SettingsToModify (component_id, parameter_key , parameter_value, parameter_type, [description])
	VALUES 
		(@ComponentId, 'EfficiencyModelCpmGoal', '1', 'int', 'The dollar amount to use for the Cpm Goal for the Efficiency call.')
		, (@ComponentId, 'FloorModelCpmGoal', '1', 'int', 'The dollar amount to use for the Cpm Goal for the Floor call.')
		, (@ComponentId, 'PlanPricingAllocationsEfficiencyModelUrl', 
			CASE @Environment
				WHEN 'PROD' THEN 'https://datascience-prod.cadent.tv/broadcast-openmarket-allocations/v4/allocation'
				WHEN 'UAT' THEN 'https://datascience-uat.cadent.tv/broadcast-openmarket-allocations/v4/allocation'
				ELSE 'https://datascience-dev.cadent.tv/broadcast-openmarket-allocations/v4/allocation'
			END 
		, 'string', 'The url for the Efficiency Projection.')

UPDATE s SET
	already_exists = 1
FROM @SettingsToModify s
JOIN system_settings.dbo.system_component_parameters p
	ON s.component_id = p.component_id
	AND s.parameter_key = p.parameter_key	

INSERT INTO system_settings.dbo.system_component_parameters (component_id, parameter_key , parameter_value, parameter_type, [description], last_modified_time)
	SELECT component_id, parameter_key , parameter_value, parameter_type, [description], SYSDATETIME()
	FROM @SettingsToModify
	WHERE already_exists = 0

UPDATE p SET
	parameter_value = s.parameter_value,
	last_modified_time = SYSDATETIME()
FROM system_settings.dbo.system_component_parameters p
JOIN @SettingsToModify s
	ON s.component_id = p.component_id
	AND s.parameter_key = p.parameter_key
WHERE s.already_exists = 1
AND s.parameter_value <> p.parameter_value

DELETE FROM system_settings.dbo.system_component_parameters 
WHERE component_id = @ComponentId
AND parameter_key = 'PlanPricingEndpointVersion'

DELETE FROM system_settings.dbo.system_component_parameters 
WHERE component_id = @ComponentId
AND parameter_key = 'PlanPricingAllocationsUrlV3'

DELETE FROM system_settings.dbo.system_component_parameters 
WHERE component_id = @ComponentId
AND parameter_key = 'PlanBuyingEndpointVersion'

GO

/*************************************** End BP-1925 *****************************************************/

/*************************************** Start BP-1894 *****************************************************/

DECLARE @SpotAllocationCreateSqlTemplate VARCHAR(MAX) = '
-- Create the columns if they do not exist
IF NOT EXISTS (SELECT 1 FROM sys.columns 
          WHERE Name = N''spot_allocation_model_mode''
          AND Object_ID = Object_ID(N''[TABLE]''))
BEGIN	
	ALTER TABLE [TABLE]
		ADD spot_allocation_model_mode INT NULL
END'

DECLARE @SpotAllocationPopulationSqlTemplate VARCHAR(MAX) =
'UPDATE [TABLE] SET spot_allocation_model_mode = 1 WHERE spot_allocation_model_mode IS NULL'

DECLARE @SpotAllocationMakeNotNullSqlTemplate VARCHAR(MAX) = '
ALTER TABLE [TABLE]
	ALTER COLUMN spot_allocation_model_mode INT NOT NULL
'

DECLARE @SpotAllocationCreateSql VARCHAR(MAX)
DECLARE @SpotAllocationPopulationSql VARCHAR(MAX)
DECLARE @SpotAllocationMakeNotNullSql VARCHAR(MAX)

-- Pricing
SET @SpotAllocationCreateSql = REPLACE(@SpotAllocationCreateSqlTemplate, '[TABLE]', 'plan_version_pricing_api_results')
SET @SpotAllocationPopulationSql = REPLACE(@SpotAllocationPopulationSqlTemplate, '[TABLE]', 'plan_version_pricing_api_results')
SET @SpotAllocationMakeNotNullSql = REPLACE(@SpotAllocationMakeNotNullSqlTemplate, '[TABLE]', 'plan_version_pricing_api_results')
EXEC(@SpotAllocationCreateSql)
EXEC(@SpotAllocationPopulationSql)
EXEC(@SpotAllocationMakeNotNullSql)

SET @SpotAllocationCreateSql = REPLACE(@SpotAllocationCreateSqlTemplate, '[TABLE]', 'plan_version_pricing_bands')
SET @SpotAllocationPopulationSql = REPLACE(@SpotAllocationPopulationSqlTemplate, '[TABLE]', 'plan_version_pricing_bands')
SET @SpotAllocationMakeNotNullSql = REPLACE(@SpotAllocationMakeNotNullSqlTemplate, '[TABLE]', 'plan_version_pricing_bands')
EXEC(@SpotAllocationCreateSql)
EXEC(@SpotAllocationPopulationSql)
EXEC(@SpotAllocationMakeNotNullSql)

SET @SpotAllocationCreateSql = REPLACE(@SpotAllocationCreateSqlTemplate, '[TABLE]', 'plan_version_pricing_markets')
SET @SpotAllocationPopulationSql = REPLACE(@SpotAllocationPopulationSqlTemplate, '[TABLE]', 'plan_version_pricing_markets')
SET @SpotAllocationMakeNotNullSql = REPLACE(@SpotAllocationMakeNotNullSqlTemplate, '[TABLE]', 'plan_version_pricing_markets')
EXEC(@SpotAllocationCreateSql)
EXEC(@SpotAllocationPopulationSql)
EXEC(@SpotAllocationMakeNotNullSql)

SET @SpotAllocationCreateSql = REPLACE(@SpotAllocationCreateSqlTemplate, '[TABLE]', 'plan_version_pricing_stations')
SET @SpotAllocationPopulationSql = REPLACE(@SpotAllocationPopulationSqlTemplate, '[TABLE]', 'plan_version_pricing_stations')
SET @SpotAllocationMakeNotNullSql = REPLACE(@SpotAllocationMakeNotNullSqlTemplate, '[TABLE]', 'plan_version_pricing_stations')
EXEC(@SpotAllocationCreateSql)
EXEC(@SpotAllocationPopulationSql)
EXEC(@SpotAllocationMakeNotNullSql)

SET @SpotAllocationCreateSql = REPLACE(@SpotAllocationCreateSqlTemplate, '[TABLE]', 'plan_version_pricing_results')
SET @SpotAllocationPopulationSql = REPLACE(@SpotAllocationPopulationSqlTemplate, '[TABLE]', 'plan_version_pricing_results')
SET @SpotAllocationMakeNotNullSql = REPLACE(@SpotAllocationMakeNotNullSqlTemplate, '[TABLE]', 'plan_version_pricing_results')
EXEC(@SpotAllocationCreateSql)
EXEC(@SpotAllocationPopulationSql)
EXEC(@SpotAllocationMakeNotNullSql)

-- Buying
SET @SpotAllocationCreateSql = REPLACE(@SpotAllocationCreateSqlTemplate, '[TABLE]', 'plan_version_buying_api_results')
SET @SpotAllocationPopulationSql = REPLACE(@SpotAllocationPopulationSqlTemplate, '[TABLE]', 'plan_version_buying_api_results')
SET @SpotAllocationMakeNotNullSql = REPLACE(@SpotAllocationMakeNotNullSqlTemplate, '[TABLE]', 'plan_version_buying_api_results')
EXEC(@SpotAllocationCreateSql)
EXEC(@SpotAllocationPopulationSql)
EXEC(@SpotAllocationMakeNotNullSql)

SET @SpotAllocationCreateSql = REPLACE(@SpotAllocationCreateSqlTemplate, '[TABLE]', 'plan_version_buying_results')
SET @SpotAllocationPopulationSql = REPLACE(@SpotAllocationPopulationSqlTemplate, '[TABLE]', 'plan_version_buying_results')
SET @SpotAllocationMakeNotNullSql = REPLACE(@SpotAllocationMakeNotNullSqlTemplate, '[TABLE]', 'plan_version_buying_results')
EXEC(@SpotAllocationCreateSql)
EXEC(@SpotAllocationPopulationSql)
EXEC(@SpotAllocationMakeNotNullSql)

GO
/*************************************** End BP-1894 *****************************************************/

/*************************************** START - BP-1974 **************************************************/
--Allocations
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_results') 
				AND name = 'posting_type')
BEGIN

	ALTER TABLE [plan_version_pricing_api_results] ADD [posting_type] INT NULL

	EXEC ('
		UPDATE
			pvps
		SET
			pvps.posting_type = COALESCE(pv.posting_type, 1)
		FROM 
			[plan_version_pricing_api_results] pvps
			  INNER JOIN [plan_version_pricing_job] pvpj
				ON	pvpj.id = pvps.plan_version_pricing_job_id
			  LEFT JOIN [plan_versions] pv
				ON pv.id = pvpj.plan_version_id')

	ALTER TABLE [plan_version_pricing_api_results] ALTER COLUMN [posting_type] INT NOT NULL
END

GO

--Parameters
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_parameters') 
				AND name = 'posting_type')
BEGIN

	ALTER TABLE [plan_version_pricing_parameters] ADD [posting_type] INT NULL

	EXEC ('
		UPDATE
			pvps
		SET
			pvps.posting_type = COALESCE(pv.posting_type, 1)
		FROM 
			[plan_version_pricing_parameters] pvps
			  LEFT JOIN [plan_version_pricing_job] pvpj
				ON	pvpj.id = pvps.plan_version_pricing_job_id
			  LEFT JOIN [plan_versions] pv
				ON pv.id = pvpj.plan_version_id')

	ALTER TABLE [plan_version_pricing_parameters] ALTER COLUMN [posting_type] INT NOT NULL
END

GO
/*************************************** END - BP-1974 **************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
	SET parameter_value = '20.11.1' -- Current release version
	, last_modified_time = SYSDATETIME()
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.10.1' -- Previous release version
		OR [version] = '20.11.1') -- Current release version
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