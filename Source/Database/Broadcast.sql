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

/*************************************** START BP-5123 ***************************************/

IF OBJECT_ID('stations_secondary_affiliations') IS NULL
BEGIN
 CREATE TABLE dbo.stations_secondary_affiliations
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,		
		stations_id INT NOT NULL,
		affiliate_id INT NOT NULL
	)
ALTER TABLE stations_secondary_affiliations ADD CONSTRAINT FK_stations_secondary_affiliations_affiliates FOREIGN KEY(affiliate_id) REFERENCES affiliates(id)
ALTER TABLE stations_secondary_affiliations ADD CONSTRAINT FK_stations_secondary_affiliations_stations FOREIGN KEY(stations_id) REFERENCES stations(id)
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'affiliates' AND COLUMN_NAME= 'name')
BEGIN
	IF NOT EXISTS (select * from affiliates where name = 'MyNet')
		BEGIN
		INSERT INTO affiliates values('MyNet','System',GETDATE(),'System',GETDATE())
		END
END
GO

/*************************************** END BP-5123 ***************************************/

/*************************************** START BP-5414 ***************************************/

if exists(select 1 from audiences where sub_category_code='C')
begin
DECLARE @AUDIENCE TABLE
(ID INT
)
INSERT INTO @AUDIENCE
SELECT id FROM audiences WHERE sub_category_code='C'
update audiences set sub_category_code='K', [name]=REPLACE([name],'Children','Kids') , 
code=REPLACE(code,'C','K') where id in(select ID from @AUDIENCE)
end
GO

/*************************************** END BP-5414 ***************************************/

/*************************************** START BP-5447 ***************************************/

DECLARE @AffiliateId INT

SET @AffiliateId = Isnull((SELECT id
                           FROM   affiliates
                           WHERE  NAME = 'MyNet'), 0)

