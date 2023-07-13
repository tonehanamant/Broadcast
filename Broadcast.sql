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

/*

SDE 8/10/2021 
Domain migrations are changing these urls.
The Aab feature has been released so we're commenting this out.

DECLARE @envName VARCHAR(50)
DECLARE @trafficUrlBase VARCHAR(500) = 'http://[MachineName]/traffic/api/company'
DECLARE @urlBase VARCHAR(500) = 'http://[MachineName]/aabapiinternal/api/v1'
DECLARE @envTrafficUrl VARCHAR(500)
DECLARE @envUrl VARCHAR(500)
DECLARE @IsStg BIT

SELECT @IsStg = CASE WHEN @@SERVERNAME = 'CADSQL-STG' THEN 1 ELSE 0 END 

SELECT @envName = parameter_value
FROM system_settings.dbo.system_component_parameters
WHERE component_id = 'MaestroEnvironment'
AND parameter_key = 'Environment'


SELECT @envUrl = CASE 
		WHEN @envName = 'PROD' AND @IsStg = 1 THEN REPLACE(@urlBase, '[MachineName]', 'cadapps-stg6.crossmw.com')
		WHEN @envName = 'PROD' THEN REPLACE(@urlBase, '[MachineName]', 'cadapps-prod6.crossmw.com')
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

*/

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

/*************************************** Start BP-2410 *****************************************************/

UPDATE plans SET
	spot_allocation_model_mode = 1
WHERE spot_allocation_model_mode IS NULL

GO
/*************************************** End BP-2410 *****************************************************/

/*************************************** Start BP-2379 *****************************************************/

UPDATE plans SET
	spot_allocation_model_mode = 1
WHERE spot_allocation_model_mode IS NULL

GO
/*************************************** End BP-2410 *****************************************************/
UPDATE spot_length_cost_multipliers
	SET cost_multiplier = 0.5
	WHERE spot_length_id = 3

/*************************************** End BP-2410 *****************************************************/

/*************************************** Start BP-2514 *****************************************************/

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_pricing_results' AND COLUMN_NAME= 'calculated_vpvh')
BEGIN
	ALTER TABLE plan_version_pricing_results
	DROP COLUMN calculated_vpvh
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'plan_version_pricing_results_dayparts')
BEGIN
	CREATE TABLE [dbo].plan_version_pricing_results_dayparts
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY, 
		[plan_version_pricing_result_id] INT NOT NULL, 
		[standard_daypart_id] INT NOT NULL, 
		[calculated_vpvh] FLOAT NOT NULL,
		CONSTRAINT [FK_plan_version_pricing_results_dayparts_plan_version_pricing_results] FOREIGN KEY ([plan_version_pricing_result_id]) REFERENCES [dbo].[plan_version_pricing_results],
		CONSTRAINT [FK_plan_version_pricing_results_dayparts_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts]
	)
END

GO

/*************************************** END BP-2514 *****************************************************/

/*************************************** START BP-2642 *****************************************************/

DECLARE @AddColumnSql VARCHAR(MAX) = 
'ALTER TABLE plan_version_buying_parameters
	ADD posting_type INT NULL'

DECLARE @PopulateSql VARCHAR(MAX) =
'UPDATE bp SET
	posting_type = v.posting_type
FROM plan_versions v
JOIN plan_version_buying_parameters bp
	ON bp.plan_version_id = v.id
WHERE bp.posting_type IS NULL'

DECLARE @AlterSql VARCHAR(MAX) = 
'ALTER TABLE plan_version_buying_parameters
ALTER COLUMN posting_type INT NOT NULL'

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_buying_parameters' AND COLUMN_NAME= 'posting_type')
BEGIN
	EXEC (@AddColumnSql)
	EXEC (@PopulateSql)
	EXEC (@AlterSql)
END

GO

/*************************************** END BP-2642 *****************************************************/

/*************************************** START BP-2645 *****************************************************/
IF OBJECT_ID('real_isci_ingest_jobs') IS NOT NULL
BEGIN
    DROP TABLE real_isci_ingest_jobs
END
IF OBJECT_ID('reel_isci_ingest_jobs') IS NULL
BEGIN
	CREATE TABLE [dbo].[reel_isci_ingest_jobs]
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
		[status] INT NOT NULL,
		[queued_at] DATETIME2 NOT NULL,
		[queued_by] VARCHAR(100) NOT NULL,
		[completed_at] DATETIME2 NULL,
		[error_message] NVARCHAR(MAX) NULL
	)
END

GO
/*************************************** END BP-2645 *****************************************************/

/*************************************** START BP-2645 *****************************************************/

IF OBJECT_ID('reel_iscis') IS NULL
BEGIN
	CREATE TABLE [dbo].[reel_iscis]
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
		[isci] VARCHAR(50) NOT NULL,
		[spot_length_id] INT NOT NULL,
		[active_start_date] DATETIME2 NOT NULL,
		[active_end_date] DATETIME2 NOT NULL,
		[ingested_at] DATETIME2 NOT NULL,
		CONSTRAINT [FK_reel_iscis_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths]([ID])
	)	
END

IF OBJECT_ID('reel_isci_advertiser_name_references') IS NULL
BEGIN
	CREATE TABLE [dbo].[reel_isci_advertiser_name_references]
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
		[reel_isci_id] INT NOT NULL,
		[advertiser_name_reference] NVARCHAR(100) NOT NULL,
		CONSTRAINT [FK_reel_isci_advertiser_name_references_reel_iscis] FOREIGN KEY ([reel_isci_id]) REFERENCES [dbo].[reel_iscis]([ID]) ON DELETE CASCADE
	) 
END

IF OBJECT_ID('reel_isci_products') IS NULL
BEGIN
	CREATE TABLE [dbo].[reel_isci_products]
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
		[isci] VARCHAR(50) NOT NULL,
		[product_name] VARCHAR(50) NOT NULL,
		[created_at] DATETIME2 NOT NULL,
		[created_by] VARCHAR(100) NOT NULL
	)
END

IF OBJECT_ID('plan_iscis') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_iscis]
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
		[plan_id] INT NOT NULL, 
		[isci] VARCHAR(50) NOT NULL, 
		[created_at] DATETIME2 NOT NULL,
		[created_by] VARCHAR(100) NOT NULL,
		[deleted_at] DATETIME2 NULL,
		[deleted_by] VARCHAR(100) NULL,
		CONSTRAINT [FK_plan_iscis_plans] FOREIGN KEY ([plan_id]) REFERENCES [dbo].[plans] ([id])
	)
END

GO
/*************************************** END BP-2645 *****************************************************/

/*************************************** START BP-2651 *****************************************************/

