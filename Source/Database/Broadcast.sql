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

IF NOT EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'spot_unique_hash'
        AND OBJECT_ID = OBJECT_ID('staged_out_of_specs'))
BEGIN
	DROP TABLE staged_out_of_specs	
END

IF OBJECT_ID('staged_out_of_specs') IS NULL
BEGIN
	CREATE TABLE staged_out_of_specs
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_unique_hash_external VARCHAR(255) NOT NULL,
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

IF NOT EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'client_spot_length_id'
        AND OBJECT_ID = OBJECT_ID('staged_unposted_no_plan'))
BEGIN
	DROP TABLE staged_unposted_no_plan	
	DROP TABLE spot_exceptions_unposted_no_plan	
	DROP TABLE spot_exceptions_unposted_no_reel_roster	
END

IF OBJECT_ID('staged_unposted_no_plan') IS NULL
BEGIN
	CREATE TABLE staged_unposted_no_plan
	(
		id INT IDENTITY(1,1) PRIMARY KEY,
		house_isci VARCHAR(50) NOT NULL,
		client_isci VARCHAR(50) NOT NULL,
		client_spot_length INT NOT NULL,
		spot_count INT NOT NULL,	
		program_air_time DATETIME2 NOT NULL,	
		estimate_id BIGINT NOT NULL,	
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL
	)
END

IF OBJECT_ID('spot_exceptions_unposted_no_plan') IS NULL
BEGIN
	CREATE TABLE [dbo].spot_exceptions_unposted_no_plan
    (
		id INT IDENTITY(1,1) PRIMARY KEY,
		house_isci VARCHAR(50) NOT NULL,
		client_isci VARCHAR(50) NOT NULL,
		client_spot_length_id INT NOT NULL,
		[count] INT NOT NULL,	
		program_air_time DATETIME NOT NULL,	
		estimate_id BIGINT NOT NULL,	
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL,
		created_by VARCHAR(100) NOT NULL,
		created_at DATETIME NOT NULL,
		modified_by VARCHAR(100) NOT NULL,
		modified_at DATETIME NOT NULL
    )
END

IF OBJECT_ID('spot_exceptions_unposted_no_reel_roster') IS NULL
BEGIN
    CREATE TABLE [dbo].spot_exceptions_unposted_no_reel_roster
     (
		id INT IDENTITY(1,1) PRIMARY KEY,
		house_isci VARCHAR(50) NOT NULL,
		[count] INT NOT NULL,	
		program_air_time DATETIME NOT NULL,	
		estimate_id BIGINT NOT NULL,
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL,
		created_by VARCHAR(100) NOT NULL,
		created_at DATETIME NOT NULL,
		modified_by VARCHAR(100) NOT NULL,
		modified_at DATETIME NOT NULL
    )
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'staged_recommended_plans' 
	AND COLUMN_NAME= 'program_genre' 
	AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE staged_recommended_plans
		ALTER COLUMN program_genre VARCHAR(127) NULL
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'staged_out_of_specs' 
	AND COLUMN_NAME= 'program_genre' 
	AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE staged_out_of_specs
		ALTER COLUMN program_genre VARCHAR(127) NULL
END

GO
/*************************************** END BP-3285 ***************************************/

/*************************************** START BP-3774 ***************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND (COLUMN_NAME= 'market_code' OR COLUMN_NAME='market_rank'))
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs
	ADD market_code int NULL,
	market_rank int NULL
END

GO
/*************************************** END BP-3774 ***************************************/

/*************************************** START BP-4198 ***************************************/

IF OBJECT_ID('plan_version_buying_band_inventory_stations') IS NULL
BEGIN
	CREATE TABLE plan_version_buying_band_inventory_stations
	(
		id INT IDENTITY(1,1) NOT NULL,
		plan_version_buying_job_id INT NOT NULL,
		posting_type_id INT NOT NULL,  
		station_id INT NOT NULL,
		impressions FLOAT NOT NULL,
		cost MONEY NOT NULL,
		manifest_weeks_count INT NOT NULL,
		CONSTRAINT [PK_plan_version_buying_band_inventory_stations] PRIMARY KEY CLUSTERED ([id] ASC),
		CONSTRAINT [FK_plan_version_buying_band_inventory_stations_plan_version_buying_job_id] FOREIGN KEY (plan_version_buying_job_id) REFERENCES plan_version_buying_job(id) ON DELETE CASCADE,
		CONSTRAINT [FK_plan_version_buying_band_inventory_stations_stations_id] FOREIGN KEY (station_id) REFERENCES stations(id) ON DELETE CASCADE
	)

	CREATE TABLE plan_version_buying_band_inventory_station_dayparts
	(
		id INT IDENTITY(1,1) NOT NULL,
		plan_version_buying_band_inventory_station_id INT NOT NULL,
		active_days INT NOT NULL,
		[hours] INT NOT NULL,
		CONSTRAINT [PK_plan_version_buying_band_inventory_station_dayparts] PRIMARY KEY CLUSTERED ([id] ASC),
		CONSTRAINT [FK_plan_version_buying_band_inventory_station_dayparts_plan_version_buying_band_inventory_station_id] FOREIGN KEY (plan_version_buying_band_inventory_station_id) REFERENCES plan_version_buying_band_inventory_stations(id) ON DELETE CASCADE
	)
