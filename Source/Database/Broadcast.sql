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

/*************************************** END UPDATE SCRIPT *******************************************************/

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