IF OBJECT_ID('export_unmapped_program_names_jobs') IS NULL
BEGIN
	CREATE TABLE [dbo].[export_unmapped_program_names_jobs]
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
		[status] INT NOT NULL,
		[queued_at] DATETIME2 NOT NULL,
		[queued_by] VARCHAR(100) NOT NULL,
		[completed_at] DATETIME2 NULL,
		[error_message] NVARCHAR(MAX) NULL
	)
END
GO
/*************************************** END BP-2651 *****************************************************/

/*************************************** START BP-2741 *****************************************************/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'flight_notes_internal')
BEGIN	
	ALTER TABLE plan_versions
		ADD [flight_notes_internal] NVARCHAR (1024) NULL
END
GO

/*************************************** END BP-2741 *****************************************************/


/*************************************** START BP-2783 *****************************************************/

DECLARE @AudienceId_A21_54 INT,
		@AudienceId_W21_54 INT,
		@AudienceId_M21_54 INT

SELECT @AudienceId_A21_54 = id FROM audiences WHERE name = 'Adults 21-54'
SELECT @AudienceId_W21_54 = id FROM audiences WHERE name = 'Women 21-54'
SELECT @AudienceId_M21_54 = id FROM audiences WHERE name = 'Men 21-54'

DECLARE @new_mappings TABLE
(
	b_audience_id INT,
	nti_audience_code VARCHAR(30),
	nti_universe_audience_mappings_id INT
)

INSERT INTO @new_mappings (b_audience_id, nti_audience_code) VALUES
	/* Adults */
	(@AudienceId_A21_54, 'F21-24')
	,(@AudienceId_A21_54, 'F25-29')
	,(@AudienceId_A21_54, 'F30-34')
	,(@AudienceId_A21_54, 'F35-39')
	,(@AudienceId_A21_54, 'F40-44')
	,(@AudienceId_A21_54, 'F45-49')
	,(@AudienceId_A21_54, 'F50-54')
	,(@AudienceId_A21_54, 'M21-24')
	,(@AudienceId_A21_54, 'M25-29')
	,(@AudienceId_A21_54, 'M30-34')
	,(@AudienceId_A21_54, 'M35-39')
	,(@AudienceId_A21_54, 'M40-44')
	,(@AudienceId_A21_54, 'M45-49')
	,(@AudienceId_A21_54, 'M50-54')
	/* Men */
	,(@AudienceId_M21_54, 'M21-24')
	,(@AudienceId_M21_54, 'M25-29')
	,(@AudienceId_M21_54, 'M30-34')
	,(@AudienceId_M21_54, 'M35-39')
	,(@AudienceId_M21_54, 'M40-44')
	,(@AudienceId_M21_54, 'M45-49')
	,(@AudienceId_M21_54, 'M50-54')
	/* Women */
	,(@AudienceId_W21_54, 'F21-24')
	,(@AudienceId_W21_54, 'F25-29')
	,(@AudienceId_W21_54, 'F30-34')
	,(@AudienceId_W21_54, 'F35-39')
	,(@AudienceId_W21_54, 'F40-44')
	,(@AudienceId_W21_54, 'F45-49')
	,(@AudienceId_W21_54, 'F50-54')

UPDATE n SET
	nti_universe_audience_mappings_id = id
FROM @new_mappings n
JOIN nti_universe_audience_mappings m
	ON n.b_audience_id = m.audience_id
	AND n.nti_audience_code = m.nti_audience_code

INSERT INTO nti_universe_audience_mappings (audience_id, nti_audience_code)
	SELECT b_audience_id, nti_audience_code
	FROM @new_mappings
	WHERE nti_universe_audience_mappings_id IS NULL

GO

/*************************************** END BP-2783 *****************************************************/


/*************************************** START BP-2889 *****************************************************/
IF OBJECT_ID('plan_version_buying_result_spot_stations') IS NULL
BEGIN
CREATE TABLE [dbo].[plan_version_buying_result_spot_stations]
(
		[id] INT NOT NULL primary key IDENTITY, 
		[plan_version_buying_result_id] int not null,
		[program_name] varchar(255) NOT NULL,
		[genre] varchar(500) NOT NULL,
		[station] varchar(15) NOT NULL,
		[impressions] FLOAT NOT NULL,
		[spots] INT NOT NULL, 
		[budget] decimal NOT NULL,
		CONSTRAINT [FK_plan_version_buying_result_spot_stations_plan_version_buying_results] FOREIGN KEY ([plan_version_buying_result_id]) REFERENCES [dbo].[plan_version_buying_results]
		
)
END
GO
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_buying_result_spot_stations' AND COLUMN_NAME = 'budget' AND NUMERIC_PRECISION = 19 AND NUMERIC_SCALE = 4)
BEGIN
ALTER TABLE plan_version_buying_result_spot_stations
ALTER COLUMN budget decimal(19,4) not null;
END
GO
/*************************************** END BP-2889 *****************************************************/

/*************************************** START BP-2937 *****************************************************/

update plan_version_pricing_parameters_entity
    set plan_version_id = latest_version_id
from (
    select
        ROW_NUMBER() OVER(PARTITION BY pv.plan_id ORDER BY pvpp.id desc) RowNumber
        ,pv.plan_id
        ,p.latest_version_id
        ,pvpp.*
    from plan_version_pricing_parameters pvpp
            inner join plan_versions pv on pvpp.plan_version_id = pv.id
            inner join plans p on pv.plan_id = p.id
    where p.id in (
					select PP.Id
					from plans pp
						join plan_versions v
						on pp.latest_version_id = v.id 
						left outer join plan_version_pricing_parameters p
						on v.id =p.plan_version_id
					where p.id is null 
						and v.is_draft = 0 
					) 
    ) plan_version_pricing_parameters_entity
where RowNumber = 1
GO
/*************************************** End BP-2937 *****************************************************/

/*************************************** Start BP-2920 *****************************************************/

/*** Update the standard dayparts ***/
DECLARE @newDaypartIds TABLE
(
	daypart_code VARCHAR(5),
	daypart_id INT
)

-- there should be only one each, but just in case grab the earlier one as it existed at the time of this implementation.
INSERT INTO @newDaypartIds (daypart_code, daypart_id)
	SELECT TOP 1 'EM', id FROM dayparts WHERE daypart_text = 'M-SU 4AM-10AM' ORDER BY ID ASC

INSERT INTO @newDaypartIds (daypart_code, daypart_id)
	SELECT TOP 1 'PMN', id FROM dayparts WHERE daypart_text = 'M-SU 4PM-12:05AM' ORDER BY ID ASC

INSERT INTO @newDaypartIds (daypart_code, daypart_id)
	SELECT TOP 1 'EN', id FROM dayparts WHERE daypart_text = 'M-SU 4PM-8PM' ORDER BY ID ASC 

UPDATE sd SET 
	daypart_id = nd.daypart_id
FROM standard_dayparts sd
JOIN @newDaypartIds nd
	ON nd.daypart_code = sd.code

/*** Update Existing Plans ***/

