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

-- Only run this script when the schema is in the correct pervious version
INSERT INTO #previous_version
		SELECT parameter_value 
		FROM system_component_parameters 
		WHERE parameter_key = 'SchemaVersion' 


/*************************************** START UPDATE SCRIPT *****************************************************/



/*************************************** BCOP-1694 - START *****************************************************/
IF EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE  TABLE_NAME = 'proposal_version_detail_quarter_weeks'
                 AND COLUMN_NAME = 'impressions')
BEGIN
	EXEC sp_rename 'proposal_version_detail_quarter_weeks.[impressions]', 'impressions_goal', 'COLUMN'
END

IF EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE  TABLE_NAME = 'bvs_map_types'
                 AND COLUMN_NAME = 'last_modified_by')
BEGIN
	EXEC sp_rename 'bvs_map_types.[last_modified_by]', 'modified_by', 'COLUMN'
END

IF EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE  TABLE_NAME = 'bvs_map_types'
                 AND COLUMN_NAME = 'last_modified_date')
BEGIN
	EXEC sp_rename 'bvs_map_types.[last_modified_date]', 'modified_date', 'COLUMN'
END


IF EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE  TABLE_NAME = 'schedule_detail_audiences'
                 AND COLUMN_NAME = 'demo_rank')
BEGIN
	EXEC sp_rename 'schedule_detail_audiences.[demo_rank]', 'audience_rank', 'COLUMN'
END 

IF EXISTS(SELECT *
          FROM   INFORMATION_SCHEMA.COLUMNS
          WHERE  TABLE_NAME = 'schedule_detail_audiences'
                 AND COLUMN_NAME = 'demo_population')
BEGIN
	EXEC sp_rename 'schedule_detail_audiences.[demo_population]', 'audience_population', 'COLUMN'
END

/*************************************** BCOP-1694 - END *****************************************************/



/*************************************** BCOP-1643 - START *****************************************************/

-- inventory_detail_slot_component_proposal

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'inventory_detail_slot_component_proposal')
BEGIN
	CREATE TABLE inventory_detail_slot_component_proposal
	(
		inventory_detail_slot_component_id INT NOT NULL,
		proprosal_version_detail_quarter_week_id INT NOT NULL,
		inventory_detail_slot_id INT NOT NULL,
		daypart_id INT NOT NULL,
		station_code SMALLINT NOT NULL,
		station_program_id INT NOT NULL,
		station_program_flight_id INT NOT NULL
		CONSTRAINT PK_inventory_detail_slot_component_proposal PRIMARY KEY CLUSTERED (inventory_detail_slot_component_id, proprosal_version_detail_quarter_week_id)
	)
END

-- inventory_detail_slot_proposal

IF NOT EXISTS(SELECT 1 FROM sys.columns
              WHERE name = 'rolled_up_daypart_id'
              AND object_id = object_id('inventory_detail_slot_proposal'))
BEGIN
	ALTER TABLE inventory_detail_slot_proposal 
	ADD rolled_up_daypart_id INT NULL

	-- Avoid error when validating the query because column does not exists yet.
	EXEC('UPDATE
			inventory_detail_slot_proposal
		 SET
			inventory_detail_slot_proposal.rolled_up_daypart_id = inventory_detail_slots.rolled_up_daypart_id
		 FROM
			inventory_detail_slot_proposal
			JOIN inventory_detail_slots ON inventory_detail_slots.id = inventory_detail_slot_proposal.inventory_detail_slot_id')

	ALTER TABLE inventory_detail_slot_proposal 
	ALTER COLUMN rolled_up_daypart_id INT NOT NULL
END


IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'slot_cost'
              AND object_id = object_id('inventory_detail_slot_proposal'))
BEGIN
	ALTER TABLE inventory_detail_slot_proposal 
	ADD slot_cost MONEY NULL

	EXEC('UPDATE inventory_detail_slot_proposal
		  SET slot_cost = 0')

	ALTER TABLE inventory_detail_slot_proposal 
	ALTER COLUMN slot_cost MONEY NOT NULL
END


IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'spot_length_id'
              AND object_id = object_id('inventory_detail_slot_proposal'))
