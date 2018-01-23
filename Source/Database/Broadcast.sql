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

/*************************************** BCOP-2155 ***************************************************************/

IF OBJECT_ID('station_inventory_spots', 'U') IS NULL
BEGIN
	CREATE TABLE station_inventory_spots
	(
		id INT IDENTITY(1,1) NOT NULL,
		proposal_version_detail_quarter_week_id INT NULL,
		station_inventory_manifest_id INT NOT NULL,
		media_week_id INT NOT NULL,
		inventory_lost BIT NOT NULL,
		overridden_impressions FLOAT NULL,
		overridden_rate MONEY NULL,
		delivery_cpm MONEY NULL,
		CONSTRAINT [PK_station_inventory_spots] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)

	ALTER TABLE [dbo].[station_inventory_spots]  WITH CHECK ADD CONSTRAINT [FK_station_inventory_spots_proposal_version_detail_quarter_weeks] FOREIGN KEY(proposal_version_detail_quarter_week_id)
	REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_spots] CHECK CONSTRAINT [FK_station_inventory_spots_proposal_version_detail_quarter_weeks]

	ALTER TABLE [dbo].[station_inventory_spots]  WITH CHECK ADD CONSTRAINT [FK_station_inventory_spots_station_inventory_manifest] FOREIGN KEY(station_inventory_manifest_id)
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_spots] CHECK CONSTRAINT [FK_station_inventory_spots_station_inventory_manifest]
END

IF OBJECT_ID('station_inventory_spot_audiences', 'U') IS NULL
BEGIN
	CREATE TABLE station_inventory_spot_audiences
	(
		station_inventory_spot_id INT NOT NULL,
		audience_id INT NOT NULL,
		calculated_impressions FLOAT NULL,
		calculated_rate MONEY NULL,
		CONSTRAINT [PK_station_inventory_spot_audiences] PRIMARY KEY CLUSTERED
		(
		  station_inventory_spot_id ASC,
		  audience_id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)

	ALTER TABLE [dbo].[station_inventory_spot_audiences]  WITH CHECK ADD CONSTRAINT [FK_station_inventory_spot_audiences_station_inventory_spots] FOREIGN KEY(station_inventory_spot_id)
	REFERENCES [dbo].[station_inventory_spots] (id)
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_spot_audiences] CHECK CONSTRAINT [FK_station_inventory_spot_audiences_station_inventory_spots]

	ALTER TABLE [dbo].[station_inventory_spot_audiences]  WITH CHECK ADD CONSTRAINT [FK_station_inventory_spot_audiences_audiences] FOREIGN KEY(audience_id)
	REFERENCES [dbo].[audiences] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[station_inventory_spot_audiences] CHECK CONSTRAINT [FK_station_inventory_spot_audiences_audiences]
END

/*************************************** BCOP-2155 ***************************************************************/

/*************************************** BCOP-2261 ***************************************************************/

IF OBJECT_ID('proposal_version_detail_quarter_week_iscis', 'U') IS NULL
BEGIN
	CREATE TABLE proposal_version_detail_quarter_week_iscis
	(
		id INT IDENTITY(1,1) NOT NULL,
		proposal_version_detail_quarter_week_id INT NOT NULL,
		client_isci VARCHAR(63) NOT NULL,
		house_isci VARCHAR(63) NOT NULL,
		brand VARCHAR(127) NULL,
		married_house_iscii BIT NOT NULL,
		CONSTRAINT [PK_proposal_version_detail_quarter_week_iscis] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)

	ALTER TABLE [dbo].[proposal_version_detail_quarter_week_iscis]  WITH CHECK ADD CONSTRAINT [FK_proposal_version_detail_quarter_week_iscis_proposal_version_detail_quarter_weeks] FOREIGN KEY(proposal_version_detail_quarter_week_id)
	REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[proposal_version_detail_quarter_week_iscis] CHECK CONSTRAINT [FK_proposal_version_detail_quarter_week_iscis_proposal_version_detail_quarter_weeks]
END

/*************************************** BCOP-2261 - END ***************************************************************/






