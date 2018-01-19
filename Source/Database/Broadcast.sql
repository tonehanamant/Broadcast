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

/*************************************** BCOP-2024/2112 - START *****************************************************/
if Object_ID(N'[dbo].[station_inventory_manifest_generation]') is null
BEGIN
	CREATE TABLE [dbo].[station_inventory_manifest_generation](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[station_inventory_manifest_id] [int] NOT NULL,
		[media_week_id] [int] NOT NULL,
		CONSTRAINT [PK_station_inventory_manifest_generation] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


	ALTER TABLE [dbo].[station_inventory_manifest_generation]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_generation_station_inventory_manifest] FOREIGN KEY([station_inventory_manifest_id])
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_manifest_generation] CHECK CONSTRAINT [FK_station_inventory_manifest_generation_station_inventory_manifest]

	ALTER TABLE [dbo].[station_inventory_manifest_generation]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_generation_media_weeks] FOREIGN KEY([media_week_id])
	REFERENCES [dbo].[media_weeks] ([id])	

	ALTER TABLE [dbo].[station_inventory_manifest_generation] CHECK CONSTRAINT [FK_station_inventory_manifest_generation_media_weeks]
	
END
GO
/*************************************** BCOP-2024/2112 - END *****************************************************/

/*************************************** BCOP-1782/2097 - START *****************************************************/

IF NOT EXISTS(SELECT * FROM inventory_sources
			  WHERE name = 'TVB')
BEGIN
	SET IDENTITY_INSERT inventory_sources ON 

	INSERT INTO inventory_sources  (id, name, is_active, inventory_source_type)
	VALUES (3, 'TVB', 1, 1)

	SET IDENTITY_INSERT inventory_sources OFF
END

IF NOT EXISTS(SELECT * FROM inventory_sources
			  WHERE name = 'OpenMarket')
BEGIN
	SET IDENTITY_INSERT inventory_sources ON

	INSERT INTO inventory_sources  (id, name, is_active, inventory_source_type)
	VALUES (4, 'OpenMarket', 1, 1)

	SET IDENTITY_INSERT inventory_sources OFF
END

IF NOT EXISTS(SELECT * FROM inventory_sources
			  WHERE name = 'Assembly')
BEGIN
	SET IDENTITY_INSERT inventory_sources ON

	INSERT INTO inventory_sources  (id, name, is_active, inventory_source_type)
	VALUES (5, 'Assembly', 1, 1)

	SET IDENTITY_INSERT inventory_sources OFF
END

GO

IF (SELECT id FROM inventory_sources
	WHERE name = 'TTNW') = 1
BEGIN
	UPDATE inventory_sources
	SET name = 'TTNW'
	WHERE id = 4

	UPDATE inventory_sources
	SET name = 'OpenMarket'
	WHERE id = 1

	UPDATE station_inventory_group
	SET inventory_source_id = 4
	WHERE inventory_source_id = 1

	UPDATE station_inventory_manifest
	SET inventory_source_id = 4
	WHERE inventory_source_id = 1
END

GO

IF (SELECT id FROM inventory_sources
	WHERE name = 'CNN') = 2
BEGIN
	UPDATE inventory_sources
	SET name = 'CNN'
	WHERE id = 5

	UPDATE inventory_sources
	SET name = 'Assembly'
	WHERE id = 2

	UPDATE station_inventory_group
	SET inventory_source_id = 5
	WHERE inventory_source_id = 2

	UPDATE station_inventory_manifest
	SET inventory_source_id = 5
	WHERE inventory_source_id = 2
END

GO

IF NOT EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'inventory_files'
	AND COLUMN_NAME = 'inventory_source_id')
BEGIN
	ALTER TABLE inventory_files
	ADD inventory_source_id INT NULL

	EXEC('UPDATE inventory_files
		  SET inventory_source_id = inventory_source')

    ALTER TABLE inventory_files
	ALTER COLUMN inventory_source_id INT NOT NULL

	ALTER TABLE inventory_files
	DROP COLUMN inventory_source

	ALTER TABLE inventory_files
	ADD CONSTRAINT FK_inventory_files_inventory_source FOREIGN KEY (inventory_source_id)
	REFERENCES inventory_sources (id)
END

GO

/*************************************** BCOP-1782/2097 - END *****************************************************/

/*************************************** BCOP-2149 - START *****************************************************/

IF EXISTS (SELECT * FROM inventory_sources
		   WHERE name = 'Open Market')
BEGIN
	UPDATE inventory_sources
	SET name = 'OpenMarket'
	WHERE name = 'Open Market'
END

GO

/*************************************** BCOP-2149 - END *****************************************************/

/*************************************** BCOP-1945 - START *****************************************************/

-- Spots per week are not available for open market inventory
ALTER TABLE station_inventory_manifest
ALTER COLUMN spots_per_week int null
GO
-- Manifest groups not applicable to open market inventory
ALTER TABLE station_inventory_manifest
ALTER COLUMN station_inventory_group_id int null
GO
-- Updates to manifest audiences
ALTER TABLE station_inventory_manifest_audiences
ALTER COLUMN impressions float null
GO
IF NOT EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_manifest_audiences'
	AND COLUMN_NAME = 'rating')
