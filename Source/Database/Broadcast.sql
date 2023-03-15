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

/*************************************** START BP-6037 ************************************/

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

/*************************************** END BP-6037 ************************************/

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

/*************************************** START BP-6134 ************************************/

-- Setup the raw data for merge
IF OBJECT_ID('tempdb..#master_stations_for_merge') IS NOT NULL
BEGIN 
	drop table #master_stations_for_merge
END

CREATE TABLE  #master_stations_for_merge
(
	id int identity(1,1) primary key,
	call_letters NVARCHAR(500) NULL, 
	affiliation NVARCHAR(500) NULL, 	
	[owner] NVARCHAR(500) NULL, 
	rep_name NVARCHAR(500) NULL,
	market_code INT NULL
)

INSERT INTO #master_stations_for_merge (call_letters, affiliation, [owner], rep_name, market_code) VALUES
	('EAHU','MYNET','Gray Television, Inc','Millennium Sales & Marketing','184')
	,('ECBI','MyNET','Morris Multi Media Corp.','DIRECT','273')
	,('ECTV','MYNET','Gray Television, Inc','Katz ','130')
	,('EDSI','MYNET','New Age Media','Petry Television, Inc.','175')
	,('EETM','IND','Nexstar Broadcasting Group','In House','165')
	,('EFFF','IND','Smith Media, LLC (Sale pending to Nexstar)','DIRECT','123')
	,('EHSV','FOX','Gray Television, Inc','Continental Television Sales','169')
	,('EIBW','MyNET','Gray Television, Inc','Continental Television Sales','205')
	,('EIYE','CBS',NULL,NULL,'197')
	,('EKBT','MYNET','Morgan Murphy Media','Harrington, Righter & Parsons, Inc.','302')
	,('ELMT','CW',NULL,NULL,'240')
	,('EMGT','MyNet',NULL,NULL,'103')
	,('EPTZ','CW','Hearst Television, Inc.','Eagle Television Sales','123')
	,('ERDW','MYNET','Gray Television, Inc','Continental Television Sales','120')
	,('ERGT','MyNet','Sinclair Broadcast Group','Sinclair ','142')
	,('ESAW','MYNET','Gray Television, Inc','Continental Television Sales','305')
	,('ESAZ','MYNET','Gray Television, Inc','Continental Television Sales','164')
	,('ESWG','MYNET','Gray Television, Inc','In House','125')
	,('ESYX','MYNET','Sinclair Broadcast Group','Katz ','135')
	,('ETAP','FOX','Gray Television, Inc','Continental Television Sales','197')
	,('ETOK','MYNET','Gray Television, Inc','Continental Television Sales','311')
	,('ETVQ','MYNET','Morris Multi Media Corp.','TeleRep, Inc.','141')
	,('EVLT','CW','Gray Television, Inc','DIRECT','157')
	,('EXXV','MYNET','Morris Multi Media Corp.','Millennium Sales & Marketing','346')
	,('GJHG','MyNet','Gray Television, Inc','Continental Television Sales','256')
	,('GTAP','MYNET','Gray Television, Inc','Continental Television Sales','197')
	,('GTVY','MYNET','Gray Television, Inc','Continental Television Sales','206')
	,('HHSV','MYNET','Gray Television, Inc','Continental Television Sales','169')
	,('K47DF','IND','Cordillera Communications','Harrington, Righter & Parsons, Inc.','200')
	,('KAIL','MYNET','Trans-America Broadcasting Corp.',NULL,'466')
	,('KAME','MYNET','Sinclair Broadcast Group','TeleRep, Inc.','411')
	,('KAQY','ABC','Gray Television, Inc','Millennium Sales & Marketing','228')
	,('KATH','NBC','Denali Media','In House','347')
	,('KBGF','NBC','Gray Television, Inc','Petry Television, Inc.','355')
	,('KBJO','CW',NULL,NULL,'238')
	,('KCEB','IND','Gannett Broadcasting',NULL,'309')
	,('KCMB','CBS','Reiten Television, Inc.','Continental Television Sales','287')
	,('KCPM','MYNET','Central Plains Media','In House','324')
	,('KCZ','CW','Schurz Communications, Inc.','Harrington, Righter & Parsons, Inc.','219')
	,('KDOC','IND','Titan TV Broadcast Group','TeleRep, Inc.','403')
	,('KEJB','MYNET','KM Communications',NULL,'228')
	,('KFXF','FOX','Tanana Valley Television Company','In House','345')
	,('KFXP','FOX','Abraham Telecasting Company','Petry Television, Inc.','358')
	,('KHAS','NBC','Gray Television, Inc','DIRECT','322')
	,('KHBS/KHOG','ABC','Hearst Television, Inc.','DIRECT','270')
	,('KIDZ','MYNET','Bayou City Broadcasting','DIRECT','262')
	,('KIVV','FOX','Mission TV, LLC','Millennium Sales & Marketing','364')
	,('KJNE','NBC','New Moon Communications',NULL,'334')
	,('KJUD','ABC','Smith Media, LLC','Continental Television Sales','347')
	,('KLEW','CBS','Sinclair Broadcast Group',NULL,'481')
	,('KLPN','MYNET','Nexstar Broadcasting Group','Millennium Sales & Marketing','309')
	,('KMYS','CW','Sinclair Broadcast Group','Katz ','241')
	,('KNDX','FOX','Prime Cities Television','Millennium Sales & Marketing','287')
	,('KOTR','MYNET','Mirage Media 2 LLC',NULL,'428')
	,('KQEG','IND',NULL,NULL,'302')
	,('KSWT','CBS','Pappas Telecasting Companies','Harrington, Righter & Parsons, Inc.','371')
	,('KTNL','CBS',NULL,NULL,'347')
	,('KTOV','MYNET','Corpus 18, LLC','Petry Television, Inc.','200')
	,('KTTW','FOX','Elmen Company','Continental Television Sales','325')
	,('KTUD','IND','VegasTV, LLC','Harrington, Righter & Parsons, Inc.','439')
	,('KTVA','CBS','Denali Media','In House','343')
	,('KTXD','IND','Gannett Broadcasting',NULL,'223')
	,('KUBE','IND','New World TV Group, LLC','TeleRep, Inc.','218')
	,('KUQI','FOX','Gannett Broadcasting','Petry Television, Inc.','200')
	,('KVMY','MYNET','Sinclair Broadcast Group','Katz ','439')
	,('KVTV','CBS','Eagle Creek Broadcasting','Millennium Sales & Marketing','349')
	,('KXGN','CBS','Glendive Broadcasting Corporation','Millennium Sales & Marketing','398')
	,('KZBK','CBS','Cordillera Communications','Harrington, Righter & Parsons, Inc.','354')
	,('KZBZ','IND',NULL,NULL,'234')
	,('MXGN','NBC','Glendive Broadcasting Corporation','Millennium Sales & Marketing','398')
	,('NBMT','NBC','Gannett Broadcasting','Continental Television Sales','292')
	,('NDBC','MyNet','Pappas Telecasting Companies',NULL,'365')
	,('NESQ','CBS','News-Press & Gazette Company','Continental Television Sales','404')
	,('NEYT','MYNET','Smith Media, LLC(sale pending to News-Press & Company)','Continental Television Sales','455')
	,('NIDK','MYNET',NULL,NULL,'358')
	,('NMIZ','MYNET','News-Press & Gazette Company','Harrington, Righter & Parsons, Inc.','204')
	,('NNBN','MYNET','Rapid Broadcasting Company','Petry Television, Inc.','364')
	,('NNPN','CW','News-Press & Gazette Company',NULL,'238')
	,('NOSA','MYNET','Ica Broadcasting I, Ltd.','Continental Television Sales','233')
	,('NPTH','MYNET','Sinclair Broadcast Group','Harrington, Righter & Parsons, Inc.','224')
	,('NVLY','NBC',NULL,NULL,'324')
	,('NWTX','CW','Gray Television, Inc','DIRECT','225')
	,('SBT2','FOX',NULL,NULL,'188')
	,('UXII','MYNET','Gray Television, Inc','Millennium Sales & Marketing','257')
	,('WAHU','FOX','Gray Television, Inc','Millennium Sales & Marketing','184')
	,('WAMY','MYNET','Nexstar Broadcasting Group','TeleRep, Inc.','291')
	,('WAOE','MYNET','Granite Broadcasting Corporation ','Katz ','275')
	,('WBBH','NBC','Waterman Broadcasting Corporation','Continental Television Sales','171')
	,('WBIN','IND','New Age Media','Petry Television, Inc.','106')
	,('WBPN','MYNET','Stainless Broadcasting Company','In House','102')
	,('WBQD','MYNET','Tribune Broadcasting Co.','DIRECT','282')
	,('WBUW','CW','Byrne Acquisitions Group','DIRECT','269')
	,('WCGV','MYNET','Sinclair Broadcast Group','Katz ','217')
	,('WCUU','IND','Weigel Broadcasting Co.','Harrington, Righter & Parsons, Inc.','202')
	,('WDAZ','ABC',NULL,NULL,'324')
	,('WFTX','FOX','E.W. Scripps Co.','TeleRep, Inc.','171')
	,('WFXI','FOX','Bonten Media Group','Millennium Sales & Marketing','145')
	,('WFXS','FOX','Davis Television','Millennium Sales & Marketing','305')
	,('WGCL','CBS','Meredith Broadcasting Group','Harrington, Righter & Parsons, Inc.','124')
	,('WHAG','NBC','Nexstar Broadcasting Group','Katz ','111')
	,('WHTV','MYNET','Young Broadcasting Inc.(sale to Media General)',NULL,'151')
	,('WHVL','MYNET','Channel Communications',NULL,'174')
	,('WINK','CBS','Fort Myers Broadcasting Company','DIRECT','171')
	,('WJAL','IND','Entravision Communications Corporation','In House','111')
	,('WKDH','ABC','WTVA, Inc.','Continental Television Sales','273')
	,('WLGA','IND','Pappas Telecasting Companies','TeleRep, Inc.','122')
	,('WLLZ','MYNET','P&P Cable Holdings',NULL,'140')
	,('WLMO ','CBS','Block Communications','Petry Television, Inc.','158')
	,('WLWC','CW','OTA Broadcasting','TeleRep, Inc.','121')
	,('WMFD','IND',NULL,NULL,'110')
	,('WMMP','MYNET','Sinclair Broadcast Group','Sinclair ','119')
	,('WMNN','IND',NULL,NULL,'140')
	,('WMYG','MYNET','New Age Media','Petry Television, Inc.','192')
	,('WMYO','MYNET','Block Communications ','TeleRep, Inc.','129')
	,('WMYW-LP','MYNET',NULL,NULL,'150')
	,('WNAB','CW','Sinclair Broadcast Group','Katz ','259')
	,('WNEG','Ind','UGARF Media Holdings, LLC','MMT Sales','167')
	,('WNFM','MYNET','Comcast',NULL,'171')
	,('WNWS (WJLA)','ABC',NULL,NULL,'111')
	,('WNYS','MYNET','Sinclair Broadcast Group','DIRECT','155')
	,('WOTM','IND','WOTM, LLC',NULL,'230')
	,('WPME','MYNET','Ironwood Communications','Petry Television, Inc.','100')
	,('WPMY','MYNET','Sinclair Broadcast Group','Katz ','108')
	,('WQWQ','CW','Raycom Media','Harrington, Righter & Parsons, Inc.','232')
	,('WSTQ','CW','Sinclair Broadcast Group','TeleRep, Inc.','155')
	,('WTO5','CW','Block Communications','Harrington, Righter & Parsons, Inc.','147')
	,('WUTB','MYNET','Sinclair Broadcast Group','Katz ','112')
	,('WVN2','FOX',NULL,NULL,'159')
	,('WWUP','CBS','Heritage Broadcasting Group','DIRECT','140')
	,('WXCW','CW','Sun Broadcasting, Inc.','DIRECT','171')
	,('WXMS','IND','Raycom Media','DIRECT','318')
	,('WZVN','ABC','Waterman Broadcasting Corporation','Continental Television Sales','171')
	,('XETV','CW','Bay City TV, Inc.','DIRECT','425')

