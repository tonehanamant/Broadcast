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
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'spots_edited_manually'
          AND Object_ID = Object_ID(N'[dbo].[pricing_guide_distribution_open_market_inventory]'))
BEGIN
	DECLARE @default_constraint_name VARCHAR(64)
	SET @default_constraint_name = (SELECT name FROM sys.default_constraints
		WHERE object_id = (SELECT default_object_id FROM sys.columns WHERE Name = N'spots_edited_manually'
				  AND Object_ID = Object_ID(N'[dbo].[pricing_guide_distribution_open_market_inventory]')))
	IF (@default_constraint_name IS NOT NULL)
	BEGIN
		DECLARE @SQL VARCHAR(256)
		SELECT @SQL = 'ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory] DROP [' + @default_constraint_name + ']'
		EXEC (@SQL)
	END
	
	ALTER TABLE [dbo].[pricing_guide_distribution_open_market_inventory] DROP COLUMN [spots_edited_manually]
END
/*************************************** END BCOP-4186 *****************************************************/

/*************************************** START PRI-912 NEW STATIONS*****************************************************/
-- step 1: remove indexes
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_station_inventory_spot_snapshots_station_code' AND object_id = OBJECT_ID('dbo.station_inventory_spot_snapshots'))
begin
	DROP INDEX IX_station_inventory_spot_snapshots_station_code ON dbo.station_inventory_spot_snapshots;
end
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_station_inventory_manifest_station_code' AND object_id = OBJECT_ID('dbo.station_inventory_manifest'))
begin
	DROP INDEX IX_station_inventory_manifest_station_code ON dbo.station_inventory_manifest;
end
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_station_inventory_loaded_station_code' AND object_id = OBJECT_ID('dbo.station_inventory_loaded'))
begin
	DROP INDEX IX_station_inventory_loaded_station_code ON dbo.station_inventory_loaded;
end
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_station_contacts_station_code' AND object_id = OBJECT_ID('dbo.station_contacts'))
begin
	DROP INDEX IX_station_contacts_station_code ON dbo.station_contacts;
end
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_pricing_guide_distribution_open_market_inventory_station_code' AND object_id = OBJECT_ID('dbo.pricing_guide_distribution_open_market_inventory'))
begin
	DROP INDEX IX_pricing_guide_distribution_open_market_inventory_station_code ON dbo.pricing_guide_distribution_open_market_inventory;
end
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_station_inventory_manifest_inventory_source_id' AND object_id = OBJECT_ID('dbo.station_inventory_manifest'))
begin
	DROP INDEX IX_station_inventory_manifest_inventory_source_id ON dbo.station_inventory_manifest;
end

GO

-- step 2: remove foreign keys and unique contraints
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_station_inventory_spot_snapshots_stations') AND parent_object_id = OBJECT_ID(N'dbo.station_inventory_spot_snapshots'))
begin
	ALTER TABLE station_inventory_spot_snapshots DROP CONSTRAINT FK_station_inventory_spot_snapshots_stations
end
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_station_inventory_manifest_stations') AND parent_object_id = OBJECT_ID(N'dbo.station_inventory_manifest'))
begin
	ALTER TABLE station_inventory_manifest DROP CONSTRAINT FK_station_inventory_manifest_stations
end
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_station_inventory_loaded_stations') AND parent_object_id = OBJECT_ID(N'dbo.station_inventory_loaded'))
begin
	ALTER TABLE station_inventory_loaded DROP CONSTRAINT FK_station_inventory_loaded_stations
end
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_station_contacts_stations') AND parent_object_id = OBJECT_ID(N'dbo.station_contacts'))
begin
	ALTER TABLE station_contacts DROP CONSTRAINT FK_station_contacts_stations
end
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_pricing_guide_distribution_open_market_inventory_stations') AND parent_object_id = OBJECT_ID(N'dbo.pricing_guide_distribution_open_market_inventory'))
begin
	ALTER TABLE pricing_guide_distribution_open_market_inventory DROP CONSTRAINT FK_pricing_guide_distribution_open_market_inventory_stations
end
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'dbo.FK_proposal_buy_file_details_stations') AND parent_object_id = OBJECT_ID(N'dbo.proposal_buy_file_details'))
begin
	ALTER TABLE proposal_buy_file_details DROP CONSTRAINT FK_proposal_buy_file_details_stations
end
IF EXISTS (SELECT 1 FROM sys.sysindexes WHERE name = 'UQ_station_contacts_name_company_type_station_code')
begin
	ALTER TABLE station_contacts DROP CONSTRAINT UQ_station_contacts_name_company_type_station_code
end

GO

-- step 3: remove station_code PK and index
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'PK_stations' AND TABLE_NAME = 'stations' AND TABLE_SCHEMA ='dbo' )
BEGIN
	ALTER TABLE stations DROP CONSTRAINT PK_stations
END

GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='PK_stations' AND object_id = OBJECT_ID('dbo.stations'))
begin
	DROP INDEX PK_stations ON dbo.stations;
end