/******************** START BCOP-2316 *********************************************************************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = N'affidavit_files')
BEGIN
  
	CREATE TABLE [dbo].[affidavit_files](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[file_name] [VARCHAR](255) NOT NULL,
		[file_hash] [VARCHAR](255) NOT NULL,
		[source_id] [INT] NOT NULL,
		[created_date] [DATETIME] NOT NULL,
		[media_month_id] [INT] NOT NULL, --for partitioning and archiving
	 CONSTRAINT [PK_affidavit] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_NAME = N'affidavit_file_details')
BEGIN
  
	CREATE TABLE [dbo].[affidavit_file_details](
		[id] [BIGINT] IDENTITY(1,1) NOT NULL,
		[affidavit_file_id] [INT] NOT NULL,
		[station] [VARCHAR](15) NOT NULL,
		[original_air_date] [DATE] NOT NULL,
		[adjusted_air_date] [DATE] NOT NULL,
		[air_time] [INT] NOT NULL,
		[spot_length_id] [INT] NOT NULL,
		[isci] [VARCHAR](63) NOT NULL,
		[program_name] [VARCHAR](255) NULL,
		[genre] [varchar](255) NULL,
		[leadin_genre] [varchar](255) NULL,
		[leadin_program_name] [varchar](255) NULL,
		[leadout_genre] [varchar](255) NULL,
		[leadout_program_name] [varchar](255) NULL,

	 CONSTRAINT [PK_affidavit_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[affidavit_file_details]  WITH CHECK ADD  CONSTRAINT [FK_affidavit_files_affidavit_file_details] FOREIGN KEY([affidavit_file_id])
	REFERENCES [dbo].[affidavit_files] ([id])
	ON DELETE CASCADE


	CREATE NONCLUSTERED INDEX IX_affidavit_file_details_affidavit_id
		ON dbo.affidavit_file_details (affidavit_file_id)

END

GO


IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE name = 'affidavit_client_scrubs')
BEGIN
	CREATE TABLE [dbo].[affidavit_client_scrubs](
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[affidavit_file_detail_id] [BIGINT] NOT NULL,
		[proposal_version_detail_quarter_week_id] [INT] NOT NULL,
		[match_program] [BIT] NOT NULL,
		[match_genre] [BIT] NOT NULL,
		[match_market] [BIT] NOT NULL,
		[match_time] [BIT] NOT NULL,
		[status] [INT] NOT NULL,
		[comment] VARCHAR(1023) NULL,
		[modified_by] VARCHAR(255) NOT NULL,
		[modified_date] DATETIME 
	 CONSTRAINT [PK_affidavit_client_scrubs] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[affidavit_client_scrubs]  WITH CHECK ADD  CONSTRAINT [FK_affidavit_file_details_affidavit_client_scrubs] FOREIGN KEY([affidavit_file_detail_id])
	REFERENCES [dbo].[affidavit_file_details] ([id])
	ON DELETE CASCADE
	
	CREATE NONCLUSTERED INDEX IX_affidavit_client_scrubs_affidavit_file_detail_id
	ON [dbo].[affidavit_client_scrubs]  (affidavit_file_detail_id)


	ALTER TABLE [dbo].[affidavit_client_scrubs]  WITH CHECK ADD  CONSTRAINT [FK_proposal_version_detail_quarter_weeks_affidavit_client_scrubs] FOREIGN KEY([proposal_version_detail_quarter_week_id])
	REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id])
	ON DELETE CASCADE

	CREATE NONCLUSTERED INDEX IX_affidavit_client_scrubs_proposal_version_detail_quarter_week_id
	ON [dbo].[affidavit_client_scrubs]  (proposal_version_detail_quarter_week_id)

END
GO

/******************** END BCOP-2316 *********************************************************************************************/

/*************************************** BCOP-2320 - START ***************************************************************/