END

GO

/*************************************** END BP-4198 ***************************************/

/*************************************** START BP-4084 ***************************************/

-- undelete the mappings previuosly deleted by the feed
UPDATE plan_iscis SET 
	deleted_at = NULL, 
	deleted_by = NULL 
WHERE deleted_by='RecurringJobsUser'

GO

/*************************************** END BP-4084 ***************************************/

/*************************************** START BP-3285 - Part 2 ***************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'created_by'
        AND OBJECT_ID = OBJECT_ID('spot_exceptions_out_of_specs'))
BEGIN
	DROP TABLE spot_exceptions_out_of_spec_decisions
	DROP TABLE spot_exceptions_out_of_specs
END

IF OBJECT_ID('spot_exceptions_out_of_specs') IS NULL
BEGIN 
	CREATE TABLE [dbo].[spot_exceptions_out_of_specs]
	(
		[id] INT IDENTITY(1,1) PRIMARY KEY,	 
		[spot_unique_hash_external] [varchar](255) NOT NULL,
		[execution_id_external] VARCHAR(100) NOT NULL,
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
		[program_network] VARCHAR(10),
		[program_air_time] DATETIME NOT NULL,	
		[advertiser_name] NVARCHAR(100) NULL,
		[reason_code_id] INT NOT NULL,	
		[impressions] FLOAT NOT NULL, 
		[market_code] INT NULL, 
		[market_rank] INT NULL, 
		[program_genre_id] [int] NULL,
		[house_isci] [varchar](100) NULL,
		[ingested_by] VARCHAR(100) NOT NULL, 
		[ingested_at] DATETIME NOT NULL, 
		[created_by] VARCHAR(100) NOT NULL,
		[created_at] DATETIME NOT NULL,
		[modified_by] VARCHAR(100) NOT NULL,
		[modified_at] DATETIME NOT NULL
		CONSTRAINT [FK_spot_exceptions_out_of_specs_plans] FOREIGN KEY ([recommended_plan_id]) REFERENCES [dbo].[plans]([ID]),
		CONSTRAINT [FK_spot_exceptions_out_of_specs_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths]([ID]),
		CONSTRAINT [FK_spot_exceptions_out_of_specs_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences]([ID]),
		CONSTRAINT [FK_spot_exceptions_out_of_specs_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts]([ID]),
		CONSTRAINT [FK_spot_exceptions_out_of_specs_program_genre] FOREIGN KEY ([program_genre_id]) REFERENCES [dbo].[genres]([ID]),
		CONSTRAINT [FK_spot_exceptions_out_of_specs_spot_exceptions_out_of_spec_reason_codes] FOREIGN KEY (reason_code_id) REFERENCES spot_exceptions_out_of_spec_reason_codes(id)
	)

	CREATE TABLE [dbo].[spot_exceptions_out_of_spec_decisions]
	(
		id INT IDENTITY(1,1) PRIMARY KEY,
		spot_exceptions_out_of_spec_id INT NOT NULL,
		accepted_as_in_spec BIT NOT NULL,
		decision_notes NVARCHAR(1024) NULL,
		username VARCHAR(63) NOT NULL,
		created_at DATETIME NOT NULL,
		[synced_by] VARCHAR(100) NULL, 
		[synced_at] DATETIME2 NULL, 
		CONSTRAINT [FK_spot_exceptions_out_of_spec_decisions_spot_exceptions_out_of_specs] FOREIGN KEY ([spot_exceptions_out_of_spec_id]) REFERENCES [dbo].[spot_exceptions_out_of_specs]([ID])
	)
END

GO
/*************************************** END BP-3285 - Part 2 ***************************************/

/*************************************** START BP-3285 - Part 3 *************************************/

