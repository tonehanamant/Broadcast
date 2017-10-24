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

GO
-- should have been added the day this block of sql was added
DELETE inventory_files
where created_date < '10/4/2017'

GO
/*************************************** BCOP-2014 - END *****************************************************/

/*************************************** BCOP-1776/2000 - START *****************************************************/

--- inventory_sources ---
if Object_ID(N'[dbo].[inventory_sources]') is null
BEGIN
	CREATE TABLE [dbo].[inventory_sources](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[name] [varchar](50) NOT NULL,
		[is_active] [bit] NOT NULL,
		[inventory_source_type] [tinyint] NOT NULL,
		CONSTRAINT [PK_inventory_sources] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

END
GO
 -------

--- station_inventory_group ---
if Object_ID(N'[dbo].[station_inventory_group]') is null
BEGIN
	CREATE TABLE [dbo].[station_inventory_group](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[name] [varchar](50) NOT NULL,
		[daypart_code] [varchar](50) NOT NULL,
		[slot_number] [tinyint] NOT NULL,
		[inventory_source_id] [int] NOT NULL,
		CONSTRAINT [PK_station_inventory_group] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


	ALTER TABLE [dbo].[station_inventory_group]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_group_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_group] CHECK CONSTRAINT [FK_station_inventory_group_inventory_sources]

END
GO
 -------

 
-- station_inventory_manifest
if Object_ID(N'[dbo].[station_inventory_manifest]') is null
BEGIN
	CREATE TABLE [dbo].[station_inventory_manifest](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[station_code] [smallint]  NOT NULL,
		[spot_length_id] [int]  NOT NULL,
		[spots_per_week] [int]  NOT NULL,
		[effective_date] [date]  NOT NULL,
		[inventory_source_id] [int] NOT NULL,
		[station_inventory_group_id] [int] NOT NULL,
		[inventory_file_id] [int] NOT NULL,
		[spots_per_day] [int] NULL,
	 CONSTRAINT [PK_station_inventory_manifest] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[station_inventory_manifest]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_stations] FOREIGN KEY([station_code])
	REFERENCES [dbo].[stations] ([station_code])

	ALTER TABLE [dbo].[station_inventory_manifest] CHECK CONSTRAINT [FK_station_inventory_manifest_stations]


	ALTER TABLE [dbo].[station_inventory_manifest]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_spot_lengths] FOREIGN KEY([spot_length_id])
	REFERENCES [dbo].[spot_lengths] ([id])

	ALTER TABLE [dbo].[station_inventory_manifest] CHECK CONSTRAINT [FK_station_inventory_manifest_spot_lengths]


	ALTER TABLE [dbo].[station_inventory_manifest]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])

	ALTER TABLE [dbo].[station_inventory_manifest] CHECK CONSTRAINT [FK_station_inventory_manifest_inventory_sources]


	ALTER TABLE [dbo].[station_inventory_manifest]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_station_inventory_group] FOREIGN KEY([station_inventory_group_id])
	REFERENCES [dbo].[station_inventory_group] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_manifest] CHECK CONSTRAINT [FK_station_inventory_manifest_station_inventory_group]


	ALTER TABLE [dbo].[station_inventory_manifest]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_inventory_files] FOREIGN KEY([inventory_file_id])
	REFERENCES [dbo].[inventory_files] ([id])	

	ALTER TABLE [dbo].[station_inventory_manifest] CHECK CONSTRAINT [FK_station_inventory_manifest_inventory_files]


END
GO
----------------------