IF OBJECT_ID('affidavit_file_detail_audiences', 'U') IS NULL
BEGIN
	CREATE TABLE affidavit_file_detail_audiences
	(
		affidavit_file_detail_id BIGINT NOT NULL,
		audience_id INT NOT NULL,
		impressions FLOAT NOT NULL,
		CONSTRAINT [PK_affidavit_file_detail_audiences] PRIMARY KEY CLUSTERED
		(
			affidavit_file_detail_id, audience_id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)

	ALTER TABLE affidavit_file_detail_audiences WITH CHECK ADD CONSTRAINT FK_affidavit_file_detail_audiences_affidavit_file_details
	FOREIGN KEY (affidavit_file_detail_id)
	REFERENCES affidavit_file_details (id)
	ON DELETE CASCADE

	ALTER TABLE affidavit_file_detail_audiences CHECK CONSTRAINT FK_affidavit_file_detail_audiences_affidavit_file_details
	ALTER TABLE affidavit_file_detail_audiences WITH CHECK ADD CONSTRAINT FK_affidavit_file_detail_audiences_audiences
	FOREIGN KEY (audience_id)
	REFERENCES audiences (id)
	ON DELETE CASCADE

	ALTER TABLE affidavit_file_detail_audiences CHECK CONSTRAINT FK_affidavit_file_detail_audiences_audiences
END

/*************************************** BCOP-2320 - END ***************************************************************/


/**********************BEGIN BCOP2321 *******************************************************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'match_station'
          AND Object_ID = Object_ID(N'affidavit_client_scrubs'))
BEGIN
    alter table affidavit_client_scrubs add 
		match_station bit not null CONSTRAINT DF_match_station DEFAULT 0
WITH VALUES
END


/**********************END BCOP2321 *******************************************************************************************/


/*************************************** BCOP-2341 - START ***************************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
			  WHERE Name = N'lead_in' AND 
			  OBJECT_ID = OBJECT_ID(N'affidavit_client_scrubs'))
BEGIN
	ALTER TABLE affidavit_client_scrubs
	ADD lead_in BIT NULL
	
	EXEC('UPDATE affidavit_client_scrubs
		  SET lead_in = 0')

	ALTER TABLE affidavit_client_scrubs
	ALTER COLUMN lead_in BIT NOT NULL
END

/*************************************** BCOP-2341 - END ***************************************************************/

/*************************************** BCOP-2411/2456 - START ***************************************************************/

delete from proposal_version_detail_criteria_genres
where genre_id in
(select id from genres where modified_date < '2018-01-01')
go
delete from genres where modified_date < '2018-01-01'
go
if not exists(select 1 from genres)
begin
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Action', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Adult', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Adventure', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Anthology', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Auction', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Awards', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Beauty & Fashion', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Business', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Comedy', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Competition', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Crime', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Documentary', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Do It Yourself', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Drama', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Entertainment', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Family', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Fantasy', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Fitness & Exercise', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Food', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Game Show', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Health & Medicine', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('History', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Holiday', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Home Shopping', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Horror', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Informational', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Inspirational', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Interview', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Investigative', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Music', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Musical', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Mystery', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Nature', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('News', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Parade', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Performance', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Politics', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Profile', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Reality', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Religious', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Romance', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Science Fiction', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Science & Technology', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Sports', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Talk Show', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Telethon', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Thriller & Suspense', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Travel', 'System', current_timestamp, 'System', current_timestamp)
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Western', 'System', current_timestamp, 'System', current_timestamp)
end
go
/*************************************** BCOP-2411/2456 - END ***************************************************************/


/*************************************** BCOP-2320/2420 - START ***************************************************************/

IF OBJECT_ID('nsi_component_audiences', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[nsi_component_audiences] (
		[audience_id] [int] NOT NULL,
		[category_code] [tinyint] NOT NULL,
		[sub_category_code] [char](1) NOT NULL,
		[range_start] [int] NULL,
		[range_end] [int] NULL,
		[custom] [bit] NOT NULL,
		[code] [varchar](15) NOT NULL,
		[name] [varchar](127) NOT NULL,
		CONSTRAINT [PK_nsi_component_audiences] PRIMARY KEY CLUSTERED 
		(
			[audience_id] ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]


	ALTER TABLE [dbo].[nsi_component_audiences]  WITH CHECK ADD CONSTRAINT [FK_nsi_component_audiences_audiences] FOREIGN KEY(audience_id)
	REFERENCES [dbo].[audiences] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[nsi_component_audiences] CHECK CONSTRAINT [FK_nsi_component_audiences_audiences]
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (4, 0, N'F', 12, 14, 0, N'F12-14', N'Females 12-14')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (5, 0, N'F', 15, 17, 0, N'F15-17', N'Females 15-17')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (6, 0, N'F', 18, 20, 0, N'F18-20', N'Females 18-20')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (7, 0, N'F', 21, 24, 0, N'F21-24', N'Females 21-24')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (13, 0, N'F', 50, 54, 0, N'F50-54', N'Females 50-54')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (14, 0, N'F', 55, 64, 0, N'F55-64', N'Females 55-64')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (15, 0, N'F', 65, 99, 0, N'F65+', N'Females 65+')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (19, 0, N'M', 12, 14, 0, N'M12-14', N'Males 12-14')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name])
	VALUES (20, 0, N'M', 15, 17, 0, N'M15-17', N'Males 15-17')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (21, 0, N'M', 18, 20, 0, N'M18-20', N'Males 18-20')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (22, 0, N'M', 21, 24, 0, N'M21-24', N'Males 21-24')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (28, 0, N'M', 50, 54, 0, N'M50-54', N'Males 50-54')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name])
	VALUES (29, 0, N'M', 55, 64, 0, N'M55-64', N'Males 55-64')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (30, 0, N'M', 65, 99, 0, N'M65+', N'Males 65+')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name])
	VALUES (31, 0, N'H', 0, 99, 0, N'HH', N'House Holds')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (46, 0, N'K', 6, 11, 1, N'K6-11', N'Kids 6-11')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (284, 0, N'M', 25, 34, 1, N'M25-34', N'Males 25-34')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (290, 0, N'M', 35, 49, 1, N'M35-49', N'Males 35-49')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (339, 0, N'K', 2, 5, 1, N'K2-5', N'Kids 2-5')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (347, 0, N'F', 35, 49, 1, N'F35-49', N'Females 35-49')
	
	INSERT [dbo].[nsi_component_audiences] ([audience_id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) 
	VALUES (348, 0, N'F', 25, 34, 1, N'F25-34', N'Females 25-34')
END

/*************************************** BCOP-2320/2420 - START ***************************************************************/

------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '18.02.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '5.8.12' -- Previous release version
		OR [version] = '18.02.1') -- Current release version
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