GO

-- Merge
BEGIN TRANSACTION
	-- UPDATES
	UPDATE s SET
		affiliation = m.affiliation
		, market_code = m.market_code
		, rep_firm_name = m.rep_name
		, owner_name = m.[owner]
		, modified_by = 'BP-6134'
		, modified_date = SYSDATETIME()
	FROM stations s
	JOIN #master_stations_for_merge m
		ON s.legacy_call_letters = m.call_letters
	WHERE s.modified_by <> 'BP-6134' -- try to avoid nugatory updates

	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date, rep_firm_name, owner_name, is_true_ind)
		SELECT NULL, m.call_letters, m.affiliation, m.market_code, m.call_letters, 'BP-6134', SYSDATETIME(), m.rep_name, m.[owner], 0		
		FROM #master_stations_for_merge m
		LEFT OUTER JOIN stations s
			ON s.legacy_call_letters = m.call_letters
		WHERE s.legacy_call_letters IS NULL

COMMIT TRANSACTION

GO

/*************************************** END BP-6134 ************************************/

/*************************************** START BP-6104 ************************************/

-- Setup the raw data for merge
IF OBJECT_ID('tempdb..#master_stations_for_merge') IS NOT NULL
BEGIN 
	drop table #master_stations_for_merge
END

CREATE TABLE  #master_stations_for_merge
(
	id int identity(1,1) primary key,
	call_letters NVARCHAR(500) NULL, 
	affiliation NVARCHAR(500) NULL, 	
	[owner] NVARCHAR(500) NULL, 
	rep_name NVARCHAR(500) NULL,
	market_code INT NULL
)