BEGIN
	ALTER TABLE station_inventory_manifest_audiences 
	ADD rating float null
END
GO

-- Add program name
IF NOT EXISTS(SELECT *
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE TABLE_NAME = 'station_inventory_manifest_dayparts'
	AND COLUMN_NAME = 'program_name')
BEGIN
	ALTER TABLE station_inventory_manifest_dayparts
	ADD program_name varchar(63) null
END
GO

-- manifest rates table
if Object_ID(N'[dbo].[station_inventory_manifest_rates]') is null
BEGIN

	CREATE TABLE [dbo].[station_inventory_manifest_rates](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[station_inventory_manifest_id] [int] NOT NULL,
		[spot_length_id] [int] NOT NULL,
		[rate] [money] NOT NULL,
	 CONSTRAINT [PK_station_inventory_manifest_rates] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[station_inventory_manifest_rates]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_rates_spot_lengths] FOREIGN KEY([spot_length_id])
	REFERENCES [dbo].[spot_lengths] ([id])

	ALTER TABLE [dbo].[station_inventory_manifest_rates] CHECK CONSTRAINT [FK_station_inventory_manifest_rates_spot_lengths]

	ALTER TABLE [dbo].[station_inventory_manifest_rates]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_manifest_rates_station_inventory_manifest] FOREIGN KEY([station_inventory_manifest_id])
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_manifest_rates] CHECK CONSTRAINT [FK_station_inventory_manifest_rates_station_inventory_manifest]
END
GO
/*************************************** BCOP-1945 - END *****************************************************/


GO
/*************************************** BCOP-BCOP-2134 - START *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'is_reference'
          AND Object_ID = Object_ID(N'dbo.station_inventory_manifest_audiences'))
BEGIN
	ALTER TABLE station_inventory_manifest_audiences ADD is_reference bit;
	exec('update station_inventory_manifest_audiences SET is_reference = 1 WHERE is_reference is null')
	exec('ALTER TABLE station_inventory_manifest_audiences ADD CONSTRAINT DF_station_inventory_manifest_audiences_is_reference DEFAULT 0 FOR is_reference; ');
	exec('ALTER TABLE station_inventory_manifest_audiences ALTER COLUMN is_reference bit NOT NULL')
END
GO
/*************************************** BCOP-BCOP-2134 - END *****************************************************/



/*************************************** BCOP-2088 - START *****************************************************/



IF EXISTS (SELECT 1 FROM stations WHERE legacy_call_letters = 'DDTV' and station_code = '9255')
BEGIN
	DELETE FROM dbo.station_contacts
	WHERE station_code IN (
	9255
	,11062
	,2907
	,2014
	,1537
	,5297
	,5916
	,6860
	,6050
	,5512
	,5248
	,5865
	,6182
	,5972
	,5211
	,6063
	,9268
	,6045
	,7692
	,5247
	,5622
	,6067
	,6965
	,5312
	,5145
	,5501
	,5431
	,5630
	,6160
	,7124
	,9436
	,6151
	,2992
	,5223
	,7240
	,6835
	,5179
	,5930
	,5224
	,11179
	,5745
	,5301
	,7034
	,6910
	,8503
	,5958
	,8385
	,5492
	,8875
	,5030
	,5162
	,5136
	,5335
	,5532
	,5147
	,5300
	,5181
	,9357
	,6236
	,5625
	,5339
	,6936
	,5039
	,9192
	,7129
	,7022
	,6341
	,6821
	,5498
	,5453
	,5441
	,5399
	,6098
	,5668
	,8567
	,5258
	,5426
	,6931
	,6020
	,8163
	,5155
	,5487
	,7402
	,5186
	,5388
	,5142
	,6181
	,2377
	,9827
	,3793
	,4484
	,4896
	,4602
	,5705
	,5445
	,7558
	,6804
	,5107
	,5150
	,5368
	,5418
	,6040
	,6008
	,6914
	,5966
	,5596
	,5928
	,6758
	,5781
	,6721
	,5946
	,6311
	,5242
	,6730
	,6761
	,6755
	,5290
	,5334
	,5814
	,5100
	,6475
	,6810
	,5101
	,5486
	,5428
	,6890
	,5110
	,7796
	,5231
	,5711
	,6845
	,8112
	)
