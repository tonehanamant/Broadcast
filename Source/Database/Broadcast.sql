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

/*************************************** START PRI-5325 *****************************************************/
IF EXISTS(SELECT * FROM sys.columns 
          WHERE Name = N'spots_edited_manually'
          AND Object_ID = Object_ID(N'[dbo].[pricing_guide_distribution_open_market_inventory]'))
BEGIN
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory] DROP [DF__pricing_g__spots__1C3EB9D7]
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory] DROP COLUMN [spots_edited_manually]
END
/*************************************** END BCOP-4186 *****************************************************/

/*************************************** START PRI-912 *****************************************************/
IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('stations') AND name = 'affiliation')
BEGIN
	ALTER TABLE stations ALTER COLUMN affiliation [varchar](7) NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[station_inventory_manifest_weeks]'))
BEGIN
	CREATE TABLE [dbo].[station_inventory_manifest_weeks]
	(
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[station_inventory_manifest_id] [INT] NOT NULL,
		[media_week_id] [int] NOT NULL,
		[spots] [INT] NOT NULL
	 CONSTRAINT [PK_station_inventory_manifest_weeks] PRIMARY KEY CLUSTERED 
	 (
		[id] ASC
	 ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[station_inventory_manifest_weeks] WITH CHECK ADD CONSTRAINT [FK_station_inventory_manifest_weeks_station_inventory_manifest] FOREIGN KEY([station_inventory_manifest_id])
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ALTER TABLE [dbo].[station_inventory_manifest_weeks] CHECK CONSTRAINT [FK_station_inventory_manifest_weeks_station_inventory_manifest]
	CREATE INDEX IX_station_inventory_manifest_weeks_station_inventory_manifest_id ON [station_inventory_manifest_weeks] ([station_inventory_manifest_id])

	ALTER TABLE [dbo].[station_inventory_manifest_weeks] WITH CHECK ADD CONSTRAINT [FK_station_inventory_manifest_weeks_media_weeks] FOREIGN KEY([media_week_id])
	REFERENCES [dbo].[media_weeks] ([id])
	ALTER TABLE [dbo].[station_inventory_manifest_weeks] CHECK CONSTRAINT [FK_station_inventory_manifest_weeks_media_weeks]
	CREATE INDEX IX_station_inventory_manifest_weeks_media_week_id ON [station_inventory_manifest_weeks] ([media_week_id])
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('station_inventory_manifest_rates') AND name = 'rate')
BEGIN
	EXEC sp_rename 'station_inventory_manifest_rates.rate', 'spot_cost', 'COLUMN';
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('station_inventory_manifest_audiences') AND name = 'rate')
BEGIN
	EXEC sp_rename 'station_inventory_manifest_audiences.rate', 'cpm', 'COLUMN';
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('station_inventory_manifest') AND name = 'comment')
BEGIN
	ALTER TABLE station_inventory_manifest ADD comment varchar(255) NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('stations') AND name = 'market_code')
BEGIN
	ALTER TABLE stations ALTER COLUMN market_code smallint NULL
END
/*************************************** END PRI-912 *****************************************************/


/*************************************** START PRI-214 *****************************************************/
if not exists (select * from inventory_sources where [name] = 'CNN')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('CNN', 1, 1)
end

if not exists (select * from inventory_sources where [name] = 'TTNW')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('TTNW', 1, 1)
end

if not exists (select * from inventory_sources where [name] = 'TVB')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('TVB', 1, 1)
end

if not exists (select * from inventory_sources where [name] = 'Sinclair')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('Sinclair', 1, 1)
end

if not exists (select * from inventory_sources where [name] = 'LilaMax')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('LilaMax', 1, 1)
end

if not exists (select * from inventory_sources where [name] = 'MLB')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('MLB', 1, 1)
end

if not exists (select * from inventory_sources where [name] = 'Ference Media')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('Ference Media', 1, 1)
end

IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('inventory_files') AND name = 'play_back_type')
BEGIN
	ALTER TABLE [inventory_files] DROP COLUMN [play_back_type]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('inventory_files') AND name = 'sweep_book_id')