IF NOT EXISTS (SELECT *
               FROM   stations_secondary_affiliations
               WHERE  stations_id = 1024)
  IF @AffiliateId != 0
    BEGIN
        BEGIN
            DECLARE @SecondaryAffiliations TABLE
              (
                 stationid   INT,
                 affiliateid INT
              )

            INSERT INTO @SecondaryAffiliations
            VALUES      (1024,
                         @AffiliateId),
                        (1449,
                         @AffiliateId),
                        (669,
                         @AffiliateId),
                        (746,
                         @AffiliateId),
                        (2563,
                         @AffiliateId),
                        (567,
                         @AffiliateId),
                        (2071,
                         @AffiliateId),
                        (2015,
                         @AffiliateId),
                        (1329,
                         @AffiliateId),
                        (2110,
                         @AffiliateId),
                        (733,
                         @AffiliateId),
                        (1101,
                         @AffiliateId),
                        (772,
                         @AffiliateId),
                        (2138,
                         @AffiliateId),
                        (1783,
                         @AffiliateId),
                        (1025,
                         @AffiliateId),
                        (1745,
                         @AffiliateId),
                        (1267,
                         @AffiliateId),
                        (962,
                         @AffiliateId),
                        (612,
                         @AffiliateId),
                        (971,
                         @AffiliateId),
                        (997,
                         @AffiliateId),
                        (2558,
                         @AffiliateId),
                        (84,
                         @AffiliateId),
                        (717,
                         @AffiliateId),
                        (1138,
                         @AffiliateId),
                        (4,
                         @AffiliateId),
                        (1047,
                         @AffiliateId),
                        (2275,
                         @AffiliateId),
                        (859,
                         @AffiliateId),
                        (988,
                         @AffiliateId),
                        (2034,
                         @AffiliateId),
                        (1464,
                         @AffiliateId),
                        (990,
                         @AffiliateId),
                        (2525,
                         @AffiliateId),
                        (3322,
                         @AffiliateId),
                        (1182,
                         @AffiliateId),
                        (2053,
                         @AffiliateId),
                        (860,
                         @AffiliateId),
                        (1147,
                         @AffiliateId),
                        (1703,
                         @AffiliateId),
                        (1447,
                         @AffiliateId),
                        (2296,
                         @AffiliateId),
                        (1448,
                         @AffiliateId),
                        (2112,
                         @AffiliateId),
                        (1109,
                         @AffiliateId),
                        (1022,
                         @AffiliateId),
                        (2141,
                         @AffiliateId),
                        (344,
                         @AffiliateId),
                        (1012,
                         @AffiliateId),
                        (4347,
                         @AffiliateId),
                        (1416,
                         @AffiliateId),
                        (1036,
                         @AffiliateId),
                        (1654,
                         @AffiliateId),
                        (2556,
                         @AffiliateId),
                        (915,
                         @AffiliateId),
                        (2060,
                         @AffiliateId),
                        (2531,
                         @AffiliateId),
                        (679,
                         @AffiliateId),
                        (554,
                         @AffiliateId),
                        (1965,
                         @AffiliateId),
                        (1712,
                         @AffiliateId),
                        (69,
                         @AffiliateId),
                        (130,
                         @AffiliateId),
                        (2889,
                         @AffiliateId),
                        (882,
                         @AffiliateId),
                        (749,
                         @AffiliateId),
                        (1901,
                         @AffiliateId),
                        (1556,
                         @AffiliateId),
                        (1141,
                         @AffiliateId),
                        (176,
                         @AffiliateId),
                        (910,
                         @AffiliateId),
                        (365,
                         @AffiliateId),
                        (4234,
                         @AffiliateId),
                        (1051,
                         @AffiliateId),
                        (1309,
                         @AffiliateId),
                        (1356,
                         @AffiliateId),
                        (1484,
                         @AffiliateId),
                        (2462,
                         @AffiliateId),
                        (763,
                         @AffiliateId),
                        (1385,
                         @AffiliateId),
                        (942,
                         @AffiliateId),
                        (228,
                         @AffiliateId),
                        (2346,
                         @AffiliateId),
                        (1230,
                         @AffiliateId),
                        (2487,
                         @AffiliateId),
                        (240,
                         @AffiliateId),
                        (688,
                         @AffiliateId),
                        (2037,
                         @AffiliateId),
                        (113,
                         @AffiliateId),
                        (124,
                         @AffiliateId),
                        (1788,
                         @AffiliateId),
                        (1455,
                         @AffiliateId),
                        (1431,
                         @AffiliateId),
                        (913,
                         @AffiliateId),
                        (1713,
                         @AffiliateId),
                        (1374,
                         @AffiliateId),
                        (2027,
                         @AffiliateId),
                        (2614,
                         @AffiliateId),
                        (2113,
                         @AffiliateId),
                        (1418,
                         @AffiliateId),
                        (946,
                         @AffiliateId),
                        (989,
                         @AffiliateId),
                        (1631,
                         @AffiliateId),
                        (777,
                         @AffiliateId),
                        (2162,
                         @AffiliateId),
                        (734,
                         @AffiliateId),
                        (802,
                         @AffiliateId),
                        (975,
                         @AffiliateId),
                        (905,
                         @AffiliateId),
                        (886,
                         @AffiliateId),
                        (1852,
                         @AffiliateId),
                        (2926,
                         @AffiliateId),
                        (202,
                         @AffiliateId),
                        (1201,
                         @AffiliateId),
                        (119,
                         @AffiliateId),
                        (2076,
                         @AffiliateId),
                        (542,
                         @AffiliateId),
                        (1755,
                         @AffiliateId),
                        (2585,
                         @AffiliateId),
                        (1629,
                         @AffiliateId),
                        (1014,
                         @AffiliateId),
                        (1393,
                         @AffiliateId),
                        (1442,
                         @AffiliateId),
                        (1209,
                         @AffiliateId),
                        (710,
                         @AffiliateId),
                        (1233,
                         @AffiliateId),
                        (944,
                         @AffiliateId),
                        (1619,
                         @AffiliateId),
                        (9,
                         @AffiliateId),
                        (1637,
                         @AffiliateId),
                        (1477,
                         @AffiliateId),
                        (641,
                         @AffiliateId),
                        (1414,
                         @AffiliateId),
                        (1139,
                         @AffiliateId),
                        (925,
                         @AffiliateId),
                        (2103,
                         @AffiliateId),
                        (73,
                         @AffiliateId),
                        (754,
                         @AffiliateId),
                        (1968,
                         @AffiliateId),
                        (1943,
                         @AffiliateId),
                        (2133,
                         @AffiliateId),
                        (694,
                         @AffiliateId),
                        (2057,
                         @AffiliateId),
                        (1089,
                         @AffiliateId),
                        (665,
                         @AffiliateId),
                        (1173,
                         @AffiliateId),
                        (2187,
                         @AffiliateId),
                        (388,
                         @AffiliateId),
                        (919,
                         @AffiliateId),
                        (193,
                         @AffiliateId),
                        (673,
                         @AffiliateId),
                        (1027,
                         @AffiliateId),
                        (2148,
                         @AffiliateId),
                        (62,
                         @AffiliateId),
                        (2229,
                         @AffiliateId),
                        (2179,
                         @AffiliateId),
                        (521,
                         @AffiliateId)

            DECLARE @stations_id  INT,
                    @affiliate_id INT;
            DECLARE cursor_station CURSOR FOR
              SELECT stationid,
                     affiliateid
              FROM   @SecondaryAffiliations;

            OPEN cursor_station;

            FETCH next FROM cursor_station INTO @stations_id, @affiliate_id;

            WHILE @@FETCH_STATUS = 0
              BEGIN
                  IF EXISTS(SELECT id
                            FROM   stations
                            WHERE  id = @stations_id)
                    BEGIN
                        INSERT INTO stations_secondary_affiliations
                                    (stations_id,
                                     affiliate_id)
                        VALUES      (@stations_id,
                                     @AffiliateId)
                    END

                  FETCH next FROM cursor_station INTO @stations_id,
                  @affiliate_id;
              END;

            CLOSE cursor_station;

            DEALLOCATE cursor_station;
        END
    END
	GO
/*************************************** END BP-5447 ***************************************/


/*************************************** START BP-5532 ***************************************/
IF OBJECT_ID('program_genres') IS NULL
BEGIN
 CREATE TABLE dbo.program_genres
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,		
		[name] nvarchar(500) NOT NULL,		
		genre_id int NOT NULL
	)
End
GO

/*************************************** END BP-5532 ***************************************/