END
GO

UPDATE stations set station_code = '11923' WHERE legacy_call_letters = 'DDTV' and station_code = '9255'

UPDATE stations SET station_code = '9999' WHERE legacy_call_letters = 'GHLT' and station_code = '1537'
UPDATE stations set station_code = '1537' where legacy_call_letters = 'EHLT' and station_code = '11062'
--UPDATE stations set station_code = '11062' where legacy_call_letters = 'GHLT' and station_code = '1537'
UPDATE stations set station_code = '11062' where legacy_call_letters = 'GHLT' and station_code = '9999'

UPDATE stations set station_code = '907' where legacy_call_letters = 'ETTV' and station_code = '2907'
UPDATE stations set station_code = '2411' where legacy_call_letters = 'EXCW' and station_code = '2014'
UPDATE stations set station_code = '297' where legacy_call_letters = 'KAKE' and station_code = '5297'
UPDATE stations set station_code = '916' where legacy_call_letters = 'KAPP' and station_code = '5916'
UPDATE stations set station_code = '11802' where legacy_call_letters = 'KASA' and station_code = '6860'
UPDATE stations set station_code = '1050' where legacy_call_letters = 'KAWE' and station_code = '6050'
UPDATE stations set station_code = '512' where legacy_call_letters = 'KBHE' and station_code = '5512'
UPDATE stations set station_code = '248' where legacy_call_letters = 'KBJR' and station_code = '5248'
UPDATE stations set station_code = '865' where legacy_call_letters = 'KBME' and station_code = '5865'
UPDATE stations set station_code = '1182' where legacy_call_letters = 'KBMY' and station_code = '6182'
UPDATE stations set station_code = '972' where legacy_call_letters = 'KBTC' and station_code = '5972'
UPDATE stations set station_code = '211' where legacy_call_letters = 'KCEN' and station_code = '5211'
UPDATE stations set station_code = '1063' where legacy_call_letters = 'KCWC' and station_code = '6063'
UPDATE stations set station_code = '4268' where legacy_call_letters = 'KCWY' and station_code = '9268'
UPDATE stations set station_code = '1045' where legacy_call_letters = 'KCYU' and station_code = '6045'
UPDATE stations set station_code = '2685' where legacy_call_letters = 'KDEN' and station_code = '7692'

DELETE FROM stations WHERE legacy_call_letters = 'NDLH' AND station_code = '2769'
UPDATE stations set station_code = '2769' where legacy_call_letters = 'KDLH' and station_code = '5247'

