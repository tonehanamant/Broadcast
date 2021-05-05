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
		WHEN @envName = 'UAT' THEN REPLACE(@urlBase, '[MachineName]', 'cadapps-uat1.crossmw.com')
		WHEN @envName = 'QA' THEN REPLACE(@urlBase, '[MachineName]', 'test.cadent.tv')
		ELSE REPLACE(@urlBase, '[MachineName]', 'cd.cadent.tv')
	END

SELECT @envTrafficUrl = CASE 
		WHEN @envName = 'PROD' AND @IsStg = 1 THEN REPLACE(@trafficUrlBase, '[MachineName]', 'cadapps-stg5')
		WHEN @envName = 'PROD' THEN REPLACE(@trafficUrlBase, '[MachineName]', 'cadapps-prod5')
		WHEN @envName = 'UAT' THEN REPLACE(@trafficUrlBase, '[MachineName]', 'cadapps-uat1.crossmw.com')
		WHEN @envName = 'QA' THEN REPLACE(@trafficUrlBase, '[MachineName]', 'test.cadent.tv')
		ELSE REPLACE(@trafficUrlBase, '[MachineName]', 'cd.cadent.tv')
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

/*************************************** API results posting type update *****************************************************/
UPDATE
    pvps
SET
    pvps.posting_type = COALESCE(pv.posting_type, 1)
FROM 
    [plan_version_pricing_api_results] pvps
        LEFT JOIN [plan_version_pricing_job] pvpj
        ON    pvpj.id = pvps.plan_version_pricing_job_id
        LEFT JOIN [plan_versions] pv
        ON pv.id = pvpj.plan_version_id
WHERE  pvps.posting_type = 0
GO
/*************************************** End API results posting type update *****************************************************/

/*************************************** Start BP-1892 *****************************************************/

IF OBJECT_ID('tempdb..#update_for_available_markets') IS NOT NULL
BEGIN
	DROP TABLE #update_for_available_markets
END

SELECT distinct plan_version_id
	INTO #update_for_available_markets
FROM plan_version_available_markets WHERE share_of_voice_percent IS NULL

UPDATE m SET 
	is_user_share_of_voice_percent = CASE WHEN share_of_voice_percent IS NULL THEN 0 ELSE 1 END
FROM plan_version_available_markets m 
JOIN #update_for_available_markets u
	ON u.plan_version_id = m.plan_version_id

UPDATE m SET 
	share_of_voice_percent = m.percentage_of_us
FROM plan_version_available_markets m
JOIN #update_for_available_markets u
	ON u.plan_version_id = m.plan_version_id
WHERE m.share_of_voice_percent IS NULL

GO

/*************************************** End BP-1892 *****************************************************/

/*************************************** start BP-2074 *****************************************************/

IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=1 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.8, 1, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.8 WHERE [media_month_id]=474 and [standard_daypart_id]=1
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=2 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 2, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=2
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=3 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 3, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=3
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=4 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 4, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=4
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=5 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 5, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=5
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=6 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 6, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=6
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=7 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 7, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=7
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=8 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 8, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=8
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=9 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 9, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=9
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=10 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 10, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=10
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=11 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 11, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=11
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=12 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 12, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=12
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=14 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 14, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=14
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=15 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.8, 15, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.8 WHERE [media_month_id]=474 and [standard_daypart_id]=15
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=16 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 16, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=16
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=17 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.75, 17, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.75 WHERE [media_month_id]=474 and [standard_daypart_id]=17
	  END

IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=19 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 19, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=19
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=20 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 20, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=20
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=21 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 21, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=21
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=22 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 22, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=22
	  END
IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=23 ) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 23, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=23
	  END
Go
	
/*************************************** End BP-2074 *****************************************************/

/*************************************** Start BP-2275 *****************************************************/

DECLARE @AddColumnSql VARCHAR(MAX) =
	'ALTER TABLE spot_length_cost_multipliers
		ADD inventory_cost_premium decimal(10,2) NULL'

DECLARE @PopulateSql VARCHAR(MAX) =
	'UPDATE m SET
		inventory_cost_premium = CASE WHEN s.length = 15 THEN 0.15 ELSE 0 END
	FROM spot_length_cost_multipliers m
	JOIN spot_lengths s
		ON m.spot_length_id = s.id'

DECLARE @AlterSql VARCHAR(MAX) = 
	'ALTER TABLE spot_length_cost_multipliers
		ALTER COLUMN inventory_cost_premium decimal(10,2) NOT NULL'

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'inventory_cost_premium'
          AND Object_ID = Object_ID(N'spot_length_cost_multipliers'))
BEGIN
	EXEC (@AddColumnSql)
	EXEC (@PopulateSql)
	EXEC (@AlterSql)
END

GO

/*************************************** End BP-2275 *****************************************************/

/*************************************** Start BP-2301 *****************************************************/

DECLARE @AddColumnSql VARCHAR(MAX) =
	'ALTER TABLE plans
		ADD spot_allocation_model_mode INT NULL'

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'spot_allocation_model_mode'
          AND Object_ID = Object_ID(N'plans'))
BEGIN
	EXEC (@AddColumnSql)	
END

GO

/*************************************** End BP-2301 *****************************************************/

/*************************************** Start BP-2245 *****************************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_buying_api_results' AND COLUMN_NAME= 'posting_type')
BEGIN
	ALTER TABLE plan_version_buying_api_results
	ADD posting_type INT NULL
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_buying_results' AND COLUMN_NAME= 'posting_type')
BEGIN
	ALTER TABLE plan_version_buying_results
	ADD posting_type INT NULL
END

GO

UPDATE  pvbar 
SET pvbar.posting_type = pv.posting_type
FROM plan_version_buying_api_results pvbar
	JOIN plan_version_buying_job pvbj ON 
		pvbar.plan_version_buying_job_id = pvbj.id
	JOIN plan_Versions pv ON 
		pvbj.plan_version_id = pv.id
where pvbar.posting_type IS NULL

Go

UPDATE  pvbr 
SET pvbr.posting_type = pv.posting_type
FROM plan_version_buying_results pvbr
	JOIN plan_version_buying_job pvbj ON 
		pvbr.plan_version_buying_job_id = pvbj.id
	JOIN plan_Versions pv ON 
		pvbj.plan_version_id = pv.id
where pvbr.posting_type IS NULL

Go

ALTER TABLE plan_version_buying_results
ALTER COLUMN posting_type int NOT NULL

GO

ALTER TABLE plan_version_buying_api_results
ALTER COLUMN posting_type int NOT NULL

GO

/*************************************** End BP-2245 *****************************************************/

/*************************************** Start BP-2257 *****************************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_pricing_results' AND COLUMN_NAME= 'calculated_vpvh')
BEGIN
	ALTER TABLE plan_version_pricing_results
	ADD calculated_vpvh  float NULL
END
GO
/*************************************** End BP-2257 *****************************************************/

/*************************************** Start BP-2381 *****************************************************/

UPDATE plan_version_pricing_parameters SET
	impressions_goal = FLOOR(impressions_goal)
WHERE impressions_goal - FLOOR(impressions_goal) > 0

GO
/*************************************** End BP-2381 *****************************************************/

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