/*************************************** START BP-5672 ***************************************/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'spot_exceptions_recommended_plans_done')
BEGIN
	DROP TABLE spot_exceptions_ingest_jobs

	DROP TABLE staged_recommended_plan_details
	DROP TABLE staged_recommended_plans
	DROP TABLE staged_out_of_specs
	DROP TABLE staged_unposted_no_plan
	DROP TABLE staged_unposted_no_reel_roster

	DROP TABLE spot_exceptions_recommended_plan_decision
	DROP TABLE spot_exceptions_recommended_plan_details
	DROP TABLE spot_exceptions_recommended_plans

	DROP TABLE spot_exceptions_out_of_spec_decisions
	DROP TABLE spot_exceptions_out_of_specs

	DROP TABLE spot_exceptions_unposted_no_plan
	DROP TABLE spot_exceptions_unposted_no_reel_roster

	CREATE TABLE spot_exceptions_ingest_jobs
	(
		id int IDENTITY(1,1) NOT NULL,
		status int NOT NULL,
		queued_at datetime2(7) NOT NULL,
		queued_by varchar(100) NOT NULL,
		completed_at datetime2(7) NULL,
		error_message nvarchar(max) NULL,
		start_date datetime2(7) NOT NULL,
		result nvarchar(max) NULL
	)

	CREATE TABLE staged_recommended_plans
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_unique_hash_external varchar(255) NOT NULL,
		ambiguity_code int NOT NULL,
		execution_id_external varchar(100) NOT NULL,
		estimate_id int NOT NULL,
		inventory_source varchar(100) NOT NULL,
		house_isci varchar(100) NOT NULL,
		client_isci varchar(100) NOT NULL,
		client_spot_length int NOT NULL,
		broadcast_aired_date datetime2(7) NOT NULL,
		aired_time int NOT NULL,
		station_legacy_call_letters varchar(30) NULL,
		affiliate varchar(30) NULL,
		market_code int NULL,
		market_rank int NULL,
		program_name varchar(500) NOT NULL,
		program_genre varchar(127) NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL
	)

	CREATE TABLE staged_recommended_plan_details
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		staged_recommended_plan_id int NOT NULL,
		recommended_plan_id int NOT NULL,
		execution_trace_id bigint NOT NULL,
		rate money NULL,
		audience_name varchar(127) NULL,
		is_recommended_plan bit NOT NULL,
		plan_clearance_percentage float NULL,
		daypart_code varchar(10) NULL,
		start_time int NULL,
		end_time int NULL,
		monday int NULL,
		tuesday int NULL,
		wednesday int NULL,
		thursday int NULL,
		friday int NULL,
		saturday int NULL,
		sunday int NULL,
		contracted_impressions float NULL,
		delivered_impressions float NULL,
		spot_delivered_impression float NULL,
		plan_total_contracted_impressions float NULL,
		plan_total_delivered_impressions float NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL,
		spot_unique_hash_external varchar(255) NOT NULL,
		execution_id_external varchar(100) NOT NULL
	)

	ALTER TABLE staged_recommended_plan_details
		ADD CONSTRAINT FK_staged_recommended_plan_details_staged_recommended_plans
		FOREIGN KEY (staged_recommended_plan_id) REFERENCES staged_recommended_plans(id)

	CREATE TABLE staged_out_of_specs
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_unique_hash_external varchar(255) NOT NULL,
		execution_id_external varchar(100) NOT NULL,
		estimate_id int NOT NULL,
		inventory_source varchar(100) NOT NULL,
		house_isci varchar(100) NOT NULL,
		client_isci varchar(100) NOT NULL,
		client_spot_length int NOT NULL,
		broadcast_aired_date datetime2(7) NOT NULL,
		aired_time int NOT NULL,
		station_legacy_call_letters varchar(30) NULL,
		affiliate varchar(30) NULL,
		market_code int NULL,
		market_rank int NULL,
		rate money NOT NULL,
		audience_name varchar(127) NOT NULL,
		impressions float NOT NULL,
		program_name varchar(500) NOT NULL,
		program_genre varchar(127) NULL,
		reason_code int NOT NULL,
		reason_code_message varchar(500) NULL,
		lead_in_program_name varchar(500) NULL,
		lead_out_program_name varchar(500) NULL,
		plan_id int NOT NULL,
		daypart_code varchar(10) NULL,
		start_time int NULL,
		end_time int NULL,
		monday int NULL,
		tuesday int NULL,
		wednesday int NULL,
		thursday int NULL,
		friday int NULL,
		saturday int NULL,
		sunday int NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL
	)

	CREATE TABLE staged_unposted_no_plan
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		house_isci varchar(50) NOT NULL,
		client_isci varchar(50) NOT NULL,
		client_spot_length int NOT NULL,
		spot_count int NOT NULL,
		program_air_time datetime2(7) NOT NULL,
		estimate_id bigint NOT NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL
	)

	CREATE TABLE staged_unposted_no_reel_roster
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		house_isci varchar(50) NOT NULL,
		spot_count int NOT NULL,
		program_air_time datetime2(7) NOT NULL,
		estimate_id bigint NOT NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL
	)

	CREATE TABLE spot_exceptions_recommended_plans
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_unique_hash_external varchar(255) NOT NULL,
		ambiguity_code int NOT NULL,
		execution_id_external varchar(100) NOT NULL,
		estimate_id int NOT NULL,
		inventory_source varchar(100) NOT NULL,
		house_isci varchar(100) NOT NULL,
		client_isci varchar(100) NOT NULL,
		spot_length_id int NULL,
		program_air_time datetime NOT NULL,
		station_legacy_call_letters varchar(30) NULL,
		affiliate varchar(30) NULL,
		market_code int NULL,
		market_rank int NULL,
		program_name varchar(500) NOT NULL,
		program_genre varchar(127) NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL,
		ingested_media_week_id int NOT NULL
	)

	ALTER TABLE spot_exceptions_recommended_plans
		ADD CONSTRAINT FK_spot_exceptions_recommended_plans_spot_lengths
		FOREIGN KEY (spot_length_id) REFERENCES spot_lengths(id)

	CREATE TABLE spot_exceptions_recommended_plan_details
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_exceptions_recommended_plan_id int NOT NULL,
		recommended_plan_id int NOT NULL,
		execution_trace_id bigint NOT NULL,
		rate money NULL,
		audience_name varchar(127) NULL,
		contracted_impressions float NULL,
		delivered_impressions float NULL,
		is_recommended_plan bit NOT NULL,
		plan_clearance_percentage float NULL,
		daypart_code varchar(10) NULL,
		start_time int NULL,
		end_time int NULL,
		monday int NULL,
		tuesday int NULL,
		wednesday int NULL,
		thursday int NULL,
		friday int NULL,
		saturday int NULL,
		sunday int NULL,
		spot_delivered_impressions float NULL,
		plan_total_contracted_impressions float NULL,
		plan_total_delivered_impressions float NULL,
		ingested_media_week_id int NOT NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL,
		spot_unique_hash_external varchar(255) NOT NULL,
		execution_id_external varchar(100) NOT NULL
	)

	ALTER TABLE spot_exceptions_recommended_plan_details
		ADD CONSTRAINT FK_spot_exceptions_recommended_plan_details_plans
		FOREIGN KEY (recommended_plan_id) REFERENCES plans(id)

	ALTER TABLE spot_exceptions_recommended_plan_details
		ADD CONSTRAINT FK_spot_exceptions_recommended_plan_details_spot_exceptions_recommended_plans
		FOREIGN KEY (spot_exceptions_recommended_plan_id) REFERENCES spot_exceptions_recommended_plans(id)

	CREATE TABLE spot_exceptions_out_of_specs
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_unique_hash_external varchar(255) NOT NULL,
		execution_id_external varchar(100) NOT NULL,
		reason_code_message nvarchar(500) NULL,
		estimate_id int NOT NULL,
		isci_name varchar(100) NOT NULL,
		recommended_plan_id int NULL,
		program_name nvarchar(500) NULL,
		station_legacy_call_letters varchar(15) NULL,
		spot_length_id int NULL,
		audience_id int NULL,
		program_network varchar(10) NULL,
		program_air_time datetime NOT NULL,
		reason_code_id int NOT NULL,
		impressions float NOT NULL,
		market_code int NULL,
		market_rank int NULL,
		house_isci varchar(100) NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL,
		comment nvarchar(1024) NULL,
		daypart_code nvarchar(20) NULL,
		genre_name nvarchar(40) NULL,
		inventory_source_name varchar(100) NOT NULL,
		ingested_media_week_id int NOT NULL
	)

	ALTER TABLE spot_exceptions_out_of_specs
		ADD CONSTRAINT FK_spot_exceptions_out_of_specs_audiences
		FOREIGN KEY (audience_id) REFERENCES audiences(id)

	ALTER TABLE spot_exceptions_out_of_specs
		ADD CONSTRAINT FK_spot_exceptions_out_of_specs_plans
		FOREIGN KEY (recommended_plan_id) REFERENCES plans(id)

	ALTER TABLE spot_exceptions_out_of_specs
		ADD CONSTRAINT FK_spot_exceptions_out_of_specs_spot_exceptions_out_of_spec_reason_codes
		FOREIGN KEY (reason_code_id) REFERENCES spot_exceptions_out_of_spec_reason_codes(id)

	ALTER TABLE spot_exceptions_out_of_specs
		ADD CONSTRAINT FK_spot_exceptions_out_of_specs_spot_lengths
		FOREIGN KEY (spot_length_id) REFERENCES spot_lengths(id)

	CREATE TABLE spot_exceptions_unposted_no_plan
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		house_isci varchar(50) NOT NULL,
		client_isci varchar(50) NOT NULL,
		client_spot_length_id int NULL,
		count int NOT NULL,
		program_air_time datetime NOT NULL,
		estimate_id bigint NOT NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL,
		ingested_media_week_id int NOT NULL
	)

	CREATE TABLE spot_exceptions_unposted_no_reel_roster
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		house_isci varchar(50) NOT NULL,
		count int NOT NULL,
		program_air_time datetime NOT NULL,
		estimate_id bigint NOT NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL,
		ingested_media_week_id int NOT NULL
	)

	CREATE TABLE spot_exceptions_recommended_plans_done
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_unique_hash_external varchar(255) NOT NULL,
		ambiguity_code int NOT NULL,
		execution_id_external varchar(100) NOT NULL,
		estimate_id int NOT NULL,
		inventory_source varchar(100) NOT NULL,
		house_isci varchar(100) NOT NULL,
		client_isci varchar(100) NOT NULL,
		spot_length_id int NULL,
		program_air_time datetime NOT NULL,
		station_legacy_call_letters varchar(30) NULL,
		affiliate varchar(30) NULL,
		market_code int NULL,
		market_rank int NULL,
		program_name varchar(500) NOT NULL,
		program_genre varchar(127) NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL,
		ingested_media_week_id int NOT NULL
	)

	ALTER TABLE spot_exceptions_recommended_plans_done
		ADD CONSTRAINT FK_spot_exceptions_recommended_plans_done_spot_lengths
		FOREIGN KEY (spot_length_id) REFERENCES spot_lengths(id)

	CREATE TABLE spot_exceptions_recommended_plan_details_done
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_exceptions_recommended_plan_done_id int NOT NULL,
		recommended_plan_id int NOT NULL,
		execution_trace_id bigint NOT NULL,
		rate money NULL,
		audience_name varchar(127) NULL,
		contracted_impressions float NULL,
		delivered_impressions float NULL,
		is_recommended_plan bit NOT NULL,
		plan_clearance_percentage float NULL,
		daypart_code varchar(10) NULL,
		start_time int NULL,
		end_time int NULL,
		monday int NULL,
		tuesday int NULL,
		wednesday int NULL,
		thursday int NULL,
		friday int NULL,
		saturday int NULL,
		sunday int NULL,
		spot_delivered_impressions float NULL,
		plan_total_contracted_impressions float NULL,
		plan_total_delivered_impressions float NULL,
		ingested_media_week_id int NOT NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL,
		spot_unique_hash_external varchar(255) NOT NULL,
		execution_id_external varchar(100) NOT NULL
	)

	ALTER TABLE spot_exceptions_recommended_plan_details_done
		ADD CONSTRAINT FK_spot_exceptions_recommended_plan_details_done_plans
		FOREIGN KEY (recommended_plan_id) REFERENCES plans(id)

	ALTER TABLE spot_exceptions_recommended_plan_details_done
		ADD CONSTRAINT FK_spot_exceptions_recommended_plan_details_done_spot_exceptions_recommended_plans_done
		FOREIGN KEY (spot_exceptions_recommended_plan_done_id) REFERENCES spot_exceptions_recommended_plans_done(id)

	CREATE TABLE spot_exceptions_recommended_plan_done_decisions
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_exceptions_recommended_plan_details_done_id int NOT NULL,
		decided_by varchar(63) NOT NULL,
		decided_at datetime NOT NULL,
		synced_by varchar(100) NULL,
		synced_at datetime2(7) NULL
	)

	ALTER TABLE spot_exceptions_recommended_plan_done_decisions
		ADD CONSTRAINT FK_spot_exceptions_recommended_plan_done_decisions_spot_exceptions_recommended_plans_details_done
		FOREIGN KEY (spot_exceptions_recommended_plan_details_done_id) REFERENCES spot_exceptions_recommended_plan_details_done(id)

	CREATE TABLE spot_exceptions_out_of_specs_done
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_unique_hash_external varchar(255) NOT NULL,
		execution_id_external varchar(100) NOT NULL,
		reason_code_message nvarchar(500) NULL,
		estimate_id int NOT NULL,
		isci_name varchar(100) NOT NULL,
		recommended_plan_id int NULL,
		program_name nvarchar(500) NULL,
		station_legacy_call_letters varchar(15) NULL,
		spot_length_id int NULL,
		audience_id int NULL,
		program_network varchar(10) NULL,
		program_air_time datetime NOT NULL,
		reason_code_id int NOT NULL,
		impressions float NOT NULL,
		market_code int NULL,
		market_rank int NULL,
		house_isci varchar(100) NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL,
		comment nvarchar(1024) NULL,
		daypart_code nvarchar(20) NULL,
		genre_name nvarchar(40) NULL,
		inventory_source_name varchar(100) NOT NULL,
		ingested_media_week_id int NOT NULL
	)

	ALTER TABLE spot_exceptions_out_of_specs_done
		ADD CONSTRAINT FK_spot_exceptions_out_of_specs_done_audiences
		FOREIGN KEY (audience_id) REFERENCES audiences(id)

	ALTER TABLE spot_exceptions_out_of_specs_done
		ADD CONSTRAINT FK_spot_exceptions_out_of_specs_done_plans
		FOREIGN KEY (recommended_plan_id) REFERENCES plans(id)

	ALTER TABLE spot_exceptions_out_of_specs_done
		ADD CONSTRAINT FK_spot_exceptions_out_of_specs_done_spot_exceptions_out_of_spec_reason_codes
		FOREIGN KEY (reason_code_id) REFERENCES spot_exceptions_out_of_spec_reason_codes(id)

	ALTER TABLE spot_exceptions_out_of_specs_done
		ADD CONSTRAINT FK_spot_exceptions_out_of_specs_done_spot_lengths
		FOREIGN KEY (spot_length_id) REFERENCES spot_lengths(id)

	CREATE TABLE spot_exceptions_out_of_spec_done_decisions
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_exceptions_out_of_spec_done_id int NOT NULL,
		accepted_as_in_spec bit NOT NULL,
		decision_notes nvarchar(1024) NULL,
		program_name nvarchar(500) NULL,
		genre_name nvarchar(40) NULL,
		daypart_code nvarchar(20) NULL,
		decided_by varchar(63) NOT NULL,
		decided_at datetime NOT NULL,
		synced_by varchar(100) NULL,
		synced_at datetime2(7) NULL
	)

	ALTER TABLE spot_exceptions_out_of_spec_done_decisions
		ADD CONSTRAINT FK_spot_exceptions_out_of_spec_done_decisions_spot_exceptions_out_of_specs_done
		FOREIGN KEY (spot_exceptions_out_of_spec_done_id) REFERENCES spot_exceptions_out_of_specs_done(id)