BEGIN
	DROP INDEX [inventory_files].[IX_inventory_files_sweep_book_id]
	ALTER TABLE [inventory_files] DROP CONSTRAINT [FK_inventory_files_media_months]
	ALTER TABLE [inventory_files] DROP COLUMN [sweep_book_id]
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[inventory_file_barter_header]'))
BEGIN
	CREATE TABLE [dbo].[inventory_file_barter_header]
	(
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[inventory_file_id] [INT] NOT NULL,
		[daypart_code] [VARCHAR](8) NOT NULL,
		[effective_date] [DATETIME] NOT NULL,
		[end_date] [DATETIME] NOT NULL,
		[cpm] [MONEY] NOT NULL,
		[audience_id] INT NOT NULL,
		[contracted_daypart_id] INT NOT NULL,
		[share_projection_book_id] INT NOT NULL,
		[hut_projection_book_id] INT NULL,
		[playback_type] INT NOT NULL
	 CONSTRAINT [PK_inventory_file_barter_header] PRIMARY KEY CLUSTERED 
	 (
		[id] ASC
	 ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
	ALTER TABLE [dbo].[inventory_file_barter_header] WITH CHECK ADD  CONSTRAINT [FK_inventory_file_barter_header_inventory_files] FOREIGN KEY([inventory_file_id])
	REFERENCES [dbo].[inventory_files] ([id])
	ALTER TABLE [dbo].[inventory_file_barter_header] CHECK CONSTRAINT [FK_inventory_file_barter_header_inventory_files]
	CREATE INDEX IX_inventory_file_barter_header_inventory_file_id ON [inventory_file_barter_header] ([inventory_file_id])	
	
	ALTER TABLE [dbo].[inventory_file_barter_header]  WITH CHECK ADD  CONSTRAINT [FK_inventory_file_barter_header_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])
	ALTER TABLE [dbo].[inventory_file_barter_header] CHECK CONSTRAINT [FK_inventory_file_barter_header_audiences]
	CREATE INDEX IX_inventory_file_barter_header_audience_id ON [inventory_file_barter_header] ([audience_id])

	ALTER TABLE [dbo].[inventory_file_barter_header]  WITH CHECK ADD  CONSTRAINT [FK_inventory_file_barter_header_dayparts] FOREIGN KEY([contracted_daypart_id])
	REFERENCES [dbo].[dayparts] ([id])
	ALTER TABLE [dbo].[inventory_file_barter_header] CHECK CONSTRAINT [FK_inventory_file_barter_header_dayparts]
	CREATE INDEX IX_inventory_file_barter_header_contracted_daypart_id ON [inventory_file_barter_header] ([contracted_daypart_id])

	ALTER TABLE [dbo].[inventory_file_barter_header]  WITH CHECK ADD  CONSTRAINT [FK_inventory_file_barter_header_media_months] FOREIGN KEY([share_projection_book_id])
	REFERENCES [dbo].[media_months] ([id])
	ALTER TABLE [dbo].[inventory_file_barter_header] CHECK CONSTRAINT [FK_inventory_file_barter_header_media_months]
	CREATE INDEX IX_inventory_file_barter_header_share_projection_book_id ON [inventory_file_barter_header] ([share_projection_book_id])
	
	ALTER TABLE [dbo].[inventory_file_barter_header]  WITH CHECK ADD  CONSTRAINT [FK_inventory_file_barter_header_media_months_hut_book] FOREIGN KEY([hut_projection_book_id])
	REFERENCES [dbo].[media_months] ([id])
	ALTER TABLE [dbo].[inventory_file_barter_header] CHECK CONSTRAINT [FK_inventory_file_barter_header_media_months_hut_book]
	CREATE INDEX IX_inventory_file_barter_header_hut_projection_book_id ON [inventory_file_barter_header] ([hut_projection_book_id])
END

IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[inventory_file_problems]'))
BEGIN
	CREATE TABLE [dbo].[inventory_file_problems](
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[inventory_file_id] [int] NOT NULL,
		[problem_description] [nvarchar](max) NOT NULL,
	 CONSTRAINT [PK_inventory_file_problems] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	ALTER TABLE [dbo].[inventory_file_problems] WITH CHECK ADD  CONSTRAINT [FK_inventory_file_problems_inventory_files] FOREIGN KEY([inventory_file_id])
	REFERENCES [dbo].[inventory_files] ([id])
	ALTER TABLE [dbo].[inventory_file_problems] CHECK CONSTRAINT [FK_inventory_file_problems_inventory_files]
	CREATE INDEX IX_inventory_file_problems_inventory_file_id ON [inventory_file_problems] ([inventory_file_id])	
END

/*************************************** END PRI-214 *****************************************************/


/*************************************** START PRI-1071 *****************************************************/

IF NOT EXISTS (SELECT * 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_NAME='UQ_station_contacts_name_company_type_station_code')
BEGIN

	WITH CTE AS
	(
		SELECT *, ROW_NUMBER() OVER (PARTITION BY name,company,type,station_code ORDER BY modified_date desc) AS RN
		FROM station_contacts
	)

	DELETE FROM cte
	WHERE rn > 1

	ALTER TABLE station_contacts
	ADD CONSTRAINT UQ_station_contacts_name_company_type_station_code UNIQUE (name,company,type,station_code);
END

/*************************************** END PRI-1071 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.04.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.03.1' -- Previous release version
		OR [version] = '19.04.1') -- Current release version
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