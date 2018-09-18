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


/*************************************** START UPDATE SCRIPT *****************************************************/

/*************************************** BEGIN BCOP-3510 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.tables where object_id = OBJECT_ID('spot_tracker_files'))
BEGIN
	CREATE TABLE [spot_tracker_files](
	[id] [int] IDENTITY(1,1) NOT NULL,
		[file_name] [varchar](127) NOT NULL,
		[start_date] [date] NOT NULL,
		[end_date] [date] NOT NULL,
		[file_hash] [varchar](63) NOT NULL,
		[created_by] [varchar](63) NOT NULL,
		[created_date] [datetime] NOT NULL,
	 CONSTRAINT [PK_spot_tracker_files] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.tables where object_id = OBJECT_ID('spot_tracker_file_details'))
BEGIN
	CREATE TABLE [dbo].[spot_tracker_file_details](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[spot_tracker_file_id] [int] NOT NULL,
		[client] [varchar](15) NULL,
		[client_name] [varchar](63) NULL,
		[advertiser] [varchar](63) NULL,
		[release_name] [varchar](63),
		[isci] [varchar](63) NOT NULL,
		[spot_length_id] [int] NULL,
		[spot_length] [int] NOT NULL,
		[country] [varchar](63) NULL,
		[rank] [int] NULL,
		[market] [varchar](63),
		[market_code] [int] NULL,
		[station] [varchar](15) NOT NULL,
		[station_name] [varchar](64) NULL,
		[affiliate] [varchar](15) NULL,
		[date_aired] [date] NOT NULL,
		[day_of_week] [varchar](2),
		[daypart] [varchar](10),
		[time_aired] [int] NOT NULL,
		[program_name] [varchar](255),
		[encode_date] [date] NULL,
		[encode_time] [int] NULL,
		[rel_type] [varchar](15),
		[estimate_id] [int] NOT NULL,
		[identifier_2] [int] NULL,
		[identifier_3] [int] NULL,
		[sid] [int] NULL,
		[discid] [int] NULL	
	 CONSTRAINT [PK_spot_tracker_file_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[spot_tracker_file_details]  WITH CHECK ADD  CONSTRAINT [FK_spot_tracker_file_spot_tracker_file_details] FOREIGN KEY([spot_tracker_file_id])
	REFERENCES [dbo].[spot_tracker_files] ([id])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[spot_tracker_file_details] CHECK CONSTRAINT [FK_spot_tracker_file_spot_tracker_file_details]

	ALTER TABLE [dbo].[spot_tracker_file_details]  WITH CHECK ADD  CONSTRAINT [FK_spot_tracker_file_details_spot_lengths] FOREIGN KEY([spot_length_id])
	REFERENCES [dbo].[spot_lengths] ([id])

	ALTER TABLE [dbo].[spot_tracker_file_details] CHECK CONSTRAINT [FK_spot_tracker_file_details_spot_lengths]
END
/*************************************** END BCOP-3510 *****************************************************/


/*************************************** BCOP-3515 *****************************************************/

IF OBJECT_ID('proposal_version_detail_proprietary_pricing', 'U') IS NULL
BEGIN
	CREATE TABLE proposal_version_detail_proprietary_pricing
	(
		proposal_version_detail_id INT NOT NULL,
		inventory_source TINYINT NOT NULL,
		impressions_balance FLOAT NOT NULL,
		cpm MONEY NOT NULL,
		CONSTRAINT [PK_proposal_version_detail_proprietary_pricing] PRIMARY KEY CLUSTERED
		(
			proposal_version_detail_id, inventory_source ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]		
	)

	ALTER TABLE [dbo].[proposal_version_detail_proprietary_pricing]
	WITH CHECK ADD CONSTRAINT [FK_proposal_version_detail_proprietary_pricing_proposal_version_details] 
	FOREIGN KEY(proposal_version_detail_id)
	REFERENCES [dbo].[proposal_version_details] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[proposal_version_detail_proprietary_pricing] CHECK CONSTRAINT [FK_proposal_version_detail_proprietary_pricing_proposal_version_details]
END

/*************************************** BCOP-3515 - END *****************************************************/