INSERT INTO #master_stations_for_merge (call_letters, affiliation, [owner], rep_name, market_code) VALUES
	('ETVW','IND','Morgan Murphy Media',NULL,269)	

GO

-- Merge
BEGIN TRANSACTION
	-- UPDATES
	UPDATE s SET
		affiliation = m.affiliation
		, market_code = m.market_code
		, rep_firm_name = m.rep_name
		, owner_name = m.[owner]
		, modified_by = 'BP-6104'
		, modified_date = SYSDATETIME()
	FROM stations s
	JOIN #master_stations_for_merge m
		ON s.legacy_call_letters = m.call_letters
	WHERE s.modified_by <> 'BP-6104' -- try to avoid nugatory updates

	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date, rep_firm_name, owner_name, is_true_ind)
		SELECT NULL, m.call_letters, m.affiliation, m.market_code, m.call_letters, 'BP-6104', SYSDATETIME(), m.rep_name, m.[owner], 0		
		FROM #master_stations_for_merge m
		LEFT OUTER JOIN stations s
			ON s.legacy_call_letters = m.call_letters
		WHERE s.legacy_call_letters IS NULL	

COMMIT TRANSACTION

GO

/*************************************** END-6104 ************************************/

/*************************************** START BS-125 ************************************/
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'campaigns' AND COLUMN_NAME= 'account_executive')
BEGIN
	ALTER TABLE campaigns
		ADD account_executive varchar(40) NULL,
			client_contact varchar(40) NULL
END
GO
/*************************************** END BS-125 ************************************/

/*************************************** START BP-6385 ************************************/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'spot_exceptions_results_jobs')
BEGIN

	CREATE TABLE spot_exceptions_results_jobs(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		databricks_job_id bigint NOT NULL,
		databricks_run_id int NOT NULL,
		queued_at datetime2(7) NOT NULL,
		queued_by varchar(100) NOT NULL,
		completed_at datetime2(7) NULL,
		result nvarchar(200) NULL
	);
END
GO

/*************************************** END BP-6385 ************************************/

/*************************************** START BS-131 ***************************************/

IF OBJECT_ID('spot_exceptions_out_of_spec_comments') IS NULL
BEGIN
 CREATE TABLE spot_exceptions_out_of_spec_comments
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[spot_unique_hash_external] [varchar](255) NOT NULL,
		[execution_id_external] [varchar](100) NOT NULL,
		[isci_name] [varchar](100) NOT NULL,
		[program_air_time] [datetime] NOT NULL,
		[station_legacy_call_letters] [varchar](15) NOT NULL,
		[reason_code_id] [int] NOT NULL,
		[recommended_plan_id] [int] NOT NULL,
		[comment] [nvarchar](1024) NULL,
		[added_by] [varchar](100) NOT NULL,
		[added_at] [datetime] NOT NULL		
	)