UPDATE stations set station_code = '622' where legacy_call_letters = 'KDLT' and station_code = '5622'
UPDATE stations set station_code = '1067' where legacy_call_letters = 'KDRV' and station_code = '6067'
UPDATE stations set station_code = '1965' where legacy_call_letters = 'KDVR' and station_code = '6965'
UPDATE stations set station_code = '312' where legacy_call_letters = 'KECI' and station_code = '5312'
UPDATE stations set station_code = '145' where legacy_call_letters = 'KELO' and station_code = '5145'
UPDATE stations set station_code = '501' where legacy_call_letters = 'KETS' and station_code = '5501'
UPDATE stations set station_code = '431' where legacy_call_letters = 'KEVN' and station_code = '5431'
UPDATE stations set station_code = '630' where legacy_call_letters = 'KEZI' and station_code = '5630'
UPDATE stations set station_code = '1160' where legacy_call_letters = 'KFNB' and station_code = '6160'
UPDATE stations set station_code = '2124' where legacy_call_letters = 'KFPH' and station_code = '7124'
UPDATE stations set station_code = '4436' where legacy_call_letters = 'KFTU' and station_code = '9436'
UPDATE stations set station_code = '1151' where legacy_call_letters = 'KFXK' and station_code = '6151'
UPDATE stations set station_code = '2273' where legacy_call_letters = 'KFXL' and station_code = '2992'
UPDATE stations set station_code = '223' where legacy_call_letters = 'KFYR' and station_code = '5223'
UPDATE stations set station_code = '11756' where legacy_call_letters = 'KGMC' and station_code = '7240'
UPDATE stations set station_code = '1835' where legacy_call_letters = 'KGWC' and station_code = '6835'
UPDATE stations set station_code = '179' where legacy_call_letters = 'KGWN' and station_code = '5179'
UPDATE stations set station_code = '930' where legacy_call_letters = 'KHBS' and station_code = '5930'
UPDATE stations set station_code = '224' where legacy_call_letters = 'KHGI' and station_code = '5224'
UPDATE stations set station_code = '11184' where legacy_call_letters = 'KHME' and station_code = '11179'
UPDATE stations set station_code = '745' where legacy_call_letters = 'KIMA' and station_code = '5745'
UPDATE stations set station_code = '301' where legacy_call_letters = 'KLTV' and station_code = '5301'
UPDATE stations set station_code = '11801' where legacy_call_letters = 'KMSG' and station_code = '7034'
UPDATE stations set station_code = '1910' where legacy_call_letters = 'KMTR' and station_code = '6910'
UPDATE stations set station_code = '3503' where legacy_call_letters = 'KNBN' and station_code = '8503'
UPDATE stations set station_code = '958' where legacy_call_letters = 'KNDO' and station_code = '5958'
UPDATE stations set station_code = '3385' where legacy_call_letters = 'KNDX' and station_code = '8385'
UPDATE stations set station_code = '11781' where legacy_call_letters = 'KNOP' and station_code = '5492'
UPDATE stations set station_code = '2038' where legacy_call_letters = 'KNVA' and station_code = '8875'
UPDATE stations set station_code = '30' where legacy_call_letters = 'KOB' and station_code = '5030'
UPDATE stations set station_code = '162' where legacy_call_letters = 'KOBI' and station_code = '5162'
UPDATE stations set station_code = '136' where legacy_call_letters = 'KOLN' and station_code = '5136'
UPDATE stations set station_code = '335' where legacy_call_letters = 'KOTA' and station_code = '5335'
UPDATE stations set station_code = '532' where legacy_call_letters = 'KPAX' and station_code = '5532'
UPDATE stations set station_code = '147' where legacy_call_letters = 'KPNX' and station_code = '5147'
UPDATE stations set station_code = '300' where legacy_call_letters = 'KREX' and station_code = '5300'
UPDATE stations set station_code = '181' where legacy_call_letters = 'KRQE' and station_code = '5181'
UPDATE stations set station_code = '11804' where legacy_call_letters = 'KRTN' and station_code = '9357'
UPDATE stations set station_code = '1236' where legacy_call_letters = 'KSAS' and station_code = '6236'
UPDATE stations set station_code = '625' where legacy_call_letters = 'KSFY' and station_code = '5625'
UPDATE stations set station_code = '339' where legacy_call_letters = 'KSNW' and station_code = '5339'
UPDATE stations set station_code = '11836' where legacy_call_letters = 'KSPR' and station_code = '6936'
UPDATE stations set station_code = '39' where legacy_call_letters = 'KSTP' and station_code = '5039'
UPDATE stations set station_code = '11808' where legacy_call_letters = 'KTEL' and station_code = '9192'
UPDATE stations set station_code = '2129' where legacy_call_letters = 'KTFF' and station_code = '7129'
UPDATE stations set station_code = '2022' where legacy_call_letters = 'KTMF' and station_code = '7022'
UPDATE stations set station_code = '1341' where legacy_call_letters = 'KTTW' and station_code = '6341'
UPDATE stations set station_code = '821' where legacy_call_letters = 'KTVW' and station_code = '6821'
UPDATE stations set station_code = '498' where legacy_call_letters = 'KUAT' and station_code = '5498'
UPDATE stations set station_code = '453' where legacy_call_letters = 'KUID' and station_code = '5453'
UPDATE stations set station_code = '441' where legacy_call_letters = 'KULR' and station_code = '5441'
UPDATE stations set station_code = '399' where legacy_call_letters = 'KUON' and station_code = '5399'
UPDATE stations set station_code = '11803' where legacy_call_letters = 'KUPT' and station_code = '6098'
UPDATE stations set station_code = '668' where legacy_call_letters = 'KUSD' and station_code = '5668'
UPDATE stations set station_code = '3567' where legacy_call_letters = 'KUVE' and station_code = '8567'
UPDATE stations set station_code = '258' where legacy_call_letters = 'KVAL' and station_code = '5258'
UPDATE stations set station_code = '426' where legacy_call_letters = 'KVII' and station_code = '5426'
UPDATE stations set station_code = '1931' where legacy_call_letters = 'KVRR' and station_code = '6931'
UPDATE stations set station_code = '1020' where legacy_call_letters = 'KVTN' and station_code = '6020'
UPDATE stations set station_code = '3163' where legacy_call_letters = 'KWBQ' and station_code = '8163'
UPDATE stations set station_code = '155' where legacy_call_letters = 'KWCH' and station_code = '5155'
UPDATE stations set station_code = '487' where legacy_call_letters = 'KWES' and station_code = '5487'
UPDATE stations set station_code = '2402' where legacy_call_letters = 'KWYB' and station_code = '7402'
UPDATE stations set station_code = '186' where legacy_call_letters = 'KXLF' and station_code = '5186'
UPDATE stations set station_code = '11371' where legacy_call_letters = 'KXMA' and station_code = '5388'
UPDATE stations set station_code = '142' where legacy_call_letters = 'KXMC' and station_code = '5142'
UPDATE stations set station_code = '1181' where legacy_call_letters = 'KXXV' and station_code = '6181'
UPDATE stations set station_code = '11874' where legacy_call_letters = 'MXOF' and station_code = '2377'
UPDATE stations set station_code = '11381' where legacy_call_letters = 'NBJR' and station_code = '9827'
UPDATE stations set station_code = '793' where legacy_call_letters = 'NGWN' and station_code = '3793'
UPDATE stations set station_code = '2057' where legacy_call_letters = 'NHBS' and station_code = '4484'
UPDATE stations set station_code = '11875' where legacy_call_letters = 'NVYE' and station_code = '4896'
UPDATE stations set station_code = '602' where legacy_call_letters = 'TELD' and station_code = '4602'
UPDATE stations set station_code = '705' where legacy_call_letters = 'WAIQ' and station_code = '5705'
UPDATE stations set station_code = '445' where legacy_call_letters = 'WAOW' and station_code = '5445'
UPDATE stations set station_code = '2558' where legacy_call_letters = 'WBMA' and station_code = '7558'
UPDATE stations set station_code = '1804' where legacy_call_letters = 'WBPX' and station_code = '6804'
UPDATE stations set station_code = '107' where legacy_call_letters = 'WCCO' and station_code = '5107'
UPDATE stations set station_code = '150' where legacy_call_letters = 'WDAY' and station_code = '5150'
UPDATE stations set station_code = '368' where legacy_call_letters = 'WDIO' and station_code = '5368'
UPDATE stations set station_code = '418' where legacy_call_letters = 'WDSE' and station_code = '5418'
UPDATE stations set station_code = '1040' where legacy_call_letters = 'WEPX' and station_code = '6040'
UPDATE stations set station_code = '1008' where legacy_call_letters = 'WFQX' and station_code = '6008'
UPDATE stations set station_code = '1914' where legacy_call_letters = 'WFTC' and station_code = '6914'
UPDATE stations set station_code = '966' where legacy_call_letters = 'WFUT' and station_code = '5966'
UPDATE stations set station_code = '596' where legacy_call_letters = 'WFXI' and station_code = '5596'
UPDATE stations set station_code = '928' where legacy_call_letters = 'WGTU' and station_code = '5928'
UPDATE stations set station_code = '1758' where legacy_call_letters = 'WGVU' and station_code = '6758'
UPDATE stations set station_code = '781' where legacy_call_letters = 'WICS' and station_code = '5781'
UPDATE stations set station_code = '1721' where legacy_call_letters = 'WJPM' and station_code = '6721'
UPDATE stations set station_code = '946' where legacy_call_letters = 'WKLE' and station_code = '5946'
UPDATE stations set station_code = '1311' where legacy_call_letters = 'WLAX' and station_code = '6311'
UPDATE stations set station_code = '242' where legacy_call_letters = 'WMAB' and station_code = '5242'
UPDATE stations set station_code = '1730' where legacy_call_letters = 'WMPN' and station_code = '6730'
UPDATE stations set station_code = '1761' where legacy_call_letters = 'WNJN' and station_code = '6761'
UPDATE stations set station_code = '1755' where legacy_call_letters = 'WNJS' and station_code = '6755'
UPDATE stations set station_code = '290' where legacy_call_letters = 'WPBN' and station_code = '5290'
UPDATE stations set station_code = '334' where legacy_call_letters = 'WPTZ' and station_code = '5334'
UPDATE stations set station_code = '814' where legacy_call_letters = 'WPXW' and station_code = '5814'
UPDATE stations set station_code = '100' where legacy_call_letters = 'WRGB' and station_code = '5100'
UPDATE stations set station_code = '1475' where legacy_call_letters = 'WRPX' and station_code = '6475'
UPDATE stations set station_code = '1810' where legacy_call_letters = 'WRSP' and station_code = '6810'
UPDATE stations set station_code = '101' where legacy_call_letters = 'WSAZ' and station_code = '5101'
UPDATE stations set station_code = '486' where legacy_call_letters = 'WSIL' and station_code = '5486'
UPDATE stations set station_code = '428' where legacy_call_letters = 'WTEN' and station_code = '5428'
UPDATE stations set station_code = '1890' where legacy_call_letters = 'WTTO' and station_code = '6890'
UPDATE stations set station_code = '110' where legacy_call_letters = 'WTTV' and station_code = '5110'
UPDATE stations set station_code = '2594' where legacy_call_letters = 'WVUA' and station_code = '7796'
UPDATE stations set station_code = '231' where legacy_call_letters = 'WWTV' and station_code = '5231'
UPDATE stations set station_code = '711' where legacy_call_letters = 'WXOW' and station_code = '5711'
UPDATE stations set station_code = '12068' where legacy_call_letters = 'WXVT' and station_code = '6845'
UPDATE stations set station_code = '3112' where legacy_call_letters = 'WYFX' and station_code = '8112'
GO

