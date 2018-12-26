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


/*************************************** START BCOP-2801 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[nti_transmittals_audiences]'))
BEGIN
	CREATE TABLE [dbo].[nti_transmittals_audiences](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nti_transmittals_file_report_id] [INT] NOT NULL,
	[proposal_version_detail_quarter_week_id] [INT] NOT NULL,
	[audience_id] [INT] NOT NULL,
	[impressions] [FLOAT] NOT NULL
	 CONSTRAINT [PK_nti_transmittals_audiences] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[nti_transmittals_audiences]  WITH CHECK ADD  CONSTRAINT [FK_nti_transmittals_audiences_nti_transmittals_file_reports] FOREIGN KEY([nti_transmittals_file_report_id])
	REFERENCES [dbo].[nti_transmittals_file_reports] ([id])
	ON DELETE CASCADE
	ALTER TABLE [dbo].[nti_transmittals_audiences] CHECK CONSTRAINT [FK_nti_transmittals_audiences_nti_transmittals_file_reports]
	CREATE INDEX IX_nti_transmittals_audiences_nti_transmittals_file_report_id ON [nti_transmittals_audiences] ([nti_transmittals_file_report_id])

	ALTER TABLE [dbo].[nti_transmittals_audiences]  WITH CHECK ADD  CONSTRAINT [FK_nti_transmittals_audiences_proposal_version_detail_quarter_weeks] FOREIGN KEY([proposal_version_detail_quarter_week_id])
	REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id])
	ALTER TABLE [dbo].[nti_transmittals_audiences] CHECK CONSTRAINT [FK_nti_transmittals_audiences_proposal_version_detail_quarter_weeks]
	CREATE INDEX IX_nti_transmittals_audiences_proposal_version_detail_quarter_week_id ON [nti_transmittals_audiences] ([proposal_version_detail_quarter_week_id])

	ALTER TABLE [dbo].[nti_transmittals_audiences]  WITH CHECK ADD  CONSTRAINT [FK_nti_transmittals_audiences_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])
	ALTER TABLE [dbo].[nti_transmittals_audiences] CHECK CONSTRAINT [FK_nti_transmittals_audiences_audiences]
	CREATE INDEX IX_nti_transmittals_audiences_audience_id ON [nti_transmittals_audiences] ([audience_id])
END
/*************************************** END BCOP-2801 *****************************************************/

/*************************************** START BCOP-4036 *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[market_coverage_files]'))
BEGIN
	CREATE TABLE [dbo].[market_coverage_files]
	(
		[id] [int] IDENTITY(1,1) NOT NULL,
		[file_name] [varchar](255) NOT NULL,
		[file_hash] [varchar](255) NOT NULL,
		[created_date] [datetime] NOT NULL,
		[created_by] [varchar](63) NOT NULL,
	 CONSTRAINT [PK_market_coverage_files] PRIMARY KEY CLUSTERED 
	 (
		[id] ASC
	 ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	SET IDENTITY_INSERT [dbo].[market_coverage_files] ON 
	INSERT [dbo].[market_coverage_files] ([id], [file_name], [file_hash], [created_date], [created_by]) 
	VALUES (1, N'Market_Coverages.xlsx', N'3E-E1-54-2A-6F-8C-C7-0E-5A-F3-1F-80-DA-01-EF-B8-C0-78-62-0B', CAST(N'2018-12-18T00:00:00.000' AS DateTime), N'CROSSMW\bbotelho')
	SET IDENTITY_INSERT [dbo].[market_coverage_files] OFF
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'market_coverage_file_id'
          AND Object_ID = Object_ID(N'[dbo].[market_coverages]'))
BEGIN
	ALTER TABLE [market_coverages]
	ADD [market_coverage_file_id] INT NULL

	EXEC('UPDATE [market_coverages]
		  SET [market_coverage_file_id] = 1')

	ALTER TABLE [market_coverages]
	ALTER COLUMN [market_coverage_file_id] INT NOT NULL

	ALTER TABLE [dbo].[market_coverages]  WITH CHECK ADD  CONSTRAINT [FK_market_coverages_market_coverage_files] FOREIGN KEY([market_coverage_file_id])
	REFERENCES [dbo].[market_coverage_files] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[market_coverages] CHECK CONSTRAINT [FK_market_coverages_market_coverage_files]
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'market_coverage_file_id'
          AND Object_ID = Object_ID(N'[dbo].[pricing_guide_distributions]'))
BEGIN
	ALTER TABLE [dbo].[pricing_guide_distributions]
	ADD market_coverage_file_id INT NULL

	exec('update pricing_guide_distributions
	      set market_coverage_file_id = (select top 1 id from market_coverage_files order by created_date desc)')

	ALTER TABLE [dbo].[pricing_guide_distributions]
	ALTER COLUMN market_coverage_file_id INT NOT NULL
	
	ALTER TABLE [dbo].[pricing_guide_distributions]  WITH CHECK ADD  CONSTRAINT [FK_pricing_guide_distributions_market_coverage_files] FOREIGN KEY([market_coverage_file_id])
	REFERENCES [dbo].[market_coverage_files] ([id])
	
	ALTER TABLE [dbo].[pricing_guide_distributions] CHECK CONSTRAINT [FK_pricing_guide_distributions_market_coverage_files]
END

/*************************************** END BCOP-4036 *******************************************************/

/*************************************** START BCOP-4192 *****************************************************/

IF (SELECT character_maximum_length
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE 
		TABLE_NAME = 'pricing_guide_distribution_open_market_inventory' AND 
		COLUMN_NAME = 'program_name') = 255
BEGIN
	ALTER TABLE pricing_guide_distribution_open_market_inventory
	ALTER COLUMN program_name VARCHAR(MAX) NULL
END
/*************************************** END BCOP-4192 *****************************************************/

/*************************************** START BCOP-4138 *******************************************************/
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'nti_conversion_factor'
		  AND is_nullable = 0
          AND Object_ID = Object_ID(N'[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [dbo].[proposal_version_details] ALTER COLUMN [nti_conversion_factor] FLOAT NULL
END
/*************************************** END BCOP-4138 *******************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.02.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.01.1' -- Previous release version
		OR [version] = '19.02.1') -- Current release version
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