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

/*************************************** Start BP-3658 *****************************************************/

IF OBJECT_ID('plan_version_custom_daypart_affiliate_restrictions') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[plan_version_custom_daypart_affiliate_restrictions]
END

IF OBJECT_ID('plan_version_custom_daypart_genre_restrictions') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[plan_version_custom_daypart_genre_restrictions]
END

IF OBJECT_ID('plan_version_custom_daypart_program_restrictions') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[plan_version_custom_daypart_program_restrictions]
END

IF OBJECT_ID('plan_version_custom_daypart_show_type_restrictions') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[plan_version_custom_daypart_show_type_restrictions]
END

IF OBJECT_ID('plan_version_custom_dayparts') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[plan_version_custom_dayparts]
END

GO
/*************************************** END BP-3658 *****************************************************/

/*************************************** Start BP-3848 *****************************************************/
/*** Out of Spec - Staging Tables ***/
IF OBJECT_ID('staged_out_of_specs') IS NULL
BEGIN
	CREATE TABLE staged_out_of_specs
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		unique_id_external BIGINT NOT NULL,
		execution_id_external VARCHAR(100) NOT NULL,
		estimate_id INT NOT NULL,
		inventory_source VARCHAR(100) NOT NULL,
		house_isci VARCHAR(100) NOT NULL,
		client_isci VARCHAR(100) NOT NULL,
		client_spot_length INT NOT NULL,
		broadcast_aired_date DATETIME2 NOT NULL,
		aired_time INT NOT NULL,
		station_legacy_call_letters VARCHAR(30) NULL,
		affiliate VARCHAR(30) NULL,
		market_code INT NULL,
		market_rank INT NULL,
		rate MONEY NOT NULL,
		audience_name VARCHAR(127) NOT NULL,
		impressions FLOAT NOT NULL,
		[program_name] VARCHAR(500) NOT NULL,
		program_genre VARCHAR(127) NOT NULL,
		reason_code INT NOT NULL,
		reason_code_message VARCHAR(500) NULL,
		lead_in_program_name VARCHAR(500) NULL,
		lead_out_program_name VARCHAR(500) NULL,
		plan_id INT NOT NULL,
		daypart_code VARCHAR(10) NULL,
		start_time INT NULL,
		end_time INT NULL,
		monday INT NULL,
		tuesday INT NULL,
		wednesday INT NULL,
		thursday INT NULL,
		friday INT NULL,
		saturday INT NULL,
		sunday INT NULL,
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL
	)
END

/*** Out of Spec - Target Tables ***/
IF NOT EXISTS (SELECT 1 FROM sys.columns 
          WHERE [Name] = N'unique_id_external'
          AND OBJECT_ID = OBJECT_ID(N'spot_exceptions_out_of_specs'))
BEGIN	
	ALTER TABLE spot_exceptions_out_of_specs
		ADD unique_id_external BIGINT NULL

	ALTER TABLE spot_exceptions_out_of_specs
		ADD execution_id_external VARCHAR(100) NULL
END

DECLARE @oos_populationSql VARCHAR(MAX) = '
UPDATE spot_exceptions_out_of_specs SET
	 unique_id_external = 1,
	 execution_id_external = ''1''
WHERE unique_id_external IS NULL
'

DECLARE @oos_alterSql VARCHAR(MAX) = '
ALTER TABLE spot_exceptions_out_of_specs 
	ALTER COLUMN unique_id_external BIGINT NOT NULL

ALTER TABLE spot_exceptions_out_of_specs 
	ALTER COLUMN execution_id_external VARCHAR(100) NOT NULL
'
EXEC (@oos_populationSql)
EXEC (@oos_alterSql)