-- step 4: add id column with auto-increment
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'id' AND Object_ID = Object_ID(N'dbo.stations'))
BEGIN
	ALTER TABLE stations ADD id INT IDENTITY(1,1) NOT NULL
END

GO

-- step 5: make id as PK
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'id' AND Object_ID = Object_ID(N'dbo.stations'))
BEGIN
	ALTER TABLE stations ADD CONSTRAINT PK_stations PRIMARY KEY (id)
END

-- step 6: add new foreign key columns
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_spot_snapshots'))
BEGIN
	ALTER TABLE station_inventory_spot_snapshots ADD station_id INT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_manifest'))
BEGIN
	ALTER TABLE station_inventory_manifest ADD station_id INT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_loaded'))
BEGIN
	ALTER TABLE station_inventory_loaded ADD station_id INT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_contacts'))
BEGIN
	ALTER TABLE station_contacts ADD station_id INT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.pricing_guide_distribution_open_market_inventory'))
BEGIN
	ALTER TABLE pricing_guide_distribution_open_market_inventory ADD station_id INT NULL
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.proposal_buy_file_details'))
BEGIN
	ALTER TABLE proposal_buy_file_details ADD station_id INT NULL
END

GO

-- step 7: populate new foreign key columns
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = 'station_id' AND Object_ID = Object_ID('station_inventory_spot_snapshots'))
BEGIN	
	IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = 'station_code' AND Object_ID = Object_ID('station_inventory_spot_snapshots'))
	BEGIN
		EXEC(N'UPDATE station_inventory_spot_snapshots set station_id = (select top 1 id from stations where stations.station_code = station_inventory_spot_snapshots.station_code)')
	END
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_manifest'))
BEGIN
	IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.station_inventory_manifest'))
	BEGIN
		EXEC(N'UPDATE station_inventory_manifest set station_id = (select top 1 id from stations where stations.station_code = station_inventory_manifest.station_code)')
	END
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_loaded'))  
BEGIN
	IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.station_inventory_loaded'))
	BEGIN
		EXEC(N'UPDATE station_inventory_loaded set station_id = (select top 1 id from stations where stations.station_code = station_inventory_loaded.station_code)')
	END
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_contacts'))  
BEGIN
	IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.station_contacts'))
	BEGIN
		EXEC(N'UPDATE station_contacts set station_id = (select top 1 id from stations where stations.station_code = station_contacts.station_code)')
	END
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.pricing_guide_distribution_open_market_inventory'))
BEGIN
	IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.pricing_guide_distribution_open_market_inventory'))
	BEGIN
		EXEC(N'UPDATE pricing_guide_distribution_open_market_inventory set station_id = (select top 1 id from stations where stations.station_code = pricing_guide_distribution_open_market_inventory.station_code)')
	END
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.proposal_buy_file_details'))  
BEGIN
	IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.proposal_buy_file_details'))
	BEGIN
		EXEC(N'UPDATE proposal_buy_file_details set station_id = (select top 1 id from stations where stations.station_code = proposal_buy_file_details.station_code)')
	END
END

GO

-- step 8: make new foreign key columns not nullable
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_spot_snapshots'))
BEGIN	
	ALTER TABLE station_inventory_spot_snapshots ALTER COLUMN station_id INT NOT NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_manifest'))
BEGIN
	ALTER TABLE station_inventory_manifest ALTER COLUMN station_id INT NOT NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_loaded'))
BEGIN
	ALTER TABLE station_inventory_loaded ALTER COLUMN station_id INT NOT NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_contacts'))
BEGIN
	ALTER TABLE station_contacts ALTER COLUMN station_id INT NOT NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.pricing_guide_distribution_open_market_inventory'))
BEGIN
	ALTER TABLE pricing_guide_distribution_open_market_inventory ALTER COLUMN station_id INT NOT NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.proposal_buy_file_details'))
BEGIN
	ALTER TABLE proposal_buy_file_details ALTER COLUMN station_id INT NOT NULL
END

GO