END
GO

/*************************************** END BP-5672 ***************************************/

/*************************************** START BP-5366 ***************************************/

DELETE FROM program_name_exceptions WHERE custom_program_name = 'TMZ  LIVE'
DELETE FROM program_name_mappings WHERE official_program_name = 'TMZ  LIVE'
GO
/*************************************** END BP-5366 ***************************************/

/*************************************** START BP-5532 ***************************************/
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'program_genres' AND COLUMN_NAME= 'name')
BEGIN
		DROP TABLE program_genres
		
			CREATE TABLE [program_genres]
			(
				[id] INT NOT NULL PRIMARY KEY IDENTITY, 		
				[program_id] INT NOT NULL, 
				[genre_id] INT NOT NULL, 
				CONSTRAINT [FK_program_genres_programs] FOREIGN KEY ([program_id]) REFERENCES [dbo].[programs] ([id]),
				CONSTRAINT [FK_program_genres_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id])
		
			)				
END
GO
IF OBJECT_ID('program_genres') IS NOT NULL
BEGIN
   DROP TABLE program_genres;
END
GO
IF COL_LENGTH('programs','genre_id') IS NULL
BEGIN
  Truncate table programs
  ALTER TABLE programs
  ADD genre_id int not null
END
GO
DECLARE @PopulatePrograms nvarchar(1000)
SET @PopulatePrograms='TRUNCATE TABLE programs
				INSERT INTO programs([name],show_type_id,genre_id) SELECT DISTINCT official_program_name,show_type_id,genre_id FROM program_name_mappings

				ALTER TABLE dbo.program_name_mappings 
					DROP CONSTRAINT FK_program_name_mappings_genres

				ALTER TABLE dbo.program_name_mappings
					DROP COLUMN genre_id'

