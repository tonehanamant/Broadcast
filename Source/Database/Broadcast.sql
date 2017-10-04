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

/*************************************** BCOP-1915 - START *****************************************************/

-- bvs_map_types
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'modified_by' AND Object_ID = Object_ID(N'[dbo].[bvs_map_types]'))
   and (SELECT COLUMNPROPERTY(OBJECT_ID(N'[dbo].[bvs_map_types]', 'U'), 'modified_by', 'AllowsNull')) = 1
begin    
	ALTER TABLE	[dbo].[bvs_map_types] ALTER COLUMN [modified_by] varchar(63) NOT NULL	
	update [dbo].[bvs_map_types] set modified_by = 'TAM01\vmoura' 
end

-- schedule_audiences
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'rank' AND Object_ID = Object_ID(N'[dbo].[schedule_audiences]'))
   and (SELECT COLUMNPROPERTY(OBJECT_ID(N'[dbo].[schedule_audiences]', 'U'), 'rank', 'AllowsNull')) = 1
begin
    update [dbo].[schedule_audiences] set rank = 0 where rank is null
	ALTER TABLE	[dbo].[schedule_audiences] ALTER COLUMN [rank] [int] NOT NULL		
end 


/*************************************** BCOP-1915 - END *****************************************************/

/*************************************** BCOP-1951 - START *****************************************************/

-- Removing proprietary inventory tables
IF OBJECT_ID('inventory_detail_slot_component_proposal', 'U') IS NOT NULL 
  DROP TABLE inventory_detail_slot_component_proposal
GO

IF OBJECT_ID('inventory_detail_slot_components', 'U') IS NOT NULL 
  DROP TABLE inventory_detail_slot_components
GO

IF OBJECT_ID('inventory_detail_slot_proposal', 'U') IS NOT NULL 
  DROP TABLE inventory_detail_slot_proposal
GO

IF OBJECT_ID('inventory_detail_slots', 'U') IS NOT NULL 
  DROP TABLE inventory_detail_slots
GO

IF OBJECT_ID('inventory_details', 'U') IS NOT NULL 
  DROP TABLE inventory_details
GO 

/*************************************** BCOP-1951 - END *****************************************************/







/*************************************** BCOP-1963 - START *****************************************************/

/*

[dbo].[station_program_flight_audiences]
[dbo].[station_program_flight_proposal]
[dbo].[station_program_flights]
[dbo].[station_program_genres]

[dbo].[station_programs]
FK_inventory_detail_slot_components_station_program_flights
	 for inventory_detail_slot_components
*/
GO
IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_inventory_detail_slot_components_station_program_flights')
BEGIN
	ALTER TABLE [dbo].[inventory_detail_slot_components] DROP CONSTRAINT [FK_inventory_detail_slot_components_station_program_flights]
END
GO
IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_programs_stations')
BEGIN
	ALTER TABLE [dbo].[station_programs] DROP CONSTRAINT [FK_station_programs_stations]
END
GO
IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_programs_rate_files')
BEGIN
	ALTER TABLE [dbo].[station_programs] DROP CONSTRAINT [FK_station_programs_rate_files]
END
GO

IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_programs_dayparts')
BEGIN
	ALTER TABLE [dbo].[station_programs] DROP CONSTRAINT [FK_station_programs_dayparts]
END
GO

IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_program_genres_station_programs')
BEGIN
	ALTER TABLE [dbo].[station_program_genres] DROP CONSTRAINT [FK_station_program_genres_station_programs]
END
GO

IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_program_genres_genres')
BEGIN
	ALTER TABLE [dbo].[station_program_genres] DROP CONSTRAINT [FK_station_program_genres_genres]
END
GO

IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_program_flights_station_programs')
BEGIN
	ALTER TABLE [dbo].[station_program_flights] DROP CONSTRAINT [FK_station_program_flights_station_programs]
END
GO

IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_program_flights_media_weeks')
BEGIN
	ALTER TABLE [dbo].[station_program_flights] DROP CONSTRAINT [FK_station_program_flights_media_weeks]
END
GO

IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_program_flight_proposal_proposal_version_detail_quarter_weeks')
BEGIN
	ALTER TABLE [dbo].[station_program_flight_proposal] DROP CONSTRAINT [FK_station_program_flight_proposal_proposal_version_detail_quarter_weeks]
END
GO

IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_program_flight_audiences_station_program_flights')
BEGIN
	ALTER TABLE [dbo].[station_program_flight_audiences] DROP CONSTRAINT [FK_station_program_flight_audiences_station_program_flights]
END
GO

IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_program_flight_audiences_audiences')
BEGIN
	ALTER TABLE [dbo].[station_program_flight_audiences] DROP CONSTRAINT [FK_station_program_flight_audiences_audiences]
END
GO




IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'station_programs'))
BEGIN
    DROP TABLE [dbo].[station_programs]
END
GO

/****** Object:  Table [dbo].[station_program_genres]    Script Date: 9/26/2017 10:50:50 AM ******/
IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'station_program_genres'))
BEGIN
    DROP TABLE [dbo].[station_program_genres]
END
GO

/****** Object:  Table [dbo].[station_program_flights]    Script Date: 9/26/2017 10:50:50 AM ******/
IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'station_program_flights'))
BEGIN
    DROP TABLE [dbo].[station_program_flights]
END
GO

/****** Object:  Table [dbo].[station_program_flight_proposal]    Script Date: 9/26/2017 10:50:50 AM ******/
IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'station_program_flight_proposal'))
BEGIN
    DROP TABLE [dbo].[station_program_flight_proposal]
END
GO

/****** Object:  Table [dbo].[station_program_flight_audiences]    Script Date: 9/26/2017 10:50:50 AM ******/
IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'station_program_flight_audiences'))
BEGIN
    DROP TABLE [dbo].[station_program_flight_audiences]
END
GO


/*************************************** BCOP-1963 - END *****************************************************/



/*************************************** BCOP-2014 - START *****************************************************/


IF EXIsTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE  CONSTRAINT_TYPE = 'PRIMARY KEY'
    AND TABLE_NAME = 'rate_files' 
    AND TABLE_SCHEMA ='dbo' 
	and CONSTRAINT_NAME = 'PK_rate_files')
BEGIN
		EXEC sp_rename 'rate_files.PK_rate_files', 'PK_inventory_files';
END

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'rate_source'
          AND Object_ID = Object_ID(N'dbo.rate_files'))
BEGIN
		EXEC sp_rename 'rate_files.rate_source', 'inventory_source';
END

IF EXISTS(SELECT *  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_rate_files_media_months')
BEGIN
		EXEC sp_rename 'FK_rate_files_media_months', 'FK_inventory_files_media_months';
END


IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_contacts_rate_files_created')
BEGIN
		EXEC sp_rename 'FK_station_contacts_rate_files_created', 'FK_station_contacts_inventory_files_created';
END

IF EXISTS(SELECT 1  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_station_contacts_rate_files_modified')
BEGIN
		EXEC sp_rename 'FK_station_contacts_rate_files_modified', 'FK_station_contacts_inventory_files_modified';
END

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'rate_files'))
BEGIN
	EXEC sp_rename 'rate_files', 'inventory_files';
END


/*************************************** BCOP-2014 - END *****************************************************/


/*************************************** END UPDATE SCRIPT *******************************************************/

------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '5.8.11' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '5.8.10' -- Previous release version
		OR [version] = '5.8.11') -- Current release version
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









