BEGIN
	ALTER TABLE inventory_detail_slot_proposal 
	ADD spot_length_id INT NULL

	EXEC('UPDATE
			inventory_detail_slot_proposal
		 SET
			inventory_detail_slot_proposal.spot_length_id = inventory_detail_slots.spot_length_id
		 FROM
			inventory_detail_slot_proposal
			JOIN inventory_detail_slots ON inventory_detail_slots.id = inventory_detail_slot_proposal.inventory_detail_slot_id')

	ALTER TABLE inventory_detail_slot_proposal 
	ALTER COLUMN spot_length_id INT NOT NULL
END


IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'impressions'
              AND object_id = object_id('inventory_detail_slot_proposal'))
BEGIN
	ALTER TABLE inventory_detail_slot_proposal 
	ADD impressions INT NULL

	EXEC('UPDATE inventory_detail_slot_proposal
		 SET impressions = 0')

	ALTER TABLE inventory_detail_slot_proposal 
	ALTER COLUMN impressions INT NOT NULL
END

-- station_program_flight_proposal

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'impressions'
              AND object_id = object_id('station_program_flight_proposal'))
BEGIN
	ALTER TABLE station_program_flight_proposal 
	ADD impressions INT NULL

	EXEC('UPDATE station_program_flight_proposal
		  SET impressions = 0')

	ALTER TABLE station_program_flight_proposal 
	ALTER COLUMN impressions INT NOT NULL
END


IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'spot_cost'
              AND object_id = object_id('station_program_flight_proposal'))
BEGIN
	ALTER TABLE station_program_flight_proposal 
	ADD spot_cost MONEY NULL
	
	EXEC('UPDATE station_program_flight_proposal
		  SET spot_cost = 0')

	ALTER TABLE station_program_flight_proposal 
	ALTER COLUMN spot_cost MONEY NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'spot_length_id'
              AND object_id = object_id('station_program_flight_proposal'))
BEGIN
	ALTER TABLE station_program_flight_proposal 
	ADD spot_length_id INT NULL

	EXEC('UPDATE
			station_program_flight_proposal
		  SET
			station_program_flight_proposal.spot_length_id = station_programs.spot_length_id
		  FROM
			station_program_flight_proposal
			JOIN station_program_flights ON station_program_flights.id = station_program_flight_proposal.station_program_flight_id
			JOIN station_programs ON station_programs.id = station_program_flights.station_program_id')

	ALTER TABLE station_program_flight_proposal 
	ALTER COLUMN spot_length_id INT NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'station_code'
              AND object_id = object_id('station_program_flight_proposal'))
BEGIN
	ALTER TABLE station_program_flight_proposal 
	ADD station_code SMALLINT NULL

	EXEC('UPDATE
			station_program_flight_proposal
		  SET
			station_program_flight_proposal.station_code = station_programs.station_code
		  FROM
			station_program_flight_proposal
			JOIN station_program_flights ON station_program_flights.id = station_program_flight_proposal.station_program_flight_id
			JOIN station_programs ON station_programs.id = station_program_flights.station_program_id')

	ALTER TABLE station_program_flight_proposal 
	ALTER COLUMN station_code SMALLINT NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'station_program_id'
              AND object_id = object_id('station_program_flight_proposal'))