/*************************************** START BCOP-3517 *****************************************************/
IF OBJECT_ID('station_inventory_spot_genres', 'U') IS NULL
BEGIN
	CREATE TABLE [station_inventory_spot_genres](
		id INT IDENTITY(1,1) NOT NULL,
		station_inventory_spot_id INT NOT NULL,
		genre_id INT NOT NULL
		CONSTRAINT [PK_station_inventory_spot_genres] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)

	ALTER TABLE [dbo].[station_inventory_spot_genres]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_spot_genres_station_inventory_spots] FOREIGN KEY([station_inventory_spot_id])
	REFERENCES [dbo].[station_inventory_spots] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[station_inventory_spot_genres] CHECK CONSTRAINT [FK_station_inventory_spot_genres_station_inventory_spots]

	ALTER TABLE [dbo].[station_inventory_spot_genres]  WITH CHECK ADD  CONSTRAINT [FK_station_inventory_spot_genres_genres] FOREIGN KEY([genre_id])
	REFERENCES [dbo].[genres] ([id])
	ALTER TABLE [dbo].[station_inventory_spot_genres] CHECK CONSTRAINT [FK_station_inventory_spot_genres_genres]

	CREATE INDEX IX_station_inventory_spot_genres_genre_id ON [station_inventory_spot_genres] ([genre_id]);
	CREATE INDEX IX_station_inventory_spot_genres_station_inventory_spot_id ON [station_inventory_spot_genres] ([station_inventory_spot_id]);
END
GO

/*************************************** END BCOP-3517 *****************************************************/

/*************************************** BCOP-3516 *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'open_market_cpm_min' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [proposal_version_details] 
	ADD open_market_cpm_min MONEY NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'open_market_cpm_max' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [proposal_version_details] 
	ADD open_market_cpm_max MONEY NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'open_market_unit_cap_per_station' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [proposal_version_details] 
	ADD open_market_unit_cap_per_station INT NULL
END


IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'open_market_cpm_target' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [proposal_version_details] 
	ADD open_market_cpm_target TINYINT NULL
END

/*************************************** BCOP-3516 - END *****************************************************/

/*************************************** BCOP-3561 *****************************************************/