DECLARE @flat_standard_dayparts TABLE
(
	standard_daypart_id INT,
	daypart_id INT,
	code VARCHAR(15),
	[name] VARCHAR(63),
	daypart_text VARCHAR(63),
	start_time INT,
	end_time INT
)

INSERT INTO @flat_standard_dayparts (standard_daypart_id, daypart_id, code, [name], daypart_text, start_time, end_time)
	SELECT sd.id
		, sd.daypart_id 
		, sd.code
		, sd.[name]
		, d.daypart_text
		, t.start_time
		, t.end_time
	FROM standard_dayparts sd
	JOIN dayparts d
		ON d.id = sd.daypart_id
	JOIN timespans t
		ON t.id = d.timespan_id 

--if start time is different set is_start_time_modified = 1
UPDATE pd SET
	is_start_time_modified = 1
FROM plan_version_dayparts pd
JOIN @flat_standard_dayparts ssdd
	ON ssdd.standard_daypart_id = pd.standard_daypart_id
WHERE ssdd.code in ('EM', 'PMN', 'EN')
AND pd.start_time_seconds <> ssdd.start_time
AND pd.is_start_time_modified = 0

-- if end time is different set is_end_time_modified = 1
UPDATE pd SET
	is_end_time_modified = 1
FROM plan_version_dayparts pd
JOIN @flat_standard_dayparts ssdd
	ON ssdd.standard_daypart_id = pd.standard_daypart_id
WHERE ssdd.code in ('EM', 'PMN', 'EN')
AND pd.end_time_seconds <> ssdd.end_time
AND pd.is_end_time_modified = 0

GO

/*************************************** End BP-2920 *****************************************************/

/*************************************** START BP-1101 ***************************************/
GO
IF OBJECT_ID('plan_version_buying_band_stations') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_band_stations](
		[id] int NOT NULL PRIMARY KEY IDENTITY(1,1),
		[plan_version_buying_result_id] int NOT NULL,
		[station_id] int NOT NULL,
		[impressions] float NOT NULL,
		[cost] money NOT NULL,
		[manifest_weeks_count] int NOT NULL,
		CONSTRAINT [FK_plan_version_buying_band_stations_plan_version_buying_results] FOREIGN KEY([plan_version_buying_result_id]) REFERENCES [dbo].[plan_version_buying_results] ([id]),
		CONSTRAINT [FK_plan_version_buying_band_stations_stations] FOREIGN KEY([station_id]) REFERENCES [dbo].[stations] ([id])
	)
END

GO

IF OBJECT_ID('plan_version_buying_band_station_dayparts') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_band_station_dayparts](
	[id] int NOT NULL PRIMARY KEY IDENTITY(1,1),
	[plan_version_buying_band_station_id] [int] NOT NULL,
	[active_days] [int] NOT NULL,
	[hours] [int] NOT NULL,
	CONSTRAINT [FK_plan_version_buying_band_station_dayparts_plan_version_buying_band_stations] FOREIGN KEY([plan_version_buying_band_station_id]) REFERENCES [dbo].[plan_version_buying_band_stations] ([id])
	)
END
GO
/*************************************** END BP-1101 ***************************************/
/*************************************** START BP-3266 ***************************************/
IF OBJECT_ID('spot_exceptions_ingest_jobs') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_ingest_jobs]
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
		[status] INT NOT NULL,
		[queued_at] DATETIME2 NOT NULL,
		[queued_by] VARCHAR(100) NOT NULL,
		[completed_at] DATETIME2 NULL,
		[error_message] NVARCHAR(MAX) NULL
	)
END
GO
IF OBJECT_ID('spot_exceptions_recommended_plans') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_recommended_plans]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1),
	[estimate_id] INT NOT NULL,
	[isci_name] VARCHAR(50) NOT NULL,
	[recommended_plan_id] INT NULL ,
	[program_name] NVARCHAR(500) NULL,
	[program_air_time] DATETIME NOT NULL,	
	[station_legacy_call_letters] VARCHAR(15) NULL,
	[cost] MONEY NULL,
	[impressions] FLOAT NULL,
	[spot_lenth_id] INT NULL,
	[audience_id] INT NULL,
	[product] NVARCHAR(100) NULL,
	[flight_start_date] DATETIME NULL,
	[flight_end_date] DATETIME NULL,
	[daypart_id] INT NULL,
	CONSTRAINT [FK_spot_exceptions_recommended_plans_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plans_spot_lengths] FOREIGN KEY ([spot_lenth_id]) REFERENCES [dbo].[spot_lengths]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plans_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plans_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts]([ID])
)
END
GO
IF OBJECT_ID('spot_exceptions_recommended_plan_details') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_recommended_plan_details]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,
	[spot_exceptions_recommended_plan_id] INT NOT NULL,
	[recommended_plan_id] INT NOT NULL,
	[metric_percent] FLOAT NOT NULL,
	CONSTRAINT [FK_spot_exceptions_recommended_plan_details_spot_exceptions_recommended_plans] FOREIGN KEY ([spot_exceptions_recommended_plan_id]) REFERENCES [dbo].[spot_exceptions_recommended_plans]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plan_details_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),	
)
END
GO
IF OBJECT_ID('spot_exceptions_recommended_plan_decision') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_recommended_plan_decision]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,
	[selected_details_id] INT NOT NULL,
	[username] VARCHAR(63) NOT NULL,
	[created_at] DATETIME NOT NULL,	
	CONSTRAINT [FK_spot_exceptions_recommended_plan_decision_spot_exceptions_recommended_plans_details] FOREIGN KEY ([selected_details_id]) REFERENCES [dbo].[spot_exceptions_recommended_plan_details]([ID])
)
END
GO
IF OBJECT_ID('spot_exceptions_out_of_specs') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_out_of_specs]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,
	[reason_code] VARCHAR(20) NOT NULL, 
	[reason_code_message] NVARCHAR(500) NULL, 
	[estimate_id] INT NOT NULL,
	[isci_name] VARCHAR(100) NOT NULL,
	[recommended_plan_id] INT NULL,
	[program_name] NVARCHAR(500) NULL,
	[station_legacy_call_letters] VARCHAR(15) NULL,
	[spot_lenth_id] INT NULL,
	[audience_id] INT NULL,
	[product] NVARCHAR(100) NULL,
	[flight_start_date] DATETIME NULL,
	[flight_end_date] DATETIME NULL,
	[daypart_id] INT NULL,
	[program_daypart_id] INT NOT NULL,
	[program_flight_start_date] DATETIME NOT NULL,
	[program_flight_end_date] DATETIME NOT NULL,
	[program_network] VARCHAR(10),
	[program_audience_id] INT NULL,
	[program_air_time] DATETIME NOT NULL,
	CONSTRAINT [FK_spot_exceptions_out_of_specs_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_spot_lengths] FOREIGN KEY ([spot_lenth_id]) REFERENCES [dbo].[spot_lengths]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_dayparts_program] FOREIGN KEY ([program_daypart_id]) REFERENCES [dbo].[dayparts]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_audiences_program] FOREIGN KEY ([program_audience_id]) REFERENCES [dbo].[audiences]([ID])		
)
END
GO
IF OBJECT_ID('spot_exceptions_out_of_spec_decisions') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_out_of_spec_decisions]
(
	id INT IDENTITY(1,1) PRIMARY KEY,
	spot_exceptions_recommended_plan_id INT NOT NULL,
	accepted_as_in_spec BIT NOT NULL,
	decision_notes NVARCHAR(1024) NULL,
	username VARCHAR(63) NOT NULL,
	created_at DATETIME NOT NULL,
	CONSTRAINT [FK_spot_exceptions_out_of_spec_decisions_spot_exceptions_recommended_plans] FOREIGN KEY ([spot_exceptions_recommended_plan_id]) REFERENCES [dbo].[spot_exceptions_recommended_plans]([ID])
)
END
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND COLUMN_NAME='spot_lenth_id')
BEGIN

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'spot_exceptions_out_of_spec_decisions')
BEGIN
DROP TABLE spot_exceptions_out_of_spec_decisions
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'spot_exceptions_out_of_specs')
BEGIN
DROP TABLE spot_exceptions_out_of_specs
END