IF (COL_LENGTH('programs','genre_id') IS NOT NULL AND COL_LENGTH('program_name_mappings','genre_id') IS NOT NULL)
BEGIN
EXEC (@PopulatePrograms)
END
GO

/*************************************** END BP-5532 ***************************************/

/*************************************** START BP-5662 ************************************/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plans' AND COLUMN_NAME= 'nielsen_transmittal_code')
BEGIN
	ALTER TABLE plans
		ADD nielsen_transmittal_code varchar(250) null
END
GO

/*************************************** END BP-5662 ************************************/

/*************************************** START BP-5847 ************************************/

-- this is for the spot exceptions ingest lambda, not the broadcast app and so it doesn't need to be in the edmx.
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'staged_recommended_plans' AND COLUMN_NAME= 'skipped')
BEGIN
	ALTER TABLE staged_recommended_plans
		ADD skipped BIT NULL
END

GO
/*************************************** END BP-5847 ************************************/

/*************************************** START BP-5536 ************************************/
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_programs_genres')
BEGIN
	ALTER TABLE [dbo].[programs]  WITH CHECK ADD  CONSTRAINT [FK_programs_genres] FOREIGN KEY([genre_id])REFERENCES [dbo].[genres] ([id])
END
GO
/*************************************** END BP-5536 ************************************/

