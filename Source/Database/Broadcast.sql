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

/*************************************** BEGIN BCOP-3470 *****************************************************/
IF OBJECT_ID('[postlog_client_scrubs]', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[postlog_client_scrubs](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[postlog_file_detail_id] [bigint] NOT NULL,
		[proposal_version_detail_quarter_week_id] [int] NOT NULL,
		[lead_in] [bit] NOT NULL,
		[effective_program_name] [varchar](255) NULL,
		[effective_genre] [varchar](255) NULL,
		[effective_show_type] [varchar](255) NULL,			
		[effective_isci] [varchar](63) NULL,		
		[effective_client_isci] [varchar](63) NULL,
		[match_program] [bit] NOT NULL,
		[match_genre] [bit] NOT NULL,
		[match_market] [bit] NOT NULL,
		[match_time] [bit] NOT NULL,
		[match_station] [bit] NOT NULL,
		[match_isci_days] [bit] NOT NULL,
		[match_date] [bit] NULL,	
		[match_show_type] [bit] NOT NULL,	
		[match_isci] [bit] NOT NULL,
		[comment] [varchar](1023) NULL,	
		[modified_by] [varchar](255) NOT NULL,
		[modified_date] [datetime] NULL,
		[status] [int] NOT NULL,
		[status_override] [bit] NOT NULL,
	 CONSTRAINT [PK_postlog_client_scrubs] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[postlog_client_scrubs]  WITH CHECK ADD  CONSTRAINT [FK_postlog_file_details_postlog_client_scrubs] FOREIGN KEY([postlog_file_detail_id])
	REFERENCES [dbo].[postlog_file_details] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[postlog_client_scrubs] CHECK CONSTRAINT [FK_postlog_file_details_postlog_client_scrubs]
	CREATE INDEX IX_postlog_client_scrubs_postlog_file_detail_id ON [postlog_client_scrubs] ([postlog_file_detail_id])

	ALTER TABLE [dbo].[postlog_client_scrubs]  WITH CHECK ADD  CONSTRAINT [FK_proposal_version_detail_quarter_weeks_postlog_client_scrubs] FOREIGN KEY([proposal_version_detail_quarter_week_id])
	REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[postlog_client_scrubs] CHECK CONSTRAINT [FK_proposal_version_detail_quarter_weeks_postlog_client_scrubs]
	CREATE INDEX IX_postlog_client_scrubs_proposal_version_detail_quarter_week_id ON [postlog_client_scrubs] ([proposal_version_detail_quarter_week_id])
END

IF OBJECT_ID('[postlog_client_scrub_audiences]', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[postlog_client_scrub_audiences](
		[postlog_client_scrub_id] [int] NOT NULL,
		[audience_id] [int] NOT NULL,
		[impressions] [float] NOT NULL,
	 CONSTRAINT [PK_postlog_client_scrub_audiences] PRIMARY KEY CLUSTERED 
	(
		[postlog_client_scrub_id] ASC,
		[audience_id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[postlog_client_scrub_audiences]  WITH CHECK ADD  CONSTRAINT [FK_postlog_client_scrub_audiences_postlog_client_scrubs] FOREIGN KEY([postlog_client_scrub_id])
	REFERENCES [dbo].[postlog_client_scrubs] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[postlog_client_scrub_audiences] CHECK CONSTRAINT [FK_postlog_client_scrub_audiences_postlog_client_scrubs]
	CREATE INDEX IX_postlog_client_scrub_audiences_postlog_client_scrub_id ON [postlog_client_scrub_audiences] ([postlog_client_scrub_id])
	
	ALTER TABLE [dbo].[postlog_client_scrub_audiences]  WITH CHECK ADD  CONSTRAINT [FK_postlog_client_scrub_audiences_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])
	ALTER TABLE [dbo].[postlog_client_scrub_audiences] CHECK CONSTRAINT [FK_postlog_client_scrub_audiences_audiences]
	CREATE INDEX IX_postlog_client_scrub_audiences_audience_id ON [postlog_client_scrub_audiences] ([audience_id])
END

IF OBJECT_ID('[affidavit_blacklist]', 'U') IS NOT NULL
BEGIN
	EXEC sp_rename 'dbo.affidavit_blacklist', 'isci_blacklist'
END
/*************************************** END BCOP-3470 *****************************************************/






/**************************************************** START BCOP3512=>BCOP3608 **************************************************************/


IF (SELECT data_type FROM Information_Schema.Columns WHERE Table_Name = 'open_market_pricing_guide'
      AND Column_Name = 'blended_cpm' AND data_type = 'decimal') IS NOT NULL
BEGIN
	  drop table open_market_pricing_guide
END

IF OBJECT_ID('[open_market_pricing_guide]', 'U') IS NULL 
BEGIN
	CREATE TABLE [dbo].[open_market_pricing_guide](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[proposal_version_detail_id] int not null,
		[market_code] smallint not null,
		[station_code] smallint not null,
		[station_inventory_manifest_dayparts_id] int not null,
		[blended_cpm] money not null ,
		[spots] int not null,
		[impressions_per_spot] float not null,
		[impressions] float not null,
		[station_impressions] float not null ,
		[cost_per_spot] money not null,
		[cost] money not null
		CONSTRAINT [PK_open_market_pricing_guide] PRIMARY KEY CLUSTERED
		(
			[id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
END
GO


IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_open_market_pricing_guide_proposal_version_detail_id')
   AND parent_object_id = OBJECT_ID(N'dbo.open_market_pricing_guide'))
BEGIN

	ALTER TABLE [dbo].[open_market_pricing_guide]  
	ADD  CONSTRAINT [FK_open_market_pricing_guide_proposal_version_detail_id] 
		FOREIGN KEY([proposal_version_detail_id])
		REFERENCES [dbo].[proposal_version_details] ([id])
		ON DELETE CASCADE
	
	CREATE INDEX IX_open_market_pricing_guide_proposal_version_detail_id ON [open_market_pricing_guide] ([proposal_version_detail_id])

END

GO



IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_open_market_pricing_guide_market_code')
   AND parent_object_id = OBJECT_ID(N'dbo.open_market_pricing_guide'))
BEGIN

	ALTER TABLE [dbo].[open_market_pricing_guide]  
	ADD  CONSTRAINT [FK_open_market_pricing_guide_market_code] 
		FOREIGN KEY([market_code])
		REFERENCES [dbo].[markets] ([market_code])
		ON DELETE CASCADE
	
	CREATE INDEX IX_open_market_pricing_guide_market_code ON [open_market_pricing_guide] ([market_code])

END
GO

--FK_open_market_pricing_guide_station_code
--IX_open_market_pricing_guide_station_code

IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_open_market_pricing_guide_station_code')
   AND parent_object_id = OBJECT_ID(N'dbo.open_market_pricing_guide'))
BEGIN

	ALTER TABLE [dbo].[open_market_pricing_guide]  
	ADD  CONSTRAINT [FK_open_market_pricing_guide_station_code] 
		FOREIGN KEY([station_code])
		REFERENCES [dbo].[stations] ([station_code])
		ON DELETE CASCADE
	
	CREATE INDEX IX_open_market_pricing_guide_station_code ON [open_market_pricing_guide] ([station_code])

END
GO


--FK_open_market_pricing_guide_station_inventory_manifest_dayparts_id
--IX_open_market_pricing_guide_station_inventory_manifest_dayparts_id
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_open_market_pricing_guide_station_inventory_manifest_dayparts_id')
   AND parent_object_id = OBJECT_ID(N'dbo.open_market_pricing_guide'))
BEGIN

	ALTER TABLE [dbo].[open_market_pricing_guide]  
	ADD  CONSTRAINT [FK_open_market_pricing_guide_station_inventory_manifest_dayparts_id] 
		FOREIGN KEY([station_inventory_manifest_dayparts_id])
		REFERENCES [dbo].[station_inventory_manifest_dayparts] ([id])
		ON DELETE CASCADE
	
	CREATE INDEX IX_open_market_pricing_guide_station_inventory_manifest_dayparts_id ON [open_market_pricing_guide] ([station_inventory_manifest_dayparts_id])

END
GO

/**************************************************** END BCOP3512=>BCOP3608 **************************************************************/


/* START: Creation of MEDIA MONTHS and MEDIA WEEKS from 0121 to 0149 */
IF (SELECT COUNT(1) FROM dbo.media_months mm WHERE mm.media_month='0121') = 0
BEGIN
	SET IDENTITY_INSERT dbo.media_months ON

	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (472,2021,1,'0121','12/28/2020','01/31/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (473,2021,2,'0221','02/01/2021','02/28/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (474,2021,3,'0321','03/01/2021','03/28/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (475,2021,4,'0421','03/29/2021','04/25/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (476,2021,5,'0521','04/26/2021','05/30/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (477,2021,6,'0621','05/31/2021','06/27/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (478,2021,7,'0721','06/28/2021','07/25/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (479,2021,8,'0821','07/26/2021','08/29/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (480,2021,9,'0921','08/30/2021','09/26/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (481,2021,10,'1021','09/27/2021','10/31/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (482,2021,11,'1121','11/01/2021','11/28/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (483,2021,12,'1221','11/29/2021','12/26/2021')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (484,2022,1,'0122','12/27/2021','01/30/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (485,2022,2,'0222','01/31/2022','02/27/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (486,2022,3,'0322','02/28/2022','03/27/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (487,2022,4,'0422','03/28/2022','04/24/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (488,2022,5,'0522','04/25/2022','05/29/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (489,2022,6,'0622','05/30/2022','06/26/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (490,2022,7,'0722','06/27/2022','07/31/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (491,2022,8,'0822','08/01/2022','08/28/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (492,2022,9,'0922','08/29/2022','09/25/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (493,2022,10,'1022','09/26/2022','10/30/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (494,2022,11,'1122','10/31/2022','11/27/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (495,2022,12,'1222','11/28/2022','12/25/2022')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (496,2023,1,'0123','12/26/2022','01/29/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (497,2023,2,'0223','01/30/2023','02/26/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (498,2023,3,'0323','02/27/2023','03/26/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (499,2023,4,'0423','03/27/2023','04/30/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (500,2023,5,'0523','05/01/2023','05/28/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (501,2023,6,'0623','05/29/2023','06/25/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (502,2023,7,'0723','06/26/2023','07/30/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (503,2023,8,'0823','07/31/2023','08/27/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (504,2023,9,'0923','08/28/2023','09/24/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (505,2023,10,'1023','09/25/2023','10/29/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (506,2023,11,'1123','10/30/2023','11/26/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (507,2023,12,'1223','11/27/2023','12/31/2023')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (508,2024,1,'0124','01/01/2024','01/28/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (509,2024,2,'0224','01/29/2024','02/25/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (510,2024,3,'0324','02/26/2024','03/31/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (511,2024,4,'0424','04/01/2024','04/28/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (512,2024,5,'0524','04/29/2024','05/26/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (513,2024,6,'0624','05/27/2024','06/30/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (514,2024,7,'0724','07/01/2024','07/28/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (515,2024,8,'0824','07/29/2024','08/25/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (516,2024,9,'0924','08/26/2024','09/29/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (517,2024,10,'1024','09/30/2024','10/27/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (518,2024,11,'1124','10/28/2024','11/24/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (519,2024,12,'1224','11/25/2024','12/29/2024')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (520,2025,1,'0125','12/30/2024','01/26/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (521,2025,2,'0225','01/27/2025','02/23/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (522,2025,3,'0325','02/24/2025','03/30/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (523,2025,4,'0425','03/31/2025','04/27/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (524,2025,5,'0525','04/28/2025','05/25/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (525,2025,6,'0625','05/26/2025','06/29/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (526,2025,7,'0725','06/30/2025','07/27/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (527,2025,8,'0825','07/28/2025','08/31/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (528,2025,9,'0925','09/01/2025','09/28/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (529,2025,10,'1025','09/29/2025','10/26/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (530,2025,11,'1125','10/27/2025','11/30/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (531,2025,12,'1225','12/01/2025','12/28/2025')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (532,2026,1,'0126','12/29/2025','01/25/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (533,2026,2,'0226','01/26/2026','02/22/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (534,2026,3,'0326','02/23/2026','03/29/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (535,2026,4,'0426','03/30/2026','04/26/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (536,2026,5,'0526','04/27/2026','05/31/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (537,2026,6,'0626','06/01/2026','06/28/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (538,2026,7,'0726','06/29/2026','07/26/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (539,2026,8,'0826','07/27/2026','08/30/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (540,2026,9,'0926','08/31/2026','09/27/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (541,2026,10,'1026','09/28/2026','10/25/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (542,2026,11,'1126','10/26/2026','11/29/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (543,2026,12,'1226','11/30/2026','12/27/2026')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (544,2027,1,'0127','12/28/2026','01/31/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (545,2027,2,'0227','02/01/2027','02/28/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (546,2027,3,'0327','03/01/2027','03/28/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (547,2027,4,'0427','03/29/2027','04/25/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (548,2027,5,'0527','04/26/2027','05/30/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (549,2027,6,'0627','05/31/2027','06/27/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (550,2027,7,'0727','06/28/2027','07/25/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (551,2027,8,'0827','07/26/2027','08/29/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (552,2027,9,'0927','08/30/2027','09/26/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (553,2027,10,'1027','09/27/2027','10/31/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (554,2027,11,'1127','11/01/2027','11/28/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (555,2027,12,'1227','11/29/2027','12/26/2027')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (556,2028,1,'0128','12/27/2027','01/30/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (557,2028,2,'0228','01/31/2028','02/27/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (558,2028,3,'0328','02/28/2028','03/26/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (559,2028,4,'0428','03/27/2028','04/30/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (560,2028,5,'0528','05/01/2028','05/28/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (561,2028,6,'0628','05/29/2028','06/25/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (562,2028,7,'0728','06/26/2028','07/30/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (563,2028,8,'0828','07/31/2028','08/27/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (564,2028,9,'0928','08/28/2028','09/24/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (565,2028,10,'1028','09/25/2028','10/29/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (566,2028,11,'1128','10/30/2028','11/26/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (567,2028,12,'1228','11/27/2028','12/31/2028')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (568,2029,1,'0129','01/01/2029','01/28/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (569,2029,2,'0229','01/29/2029','02/25/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (570,2029,3,'0329','02/26/2029','03/25/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (571,2029,4,'0429','03/26/2029','04/29/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (572,2029,5,'0529','04/30/2029','05/27/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (573,2029,6,'0629','05/28/2029','06/24/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (574,2029,7,'0729','06/25/2029','07/29/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (575,2029,8,'0829','07/30/2029','08/26/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (576,2029,9,'0929','08/27/2029','09/30/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (577,2029,10,'1029','10/01/2029','10/28/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (578,2029,11,'1129','10/29/2029','11/25/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (579,2029,12,'1229','11/26/2029','12/30/2029')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (580,2030,1,'0130','12/31/2029','01/27/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (581,2030,2,'0230','01/28/2030','02/24/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (582,2030,3,'0330','02/25/2030','03/31/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (583,2030,4,'0430','04/01/2030','04/28/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (584,2030,5,'0530','04/29/2030','05/26/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (585,2030,6,'0630','05/27/2030','06/30/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (586,2030,7,'0730','07/01/2030','07/28/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (587,2030,8,'0830','07/29/2030','08/25/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (588,2030,9,'0930','08/26/2030','09/29/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (589,2030,10,'1030','09/30/2030','10/27/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (590,2030,11,'1130','10/28/2030','11/24/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (591,2030,12,'1230','11/25/2030','12/29/2030')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (592,2031,1,'0131','12/30/2030','01/26/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (593,2031,2,'0231','01/27/2031','02/23/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (594,2031,3,'0331','02/24/2031','03/30/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (595,2031,4,'0431','03/31/2031','04/27/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (596,2031,5,'0531','04/28/2031','05/25/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (597,2031,6,'0631','05/26/2031','06/29/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (598,2031,7,'0731','06/30/2031','07/27/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (599,2031,8,'0831','07/28/2031','08/31/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (600,2031,9,'0931','09/01/2031','09/28/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (601,2031,10,'1031','09/29/2031','10/26/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (602,2031,11,'1131','10/27/2031','11/30/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (603,2031,12,'1231','12/01/2031','12/28/2031')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (604,2032,1,'0132','12/29/2031','01/25/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (605,2032,2,'0232','01/26/2032','02/29/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (606,2032,3,'0332','03/01/2032','03/28/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (607,2032,4,'0432','03/29/2032','04/25/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (608,2032,5,'0532','04/26/2032','05/30/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (609,2032,6,'0632','05/31/2032','06/27/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (610,2032,7,'0732','06/28/2032','07/25/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (611,2032,8,'0832','07/26/2032','08/29/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (612,2032,9,'0932','08/30/2032','09/26/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (613,2032,10,'1032','09/27/2032','10/31/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (614,2032,11,'1132','11/01/2032','11/28/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (615,2032,12,'1232','11/29/2032','12/26/2032')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (616,2033,1,'0133','12/27/2032','01/30/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (617,2033,2,'0233','01/31/2033','02/27/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (618,2033,3,'0333','02/28/2033','03/27/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (619,2033,4,'0433','03/28/2033','04/24/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (620,2033,5,'0533','04/25/2033','05/29/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (621,2033,6,'0633','05/30/2033','06/26/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (622,2033,7,'0733','06/27/2033','07/31/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (623,2033,8,'0833','08/01/2033','08/28/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (624,2033,9,'0933','08/29/2033','09/25/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (625,2033,10,'1033','09/26/2033','10/30/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (626,2033,11,'1133','10/31/2033','11/27/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (627,2033,12,'1233','11/28/2033','12/25/2033')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (628,2034,1,'0134','12/26/2033','01/29/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (629,2034,2,'0234','01/30/2034','02/26/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (630,2034,3,'0334','02/27/2034','03/26/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (631,2034,4,'0434','03/27/2034','04/30/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (632,2034,5,'0534','05/01/2034','05/28/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (633,2034,6,'0634','05/29/2034','06/25/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (634,2034,7,'0734','06/26/2034','07/30/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (635,2034,8,'0834','07/31/2034','08/27/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (636,2034,9,'0934','08/28/2034','09/24/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (637,2034,10,'1034','09/25/2034','10/29/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (638,2034,11,'1134','10/30/2034','11/26/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (639,2034,12,'1234','11/27/2034','12/31/2034')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (640,2035,1,'0135','01/01/2035','01/28/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (641,2035,2,'0235','01/29/2035','02/25/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (642,2035,3,'0335','02/26/2035','03/25/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (643,2035,4,'0435','03/26/2035','04/29/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (644,2035,5,'0535','04/30/2035','05/27/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (645,2035,6,'0635','05/28/2035','06/24/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (646,2035,7,'0735','06/25/2035','07/29/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (647,2035,8,'0835','07/30/2035','08/26/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (648,2035,9,'0935','08/27/2035','09/30/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (649,2035,10,'1035','10/01/2035','10/28/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (650,2035,11,'1135','10/29/2035','11/25/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (651,2035,12,'1235','11/26/2035','12/30/2035')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (652,2036,1,'0136','12/31/2035','01/27/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (653,2036,2,'0236','01/28/2036','02/24/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (654,2036,3,'0336','02/25/2036','03/30/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (655,2036,4,'0436','03/31/2036','04/27/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (656,2036,5,'0536','04/28/2036','05/25/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (657,2036,6,'0636','05/26/2036','06/29/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (658,2036,7,'0736','06/30/2036','07/27/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (659,2036,8,'0836','07/28/2036','08/31/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (660,2036,9,'0936','09/01/2036','09/28/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (661,2036,10,'1036','09/29/2036','10/26/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (662,2036,11,'1136','10/27/2036','11/30/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (663,2036,12,'1236','12/01/2036','12/28/2036')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (664,2037,1,'0137','12/29/2036','01/25/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (665,2037,2,'0237','01/26/2037','02/22/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (666,2037,3,'0337','02/23/2037','03/29/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (667,2037,4,'0437','03/30/2037','04/26/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (668,2037,5,'0537','04/27/2037','05/31/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (669,2037,6,'0637','06/01/2037','06/28/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (670,2037,7,'0737','06/29/2037','07/26/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (671,2037,8,'0837','07/27/2037','08/30/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (672,2037,9,'0937','08/31/2037','09/27/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (673,2037,10,'1037','09/28/2037','10/25/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (674,2037,11,'1137','10/26/2037','11/29/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (675,2037,12,'1237','11/30/2037','12/27/2037')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (676,2038,1,'0138','12/28/2037','01/31/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (677,2038,2,'0238','02/01/2038','02/28/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (678,2038,3,'0338','03/01/2038','03/28/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (679,2038,4,'0438','03/29/2038','04/25/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (680,2038,5,'0538','04/26/2038','05/30/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (681,2038,6,'0638','05/31/2038','06/27/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (682,2038,7,'0738','06/28/2038','07/25/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (683,2038,8,'0838','07/26/2038','08/29/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (684,2038,9,'0938','08/30/2038','09/26/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (685,2038,10,'1038','09/27/2038','10/31/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (686,2038,11,'1138','11/01/2038','11/28/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (687,2038,12,'1238','11/29/2038','12/26/2038')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (688,2039,1,'0139','12/27/2038','01/30/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (689,2039,2,'0239','01/31/2039','02/27/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (690,2039,3,'0339','02/28/2039','03/27/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (691,2039,4,'0439','03/28/2039','04/24/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (692,2039,5,'0539','04/25/2039','05/29/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (693,2039,6,'0639','05/30/2039','06/26/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (694,2039,7,'0739','06/27/2039','07/31/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (695,2039,8,'0839','08/01/2039','08/28/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (696,2039,9,'0939','08/29/2039','09/25/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (697,2039,10,'1039','09/26/2039','10/30/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (698,2039,11,'1139','10/31/2039','11/27/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (699,2039,12,'1239','11/28/2039','12/25/2039')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (700,2040,1,'0140','12/26/2039','01/29/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (701,2040,2,'0240','01/30/2040','02/26/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (702,2040,3,'0340','02/27/2040','03/25/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (703,2040,4,'0440','03/26/2040','04/29/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (704,2040,5,'0540','04/30/2040','05/27/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (705,2040,6,'0640','05/28/2040','06/24/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (706,2040,7,'0740','06/25/2040','07/29/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (707,2040,8,'0840','07/30/2040','08/26/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (708,2040,9,'0940','08/27/2040','09/30/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (709,2040,10,'1040','10/01/2040','10/28/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (710,2040,11,'1140','10/29/2040','11/25/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (711,2040,12,'1240','11/26/2040','12/30/2040')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (712,2041,1,'0141','12/31/2040','01/27/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (713,2041,2,'0241','01/28/2041','02/24/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (714,2041,3,'0341','02/25/2041','03/31/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (715,2041,4,'0441','04/01/2041','04/28/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (716,2041,5,'0541','04/29/2041','05/26/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (717,2041,6,'0641','05/27/2041','06/30/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (718,2041,7,'0741','07/01/2041','07/28/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (719,2041,8,'0841','07/29/2041','08/25/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (720,2041,9,'0941','08/26/2041','09/29/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (721,2041,10,'1041','09/30/2041','10/27/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (722,2041,11,'1141','10/28/2041','11/24/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (723,2041,12,'1241','11/25/2041','12/29/2041')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (724,2042,1,'0142','12/30/2041','01/26/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (725,2042,2,'0242','01/27/2042','02/23/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (726,2042,3,'0342','02/24/2042','03/30/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (727,2042,4,'0442','03/31/2042','04/27/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (728,2042,5,'0542','04/28/2042','05/25/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (729,2042,6,'0642','05/26/2042','06/29/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (730,2042,7,'0742','06/30/2042','07/27/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (731,2042,8,'0842','07/28/2042','08/31/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (732,2042,9,'0942','09/01/2042','09/28/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (733,2042,10,'1042','09/29/2042','10/26/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (734,2042,11,'1142','10/27/2042','11/30/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (735,2042,12,'1242','12/01/2042','12/28/2042')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (736,2043,1,'0143','12/29/2042','01/25/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (737,2043,2,'0243','01/26/2043','02/22/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (738,2043,3,'0343','02/23/2043','03/29/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (739,2043,4,'0443','03/30/2043','04/26/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (740,2043,5,'0543','04/27/2043','05/31/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (741,2043,6,'0643','06/01/2043','06/28/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (742,2043,7,'0743','06/29/2043','07/26/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (743,2043,8,'0843','07/27/2043','08/30/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (744,2043,9,'0943','08/31/2043','09/27/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (745,2043,10,'1043','09/28/2043','10/25/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (746,2043,11,'1143','10/26/2043','11/29/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (747,2043,12,'1243','11/30/2043','12/27/2043')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (748,2044,1,'0144','12/28/2043','01/31/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (749,2044,2,'0244','02/01/2044','02/28/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (750,2044,3,'0344','02/29/2044','03/27/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (751,2044,4,'0444','03/28/2044','04/24/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (752,2044,5,'0544','04/25/2044','05/29/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (753,2044,6,'0644','05/30/2044','06/26/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (754,2044,7,'0744','06/27/2044','07/31/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (755,2044,8,'0844','08/01/2044','08/28/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (756,2044,9,'0944','08/29/2044','09/25/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (757,2044,10,'1044','09/26/2044','10/30/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (758,2044,11,'1144','10/31/2044','11/27/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (759,2044,12,'1244','11/28/2044','12/25/2044')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (760,2045,1,'0145','12/26/2044','01/29/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (761,2045,2,'0245','01/30/2045','02/26/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (762,2045,3,'0345','02/27/2045','03/26/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (763,2045,4,'0445','03/27/2045','04/30/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (764,2045,5,'0545','05/01/2045','05/28/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (765,2045,6,'0645','05/29/2045','06/25/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (766,2045,7,'0745','06/26/2045','07/30/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (767,2045,8,'0845','07/31/2045','08/27/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (768,2045,9,'0945','08/28/2045','09/24/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (769,2045,10,'1045','09/25/2045','10/29/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (770,2045,11,'1145','10/30/2045','11/26/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (771,2045,12,'1245','11/27/2045','12/31/2045')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (772,2046,1,'0146','01/01/2046','01/28/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (773,2046,2,'0246','01/29/2046','02/25/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (774,2046,3,'0346','02/26/2046','03/25/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (775,2046,4,'0446','03/26/2046','04/29/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (776,2046,5,'0546','04/30/2046','05/27/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (777,2046,6,'0646','05/28/2046','06/24/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (778,2046,7,'0746','06/25/2046','07/29/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (779,2046,8,'0846','07/30/2046','08/26/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (780,2046,9,'0946','08/27/2046','09/30/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (781,2046,10,'1046','10/01/2046','10/28/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (782,2046,11,'1146','10/29/2046','11/25/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (783,2046,12,'1246','11/26/2046','12/30/2046')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (784,2047,1,'0147','12/31/2046','01/27/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (785,2047,2,'0247','01/28/2047','02/24/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (786,2047,3,'0347','02/25/2047','03/31/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (787,2047,4,'0447','04/01/2047','04/28/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (788,2047,5,'0547','04/29/2047','05/26/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (789,2047,6,'0647','05/27/2047','06/30/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (790,2047,7,'0747','07/01/2047','07/28/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (791,2047,8,'0847','07/29/2047','08/25/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (792,2047,9,'0947','08/26/2047','09/29/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (793,2047,10,'1047','09/30/2047','10/27/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (794,2047,11,'1147','10/28/2047','11/24/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (795,2047,12,'1247','11/25/2047','12/29/2047')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (796,2048,1,'0148','12/30/2047','01/26/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (797,2048,2,'0248','01/27/2048','02/23/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (798,2048,3,'0348','02/24/2048','03/29/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (799,2048,4,'0448','03/30/2048','04/26/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (800,2048,5,'0548','04/27/2048','05/31/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (801,2048,6,'0648','06/01/2048','06/28/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (802,2048,7,'0748','06/29/2048','07/26/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (803,2048,8,'0848','07/27/2048','08/30/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (804,2048,9,'0948','08/31/2048','09/27/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (805,2048,10,'1048','09/28/2048','10/25/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (806,2048,11,'1148','10/26/2048','11/29/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (807,2048,12,'1248','11/30/2048','12/27/2048')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (808,2049,1,'0149','12/28/2048','01/31/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (809,2049,2,'0249','02/01/2049','02/28/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (810,2049,3,'0349','03/01/2049','03/28/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (811,2049,4,'0449','03/29/2049','04/25/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (812,2049,5,'0549','04/26/2049','05/30/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (813,2049,6,'0649','05/31/2049','06/27/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (814,2049,7,'0749','06/28/2049','07/25/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (815,2049,8,'0849','07/26/2049','08/29/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (816,2049,9,'0949','08/30/2049','09/26/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (817,2049,10,'1049','09/27/2049','10/31/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (818,2049,11,'1149','11/01/2049','11/28/2049')
	INSERT INTO dbo.media_months (id,[year],[month],media_month,start_date,end_date) VALUES (819,2049,12,'1249','11/29/2049','12/26/2049')

	SET IDENTITY_INSERT dbo.media_months OFF

	SET IDENTITY_INSERT dbo.media_weeks ON

	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (888,472,1,'12/28/2020','01/03/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (889,472,2,'01/04/2021','01/10/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (890,472,3,'01/11/2021','01/17/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (891,472,4,'01/18/2021','01/24/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (892,472,5,'01/25/2021','01/31/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (893,473,1,'02/01/2021','02/07/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (894,473,2,'02/08/2021','02/14/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (895,473,3,'02/15/2021','02/21/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (896,473,4,'02/22/2021','02/28/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (897,474,1,'03/01/2021','03/07/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (898,474,2,'03/08/2021','03/14/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (899,474,3,'03/15/2021','03/21/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (900,474,4,'03/22/2021','03/28/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (901,475,1,'03/29/2021','04/04/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (902,475,2,'04/05/2021','04/11/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (903,475,3,'04/12/2021','04/18/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (904,475,4,'04/19/2021','04/25/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (905,476,1,'04/26/2021','05/02/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (906,476,2,'05/03/2021','05/09/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (907,476,3,'05/10/2021','05/16/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (908,476,4,'05/17/2021','05/23/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (909,476,5,'05/24/2021','05/30/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (910,477,1,'05/31/2021','06/06/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (911,477,2,'06/07/2021','06/13/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (912,477,3,'06/14/2021','06/20/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (913,477,4,'06/21/2021','06/27/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (914,478,1,'06/28/2021','07/04/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (915,478,2,'07/05/2021','07/11/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (916,478,3,'07/12/2021','07/18/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (917,478,4,'07/19/2021','07/25/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (918,479,1,'07/26/2021','08/01/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (919,479,2,'08/02/2021','08/08/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (920,479,3,'08/09/2021','08/15/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (921,479,4,'08/16/2021','08/22/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (922,479,5,'08/23/2021','08/29/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (923,480,1,'08/30/2021','09/05/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (924,480,2,'09/06/2021','09/12/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (925,480,3,'09/13/2021','09/19/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (926,480,4,'09/20/2021','09/26/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (927,481,1,'09/27/2021','10/03/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (928,481,2,'10/04/2021','10/10/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (929,481,3,'10/11/2021','10/17/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (930,481,4,'10/18/2021','10/24/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (931,481,5,'10/25/2021','10/31/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (932,482,1,'11/01/2021','11/07/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (933,482,2,'11/08/2021','11/14/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (934,482,3,'11/15/2021','11/21/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (935,482,4,'11/22/2021','11/28/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (936,483,1,'11/29/2021','12/05/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (937,483,2,'12/06/2021','12/12/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (938,483,3,'12/13/2021','12/19/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (939,483,4,'12/20/2021','12/26/2021')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (940,484,1,'12/27/2021','01/02/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (941,484,2,'01/03/2022','01/09/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (942,484,3,'01/10/2022','01/16/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (943,484,4,'01/17/2022','01/23/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (944,484,5,'01/24/2022','01/30/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (945,485,1,'01/31/2022','02/06/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (946,485,2,'02/07/2022','02/13/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (947,485,3,'02/14/2022','02/20/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (948,485,4,'02/21/2022','02/27/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (949,486,1,'02/28/2022','03/06/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (950,486,2,'03/07/2022','03/13/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (951,486,3,'03/14/2022','03/20/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (952,486,4,'03/21/2022','03/27/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (953,487,1,'03/28/2022','04/03/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (954,487,2,'04/04/2022','04/10/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (955,487,3,'04/11/2022','04/17/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (956,487,4,'04/18/2022','04/24/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (957,488,1,'04/25/2022','05/01/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (958,488,2,'05/02/2022','05/08/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (959,488,3,'05/09/2022','05/15/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (960,488,4,'05/16/2022','05/22/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (961,488,5,'05/23/2022','05/29/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (962,489,1,'05/30/2022','06/05/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (963,489,2,'06/06/2022','06/12/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (964,489,3,'06/13/2022','06/19/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (965,489,4,'06/20/2022','06/26/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (966,490,1,'06/27/2022','07/03/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (967,490,2,'07/04/2022','07/10/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (968,490,3,'07/11/2022','07/17/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (969,490,4,'07/18/2022','07/24/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (970,490,5,'07/25/2022','07/31/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (971,491,1,'08/01/2022','08/07/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (972,491,2,'08/08/2022','08/14/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (973,491,3,'08/15/2022','08/21/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (974,491,4,'08/22/2022','08/28/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (975,492,1,'08/29/2022','09/04/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (976,492,2,'09/05/2022','09/11/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (977,492,3,'09/12/2022','09/18/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (978,492,4,'09/19/2022','09/25/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (979,493,1,'09/26/2022','10/02/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (980,493,2,'10/03/2022','10/09/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (981,493,3,'10/10/2022','10/16/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (982,493,4,'10/17/2022','10/23/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (983,493,5,'10/24/2022','10/30/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (984,494,1,'10/31/2022','11/06/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (985,494,2,'11/07/2022','11/13/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (986,494,3,'11/14/2022','11/20/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (987,494,4,'11/21/2022','11/27/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (988,495,1,'11/28/2022','12/04/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (989,495,2,'12/05/2022','12/11/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (990,495,3,'12/12/2022','12/18/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (991,495,4,'12/19/2022','12/25/2022')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (992,496,1,'12/26/2022','01/01/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (993,496,2,'01/02/2023','01/08/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (994,496,3,'01/09/2023','01/15/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (995,496,4,'01/16/2023','01/22/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (996,496,5,'01/23/2023','01/29/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (997,497,1,'01/30/2023','02/05/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (998,497,2,'02/06/2023','02/12/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (999,497,3,'02/13/2023','02/19/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1000,497,4,'02/20/2023','02/26/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1001,498,1,'02/27/2023','03/05/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1002,498,2,'03/06/2023','03/12/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1003,498,3,'03/13/2023','03/19/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1004,498,4,'03/20/2023','03/26/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1005,499,1,'03/27/2023','04/02/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1006,499,2,'04/03/2023','04/09/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1007,499,3,'04/10/2023','04/16/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1008,499,4,'04/17/2023','04/23/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1009,499,5,'04/24/2023','04/30/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1010,500,1,'05/01/2023','05/07/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1011,500,2,'05/08/2023','05/14/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1012,500,3,'05/15/2023','05/21/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1013,500,4,'05/22/2023','05/28/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1014,501,1,'05/29/2023','06/04/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1015,501,2,'06/05/2023','06/11/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1016,501,3,'06/12/2023','06/18/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1017,501,4,'06/19/2023','06/25/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1018,502,1,'06/26/2023','07/02/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1019,502,2,'07/03/2023','07/09/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1020,502,3,'07/10/2023','07/16/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1021,502,4,'07/17/2023','07/23/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1022,502,5,'07/24/2023','07/30/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1023,503,1,'07/31/2023','08/06/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1024,503,2,'08/07/2023','08/13/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1025,503,3,'08/14/2023','08/20/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1026,503,4,'08/21/2023','08/27/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1027,504,1,'08/28/2023','09/03/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1028,504,2,'09/04/2023','09/10/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1029,504,3,'09/11/2023','09/17/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1030,504,4,'09/18/2023','09/24/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1031,505,1,'09/25/2023','10/01/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1032,505,2,'10/02/2023','10/08/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1033,505,3,'10/09/2023','10/15/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1034,505,4,'10/16/2023','10/22/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1035,505,5,'10/23/2023','10/29/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1036,506,1,'10/30/2023','11/05/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1037,506,2,'11/06/2023','11/12/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1038,506,3,'11/13/2023','11/19/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1039,506,4,'11/20/2023','11/26/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1040,507,1,'11/27/2023','12/03/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1041,507,2,'12/04/2023','12/10/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1042,507,3,'12/11/2023','12/17/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1043,507,4,'12/18/2023','12/24/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1044,507,5,'12/25/2023','12/31/2023')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1045,508,1,'01/01/2024','01/07/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1046,508,2,'01/08/2024','01/14/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1047,508,3,'01/15/2024','01/21/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1048,508,4,'01/22/2024','01/28/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1049,509,1,'01/29/2024','02/04/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1050,509,2,'02/05/2024','02/11/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1051,509,3,'02/12/2024','02/18/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1052,509,4,'02/19/2024','02/25/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1053,510,1,'02/26/2024','03/03/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1054,510,2,'03/04/2024','03/10/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1055,510,3,'03/11/2024','03/17/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1056,510,4,'03/18/2024','03/24/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1057,510,5,'03/25/2024','03/31/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1058,511,1,'04/01/2024','04/07/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1059,511,2,'04/08/2024','04/14/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1060,511,3,'04/15/2024','04/21/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1061,511,4,'04/22/2024','04/28/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1062,512,1,'04/29/2024','05/05/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1063,512,2,'05/06/2024','05/12/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1064,512,3,'05/13/2024','05/19/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1065,512,4,'05/20/2024','05/26/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1066,513,1,'05/27/2024','06/02/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1067,513,2,'06/03/2024','06/09/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1068,513,3,'06/10/2024','06/16/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1069,513,4,'06/17/2024','06/23/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1070,513,5,'06/24/2024','06/30/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1071,514,1,'07/01/2024','07/07/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1072,514,2,'07/08/2024','07/14/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1073,514,3,'07/15/2024','07/21/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1074,514,4,'07/22/2024','07/28/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1075,515,1,'07/29/2024','08/04/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1076,515,2,'08/05/2024','08/11/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1077,515,3,'08/12/2024','08/18/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1078,515,4,'08/19/2024','08/25/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1079,516,1,'08/26/2024','09/01/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1080,516,2,'09/02/2024','09/08/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1081,516,3,'09/09/2024','09/15/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1082,516,4,'09/16/2024','09/22/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1083,516,5,'09/23/2024','09/29/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1084,517,1,'09/30/2024','10/06/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1085,517,2,'10/07/2024','10/13/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1086,517,3,'10/14/2024','10/20/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1087,517,4,'10/21/2024','10/27/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1088,518,1,'10/28/2024','11/03/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1089,518,2,'11/04/2024','11/10/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1090,518,3,'11/11/2024','11/17/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1091,518,4,'11/18/2024','11/24/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1092,519,1,'11/25/2024','12/01/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1093,519,2,'12/02/2024','12/08/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1094,519,3,'12/09/2024','12/15/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1095,519,4,'12/16/2024','12/22/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1096,519,5,'12/23/2024','12/29/2024')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1097,520,1,'12/30/2024','01/05/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1098,520,2,'01/06/2025','01/12/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1099,520,3,'01/13/2025','01/19/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1100,520,4,'01/20/2025','01/26/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1101,521,1,'01/27/2025','02/02/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1102,521,2,'02/03/2025','02/09/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1103,521,3,'02/10/2025','02/16/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1104,521,4,'02/17/2025','02/23/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1105,522,1,'02/24/2025','03/02/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1106,522,2,'03/03/2025','03/09/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1107,522,3,'03/10/2025','03/16/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1108,522,4,'03/17/2025','03/23/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1109,522,5,'03/24/2025','03/30/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1110,523,1,'03/31/2025','04/06/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1111,523,2,'04/07/2025','04/13/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1112,523,3,'04/14/2025','04/20/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1113,523,4,'04/21/2025','04/27/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1114,524,1,'04/28/2025','05/04/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1115,524,2,'05/05/2025','05/11/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1116,524,3,'05/12/2025','05/18/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1117,524,4,'05/19/2025','05/25/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1118,525,1,'05/26/2025','06/01/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1119,525,2,'06/02/2025','06/08/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1120,525,3,'06/09/2025','06/15/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1121,525,4,'06/16/2025','06/22/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1122,525,5,'06/23/2025','06/29/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1123,526,1,'06/30/2025','07/06/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1124,526,2,'07/07/2025','07/13/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1125,526,3,'07/14/2025','07/20/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1126,526,4,'07/21/2025','07/27/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1127,527,1,'07/28/2025','08/03/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1128,527,2,'08/04/2025','08/10/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1129,527,3,'08/11/2025','08/17/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1130,527,4,'08/18/2025','08/24/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1131,527,5,'08/25/2025','08/31/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1132,528,1,'09/01/2025','09/07/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1133,528,2,'09/08/2025','09/14/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1134,528,3,'09/15/2025','09/21/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1135,528,4,'09/22/2025','09/28/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1136,529,1,'09/29/2025','10/05/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1137,529,2,'10/06/2025','10/12/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1138,529,3,'10/13/2025','10/19/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1139,529,4,'10/20/2025','10/26/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1140,530,1,'10/27/2025','11/02/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1141,530,2,'11/03/2025','11/09/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1142,530,3,'11/10/2025','11/16/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1143,530,4,'11/17/2025','11/23/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1144,530,5,'11/24/2025','11/30/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1145,531,1,'12/01/2025','12/07/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1146,531,2,'12/08/2025','12/14/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1147,531,3,'12/15/2025','12/21/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1148,531,4,'12/22/2025','12/28/2025')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1149,532,1,'12/29/2025','01/04/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1150,532,2,'01/05/2026','01/11/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1151,532,3,'01/12/2026','01/18/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1152,532,4,'01/19/2026','01/25/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1153,533,1,'01/26/2026','02/01/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1154,533,2,'02/02/2026','02/08/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1155,533,3,'02/09/2026','02/15/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1156,533,4,'02/16/2026','02/22/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1157,534,1,'02/23/2026','03/01/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1158,534,2,'03/02/2026','03/08/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1159,534,3,'03/09/2026','03/15/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1160,534,4,'03/16/2026','03/22/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1161,534,5,'03/23/2026','03/29/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1162,535,1,'03/30/2026','04/05/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1163,535,2,'04/06/2026','04/12/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1164,535,3,'04/13/2026','04/19/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1165,535,4,'04/20/2026','04/26/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1166,536,1,'04/27/2026','05/03/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1167,536,2,'05/04/2026','05/10/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1168,536,3,'05/11/2026','05/17/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1169,536,4,'05/18/2026','05/24/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1170,536,5,'05/25/2026','05/31/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1171,537,1,'06/01/2026','06/07/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1172,537,2,'06/08/2026','06/14/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1173,537,3,'06/15/2026','06/21/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1174,537,4,'06/22/2026','06/28/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1175,538,1,'06/29/2026','07/05/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1176,538,2,'07/06/2026','07/12/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1177,538,3,'07/13/2026','07/19/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1178,538,4,'07/20/2026','07/26/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1179,539,1,'07/27/2026','08/02/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1180,539,2,'08/03/2026','08/09/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1181,539,3,'08/10/2026','08/16/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1182,539,4,'08/17/2026','08/23/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1183,539,5,'08/24/2026','08/30/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1184,540,1,'08/31/2026','09/06/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1185,540,2,'09/07/2026','09/13/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1186,540,3,'09/14/2026','09/20/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1187,540,4,'09/21/2026','09/27/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1188,541,1,'09/28/2026','10/04/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1189,541,2,'10/05/2026','10/11/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1190,541,3,'10/12/2026','10/18/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1191,541,4,'10/19/2026','10/25/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1192,542,1,'10/26/2026','11/01/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1193,542,2,'11/02/2026','11/08/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1194,542,3,'11/09/2026','11/15/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1195,542,4,'11/16/2026','11/22/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1196,542,5,'11/23/2026','11/29/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1197,543,1,'11/30/2026','12/06/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1198,543,2,'12/07/2026','12/13/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1199,543,3,'12/14/2026','12/20/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1200,543,4,'12/21/2026','12/27/2026')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1201,544,1,'12/28/2026','01/03/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1202,544,2,'01/04/2027','01/10/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1203,544,3,'01/11/2027','01/17/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1204,544,4,'01/18/2027','01/24/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1205,544,5,'01/25/2027','01/31/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1206,545,1,'02/01/2027','02/07/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1207,545,2,'02/08/2027','02/14/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1208,545,3,'02/15/2027','02/21/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1209,545,4,'02/22/2027','02/28/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1210,546,1,'03/01/2027','03/07/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1211,546,2,'03/08/2027','03/14/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1212,546,3,'03/15/2027','03/21/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1213,546,4,'03/22/2027','03/28/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1214,547,1,'03/29/2027','04/04/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1215,547,2,'04/05/2027','04/11/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1216,547,3,'04/12/2027','04/18/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1217,547,4,'04/19/2027','04/25/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1218,548,1,'04/26/2027','05/02/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1219,548,2,'05/03/2027','05/09/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1220,548,3,'05/10/2027','05/16/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1221,548,4,'05/17/2027','05/23/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1222,548,5,'05/24/2027','05/30/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1223,549,1,'05/31/2027','06/06/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1224,549,2,'06/07/2027','06/13/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1225,549,3,'06/14/2027','06/20/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1226,549,4,'06/21/2027','06/27/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1227,550,1,'06/28/2027','07/04/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1228,550,2,'07/05/2027','07/11/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1229,550,3,'07/12/2027','07/18/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1230,550,4,'07/19/2027','07/25/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1231,551,1,'07/26/2027','08/01/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1232,551,2,'08/02/2027','08/08/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1233,551,3,'08/09/2027','08/15/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1234,551,4,'08/16/2027','08/22/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1235,551,5,'08/23/2027','08/29/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1236,552,1,'08/30/2027','09/05/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1237,552,2,'09/06/2027','09/12/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1238,552,3,'09/13/2027','09/19/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1239,552,4,'09/20/2027','09/26/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1240,553,1,'09/27/2027','10/03/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1241,553,2,'10/04/2027','10/10/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1242,553,3,'10/11/2027','10/17/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1243,553,4,'10/18/2027','10/24/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1244,553,5,'10/25/2027','10/31/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1245,554,1,'11/01/2027','11/07/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1246,554,2,'11/08/2027','11/14/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1247,554,3,'11/15/2027','11/21/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1248,554,4,'11/22/2027','11/28/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1249,555,1,'11/29/2027','12/05/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1250,555,2,'12/06/2027','12/12/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1251,555,3,'12/13/2027','12/19/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1252,555,4,'12/20/2027','12/26/2027')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1253,556,1,'12/27/2027','01/02/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1254,556,2,'01/03/2028','01/09/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1255,556,3,'01/10/2028','01/16/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1256,556,4,'01/17/2028','01/23/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1257,556,5,'01/24/2028','01/30/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1258,557,1,'01/31/2028','02/06/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1259,557,2,'02/07/2028','02/13/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1260,557,3,'02/14/2028','02/20/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1261,557,4,'02/21/2028','02/27/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1262,558,1,'02/28/2028','03/05/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1263,558,2,'03/06/2028','03/12/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1264,558,3,'03/13/2028','03/19/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1265,558,4,'03/20/2028','03/26/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1266,559,1,'03/27/2028','04/02/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1267,559,2,'04/03/2028','04/09/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1268,559,3,'04/10/2028','04/16/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1269,559,4,'04/17/2028','04/23/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1270,559,5,'04/24/2028','04/30/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1271,560,1,'05/01/2028','05/07/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1272,560,2,'05/08/2028','05/14/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1273,560,3,'05/15/2028','05/21/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1274,560,4,'05/22/2028','05/28/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1275,561,1,'05/29/2028','06/04/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1276,561,2,'06/05/2028','06/11/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1277,561,3,'06/12/2028','06/18/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1278,561,4,'06/19/2028','06/25/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1279,562,1,'06/26/2028','07/02/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1280,562,2,'07/03/2028','07/09/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1281,562,3,'07/10/2028','07/16/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1282,562,4,'07/17/2028','07/23/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1283,562,5,'07/24/2028','07/30/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1284,563,1,'07/31/2028','08/06/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1285,563,2,'08/07/2028','08/13/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1286,563,3,'08/14/2028','08/20/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1287,563,4,'08/21/2028','08/27/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1288,564,1,'08/28/2028','09/03/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1289,564,2,'09/04/2028','09/10/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1290,564,3,'09/11/2028','09/17/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1291,564,4,'09/18/2028','09/24/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1292,565,1,'09/25/2028','10/01/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1293,565,2,'10/02/2028','10/08/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1294,565,3,'10/09/2028','10/15/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1295,565,4,'10/16/2028','10/22/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1296,565,5,'10/23/2028','10/29/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1297,566,1,'10/30/2028','11/05/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1298,566,2,'11/06/2028','11/12/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1299,566,3,'11/13/2028','11/19/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1300,566,4,'11/20/2028','11/26/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1301,567,1,'11/27/2028','12/03/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1302,567,2,'12/04/2028','12/10/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1303,567,3,'12/11/2028','12/17/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1304,567,4,'12/18/2028','12/24/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1305,567,5,'12/25/2028','12/31/2028')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1306,568,1,'01/01/2029','01/07/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1307,568,2,'01/08/2029','01/14/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1308,568,3,'01/15/2029','01/21/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1309,568,4,'01/22/2029','01/28/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1310,569,1,'01/29/2029','02/04/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1311,569,2,'02/05/2029','02/11/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1312,569,3,'02/12/2029','02/18/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1313,569,4,'02/19/2029','02/25/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1314,570,1,'02/26/2029','03/04/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1315,570,2,'03/05/2029','03/11/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1316,570,3,'03/12/2029','03/18/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1317,570,4,'03/19/2029','03/25/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1318,571,1,'03/26/2029','04/01/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1319,571,2,'04/02/2029','04/08/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1320,571,3,'04/09/2029','04/15/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1321,571,4,'04/16/2029','04/22/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1322,571,5,'04/23/2029','04/29/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1323,572,1,'04/30/2029','05/06/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1324,572,2,'05/07/2029','05/13/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1325,572,3,'05/14/2029','05/20/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1326,572,4,'05/21/2029','05/27/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1327,573,1,'05/28/2029','06/03/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1328,573,2,'06/04/2029','06/10/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1329,573,3,'06/11/2029','06/17/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1330,573,4,'06/18/2029','06/24/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1331,574,1,'06/25/2029','07/01/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1332,574,2,'07/02/2029','07/08/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1333,574,3,'07/09/2029','07/15/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1334,574,4,'07/16/2029','07/22/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1335,574,5,'07/23/2029','07/29/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1336,575,1,'07/30/2029','08/05/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1337,575,2,'08/06/2029','08/12/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1338,575,3,'08/13/2029','08/19/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1339,575,4,'08/20/2029','08/26/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1340,576,1,'08/27/2029','09/02/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1341,576,2,'09/03/2029','09/09/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1342,576,3,'09/10/2029','09/16/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1343,576,4,'09/17/2029','09/23/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1344,576,5,'09/24/2029','09/30/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1345,577,1,'10/01/2029','10/07/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1346,577,2,'10/08/2029','10/14/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1347,577,3,'10/15/2029','10/21/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1348,577,4,'10/22/2029','10/28/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1349,578,1,'10/29/2029','11/04/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1350,578,2,'11/05/2029','11/11/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1351,578,3,'11/12/2029','11/18/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1352,578,4,'11/19/2029','11/25/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1353,579,1,'11/26/2029','12/02/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1354,579,2,'12/03/2029','12/09/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1355,579,3,'12/10/2029','12/16/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1356,579,4,'12/17/2029','12/23/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1357,579,5,'12/24/2029','12/30/2029')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1358,580,1,'12/31/2029','01/06/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1359,580,2,'01/07/2030','01/13/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1360,580,3,'01/14/2030','01/20/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1361,580,4,'01/21/2030','01/27/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1362,581,1,'01/28/2030','02/03/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1363,581,2,'02/04/2030','02/10/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1364,581,3,'02/11/2030','02/17/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1365,581,4,'02/18/2030','02/24/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1366,582,1,'02/25/2030','03/03/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1367,582,2,'03/04/2030','03/10/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1368,582,3,'03/11/2030','03/17/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1369,582,4,'03/18/2030','03/24/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1370,582,5,'03/25/2030','03/31/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1371,583,1,'04/01/2030','04/07/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1372,583,2,'04/08/2030','04/14/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1373,583,3,'04/15/2030','04/21/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1374,583,4,'04/22/2030','04/28/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1375,584,1,'04/29/2030','05/05/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1376,584,2,'05/06/2030','05/12/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1377,584,3,'05/13/2030','05/19/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1378,584,4,'05/20/2030','05/26/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1379,585,1,'05/27/2030','06/02/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1380,585,2,'06/03/2030','06/09/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1381,585,3,'06/10/2030','06/16/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1382,585,4,'06/17/2030','06/23/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1383,585,5,'06/24/2030','06/30/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1384,586,1,'07/01/2030','07/07/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1385,586,2,'07/08/2030','07/14/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1386,586,3,'07/15/2030','07/21/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1387,586,4,'07/22/2030','07/28/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1388,587,1,'07/29/2030','08/04/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1389,587,2,'08/05/2030','08/11/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1390,587,3,'08/12/2030','08/18/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1391,587,4,'08/19/2030','08/25/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1392,588,1,'08/26/2030','09/01/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1393,588,2,'09/02/2030','09/08/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1394,588,3,'09/09/2030','09/15/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1395,588,4,'09/16/2030','09/22/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1396,588,5,'09/23/2030','09/29/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1397,589,1,'09/30/2030','10/06/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1398,589,2,'10/07/2030','10/13/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1399,589,3,'10/14/2030','10/20/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1400,589,4,'10/21/2030','10/27/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1401,590,1,'10/28/2030','11/03/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1402,590,2,'11/04/2030','11/10/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1403,590,3,'11/11/2030','11/17/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1404,590,4,'11/18/2030','11/24/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1405,591,1,'11/25/2030','12/01/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1406,591,2,'12/02/2030','12/08/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1407,591,3,'12/09/2030','12/15/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1408,591,4,'12/16/2030','12/22/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1409,591,5,'12/23/2030','12/29/2030')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1410,592,1,'12/30/2030','01/05/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1411,592,2,'01/06/2031','01/12/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1412,592,3,'01/13/2031','01/19/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1413,592,4,'01/20/2031','01/26/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1414,593,1,'01/27/2031','02/02/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1415,593,2,'02/03/2031','02/09/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1416,593,3,'02/10/2031','02/16/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1417,593,4,'02/17/2031','02/23/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1418,594,1,'02/24/2031','03/02/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1419,594,2,'03/03/2031','03/09/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1420,594,3,'03/10/2031','03/16/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1421,594,4,'03/17/2031','03/23/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1422,594,5,'03/24/2031','03/30/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1423,595,1,'03/31/2031','04/06/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1424,595,2,'04/07/2031','04/13/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1425,595,3,'04/14/2031','04/20/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1426,595,4,'04/21/2031','04/27/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1427,596,1,'04/28/2031','05/04/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1428,596,2,'05/05/2031','05/11/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1429,596,3,'05/12/2031','05/18/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1430,596,4,'05/19/2031','05/25/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1431,597,1,'05/26/2031','06/01/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1432,597,2,'06/02/2031','06/08/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1433,597,3,'06/09/2031','06/15/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1434,597,4,'06/16/2031','06/22/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1435,597,5,'06/23/2031','06/29/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1436,598,1,'06/30/2031','07/06/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1437,598,2,'07/07/2031','07/13/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1438,598,3,'07/14/2031','07/20/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1439,598,4,'07/21/2031','07/27/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1440,599,1,'07/28/2031','08/03/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1441,599,2,'08/04/2031','08/10/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1442,599,3,'08/11/2031','08/17/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1443,599,4,'08/18/2031','08/24/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1444,599,5,'08/25/2031','08/31/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1445,600,1,'09/01/2031','09/07/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1446,600,2,'09/08/2031','09/14/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1447,600,3,'09/15/2031','09/21/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1448,600,4,'09/22/2031','09/28/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1449,601,1,'09/29/2031','10/05/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1450,601,2,'10/06/2031','10/12/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1451,601,3,'10/13/2031','10/19/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1452,601,4,'10/20/2031','10/26/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1453,602,1,'10/27/2031','11/02/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1454,602,2,'11/03/2031','11/09/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1455,602,3,'11/10/2031','11/16/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1456,602,4,'11/17/2031','11/23/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1457,602,5,'11/24/2031','11/30/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1458,603,1,'12/01/2031','12/07/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1459,603,2,'12/08/2031','12/14/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1460,603,3,'12/15/2031','12/21/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1461,603,4,'12/22/2031','12/28/2031')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1462,604,1,'12/29/2031','01/04/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1463,604,2,'01/05/2032','01/11/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1464,604,3,'01/12/2032','01/18/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1465,604,4,'01/19/2032','01/25/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1466,605,1,'01/26/2032','02/01/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1467,605,2,'02/02/2032','02/08/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1468,605,3,'02/09/2032','02/15/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1469,605,4,'02/16/2032','02/22/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1470,605,5,'02/23/2032','02/29/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1471,606,1,'03/01/2032','03/07/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1472,606,2,'03/08/2032','03/14/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1473,606,3,'03/15/2032','03/21/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1474,606,4,'03/22/2032','03/28/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1475,607,1,'03/29/2032','04/04/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1476,607,2,'04/05/2032','04/11/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1477,607,3,'04/12/2032','04/18/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1478,607,4,'04/19/2032','04/25/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1479,608,1,'04/26/2032','05/02/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1480,608,2,'05/03/2032','05/09/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1481,608,3,'05/10/2032','05/16/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1482,608,4,'05/17/2032','05/23/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1483,608,5,'05/24/2032','05/30/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1484,609,1,'05/31/2032','06/06/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1485,609,2,'06/07/2032','06/13/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1486,609,3,'06/14/2032','06/20/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1487,609,4,'06/21/2032','06/27/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1488,610,1,'06/28/2032','07/04/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1489,610,2,'07/05/2032','07/11/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1490,610,3,'07/12/2032','07/18/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1491,610,4,'07/19/2032','07/25/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1492,611,1,'07/26/2032','08/01/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1493,611,2,'08/02/2032','08/08/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1494,611,3,'08/09/2032','08/15/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1495,611,4,'08/16/2032','08/22/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1496,611,5,'08/23/2032','08/29/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1497,612,1,'08/30/2032','09/05/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1498,612,2,'09/06/2032','09/12/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1499,612,3,'09/13/2032','09/19/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1500,612,4,'09/20/2032','09/26/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1501,613,1,'09/27/2032','10/03/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1502,613,2,'10/04/2032','10/10/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1503,613,3,'10/11/2032','10/17/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1504,613,4,'10/18/2032','10/24/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1505,613,5,'10/25/2032','10/31/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1506,614,1,'11/01/2032','11/07/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1507,614,2,'11/08/2032','11/14/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1508,614,3,'11/15/2032','11/21/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1509,614,4,'11/22/2032','11/28/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1510,615,1,'11/29/2032','12/05/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1511,615,2,'12/06/2032','12/12/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1512,615,3,'12/13/2032','12/19/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1513,615,4,'12/20/2032','12/26/2032')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1514,616,1,'12/27/2032','01/02/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1515,616,2,'01/03/2033','01/09/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1516,616,3,'01/10/2033','01/16/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1517,616,4,'01/17/2033','01/23/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1518,616,5,'01/24/2033','01/30/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1519,617,1,'01/31/2033','02/06/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1520,617,2,'02/07/2033','02/13/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1521,617,3,'02/14/2033','02/20/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1522,617,4,'02/21/2033','02/27/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1523,618,1,'02/28/2033','03/06/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1524,618,2,'03/07/2033','03/13/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1525,618,3,'03/14/2033','03/20/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1526,618,4,'03/21/2033','03/27/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1527,619,1,'03/28/2033','04/03/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1528,619,2,'04/04/2033','04/10/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1529,619,3,'04/11/2033','04/17/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1530,619,4,'04/18/2033','04/24/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1531,620,1,'04/25/2033','05/01/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1532,620,2,'05/02/2033','05/08/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1533,620,3,'05/09/2033','05/15/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1534,620,4,'05/16/2033','05/22/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1535,620,5,'05/23/2033','05/29/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1536,621,1,'05/30/2033','06/05/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1537,621,2,'06/06/2033','06/12/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1538,621,3,'06/13/2033','06/19/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1539,621,4,'06/20/2033','06/26/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1540,622,1,'06/27/2033','07/03/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1541,622,2,'07/04/2033','07/10/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1542,622,3,'07/11/2033','07/17/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1543,622,4,'07/18/2033','07/24/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1544,622,5,'07/25/2033','07/31/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1545,623,1,'08/01/2033','08/07/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1546,623,2,'08/08/2033','08/14/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1547,623,3,'08/15/2033','08/21/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1548,623,4,'08/22/2033','08/28/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1549,624,1,'08/29/2033','09/04/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1550,624,2,'09/05/2033','09/11/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1551,624,3,'09/12/2033','09/18/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1552,624,4,'09/19/2033','09/25/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1553,625,1,'09/26/2033','10/02/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1554,625,2,'10/03/2033','10/09/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1555,625,3,'10/10/2033','10/16/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1556,625,4,'10/17/2033','10/23/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1557,625,5,'10/24/2033','10/30/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1558,626,1,'10/31/2033','11/06/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1559,626,2,'11/07/2033','11/13/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1560,626,3,'11/14/2033','11/20/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1561,626,4,'11/21/2033','11/27/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1562,627,1,'11/28/2033','12/04/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1563,627,2,'12/05/2033','12/11/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1564,627,3,'12/12/2033','12/18/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1565,627,4,'12/19/2033','12/25/2033')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1566,628,1,'12/26/2033','01/01/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1567,628,2,'01/02/2034','01/08/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1568,628,3,'01/09/2034','01/15/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1569,628,4,'01/16/2034','01/22/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1570,628,5,'01/23/2034','01/29/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1571,629,1,'01/30/2034','02/05/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1572,629,2,'02/06/2034','02/12/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1573,629,3,'02/13/2034','02/19/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1574,629,4,'02/20/2034','02/26/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1575,630,1,'02/27/2034','03/05/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1576,630,2,'03/06/2034','03/12/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1577,630,3,'03/13/2034','03/19/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1578,630,4,'03/20/2034','03/26/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1579,631,1,'03/27/2034','04/02/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1580,631,2,'04/03/2034','04/09/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1581,631,3,'04/10/2034','04/16/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1582,631,4,'04/17/2034','04/23/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1583,631,5,'04/24/2034','04/30/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1584,632,1,'05/01/2034','05/07/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1585,632,2,'05/08/2034','05/14/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1586,632,3,'05/15/2034','05/21/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1587,632,4,'05/22/2034','05/28/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1588,633,1,'05/29/2034','06/04/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1589,633,2,'06/05/2034','06/11/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1590,633,3,'06/12/2034','06/18/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1591,633,4,'06/19/2034','06/25/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1592,634,1,'06/26/2034','07/02/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1593,634,2,'07/03/2034','07/09/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1594,634,3,'07/10/2034','07/16/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1595,634,4,'07/17/2034','07/23/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1596,634,5,'07/24/2034','07/30/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1597,635,1,'07/31/2034','08/06/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1598,635,2,'08/07/2034','08/13/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1599,635,3,'08/14/2034','08/20/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1600,635,4,'08/21/2034','08/27/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1601,636,1,'08/28/2034','09/03/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1602,636,2,'09/04/2034','09/10/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1603,636,3,'09/11/2034','09/17/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1604,636,4,'09/18/2034','09/24/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1605,637,1,'09/25/2034','10/01/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1606,637,2,'10/02/2034','10/08/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1607,637,3,'10/09/2034','10/15/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1608,637,4,'10/16/2034','10/22/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1609,637,5,'10/23/2034','10/29/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1610,638,1,'10/30/2034','11/05/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1611,638,2,'11/06/2034','11/12/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1612,638,3,'11/13/2034','11/19/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1613,638,4,'11/20/2034','11/26/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1614,639,1,'11/27/2034','12/03/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1615,639,2,'12/04/2034','12/10/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1616,639,3,'12/11/2034','12/17/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1617,639,4,'12/18/2034','12/24/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1618,639,5,'12/25/2034','12/31/2034')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1619,640,1,'01/01/2035','01/07/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1620,640,2,'01/08/2035','01/14/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1621,640,3,'01/15/2035','01/21/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1622,640,4,'01/22/2035','01/28/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1623,641,1,'01/29/2035','02/04/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1624,641,2,'02/05/2035','02/11/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1625,641,3,'02/12/2035','02/18/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1626,641,4,'02/19/2035','02/25/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1627,642,1,'02/26/2035','03/04/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1628,642,2,'03/05/2035','03/11/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1629,642,3,'03/12/2035','03/18/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1630,642,4,'03/19/2035','03/25/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1631,643,1,'03/26/2035','04/01/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1632,643,2,'04/02/2035','04/08/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1633,643,3,'04/09/2035','04/15/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1634,643,4,'04/16/2035','04/22/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1635,643,5,'04/23/2035','04/29/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1636,644,1,'04/30/2035','05/06/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1637,644,2,'05/07/2035','05/13/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1638,644,3,'05/14/2035','05/20/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1639,644,4,'05/21/2035','05/27/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1640,645,1,'05/28/2035','06/03/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1641,645,2,'06/04/2035','06/10/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1642,645,3,'06/11/2035','06/17/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1643,645,4,'06/18/2035','06/24/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1644,646,1,'06/25/2035','07/01/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1645,646,2,'07/02/2035','07/08/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1646,646,3,'07/09/2035','07/15/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1647,646,4,'07/16/2035','07/22/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1648,646,5,'07/23/2035','07/29/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1649,647,1,'07/30/2035','08/05/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1650,647,2,'08/06/2035','08/12/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1651,647,3,'08/13/2035','08/19/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1652,647,4,'08/20/2035','08/26/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1653,648,1,'08/27/2035','09/02/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1654,648,2,'09/03/2035','09/09/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1655,648,3,'09/10/2035','09/16/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1656,648,4,'09/17/2035','09/23/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1657,648,5,'09/24/2035','09/30/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1658,649,1,'10/01/2035','10/07/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1659,649,2,'10/08/2035','10/14/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1660,649,3,'10/15/2035','10/21/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1661,649,4,'10/22/2035','10/28/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1662,650,1,'10/29/2035','11/04/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1663,650,2,'11/05/2035','11/11/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1664,650,3,'11/12/2035','11/18/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1665,650,4,'11/19/2035','11/25/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1666,651,1,'11/26/2035','12/02/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1667,651,2,'12/03/2035','12/09/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1668,651,3,'12/10/2035','12/16/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1669,651,4,'12/17/2035','12/23/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1670,651,5,'12/24/2035','12/30/2035')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1671,652,1,'12/31/2035','01/06/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1672,652,2,'01/07/2036','01/13/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1673,652,3,'01/14/2036','01/20/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1674,652,4,'01/21/2036','01/27/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1675,653,1,'01/28/2036','02/03/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1676,653,2,'02/04/2036','02/10/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1677,653,3,'02/11/2036','02/17/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1678,653,4,'02/18/2036','02/24/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1679,654,1,'02/25/2036','03/02/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1680,654,2,'03/03/2036','03/09/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1681,654,3,'03/10/2036','03/16/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1682,654,4,'03/17/2036','03/23/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1683,654,5,'03/24/2036','03/30/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1684,655,1,'03/31/2036','04/06/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1685,655,2,'04/07/2036','04/13/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1686,655,3,'04/14/2036','04/20/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1687,655,4,'04/21/2036','04/27/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1688,656,1,'04/28/2036','05/04/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1689,656,2,'05/05/2036','05/11/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1690,656,3,'05/12/2036','05/18/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1691,656,4,'05/19/2036','05/25/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1692,657,1,'05/26/2036','06/01/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1693,657,2,'06/02/2036','06/08/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1694,657,3,'06/09/2036','06/15/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1695,657,4,'06/16/2036','06/22/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1696,657,5,'06/23/2036','06/29/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1697,658,1,'06/30/2036','07/06/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1698,658,2,'07/07/2036','07/13/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1699,658,3,'07/14/2036','07/20/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1700,658,4,'07/21/2036','07/27/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1701,659,1,'07/28/2036','08/03/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1702,659,2,'08/04/2036','08/10/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1703,659,3,'08/11/2036','08/17/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1704,659,4,'08/18/2036','08/24/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1705,659,5,'08/25/2036','08/31/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1706,660,1,'09/01/2036','09/07/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1707,660,2,'09/08/2036','09/14/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1708,660,3,'09/15/2036','09/21/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1709,660,4,'09/22/2036','09/28/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1710,661,1,'09/29/2036','10/05/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1711,661,2,'10/06/2036','10/12/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1712,661,3,'10/13/2036','10/19/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1713,661,4,'10/20/2036','10/26/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1714,662,1,'10/27/2036','11/02/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1715,662,2,'11/03/2036','11/09/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1716,662,3,'11/10/2036','11/16/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1717,662,4,'11/17/2036','11/23/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1718,662,5,'11/24/2036','11/30/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1719,663,1,'12/01/2036','12/07/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1720,663,2,'12/08/2036','12/14/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1721,663,3,'12/15/2036','12/21/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1722,663,4,'12/22/2036','12/28/2036')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1723,664,1,'12/29/2036','01/04/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1724,664,2,'01/05/2037','01/11/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1725,664,3,'01/12/2037','01/18/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1726,664,4,'01/19/2037','01/25/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1727,665,1,'01/26/2037','02/01/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1728,665,2,'02/02/2037','02/08/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1729,665,3,'02/09/2037','02/15/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1730,665,4,'02/16/2037','02/22/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1731,666,1,'02/23/2037','03/01/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1732,666,2,'03/02/2037','03/08/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1733,666,3,'03/09/2037','03/15/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1734,666,4,'03/16/2037','03/22/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1735,666,5,'03/23/2037','03/29/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1736,667,1,'03/30/2037','04/05/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1737,667,2,'04/06/2037','04/12/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1738,667,3,'04/13/2037','04/19/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1739,667,4,'04/20/2037','04/26/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1740,668,1,'04/27/2037','05/03/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1741,668,2,'05/04/2037','05/10/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1742,668,3,'05/11/2037','05/17/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1743,668,4,'05/18/2037','05/24/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1744,668,5,'05/25/2037','05/31/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1745,669,1,'06/01/2037','06/07/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1746,669,2,'06/08/2037','06/14/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1747,669,3,'06/15/2037','06/21/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1748,669,4,'06/22/2037','06/28/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1749,670,1,'06/29/2037','07/05/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1750,670,2,'07/06/2037','07/12/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1751,670,3,'07/13/2037','07/19/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1752,670,4,'07/20/2037','07/26/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1753,671,1,'07/27/2037','08/02/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1754,671,2,'08/03/2037','08/09/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1755,671,3,'08/10/2037','08/16/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1756,671,4,'08/17/2037','08/23/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1757,671,5,'08/24/2037','08/30/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1758,672,1,'08/31/2037','09/06/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1759,672,2,'09/07/2037','09/13/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1760,672,3,'09/14/2037','09/20/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1761,672,4,'09/21/2037','09/27/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1762,673,1,'09/28/2037','10/04/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1763,673,2,'10/05/2037','10/11/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1764,673,3,'10/12/2037','10/18/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1765,673,4,'10/19/2037','10/25/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1766,674,1,'10/26/2037','11/01/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1767,674,2,'11/02/2037','11/08/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1768,674,3,'11/09/2037','11/15/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1769,674,4,'11/16/2037','11/22/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1770,674,5,'11/23/2037','11/29/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1771,675,1,'11/30/2037','12/06/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1772,675,2,'12/07/2037','12/13/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1773,675,3,'12/14/2037','12/20/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1774,675,4,'12/21/2037','12/27/2037')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1775,676,1,'12/28/2037','01/03/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1776,676,2,'01/04/2038','01/10/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1777,676,3,'01/11/2038','01/17/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1778,676,4,'01/18/2038','01/24/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1779,676,5,'01/25/2038','01/31/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1780,677,1,'02/01/2038','02/07/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1781,677,2,'02/08/2038','02/14/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1782,677,3,'02/15/2038','02/21/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1783,677,4,'02/22/2038','02/28/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1784,678,1,'03/01/2038','03/07/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1785,678,2,'03/08/2038','03/14/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1786,678,3,'03/15/2038','03/21/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1787,678,4,'03/22/2038','03/28/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1788,679,1,'03/29/2038','04/04/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1789,679,2,'04/05/2038','04/11/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1790,679,3,'04/12/2038','04/18/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1791,679,4,'04/19/2038','04/25/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1792,680,1,'04/26/2038','05/02/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1793,680,2,'05/03/2038','05/09/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1794,680,3,'05/10/2038','05/16/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1795,680,4,'05/17/2038','05/23/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1796,680,5,'05/24/2038','05/30/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1797,681,1,'05/31/2038','06/06/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1798,681,2,'06/07/2038','06/13/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1799,681,3,'06/14/2038','06/20/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1800,681,4,'06/21/2038','06/27/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1801,682,1,'06/28/2038','07/04/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1802,682,2,'07/05/2038','07/11/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1803,682,3,'07/12/2038','07/18/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1804,682,4,'07/19/2038','07/25/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1805,683,1,'07/26/2038','08/01/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1806,683,2,'08/02/2038','08/08/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1807,683,3,'08/09/2038','08/15/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1808,683,4,'08/16/2038','08/22/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1809,683,5,'08/23/2038','08/29/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1810,684,1,'08/30/2038','09/05/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1811,684,2,'09/06/2038','09/12/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1812,684,3,'09/13/2038','09/19/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1813,684,4,'09/20/2038','09/26/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1814,685,1,'09/27/2038','10/03/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1815,685,2,'10/04/2038','10/10/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1816,685,3,'10/11/2038','10/17/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1817,685,4,'10/18/2038','10/24/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1818,685,5,'10/25/2038','10/31/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1819,686,1,'11/01/2038','11/07/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1820,686,2,'11/08/2038','11/14/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1821,686,3,'11/15/2038','11/21/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1822,686,4,'11/22/2038','11/28/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1823,687,1,'11/29/2038','12/05/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1824,687,2,'12/06/2038','12/12/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1825,687,3,'12/13/2038','12/19/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1826,687,4,'12/20/2038','12/26/2038')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1827,688,1,'12/27/2038','01/02/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1828,688,2,'01/03/2039','01/09/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1829,688,3,'01/10/2039','01/16/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1830,688,4,'01/17/2039','01/23/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1831,688,5,'01/24/2039','01/30/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1832,689,1,'01/31/2039','02/06/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1833,689,2,'02/07/2039','02/13/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1834,689,3,'02/14/2039','02/20/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1835,689,4,'02/21/2039','02/27/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1836,690,1,'02/28/2039','03/06/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1837,690,2,'03/07/2039','03/13/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1838,690,3,'03/14/2039','03/20/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1839,690,4,'03/21/2039','03/27/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1840,691,1,'03/28/2039','04/03/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1841,691,2,'04/04/2039','04/10/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1842,691,3,'04/11/2039','04/17/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1843,691,4,'04/18/2039','04/24/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1844,692,1,'04/25/2039','05/01/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1845,692,2,'05/02/2039','05/08/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1846,692,3,'05/09/2039','05/15/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1847,692,4,'05/16/2039','05/22/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1848,692,5,'05/23/2039','05/29/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1849,693,1,'05/30/2039','06/05/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1850,693,2,'06/06/2039','06/12/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1851,693,3,'06/13/2039','06/19/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1852,693,4,'06/20/2039','06/26/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1853,694,1,'06/27/2039','07/03/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1854,694,2,'07/04/2039','07/10/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1855,694,3,'07/11/2039','07/17/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1856,694,4,'07/18/2039','07/24/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1857,694,5,'07/25/2039','07/31/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1858,695,1,'08/01/2039','08/07/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1859,695,2,'08/08/2039','08/14/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1860,695,3,'08/15/2039','08/21/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1861,695,4,'08/22/2039','08/28/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1862,696,1,'08/29/2039','09/04/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1863,696,2,'09/05/2039','09/11/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1864,696,3,'09/12/2039','09/18/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1865,696,4,'09/19/2039','09/25/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1866,697,1,'09/26/2039','10/02/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1867,697,2,'10/03/2039','10/09/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1868,697,3,'10/10/2039','10/16/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1869,697,4,'10/17/2039','10/23/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1870,697,5,'10/24/2039','10/30/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1871,698,1,'10/31/2039','11/06/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1872,698,2,'11/07/2039','11/13/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1873,698,3,'11/14/2039','11/20/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1874,698,4,'11/21/2039','11/27/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1875,699,1,'11/28/2039','12/04/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1876,699,2,'12/05/2039','12/11/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1877,699,3,'12/12/2039','12/18/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1878,699,4,'12/19/2039','12/25/2039')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1879,700,1,'12/26/2039','01/01/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1880,700,2,'01/02/2040','01/08/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1881,700,3,'01/09/2040','01/15/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1882,700,4,'01/16/2040','01/22/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1883,700,5,'01/23/2040','01/29/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1884,701,1,'01/30/2040','02/05/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1885,701,2,'02/06/2040','02/12/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1886,701,3,'02/13/2040','02/19/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1887,701,4,'02/20/2040','02/26/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1888,702,1,'02/27/2040','03/04/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1889,702,2,'03/05/2040','03/11/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1890,702,3,'03/12/2040','03/18/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1891,702,4,'03/19/2040','03/25/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1892,703,1,'03/26/2040','04/01/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1893,703,2,'04/02/2040','04/08/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1894,703,3,'04/09/2040','04/15/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1895,703,4,'04/16/2040','04/22/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1896,703,5,'04/23/2040','04/29/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1897,704,1,'04/30/2040','05/06/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1898,704,2,'05/07/2040','05/13/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1899,704,3,'05/14/2040','05/20/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1900,704,4,'05/21/2040','05/27/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1901,705,1,'05/28/2040','06/03/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1902,705,2,'06/04/2040','06/10/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1903,705,3,'06/11/2040','06/17/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1904,705,4,'06/18/2040','06/24/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1905,706,1,'06/25/2040','07/01/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1906,706,2,'07/02/2040','07/08/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1907,706,3,'07/09/2040','07/15/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1908,706,4,'07/16/2040','07/22/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1909,706,5,'07/23/2040','07/29/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1910,707,1,'07/30/2040','08/05/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1911,707,2,'08/06/2040','08/12/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1912,707,3,'08/13/2040','08/19/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1913,707,4,'08/20/2040','08/26/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1914,708,1,'08/27/2040','09/02/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1915,708,2,'09/03/2040','09/09/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1916,708,3,'09/10/2040','09/16/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1917,708,4,'09/17/2040','09/23/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1918,708,5,'09/24/2040','09/30/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1919,709,1,'10/01/2040','10/07/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1920,709,2,'10/08/2040','10/14/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1921,709,3,'10/15/2040','10/21/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1922,709,4,'10/22/2040','10/28/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1923,710,1,'10/29/2040','11/04/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1924,710,2,'11/05/2040','11/11/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1925,710,3,'11/12/2040','11/18/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1926,710,4,'11/19/2040','11/25/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1927,711,1,'11/26/2040','12/02/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1928,711,2,'12/03/2040','12/09/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1929,711,3,'12/10/2040','12/16/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1930,711,4,'12/17/2040','12/23/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1931,711,5,'12/24/2040','12/30/2040')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1932,712,1,'12/31/2040','01/06/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1933,712,2,'01/07/2041','01/13/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1934,712,3,'01/14/2041','01/20/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1935,712,4,'01/21/2041','01/27/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1936,713,1,'01/28/2041','02/03/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1937,713,2,'02/04/2041','02/10/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1938,713,3,'02/11/2041','02/17/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1939,713,4,'02/18/2041','02/24/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1940,714,1,'02/25/2041','03/03/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1941,714,2,'03/04/2041','03/10/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1942,714,3,'03/11/2041','03/17/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1943,714,4,'03/18/2041','03/24/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1944,714,5,'03/25/2041','03/31/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1945,715,1,'04/01/2041','04/07/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1946,715,2,'04/08/2041','04/14/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1947,715,3,'04/15/2041','04/21/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1948,715,4,'04/22/2041','04/28/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1949,716,1,'04/29/2041','05/05/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1950,716,2,'05/06/2041','05/12/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1951,716,3,'05/13/2041','05/19/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1952,716,4,'05/20/2041','05/26/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1953,717,1,'05/27/2041','06/02/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1954,717,2,'06/03/2041','06/09/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1955,717,3,'06/10/2041','06/16/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1956,717,4,'06/17/2041','06/23/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1957,717,5,'06/24/2041','06/30/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1958,718,1,'07/01/2041','07/07/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1959,718,2,'07/08/2041','07/14/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1960,718,3,'07/15/2041','07/21/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1961,718,4,'07/22/2041','07/28/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1962,719,1,'07/29/2041','08/04/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1963,719,2,'08/05/2041','08/11/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1964,719,3,'08/12/2041','08/18/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1965,719,4,'08/19/2041','08/25/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1966,720,1,'08/26/2041','09/01/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1967,720,2,'09/02/2041','09/08/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1968,720,3,'09/09/2041','09/15/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1969,720,4,'09/16/2041','09/22/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1970,720,5,'09/23/2041','09/29/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1971,721,1,'09/30/2041','10/06/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1972,721,2,'10/07/2041','10/13/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1973,721,3,'10/14/2041','10/20/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1974,721,4,'10/21/2041','10/27/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1975,722,1,'10/28/2041','11/03/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1976,722,2,'11/04/2041','11/10/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1977,722,3,'11/11/2041','11/17/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1978,722,4,'11/18/2041','11/24/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1979,723,1,'11/25/2041','12/01/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1980,723,2,'12/02/2041','12/08/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1981,723,3,'12/09/2041','12/15/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1982,723,4,'12/16/2041','12/22/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1983,723,5,'12/23/2041','12/29/2041')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1984,724,1,'12/30/2041','01/05/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1985,724,2,'01/06/2042','01/12/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1986,724,3,'01/13/2042','01/19/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1987,724,4,'01/20/2042','01/26/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1988,725,1,'01/27/2042','02/02/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1989,725,2,'02/03/2042','02/09/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1990,725,3,'02/10/2042','02/16/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1991,725,4,'02/17/2042','02/23/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1992,726,1,'02/24/2042','03/02/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1993,726,2,'03/03/2042','03/09/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1994,726,3,'03/10/2042','03/16/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1995,726,4,'03/17/2042','03/23/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1996,726,5,'03/24/2042','03/30/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1997,727,1,'03/31/2042','04/06/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1998,727,2,'04/07/2042','04/13/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (1999,727,3,'04/14/2042','04/20/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2000,727,4,'04/21/2042','04/27/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2001,728,1,'04/28/2042','05/04/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2002,728,2,'05/05/2042','05/11/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2003,728,3,'05/12/2042','05/18/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2004,728,4,'05/19/2042','05/25/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2005,729,1,'05/26/2042','06/01/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2006,729,2,'06/02/2042','06/08/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2007,729,3,'06/09/2042','06/15/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2008,729,4,'06/16/2042','06/22/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2009,729,5,'06/23/2042','06/29/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2010,730,1,'06/30/2042','07/06/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2011,730,2,'07/07/2042','07/13/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2012,730,3,'07/14/2042','07/20/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2013,730,4,'07/21/2042','07/27/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2014,731,1,'07/28/2042','08/03/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2015,731,2,'08/04/2042','08/10/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2016,731,3,'08/11/2042','08/17/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2017,731,4,'08/18/2042','08/24/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2018,731,5,'08/25/2042','08/31/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2019,732,1,'09/01/2042','09/07/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2020,732,2,'09/08/2042','09/14/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2021,732,3,'09/15/2042','09/21/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2022,732,4,'09/22/2042','09/28/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2023,733,1,'09/29/2042','10/05/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2024,733,2,'10/06/2042','10/12/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2025,733,3,'10/13/2042','10/19/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2026,733,4,'10/20/2042','10/26/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2027,734,1,'10/27/2042','11/02/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2028,734,2,'11/03/2042','11/09/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2029,734,3,'11/10/2042','11/16/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2030,734,4,'11/17/2042','11/23/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2031,734,5,'11/24/2042','11/30/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2032,735,1,'12/01/2042','12/07/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2033,735,2,'12/08/2042','12/14/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2034,735,3,'12/15/2042','12/21/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2035,735,4,'12/22/2042','12/28/2042')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2036,736,1,'12/29/2042','01/04/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2037,736,2,'01/05/2043','01/11/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2038,736,3,'01/12/2043','01/18/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2039,736,4,'01/19/2043','01/25/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2040,737,1,'01/26/2043','02/01/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2041,737,2,'02/02/2043','02/08/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2042,737,3,'02/09/2043','02/15/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2043,737,4,'02/16/2043','02/22/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2044,738,1,'02/23/2043','03/01/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2045,738,2,'03/02/2043','03/08/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2046,738,3,'03/09/2043','03/15/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2047,738,4,'03/16/2043','03/22/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2048,738,5,'03/23/2043','03/29/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2049,739,1,'03/30/2043','04/05/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2050,739,2,'04/06/2043','04/12/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2051,739,3,'04/13/2043','04/19/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2052,739,4,'04/20/2043','04/26/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2053,740,1,'04/27/2043','05/03/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2054,740,2,'05/04/2043','05/10/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2055,740,3,'05/11/2043','05/17/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2056,740,4,'05/18/2043','05/24/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2057,740,5,'05/25/2043','05/31/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2058,741,1,'06/01/2043','06/07/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2059,741,2,'06/08/2043','06/14/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2060,741,3,'06/15/2043','06/21/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2061,741,4,'06/22/2043','06/28/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2062,742,1,'06/29/2043','07/05/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2063,742,2,'07/06/2043','07/12/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2064,742,3,'07/13/2043','07/19/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2065,742,4,'07/20/2043','07/26/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2066,743,1,'07/27/2043','08/02/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2067,743,2,'08/03/2043','08/09/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2068,743,3,'08/10/2043','08/16/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2069,743,4,'08/17/2043','08/23/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2070,743,5,'08/24/2043','08/30/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2071,744,1,'08/31/2043','09/06/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2072,744,2,'09/07/2043','09/13/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2073,744,3,'09/14/2043','09/20/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2074,744,4,'09/21/2043','09/27/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2075,745,1,'09/28/2043','10/04/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2076,745,2,'10/05/2043','10/11/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2077,745,3,'10/12/2043','10/18/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2078,745,4,'10/19/2043','10/25/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2079,746,1,'10/26/2043','11/01/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2080,746,2,'11/02/2043','11/08/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2081,746,3,'11/09/2043','11/15/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2082,746,4,'11/16/2043','11/22/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2083,746,5,'11/23/2043','11/29/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2084,747,1,'11/30/2043','12/06/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2085,747,2,'12/07/2043','12/13/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2086,747,3,'12/14/2043','12/20/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2087,747,4,'12/21/2043','12/27/2043')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2088,748,1,'12/28/2043','01/03/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2089,748,2,'01/04/2044','01/10/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2090,748,3,'01/11/2044','01/17/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2091,748,4,'01/18/2044','01/24/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2092,748,5,'01/25/2044','01/31/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2093,749,1,'02/01/2044','02/07/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2094,749,2,'02/08/2044','02/14/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2095,749,3,'02/15/2044','02/21/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2096,749,4,'02/22/2044','02/28/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2097,750,1,'02/29/2044','03/06/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2098,750,2,'03/07/2044','03/13/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2099,750,3,'03/14/2044','03/20/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2100,750,4,'03/21/2044','03/27/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2101,751,1,'03/28/2044','04/03/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2102,751,2,'04/04/2044','04/10/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2103,751,3,'04/11/2044','04/17/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2104,751,4,'04/18/2044','04/24/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2105,752,1,'04/25/2044','05/01/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2106,752,2,'05/02/2044','05/08/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2107,752,3,'05/09/2044','05/15/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2108,752,4,'05/16/2044','05/22/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2109,752,5,'05/23/2044','05/29/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2110,753,1,'05/30/2044','06/05/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2111,753,2,'06/06/2044','06/12/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2112,753,3,'06/13/2044','06/19/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2113,753,4,'06/20/2044','06/26/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2114,754,1,'06/27/2044','07/03/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2115,754,2,'07/04/2044','07/10/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2116,754,3,'07/11/2044','07/17/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2117,754,4,'07/18/2044','07/24/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2118,754,5,'07/25/2044','07/31/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2119,755,1,'08/01/2044','08/07/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2120,755,2,'08/08/2044','08/14/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2121,755,3,'08/15/2044','08/21/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2122,755,4,'08/22/2044','08/28/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2123,756,1,'08/29/2044','09/04/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2124,756,2,'09/05/2044','09/11/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2125,756,3,'09/12/2044','09/18/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2126,756,4,'09/19/2044','09/25/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2127,757,1,'09/26/2044','10/02/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2128,757,2,'10/03/2044','10/09/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2129,757,3,'10/10/2044','10/16/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2130,757,4,'10/17/2044','10/23/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2131,757,5,'10/24/2044','10/30/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2132,758,1,'10/31/2044','11/06/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2133,758,2,'11/07/2044','11/13/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2134,758,3,'11/14/2044','11/20/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2135,758,4,'11/21/2044','11/27/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2136,759,1,'11/28/2044','12/04/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2137,759,2,'12/05/2044','12/11/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2138,759,3,'12/12/2044','12/18/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2139,759,4,'12/19/2044','12/25/2044')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2140,760,1,'12/26/2044','01/01/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2141,760,2,'01/02/2045','01/08/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2142,760,3,'01/09/2045','01/15/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2143,760,4,'01/16/2045','01/22/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2144,760,5,'01/23/2045','01/29/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2145,761,1,'01/30/2045','02/05/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2146,761,2,'02/06/2045','02/12/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2147,761,3,'02/13/2045','02/19/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2148,761,4,'02/20/2045','02/26/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2149,762,1,'02/27/2045','03/05/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2150,762,2,'03/06/2045','03/12/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2151,762,3,'03/13/2045','03/19/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2152,762,4,'03/20/2045','03/26/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2153,763,1,'03/27/2045','04/02/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2154,763,2,'04/03/2045','04/09/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2155,763,3,'04/10/2045','04/16/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2156,763,4,'04/17/2045','04/23/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2157,763,5,'04/24/2045','04/30/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2158,764,1,'05/01/2045','05/07/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2159,764,2,'05/08/2045','05/14/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2160,764,3,'05/15/2045','05/21/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2161,764,4,'05/22/2045','05/28/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2162,765,1,'05/29/2045','06/04/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2163,765,2,'06/05/2045','06/11/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2164,765,3,'06/12/2045','06/18/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2165,765,4,'06/19/2045','06/25/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2166,766,1,'06/26/2045','07/02/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2167,766,2,'07/03/2045','07/09/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2168,766,3,'07/10/2045','07/16/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2169,766,4,'07/17/2045','07/23/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2170,766,5,'07/24/2045','07/30/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2171,767,1,'07/31/2045','08/06/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2172,767,2,'08/07/2045','08/13/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2173,767,3,'08/14/2045','08/20/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2174,767,4,'08/21/2045','08/27/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2175,768,1,'08/28/2045','09/03/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2176,768,2,'09/04/2045','09/10/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2177,768,3,'09/11/2045','09/17/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2178,768,4,'09/18/2045','09/24/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2179,769,1,'09/25/2045','10/01/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2180,769,2,'10/02/2045','10/08/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2181,769,3,'10/09/2045','10/15/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2182,769,4,'10/16/2045','10/22/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2183,769,5,'10/23/2045','10/29/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2184,770,1,'10/30/2045','11/05/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2185,770,2,'11/06/2045','11/12/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2186,770,3,'11/13/2045','11/19/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2187,770,4,'11/20/2045','11/26/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2188,771,1,'11/27/2045','12/03/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2189,771,2,'12/04/2045','12/10/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2190,771,3,'12/11/2045','12/17/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2191,771,4,'12/18/2045','12/24/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2192,771,5,'12/25/2045','12/31/2045')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2193,772,1,'01/01/2046','01/07/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2194,772,2,'01/08/2046','01/14/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2195,772,3,'01/15/2046','01/21/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2196,772,4,'01/22/2046','01/28/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2197,773,1,'01/29/2046','02/04/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2198,773,2,'02/05/2046','02/11/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2199,773,3,'02/12/2046','02/18/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2200,773,4,'02/19/2046','02/25/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2201,774,1,'02/26/2046','03/04/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2202,774,2,'03/05/2046','03/11/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2203,774,3,'03/12/2046','03/18/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2204,774,4,'03/19/2046','03/25/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2205,775,1,'03/26/2046','04/01/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2206,775,2,'04/02/2046','04/08/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2207,775,3,'04/09/2046','04/15/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2208,775,4,'04/16/2046','04/22/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2209,775,5,'04/23/2046','04/29/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2210,776,1,'04/30/2046','05/06/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2211,776,2,'05/07/2046','05/13/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2212,776,3,'05/14/2046','05/20/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2213,776,4,'05/21/2046','05/27/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2214,777,1,'05/28/2046','06/03/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2215,777,2,'06/04/2046','06/10/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2216,777,3,'06/11/2046','06/17/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2217,777,4,'06/18/2046','06/24/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2218,778,1,'06/25/2046','07/01/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2219,778,2,'07/02/2046','07/08/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2220,778,3,'07/09/2046','07/15/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2221,778,4,'07/16/2046','07/22/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2222,778,5,'07/23/2046','07/29/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2223,779,1,'07/30/2046','08/05/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2224,779,2,'08/06/2046','08/12/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2225,779,3,'08/13/2046','08/19/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2226,779,4,'08/20/2046','08/26/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2227,780,1,'08/27/2046','09/02/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2228,780,2,'09/03/2046','09/09/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2229,780,3,'09/10/2046','09/16/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2230,780,4,'09/17/2046','09/23/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2231,780,5,'09/24/2046','09/30/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2232,781,1,'10/01/2046','10/07/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2233,781,2,'10/08/2046','10/14/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2234,781,3,'10/15/2046','10/21/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2235,781,4,'10/22/2046','10/28/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2236,782,1,'10/29/2046','11/04/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2237,782,2,'11/05/2046','11/11/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2238,782,3,'11/12/2046','11/18/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2239,782,4,'11/19/2046','11/25/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2240,783,1,'11/26/2046','12/02/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2241,783,2,'12/03/2046','12/09/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2242,783,3,'12/10/2046','12/16/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2243,783,4,'12/17/2046','12/23/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2244,783,5,'12/24/2046','12/30/2046')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2245,784,1,'12/31/2046','01/06/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2246,784,2,'01/07/2047','01/13/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2247,784,3,'01/14/2047','01/20/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2248,784,4,'01/21/2047','01/27/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2249,785,1,'01/28/2047','02/03/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2250,785,2,'02/04/2047','02/10/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2251,785,3,'02/11/2047','02/17/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2252,785,4,'02/18/2047','02/24/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2253,786,1,'02/25/2047','03/03/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2254,786,2,'03/04/2047','03/10/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2255,786,3,'03/11/2047','03/17/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2256,786,4,'03/18/2047','03/24/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2257,786,5,'03/25/2047','03/31/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2258,787,1,'04/01/2047','04/07/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2259,787,2,'04/08/2047','04/14/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2260,787,3,'04/15/2047','04/21/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2261,787,4,'04/22/2047','04/28/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2262,788,1,'04/29/2047','05/05/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2263,788,2,'05/06/2047','05/12/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2264,788,3,'05/13/2047','05/19/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2265,788,4,'05/20/2047','05/26/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2266,789,1,'05/27/2047','06/02/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2267,789,2,'06/03/2047','06/09/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2268,789,3,'06/10/2047','06/16/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2269,789,4,'06/17/2047','06/23/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2270,789,5,'06/24/2047','06/30/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2271,790,1,'07/01/2047','07/07/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2272,790,2,'07/08/2047','07/14/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2273,790,3,'07/15/2047','07/21/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2274,790,4,'07/22/2047','07/28/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2275,791,1,'07/29/2047','08/04/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2276,791,2,'08/05/2047','08/11/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2277,791,3,'08/12/2047','08/18/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2278,791,4,'08/19/2047','08/25/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2279,792,1,'08/26/2047','09/01/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2280,792,2,'09/02/2047','09/08/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2281,792,3,'09/09/2047','09/15/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2282,792,4,'09/16/2047','09/22/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2283,792,5,'09/23/2047','09/29/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2284,793,1,'09/30/2047','10/06/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2285,793,2,'10/07/2047','10/13/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2286,793,3,'10/14/2047','10/20/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2287,793,4,'10/21/2047','10/27/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2288,794,1,'10/28/2047','11/03/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2289,794,2,'11/04/2047','11/10/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2290,794,3,'11/11/2047','11/17/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2291,794,4,'11/18/2047','11/24/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2292,795,1,'11/25/2047','12/01/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2293,795,2,'12/02/2047','12/08/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2294,795,3,'12/09/2047','12/15/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2295,795,4,'12/16/2047','12/22/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2296,795,5,'12/23/2047','12/29/2047')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2297,796,1,'12/30/2047','01/05/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2298,796,2,'01/06/2048','01/12/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2299,796,3,'01/13/2048','01/19/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2300,796,4,'01/20/2048','01/26/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2301,797,1,'01/27/2048','02/02/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2302,797,2,'02/03/2048','02/09/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2303,797,3,'02/10/2048','02/16/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2304,797,4,'02/17/2048','02/23/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2305,798,1,'02/24/2048','03/01/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2306,798,2,'03/02/2048','03/08/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2307,798,3,'03/09/2048','03/15/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2308,798,4,'03/16/2048','03/22/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2309,798,5,'03/23/2048','03/29/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2310,799,1,'03/30/2048','04/05/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2311,799,2,'04/06/2048','04/12/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2312,799,3,'04/13/2048','04/19/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2313,799,4,'04/20/2048','04/26/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2314,800,1,'04/27/2048','05/03/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2315,800,2,'05/04/2048','05/10/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2316,800,3,'05/11/2048','05/17/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2317,800,4,'05/18/2048','05/24/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2318,800,5,'05/25/2048','05/31/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2319,801,1,'06/01/2048','06/07/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2320,801,2,'06/08/2048','06/14/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2321,801,3,'06/15/2048','06/21/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2322,801,4,'06/22/2048','06/28/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2323,802,1,'06/29/2048','07/05/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2324,802,2,'07/06/2048','07/12/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2325,802,3,'07/13/2048','07/19/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2326,802,4,'07/20/2048','07/26/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2327,803,1,'07/27/2048','08/02/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2328,803,2,'08/03/2048','08/09/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2329,803,3,'08/10/2048','08/16/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2330,803,4,'08/17/2048','08/23/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2331,803,5,'08/24/2048','08/30/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2332,804,1,'08/31/2048','09/06/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2333,804,2,'09/07/2048','09/13/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2334,804,3,'09/14/2048','09/20/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2335,804,4,'09/21/2048','09/27/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2336,805,1,'09/28/2048','10/04/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2337,805,2,'10/05/2048','10/11/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2338,805,3,'10/12/2048','10/18/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2339,805,4,'10/19/2048','10/25/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2340,806,1,'10/26/2048','11/01/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2341,806,2,'11/02/2048','11/08/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2342,806,3,'11/09/2048','11/15/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2343,806,4,'11/16/2048','11/22/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2344,806,5,'11/23/2048','11/29/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2345,807,1,'11/30/2048','12/06/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2346,807,2,'12/07/2048','12/13/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2347,807,3,'12/14/2048','12/20/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2348,807,4,'12/21/2048','12/27/2048')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2349,808,1,'12/28/2048','01/03/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2350,808,2,'01/04/2049','01/10/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2351,808,3,'01/11/2049','01/17/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2352,808,4,'01/18/2049','01/24/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2353,808,5,'01/25/2049','01/31/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2354,809,1,'02/01/2049','02/07/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2355,809,2,'02/08/2049','02/14/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2356,809,3,'02/15/2049','02/21/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2357,809,4,'02/22/2049','02/28/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2358,810,1,'03/01/2049','03/07/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2359,810,2,'03/08/2049','03/14/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2360,810,3,'03/15/2049','03/21/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2361,810,4,'03/22/2049','03/28/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2362,811,1,'03/29/2049','04/04/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2363,811,2,'04/05/2049','04/11/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2364,811,3,'04/12/2049','04/18/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2365,811,4,'04/19/2049','04/25/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2366,812,1,'04/26/2049','05/02/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2367,812,2,'05/03/2049','05/09/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2368,812,3,'05/10/2049','05/16/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2369,812,4,'05/17/2049','05/23/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2370,812,5,'05/24/2049','05/30/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2371,813,1,'05/31/2049','06/06/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2372,813,2,'06/07/2049','06/13/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2373,813,3,'06/14/2049','06/20/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2374,813,4,'06/21/2049','06/27/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2375,814,1,'06/28/2049','07/04/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2376,814,2,'07/05/2049','07/11/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2377,814,3,'07/12/2049','07/18/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2378,814,4,'07/19/2049','07/25/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2379,815,1,'07/26/2049','08/01/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2380,815,2,'08/02/2049','08/08/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2381,815,3,'08/09/2049','08/15/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2382,815,4,'08/16/2049','08/22/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2383,815,5,'08/23/2049','08/29/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2384,816,1,'08/30/2049','09/05/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2385,816,2,'09/06/2049','09/12/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2386,816,3,'09/13/2049','09/19/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2387,816,4,'09/20/2049','09/26/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2388,817,1,'09/27/2049','10/03/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2389,817,2,'10/04/2049','10/10/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2390,817,3,'10/11/2049','10/17/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2391,817,4,'10/18/2049','10/24/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2392,817,5,'10/25/2049','10/31/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2393,818,1,'11/01/2049','11/07/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2394,818,2,'11/08/2049','11/14/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2395,818,3,'11/15/2049','11/21/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2396,818,4,'11/22/2049','11/28/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2397,819,1,'11/29/2049','12/05/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2398,819,2,'12/06/2049','12/12/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2399,819,3,'12/13/2049','12/19/2049')
	INSERT INTO dbo.media_weeks (id,media_month_id,week_number,start_date,end_date) VALUES (2400,819,4,'12/20/2049','12/26/2049')
	SET IDENTITY_INSERT dbo.media_weeks OFF
END
GO
/* END: Creation of MEDIA MONTHS and MEDIA WEEKS from 0121 to 0149 */

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