END
GO

IF OBJECT_ID('spot_exceptions_out_of_specs') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_out_of_specs]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,
	[reason_code] VARCHAR(20) NOT NULL, 
	[reason_code_message] NVARCHAR(500) NULL, 
	[estimate_id] INT NOT NULL,
	[isci_name] VARCHAR(100) NOT NULL,
	[recommended_plan_id] INT NULL,
	[program_name] NVARCHAR(500) NULL,
	[station_legacy_call_letters] VARCHAR(15) NULL,
	[spot_length_id] INT NULL,
	[audience_id] INT NULL,
	[product] NVARCHAR(100) NULL,
	[flight_start_date] DATETIME NULL,
	[flight_end_date] DATETIME NULL,
	[daypart_id] INT NULL,
	[program_daypart_id] INT NOT NULL,
	[program_flight_start_date] DATETIME NOT NULL,
	[program_flight_end_date] DATETIME NOT NULL,
	[program_network] VARCHAR(10),
	[program_audience_id] INT NULL,
	[program_air_time] DATETIME NOT NULL,
	[ingested_by] VARCHAR(100) NOT NULL, 
    [ingested_at] DATETIME NOT NULL,
	CONSTRAINT [FK_spot_exceptions_out_of_specs_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_dayparts_program] FOREIGN KEY ([program_daypart_id]) REFERENCES [dbo].[dayparts]([ID]),
	CONSTRAINT [FK_spot_exceptions_out_of_specs_audiences_program] FOREIGN KEY ([program_audience_id]) REFERENCES [dbo].[audiences]([ID])		
)
END
GO

IF OBJECT_ID('spot_exceptions_out_of_spec_decisions') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_out_of_spec_decisions]
(
	id INT IDENTITY(1,1) PRIMARY KEY,
	spot_exceptions_out_of_spec_id INT NOT NULL,
	accepted_as_in_spec BIT NOT NULL,
	decision_notes NVARCHAR(1024) NULL,
	username VARCHAR(63) NOT NULL,
	created_at DATETIME NOT NULL,
	CONSTRAINT [FK_spot_exceptions_out_of_spec_decisions_spot_exceptions_out_of_specs] FOREIGN KEY ([spot_exceptions_out_of_spec_id]) REFERENCES [dbo].[spot_exceptions_out_of_specs]([ID])
)
END
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_recommended_plans' AND COLUMN_NAME='spot_lenth_id')
BEGIN

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'spot_exceptions_recommended_plan_decision')
BEGIN
DROP TABLE spot_exceptions_recommended_plan_decision
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'spot_exceptions_recommended_plan_details')
BEGIN
DROP TABLE spot_exceptions_recommended_plan_details
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'spot_exceptions_recommended_plans')
BEGIN
DROP TABLE spot_exceptions_recommended_plans
END

END
GO

IF OBJECT_ID('spot_exceptions_recommended_plans') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_recommended_plans]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1),
	[estimate_id] INT NOT NULL,
	[isci_name] VARCHAR(50) NOT NULL,
	[recommended_plan_id] INT NULL ,
	[program_name] NVARCHAR(500) NULL,
	[program_air_time] DATETIME NOT NULL,	
	[station_legacy_call_letters] VARCHAR(15) NULL,
	[cost] MONEY NULL,
	[impressions] FLOAT NULL,
	[spot_length_id] INT NULL,
	[audience_id] INT NULL,
	[product] NVARCHAR(100) NULL,
	[flight_start_date] DATETIME NULL,
	[flight_end_date] DATETIME NULL,
	[daypart_id] INT NULL,
	[ingested_by] VARCHAR(100) NOT NULL, 
    [ingested_at] DATETIME NOT NULL,
	CONSTRAINT [FK_spot_exceptions_recommended_plans_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plans_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plans_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plans_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts]([ID])
)
END
GO

IF OBJECT_ID('spot_exceptions_recommended_plan_details') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_recommended_plan_details]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,
	[spot_exceptions_recommended_plan_id] INT NOT NULL,-- FK (spot_exceptions_recommended_plans.id),
	[recommended_plan_id] INT NOT NULL,-- FK(plans.id),
	[metric_percent] FLOAT NOT NULL,
	[is_recommended_plan] BIT NOT NULL,
	CONSTRAINT [FK_spot_exceptions_recommended_plan_details_spot_exceptions_recommended_plans] FOREIGN KEY ([spot_exceptions_recommended_plan_id]) REFERENCES [dbo].[spot_exceptions_recommended_plans]([ID]),
	CONSTRAINT [FK_spot_exceptions_recommended_plan_details_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),	
)
END
GO

IF OBJECT_ID('spot_exceptions_recommended_plan_decision') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_recommended_plan_decision]
(
	[id] INT IDENTITY(1,1) PRIMARY KEY,	
	[spot_exceptions_recommended_plan_detail_id] INT NOT NULL,
	[username] VARCHAR(63) NOT NULL,
	[created_at] DATETIME NOT NULL,
	CONSTRAINT [FK_spot_exceptions_recommended_plan_decision_spot_exceptions_recommended_plans_details] FOREIGN KEY ([spot_exceptions_recommended_plan_detail_id]) REFERENCES [dbo].[spot_exceptions_recommended_plan_details]([ID])
)
END
GO

/*************************************** END BP-3266 ***************************************/