/*************************************** START BP-5685 ************************************/

IF OBJECT_ID('scx_generation_open_market_jobs') IS NULL
BEGIN
CREATE TABLE dbo.scx_generation_open_market_jobs
    (
    [id] [int] identity(1,1) not null primary key,        
    [start_date] [datetime] not null,
    [end_date] [datetime] not null,
    [status] [int] not null,
    [queued_at] [datetime] not null,
    [completed_at] [datetime] null,
    [requested_by] [varchar](63) not null,
	[export_genre_type_id] [int] not null,
	[inventory_source_id] [int] not null
    )
END
GO
IF OBJECT_ID('scx_generation_open_market_job_dayparts') IS NULL
BEGIN
CREATE TABLE dbo.scx_generation_open_market_job_dayparts
    (
    [id] [int] identity(1,1) not null primary key,        
    [standard_daypart_id] [int] not null,
    [scx_generation_open_market_job_id] [int] not null
    )
ALTER TABLE scx_generation_open_market_job_dayparts ADD CONSTRAINT FK_scx_generation_open_market_job_dayparts_standard_dayparts 
FOREIGN KEY(standard_daypart_id) REFERENCES standard_dayparts(id)
ALTER TABLE scx_generation_open_market_job_dayparts ADD CONSTRAINT FK_scx_generation_open_market_job_dayparts_scx_generation_open_market_jobs 
FOREIGN KEY(scx_generation_open_market_job_id) REFERENCES scx_generation_open_market_jobs(id)
END
GO

IF OBJECT_ID('scx_generation_open_market_job_markets') IS NULL
BEGIN
CREATE TABLE dbo.scx_generation_open_market_job_markets
    (
    [id] [int] identity(1,1) not null primary key,        
    [market_code] [smallint] not null,
    [scx_generation_open_market_job_id] [int] not null
    )
ALTER TABLE scx_generation_open_market_job_markets ADD CONSTRAINT FK_scx_generation_open_market_job_markets_markets 
FOREIGN KEY(market_code) REFERENCES markets(market_code)
ALTER TABLE scx_generation_open_market_job_markets ADD CONSTRAINT FK_scx_generation_open_market_job_markets_scx_generation_open_market_jobs 
FOREIGN KEY(scx_generation_open_market_job_id) REFERENCES scx_generation_open_market_jobs(id)
END
GO

IF OBJECT_ID('scx_generation_open_market_job_affiliates') IS NULL
BEGIN
CREATE TABLE dbo.scx_generation_open_market_job_affiliates
    (
    [id] [int] identity(1,1) not null primary key,        
    [affiliate] [varchar](7) not null,
    [scx_generation_open_market_job_id] [int] not null
    )