END

DECLARE @SQL_from_todo nvarchar(1000)
DECLARE @SQL_from_done nvarchar(1000)
set @SQL_from_todo='INSERT INTO spot_exceptions_out_of_spec_comments (spot_unique_hash_external, execution_id_external, isci_name, program_air_time, station_legacy_call_letters, reason_code_id, recommended_plan_id, COMMENT, added_by, added_at)
SELECT spot_unique_hash_external,
       execution_id_external,
       isci_name,
       program_air_time,
       station_legacy_call_letters,
       reason_code_id,
       recommended_plan_id,
       COMMENT,
       ingested_at,
       ingested_at
FROM spot_exceptions_out_of_specs where comment is not null'
set @SQL_from_done='INSERT INTO spot_exceptions_out_of_spec_comments (spot_unique_hash_external, execution_id_external, isci_name, program_air_time, station_legacy_call_letters, reason_code_id, recommended_plan_id, COMMENT, added_by, added_at)
SELECT spot_unique_hash_external,
       execution_id_external,
       isci_name,
       program_air_time,
       station_legacy_call_letters,
       reason_code_id,
       recommended_plan_id,
       COMMENT,
       ingested_at,
       ingested_at
FROM spot_exceptions_out_of_specs_done where comment is not null'
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND COLUMN_NAME= 'comment')
BEGIN
EXEC (@SQL_from_todo)
EXEC (@SQL_from_done)
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND COLUMN_NAME= 'comment')
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs
	DROP COLUMN comment
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs_done' AND COLUMN_NAME= 'comment')
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs_done
	DROP COLUMN comment
END

GO
/*************************************** END BS-131 ***************************************/

/*************************************** START BS-443 *************************************/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'plan_versions' 
	AND COLUMN_NAME= 'is_adu_plan')
BEGIN
    ALTER TABLE plan_versions
		ADD is_adu_plan BIT NULL

	ALTER TABLE plan_version_summaries
	ADD is_adu_plan BIT NULL
END

GO

/*************************************** END BS-443 ***************************************/

/*************************************** START BS-429 *************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_spot_exceptions_unposted_no_plan_genres_spot_lengths')
BEGIN
	ALTER TABLE spot_exceptions_unposted_no_plan  WITH CHECK ADD  CONSTRAINT [FK_spot_exceptions_unposted_no_plan_genres_spot_lengths] 
	FOREIGN KEY(client_spot_length_id) REFERENCES spot_lengths (id)
END

GO

/*************************************** END BS-429 ***************************************/

/*************************************** START BP-6299 ************************************/
IF NOT EXISTS (SELECT 1 FROM stations WHERE legacy_call_letters='BMGT')
BEGIN
INSERT INTO stations(station_code, station_call_letters, affiliation, market_code,legacy_call_letters,modified_by,modified_date,rep_firm_name,owner_name,is_true_ind) VALUES
(NULL,'BMGT','BTV',103,'BMGT','BP-6299',SYSDATETIME(),'Katz Media','Morris Multimedia',0)
END

IF NOT EXISTS (SELECT 1 FROM stations WHERE legacy_call_letters='EZDX')
BEGIN
INSERT INTO stations(station_code, station_call_letters, affiliation, market_code,legacy_call_letters,modified_by,modified_date,rep_firm_name,owner_name,is_true_ind) VALUES
(NULL,'EZDX','MET',291,'EZDX','BP-6299',SYSDATETIME(),'Tegna','Tegna Inc',0)
END

IF NOT EXISTS (SELECT 1 FROM stations WHERE legacy_call_letters='NNOECW')
BEGIN
INSERT INTO stations(station_code, station_call_letters, affiliation, market_code,legacy_call_letters,modified_by,modified_date,rep_firm_name,owner_name,is_true_ind) VALUES
(NULL,'NNOE','CW',228,'NNOECW','BP-6299',SYSDATETIME(),'Gray','Gray',0)
END

IF NOT EXISTS (SELECT 1 FROM stations WHERE legacy_call_letters='WECX')
BEGIN
INSERT INTO stations(station_code, station_call_letters, affiliation, market_code,legacy_call_letters,modified_by,modified_date,rep_firm_name,owner_name,is_true_ind) VALUES
(NULL,'WECX-LD','CW',302,'WECX','BP-6299',SYSDATETIME(),'Gray','Gray',0)
END

IF NOT EXISTS (SELECT 1 FROM stations WHERE legacy_call_letters='WYME')
BEGIN
INSERT INTO stations(station_code, station_call_letters, affiliation, market_code,legacy_call_letters,modified_by,modified_date,rep_firm_name,owner_name,is_true_ind) VALUES
(NULL,'WYME-CD','ANT',192,'WYME','BP-6299',SYSDATETIME(),NULL,'New Age Media',0)
END
GO
/*************************************** END BP-6299 ************************************/

/*************************************** START BS-509 *************************************/

IF NOT EXISTS(
select 1
from sysobjects 
where xtype='PK' and 
   parent_obj in (select id from sysobjects where name='spot_exceptions_ingest_jobs'))
BEGIN
    ALTER TABLE spot_exceptions_ingest_jobs
	ADD PRIMARY KEY (id); 
END
GO

/*************************************** END BS-509 ***************************************/