/*************************************** START BP-3244 ***************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'is_locked'
          AND Object_ID = Object_ID(N'plan_version_weekly_breakdown'))
BEGIN
    ALTER TABLE plan_version_weekly_breakdown
		ADD is_locked bit NULL	
END

/*************************************** END BP-3244 ***************************************/

/*************************************** START BP-3216 ***************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'advertiser_name'
          AND Object_ID = Object_ID(N'spot_exceptions_recommended_plans'))
BEGIN
    ALTER TABLE spot_exceptions_recommended_plans
		ADD advertiser_name NVARCHAR(100) NULL	
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'advertiser_name'
          AND Object_ID = Object_ID(N'spot_exceptions_out_of_specs'))
BEGIN
    ALTER TABLE spot_exceptions_out_of_specs
		ADD advertiser_name NVARCHAR(100) NULL	
END
/*************************************** START BP-3216 ***************************************/


/*************************************** START BP-3128 ***************************************/

DECLARE @pricingAddColumnSql VARCHAR(MAX) = 
'ALTER TABLE plan_version_pricing_parameters 
	ADD budget_cpm_lever INT NULL'

DECLARE @pricingPopulateSql VARCHAR(MAX) =
'UPDATE plan_version_pricing_parameters SET budget_cpm_lever = 1 WHERE budget_cpm_lever IS NULL'

DECLARE @pricingAlterSql VARCHAR(MAX) = 
'ALTER TABLE plan_version_pricing_parameters 
	ALTER COLUMN budget_cpm_lever INT NOT NULL'

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE  object_id = OBJECT_ID('plan_version_pricing_parameters') AND name = 'budget_cpm_lever')
BEGIN 	
	EXEC (@pricingAddColumnSql)
	EXEC (@pricingPopulateSql)
	EXEC (@pricingAlterSql)
END

DECLARE @buyingAddColumnSql VARCHAR(MAX) = 
'ALTER TABLE plan_version_buying_parameters 
	ADD budget_cpm_lever INT NULL'

DECLARE @buyingPopulateSql VARCHAR(MAX) =
'UPDATE plan_version_buying_parameters SET budget_cpm_lever = 1 WHERE budget_cpm_lever IS NULL'

DECLARE @buyingAlterSql VARCHAR(MAX) = 
'ALTER TABLE plan_version_buying_parameters 
	ALTER COLUMN budget_cpm_lever INT NOT NULL'

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE  object_id = OBJECT_ID('plan_version_buying_parameters') AND name = 'budget_cpm_lever')
BEGIN 	
	EXEC (@buyingAddColumnSql)
	EXEC (@buyingPopulateSql)
	EXEC (@buyingAlterSql)
END

GO
/*************************************** END BP-3128 ***************************************/

/*************************************** START BP-3163 ***************************************/

DECLARE @AddSql VARCHAR(MAX) = 
'ALTER TABLE scx_generation_job_files 
	ADD shared_folder_files_id UNIQUEIDENTIFIER NULL'

DECLARE @IndexSql VARCHAR(MAX) = 
'ALTER TABLE scx_generation_job_files
	ADD CONSTRAINT FK_scx_generation_job_files_shared_folder_files_id
	FOREIGN KEY (shared_folder_files_id) REFERENCES shared_folder_files(id)'

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[scx_generation_job_files]') AND name = 'shared_folder_files_id')
BEGIN 
	EXEC(@AddSql)
	EXEC(@IndexSql)
END

GO
/*************************************** END BP-3163 ***************************************/

/*************************************** START BP-3387 ***************************************/

DECLARE @MetaData TABLE
(
	plan_version_id INT,
	audience_id INT,
	standard_daypart_id INT,
	vpvh_type INT,
	starting_point DATETIME
)

DECLARE @DefaultVpvhs TABLE
(
	audience_id INT,
	standard_daypart_id INT,
	default_vpvh_value FLOAT
)

DECLARE @RecordsToAdd TABLE
(
	plan_version_id INT,
	audience_id INT,
	standard_daypart_id INT,
	vpvh_type INT,
	starting_point DATETIME,
	default_vpvh_value FLOAT
)

INSERT INTO @MetaData (plan_version_id, audience_id, standard_daypart_id, vpvh_type, starting_point) 
	SELECT DISTINCT
		v.id AS plan_version_id
		, v.target_audience_id AS audience_id 
		, pd.standard_daypart_id AS standard_daypart_id
		, CASE 
			WHEN v.target_audience_id = 46 THEN 1 
			ELSE 2 
		END AS vpvh_type -- Custom=1;FourBookAverage=2;PreviousQuarter=3;LastYear=4;		
		, v.created_date AS starting_point
	FROM plans p
	JOIN plan_versions v
		ON p.latest_version_id = v.id
	JOIN plan_version_dayparts pd
		ON pd.plan_version_id = v.id
	JOIN standard_dayparts sd
		ON sd.id = pd.standard_daypart_id
	LEFT OUTER JOIN plan_version_audience_daypart_vpvh dv
		ON dv.plan_version_id = v.id
		AND dv.standard_daypart_id = pd.standard_daypart_id
	WHERE dv.id IS NULL
	ORDER BY v.id DESC

INSERT INTO @DefaultVpvhs (audience_id, standard_daypart_id, default_vpvh_value) VALUES
	(37,1,0.36375)
	,(40,1,0.6365)
	,(56,1,0.1565)
	,(277,1,0.475)
	,(31,2,1)
	,(31,3,1)
	,(37,3,0.34)
	,(40,3,0.599)
	,(37,4,0.34)
	,(33,6,0.34025)
	,(37,8,0.424)
	,(57,8,0.1775)
	,(31,10,1)
	,(33,10,0.34025)
	,(46,10,0.021954) 
	,(56,10,0.19525)
	,(58,10,0.24675)
	,(62,10,0.49825)
	,(31,11,1)
	,(31,12,1)
	,(37,12,0.424)
	,(56,12,0.19525)
	,(277,12,0.474)
	,(37,14,0.424)
	,(56,14,0.19525)
	,(37,15,0.36375)
	,(37,17,0.35175)
	,(40,17,0.618)
	,(58,17,0.20025)
	,(277,17,0.505)
	,(37,19,0.424)
	,(48,19,0.05025)
	,(58,19,0.24675)
	,(36,22,0.264)
	,(40,22,0.63025)
	,(58,22,0.2155)

INSERT INTO @RecordsToAdd (plan_version_id, audience_id, standard_daypart_id, vpvh_type, starting_point, default_vpvh_value) 
	SELECT p.plan_version_id, p.audience_id, p.standard_daypart_id, p.vpvh_type, p.starting_point
		, v.default_vpvh_value
	FROM @MetaData p
	LEFT OUTER JOIN @DefaultVpvhs v
		ON v.audience_id = p.audience_id
		AND v.standard_daypart_id = p.standard_daypart_id