ALTER TABLE scx_generation_open_market_job_affiliates ADD CONSTRAINT FK_scx_generation_open_market_job_affiliates_scx_generation_open_market_jobs 
FOREIGN KEY(scx_generation_open_market_job_id) REFERENCES scx_generation_open_market_jobs(id)
END
GO


IF OBJECT_ID('scx_generation_open_market_job_files') IS NULL
BEGIN
CREATE TABLE dbo.scx_generation_open_market_job_files
    (
    [id] [int] identity(1,1) not null primary key,
	[scx_generation_open_market_job_id] [int] NOT NULL,
	[file_name] [varchar](255) NOT NULL,
	[standard_daypart_id] [int] NOT NULL,
	[start_date] [datetime] NOT NULL,
	[end_date] [datetime] NOT NULL,
	[market_code] [smallint] NOT NULL,
	[export_genre_type_id] [int] NOT NULL,
	[affiliate] [varchar](7) NOT NULL,
	[shared_folder_files_id] [uniqueidentifier] NULL,
    )
ALTER TABLE scx_generation_open_market_job_files ADD CONSTRAINT FK_scx_generation_open_market_job_files_scx_generation_open_market_jobs 
FOREIGN KEY(scx_generation_open_market_job_id) REFERENCES scx_generation_open_market_jobs(id)
END
GO

IF NOT EXISTS (SELECT *
               FROM   information_schema.columns
               WHERE  table_name = 'scx_generation_open_market_jobs'
                      AND column_name = 'export_genre_type_id')
  BEGIN
      DROP TABLE scx_generation_open_market_job_dayparts

      DROP TABLE scx_generation_open_market_job_markets

      DROP TABLE scx_generation_open_market_job_affiliates

      DROP TABLE scx_generation_open_market_job_files

      DROP TABLE scx_generation_open_market_jobs
  END
GO

/*************************************** END BP-5685 ************************************/

/*************************************** START BP-5878 ************************************/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
where CONSTRAINT_NAME = 'FK_spot_exceptions_recommended_plan_details_spot_exceptions_recommended_plans' 
AND DELETE_RULE ='CASCADE')
BEGIN
	ALTER TABLE spot_exceptions_recommended_plan_details DROP CONSTRAINT FK_spot_exceptions_recommended_plan_details_spot_exceptions_recommended_plans

	ALTER TABLE spot_exceptions_recommended_plan_details  WITH CHECK ADD  CONSTRAINT FK_spot_exceptions_recommended_plan_details_spot_exceptions_recommended_plans FOREIGN KEY(spot_exceptions_recommended_plan_id)
	REFERENCES spot_exceptions_recommended_plans (id) ON DELETE CASCADE
END
GO
/*************************************** END BP-5878 ***************************************/

/*************************************** START BP-5966 ************************************/

-- cleanup program name exceptions
DELETE FROM program_name_exceptions WHERE created_by LIKE 'BP-4864%' 

-- ensure all keywords are in exceptions so mappings will be accepted
INSERT INTO program_name_exceptions (custom_program_name, genre_id, show_type_id, created_by, created_at)
	SELECT k.[program_name], k.genre_id, k.show_type_id
		, 'BP-5966_ReleaseNewProgramsListAndMappings'
		, SYSDATETIME()
	FROM program_name_mapping_keywords k
	LEFT OUTER JOIN program_name_exceptions e
		ON k.[program_name] = e.custom_program_name
	WHERE e.id IS NULL

GO

/*************************************** END BP-5966 ************************************/

/*************************************** START BP-5110 ************************************/

DELETE FROM spot_exceptions_recommended_plan_details WHERE ingested_by = 'Mock Data';
DELETE FROM spot_exceptions_recommended_plans WHERE ingested_by = 'Mock Data';

DELETE FROM spot_exceptions_recommended_plan_done_decisions WHERE decided_by = 'Mock Data';
DELETE FROM spot_exceptions_recommended_plan_details_done WHERE ingested_by = 'Mock Data';
DELETE FROM spot_exceptions_recommended_plans_done WHERE ingested_by = 'Mock Data';

DELETE FROM spot_exceptions_out_of_specs WHERE ingested_by = 'Mock Data';
DELETE FROM spot_exceptions_out_of_spec_done_decisions WHERE decided_by = 'Mock Data';
DELETE FROM spot_exceptions_out_of_specs_done WHERE ingested_by = 'Mock Data';

DELETE FROM spot_exceptions_unposted_no_plan WHERE ingested_by = 'Mock Data';
DELETE FROM spot_exceptions_unposted_no_reel_roster WHERE ingested_by = 'Mock Data';

GO
/*************************************** END BP-5110 ************************************/


/*************************************** START BP-5966 - Part 2 ************************************/

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('usp_UpdateProgramNameMappings'))
BEGIN 
	DROP PROCEDURE usp_UpdateProgramNameMappings	
END

GO

IF EXISTS (SELECT TYPE_ID('ProgramMappingUpdateRequests'))
BEGIN 
	DROP TYPE ProgramMappingUpdateRequests
END

GO