/*************************************** START BP-6445 ***************************************/
IF EXISTS (SELECT 1 FROM stations WHERE legacy_call_letters='ETVW' and affiliation != 'BTV')
BEGIN
UPDATE stations SET
station_code = NULL,
station_call_letters = 'ETVW',
affiliation = 'BTV',
market_code = 249,
modified_by = 'BP-6299',
modified_date = SYSDATETIME(),
rep_firm_name = 'Katz Media',
owner_name = 'Morris Multimedia',
is_true_ind = 0
WHERE legacy_call_letters = 'ETVW';
END
/*************************************** END BP-6445 ***************************************/

/*************************************** START BS-535 ***************************************/
IF OBJECT_ID('time_zones') IS NULL
BEGIN
 CREATE TABLE dbo.time_zones
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,		
		[name] VARCHAR(30) NOT NULL,
        code VARCHAR(5) NOT NULL
	)
END
IF NOT EXISTS (SELECT 1 FROM time_zones)
BEGIN
SET IDENTITY_INSERT time_zones ON;
INSERT INTO [dbo].[time_zones]([id],[name],[code])values('1','Alaskan','AK')
INSERT INTO [dbo].[time_zones]([id],[name],[code])values('2','Central','CT')
INSERT INTO [dbo].[time_zones]([id],[name],[code])values('3','Eastern','ET')
INSERT INTO [dbo].[time_zones]([id],[name],[code])values('4','Hawaiian','HAST')
INSERT INTO [dbo].[time_zones]([id],[name],[code])values('5','Mountain','MT')
INSERT INTO [dbo].[time_zones]([id],[name],[code])values('6','Pacific','PT')
SET IDENTITY_INSERT time_zones OFF;
END
IF OBJECT_ID('market_time_zones') IS NULL
BEGIN
 CREATE TABLE dbo.market_time_zones
	(
	  id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,		
      market_code smallint NOT NULL, 
      time_zone_id INT NOT NULL 
	)