IF NOT EXISTS(SELECT 1 FROM stations WHERE station_code = '2911' AND legacy_call_letters = 'DCMV')
BEGIN
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('2911','WCMV 27.2','PBS','140','DCMV','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('9900','WMAH-TV 19.2','PBS','346','DMAH','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11740','WTVS 56.2','PBS','105','DTVS','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10709','WDIV-TV 4.3','MET','105','EDIV','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('2800','WDSI-TV 61.2','IND','175','EDSI','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11386','WEEK-TV 25.2','ABC','275','EEEK','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11817','WFQX-TV 32.2','MET','140','EFQX','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11727','WHA-TV 21.4','PBS','269','EHA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11730','WHLA-TV 31.4','PBS','302','EHLA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10193','WHME-TV 46.2','ION','188','EHME','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11385','WHRO-TV 15.4','PBS','144','EHRO','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10137','WILM-LD 10.2','MET','150','EILM','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11864','WISC-TV 3.3','ION','269','EISC','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11074','WLFI-TV 18.3','ION','182','ELFI','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('8962','WMAO-TV 23.2','PBS','247','EMAO','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11872','WNED-TV 17.3','PBS','114','ENED','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11596','WNYF-CD 28.2','MET','149','ENYF','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11676','WPBI-LD 16.2','NBC','182','EPBI','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11728','WPNE-TV 38.4','PBS','258','EPNE','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('2595','WREG-TV 3.2','IND','240','EREG','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11040','WSPA-TV 7.3','ION','167','ESPA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11256','WUNC-TV 4.4','PBS','160','EUNC','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10457','WWAY 3.3','CW','150','EWAY','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4861','WZVN-TV 26.2','MET','171','EZVN','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11016','WAND 17.3','ION','248','GAND','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11725','WBNG-TV 12.3','MET','102','GBNG','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('1658','WCIV 36.3','MET','119','GCIV','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10359','WDAY-TV 6.3','IND','324','GDAY','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('1155','WDCW 50.2','IND','111','GDCW','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11940','WDRB 41.3','ION','129','GDRB','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11400','WEEK-TV 25.3','CW','275','GEEK','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('2314','WETK 33.3','PBS','123','GETK','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('1172','WGN-TV 9.2','IND','202','GGN','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11526','WHNT-TV 19.2','IND','291','GHNT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('12023','WICU-TV 12.3','ION','116','GICU','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4203','WITN-TV 7.3','MET','145','GITN','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('3144','WJTV 12.3','ION','318','GJTV','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('7912','WKMG-TV 6.3','IND','134','GKMG','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11771','WKNO 10.3','PBS','240','GKNO','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('9785','WMVS 10.3','PBS','217','GMVS','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('12024','WOGX 51.3','ION','192','GOGX','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11151','WOWT-TV 6.3','IND','252','GOWT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11585','WPTA 21.3','IND','109','GPTA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('7562','WPTO 14.3','PBS','115','GPTO','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('9572','WDJT-TV 58.4','IND','217','HDJT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11789','WKGB-TV 53.4','PBS','336','HKGB','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11480','WMTV 15.2','CW','269','HMTV','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11991','WNEM-TV 5.4','ION','113','HNEM','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('1168','WPIX 11.2','IND','101','HPIX','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('7168','WUAB 43.2','BOU','110','HUAB','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11907','WXGA-TV 8.4','PBS','161','HXGA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11812','WXXI-TV 21.4','PBS','138','HXXI','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('9911','WPTD 16.5','PBS','142','JPTD','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11805','KUPT 15.1','MET','390','JUPT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) 
	select '4201','KCJO-LD 30.1','CBS','238','KCJO','system',getdate()
	where not exists(select 1 from stations where station_code = '4201')
	
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) 
	select '1562','KHSV 21.1','IND','439','KHSV','system',getdate()
	where not exists(select 1 from stations where station_code = '1562')

	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) 
	select '8277','KMAS-LD 33.1','TEL','351','KMAS','system',getdate()
	where not exists(select 1 from stations where station_code = '8277')

	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10934','KMLU 11.1','MET','228','KMLU','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10447','KNPG-LD 21.1','NBC','238','KNPG','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('1534','KRTN-LD 39.1','MET','390','KRT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11871','KSWL-LD 17.1','CBS','243','KSWL','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('6680','KTEL-CD 15.3','TEL','390','KTL','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) 
	select '8859','KTPN-LD 48.1','IND','309','KTPN','system',getdate()
	where not exists(select 1 from stations where station_code = '8859')

	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10002','KUKL-TV 46.1','PBS','362','KUKL','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11588','KXJB-LD 30.1','CBS','324','KXJB','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('12042','KYCW-LD 25.1','CW','219','KYCW','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('9904','KETC 9.3','PBS','209','METC','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11961','KGTV 10.2','MET','425','MGTV','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11751','KHSC-LP 16.5','ETV','466','MHSC','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11794','KRTN-TV 2.1','TEL','390','MRTN','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('1548','KERA-TV 13.2','PBS','223','NERA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11185','NHME+','IND','364','NHME','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11752','KHSC-LP 16.6','AZA','466','NHSC','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11860','KJTV-TV 34.3','ION','251','NJTV','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('3069','KMOS-TV 6.2','PBS','204','NMOS','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('12025','KNPG-LD 21.2','CW','238','NNPG','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4527','KNTV 11.2','COZ','407','NNTV','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('2243','KOKI-TV 23.2','MET','271','NOKI','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4166','KOZJ 26.2','PBS','203','NOZJ','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4168','KOZK 21.2','PBS','219','NOZK','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11735','KPBS 15.3','PBS','425','NPBS','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11178','KQME 5.2','IND','364','NQME','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10141','KRBK 49.2','MET','219','NRBK','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11168','KRQE 13.2','FOX','390','NRQE','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10167','KTAB-TV 32.2','TEL','262','NTAB','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10430','KTAZ 39.2','I-S','353','NTAZ','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4721','KTEJ 19.2','PBS','334','NTEJ','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('9192','KTEL-TV 2.1','TEL','390','NTEL','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11815','KTTZ-TV 5.3','PBS','251','NTTZ','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('2744','KTXL 40.2','IND','462','NTXL','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4558','KUHT 8.2','PBS','218','NUHT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11990','KUIL-LD 12.6','ION','292','NUIL','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11407','KXJB-LD 30.2','CW','324','NXJB','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('5388','KXMA-TV 2.2','CBS','287','NXMA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('12034','KYMA-DT 11.2','ION','371','NYMA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11837','KAKM 7.4','PBS','343','OAKM','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11499','KALB-TV 5.3','CW','244','OALB','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10670','KASA-TV 15.1','MET','390','OASA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('9827','KBJR-TV 6.3','IND','276','OBJR','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('1446','KEET 13.3','PBS','402','OEET','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11161','KERA-TV 13.3','PBS','223','OERA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4950','OFYR+','MET','287','OFYR','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('8986','KLRU 18.3','PBS','235','OLRU','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('2664','KLVX 10.3','PBS','439','OLVX','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10660','KNCT 46.2','PBS','225','ONCT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11851','KNSD 39.20','TEL','425','ONSD','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('1305','KREN-TV 27.3','UMA','411','OREN','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11202','KSBW 8.3','ETV','428','OSBW','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('2017','KTCA-TV 2.4','PBS','213','OTCA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11806','KTEL-TV 33.1','IND','390','OTEL','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10419','KTTC 10.3','IND','211','OTTC','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('8587','KUAT-TV 6.2','PBS','389','OUAT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('1152','KUPT 33.1','IND','390','OUPT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11736','KAET 8.4','PBS','353','QAET','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11166','KASA-TV 33.1','IND','390','QASA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('9547','KCEN-TV 6.5','ION','225','QCEN','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4025','KETC 9.4','PBS','209','QETC','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11352','KMOS-TV 6.4','PBS','204','QMOS','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('2580','KOLD-TV 13.2','MET','389','QOLD','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11734','KPBS 15.4','PBS','425','QPBS','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11937','KPNE-TV 9.4','PBS','340','QPNE','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11953','KRBK 49.4','ION','219','QRBK','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11798','KTEL-TV 29.1','IND','390','QTEL','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('10999','KVPT 18.4','PBS','466','QVPT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11878','KWBQ 19.4','ION','390','QWBQ','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11800','KASA-TV 29.1','IND','390','RASA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11799','KRTN-TV 29.1','IND','390','RRTN','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('1156','KSWB-TV 69.2','IND','425','RSWB','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11792','KUPT 2.1','TEL','390','RUPT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('9357','KRTN-TV 15.1','MET','390','SRTN','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11797','KUPT-LD 16.3','IND','390','SUPT','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('8822','WBQC-LD 25.1','COZ','115','WBQC','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11704','WBTS-LD 8.1','NBC','106','WBTS','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date)
	Select '5733','WDVM 25.1','IND','111','WDVM','system',getdate()
	where not exists(select 1 from stations where station_code = '5733')

	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('7029','WGNM 45.1','IND','103','WGNM','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('6152','WGTA 32.1','MET','124','WGTA','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) 
	select '8312','WIFS 57.1','IND','269','WIFS','system',getdate()
	where not exists(select 1 from stations where station_code = '8312')

	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4368','WIWN 68.1','IND','217','WIWN','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('6700','WKBS-TV 47.1','IND','174','WKBS','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('6795','WLEF-TV 36.1','PBS','305','WLEF','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11157','WNYN-LD 39.1','AZA','101','WNYN','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11675','WPBI-LD 16.1','FOX','182','WPBI','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('4182','WRBJ-TV 34.1','IND','318','WRBJ','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('11718','WVNC-LD 45.1','NBC','149','WVNC','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date)
	select '8527','WVUA-CD 7.1','IND','230','WVU','system',getdate()
	where not exists(select 1 from stations where station_code = '8527')

	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) 
	select '5824','WXBU 15.1','GRT','166','WXBU','system',getdate()
	where not exists(select 1 from stations where station_code = '5824')

	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('7078','WYTU-LD 63.1','TEL','217','WYTU','system',getdate())
	INSERT INTO stations (station_code, station_call_letters, affiliation, market_code, legacy_call_letters, modified_by, modified_date) values ('5475','XEJ','I-S','365','XEJ','system',getdate())

END
GO


/*************************************** BCOP-2088 - END *****************************************************/

/*************************************** BCOP-2237 - START *****************************************************/
if not exists (
	select 1 from spot_length_cost_multipliers slcm 
	inner join spot_lengths sl on slcm.spot_length_id = sl.id
	where sl.length = 45)
begin
	print 'Adding 45s spot length cost multiplier'
	insert into spot_length_cost_multipliers (spot_length_id, cost_multiplier)
	select id, delivery_multiplier from spot_lengths where length = 45
end
go

if not exists (
	select 1 from spot_length_cost_multipliers slcm 
	inner join spot_lengths sl on slcm.spot_length_id = sl.id
	where sl.length = 180)
begin
	print 'Adding 180s spot length cost multiplier'
	insert into spot_length_cost_multipliers (spot_length_id, cost_multiplier)
	select id, delivery_multiplier from spot_lengths where length = 180
end
go

if not exists (
	select 1 from spot_length_cost_multipliers slcm 
	inner join spot_lengths sl on slcm.spot_length_id = sl.id
	where sl.length = 300)
begin
	print 'Adding 300s spot length cost multiplier'
	insert into spot_length_cost_multipliers (spot_length_id, cost_multiplier)
	select id, delivery_multiplier from spot_lengths where length = 300
end
go
/*************************************** BCOP-2237 - END *****************************************************/


GO
/*************************************** END UPDATE SCRIPT *******************************************************/

------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '5.8.12' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '5.8.11' -- Previous release version
		OR [version] = '5.8.12') -- Current release version
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