INSERT INTO plan_version_audience_daypart_vpvh (plan_version_id, audience_id, standard_daypart_id, vpvh_type, starting_point, vpvh_value)
	SELECT a.plan_version_id, a.audience_id, a.standard_daypart_id, a.vpvh_type, a.starting_point, a.default_vpvh_value
	FROM @RecordsToAdd a
	LEFT OUTER JOIN plan_version_audience_daypart_vpvh p
		ON p.plan_version_id = a.plan_version_id
		AND p.audience_id = a.audience_id
		AND p.standard_daypart_id = a.standard_daypart_id
	WHERE p.id IS NULL

UPDATE plan_versions SET impressions_per_unit = 1 WHERE impressions_per_unit = 0 

GO
/*************************************** END BP-3387 ***************************************/

/*************************************** START BP-3164 ***************************************/

DECLARE @AddSql VARCHAR(MAX) = 
'ALTER TABLE inventory_export_jobs 
	ADD shared_folder_files_id UNIQUEIDENTIFIER NULL'

DECLARE @IndexSql VARCHAR(MAX) = 
'ALTER TABLE inventory_export_jobs
	ADD CONSTRAINT FK_inventory_export_job_files_shared_folder_files_id
	FOREIGN KEY (shared_folder_files_id) REFERENCES shared_folder_files(id)'

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[inventory_export_jobs]') AND name = 'shared_folder_files_id')
BEGIN 
	EXEC(@AddSql)
	EXEC(@IndexSql)
END

GO
/*************************************** END BP-3164 ***************************************/

/*************************************** START BP-3165 ***************************************/

DECLARE @AddSql VARCHAR(MAX) = 
'ALTER TABLE inventory_files 
	ADD shared_folder_files_id UNIQUEIDENTIFIER NULL;

ALTER TABLE inventory_files 
	ADD error_file_shared_folder_files_id UNIQUEIDENTIFIER NULL;'

DECLARE @IndexSql VARCHAR(MAX) = 
'ALTER TABLE inventory_files
	ADD CONSTRAINT FK_inventory_files_shared_folder_files_id
	FOREIGN KEY (shared_folder_files_id) REFERENCES shared_folder_files(id);

ALTER TABLE inventory_files
	ADD CONSTRAINT FK_inventory_files_error_file_shared_folder_files_id
	FOREIGN KEY (error_file_shared_folder_files_id) REFERENCES shared_folder_files(id);'

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[inventory_files]') AND name = 'shared_folder_files_id')
BEGIN 
	EXEC(@AddSql)
	EXEC(@IndexSql)
END

GO
/*************************************** END BP-3165 ***************************************/

/*************************************** START BP-3504 ***************************************/
GO

IF OBJECT_ID('spot_exceptions_out_of_spec_reason_codes') IS NULL
BEGIN
	CREATE TABLE [dbo].[spot_exceptions_out_of_spec_reason_codes](
		[id] [int] NOT NULL PRIMARY KEY IDENTITY(1,1),
		[reason_code] [int] NOT NULL,
		[reason] [nvarchar](200) NOT NULL,
		[label] [nvarchar](200) NULL
	)
END

GO

IF NOT EXISTS (SELECT 1 FROM spot_exceptions_out_of_spec_reason_codes WHERE reason_code = 0)
BEGIN
	INSERT INTO [dbo].[spot_exceptions_out_of_spec_reason_codes]([reason_code],[reason],[label])
		 VALUES (0,'spot aired on hiatus day','Hiatus')
END

IF NOT EXISTS (SELECT 1 FROM spot_exceptions_out_of_spec_reason_codes WHERE reason_code = 1)
BEGIN
	INSERT INTO [dbo].[spot_exceptions_out_of_spec_reason_codes]([reason_code],[reason],[label])
		 VALUES (1,'spot aired outside daypart','Daypart')
END

IF NOT EXISTS (SELECT 1 FROM spot_exceptions_out_of_spec_reason_codes WHERE reason_code = 2)
BEGIN
	INSERT INTO [dbo].[spot_exceptions_out_of_spec_reason_codes]([reason_code],[reason],[label])
		 VALUES (2,'genre content restriction','Genre')
END

IF NOT EXISTS (SELECT 1 FROM spot_exceptions_out_of_spec_reason_codes WHERE reason_code = 3)
BEGIN
	INSERT INTO [dbo].[spot_exceptions_out_of_spec_reason_codes]([reason_code],[reason],[label])
		 VALUES (3,'affiliate content restriction','Affiliate')
END

IF NOT EXISTS (SELECT 1 FROM spot_exceptions_out_of_spec_reason_codes WHERE reason_code = 4)
BEGIN
	INSERT INTO [dbo].[spot_exceptions_out_of_spec_reason_codes]([reason_code],[reason],[label])
		 VALUES (4,'program content restriction','Program')
END

GO

DECLARE @SpotExceptionsOutOfSpecsAddColumnSql VARCHAR(MAX) = 
'ALTER TABLE spot_exceptions_out_of_specs
		ADD reason_code_id INT NULL'

DECLARE @SpotExceptionsOutOfSpecsUpdateColumnSql VARCHAR(MAX) = 
'UPDATE spot_exceptions_out_of_specs
		SET reason_code_id = 1
	WHERE reason_code_id IS NULL'

DECLARE @SpotExceptionsOutOfSpecsAlterColumnSql VARCHAR(MAX) = 
'ALTER TABLE spot_exceptions_out_of_specs 
		ALTER COLUMN reason_code_id INT NOT NULL'

DECLARE @SpotExceptionsOutOfSpecsAddConstraintSql VARCHAR(MAX) = 
'ALTER TABLE spot_exceptions_out_of_specs
		ADD CONSTRAINT FK_spot_exceptions_out_of_specs_spot_exceptions_out_of_spec_reason_codes
		FOREIGN KEY (reason_code_id) REFERENCES spot_exceptions_out_of_spec_reason_codes(id)'

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'reason_code_id' AND Object_ID = Object_ID(N'spot_exceptions_out_of_specs'))
BEGIN
	EXEC (@SpotExceptionsOutOfSpecsAddColumnSql)
	EXEC (@SpotExceptionsOutOfSpecsUpdateColumnSql)
	EXEC (@SpotExceptionsOutOfSpecsAlterColumnSql)
	EXEC (@SpotExceptionsOutOfSpecsAddConstraintSql)
END

GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'reason_code' AND Object_ID = Object_ID(N'spot_exceptions_out_of_specs'))
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs 
			DROP COLUMN reason_code
END

GO

/*************************************** END BP-3504 ***************************************/
/*************************************** START BP-3567 ***************************************/
IF OBJECT_ID('custom_daypart_organizations') IS NULL
BEGIN
	CREATE TABLE [dbo].[custom_daypart_organizations]
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
		[organization_name] NVARCHAR(100) NOT NULL,
	)	
END