IF OBJECT_ID('market_coverages', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[market_coverages]
	(  
		[id] int IDENTITY(1,1) NOT NULL,
		[rank] int NOT NULL,
		[market_code] smallint NOT NULL,
		[tv_homes] int NOT NULL,
		[percentage_of_us] float NOT NULL
		CONSTRAINT [PK_market_coverages] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)
	ALTER TABLE [dbo].[market_coverages]  WITH CHECK ADD  CONSTRAINT [FK_market_coverages_markets] 
	FOREIGN KEY([market_code]) REFERENCES [dbo].[markets] ([market_code])

	ALTER TABLE [dbo].[market_coverages] CHECK CONSTRAINT [FK_market_coverages_markets]

	CREATE INDEX IX_market_coverages_market_code ON [market_coverages] ([market_code])
END

/*************************************** BCOP-3561 - END *****************************************************/
-- BEGIN BCOP-3509
ALTER TABLE [dbo].[station_inventory_manifest_dayparts]
ALTER COLUMN [program_name] varchar(255) NULL
-- END BCOP-3509
/*************************************** START BCOP-3476 *****************************************************/

IF OBJECT_ID('proposal_buy_files', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[proposal_buy_files](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[file_name] [varchar](255) NOT NULL,
		[file_hash] [varchar](63) NOT NULL,
		[estimate_id] [int] NULL,
		[proposal_version_detail_id] [int] NOT NULL,
		[start_date] [date] NOT NULL,
		[end_date] [date] NOT NULL,
		[created_by] [varchar](63) NOT NULL,
		[created_date] [datetime] NOT NULL,
		CONSTRAINT [PK_proposal_buy_files] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)
	ALTER TABLE [dbo].[proposal_buy_files]  WITH CHECK ADD  CONSTRAINT [FK_proposal_buy_files_proposal_version_details] 
	FOREIGN KEY([proposal_version_detail_id]) REFERENCES [dbo].[proposal_version_details] ([id])

	ALTER TABLE [dbo].[proposal_buy_files] CHECK CONSTRAINT [FK_proposal_buy_files_proposal_version_details]
END
GO


IF OBJECT_ID('proposal_buy_file_details', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[proposal_buy_file_details](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[proposal_buy_file_id] [int] NOT NULL,
		[station_code] [smallint] NOT NULL,
		[total_spots] [int] NOT NULL,
		[spot_cost] [decimal](10, 2) NOT NULL,
		[total_cost] [decimal](10, 2) NOT NULL,
		[spot_length_id] [int] NOT NULL,
		[daypart_id] [int] NOT NULL,
		 CONSTRAINT [PK_proposal_buy_file_details] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)
	ALTER TABLE [dbo].[proposal_buy_file_details]  WITH CHECK ADD  CONSTRAINT [FK_proposal_buy_file_details_proposal_buy_files]
	FOREIGN KEY([proposal_buy_file_id]) REFERENCES [dbo].[proposal_buy_files] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[proposal_buy_file_details] CHECK CONSTRAINT [FK_proposal_buy_file_details_proposal_buy_files]

	ALTER TABLE [dbo].[proposal_buy_file_details]  WITH CHECK ADD  CONSTRAINT [FK_proposal_buy_file_details_stations]
	FOREIGN KEY([station_code]) REFERENCES [dbo].[stations] ([station_code])

	ALTER TABLE [dbo].[proposal_buy_file_details] CHECK CONSTRAINT [FK_proposal_buy_file_details_stations]

	ALTER TABLE [dbo].[proposal_buy_file_details]  WITH CHECK ADD  CONSTRAINT [FK_proposal_buy_file_details_spot_lengths]
	FOREIGN KEY([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])

	ALTER TABLE [dbo].[proposal_buy_file_details] CHECK CONSTRAINT [FK_proposal_buy_file_details_spot_lengths]

	ALTER TABLE [dbo].[proposal_buy_file_details]  WITH CHECK ADD  CONSTRAINT [FK_proposal_buy_file_details_dayparts]
	FOREIGN KEY([daypart_id]) REFERENCES [dbo].[dayparts] ([id])

END
GO

IF OBJECT_ID('proposal_buy_file_detail_weeks', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[proposal_buy_file_detail_weeks](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[proposal_buy_file_detail_id] [int] NOT NULL,
		[media_week_id] [int] NOT NULL,
		[spots] [int] NOT NULL,
		 CONSTRAINT [PK_proposal_buy_file_detail_weeks] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)

	ALTER TABLE [dbo].[proposal_buy_file_detail_weeks]  WITH CHECK ADD  CONSTRAINT [FK_proposal_buy_file_detail_weeks_proposal_buy_file_details]
	FOREIGN KEY([proposal_buy_file_detail_id]) REFERENCES [dbo].[proposal_buy_file_details] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[proposal_buy_file_detail_weeks] CHECK CONSTRAINT [FK_proposal_buy_file_detail_weeks_proposal_buy_file_details]

	ALTER TABLE [dbo].[proposal_buy_file_detail_weeks]  WITH CHECK ADD  CONSTRAINT [FK_proposal_buy_file_detail_weeks_media_weeks]
	FOREIGN KEY([media_week_id]) REFERENCES [dbo].[media_weeks] ([id])

	ALTER TABLE [dbo].[proposal_buy_file_detail_weeks] CHECK CONSTRAINT [FK_proposal_buy_file_detail_weeks_media_weeks]
END
GO

IF OBJECT_ID('proposal_buy_file_detail_audiences', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[proposal_buy_file_detail_audiences](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[proposal_buy_file_detail_id] [int] NOT NULL,
		[audience_id] [int] NOT NULL,
		[audience_rank] [int] NOT NULL,
		[audience_population] [int] NOT NULL,
		[impressions] [float] NOT NULL,
		 CONSTRAINT [PK_proposal_buy_file_detail_audiences] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)

	ALTER TABLE [dbo].[proposal_buy_file_detail_audiences]  WITH CHECK ADD  CONSTRAINT [FK_proposal_buy_file_detail_audiences_proposal_buy_file_details]
	FOREIGN KEY([proposal_buy_file_detail_id]) REFERENCES [dbo].[proposal_buy_file_details] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[proposal_buy_file_detail_audiences] CHECK CONSTRAINT [FK_proposal_buy_file_detail_audiences_proposal_buy_file_details]

	ALTER TABLE [dbo].[proposal_buy_file_detail_audiences]  WITH CHECK ADD  CONSTRAINT [FK_proposal_buy_file_detail_audiences_audiences]
	FOREIGN KEY([audience_id]) REFERENCES [dbo].[audiences] ([id])

	ALTER TABLE [dbo].[proposal_buy_file_detail_audiences] CHECK CONSTRAINT [FK_proposal_buy_file_detail_audiences_audiences]
END
GO

/*************************************** END BCOP-3476 *****************************************************/

/*************************************** BEGIN BCOP-3469 *****************************************************/
IF OBJECT_ID('[postlog_files]', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[postlog_files](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[file_name] [varchar](255) NOT NULL,
	[file_hash] [varchar](255) NOT NULL,
	[source_id] [int] NOT NULL,
	[created_date] [datetime] NOT NULL,
	[status] [int] NOT NULL,
	 CONSTRAINT [PK_postlog_files] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

IF OBJECT_ID('[postlog_file_problems]', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[postlog_file_problems](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[postlog_file_id] [int] NOT NULL,
	[problem_description] [nvarchar](max) NOT NULL,
	 CONSTRAINT [PK_postlog_file_problems] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	
	ALTER TABLE [dbo].[postlog_file_problems]  WITH CHECK ADD  CONSTRAINT [FK_postlog_file_problems_postlog_files] FOREIGN KEY([postlog_file_id])
	REFERENCES [dbo].[postlog_files] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[postlog_file_problems] CHECK CONSTRAINT [FK_postlog_file_problems_postlog_files]
	CREATE INDEX IX_postlog_file_problems_postlog_files ON [postlog_file_problems] ([postlog_file_id])
END
GO

IF OBJECT_ID('[postlog_file_details]', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[postlog_file_details](
		[id] [bigint] IDENTITY(1,1) NOT NULL,
		[postlog_file_id] [int] NOT NULL,
		[station] [varchar](15) NOT NULL,
		[original_air_date] [date] NOT NULL,
		[adjusted_air_date] [date] NOT NULL,
		[air_time] [int] NOT NULL,
		[spot_length_id] [int] NOT NULL,
		[isci] [varchar](63) NOT NULL,
		[program_name] [varchar](255) NULL,
		[genre] [varchar](255) NULL,
		[leadin_genre] [varchar](255) NULL,
		[leadin_program_name] [varchar](255) NULL,
		[leadout_genre] [varchar](255) NULL,
		[leadout_program_name] [varchar](255) NULL,
		[market] [varchar](63) NULL,
		[estimate_id] [int] NULL,
		[inventory_source] [int] NULL,
		[spot_cost] [float] NULL,
		[affiliate] [varchar](15) NULL,
		[leadin_end_time] [int] NULL,
		[leadout_start_time] [int] NULL,
		[program_show_type] [varchar](255) NULL,
		[leadin_show_type] [varchar](255) NULL,
		[leadout_show_type] [varchar](255) NULL,
		[archived] [bit] NOT NULL,
	 CONSTRAINT [PK_postlog_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[postlog_file_details]  WITH CHECK ADD  CONSTRAINT [FK_postlog_file_details_postlog_files] FOREIGN KEY([postlog_file_id])
	REFERENCES [dbo].[postlog_files] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[postlog_file_details] CHECK CONSTRAINT [FK_postlog_file_details_postlog_files]
	CREATE INDEX IX_postlog_file_details_postlog_files ON [postlog_file_details] ([postlog_file_id])
END
GO

IF OBJECT_ID('[postlog_file_detail_problems]', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[postlog_file_detail_problems](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[postlog_file_detail_id] [bigint] NOT NULL,
		[problem_type] [int] NOT NULL,
		[problem_description] [varchar](255) NULL,
	 CONSTRAINT [PK_postlog_file_detail_problems] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[postlog_file_detail_problems]  WITH CHECK ADD  CONSTRAINT [FK_postlog_file_detail_problems_postlog_file_details] FOREIGN KEY([postlog_file_detail_id])
	REFERENCES [dbo].[postlog_file_details] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[postlog_file_detail_problems] CHECK CONSTRAINT [FK_postlog_file_detail_problems_postlog_file_details]
	CREATE INDEX IX_postlog_file_detail_problems_postlog_file_details ON [postlog_file_detail_problems] ([postlog_file_detail_id])
END
GO

IF OBJECT_ID('[postlog_file_detail_demographics]', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[postlog_file_detail_demographics](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[audience_id] [int] NULL,
		[postlog_file_detail_id] [bigint] NULL,
		[overnight_rating] [float] NULL,
		[overnight_impressions] [float] NULL,
	 CONSTRAINT [PK_postlog_file_detail_demographics] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[postlog_file_detail_demographics]  WITH CHECK ADD  CONSTRAINT [FK_postlog_file_detail_demographics_postlog_file_details] FOREIGN KEY([postlog_file_detail_id])
	REFERENCES [dbo].[postlog_file_details] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[postlog_file_detail_demographics] CHECK CONSTRAINT [FK_postlog_file_detail_demographics_postlog_file_details]
	CREATE INDEX IX_postlog_file_detail_demographics_postlog_file_details ON [postlog_file_detail_demographics] ([postlog_file_detail_id])

	ALTER TABLE [dbo].[postlog_file_detail_demographics]  WITH CHECK ADD  CONSTRAINT [FK_postlog_file_detail_demographics_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[postlog_file_detail_demographics] CHECK CONSTRAINT [FK_postlog_file_detail_demographics_audiences]
	CREATE INDEX IX_postlog_file_detail_demographics_audiences ON [postlog_file_detail_demographics] ([audience_id])
END
GO
/*************************************** END BCOP-3469 *****************************************************/

/*************************************** START BCOP-3625 *****************************************************/
IF OBJECT_ID('[station_inventory_manifest_daypart_genres]', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[station_inventory_manifest_daypart_genres]
	(  
		[id] int IDENTITY(1,1) NOT NULL,
		[station_inventory_manifest_daypart_id] int NOT NULL,
		[genre_id] int NOT NULL,
		CONSTRAINT [PK_station_inventory_manifest_daypart_genres] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)
	ALTER TABLE [dbo].[station_inventory_manifest_daypart_genres]  WITH CHECK ADD CONSTRAINT [FK_station_inventory_manifest_daypart_genres_station_inventory_manifest_dayparts] 
	FOREIGN KEY([station_inventory_manifest_daypart_id]) REFERENCES [dbo].[station_inventory_manifest_dayparts] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[station_inventory_manifest_daypart_genres] CHECK CONSTRAINT [FK_station_inventory_manifest_daypart_genres_station_inventory_manifest_dayparts]
	
	ALTER TABLE [dbo].[station_inventory_manifest_daypart_genres]  WITH CHECK ADD CONSTRAINT [FK_station_inventory_manifest_daypart_genres_genres] 
	FOREIGN KEY([genre_id]) REFERENCES [genres] ([id])	
	ALTER TABLE [dbo].[station_inventory_manifest_daypart_genres] CHECK CONSTRAINT [FK_station_inventory_manifest_daypart_genres_genres]
	
	CREATE INDEX IX_station_inventory_manifest_daypart_genres_station_inventory_manifest_daypart_id ON [station_inventory_manifest_daypart_genres] ([station_inventory_manifest_daypart_id]);
	CREATE INDEX IX_station_inventory_manifest_daypart_genres_genre_id ON [station_inventory_manifest_daypart_genres] ([genre_id]);
END
GO

IF OBJECT_ID('[station_inventory_spot_genres]', 'U') IS NOT NULL
BEGIN
	DROP TABLE [station_inventory_spot_genres]
END
/*************************************** END BCOP-3625 *****************************************************/


/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '18.10.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '18.09.1' -- Previous release version
		OR [version] = '18.10.1') -- Current release version
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