IF EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'flight_start_date'
        AND OBJECT_ID = OBJECT_ID('spot_exceptions_out_of_specs'))
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs
		DROP COLUMN flight_start_date

	ALTER TABLE spot_exceptions_out_of_specs
		DROP COLUMN flight_end_date

	ALTER TABLE spot_exceptions_out_of_specs
		DROP COLUMN advertiser_name

	ALTER TABLE spot_exceptions_out_of_specs
		DROP COLUMN product
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'spot_exceptions_unposted_no_plan' 
	AND COLUMN_NAME= 'client_spot_length_id' 
	AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE spot_exceptions_unposted_no_plan
		ALTER COLUMN client_spot_length_id INT NULL
END

GO

/*************************************** END BP-3285 - Part 3 ***************************************/

/*************************************** START BP-4221 ***************************************/

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'flight_start_date' AND UPPER(IS_NULLABLE) = UPPER('YES'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN flight_start_date datetime NOT NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'flight_end_date' AND UPPER(IS_NULLABLE) = UPPER('YES'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN flight_end_date datetime NOT NULL
END

GO

/*************************************** END BP-4221 ***************************************/

/*************************************** Start BP-4264 ***************************************/

IF NOT EXISTS (SELECT 1 FROM spot_exceptions_out_of_spec_reason_codes WHERE reason_code = 12)
BEGIN 
	-- no need to retain the data
	-- the feature is not released yet.
	DELETE FROM spot_exceptions_out_of_spec_decisions
	DELETE FROM spot_exceptions_out_of_specs

	DELETE FROM spot_exceptions_out_of_spec_reason_codes
	DBCC CHECKIDENT ('spot_exceptions_out_of_spec_reason_codes', RESEED, 0);

	INSERT INTO spot_exceptions_out_of_spec_reason_codes (reason_code, reason, [label]) VALUES
		(3,'Hiatus Day','Hiatus')
		,(4,'Incorrect Flight Week','Flight')
		,(5,'Incorrect ISCI','ISCI')
		,(6,'Incorrect ISCI (Day)','Day')
		,(7,'Incorrect Length','Length')
		,(8,'Incorrect Market','Market')
		,(9,'Incorrect Time','Daypart')
		,(10,'Incorrect Genre','Genre')
		,(11,'Incorrect Affiliate','Affiliate')
		,(12,'Incorrect Program','Program')

END

GO
/*************************************** END BP-4264 ***************************************/

/*************************************** START BP-4211 ***************************************/

IF EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'product_name'
        AND OBJECT_ID = OBJECT_ID('reel_isci_products'))
BEGIN
	DROP TABLE reel_isci_products
END

/*************************************** END BP-4211 ***************************************/

/*************************************** START BP-4245 ***************************************/

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'target_audience_id' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN target_audience_id int NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'equivalized' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN equivalized bit NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'is_draft' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN is_draft bit NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'audience_type' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN audience_type int NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'posting_type' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN posting_type int NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'status' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN status int NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'target_vpvh' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN target_vpvh float NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND COLUMN_NAME= 'share_book_id' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_versions
			ALTER COLUMN share_book_id  int NULL
END

GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_flight_days' AND COLUMN_NAME= 'day_id' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_version_flight_days
			ALTER COLUMN day_id int NULL
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_summaries' AND COLUMN_NAME= 'processing_status' AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE plan_version_summaries
			ALTER COLUMN processing_status int NULL
END

/*************************************** END BP-4245 ***************************************/

/*************************************** START BP-4198 *************************************/

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_NAME = 'plan_version_buying_band_station_dayparts')

BEGIN
	DROP TABLE plan_version_buying_band_station_dayparts;
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_NAME = 'plan_version_buying_band_stations')

BEGIN
	DROP TABLE plan_version_buying_band_stations;
END
GO

/*************************************** END BP-4198 ****************************************/

/*************************************** START BP-4199 ***************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_buying_job' AND COLUMN_NAME= 'inventory_raw_file')
BEGIN
	ALTER TABLE plan_version_buying_job
			ADD inventory_raw_file varchar(2048) NULL
END

GO

/*************************************** END BP-4199 ***************************************/

/*************************************** START BP-4198 ***************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_buying_band_inventory_stations' AND COLUMN_NAME= 'posting_type_conversion_rate')
BEGIN
	ALTER TABLE plan_version_buying_band_inventory_stations
			ADD posting_type_conversion_rate float NULL
END

GO

/*************************************** END BP-4198 ***************************************/

/*************************************** START BP-4198 ***************************************/

