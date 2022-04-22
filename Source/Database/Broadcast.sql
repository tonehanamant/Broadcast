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

IF EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'category'
        AND OBJECT_ID = OBJECT_ID('plan_version_pricing_parameters'))
BEGIN
		ALTER TABLE plan_version_pricing_parameters
		DROP COLUMN category
		ALTER TABLE plan_version_pricing_parameters
		DROP COLUMN fluidity_child_category
END

IF EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'category'
        AND OBJECT_ID = OBJECT_ID('plan_versions'))
BEGIN
		ALTER TABLE plan_versions
		DROP COLUMN category
		ALTER TABLE plan_versions
		ADD fluidity_category int null
END

GO
/*************************************** END BP-4419 ***************************************/

/*************************************** START BP-4406 ***************************************/
BEGIN TRANSACTION  
update station_inventory_manifest_dayparts set primary_program_id=null where primary_program_id
in (select id from station_inventory_manifest_daypart_programs where [name]='unattached')

Delete from station_inventory_manifest_daypart_programs where [name]='unattached'

COMMIT TRANSACTION 

BEGIN
Delete from program_name_mappings where official_program_name='unattached'
END
GO
/*************************************** END BP-4406 ***************************************/

/*************************************** START BP-4491 ***************************************/
IF OBJECT_ID('fluidity_categories') IS NULL
BEGIN
	CREATE TABLE [dbo].fluidity_categories
	(
		id INT IDENTITY(1,1) PRIMARY KEY,
		code VARCHAR(50) NULL,
		category VARCHAR(50) NOT NULL,
		parent_category_id INT NULL
	)
	ALTER TABLE plan_versions
		ADD CONSTRAINT FK_fluidity_categories_plan_versions
		FOREIGN KEY (fluidity_category) REFERENCES fluidity_categories(id)
END

GO