--- station_inventory_manifest_dayparts --
if Object_ID(N'[dbo].[station_inventory_manifest_dayparts]') is null
BEGIN
	CREATE TABLE [dbo].[station_inventory_manifest_dayparts](		
		[daypart_id] [int] NOT NULL,
		[station_inventory_manifest_id] [int] NOT NULL)

	ALTER TABLE [dbo].[station_inventory_manifest_dayparts]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_dayparts_station_inventory_manifest] FOREIGN KEY([station_inventory_manifest_id])
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_manifest_dayparts] CHECK CONSTRAINT [FK_station_inventory_manifest_dayparts_station_inventory_manifest]


	ALTER TABLE [dbo].[station_inventory_manifest_dayparts]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_dayparts_dayparts] FOREIGN KEY([daypart_id])
	REFERENCES [dbo].[dayparts] ([id])

	ALTER TABLE [dbo].[station_inventory_manifest_dayparts] CHECK CONSTRAINT [FK_station_inventory_manifest_dayparts_dayparts]

END
GO

-----------------

--- station_inventory_manifest_audiences --
if Object_ID(N'[dbo].[station_inventory_manifest_audiences]') is null
BEGIN
	CREATE TABLE [dbo].[station_inventory_manifest_audiences](				
		[station_inventory_manifest_id] [int] NOT NULL,
		[audience_id] [int] NOT NULL,
		[impressions] [float] NOT NULL,
		[rate] [money] NOT NULL
		)

	ALTER TABLE [dbo].[station_inventory_manifest_audiences]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_audiences_station_inventory_manifest] FOREIGN KEY([station_inventory_manifest_id])
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_manifest_audiences] CHECK CONSTRAINT [FK_station_inventory_manifest_audiences_station_inventory_manifest]


	ALTER TABLE [dbo].[station_inventory_manifest_audiences]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_audiences_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])

	ALTER TABLE [dbo].[station_inventory_manifest_audiences] CHECK CONSTRAINT [FK_station_inventory_manifest_audiences_audiences]

END
GO


/*************************************** BCOP-1776/2000 - END *****************************************************/

/*************************************** BCOP-1776/2043 - END *****************************************************/
if Object_ID(N'[dbo].[station_inventory_manifest_dayparts]') is not null and 
  not EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'id'
          AND Object_ID = Object_ID(N'dbo.station_inventory_manifest_dayparts'))
BEGIN
	ALTER TABLE [dbo].[station_inventory_manifest_dayparts] ADD id int not null identity (1,1)
	
	ALTER TABLE [dbo].[station_inventory_manifest_dayparts] ADD CONSTRAINT [PK_station_inventory_manifest_dayparts] PRIMARY KEY CLUSTERED (id)
END
GO

if Object_ID(N'[dbo].[station_inventory_manifest_audiences]') is not null and 
  not EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'id'
          AND Object_ID = Object_ID(N'dbo.station_inventory_manifest_audiences'))
BEGIN
  ALTER TABLE [dbo].[station_inventory_manifest_audiences] ADD id int not null identity (1,1)
  
  ALTER TABLE [dbo].[station_inventory_manifest_audiences] ADD CONSTRAINT [PK_station_inventory_manifest_audiences] PRIMARY KEY CLUSTERED (id) 
END
GO

/*************************************** BCOP-1776/2043 - END *****************************************************/

/*************************************** BCOP-1776/2100 - START *****************************************************/

IF NOT EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_group'
	AND COLUMN_NAME = 'start_date')
BEGIN
	ALTER TABLE [dbo].[station_inventory_group] ADD [start_date] DATE
END
GO

IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_group'
	AND COLUMN_NAME = 'start_date')
BEGIN
	UPDATE [dbo].[station_inventory_group]
	SET [start_date] = GETDATE()
	WHERE [start_date] is null
END
GO

IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_group'
	AND COLUMN_NAME = 'start_date')
BEGIN
	ALTER TABLE [dbo].[station_inventory_group] ALTER COLUMN [start_date] DATE NOT NULL
END
GO

IF NOT EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_group'
	AND COLUMN_NAME = 'end_date')
BEGIN
	ALTER TABLE [dbo].[station_inventory_group] ADD [end_date] DATE
END
GO

IF NOT EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_manifest'
	AND COLUMN_NAME = 'end_date')
BEGIN
	ALTER TABLE [dbo].[station_inventory_manifest] ADD [end_date] DATE 
END
GO

IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_manifest'
	AND COLUMN_NAME = 'end_date')
BEGIN
	UPDATE [dbo].[station_inventory_manifest]
	SET [end_date] = GETDATE()
	WHERE [end_date] is null
END
GO

IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_manifest'
	AND COLUMN_NAME = 'end_date')
BEGIN
	ALTER TABLE [station_inventory_manifest] ALTER COLUMN [end_date] DATE NOT NULL
END
GO

IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_manifest'
	AND COLUMN_NAME = 'inventory_file_id')
BEGIN	
	EXEC sp_rename 'dbo.station_inventory_manifest.inventory_file_id', 'file_id', 'COLUMN'
END
GO

IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_manifest'
	AND COLUMN_NAME = 'file_id')
BEGIN
	ALTER TABLE [station_inventory_manifest] ALTER COLUMN [file_id] INT NULL
END
GO

/*************************************** BCOP-1776/2100 - END *****************************************************/


/*************************************** BCOP-1782/2097 - START *****************************************************/

-- fix old data in case it exists
IF EXISTS(select 1 from [dbo].[inventory_sources] where name = 'TTNW' and inventory_source_type = 4)
begin
   UPDATE [dbo].[inventory_sources] 
      SET inventory_source_type = 1
   WHERE name = 'TTNW' AND inventory_source_type = 4
END ELSE BEGIN
  -- insert ttnw source
  IF NOT EXISTS(select 1 from [dbo].[inventory_sources] where name = 'TTNW' and inventory_source_type = 1)
  BEGIN
	  SET IDENTITY_INSERT [dbo].[inventory_sources] ON
	  INSERT INTO [dbo].[inventory_sources] (id, name, is_active, inventory_source_type) VALUES (1, 'TTNW', 1, 1)
	  SET IDENTITY_INSERT [dbo].[inventory_sources] OFF
  END
END
GO

-- fix old data
IF EXISTS(select 1 from [dbo].[inventory_sources] where name = 'CNN' and inventory_source_type = 5)
BEGIN
   UPDATE [dbo].[inventory_sources] 
      SET inventory_source_type = 1
   WHERE name = 'CNN' AND inventory_source_type = 5
END ELSE BEGIN
  -- insert cnn source
  IF NOT EXISTS(select 1 from [dbo].[inventory_sources] where name = 'CNN' and inventory_source_type = 1)
  BEGIN
	  SET IDENTITY_INSERT [dbo].[inventory_sources] ON
	  INSERT INTO [dbo].[inventory_sources] (id, name, is_active, inventory_source_type) VALUES (2, 'CNN', 1, 1)
	  SET IDENTITY_INSERT [dbo].[inventory_sources] OFF
  END
END
GO

/*************************************** BCOP-1782/2097 - END *****************************************************/

/*************************************** BCOP-1944/2107 - START *****************************************************/

IF EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_manifest'
	AND COLUMN_NAME = 'end_date')
BEGIN
	ALTER TABLE [station_inventory_manifest] ALTER COLUMN [end_date] DATE NULL
END
GO

/*************************************** BCOP-1944/2107 - end *****************************************************/


/*************************************** BCOP-1953 - START *****************************************************/

ALTER TABLE post_file_details
ALTER COLUMN detected_via VARCHAR(255) NULL

/*************************************** BCOP-1953 - END *****************************************************/

/*************************************** BCOP-2052 - START ***************************************************/

IF NOT EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'post_file_details'
	AND COLUMN_NAME = 'advertiser_out_of_spec_reason')
BEGIN
	ALTER TABLE post_file_details ADD advertiser_out_of_spec_reason VARCHAR(255) NULL
	
	EXEC('UPDATE post_file_details
	      SET advertiser_out_of_spec_reason = ''''')

	ALTER TABLE post_file_details ALTER COLUMN advertiser_out_of_spec_reason VARCHAR(255) NOT NULL
END
GO

/*************************************** BCOP-2052 - END *****************************************************/

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









