IF NOT EXISTS (SELECT top 1 * FROM [custom_daypart_organizations])
BEGIN	
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('NFL')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('MLB')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('MLS')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('NBA')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('PGA')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('LPGA')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('NHL')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('NCAA')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('NCAA Football')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('NCAA Basketball')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('NASCAR')
INSERT INTO [dbo].[custom_daypart_organizations]([organization_name])VALUES('Other')
END
GO
/*************************************** END BP-3567 ***************************************/
/*************************************** START BP-3568 ***************************************/

IF OBJECT_ID('plan_version_custom_dayparts') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_custom_dayparts]
	(
		[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
		[custom_daypart_organization_id] INT NOT NULL, 
		[custom_daypart_name] NVARCHAR(100)  NOT NULL,
		[start_time_seconds] [int] NOT NULL,
		[end_time_seconds] [int] NOT NULL,
		[weighting_goal_percent] [float] NULL,
		[daypart_type] [int] NOT NULL,
		[is_start_time_modified] [bit] NOT NULL,
		[is_end_time_modified] [bit] NOT NULL,
		[plan_version_id] [int] NOT NULL,
		[show_type_restrictions_contain_type] [int] NULL,
		[genre_restrictions_contain_type] [int] NULL,
		[program_restrictions_contain_type] [int] NULL,
		[affiliate_restrictions_contain_type] [int] NULL,
		[weekdays_weighting] [float] NULL,
		[weekend_weighting] [float] NULL,
		CONSTRAINT [FK_plan_version_custom_dayparts_custom_daypart_organizations] FOREIGN KEY ([custom_daypart_organization_id]) REFERENCES [dbo].[custom_daypart_organizations]([ID])
	)	
END
Go
/*************************************** END BP-3568 ***************************************/
/*************************************** START BP-3612 ***************************************/
IF OBJECT_ID('plan_version_custom_daypart_affiliate_restrictions') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_custom_daypart_affiliate_restrictions]
	(
		[id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
		[plan_version_custom_daypart_id] [int] NOT NULL,
		[affiliate_id] [int] NOT NULL,
		CONSTRAINT [FK_plan_version_custom_daypart_affiliate_restrictions_affiliates] FOREIGN KEY ([affiliate_id]) REFERENCES [dbo].[affiliates]([ID]),
		CONSTRAINT [FK_plan_version_custom_daypart_affiliate_restrictions_plan_version_custom_dayparts] FOREIGN KEY([plan_version_custom_daypart_id])REFERENCES [dbo].[plan_version_custom_dayparts] ([id])
		
	)	
END

IF OBJECT_ID('plan_version_custom_daypart_genre_restrictions') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_custom_daypart_genre_restrictions]
	(
		[id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
		[plan_version_custom_daypart_id] [int] NOT NULL,
		[genre_id] [int] NOT NULL,
		CONSTRAINT UQ_plan_version_custom_daypart_genre_restrictions UNIQUE (plan_version_custom_daypart_id,genre_id),
		CONSTRAINT [FK_plan_version_custom_daypart_genre_restrictions_genres] FOREIGN KEY([genre_id]) REFERENCES [dbo].[genres] ([id]),
		CONSTRAINT [FK_plan_version_custom_daypart_genre_restrictions_plan_version_custom_dayparts] FOREIGN KEY([plan_version_custom_daypart_id]) REFERENCES [dbo].[plan_version_custom_dayparts] ([id])
		
	)	
END


IF OBJECT_ID('plan_version_custom_daypart_program_restrictions') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_custom_daypart_program_restrictions]
	(
		[id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
		[plan_version_custom_daypart_id] [int] NOT NULL,
		[program_name] [varchar](255) NULL,
		[genre_id] [int] NULL,
		[content_rating] [varchar](15) NULL,
		CONSTRAINT UC_plan_version_custom_daypart_program_restrictions UNIQUE (plan_version_custom_daypart_id,[program_name],genre_id),
		CONSTRAINT [FK_plan_version_custom_daypart_program_restrictions_plan_version_custom_dayparts] FOREIGN KEY([plan_version_custom_daypart_id])REFERENCES [dbo].[plan_version_custom_dayparts] ([id])
		
	)	
END

IF OBJECT_ID('plan_version_custom_daypart_show_type_restrictions') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_custom_daypart_show_type_restrictions]
	(
		[id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
		[plan_version_custom_daypart_id] [int] NOT NULL,
		[show_type_id] [int] NOT NULL,
		CONSTRAINT UQ_plan_version_custom_daypart_show_type_restrictions UNIQUE (plan_version_custom_daypart_id,show_type_id),
		CONSTRAINT [FK_plan_version_custom_daypart_show_type_restrictions_plan_version_custom_dayparts] FOREIGN KEY([plan_version_custom_daypart_id])REFERENCES [dbo].[plan_version_custom_dayparts] ([id]),
		CONSTRAINT [FK_plan_version_custom_daypart_show_type_restrictions_show_types] FOREIGN KEY([show_type_id])REFERENCES [dbo].[show_types] ([id])
		
	)	
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'plan_version_id' AND Object_ID = Object_ID(N'plan_version_custom_dayparts'))
BEGIN
	
	IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_plan_version_custom_dayparts_plan_versions')
	    BEGIN
		   ALTER TABLE plan_version_custom_dayparts
			ADD CONSTRAINT FK_plan_version_custom_dayparts_plan_versions
			FOREIGN KEY (plan_version_id) REFERENCES plan_versions(id)
		END
	
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'genre_id' AND Object_ID = Object_ID(N'plan_version_custom_daypart_program_restrictions'))
BEGIN
	
	IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_plan_version_custom_daypart_program_restrictions_genres')
	    BEGIN
		   ALTER TABLE plan_version_custom_daypart_program_restrictions
		   ADD CONSTRAINT FK_plan_version_custom_daypart_program_restrictions_genres
			FOREIGN KEY([genre_id])REFERENCES [dbo].[genres] ([id])
		END
	
END
GO
/*************************************** END BP-3612 ***************************************/

/*************************************** START BP-3156 ***************************************/

DECLARE @Sql_RemoveUniqueConstraint VARCHAR(MAX) = '
DROP INDEX [UX_plan_iscis_plan_id_isci] ON [dbo].[plan_iscis]
'

DECLARE @Sql_AddColumns VARCHAR(MAX) = '
ALTER TABLE plan_iscis ADD flight_start_date DATE NULL
ALTER TABLE plan_iscis ADD flight_end_date DATE NULL
'

DECLARE @Sql_PopulateColumns VARCHAR(MAX) = '
SELECT DISTINCT pip.plan_id, pip.isci			
	, CASE 
		WHEN v.flight_start_date >= r.active_start_date THEN v.flight_start_date
		ELSE r.active_start_date
	END AS plan_isci_flight_start_date
	, CASE 
		WHEN v.flight_end_date <= r.active_end_date THEN v.flight_end_date
		ELSE r.active_end_date
	END AS plan_isci_flight_end_date
	INTO #plan_isci_flight_dates