IF NOT Exists(select * from fluidity_categories where id =1)
	BEGIN 
		SET IDENTITY_INSERT fluidity_categories ON

		INSERT INTO fluidity_categories (id,code,category,parent_category_id) 
		VALUES		(1,'all','all', NULL),
					(2,'IAB1','Arts & Entertainment', NULL),
					(3,'IAB2','Automotive', NULL),
					(4,'IAB3','Business', NULL),
					(5,'IAB4','Careers', NULL),
					(6,'IAB5','Education', NULL),
					(7,'IAB6','Family & Parenting', NULL),
					(8,'IAB7','Health & Fitness', NULL),
					(9,'IAB8','Food & Drink', NULL),
					(10,'IAB9','Hobbies & Interests', NULL),
					(11,'IAB10','Home & Garden', NULL),
					(12,'IAB11','Law Gov\t & Politics', NULL),
					(13,'IAB12','News', NULL),
					(14,'IAB13','Personal Finance', NULL),
					(15,'IAB14','Society', NULL),
					(16,'IAB15','Science', NULL),
					(17,'IAB16','Pets', NULL),
					(18,'IAB17','Sports', NULL),
					(19,'IAB18','Style & Fashion', NULL),
					(20,'IAB19','Technology & Computing', NULL),
					(21,'IAB20','Travel', NULL),
					(22,'IAB21','Real Estate', NULL),
					(23,'IAB22','Shopping', NULL),
					(24,'IAB23','Religion & Spirituality', NULL),
					(25,'IAB24','Uncategorized', NULL),
					(26,'IAB25','Non-Standard Content', NULL),
					(27,'IAB26','Illegal Content', NULL),
					(33,'IAB1-1','Books & Literature',2),
					(34,'IAB1-2','Celebrity Fan/Gossip',2),
					(35,'IAB1-3','Fine Art',2),
					(36,'IAB1-4','Humor',2),
					(37,'IAB1-5','Movies',2),
					(38,'IAB1-6','Music',2),
					(39,'IAB1-7','Television',2),
					(40,'IAB2-1','Auto Parts',3),
					(41,'IAB2-2','Auto Repair',3),
					(42,'IAB2-3','Buying/Selling Cars',3),
					(43,'IAB2-4','Car Culture',3),
					(44,'IAB2-5','Certified Pre-Owned',3),
					(45,'IAB2-6','Convertible',3),
					(46,'IAB2-7','Coupe',3),
					(47,'IAB2-8','Crossover',3),
					(48,'IAB2-9','Diesel',3),
					(49,'IAB2-10','Electric Vehicle',3),
					(50,'IAB2-11','Hatchback',3),
					(51,'IAB2-12','Hybrid',3),
					(52,'IAB2-13','Luxury',3),
					(53,'IAB2-14','MiniVan',3),
					(54,'IAB2-15','Motorcycles',3),
					(55,'IAB2-16','Off-Road Vehicles',3),
					(56,'IAB2-17','Performance Vehicles',3),
					(57,'IAB2-18','Pickup',3),
					(58,'IAB2-19','Road-Side Assistance',3),
					(59,'IAB2-20','Sedan',3),
					(60,'IAB2-21','Trucks & Accessories',3),
					(61,'IAB2-22','Vintage Cars',3),
					(62,'IAB2-23','Wagon',3),
					(71,'IAB3-1','Advertising',4),
					(72,'IAB3-2','Agriculture',4),
					(73,'IAB3-3','Biotech/Biomedical',4),
					(74,'IAB3-4','Business Software',4),
					(75,'IAB3-5','Construction',4),
					(76,'IAB3-6','Forestry',4),
					(77,'IAB3-7','Government',4),
					(78,'IAB3-8','Green Solutions',4),
					(79,'IAB3-9','Human Resources',4),
					(80,'IAB3-10','Logistics',4),
					(81,'IAB3-11','Marketing',4),
					(82,'IAB3-12','Metals',4),
					(86,'IAB4-1','Career Planning',5),
					(87,'IAB4-2','College',5),
					(88,'IAB4-3','Financial Aid',5),
					(89,'IAB4-4','Job Fairs',5),
					(90,'IAB4-5','Job Search',5),
					(91,'IAB4-6','Resume Writing/Advice',5),
					(92,'IAB4-7','Nursing',5),
					(93,'IAB4-8','Scholarships',5),
					(94,'IAB4-9','Telecommuting',5),
					(95,'IAB4-10','U.S. Military',5),
					(96,'IAB4-11','Career Advice',5),
					(101,'IAB5-1','7-12 Education',6),
					(102,'IAB5-2','Adult Education',6),
					(103,'IAB5-3','Art History',6),
					(104,'IAB5-4','College Administration',6),
					(105,'IAB5-5','College Life',6),
					(106,'IAB5-6','Distance Learning',6),
					(107,'IAB5-7','English as a 2nd Language',6),
					(108,'IAB5-8','Language Learning',6),
					(109,'IAB5-9','Graduate School',6),
					(110,'IAB5-10','Homeschooling',6),
					(111,'IAB5-11','Homework/Study Tips',6),
					(112,'IAB5-12','K-6 Educators',6),
					(113,'IAB5-13','Private School',6),
					(114,'IAB5-14','Special Education',6),
					(115,'IAB5-15','Studying Business',6),
					(116,'IAB6-1','Adoption',7),
					(117,'IAB6-2','Babies & Toddlers',7),
					(118,'IAB6-3','Daycare/Pre School',7),
					(119,'IAB6-4','Family Internet',7),
					(120,'IAB6-5','Parenting - K-6 Kids',7),
					(121,'IAB6-6','Parenting teens',7),
					(122,'IAB6-7','Pregnancy',7),
					(123,'IAB6-8','Special Needs Kids',7),
					(124,'IAB6-9','Eldercare',7),
					(131,'IAB7-1','Exercise',8),
					(132,'IAB7-2','A.D.D.',8),
					(133,'IAB7-3','AIDS/HIV',8),
					(134,'IAB7-4','Allergies',8),
					(135,'IAB7-5','Alternative Medicine',8),
					(136,'IAB7-6','Arthritis',8),
					(137,'IAB7-7','Asthma',8),
					(138,'IAB7-8','Autism/PDD',8),
					(139,'IAB7-9','Bipolar Disorder',8),
					(140,'IAB7-10','Brain Tumor',8),
					(141,'IAB7-11','Cancer',8),
					(142,'IAB7-12','Cholesterol',8),
					(143,'IAB7-13','Chronic Fatigue Syndrome',8),
					(144,'IAB7-14','Chronic Pain',8),
					(145,'IAB7-15','Cold & Flu',8),
					(146,'IAB7-16','Deafness',8),
					(147,'IAB7-17','Dental Care',8),
					(148,'IAB7-18','Depression',8),
					(149,'IAB7-19','Dermatology',8),
					(150,'IAB7-20','Diabetes',8),
					(151,'IAB7-21','Epilepsy',8),
					(152,'IAB7-22','GERD/Acid Reflux',8),
					(153,'IAB7-23','Headaches/Migraines',8),
					(154,'IAB7-24','Heart Disease',8),
					(155,'IAB7-25','Herbs for Health',8),
					(156,'IAB7-26','Holistic Healing',8),
					(157,'IAB7-27','IBS/Crohn\s Disease',8),
					(158,'IAB7-28','Incest/Abuse Support',8),
					(159,'IAB7-29','Incontinence',8),
					(160,'IAB7-30','Infertility',8),
					(161,'IAB7-31','Men\s Health',8),
					(162,'IAB7-32','Nutrition',8),
					(163,'IAB7-33','Orthopedics',8),
					(164,'IAB7-34','Panic/Anxiety Disorders',8),
					(165,'IAB7-35','Pediatrics',8),
					(166,'IAB7-36','Physical Therapy',8),
					(167,'IAB7-37','Psychology/Psychiatry',8),
					(168,'IAB7-38','Senor Health',8),
					(169,'IAB7-39','Sexuality',8),
					(170,'IAB7-40','Sleep Disorders',8),
					(171,'IAB7-41','Smoking Cessation',8),
					(172,'IAB7-42','Substance Abuse',8),
					(173,'IAB7-43','Thyroid Disease',8),
					(174,'IAB7-44','Weight Loss',8),
					(175,'IAB7-45','Women\s Health',8),
					(194,'IAB8-1','American Cuisine',9),
					(195,'IAB8-2','Barbecues & Grilling',9),
					(196,'IAB8-3','Cajun/Creole',9),
					(197,'IAB8-4','Chinese Cuisine',9),
					(198,'IAB8-5','Cocktails/Beer',9),
					(199,'IAB8-6','Coffee/Tea',9),
					(200,'IAB8-7','Cuisine-Specific',9),
					(201,'IAB8-8','Desserts & Baking',9),
					(202,'IAB8-9','Dining Out',9),
					(203,'IAB8-10','Food Allergies',9),
					(204,'IAB8-11','French Cuisine',9),
					(205,'IAB8-12','Health/Lowfat Cooking',9),
					(206,'IAB8-13','Italian Cuisine',9),
					(207,'IAB8-14','Japanese Cuisine',9),
					(208,'IAB8-15','Mexican Cuisine',9),
					(209,'IAB8-16','Vegan',9),
					(210,'IAB8-17','Vegetarian',9),
					(211,'IAB8-18','Wine',9),
					(225,'IAB9-1','Art/Technology',10),
					(226,'IAB9-2','Arts & Crafts',10),
					(227,'IAB9-3','Beadwork',10),
					(228,'IAB9-4','Birdwatching',10),
					(229,'IAB9-5','Board Games/Puzzles',10),
					(230,'IAB9-6','Candle & Soap Making',10),
					(231,'IAB9-7','Card Games',10),
					(232,'IAB9-8','Chess',10),
					(233,'IAB9-9','Cigars',10),
					(234,'IAB9-10','Collecting',10),
					(235,'IAB9-11','Comic Books',10),
					(236,'IAB9-12','Drawing/Sketching',10),
					(237,'IAB9-13','Freelance Writing',10),
					(238,'IAB9-14','Genealogy',10),
					(239,'IAB9-15','Getting Published',10),
					(240,'IAB9-16','Guitar',10),
					(241,'IAB9-17','Home Recording',10),
					(242,'IAB9-18','Investors & Patents',10),
					(243,'IAB9-19','Jewelry Making',10),
					(244,'IAB9-20','Magic & Illusion',10),
					(245,'IAB9-21','Needlework',10),
					(246,'IAB9-22','Painting',10),
					(247,'IAB9-23','Photography',10),
					(248,'IAB9-24','Radio',10),
					(249,'IAB9-25','Roleplaying Games',10),
					(250,'IAB9-26','Sci-Fi & Fantasy',10),
					(251,'IAB9-27','Scrapbooking',10),
					(252,'IAB9-28','Screenwriting',10),
					(253,'IAB9-29','Stamps & Coins',10),
					(254,'IAB9-30','Video & Computer Games',10),
					(255,'IAB9-31','Woodworking',10),
					(256,'IAB10-1','Appliances',11),
					(257,'IAB10-2','Entertaining',11),
					(258,'IAB10-3','Environmental Safety',11),
					(259,'IAB10-4','Gardening',11),
					(260,'IAB10-5','Home Repair',11),
					(261,'IAB10-6','Home Theater',11),
					(262,'IAB10-7','Interior Decorating',11),
					(263,'IAB10-8','Landscaping',11),
					(264,'IAB10-9','Remodeling & Construction',11),
					(271,'IAB11-1','Immigration',12),
					(272,'IAB11-2','Legal Issues',12),
					(273,'IAB11-3','U.S. Government Resources',12),
					(274,'IAB11-4','Politics',12),
					(275,'IAB11-5','Commentary',12),
					(278,'IAB12-1','International News',13),
					(279,'IAB12-2','National News',13),
					(280,'IAB12-3','Local News',13),
					(281,'IAB13-1','Beginning Investing',14),
					(282,'IAB13-2','Credit/Debt & Loans',14),
					(283,'IAB13-3','Financial News',14),
					(284,'IAB13-4','Financial Planning',14),
					(285,'IAB13-5','Hedge Fund',14),
					(286,'IAB13-6','Insurance',14),
					(287,'IAB13-7','Investing',14),
					(288,'IAB13-8','Mutual Funds',14),
					(289,'IAB13-9','Options',14),
					(290,'IAB13-10','Retirement Planning',14),
					(291,'IAB13-11','Stocks',14),
					(292,'IAB13-12','Tax Planning',14),
					(296,'IAB14-1','Dating',15),
					(297,'IAB14-2','Divorce Support',15),
					(298,'IAB14-3','Gay Life',15),
					(299,'IAB14-4','Marriage',15),
					(300,'IAB14-5','Senior Living',15),
					(301,'IAB14-6','Teens',15),
					(302,'IAB14-7','Weddings',15),
					(303,'IAB14-8','Ethnic Specific',15),
					(311,'IAB15-1','Astrology',16),
					(312,'IAB15-2','Biology',16),
					(313,'IAB15-3','Chemistry',16),
					(314,'IAB15-4','Geology',16),
					(315,'IAB15-5','Paranormal Phenomena',16),
					(316,'IAB15-6','Physics',16),
					(317,'IAB15-7','Space/Astronomy',16),
					(318,'IAB15-8','Geography',16),
					(319,'IAB15-9','Botany',16),
					(320,'IAB15-10','Weather',16),
					(326,'IAB16-1','Aquariums',17),
					(327,'IAB16-2','Birds',17),
					(328,'IAB16-3','Cats',17),
					(329,'IAB16-4','Dogs',17),
					(330,'IAB16-5','Large Animals',17),
					(331,'IAB16-6','Reptiles',17),
					(332,'IAB16-7','Veterinary Medicine',17),
					(333,'IAB17-1','Auto Racing',18),
					(334,'IAB17-2','Baseball',18),
					(335,'IAB17-3','Bicycling',18),
					(336,'IAB17-4','Bodybuilding',18),
					(337,'IAB17-5','Boxing',18),
					(338,'IAB17-6','Canoeing/Kayaking',18),
					(339,'IAB17-7','Cheerleading',18),
					(340,'IAB17-8','Climbing',18),
					(341,'IAB17-9','Cricket',18),
					(342,'IAB17-10','Figure Skating',18),
					(343,'IAB17-11','Fly Fishing',18),
					(344,'IAB17-12','Football',18),
					(345,'IAB17-13','Freshwater Fishing',18),
					(346,'IAB17-14','Game & Fish',18),
					(347,'IAB17-15','Golf',18),
					(348,'IAB17-16','Horse Racing',18),
					(349,'IAB17-17','Horses',18),
					(350,'IAB17-18','Hunting/Shooting',18),
					(351,'IAB17-19','Inline Skating',18),
					(352,'IAB17-20','Martial Arts',18),
					(353,'IAB17-21','Mountain Biking',18),
					(354,'IAB17-22','NASCAR Racing',18),
					(355,'IAB17-23','Olympics',18),
					(356,'IAB17-24','Paintball',18),
					(357,'IAB17-25','Power & Motorcycles',18),
					(358,'IAB17-26','Pro Basketball',18),
					(359,'IAB17-27','Pro Ice Hockey',18),
					(360,'IAB17-28','Rodeo',18),
					(361,'IAB17-29','Rugby',18),
					(362,'IAB17-30','Running/Jogging',18),
					(363,'IAB17-31','Sailing',18),
					(364,'IAB17-32','Saltwater Fishing',18),
					(365,'IAB17-33','Scuba Diving',18),
					(366,'IAB17-34','Skateboarding',18),
					(367,'IAB17-35','Skiing',18),
					(368,'IAB17-36','Snowboarding',18),
					(369,'IAB17-37','Surfing/Bodyboarding',18),
					(370,'IAB17-38','Swimming',18),
					(371,'IAB17-39','Table Tennis/Ping-Pong',18),
					(372,'IAB17-40','Tennis',18),
					(373,'IAB17-41','Volleyball',18),
					(374,'IAB17-42','Walking',18),
					(375,'IAB17-43','Waterski/Wakeboard',18),
					(376,'IAB17-44','World Soccer',18),
					(396,'IAB18-1','Beauty',19),
					(397,'IAB18-2','Body Art',19),
					(398,'IAB18-3','Fashion',19),
					(399,'IAB18-4','Jewelry',19),
					(400,'IAB18-5','Clothing',19),
					(401,'IAB18-6','Accessories',19),
					(403,'IAB19-1','3-D Graphics',20),
					(404,'IAB19-2','Animation',20),
					(405,'IAB19-3','Antivirus Software',20),
					(406,'IAB19-4','C/C++',20),
					(407,'IAB19-5','Cameras & Camcorders',20),
					(408,'IAB19-6','Cell Phones',20),
					(409,'IAB19-7','Computer Certification',20),
					(410,'IAB19-8','Computer Networking',20),
					(411,'IAB19-9','Computer Peripherals',20),
					(412,'IAB19-10','Computer Reviews',20),
					(413,'IAB19-11','Data Centers',20),
					(414,'IAB19-12','Databases',20),
					(415,'IAB19-13','Desktop Publishing',20),
					(416,'IAB19-14','Desktop Video',20),
					(417,'IAB19-15','Email',20),
					(418,'IAB19-16','Graphics Software',20),
					(419,'IAB19-17','Home Video/DVD',20),
					(420,'IAB19-18','Internet Technology',20),
					(421,'IAB19-19','Java',20),
					(422,'IAB19-20','JavaScript',20),
					(423,'IAB19-21','Mac Support',20),
					(424,'IAB19-22','MP3/MIDI',20),
					(425,'IAB19-23','Net Conferencing',20),
					(426,'IAB19-24','Net for Beginners',20),
					(427,'IAB19-25','Network Security',20),
					(428,'IAB19-26','Palmtops/PDAs',20),
					(429,'IAB19-27','PC Support',20),
					(430,'IAB19-28','Portable',20),
					(431,'IAB19-29','Entertainment',20),
					(432,'IAB19-30','Shareware/Freeware',20),
					(433,'IAB19-31','Unix',20),
					(434,'IAB19-32','Visual Basic',20),
					(435,'IAB19-33','Web Clip Art',20),
					(436,'IAB19-34','Web Design/HTML',20),
					(437,'IAB19-35','Web Search',20),
					(438,'IAB19-36','Windows',20),
					(466,'IAB20-1','Adventure Travel',21),
					(467,'IAB20-2','Africa',21),
					(468,'IAB20-3','Air Travel',21),
					(469,'IAB20-4','Australia & New Zealand',21),
					(470,'IAB20-5','Bed & Breakfasts',21),
					(471,'IAB20-6','Budget Travel',21),
					(472,'IAB20-7','Business Travel',21),
					(473,'IAB20-8','By US Locale',21),
					(474,'IAB20-9','Camping',21),
					(475,'IAB20-10','Canada',21),
					(476,'IAB20-11','Caribbean',21),
					(477,'IAB20-12','Cruises',21),
					(478,'IAB20-13','Eastern Europe',21),
					(479,'IAB20-14','Europe',21),
					(480,'IAB20-15','France',21),
					(481,'IAB20-16','Greece',21),
					(482,'IAB20-17','Honeymoons/Getaways',21),
					(483,'IAB20-18','Hotels',21),
					(484,'IAB20-19','Italy',21),
					(485,'IAB20-20','Japan',21),
					(486,'IAB20-21','Mexico & Central America',21),
					(487,'IAB20-22','National Parks',21),
					(488,'IAB20-23','South America',21),
					(489,'IAB20-24','Spas',21),
					(490,'IAB20-25','Theme Parks',21),
					(491,'IAB20-26','Traveling with Kids',21),
					(492,'IAB20-27','United Kingdom',21),
					(497,'IAB21-1','Apartments',22),
					(498,'IAB21-2','Architects',22),
					(499,'IAB21-3','Buying/Selling Homes',22),
					(500,'IAB22-1','Contests & Freebies',23),
					(501,'IAB22-2','Couponing',23),
					(502,'IAB22-3','Comparison',23),
					(503,'IAB22-4','Engines',23),
					(507,'IAB23-1','Alternative Religions',24),
					(508,'IAB23-2','Atheism/Agnosticism',24),
					(509,'IAB23-3','Buddhism',24),
					(510,'IAB23-4','Catholicism',24),
					(511,'IAB23-5','Christianity',24),
					(512,'IAB23-6','Hinduism',24),
					(513,'IAB23-7','Islam',24),
					(514,'IAB23-8','Judaism',24),
					(515,'IAB23-9','Latter-Day Saints',24),
					(516,'IAB23-10','Pagan/Wiccan',24),
					(522,'IAB25-1','Unmoderated UGC',26),
					(523,'IAB25-2','Extreme Graphic/Explicit Violence',26),
					(524,'IAB25-3','Pornography',26),
					(525,'IAB25-4','Profane Content',26),
					(526,'IAB25-5','Hate Content',26),
					(527,'IAB25-6','Under Construction',26),
					(528,'IAB25-7','Incentivized',26),
					(529,'IAB26-1','Illegal Content',27),
					(530,'IAB26-2','Warez',27),
					(531,'IAB26-3','Spyware/Malware',27),
					(532,'IAB26-4','Copyright Infringement',27)

		SET IDENTITY_INSERT fluidity_categories OFF

	END

GO


/*************************************** END BP-4491 ***************************************/

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