CREATE TYPE [dbo].[ProgramMappingUpdateRequests] AS TABLE(
	[program_name_mapping_id] [int] NOT NULL,
	[official_program_name] [nvarchar](500) NOT NULL,
	[show_type_id] [int] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[program_name_mapping_id] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO

CREATE PROCEDURE [dbo].[usp_UpdateProgramNameMappings]
	@update_requests ProgramMappingUpdateRequests READONLY,
    @modified_by varchar(63),
	@modified_at datetime
AS

/*
DECLARE
	@modified_by varchar(63)='db_queries_tester1',
	@modified_at datetime='2020-6-20',
	@update_requests ProgramMappingUpdateRequests

INSERT INTO @update_requests SELECT 49,'Program A v3',10
INSERT INTO @update_requests SELECT 51,'Program B v3',10
INSERT INTO @update_requests SELECT 52,'Program C v2',5

EXEC [dbo].[usp_UpdateProgramNameMappings] @update_requests, @modified_by, @modified_at
*/

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON

	BEGIN TRAN
	BEGIN TRY
        update mapping
		set 
		mapping.official_program_name = request.official_program_name,		
		mapping.show_type_id = request.show_type_id,
		mapping.modified_at = @modified_at,
		mapping.modified_by = @modified_by
		from program_name_mappings as mapping
		join @update_requests as request on mapping.id = request.program_name_mapping_id
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        IF (@@TRANCOUNT > 0)
		BEGIN
		  ROLLBACK TRAN;
		END;

		THROW
    END CATCH
END

GO
/*************************************** END BP-5966 - Part 2 ************************************/

/*************************************** START BP-5992 ************************************/

IF OBJECT_ID('tempdb..#GenresToLoad') IS NOT NULL
BEGIN 
	DROP TABLE #GenresToLoad
END

CREATE TABLE #GenresToLoad
(
	[name] VARCHAR(50)
)

INSERT INTO #GenresToLoad ([name]) VALUES ('CRIME/INFORMATIONAL')
INSERT INTO #GenresToLoad ([name]) VALUES ('DOCUMENTARY/INFORMATIONAL')
INSERT INTO #GenresToLoad ([name]) VALUES ('ENTERTAINMENT/NEWS')
INSERT INTO #GenresToLoad ([name]) VALUES ('INFORMATIONAL/SPECIAL')
INSERT INTO #GenresToLoad ([name]) VALUES ('LIFESTYLE/NEWS')
INSERT INTO #GenresToLoad ([name]) VALUES ('NEWS/SPECIAL')
INSERT INTO #GenresToLoad ([name]) VALUES ('REALITY TALK')
INSERT INTO #GenresToLoad ([name]) VALUES ('SPECIAL/SPORTS')

INSERT INTO genres ([name], program_source_id, created_by, created_date, modified_by, modified_date)
	SELECT l.[name]
		, 1 -- maestro program source id
		, 'BP-5992: Programs File Load: Resolve unknown genres', SYSDATETIME()
		, 'BP-5992: Programs File Load: Resolve unknown genres', SYSDATETIME()
	FROM #GenresToLoad l
	LEFT OUTER JOIN genres g
		ON l.name = g.name
	WHERE g.id is null

GO

/*************************************** END BP-5992 ************************************/

IF EXISTS (SELECT *
   FROM   information_schema.columns
    WHERE  table_name = 'scx_generation_open_market_job_markets' AND column_name = 'market_code')
					  BEGIN
					  TRUNCATE TABLE scx_generation_open_market_job_markets
					  ALTER TABLE scx_generation_open_market_job_markets
					  DROP CONSTRAINT FK_scx_generation_open_market_job_markets_markets
					  ALTER TABLE scx_generation_open_market_job_markets
					  DROP COLUMN market_code
					  ALTER TABLE scx_generation_open_market_job_markets
					  ADD [rank] int not null
					  END
					  GO          

IF EXISTS (SELECT *
   FROM   information_schema.columns
   WHERE  table_name = 'scx_generation_open_market_job_files' AND column_name = 'market_code')
					  BEGIN
					  TRUNCATE TABLE scx_generation_open_market_job_files
					  ALTER TABLE scx_generation_open_market_job_files
					  DROP COLUMN market_code
					  ALTER TABLE scx_generation_open_market_job_files
					  ADD [rank] varchar(255) not null
					  ALTER TABLE scx_generation_open_market_job_files
					  ALTER COLUMN standard_daypart_id varchar(255) not null
					  ALTER TABLE scx_generation_open_market_job_files
					  ALTER COLUMN  affiliate varchar(255) not null
					  END
					  GO

/*************************************** END UPDATE SCRIPT *******************************************************/

/*************************************** Start BP-6149 ***************************************/

IF NOT EXISTS (SELECT 1 FROM spot_exceptions_out_of_spec_reason_codes WHERE reason_code = 13)
BEGIN 
	
	INSERT INTO spot_exceptions_out_of_spec_reason_codes (reason_code, reason, [label]) VALUES
		(13,'Incorrect ISCI Time','ISCI Time')
		,(14,'Incorrect Program Blank','No Available Program')

	UPDATE spot_exceptions_out_of_spec_reason_codes set label = 'Time'
	WHERE reason_code = 9;

	UPDATE spot_exceptions_out_of_spec_reason_codes set reason = 'Incorrect ISCI Day', label = 'ISCI Day'
	WHERE reason_code = 6;

END

GO
/*************************************** END BP-6149 ***************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '22.10.1', -- Current release version
last_modified_time = SYSDATETIME()
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '22.06.1' -- Previous release version
		OR [version] = '22.10.1') -- Current release version
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