/*** Recommended Plans - Staging Tables ***/
IF OBJECT_ID('staged_recommended_plans') IS NULL
BEGIN
	CREATE TABLE staged_recommended_plans
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		unique_id_external BIGINT NOT NULL,
		execution_id_external VARCHAR(100) NOT NULL,
		ambiguity_cod INT NOT NULL,
		estimate_id INT NOT NULL,
		inventory_source VARCHAR(100) NOT NULL,
		house_isci VARCHAR(100) NOT NULL,
		client_isci VARCHAR(100) NOT NULL,
		client_spot_length INT NOT NULL,
		broadcast_aired_date DATETIME2 NOT NULL,
		aired_time INT NOT NULL,
		station_legacy_call_letters VARCHAR(30) NULL,
		affiliate VARCHAR(30) NULL,
		market_code INT NULL,
		market_rank INT NULL,
		rate MONEY NOT NULL,
		audience_name VARCHAR(127) NOT NULL,
		impressions FLOAT NOT NULL,
		[program_name] VARCHAR(500) NOT NULL,
		program_genre VARCHAR(127) NOT NULL,
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL
	)

	CREATE TABLE staged_recommended_plan_details
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		staged_recommended_plan_id INT NOT NULL, -- FK
		recommended_plan_id INT NOT NULL,
		is_recommended_plan BIT NOT NULL,
		plan_clearance_percentage FLOAT NULL,	
		daypart_code VARCHAR(10) NULL,
		start_time INT NULL,
		end_time INT NULL,
		monday INT NULL,
		tuesday INT NULL,
		wednesday INT NULL,
		thursday INT NULL,
		friday INT NULL,
		saturday INT NULL,
		sunday INT NULL,
	)

	ALTER TABLE staged_recommended_plan_details
		ADD CONSTRAINT FK_staged_recommended_plan_details_staged_recommended_plans
		FOREIGN KEY (staged_recommended_plan_id) REFERENCES staged_recommended_plans(id)
END

/*** Recommended Plans - Target Tables ***/
IF NOT EXISTS (SELECT 1 FROM sys.columns 
          WHERE [Name] = N'unique_id_external'
          AND OBJECT_ID = OBJECT_ID(N'spot_exceptions_recommended_plans'))
BEGIN	
	ALTER TABLE spot_exceptions_recommended_plans
		ADD unique_id_external BIGINT NULL

	ALTER TABLE spot_exceptions_recommended_plans
		ADD execution_id_external VARCHAR(100) NULL
END

DECLARE @rp_populationSql VARCHAR(MAX) = '
UPDATE spot_exceptions_recommended_plans SET
	 unique_id_external = 1,
	 execution_id_external = ''1''
WHERE unique_id_external IS NULL
'

DECLARE @rp_alterSql VARCHAR(MAX) = '
ALTER TABLE spot_exceptions_recommended_plans 
	ALTER COLUMN unique_id_external BIGINT NOT NULL

ALTER TABLE spot_exceptions_recommended_plans 
	ALTER COLUMN execution_id_external VARCHAR(100) NOT NULL
'
EXEC (@rp_populationSql)
EXEC (@rp_alterSql)

/*** Aggregate Tables ***/
IF OBJECT_ID('staged_unposted_no_plan') IS NULL
BEGIN
	CREATE TABLE staged_unposted_no_plan
	(
		id INT IDENTITY(1,1) PRIMARY KEY,
		house_isci VARCHAR(50) NOT NULL,
		client_isci VARCHAR(50) NOT NULL,
		spot_count INT NOT NULL,	
		program_air_time DATETIME2 NOT NULL,	
		estimate_id BIGINT NOT NULL,	
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL
	)
END

IF OBJECT_ID('staged_unposted_no_reel_roster') IS NULL
BEGIN
	CREATE TABLE staged_unposted_no_reel_roster
	(
		id INT IDENTITY(1,1) PRIMARY KEY,
		house_isci VARCHAR(50) NOT NULL,
		spot_count INT NOT NULL,	
		program_air_time DATETIME2 NOT NULL,
		estimate_id BIGINT NOT NULL,
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL
	)
END

GO

/*************************************** END BP-3848 *****************************************************/

/*************************************** Start BP-3770 *****************************************************/


DECLARE @AddColumnSql VARCHAR(MAX) = 
	'ALTER TABLE spot_exceptions_out_of_specs
			ADD impressions float NULL'

DECLARE @PopulateSql VARCHAR(MAX) =
	'UPDATE spot_exceptions_out_of_specs 
			SET impressions = 50000 
		WHERE impressions IS NULL'