-- step 9: add contrainsts and indexes for new foreign key columns
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_spot_snapshots'))
BEGIN	
	ALTER TABLE station_inventory_spot_snapshots ADD CONSTRAINT FK_station_inventory_spot_snapshots_stations FOREIGN KEY(station_id) REFERENCES stations (id)
	ALTER TABLE station_inventory_spot_snapshots CHECK CONSTRAINT FK_station_inventory_spot_snapshots_stations

	IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_station_inventory_spot_snapshots_station_id' AND object_id = OBJECT_ID('dbo.station_inventory_spot_snapshots'))
	begin
		CREATE INDEX IX_station_inventory_spot_snapshots_station_id ON station_inventory_spot_snapshots (station_id)
	end
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_manifest'))
BEGIN
	ALTER TABLE station_inventory_manifest ADD CONSTRAINT FK_station_inventory_manifest_stations FOREIGN KEY(station_id) REFERENCES stations (id)
	ALTER TABLE station_inventory_manifest CHECK CONSTRAINT FK_station_inventory_manifest_stations

	IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_station_inventory_manifest_station_id' AND object_id = OBJECT_ID('dbo.station_inventory_manifest'))
	begin
		CREATE INDEX IX_station_inventory_manifest_station_id ON station_inventory_manifest (station_id)
	end
	IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_station_inventory_manifest_inventory_source_id' AND object_id = OBJECT_ID('dbo.station_inventory_manifest'))
	begin
		CREATE INDEX IX_station_inventory_manifest_inventory_source_id ON station_inventory_manifest (inventory_source_id)
		INCLUDE (id, station_id, spots_per_week, effective_date, station_inventory_group_id, [file_id], spots_per_day, end_date)
	end
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_inventory_loaded'))
BEGIN
	ALTER TABLE station_inventory_loaded ADD CONSTRAINT FK_station_inventory_loaded_stations FOREIGN KEY(station_id) REFERENCES stations (id)
	ALTER TABLE station_inventory_loaded CHECK CONSTRAINT FK_station_inventory_loaded_stations

	IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_station_inventory_loaded_station_id' AND object_id = OBJECT_ID('dbo.station_inventory_loaded'))
	begin
		CREATE INDEX IX_station_inventory_loaded_station_id ON station_inventory_loaded (station_id)
	end
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.station_contacts'))
BEGIN
	ALTER TABLE station_contacts ADD CONSTRAINT FK_station_contacts_stations FOREIGN KEY(station_id) REFERENCES stations (id)
	ALTER TABLE station_contacts CHECK CONSTRAINT FK_station_contacts_stations

	IF NOT EXISTS (SELECT 1 FROM sys.sysindexes WHERE name = 'UQ_station_contacts_name_company_type_station_id')
	BEGIN
		ALTER TABLE station_contacts ADD CONSTRAINT UQ_station_contacts_name_company_type_station_id UNIQUE NONCLUSTERED 
		(
			[name] ASC,
			[company] ASC,
			[type] ASC,
			[station_id] ASC
		)
	END

	IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_station_contacts_station_id' AND object_id = OBJECT_ID('dbo.station_contacts'))
	begin
		CREATE INDEX IX_station_contacts_station_id ON station_contacts (station_id)
	end
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.pricing_guide_distribution_open_market_inventory'))
BEGIN
	ALTER TABLE pricing_guide_distribution_open_market_inventory ADD CONSTRAINT FK_pricing_guide_distribution_open_market_inventory_stations FOREIGN KEY(station_id) REFERENCES stations (id)
	ALTER TABLE pricing_guide_distribution_open_market_inventory CHECK CONSTRAINT FK_pricing_guide_distribution_open_market_inventory_stations

	IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_pricing_guide_distribution_open_market_inventory_station_id' AND object_id = OBJECT_ID('dbo.pricing_guide_distribution_open_market_inventory'))
	begin
		CREATE INDEX IX_pricing_guide_distribution_open_market_inventory_station_id ON pricing_guide_distribution_open_market_inventory (station_id)
	end
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_id' AND Object_ID = Object_ID(N'dbo.proposal_buy_file_details'))
BEGIN
	ALTER TABLE proposal_buy_file_details ADD CONSTRAINT FK_proposal_buy_file_details_stations FOREIGN KEY(station_id) REFERENCES stations (id)
	ALTER TABLE proposal_buy_file_details CHECK CONSTRAINT FK_proposal_buy_file_details_stations

	IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_proposal_buy_file_details_station_id' AND object_id = OBJECT_ID('dbo.proposal_buy_file_details'))
	begin
		CREATE INDEX IX_proposal_buy_file_details_station_id ON proposal_buy_file_details (station_id)
	end
END

-- step 10: make legacy_call_letters column as unique index
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_stations_legacy_call_letters' AND object_id = OBJECT_ID('dbo.stations'))
BEGIN
	CREATE UNIQUE INDEX IX_stations_legacy_call_letters ON stations (legacy_call_letters)
END

GO

-- step 11: make station_code nullable
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.stations'))
BEGIN
	ALTER TABLE stations ALTER COLUMN station_code INT NULL
END

-- step 12: remove old foreign key columns
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.station_inventory_spot_snapshots'))
BEGIN	
	ALTER TABLE station_inventory_spot_snapshots DROP COLUMN station_code
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.station_inventory_manifest'))
BEGIN
	ALTER TABLE station_inventory_manifest DROP COLUMN station_code
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.station_inventory_loaded'))
BEGIN
	ALTER TABLE station_inventory_loaded DROP COLUMN station_code
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.station_contacts'))
BEGIN
	ALTER TABLE station_contacts DROP COLUMN station_code
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.pricing_guide_distribution_open_market_inventory'))
BEGIN
	ALTER TABLE pricing_guide_distribution_open_market_inventory DROP COLUMN station_code
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'station_code' AND Object_ID = Object_ID(N'dbo.proposal_buy_file_details'))
BEGIN
	ALTER TABLE proposal_buy_file_details DROP COLUMN station_code
END
/*************************************** END PRI-912 NEW STATIONS*****************************************************/

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