DECLARE @Sql_SetUpColumns VARCHAR(MAX) = '
ALTER TABLE plan_version_buying_band_inventory_stations ALTER COLUMN posting_type_conversion_rate float NULL
'

DECLARE @Sql_PopulateColumns VARCHAR(MAX) = '
UPDATE plan_version_buying_band_inventory_stations
SET posting_type_conversion_rate = 
	(SELECT 
		MAX(conv.conversion_rate) AS conversion_rate
		FROM plan_version_buying_band_inventory_stations pvs
	LEFT JOIN plan_version_buying_job pvbj
	ON pvs.plan_version_buying_job_id = pvbj.id
	LEFT JOIN plan_version_dayparts pvd
	ON pvbj.plan_version_id = pvd.plan_version_id
	LEFT JOIN nti_to_nsi_conversion_rates conv
	ON pvd.standard_daypart_id = conv.standard_daypart_id)
'

DECLARE @Sql_FinalizeColumns VARCHAR(MAX) = '
ALTER TABLE plan_version_buying_band_inventory_stations ALTER COLUMN posting_type_conversion_rate float NOT NULL
'

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'posting_type_conversion_rate' AND OBJECT_ID = OBJECT_ID(N'plan_version_buying_band_inventory_stations'))
BEGIN
	EXEC (@Sql_SetUpColumns)
	EXEC (@Sql_PopulateColumns)
	EXEC (@Sql_FinalizeColumns)
END

GO

/*************************************** END BP-4198 ***************************************/

/*************************************** START BP-4198 ***************************************/

DECLARE @Sql_SetUpColumns VARCHAR(MAX) = '
ALTER TABLE plan_version_buying_band_inventory_stations ALTER COLUMN posting_type_conversion_rate float NULL
'

DECLARE @Sql_PopulateColumns VARCHAR(MAX) = '
UPDATE plan_version_buying_band_inventory_stations
SET posting_type_conversion_rate = 
	(SELECT 
		MAX(conv.conversion_rate) AS conversion_rate
		FROM plan_version_buying_band_inventory_stations pvs
	LEFT JOIN plan_version_buying_job pvbj
	ON pvs.plan_version_buying_job_id = pvbj.id
	LEFT JOIN plan_version_dayparts pvd
	ON pvbj.plan_version_id = pvd.plan_version_id
	LEFT JOIN nti_to_nsi_conversion_rates conv
	ON pvd.standard_daypart_id = conv.standard_daypart_id)
'

DECLARE @Sql_FinalizeColumns VARCHAR(MAX) = '
ALTER TABLE plan_version_buying_band_inventory_stations ALTER COLUMN posting_type_conversion_rate float NOT NULL
'

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = N'posting_type_conversion_rate' AND OBJECT_ID = OBJECT_ID(N'plan_version_buying_band_inventory_stations'))
BEGIN
	EXEC (@Sql_SetUpColumns)
	EXEC (@Sql_PopulateColumns)
	EXEC (@Sql_FinalizeColumns)
END

GO

/*************************************** END BP-4198 ***************************************/

/*************************************** START BP-4418 ***************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_versions' AND (COLUMN_NAME= 'fluidity_percentage' OR COLUMN_NAME='category' OR COLUMN_NAME='fluidity_child_category'))
BEGIN
	ALTER TABLE plan_versions
	ADD fluidity_percentage float NULL,
	category int NULL,
	fluidity_child_category int NULL
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_pricing_parameters' AND (COLUMN_NAME= 'fluidity_percentage' OR COLUMN_NAME='category' OR COLUMN_NAME='fluidity_child_category'))
BEGIN
	ALTER TABLE plan_version_pricing_parameters
	ADD fluidity_percentage float NULL,
	category int NULL,
	fluidity_child_category int NULL
END

/*************************************** END BP-4418 ***************************************/

/*************************************** START BP-4410 ***************************************/
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_buying_parameters' AND (COLUMN_NAME= 'share_book_id' OR COLUMN_NAME='hut_book_id'))
BEGIN
	ALTER TABLE plan_version_buying_parameters
	ADD share_book_id int NULL,
	hut_book_id int NULL
END

GO
/*************************************** END BP-4410 ***************************************/

/*************************************** START BP-4419 ***************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_version_buying_parameters' AND (COLUMN_NAME= 'fluidity_percentage'))
BEGIN
ALTER TABLE plan_version_buying_parameters
ADD fluidity_percentage float NULL
END

GO
/*************************************** END BP-4419 ***************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '22.02.2' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '22.01.2' -- Previous release version
		OR [version] = '22.02.2') -- Current release version
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