DECLARE @AlterSql VARCHAR(MAX) = 
	'ALTER TABLE spot_exceptions_out_of_specs
			ALTER COLUMN impressions float NOT NULL'

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND COLUMN_NAME= 'impressions')
BEGIN
	EXEC (@AddColumnSql)
	EXEC (@PopulateSql)
	EXEC (@AlterSql)
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_recommended_plan_decision' AND (COLUMN_NAME= 'synced_by' OR COLUMN_NAME='synced_at'))
BEGIN
	ALTER TABLE spot_exceptions_recommended_plan_decision
	ADD synced_by VARCHAR(100) NULL,
	synced_at DATETIME2 NULL
END


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_spec_decisions' AND (COLUMN_NAME= 'synced_by' OR COLUMN_NAME='synced_at'))
BEGIN
	ALTER TABLE spot_exceptions_out_of_spec_decisions
	ADD synced_by VARCHAR(100) NULL,
	synced_at DATETIME2 NULL
END



IF OBJECT_ID('spot_exceptions_unposted_no_plan') IS NULL
BEGIN
	CREATE TABLE [dbo].spot_exceptions_unposted_no_plan
    (
		id INT IDENTITY(1,1) PRIMARY KEY,
		house_isci VARCHAR(50) NOT NULL,
		client_isci VARCHAR(50) NOT NULL,
		count INT NOT NULL,	
		program_air_time DATETIME NOT NULL,	
		estimate_id BIGINT NOT NULL,	
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL
    )
END


IF OBJECT_ID('spot_exceptions_unposted_no_reel_roster') IS NULL
BEGIN
    CREATE TABLE [dbo].spot_exceptions_unposted_no_reel_roster
     (
		id INT IDENTITY(1,1) PRIMARY KEY,
		house_isci VARCHAR(50) NOT NULL,
		count INT NOT NULL,	
		program_air_time DATETIME NOT NULL,	
		estimate_id BIGINT NOT NULL,
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL
    )
END

GO
/*************************************** END BP3770 *****************************************************/

/*************************************** START BP-3825 *****************************************************/

DECLARE @AddColumnSql VARCHAR(MAX) = 
	'ALTER TABLE plans
			ADD plan_mode INT NULL'

DECLARE @PopulateSql VARCHAR(MAX) =
	'UPDATE plans 
			SET plan_mode = 1 
		WHERE plan_mode IS NULL'

DECLARE @AlterSql VARCHAR(MAX) = 
	'ALTER TABLE plans
			ALTER COLUMN plan_mode INT NOT NULL'

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plans' AND COLUMN_NAME= 'plan_mode')
BEGIN
	EXEC (@AddColumnSql)
	EXEC (@PopulateSql)
	EXEC (@AlterSql)
END

GO

IF OBJECT_ID('[dbo].[plan_version_daypart_goals]') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_daypart_goals]
	(
		id INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
		plan_version_daypart_id INT NOT NULL,
		flight_start_Date DATETIME NOT NULL,
		flight_end_Date DATETIME NOT NULL,
		budget money NOT NULL,
		target_impression float NOT NULL,
		target_cpm money NOT NULL,
		target_rating_points float NOT NULL,
		target_cpp money NOT NULL,
		target_universe float NOT NULL,
		hh_impressions float NOT NULL,
		hh_cpm money NOT NULL,
		hh_rating_points float NOT NULL,
		hh_cpp money NOT NULL,
		hh_universe float NOT NULL,
		currency int NOT NULL,  
		coverage_goal_percent float NOT NULL,
		goal_breakdown_type int NOT NULL,
		is_adu_enabled bit NOT NULL,
		impressions_per_unit float NULL,
		CONSTRAINT [FK_plan_version_daypart_goals_plan_version_dayparts] FOREIGN KEY ([plan_version_daypart_id]) REFERENCES [dbo].[plan_version_dayparts](id) ON DELETE CASCADE
	)
END

GO