FROM plan_iscis pip
JOIN plans p
	ON pip.plan_id = p.id
JOIN plan_versions v
	ON p.latest_version_id = v.id
JOIN reel_iscis r
	ON r.isci = pip.isci
WHERE r.active_start_date <= v.flight_end_date
AND r.active_end_date >= v.flight_start_date

UPDATE pip SET 
	flight_start_date = pid.plan_isci_flight_start_date
	, flight_end_date = pid.plan_isci_flight_end_date
FROM plan_iscis pip	
JOIN #plan_isci_flight_dates pid
	ON pip.plan_id = pid.plan_id
	AND pip.isci = pid.isci
WHERE pip.flight_start_date IS NULL

INSERT INTO plan_iscis (plan_id, isci, created_at, created_by, flight_start_date, flight_end_date)
	SELECT pid.plan_id, pid.isci, SYSDATETIME(), ''system_migration_script'', pid.plan_isci_flight_start_date, pid.plan_isci_flight_end_date
	FROM #plan_isci_flight_dates pid
	LEFT OUTER JOIN plan_iscis pip
		ON pid.plan_id = pip.plan_id
		AND pid.isci = pip.isci
		AND CAST(pid.plan_isci_flight_start_date AS DATE) = CAST(pip.flight_start_date AS DATE)
		AND CAST(pid.plan_isci_flight_end_date AS DATE) = CAST(pip.flight_end_date AS DATE)
	WHERE pip.plan_id IS NULL

DELETE FROM plan_iscis 
WHERE flight_start_date is null 
OR flight_end_date is null
'

DECLARE @Sql_FinalizeColumns VARCHAR(MAX) = '
ALTER TABLE plan_iscis ALTER COLUMN flight_start_date DATE NOT NULL
ALTER TABLE plan_iscis ALTER COLUMN flight_end_date DATE NOT NULL
'

DECLARE @Sql_ReplaceUniqueConstraint VARCHAR(MAX) = '
CREATE UNIQUE NONCLUSTERED INDEX [UX_plan_iscis_plan_id_isci] ON [dbo].[plan_iscis]
(
	[plan_id] ASC,
	[isci] ASC,
	[flight_start_date] ASC,
	[flight_end_date] ASC
)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
'

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'flight_start_date' AND OBJECT_ID = OBJECT_ID(N'plan_iscis'))
BEGIN
	EXEC (@Sql_RemoveUniqueConstraint)
	EXEC (@Sql_AddColumns)
	EXEC (@Sql_PopulateColumns)
	EXEC (@Sql_FinalizeColumns)
	EXEC (@Sql_ReplaceUniqueConstraint)
END

GO
/*************************************** END BP-3156 ***************************************/

/*************************************** START BP-3657 ***************************************/

IF NOT EXISTS (SELECT 1 FROM standard_dayparts WHERE daypart_type = 4 AND code = 'CSP')
BEGIN	
	DECLARE @daypart_id INT = NULL
	SELECT @daypart_id = id FROM dayparts WHERE daypart_text = 'M-SU 4AM-2AM'

	INSERT INTO standard_dayparts (daypart_type, daypart_id, code, [name], vpvh_calculation_source_type) VALUES
		(4, @daypart_id, 'CSP', 'Custom Sports', 3)
END

-- originally was deployed with vpvh_calculation_source_type = 1.  This will correct the mistake.
IF EXISTS (SELECT 1 FROM standard_dayparts WHERE daypart_type = 4 AND code = 'CSP' AND vpvh_calculation_source_type = 1)
BEGIN 
	UPDATE sd SET
		vpvh_calculation_source_type = 3 -- SYS_ALL
	FROM standard_dayparts sd 
	WHERE daypart_type = 4 
	AND code = 'CSP' 
	AND vpvh_calculation_source_type = 1
END

GO

IF OBJECT_ID('plan_version_daypart_customizations') IS NULL
BEGIN
	CREATE TABLE plan_version_daypart_customizations
	(
		id INT IDENTITY(1,1) ,
		plan_version_daypart_id INT NOT NULL,
		custom_daypart_organization_id INT NOT NULL, 
		custom_daypart_name NVARCHAR(100) NOT NULL,
		CONSTRAINT [FK_plan_version_daypart_customizations_plan_version_daypart] FOREIGN KEY([plan_version_daypart_id])REFERENCES [dbo].[plan_version_dayparts] ([id]) ON DELETE CASCADE,
		CONSTRAINT [FK_plan_version_daypart_customizations_custom_daypart_organizations] FOREIGN KEY([custom_daypart_organization_id])REFERENCES [dbo].[custom_daypart_organizations] ([id]),
		CONSTRAINT UQ_plan_version_daypart_customizations_plan_version_daypart_id UNIQUE (plan_version_daypart_id), 
		PRIMARY KEY ([id])
	)
END 

GO

-- this is to correct a mistake in the original.
IF OBJECT_ID('plan_version_daypart_customizations') IS NOT NULL
BEGIN
	DECLARE @pk_column VARCHAR(100),
		@constraint_name VARCHAR(100)
	
	SELECT @pk_column = c.COLUMN_NAME, @constraint_name = c.CONSTRAINT_NAME
	FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS t
	JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE c
		ON c.CONSTRAINT_NAME = t.CONSTRAINT_NAME  
	WHERE c.TABLE_NAME='plan_version_daypart_customizations'  
	AND t.CONSTRAINT_TYPE='PRIMARY KEY'

	IF (@pk_column <> 'id')
	BEGIN 
		DECLARE @Sql_DropPrimaryKey VARCHAR(MAX) = 'ALTER TABLE plan_version_daypart_customizations 
		DROP CONSTRAINT [' + @constraint_name + '];'

		DECLARE @Sql_CreatePrimaryKey VARCHAR(MAX) = 'ALTER TABLE plan_version_daypart_customizations 
		ADD PRIMARY KEY (ID);'

		EXEC(@Sql_DropPrimaryKey)
		EXEC(@Sql_CreatePrimaryKey)
	END
END

GO

/*************************************** END BP-3657 ***************************************/

/*************************************** Start BP-3716 *****************************************************/

IF NOT EXISTS(SELECT * FROM nti_to_nsi_conversion_rates WHERE [media_month_id]=474 and [standard_daypart_id]=24) 
	 BEGIN
	 INSERT [dbo].[nti_to_nsi_conversion_rates] ( [conversion_rate], [standard_daypart_id], [media_month_id]) VALUES (0.7, 24, 474)
	 END
ELSE
	  BEGIN
	  Update [dbo].[nti_to_nsi_conversion_rates] set [conversion_rate]=0.7 WHERE [media_month_id]=474 and [standard_daypart_id]=24
	  END
Go

/*************************************** Start BP-3716 *****************************************************/

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