ALTER TABLE market_time_zones ADD CONSTRAINT FK_market_time_zones_markets FOREIGN KEY(market_code) REFERENCES markets(market_code)
ALTER TABLE market_time_zones ADD CONSTRAINT FK_market_time_zones_time_zones FOREIGN KEY(time_zone_id) REFERENCES time_zones(id)
END
IF NOT EXISTS (SELECT 1 FROM market_time_zones)
BEGIN
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('100','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('101','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('102','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('103','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('104','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('105','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('106','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('107','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('108','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('109','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('110','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('111','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('112','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('113','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('114','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('115','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('116','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('117','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('118','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('119','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('120','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('121','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('122','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('123','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('124','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('125','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('126','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('127','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('128','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('129','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('130','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('131','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('132','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('133','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('134','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('135','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('136','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('137','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('138','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('139','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('140','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('141','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('142','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('143','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('144','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('145','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('146','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('147','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('148','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('149','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('150','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('151','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('152','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('153','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('154','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('155','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('156','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('157','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('158','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('159','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('160','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('161','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('163','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('164','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('165','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('166','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('167','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('169','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('170','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('171','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('173','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('174','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('175','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('176','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('177','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('181','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('182','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('183','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('184','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('188','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('192','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('196','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('197','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('198','3')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('200','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('202','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('203','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('204','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('205','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('206','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('209','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('210','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('211','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('212','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('213','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('216','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('217','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('218','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('219','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('222','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('223','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('224','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('225','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('226','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('227','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('228','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('230','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('231','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('232','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('233','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('234','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('235','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('236','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('237','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('238','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('239','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('240','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('241','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('242','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('243','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('244','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('247','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('248','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('249','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('250','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('251','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('252','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('256','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('257','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('258','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('259','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('261','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('262','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('269','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('270','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('271','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('273','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('275','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('276','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('278','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('279','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('282','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('286','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('287','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('291','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('292','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('293','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('298','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('302','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('305','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('309','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('310','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('311','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('316','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('317','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('318','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('322','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('324','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('325','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('334','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('336','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('337','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('340','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('343','1')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('344','4')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('345','1')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('346','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('347','1')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('349','2')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('351','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('352','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('353','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('354','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('355','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('356','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('357','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('358','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('359','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('360','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('362','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('364','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('365','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('366','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('367','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('370','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('371','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('373','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('389','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('390','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('398','5')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('400','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('401','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('402','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('403','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('404','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('407','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('410','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('411','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('413','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('419','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('420','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('421','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('425','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('428','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('439','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('455','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('462','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('466','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('468','6')
INSERT INTO [dbo].[market_time_zones]([market_code],[time_zone_id])values('481','6')
END
GO
/*************************************** END BS-535 ***************************************/

/*************************************** START BS-640 ***************************************/

CREATE OR ALTER VIEW [dbo].[vw_audiences]
/* View for external consumers. */
AS 
	SELECT [id]
		,[category_code]
		,[sub_category_code]
		,[range_start]
		,[range_end]
		,[custom]
		,[code]
		,[name]
	FROM [dbo].[audiences]
GO

CREATE OR ALTER VIEW [dbo].[vw_audience_audiences]
/* View for external consumers. */
AS 
	SELECT [rating_category_group_id]
		,[custom_audience_id]
		,[rating_audience_id]
	FROM [dbo].[audience_audiences]
GO

CREATE OR ALTER VIEW [dbo].[vw_affiliates]
/* View for external consumers. */
AS 
	SELECT [id]
		,[name]
		,[created_by]
		,[created_date]
		,[modified_by]
		,[modified_date]
	FROM [dbo].[affiliates]

GO

CREATE OR ALTER VIEW [dbo].[vw_campaigns]
/* View for external consumers. */
AS
	SELECT [id]
		,[name]
		,[advertiser_id]
		,[agency_id]
		,[created_date]
		,[created_by]
		,[modified_date]
		,[modified_by]
		,[notes]
		,[agency_master_id]
		,[advertiser_master_id]
		,[unified_id]
		,[max_fluidity_percent]
		,[unified_campaign_last_sent_at]
		,[unified_campaign_last_received_at]
		,[view_details_url]
		,[account_executive]
		,[client_contact]
	FROM [dbo].[campaigns]

GO

CREATE OR ALTER VIEW [dbo].[vw_genres]
/* View for external consumers. */
AS
	SELECT [id]
		,[name]
		,[created_by]
		,[created_date]
		,[modified_by]
		,[modified_date]
		,[program_source_id]
	FROM [dbo].[genres]

GO

CREATE OR ALTER VIEW [dbo].[vw_inventory_sources]
/* View for external consumers. */
AS
	SELECT [id]
		,[name]
		,[is_active]
		,[inventory_source_type]
	FROM [dbo].[inventory_sources]
	
GO

CREATE OR ALTER VIEW [dbo].[vw_market_coverages]
/* View for external consumers. */
AS
	SELECT [id]
		,[rank]
		,[market_code]
		,[tv_homes]
		,[percentage_of_us]
		,[market_coverage_file_id]
	FROM [dbo].[market_coverages]

GO

CREATE OR ALTER VIEW [dbo].[vw_market_dma_map]
/* View for external consumers. */
AS
	SELECT [market_code]
		,[dma_mapped_value]
	FROM [dbo].[market_dma_map]

GO

CREATE OR ALTER VIEW [dbo].[vw_markets]
/* View for external consumers. */
AS
	SELECT [market_code]
		,[geography_name]
	FROM [dbo].[markets]

GO

CREATE OR ALTER VIEW [dbo].[vw_media_weeks]
/* View for external consumers. */
AS
	SELECT [id]
		,[media_month_id]
		,[week_number]
		,[start_date]
		,[end_date]
	FROM [dbo].[media_weeks]
GO

CREATE OR ALTER VIEW [dbo].[vw_plan_iscis]
/* View for external consumers. */
AS
	SELECT [id]
		,[plan_id]
		,[isci]
		,[created_at]
		,[created_by]
		,[deleted_at]
		,[deleted_by]
		,[flight_start_date]
		,[flight_end_date]
		,[modified_at]
		,[modified_by]
		,[spot_length_id]
		,[start_time]
		,[end_time]
	FROM [dbo].[plan_iscis]
GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_available_markets]
/* View for external consumers. */
AS
	SELECT [id]
		,[market_code]
		,[market_coverage_File_id]
		,[rank]
		,[percentage_of_us]
		,[share_of_voice_percent]
		,[plan_version_id]
		,[is_user_share_of_voice_percent]
	FROM [dbo].[plan_version_available_markets]
GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_blackout_markets]
/* View for external consumers. */
AS
	SELECT [id]
		,[market_code]
		,[market_coverage_file_id]
		,[rank]
		,[percentage_of_us]
		,[plan_version_id]
	FROM [dbo].[plan_version_blackout_markets]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_creative_lengths]
/* View for external consumers. */
AS
	SELECT [id]
		,[plan_version_id]
		,[spot_length_id]
		,[weight]
	FROM [dbo].[plan_version_creative_lengths]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_daypart_program_restrictions]
/* View for external consumers. */
AS
	SELECT [id]
		,[plan_version_daypart_id]
		,[program_name]
		,[genre_id]
		,[content_rating]
	FROM [dbo].[plan_version_daypart_program_restrictions]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_daypart_genre_restrictions]
/* View for external consumers. */
AS
	SELECT [id]
		,[plan_version_daypart_id]
		,[genre_id]
	FROM [dbo].[plan_version_daypart_genre_restrictions]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_daypart_affiliate_restrictions]
/* View for external consumers. */
AS
	SELECT [id]
		,[plan_version_daypart_id]
		,[affiliate_id]
	FROM [dbo].[plan_version_daypart_affiliate_restrictions]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_daypart_show_type_restrictions]
/* View for external consumers. */
AS
	SELECT [id]
		,[plan_version_daypart_id]
		,[show_type_id]
	FROM [dbo].[plan_version_daypart_show_type_restrictions]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_dayparts]
/* View for external consumers. */
AS
	SELECT [id]
		,[standard_daypart_id]
		,[start_time_seconds]
		,[end_time_seconds]
		,[weighting_goal_percent]
		,[daypart_type]
		,[is_start_time_modified]
		,[is_end_time_modified]
		,[plan_version_id]
		,[show_type_restrictions_contain_type]
		,[genre_restrictions_contain_type]
		,[program_restrictions_contain_type]
		,[affiliate_restrictions_contain_type]
		,[weekdays_weighting]
		,[weekend_weighting]
	FROM [dbo].[plan_version_dayparts]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_daypart_customizations]
/* View for external consumers. */
AS
	SELECT [id]
		,[plan_version_daypart_id]
		,[custom_daypart_organization_id]
		,[custom_daypart_name]
	FROM [dbo].[plan_version_daypart_customizations]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_flight_hiatus_days]
/* View for external consumers. */
AS
	SELECT [id]
		,[hiatus_day]
		,[plan_version_id]
	FROM [dbo].[plan_version_flight_hiatus_days]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_secondary_audiences]
/* View for external consumers. */
AS
	SELECT [id]
		,[audience_type]
		,[audience_id]
		,[vpvh]
		,[rating_points]
		,[impressions]
		,[cpm]
		,[cpp]
		,[universe]
		,[plan_version_id]
	FROM [dbo].[plan_version_secondary_audiences]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_summaries]
/* View for external consumers. */
AS
	SELECT [id]
		,[processing_status]
		,[hiatus_days_count]
		,[active_day_count]
		,[available_market_count]
		,[available_market_total_us_coverage_percent]
		,[blackout_market_count]
		,[blackout_market_total_us_coverage_percent]
		,[product_name]
		,[available_market_with_sov_count]
		,[plan_version_id]
		,[fluidity_percentage]
		,[is_adu_plan]
	FROM [dbo].[plan_version_summaries]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_daypart_weekly_breakdown]
/* View for external consumers. */
AS
	SELECT [id]
		,[plan_version_daypart_goal_id]
		,[media_week_id]
		,[number_active_days]
		,[active_days_label]
		,[impressions]
		,[impressions_percentage]
		,[plan_version_id]
		,[rating_points]
		,[budget]
		,[spot_length_id]
		,[percentage_of_week]
		,[adu_impressions]
		,[unit_impressions]
		,[is_locked]
	FROM [dbo].[plan_version_daypart_weekly_breakdown]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_versions]
/* View for external consumers. */
AS
	SELECT [id]
		,[plan_id]
		,[is_draft]
		,[equivalized]
		,[flight_start_date]
		,[flight_end_date]
		,[flight_notes]
		,[audience_type]
		,[posting_type]
		,[target_audience_id]
		,[share_book_id]
		,[hut_book_id]
		,[budget]
		,[target_impression]
		,[target_cpm]
		,[target_rating_points]
		,[target_cpp]
		,[target_universe]
		,[hh_impressions]
		,[hh_cpm]
		,[hh_rating_points]
		,[hh_cpp]
		,[hh_universe]
		,[currency]
		,[target_vpvh]
		,[coverage_goal_percent]
		,[goal_breakdown_type]
		,[status]
		,[created_by]
		,[created_date]
		,[modified_by]
		,[modified_date]
		,[version_number]
		,[is_adu_enabled]
		,[impressions_per_unit]
		,[flight_notes_internal]
		,[fluidity_percentage]
		,[fluidity_child_category]
		,[fluidity_category]
		,[is_adu_plan]
	FROM [dbo].[plan_versions]

GO

CREATE OR ALTER VIEW [dbo].[vw_plans]
/* View for external consumers. */
AS
	SELECT [id]
		,[campaign_id]
		,[name]
		,[product_id]
		,[latest_version_id]
		,[product_master_id]
		,[spot_allocation_model_mode]
		,[plan_mode]
		,[deleted_by]
		,[deleted_at]
		,[unified_tactic_line_id]
		,[unified_campaign_last_sent_at]
		,[unified_campaign_last_received_at]
		,[nielsen_transmittal_code]
	FROM [dbo].[plans]

GO

CREATE OR ALTER VIEW [dbo].[vw_program_name_mappings]
/* View for external consumers. */
AS
	SELECT [id]
		,[inventory_program_name]
		,[official_program_name]
		,[show_type_id]
		,[created_by]
		,[created_at]
		,[modified_by]
		,[modified_at]
	FROM [dbo].[program_name_mappings]

GO

CREATE OR ALTER VIEW [dbo].[vw_show_types]
/* View for external consumers. */
AS
	SELECT [id]
		,[name]
		,[created_by]
		,[created_date]
		,[modified_by]
		,[modified_date]
		,[program_source_id]
	FROM [dbo].[show_types]

GO

CREATE OR ALTER VIEW [dbo].[vw_spot_lengths]
/* View for external consumers. */
AS
	SELECT [id]
		,[length]
		,[delivery_multiplier]
		,[order_by]
		,[is_default]
	FROM [dbo].[spot_lengths]

GO

CREATE OR ALTER VIEW [dbo].[vw_standard_dayparts]
/* View for external consumers. */
AS
	SELECT [id]
		,[daypart_type]
		,[daypart_id]
		,[code]
		,[name]
		,[vpvh_calculation_source_type]
	FROM [dbo].[standard_dayparts]

GO

CREATE OR ALTER VIEW [dbo].[vw_stations]
/* View for external consumers. */
AS
	SELECT [station_code]
		,[station_call_letters]
		,[affiliation]
		,[market_code]
		,[legacy_call_letters]
		,[modified_by]
		,[modified_date]
		,[id]
		,[rep_firm_name]
		,[owner_name]
		,[is_true_ind]
	FROM [dbo].[stations]

GO

CREATE OR ALTER VIEW [dbo].[vw_station_mappings]
/* View for external consumers. */
AS
	SELECT [id]
		,[station_id]
		,[mapped_call_letters]
		,[map_set]
		,[created_date]
		,[created_by]
	FROM [dbo].[station_mappings]

GO

CREATE OR ALTER VIEW [dbo].[vw_plan_version_daypart_flat]
/* View for external consumers. */
AS
	SELECT
		pvd.id AS plan_version_daypart_id
		, pvd.plan_version_id AS plan_version_id
		, pvd.standard_daypart_id 
		, sd.code AS standard_daypart_code
		, sd.[name] AS standard_daypart_name
		, sd.daypart_type AS daypart_type_id
		, CASE sd.daypart_type
			WHEN 1 THEN 'News'
			WHEN 2 THEN 'Entertainment/Non-News'
			WHEN 3 THEN 'ROS'
			WHEN 4 THEN 'Sports'
			ELSE NULL
		END AS [daypart_type_name]
		, pvdc.custom_daypart_organization_id	
		, pvdc.custom_daypart_name
		, pvdc.custom_daypart_organization_id AS organization_id
		, cdo.organization_name
		, d.id AS daypart_id
		, d.tier
		, pvd.start_time_seconds 
		, pvd.is_start_time_modified
		, pvd.end_time_seconds 
		, pvd.is_end_time_modified
		, d.mon
		, d.tue
		, d.wed
		, d.thu
		, d.fri
		, d.sat
		, d.sun
		, d.daypart_text
		, d.total_hours
		, pvd.weighting_goal_percent
		, pvd.weekdays_weighting
		, pvd.weekend_weighting
	FROM plan_version_dayparts pvd
	JOIN standard_dayparts sd
		ON pvd.standard_daypart_id = sd.id
	LEFT OUTER JOIN plan_version_daypart_customizations pvdc
		ON pvdc.plan_version_daypart_id = pvd.id
	LEFT OUTER JOIN custom_daypart_organizations cdo
		ON cdo.id = pvdc.custom_daypart_organization_id
	JOIN vw_ccc_daypart d
		ON sd.daypart_id = d.id
GO

/*************************************** END BS-640 ***************************************/

/*************************************** START BS-692 ***************************************/

IF (SELECT COLUMNPROPERTY(OBJECT_ID('spot_exceptions_out_of_specs', 'U'), 'estimate_id', 'AllowsNull')) = 0
BEGIN

	ALTER TABLE staged_out_of_specs
		ALTER COLUMN estimate_id int NULL;

	ALTER TABLE staged_recommended_plans
		ALTER COLUMN estimate_id int NULL;

	ALTER TABLE staged_unposted_no_plan
		ALTER COLUMN estimate_id int NULL;

	ALTER TABLE staged_unposted_no_reel_roster
		ALTER COLUMN estimate_id int NULL;

	ALTER TABLE spot_exceptions_out_of_specs
		ALTER COLUMN estimate_id int NULL;

	ALTER TABLE spot_exceptions_out_of_specs_done
		ALTER COLUMN estimate_id int NULL;

	ALTER TABLE spot_exceptions_recommended_plans
		ALTER COLUMN estimate_id int NULL;

	ALTER TABLE spot_exceptions_recommended_plans_done
		ALTER COLUMN estimate_id int NULL;

	ALTER TABLE spot_exceptions_unposted_no_plan
		ALTER COLUMN estimate_id int NULL;

	ALTER TABLE spot_exceptions_unposted_no_reel_roster
		ALTER COLUMN estimate_id int NULL;

END
GO
/*************************************** END BS-692 ***************************************/

/*************************************** START BP-6481 ***************************************/

IF (SELECT COLUMNPROPERTY(OBJECT_ID('spot_exceptions_out_of_spec_comments', 'U'), 'station_legacy_call_letters', 'AllowsNull')) = 0
BEGIN

	ALTER TABLE spot_exceptions_out_of_spec_comments
		ALTER COLUMN station_legacy_call_letters varchar(15) NULL;

END
GO
/*************************************** END BP-6481 ***************************************/

/*************************************** START BS-640 - 2 ***************************************/

CREATE OR ALTER VIEW [dbo].[vw_plan_version_weekly_breakdown]
/* View for external consumers. */
AS
SELECT [id]
      ,[media_week_id]
      ,[number_active_days]
      ,[active_days_label]
      ,[impressions]
      ,[impressions_percentage]
      ,[plan_version_id]
      ,[rating_points]
      ,[budget]
      ,[spot_length_id]
      ,[standard_daypart_id]
      ,[percentage_of_week]
      ,[adu_impressions]
      ,[unit_impressions]
      ,[is_locked]
      ,[custom_daypart_organization_id]
      ,[custom_daypart_name]
  FROM [dbo].[plan_version_weekly_breakdown]

GO

IF EXISTS(SELECT 1 FROM sys.views WHERE [name] = 'vw_plan_version_daypart_weekly_breakdown')
BEGIN
	DROP VIEW vw_plan_version_daypart_weekly_breakdown
END

GO

/*************************************** END BS-640 - 2 ***************************************/

/*************************************** START BS-2281 ***************************************/

DECLARE @NewDiginetPlatformCodesUser VARCHAR(40) = 'NewDiginetPlatformCodes_2023_3'

IF OBJECT_ID('tempdb..#affiliatesToAdd') IS NOT NULL
BEGIN
	DROP TABLE #affiliatesToAdd
END

CREATE TABLE #affiliatesToAdd
(
	affiliate VARCHAR(5) 
)

INSERT INTO #affiliatesToAdd (affiliate) VALUES ('MYS')
INSERT INTO #affiliatesToAdd (affiliate) VALUES ('LAF')
INSERT INTO #affiliatesToAdd (affiliate) VALUES ('CTV')
INSERT INTO #affiliatesToAdd (affiliate) VALUES ('BOU')
INSERT INTO #affiliatesToAdd (affiliate) VALUES ('DFY')
INSERT INTO #affiliatesToAdd (affiliate) VALUES ('TRN')
INSERT INTO #affiliatesToAdd (affiliate) VALUES ('ANT')

INSERT INTO affiliates ([name], created_by, created_date, modified_by, modified_date)
	SELECT n.affiliate, @NewDiginetPlatformCodesUser, SYSDATETIME(), @NewDiginetPlatformCodesUser, SYSDATETIME()
	FROM #affiliatesToAdd n
	LEFT OUTER JOIN affiliates a
		ON n.affiliate = a.[name]
	WHERE a.[name] IS NULL

GO

/*************************************** END BS-2281 *****************************************/

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