IF OBJECT_ID('[dbo].[plan_version_daypart_weekly_breakdown]') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_daypart_weekly_breakdown]
	(
		id int PRIMARY KEY IDENTITY(1,1) NOT NULL,
		plan_version_daypart_goal_id INT NOT NULL,
		media_week_id int NOT NULL,
		number_active_days int NOT NULL,
		active_days_label varchar(20) NULL,
		impressions float NOT NULL,
		impressions_percentage float NOT NULL,
		plan_version_id int NOT NULL,
		rating_points float NOT NULL,
		budget money NOT NULL,
		spot_length_id int NULL,
		percentage_of_week float NULL,
		adu_impressions float NOT NULL,
		unit_impressions float NULL,
		is_locked bit NULL,
		CONSTRAINT [FK_plan_version_daypart_weekly_breakdown_plan_version_daypart_goals] FOREIGN KEY ([plan_version_daypart_goal_id]) REFERENCES [dbo].[plan_version_daypart_goals](id) ON DELETE CASCADE,
		CONSTRAINT [FK_plan_version_daypart_weekly_breakdown_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] (id),
		CONSTRAINT [FK_plan_version_daypart_weekly_breakdown_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] (id)
	)	
END

GO

IF OBJECT_ID('[dbo].[plan_version_daypart_available_markets]') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_daypart_available_markets]
	(
		id int PRIMARY KEY IDENTITY(1,1) NOT NULL,
		plan_version_daypart_goal_id INT NOT NULL,
		market_code smallint NOT NULL,
		market_coverage_file_id int NOT NULL,
		share_of_voice_percent float NULL,	
		is_user_share_of_voice_percent bit NOT NULL,
		CONSTRAINT [FK_plan_version_daypart_available_markets_plan_version_daypart_goals] FOREIGN KEY ([plan_version_daypart_goal_id]) REFERENCES [dbo].[plan_version_daypart_goals](id) ON DELETE CASCADE,
		CONSTRAINT [FK_plan_version_daypart_available_markets_markets] FOREIGN KEY ([market_code]) REFERENCES [dbo].[markets] (market_code),
		CONSTRAINT [FK_plan_version_daypart_available_markets_market_coverage_file] FOREIGN KEY ([market_coverage_file_id]) REFERENCES [dbo].[market_coverage_files] (id)
	)		
END

GO

IF OBJECT_ID('[dbo].[plan_version_daypart_flight_days]') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_daypart_flight_days]
	(
		id int PRIMARY KEY IDENTITY(1,1) NOT NULL,
		plan_version_daypart_goal_id INT NOT NULL,
		day_id INT NOT NULL,
		CONSTRAINT [FK_plan_version_daypart_flight_days_plan_version_daypart_goals] FOREIGN KEY ([plan_version_daypart_goal_id]) REFERENCES [dbo].[plan_version_daypart_goals](id) ON DELETE CASCADE,
		CONSTRAINT [FK_plan_version_daypart_flight_days_days] FOREIGN KEY ([day_id]) REFERENCES [dbo].[days] (id)
	)	
END

GO

IF OBJECT_ID('[dbo].[plan_version_daypart_flight_hiatus_days]') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_daypart_flight_hiatus_days]
	(
		id int PRIMARY KEY IDENTITY(1,1) NOT NULL,
		plan_version_daypart_goal_id INT NOT NULL,
		hiatus_day DATETIME NOT NULL,
		CONSTRAINT [FK_plan_version_daypart_flight_hiatus_days_plan_version_daypart_goals] FOREIGN KEY ([plan_version_daypart_goal_id]) REFERENCES [dbo].[plan_version_daypart_goals](id) ON DELETE CASCADE
	)
END

GO

/*************************************** END BP-3825 *****************************************************/

/*************************************** START BP-3786 ***************************************/
DECLARE @Sql_AddColumns VARCHAR(MAX) = '
ALTER TABLE plan_iscis ADD modified_at DATETIME2(7) NULL
ALTER TABLE plan_iscis ADD modified_by VARCHAR(100) NULL
'

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = N'modified_at' AND OBJECT_ID = OBJECT_ID(N'plan_iscis'))
BEGIN
	EXEC (@Sql_AddColumns)
END

GO
/*************************************** END BP-3786 ***************************************/