BEGIN
	ALTER TABLE station_program_flight_proposal 
	ADD station_program_id INT NULL

	EXEC('UPDATE
			station_program_flight_proposal
		  SET
			station_program_flight_proposal.station_program_id = station_program_flights.station_program_id
		  FROM
			station_program_flight_proposal
			JOIN station_program_flights ON station_program_flights.id = station_program_flight_proposal.station_program_flight_id')

	ALTER TABLE station_program_flight_proposal 
	ALTER COLUMN station_program_id INT NOT NULL
END

IF EXISTS (SELECT * 
		   FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
		   WHERE CONSTRAINT_NAME ='FK_station_program_flight_proposal_station_program_flights')
BEGIN
	ALTER TABLE station_program_flight_proposal
	DROP CONSTRAINT FK_station_program_flight_proposal_station_program_flights
END

IF EXISTS (SELECT * 
		   FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS 
		   WHERE CONSTRAINT_NAME ='FK_inventory_detail_slot_proposal_inventory_detail_slots')
BEGIN
	ALTER TABLE inventory_detail_slot_proposal
	DROP CONSTRAINT FK_inventory_detail_slot_proposal_inventory_detail_slots
END

/*************************************** BCOP-1643 - END *****************************************************/

/*************************************** BCOP-1823 - START *****************************************************/
IF EXISTS(select 1
      from sys.all_columns c
      join sys.tables t on t.object_id = c.object_id
      join sys.schemas s on s.schema_id = t.schema_id
      join sys.default_constraints d on c.default_object_id = d.object_id
    where d.name = 'DF_station_programs_spot_length_id')
BEGIN
	ALTER TABLE dbo.station_programs DROP CONSTRAINT DF_station_programs_spot_length_id
END
GO

IF EXISTS(select 1
      from sys.all_columns c
      join sys.tables t on t.object_id = c.object_id
      join sys.schemas s on s.schema_id = t.schema_id
      join sys.default_constraints d on c.default_object_id = d.object_id
    where d.name = 'DF_inventory_detail_slot_proposal_created_by')
BEGIN
	ALTER TABLE dbo.inventory_detail_slot_proposal DROP CONSTRAINT DF_inventory_detail_slot_proposal_created_by
END
GO

IF EXISTS(select 1
      from sys.all_columns c
      join sys.tables t on t.object_id = c.object_id
      join sys.schemas s on s.schema_id = t.schema_id
      join sys.default_constraints d on c.default_object_id = d.object_id
    where d.name = 'DF_proposal_version_detail_quarter_weeks_open_market_cost_total')
BEGIN
	ALTER TABLE dbo.proposal_version_detail_quarter_weeks DROP CONSTRAINT DF_proposal_version_detail_quarter_weeks_open_market_cost_total
END
GO

IF EXISTS(select 1
      from sys.all_columns c
      join sys.tables t on t.object_id = c.object_id
      join sys.schemas s on s.schema_id = t.schema_id
      join sys.default_constraints d on c.default_object_id = d.object_id
    where d.name = 'DF_proposal_version_detail_quarter_weeks_open_market_impressions_total')
BEGIN
	ALTER TABLE dbo.proposal_version_detail_quarter_weeks DROP CONSTRAINT DF_proposal_version_detail_quarter_weeks_open_market_impressions_total
END
GO

IF EXISTS(select 1
      from sys.all_columns c
      join sys.tables t on t.object_id = c.object_id
      join sys.schemas s on s.schema_id = t.schema_id
      join sys.default_constraints d on c.default_object_id = d.object_id
    where d.name = 'DF_proposal_version_detail_quarter_weeks_proprietary_cost_total')
BEGIN
	ALTER TABLE dbo.proposal_version_detail_quarter_weeks DROP CONSTRAINT DF_proposal_version_detail_quarter_weeks_proprietary_cost_total
END
GO

IF EXISTS(select 1
      from sys.all_columns c
      join sys.tables t on t.object_id = c.object_id
      join sys.schemas s on s.schema_id = t.schema_id
      join sys.default_constraints d on c.default_object_id = d.object_id
    where d.name = 'DF_proposal_version_detail_quarter_weeks_proprietary_impressions_total')
BEGIN
	ALTER TABLE dbo.proposal_version_detail_quarter_weeks DROP CONSTRAINT DF_proposal_version_detail_quarter_weeks_proprietary_impressions_total
END
GO

IF EXISTS(select 1
      from sys.all_columns c
      join sys.tables t on t.object_id = c.object_id
      join sys.schemas s on s.schema_id = t.schema_id
      join sys.default_constraints d on c.default_object_id = d.object_id
    where d.name = 'DF_schedule_detail_weeks_filled_spots')
BEGIN
	ALTER TABLE dbo.schedule_detail_weeks DROP CONSTRAINT DF_schedule_detail_weeks_filled_spots
END
GO

IF EXISTS(select 1
      from sys.all_columns c
      join sys.tables t on t.object_id = c.object_id
      join sys.schemas s on s.schema_id = t.schema_id
      join sys.default_constraints d on c.default_object_id = d.object_id
    where d.name = 'DF_schedules_inventory_source')
BEGIN
	ALTER TABLE dbo.schedules DROP CONSTRAINT DF_schedules_inventory_source
END
GO

IF EXISTS(select 1
      from sys.all_columns c
      join sys.tables t on t.object_id = c.object_id
      join sys.schemas s on s.schema_id = t.schema_id
      join sys.default_constraints d on c.default_object_id = d.object_id
    where d.name = 'DF_schedules_post_type')
BEGIN
	ALTER TABLE dbo.schedules DROP CONSTRAINT DF_schedules_post_type
END
GO

IF EXISTS(select 1
      from sys.all_columns c
      join sys.tables t on t.object_id = c.object_id
      join sys.schemas s on s.schema_id = t.schema_id
      join sys.default_constraints d on c.default_object_id = d.object_id
    where d.name = 'DF_stations_modified_date')
BEGIN
	ALTER TABLE dbo.stations DROP CONSTRAINT DF_stations_modified_date
END
GO
/*************************************** BCOP-1823 - END *****************************************************/

/*************************************** BCOP-1738 - END *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'cost_total'
              AND object_id = object_id('proposal_versions'))
BEGIN
	ALTER TABLE proposal_versions
	ADD cost_total MONEY NULL

	EXEC('UPDATE proposal_versions
		  SET cost_total = 0')
	
	ALTER TABLE proposal_versions
	ALTER COLUMN cost_total MONEY NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'impressions_total'
              AND object_id = object_id('proposal_versions'))
BEGIN
	ALTER TABLE proposal_versions
	ADD impressions_total BIGINT NULL

	EXEC('UPDATE proposal_versions
		  SET impressions_total = 0')

	ALTER TABLE proposal_versions
	ALTER COLUMN impressions_total BIGINT NOT NULL
END

/*************************************** BCOP-1738 - END *****************************************************/


-- BEGIN BCOP-1767 --
IF ((SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'bvs_post_details' AND COLUMN_NAME = 'delivery') != 'float')
BEGIN
	ALTER TABLE bvs_post_details ALTER COLUMN delivery float NOT NULL;
END
GO

IF ((SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'post_file_detail_impressions' AND COLUMN_NAME = 'impression') != 'float')
BEGIN
	ALTER TABLE post_file_detail_impressions ALTER COLUMN impression float NOT NULL;
END
GO

IF EXISTS(select name from sys.objects where type_desc = 'DEFAULT_CONSTRAINT' and name = 'DF_proposal_version_detail_quarter_weeks_open_market_impressions_total')
BEGIN
	ALTER TABLE proposal_version_detail_quarter_weeks DROP CONSTRAINT DF_proposal_version_detail_quarter_weeks_open_market_impressions_total;
END
GO

IF EXISTS(select name from sys.objects where type_desc = 'DEFAULT_CONSTRAINT' and name = 'DF_proposal_version_detail_quarter_weeks_proprietary_impressions_total')
BEGIN
	ALTER TABLE proposal_version_detail_quarter_weeks DROP CONSTRAINT DF_proposal_version_detail_quarter_weeks_proprietary_impressions_total;
END
GO

IF ((SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_version_detail_quarter_weeks' AND COLUMN_NAME = 'impressions') != 'float'
	OR (SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_version_detail_quarter_weeks' AND COLUMN_NAME = 'open_market_impressions_total') != 'float'
	OR (SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_version_detail_quarter_weeks' AND COLUMN_NAME = 'proprietary_impressions_total') != 'float')
BEGIN
	UPDATE proposal_version_detail_quarter_weeks SET impressions_goal = 0 where impressions_goal is null;
	ALTER TABLE proposal_version_detail_quarter_weeks ALTER COLUMN impressions float NOT NULL;
	ALTER TABLE proposal_version_detail_quarter_weeks ALTER COLUMN open_market_impressions_total float NOT NULL;
	ALTER TABLE proposal_version_detail_quarter_weeks ALTER COLUMN proprietary_impressions_total float NOT NULL;
END
GO

IF ((SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_version_detail_quarters' AND COLUMN_NAME = 'impressions_goal') != 'float')
BEGIN
	UPDATE proposal_version_detail_quarters SET impressions_goal = 0 where impressions_goal is null;
	ALTER TABLE proposal_version_detail_quarters ALTER COLUMN impressions_goal float NOT NULL;
END
GO

IF ((SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_version_details' AND COLUMN_NAME = 'impressions_total') != 'float'
	OR (SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_version_details' AND COLUMN_NAME = 'open_market_impressions_total') != 'float'
	OR (SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_version_details' AND COLUMN_NAME = 'proprietary_impressions_total') != 'float')
BEGIN
	UPDATE proposal_version_details SET impressions_total = 0 where impressions_total is null;
	ALTER TABLE proposal_version_details ALTER COLUMN impressions_total float NOT NULL;
	ALTER TABLE proposal_version_details ALTER COLUMN open_market_impressions_total float NOT NULL;
	ALTER TABLE proposal_version_details ALTER COLUMN proprietary_impressions_total float NOT NULL;
END
GO

IF ((SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_versions' AND COLUMN_NAME = 'target_impressions') != 'float'
OR (SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_versions' AND COLUMN_NAME = 'impressions_total') != 'float')
BEGIN
	UPDATE proposal_versions SET target_impressions = 0 where target_impressions is null;
	ALTER TABLE proposal_versions ALTER COLUMN target_impressions float NOT NULL;
	ALTER TABLE proposal_versions ALTER COLUMN impressions_total float NOT NULL;
END
GO

IF ((SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'schedule_detail_audiences' AND COLUMN_NAME = 'impressions') != 'float')
BEGIN
	ALTER TABLE schedule_detail_audiences ALTER COLUMN impressions float NOT NULL;
END
GO

IF ((SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'station_program_flight_audiences' AND COLUMN_NAME = 'impressions') != 'float'
OR (SELECT IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'station_program_flight_audiences' AND COLUMN_NAME = 'impressions') != 'FALSE')
BEGIN
	UPDATE station_program_flight_audiences SET impressions = 0 where impressions is null;
	ALTER TABLE station_program_flight_audiences ALTER COLUMN impressions float NOT NULL;
END
GO

IF ((SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'station_program_flight_proposal' AND COLUMN_NAME = 'impressions') != 'float')
BEGIN
	ALTER TABLE station_program_flight_proposal ALTER COLUMN impressions float NOT NULL;
END
GO

IF ((SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory_detail_slot_proposal' AND COLUMN_NAME = 'impressions') != 'float')
BEGIN
	ALTER TABLE inventory_detail_slot_proposal ALTER COLUMN impressions float NOT NULL;
END
GO
-- END BCOP-1767 --

/*************************************** BCOP-1693 - START ***************************************************/

IF (OBJECT_ID('FK_post_files_media_months', 'F') IS NULL)
BEGIN
    ALTER TABLE post_files   
	ADD CONSTRAINT FK_post_files_media_months 
	FOREIGN KEY (posting_book_id)     
    REFERENCES media_months (id) 
END
GO
IF (OBJECT_ID('FK_proposal_version_audiences_audiences', 'F') IS NULL)
BEGIN
    ALTER TABLE proposal_version_audiences   
	ADD CONSTRAINT FK_proposal_version_audiences_audiences
	FOREIGN KEY (audience_id)     
    REFERENCES audiences (id) 
END
GO
IF (OBJECT_ID('FK_proposal_version_detail_quarter_weeks_media_weeks', 'F') IS NULL)
BEGIN
    ALTER TABLE proposal_version_detail_quarter_weeks   
	ADD CONSTRAINT FK_proposal_version_detail_quarter_weeks_media_weeks
	FOREIGN KEY (media_week_id)     
    REFERENCES media_weeks (id) 
END   
GO
IF (OBJECT_ID('FK_proposal_version_details_spot_lengths', 'F') IS NULL)
BEGIN
    ALTER TABLE proposal_version_details   
	ADD CONSTRAINT FK_proposal_version_details_spot_lengths
	FOREIGN KEY (spot_length_id)     
    REFERENCES dbo.spot_lengths (id) 
END   
GO
IF (OBJECT_ID('FK_proposal_version_details_dayparts', 'F') IS NULL)
BEGIN
    ALTER TABLE proposal_version_details   
	ADD CONSTRAINT FK_proposal_version_details_dayparts
	FOREIGN KEY (daypart_id)     
    REFERENCES dbo.dayparts (id) 
END   
GO
--early development of single/hut/share books was using -1 as a placeholder for hut book instead of null
--this fixes that
UPDATE dbo.proposal_version_details
SET hut_posting_book_id = NULL, share_posting_book_id = NULL
WHERE single_posting_book_id IS NOT NULL
GO
IF (OBJECT_ID('FK_proposal_version_details_single_media_months', 'F') IS NULL)
BEGIN
    ALTER TABLE proposal_version_details   
	ADD CONSTRAINT FK_proposal_version_details_single_media_months
	FOREIGN KEY (single_posting_book_id)     
    REFERENCES dbo.media_months (id) 
END   
GO
IF (OBJECT_ID('FK_proposal_version_details_hut_media_months', 'F') IS NULL)
BEGIN
    ALTER TABLE proposal_version_details   
	ADD CONSTRAINT FK_proposal_version_details_hut_media_months
	FOREIGN KEY (hut_posting_book_id)     
    REFERENCES dbo.media_months (id) 
END  
GO
IF (OBJECT_ID('FK_proposal_version_details_share_media_months', 'F') IS NULL)
BEGIN
    ALTER TABLE proposal_version_details   
	ADD CONSTRAINT FK_proposal_version_details_share_media_months
	FOREIGN KEY (share_posting_book_id)     
    REFERENCES dbo.media_months (id) 
END  
GO
IF (OBJECT_ID('FK_proposal_version_flight_weeks_media_weeks', 'F') IS NULL)
BEGIN
    ALTER TABLE proposal_version_flight_weeks   
	ADD CONSTRAINT FK_proposal_version_flight_weeks_media_weeks
	FOREIGN KEY (media_week_id)     
    REFERENCES dbo.media_weeks (id) 
END 
GO
IF (OBJECT_ID('FK_proposal_version_spot_length_spot_lengths', 'F') IS NULL)
BEGIN
    ALTER TABLE proposal_version_spot_length   
	ADD CONSTRAINT FK_proposal_version_spot_length_spot_lengths
	FOREIGN KEY (spot_length_id)     
    REFERENCES dbo.spot_lengths (id) 
END 
GO
IF (OBJECT_ID('FK_proposals_proposal_versions', 'F') IS NOT NULL)
BEGIN
    ALTER TABLE proposals   
	DROP CONSTRAINT FK_proposals_proposal_versions
END 
GO
--data cleanup of invalid sweep book id values
UPDATE dbo.rate_files
SET sweep_book_id = NULL
WHERE sweep_book_id = 0
GO
IF (OBJECT_ID('FK_rate_files_media_months', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.rate_files   
	ADD CONSTRAINT FK_rate_files_media_months
	FOREIGN KEY (sweep_book_id)     
    REFERENCES dbo.media_months (id) 
END
GO
IF (OBJECT_ID('FK_rating_adjustments_media_months', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.rating_adjustments   
	ADD CONSTRAINT FK_rating_adjustments_media_months
	FOREIGN KEY (media_month_id)     
    REFERENCES dbo.media_months (id) 
END
GO
IF (OBJECT_ID('FK_schedule_audiences_audiences', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.schedule_audiences   
	ADD CONSTRAINT FK_schedule_audiences_audiences
	FOREIGN KEY (audience_id)     
    REFERENCES dbo.audiences (id) 
END
GO
IF (OBJECT_ID('FK_schedule_detail_audiences_audiences', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.schedule_detail_audiences
	ADD CONSTRAINT FK_schedule_detail_audiences_audiences
	FOREIGN KEY (audience_id)     
    REFERENCES dbo.audiences (id) 
END
GO
IF (OBJECT_ID('FK_schedule_detail_weeks_media_weeks', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.schedule_detail_weeks
	ADD CONSTRAINT FK_schedule_detail_weeks_media_weeks
	FOREIGN KEY (media_week_id)     
    REFERENCES dbo.media_weeks (id) 
END
GO
IF (OBJECT_ID('FK_schedule_details_spot_lengths', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.schedule_details
	ADD CONSTRAINT FK_schedule_details_spot_lengths
	FOREIGN KEY (spot_length_id)     
    REFERENCES dbo.spot_lengths (id) 
END
GO
UPDATE schedule_details
SET daypart_id = 1
WHERE daypart_id IN (SELECT DISTINCT sd.daypart_id 
						FROM dbo.schedule_details sd
						LEFT OUTER JOIN dbo.dayparts d ON sd.daypart_id = d.id
						WHERE d.id IS NULL
						AND sd.daypart_id IS NOT NULL)
GO
IF (OBJECT_ID('FK_schedule_details_dayparts', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.schedule_details
	ADD CONSTRAINT FK_schedule_details_dayparts
	FOREIGN KEY (daypart_id)     
    REFERENCES dbo.dayparts (id) 
END
GO
IF (OBJECT_ID('FK_schedule_restriction_dayparts_dayparts', 'F') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.schedule_restriction_dayparts
	DROP CONSTRAINT FK_schedule_restriction_dayparts_dayparts
END
GO
IF (OBJECT_ID('FK_schedules_media_months', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.schedules
	ADD CONSTRAINT FK_schedules_media_months
	FOREIGN KEY (posting_book_id)     
    REFERENCES dbo.media_months (id) 
END
GO
IF (OBJECT_ID('FK_spot_length_cost_multipliers_spot_lengths', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.spot_length_cost_multipliers
	ADD CONSTRAINT FK_spot_length_cost_multipliers_spot_lengths
	FOREIGN KEY (spot_length_id)     
    REFERENCES dbo.spot_lengths (id) 
END
GO
IF (OBJECT_ID('FK_station_program_flight_audiences_audiences', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.station_program_flight_audiences
	ADD CONSTRAINT FK_station_program_flight_audiences_audiences
	FOREIGN KEY (audience_id)     
    REFERENCES dbo.audiences (id) 
END
GO
IF (OBJECT_ID('FK_station_program_flights_media_weeks', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.station_program_flights
	ADD CONSTRAINT FK_station_program_flights_media_weeks
	FOREIGN KEY (media_week_id)     
    REFERENCES dbo.media_weeks (id) 
END
GO
IF (OBJECT_ID('FK_station_programs_dayparts', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.station_programs
	ADD CONSTRAINT FK_station_programs_dayparts
	FOREIGN KEY (daypart_id)     
    REFERENCES dbo.dayparts (id) 
END
GO
--UPDATE dbo.station_programs
--SET spot_length_id = 1
--WHERE spot_length_id = 0

IF (OBJECT_ID('FK_station_programs_spot_lengths', 'F') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.station_programs
	DROP CONSTRAINT FK_station_programs_spot_lengths
END
GO
IF (OBJECT_ID('FK_audience_audiences_audiences', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.audience_audiences
	ADD CONSTRAINT FK_audience_audiences_audiences
	FOREIGN KEY (custom_audience_id)     
    REFERENCES dbo.audiences (id) 
END
GO
IF (OBJECT_ID('FK_bvs_file_details_spot_lengths', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.bvs_file_details
	ADD CONSTRAINT FK_bvs_file_details_spot_lengths
	FOREIGN KEY (spot_length_id)     
    REFERENCES dbo.spot_lengths (id) 
END
GO
IF (OBJECT_ID('FK_bvs_post_details_audiences', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.bvs_post_details
	ADD CONSTRAINT FK_bvs_post_details_audiences
	FOREIGN KEY (audience_id)     
    REFERENCES dbo.audiences (id) 
END
GO
IF (OBJECT_ID('FK_inventory_detail_slot_components_dayparts', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.inventory_detail_slot_components
	ADD CONSTRAINT FK_inventory_detail_slot_components_dayparts
	FOREIGN KEY (daypart_id)     
    REFERENCES dbo.dayparts (id) 
END
GO
IF (OBJECT_ID('FK_inventory_detail_slot_components_stations', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.inventory_detail_slot_components
	ADD CONSTRAINT FK_inventory_detail_slot_components_stations
	FOREIGN KEY (station_code)     
    REFERENCES dbo.stations (station_code) 
END
GO
IF (OBJECT_ID('FK_inventory_detail_slots_spot_lengths', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.inventory_detail_slots
	ADD CONSTRAINT FK_inventory_detail_slots_spot_lengths
	FOREIGN KEY (spot_length_id)     
    REFERENCES dbo.spot_lengths (id) 
END
GO
IF (OBJECT_ID('FK_inventory_detail_slots_media_weeks', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.inventory_detail_slots
	ADD CONSTRAINT FK_inventory_detail_slots_media_weeks
	FOREIGN KEY (media_week_id)     
    REFERENCES dbo.media_weeks (id) 
END
GO
UPDATE dbo.inventory_detail_slots
SET rolled_up_daypart_id = 6
WHERE rolled_up_daypart_id = 0
GO
IF (OBJECT_ID('FK_inventory_detail_slots_dayparts', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.inventory_detail_slots
	ADD CONSTRAINT FK_inventory_detail_slots_dayparts
	FOREIGN KEY (rolled_up_daypart_id)     
    REFERENCES dbo.dayparts (id) 
END
GO
IF (OBJECT_ID('FK_media_weeks_media_months', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.media_weeks
	ADD CONSTRAINT FK_media_weeks_media_months
	FOREIGN KEY (media_month_id)     
    REFERENCES dbo.media_months (id) 
END
GO
IF (OBJECT_ID('FK_post_file_demos_audiences', 'F') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.post_file_demos
	DROP CONSTRAINT FK_post_file_demos_audiences
END
GO
IF (OBJECT_ID('FK_post_file_detail_impressions_audiences', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.post_file_detail_impressions
	ADD CONSTRAINT FK_post_file_detail_impressions_audiences
	FOREIGN KEY (demo)     
    REFERENCES dbo.audiences (id) 
END
GO
IF (OBJECT_ID('FK_post_file_details_spot_lengths', 'F') IS NULL)
BEGIN
    ALTER TABLE dbo.post_file_details
	ADD CONSTRAINT FK_post_file_details_spot_lengths
	FOREIGN KEY (spot_length_id)     
    REFERENCES dbo.spot_lengths (id) 
END
GO
/*************************************** BCOP-1693 - END *****************************************************/



/*************************************** END UPDATE SCRIPT *******************************************************/

------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '5.8.10' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '5.8.9' -- Previous release version
		OR [version] = '5.8.10') -- Current release version
	BEGIN
		PRINT 'Database Successfully Updated'
		COMMIT TRANSACTION
		DROP TABLE #previous_version
	END
	ELSE
	BEGIN
		PRINT 'Incorrect Previous Database Version'
		ROLLBACK TRANSACTION
	END

END
GO

IF(XACT_STATE() = -1)
BEGIN
	ROLLBACK TRANSACTION
	PRINT 'Database Update Failed. Transaction rolled back.'
END
GO







