/*************************************** START BP-3661 ***************************************/
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_weekly_breakdown' AND (COLUMN_NAME= 'custom_daypart_organization_id' OR COLUMN_NAME='custom_daypart_name'))
BEGIN
	ALTER TABLE plan_version_weekly_breakdown
	ADD custom_daypart_organization_id INT NULL,
	custom_daypart_name NVARCHAR(100) NULL
END

GO
/*************************************** END BP-3661 ***************************************/

/*************************************** START BP-3786 ***************************************/
DECLARE @Sql_PopulateColumns VARCHAR(MAX) = '
UPDATE plan_iscis SET modified_at = created_at, modified_by = created_by
'

DECLARE @Sql_FinalizeColumns VARCHAR(MAX) = '
ALTER TABLE plan_iscis ALTER COLUMN modified_at DATETIME2(7) NOT NULL
ALTER TABLE plan_iscis ALTER COLUMN modified_by VARCHAR(100) NOT NULL
'

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'modified_at' AND OBJECT_ID = OBJECT_ID(N'plan_iscis') and is_nullable = 1)
BEGIN
	EXEC (@Sql_PopulateColumns)
	EXEC (@Sql_FinalizeColumns)
END

GO
/*************************************** END BP-3786 ***************************************/

/*************************************** START BP-3284 *****************************************************/

DECLARE @AddColumnSql VARCHAR(MAX) = 
	'ALTER TABLE spot_exceptions_ingest_jobs ADD start_date DATETIME2(7) NULL;
	ALTER TABLE spot_exceptions_ingest_jobs ADD end_date DATETIME2(7) NULL;'

DECLARE @PopulateSql VARCHAR(MAX) =
	'UPDATE spot_exceptions_ingest_jobs  SET
			start_date = ''2022-01-03'',
			end_date = ''2022-01-09''
		WHERE start_date IS NULL'

DECLARE @AlterSql VARCHAR(MAX) = 
	'ALTER TABLE spot_exceptions_ingest_jobs ALTER COLUMN start_date DATETIME2(7) NOT NULL;
	ALTER TABLE spot_exceptions_ingest_jobs ALTER COLUMN end_date DATETIME2(7) NOT NULL;'

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_ingest_jobs' AND COLUMN_NAME= 'start_date')
BEGIN
	EXEC (@AddColumnSql)
	EXEC (@PopulateSql)
	EXEC (@AlterSql)
END

GO

/***************************************  END BP-3284  *****************************************************/

/*************************************** START BP-3829 *****************************************************/

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'flight_start_date' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN flight_start_date datetime NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'flight_end_date' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN flight_end_date datetime NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'budget' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN budget money NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'target_impression' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN target_impression float NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'target_cpm' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN target_cpm money NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'target_rating_points' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN target_rating_points float NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'target_cpp' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN target_cpp money NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'target_universe' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN target_universe float NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'hh_impressions' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN hh_impressions float NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'hh_cpm' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN hh_cpm money NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'hh_rating_points' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN hh_rating_points float NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'hh_cpp' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN hh_cpp money NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'hh_universe' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN hh_universe float NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'currency' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN currency int NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'coverage_goal_percent' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN coverage_goal_percent float NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'goal_breakdown_type' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN goal_breakdown_type int NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'is_adu_enabled' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN is_adu_enabled bit NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'impressions_per_unit' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN impressions_per_unit float NULL
END

GO

/*************************************** END BP-3829 *****************************************************/

/*************************************** START BP-3816 ***************************************************/

GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plans' AND COLUMN_NAME= 'deleted_by')
BEGIN
	ALTER TABLE plans
			ADD deleted_by VARCHAR(100) NULL
END

GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plans' AND COLUMN_NAME= 'deleted_at')
BEGIN
	ALTER TABLE plans
			ADD deleted_at DATETIME2 NULL
END

GO

/*************************************** END BP-3816 *****************************************************/

/*************************************** START BP-3786 ***************************************/

DECLARE @Sql_RemoveUniqueConstraint VARCHAR(MAX) = '
DROP INDEX [UX_plan_iscis_plan_id_isci] ON [dbo].[plan_iscis]
'

DECLARE @Sql_ReplaceUniqueConstraint VARCHAR(MAX) = '
CREATE UNIQUE NONCLUSTERED INDEX [UX_plan_iscis_plan_id_isci] ON [dbo].[plan_iscis]
(
	[plan_id] ASC,
	[isci] ASC,
	[deleted_at] ASC,
	[flight_start_date] ASC,
	[flight_end_date] ASC
)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
'

IF NOT EXISTS(select 1 from sys.tables as t
				inner join sys.columns as c
					on  t.object_id = c.object_id
				inner join sys.index_columns as ic 
					on c.column_id = ic.column_id and c.object_id = ic.object_id
				inner join sys.indexes as i
					on ic.index_id = i.index_id and ic.object_id = i.object_id
				where t.name = N'plan_iscis' and c.name = N'deleted_at')
BEGIN
	EXEC (@Sql_RemoveUniqueConstraint)
	EXEC (@Sql_ReplaceUniqueConstraint)
END

GO
/*************************************** END BP-3786 ***************************************/

/*************************************** START BP-4060 ***************************************/
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_audience_daypart_vpvh' AND COLUMN_NAME= 'daypart_customization_id')
BEGIN
    ALTER TABLE plan_version_audience_daypart_vpvh
    ADD daypart_customization_id INT NULL,
    CONSTRAINT [FK_ plan_version_audience_daypart_vpvh_plan_version_daypart_customizations]
    FOREIGN KEY([daypart_customization_id])REFERENCES [dbo].[plan_version_daypart_customizations] ([id])
END
GO
/*************************************** END BP-4060 ***************************************/

/*************************************** START BP-4119 ***************************************/
IF EXISTS(SELECT 1 FROM system_settings.INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME='system_component_parameters')
BEGIN
	DELETE FROM system_settings.dbo.system_component_parameters where component_id in ('BroadcastComposerWeb','BroadcastService');
END
GO

/*************************************** END BP-4119 ***************************************/

/*************************************** START BP-3285 ***************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'spot_unique_hash'
        AND OBJECT_ID = OBJECT_ID('staged_recommended_plans'))
BEGIN
	DROP TABLE staged_recommended_plan_details
	DROP TABLE staged_recommended_plans
END

IF OBJECT_ID('staged_recommended_plans') IS NULL
BEGIN
	CREATE TABLE staged_recommended_plans
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_unique_hash_external VARCHAR(255) NOT NULL,
		ambiguity_code INT NOT NULL,
		estimate_id INT NOT NULL,
		inventory_source VARCHAR(100) NOT NULL,
		house_isci VARCHAR(100) NOT NULL,
		client_isci VARCHAR(100) NOT NULL,
		client_spot_length INT NOT NULL,
		broadcast_aired_date DATETIME2 NOT NULL,
		aired_time INT NOT NULL,
		station_legacy_call_letters VARCHAR(30) NULL,
		affiliate VARCHAR(30) NULL,
		market_code INT NULL,
		market_rank INT NULL,		
		[program_name] VARCHAR(500) NOT NULL,
		program_genre VARCHAR(127) NOT NULL,
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL
	)

	CREATE TABLE staged_recommended_plan_details
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		staged_recommended_plan_id INT NOT NULL, -- FK
		recommended_plan_id INT NOT NULL,
		execution_trace_id BIGINT NOT NULL,
		rate MONEY NULL,
		audience_name VARCHAR(127) NULL,
		impressions FLOAT NULL,
		is_recommended_plan BIT NOT NULL,
		plan_clearance_percentage FLOAT NULL,	
		daypart_code VARCHAR(10) NULL,
		start_time INT NULL,
		end_time INT NULL,
		monday INT NULL,
		tuesday INT NULL,
		wednesday INT NULL,
		thursday INT NULL,
		friday INT NULL,
		saturday INT NULL,
		sunday INT NULL,
	)

	ALTER TABLE staged_recommended_plan_details
		ADD CONSTRAINT FK_staged_recommended_plan_details_staged_recommended_plans
		FOREIGN KEY (staged_recommended_plan_id) REFERENCES staged_recommended_plans(id)
END

GO
/*************************************** END BP-3285 ***************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '22.01.2' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '21.02.2' -- Previous release version
		OR [version] = '22.01.2') -- Current release version
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

