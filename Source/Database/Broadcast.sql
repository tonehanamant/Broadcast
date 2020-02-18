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

/*************************************** START - PRI-20845 ****************************************************/
IF OBJECT_ID('shared_folder_files') IS NULL
BEGIN
	CREATE TABLE [dbo].[shared_folder_files](
		[id] uniqueidentifier NOT NULL,
		[folder_path] varchar(MAX) NOT NULL,
		[file_name] varchar(255) NOT NULL,
		[file_extension] varchar(15) NOT NULL,
		[file_media_type] varchar(255) NOT NULL,
		[file_usage] int NOT NULL,
		[created_date] datetime2(7) NOT NULL,
		[created_by] varchar(63) NOT NULL,
	 CONSTRAINT [PK_shared_folder_files] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
END
/*************************************** END - PRI-20845 ****************************************************/

/*************************************** START - PRI-17740 ****************************************************/

IF OBJECT_ID('nti_universe_headers') IS NULL
BEGIN
	CREATE TABLE [dbo].[nti_universe_headers](
		[id] int IDENTITY(1,1) NOT NULL,
		[created_date] datetime2(7) NOT NULL,
		[created_by] varchar(63) NOT NULL,
		[year] int NOT NULL,
		[month] int NOT NULL,
		[week_number] int NOT NULL
	 CONSTRAINT [PK_nti_universe_headers] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
END

IF OBJECT_ID('nti_universe_details') IS NULL
BEGIN
	CREATE TABLE [dbo].[nti_universe_details](
		[id] int IDENTITY(1,1) NOT NULL,
		[nti_universe_header_id] int NOT NULL,
		[nti_audience_id] int NOT NULL,
		[nti_audience_code] varchar(15) NOT NULL,
		[universe] float NOT NULL,
	 CONSTRAINT [PK_nti_universe_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[nti_universe_details]  WITH CHECK ADD CONSTRAINT [FK_nti_universe_details_nti_universe_headers] FOREIGN KEY([nti_universe_header_id])
	REFERENCES [dbo].[nti_universe_headers] ([id])

	ALTER TABLE [dbo].[nti_universe_details] CHECK CONSTRAINT [FK_nti_universe_details_nti_universe_headers]
END

IF OBJECT_ID('nti_universe_audience_mappings') IS NULL
BEGIN
	CREATE TABLE [dbo].[nti_universe_audience_mappings](
		[id] int IDENTITY(1,1) NOT NULL,
		[audience_id] int NOT NULL,
		[nti_audience_code] varchar(15) NOT NULL,
	 CONSTRAINT [PK_nti_universe_audience_mappings] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[nti_universe_audience_mappings]  WITH CHECK ADD CONSTRAINT [FK_nti_universe_audience_mappings_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])

	ALTER TABLE [dbo].[nti_universe_audience_mappings] CHECK CONSTRAINT [FK_nti_universe_audience_mappings_audiences]
END

IF OBJECT_ID('nti_universes') IS NULL
BEGIN
	CREATE TABLE [dbo].[nti_universes](
		[id] int IDENTITY(1,1) NOT NULL,
		[audience_id] int NOT NULL,
		[universe] float NOT NULL,
		[nti_universe_header_id] int NOT NULL
	 CONSTRAINT [PK_nti_universes] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[nti_universes]  WITH CHECK ADD CONSTRAINT [FK_nti_universes_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])

	ALTER TABLE [dbo].[nti_universes] CHECK CONSTRAINT [FK_nti_universes_audiences]

	ALTER TABLE [dbo].[nti_universes]  WITH CHECK ADD CONSTRAINT [FK_nti_universes_nti_universe_headers] FOREIGN KEY([nti_universe_header_id])
	REFERENCES [dbo].[nti_universe_headers] ([id])

	ALTER TABLE [dbo].[nti_universes] CHECK CONSTRAINT [FK_nti_universes_nti_universe_headers]
END

------------------------------ START HH ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''HH'');
  declare @nti_audience_code varchar(15) = ''HH'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END HH ------------------------------

------------------------------ START A18+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A18+ ------------------------------

------------------------------ START A18-20 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-20'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-20'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A18-20 ------------------------------

------------------------------ START A18-34 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-34'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-34'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-34'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-34'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-34'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-34'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-34'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-34'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A18-34 ------------------------------

------------------------------ START A18-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-49'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A18-49 ------------------------------

------------------------------ START A18-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A18-54'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A18-54 ------------------------------

------------------------------ START A21-24 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-24'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-24'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A21-24 ------------------------------

------------------------------ START A21-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A21-49'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A21-49 ------------------------------

------------------------------ START A25+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A25+ ------------------------------

------------------------------ START A25-34 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-34'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-34'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-34'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-34'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A25-34 ------------------------------

------------------------------ START A25-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-49'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-49'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-49'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-49'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-49'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-49'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-49'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-49'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-49'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-49'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A25-49 ------------------------------

------------------------------ START A25-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-54'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A25-54 ------------------------------

------------------------------ START A25-64 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A25-64'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A25-64 ------------------------------

------------------------------ START A35+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A35+ ------------------------------

------------------------------ START A35-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-49'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-49'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-49'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-49'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-49'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-49'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A35-49 ------------------------------

------------------------------ START A35-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-54'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-54'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-54'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-54'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-54'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-54'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-54'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-54'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A35-54 ------------------------------

------------------------------ START A35-64 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-64'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-64'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-64'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-64'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-64'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-64'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-64'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-64'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-64'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A35-64'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A35-64 ------------------------------

------------------------------ START A50+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A50+'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A50+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A50+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A50+'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A50+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A50+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A50+ ------------------------------

------------------------------ START A50-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A50-54'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A50-54'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A50-54 ------------------------------

------------------------------ START A55+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A55+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A55+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A55+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A55+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A55+ ------------------------------

------------------------------ START A55-64 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A55-64'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A55-64'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A55-64 ------------------------------

------------------------------ START A65+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A65+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''A65+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END A65+ ------------------------------

------------------------------ START M12-14 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M12-14'');
  declare @nti_audience_code varchar(15) = ''M12-14'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M12-14 ------------------------------

------------------------------ START M15-17 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M15-17'');
  declare @nti_audience_code varchar(15) = ''M15-17'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M15-17 ------------------------------

------------------------------ START M18+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18+'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18+'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18+'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18+'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18+'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18+'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18+'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18+'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M18+ ------------------------------

------------------------------ START M18-20 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-20'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M18-20 ------------------------------

------------------------------ START M18-34 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-34'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-34'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-34'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-34'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M18-34 ------------------------------

------------------------------ START M18-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-49'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-49'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-49'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-49'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-49'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-49'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-49'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M18-49 ------------------------------

------------------------------ START M18-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-54'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-54'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-54'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-54'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-54'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-54'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-54'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M18-54'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M18-54 ------------------------------

------------------------------ START M21-24 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M21-24'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M21-24 ------------------------------

------------------------------ START M21-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M21-49'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M21-49'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M21-49'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M21-49'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M21-49'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M21-49'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M21-49 ------------------------------

------------------------------ START M25+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25+'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25+'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25+'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25+'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25+'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25+'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M25+ ------------------------------

------------------------------ START M25-34 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-34'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-34'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M25-34 ------------------------------

------------------------------ START M25-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-49'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-49'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-49'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-49'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-49'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M25-49 ------------------------------

------------------------------ START M25-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-54'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-54'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-54'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-54'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-54'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-54'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M25-54 ------------------------------

------------------------------ START M25-64 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-64'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-64'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-64'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-64'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-64'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-64'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M25-64'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M25-64 ------------------------------

------------------------------ START M35+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35+'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35+'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35+'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35+'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M35+ ------------------------------

------------------------------ START M35-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-49'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-49'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-49'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M35-49 ------------------------------

------------------------------ START M35-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-54'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-54'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-54'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-54'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M35-54 ------------------------------

------------------------------ START M35-64 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-64'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-64'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-64'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-64'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M35-64'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M35-64 ------------------------------

------------------------------ START M50+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M50+'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M50+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M50+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M50+ ------------------------------

------------------------------ START M50-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M50-54'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M50-54 ------------------------------

------------------------------ START M55+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M55+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M55+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M55+ ------------------------------

------------------------------ START M55-64 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M55-64'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M55-64 ------------------------------

------------------------------ START M65+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''M65+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END M65+ ------------------------------

------------------------------ START W12-14 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W12-14'');
  declare @nti_audience_code varchar(15) = ''F12-14'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W12-14 ------------------------------

------------------------------ START W15-17 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W15-17'');
  declare @nti_audience_code varchar(15) = ''F15-17'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W15-17 ------------------------------

------------------------------ START W18+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18+'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18+'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18+'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18+'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18+'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18+'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18+'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18+'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W18+ ------------------------------

------------------------------ START W18-20 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-20'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W18-20 ------------------------------

------------------------------ START W18-34 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-34'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-34'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-34'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-34'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W18-34 ------------------------------

------------------------------ START W18-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-49'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-49'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-49'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-49'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-49'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-49'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-49'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W18-49 ------------------------------

------------------------------ START W18-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-54'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-54'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-54'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-54'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-54'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-54'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-54'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W18-54'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W18-54 ------------------------------

------------------------------ START W21-24 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W21-24'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W21-24 ------------------------------

------------------------------ START W21-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W21-49'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W21-49'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W21-49'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W21-49'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W21-49'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W21-49'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W21-49 ------------------------------

------------------------------ START W25+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25+'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25+'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25+'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25+'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25+'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25+'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W25+ ------------------------------

------------------------------ START W25-34 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-34'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-34'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W25-34 ------------------------------

------------------------------ START W25-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-49'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-49'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-49'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-49'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-49'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W25-49 ------------------------------

------------------------------ START W25-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-54'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-54'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-54'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-54'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-54'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-54'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W25-54 ------------------------------

------------------------------ START W25-64 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-64'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-64'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-64'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-64'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-64'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-64'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W25-64'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W25-64 ------------------------------

------------------------------ START W35+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35+'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35+'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35+'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35+'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W35+ ------------------------------

------------------------------ START W35-49 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-49'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-49'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-49'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W35-49 ------------------------------

------------------------------ START W35-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-54'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-54'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-54'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-54'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W35-54 ------------------------------

------------------------------ START W35-64 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-64'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-64'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-64'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-64'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W35-64'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W35-64 ------------------------------

------------------------------ START W50+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W50+'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W50+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W50+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W50+ ------------------------------

------------------------------ START W50-54 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W50-54'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W50-54 ------------------------------

------------------------------ START W55+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W55+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W55+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W55+ ------------------------------

------------------------------ START W55-64 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W55-64'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W55-64 ------------------------------

------------------------------ START W65+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''W65+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END W65+ ------------------------------

------------------------------ START C2-5 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''C2-5'');
  declare @nti_audience_code varchar(15) = ''F2-5'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''C2-5'');
  declare @nti_audience_code varchar(15) = ''M2-5'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END C2-5 ------------------------------

------------------------------ START C6-11 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''C6-11'');
  declare @nti_audience_code varchar(15) = ''F6-8'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''C6-11'');
  declare @nti_audience_code varchar(15) = ''F9-11'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''C6-11'');
  declare @nti_audience_code varchar(15) = ''M6-8'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''C6-11'');
  declare @nti_audience_code varchar(15) = ''M9-11'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END C6-11 ------------------------------

------------------------------ START P12-14 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P12-14'');
  declare @nti_audience_code varchar(15) = ''F12-14'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P12-14'');
  declare @nti_audience_code varchar(15) = ''M12-14'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END P12-14 ------------------------------

------------------------------ START P15-17 ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P15-17'');
  declare @nti_audience_code varchar(15) = ''F15-17'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P15-17'');
  declare @nti_audience_code varchar(15) = ''M15-17'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END P15-17 ------------------------------

/*************************************** END - PRI-17740 ****************************************************/

/*************************************** START PRI-20798 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE  object_id = OBJECT_ID('plan_version_weeks') AND name = 'weekly_adu')
BEGIN	
	ALTER TABLE plan_version_weeks
	ADD weekly_adu INT

	EXEC('UPDATE plan_version_weeks SET weekly_adu = 0')

	ALTER TABLE plan_version_weeks
	ALTER COLUMN weekly_adu INT NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE  object_id = OBJECT_ID('plan_versions') AND name = 'is_adu_enabled')
BEGIN
	ALTER TABLE plan_versions
	ADD is_adu_enabled BIT 

	EXEC('UPDATE plan_versions
	SET is_adu_enabled = 0')

	ALTER TABLE plan_versions
	ALTER COLUMN is_adu_enabled BIT NOT NULL
END
/*************************************** END PRI-20798 *****************************************************/

/*************************************** START - PRI-21850 ****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE  object_id = OBJECT_ID('daypart_defaults') AND name = 'code')
BEGIN	

	EXEC('ALTER TABLE daypart_defaults
	ADD code VARCHAR(15)

	ALTER TABLE daypart_defaults
	ADD name VARCHAR(63)')

	EXEC ('UPDATE daypart_defaults
	SET name = dayparts.name,
	code = dayparts.code
	FROM daypart_defaults
	INNER JOIN dayparts
	ON dayparts.id = daypart_defaults.daypart_id')

	EXEC('ALTER TABLE daypart_defaults
	ALTER COLUMN code VARCHAR(15) NOT NULL

	ALTER TABLE daypart_defaults
	ALTER COLUMN name VARCHAR(63) NOT NULL')

	-- Delete delete duplicate dayparts

	EXEC('
	DECLARE @DAYPART_TEXT VARCHAR(63)
	DECLARE @MIN_ID INTEGER

	SELECT daypart_text INTO #daypartduplicated FROM dayparts
	GROUP BY daypart_text
	HAVING COUNT(*) > 1

	select TOP 1 @DAYPART_TEXT = daypart_text from #daypartduplicated

	while @@rowcount <> 0
	begin
		select @MIN_ID = MIN(id) from dayparts where daypart_text = @DAYPART_TEXT

		UPDATE daypart_defaults
		SET daypart_id = @MIN_ID
		FROM daypart_defaults
		INNER JOIN dayparts
		ON dayparts.id = daypart_defaults.daypart_id
		WHERE daypart_text = @DAYPART_TEXT

    UPDATE station_inventory_manifest_dayparts
		SET daypart_id = @MIN_ID
		FROM station_inventory_manifest_dayparts
		INNER JOIN dayparts
		ON dayparts.id = station_inventory_manifest_dayparts.daypart_id
		WHERE daypart_text = @DAYPART_TEXT

    UPDATE proposal_version_details 
		SET daypart_id = @MIN_ID
		FROM proposal_version_details 
		INNER JOIN dayparts
		ON dayparts.id = proposal_version_details.daypart_id
		WHERE daypart_text = @DAYPART_TEXT

    UPDATE schedule_details
		SET daypart_id = @MIN_ID
		FROM schedule_details
		INNER JOIN dayparts
		ON dayparts.id = schedule_details.daypart_id
		WHERE daypart_text = @DAYPART_TEXT

		DELETE FROM daypart_days WHERE daypart_id IN (SELECT id from dayparts where daypart_text = @DAYPART_TEXT AND id <> @MIN_ID)
	
		DELETE from dayparts where daypart_text = @DAYPART_TEXT AND id <> @MIN_ID
		
		DELETE FROM #daypartduplicated where daypart_text = @DAYPART_TEXT

    		select TOP 1 @DAYPART_TEXT = daypart_text from #daypartduplicated
	end
 
	DROP TABLE #daypartduplicated
	')
END
/*************************************** END - PRI-21850 ****************************************************/

/*************************************** START PRI-15880 *****************************************************/

IF OBJECT_ID('inventory_file_program_enrichment_jobs') IS NOT NULL
BEGIN
	DROP TABLE inventory_file_program_enrichment_jobs
END

IF OBJECT_ID('inventory_programs_by_file_jobs') IS NULL
BEGIN

	CREATE TABLE [dbo].[inventory_programs_by_file_jobs]
	(
		[id] INT IDENTITY(1,1) NOT NULL,
		[status] INT NOT NULL,
		[inventory_file_id] INT NOT NULL,
		[queued_at] DATETIME2(7) NOT NULL,
		[queued_by] VARCHAR(63) NOT NULL,
		[completed_at] DATETIME2(7) NULL,
		CONSTRAINT [PK_inventory_programs_by_file_jobs] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_programs_by_file_jobs]  WITH CHECK ADD  CONSTRAINT [FK_inventory_programs_by_file_jobs_inventory_file] FOREIGN KEY([inventory_file_id])
	REFERENCES [dbo].[inventory_files] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[inventory_programs_by_file_jobs] CHECK CONSTRAINT [FK_inventory_programs_by_file_jobs_inventory_file]

END

IF OBJECT_ID('inventory_programs_by_file_job_notes') IS NULL
BEGIN

	CREATE TABLE [dbo].[inventory_programs_by_file_job_notes]
	(
		[id] INT IDENTITY(1,1) NOT NULL,
		[job_id] INT NOT NULL,
		[text] VARCHAR(MAX) NOT NULL,
		[created_at] DATETIME2(7) NOT NULL,
		CONSTRAINT [PK_inventory_programs_by_file_job_notes] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_programs_by_file_job_notes]  WITH CHECK ADD  CONSTRAINT [FK_inventory_programs_by_file_job_notes_job] FOREIGN KEY([job_id])
	REFERENCES [dbo].[inventory_programs_by_file_jobs] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[inventory_programs_by_file_job_notes] CHECK CONSTRAINT [FK_inventory_programs_by_file_job_notes_job]

END

IF OBJECT_ID('inventory_programs_by_source_jobs') IS NULL
BEGIN

	CREATE TABLE [dbo].[inventory_programs_by_source_jobs]
	(
		[id] INT IDENTITY(1,1) NOT NULL,
		[status] INT NOT NULL,
		[inventory_source_id] INT NOT NULL,
		[start_date] DATE NOT NULL,
		[end_date] DATE NOT NULL,
		[queued_at] DATETIME2(7) NOT NULL,
		[queued_by] VARCHAR(63) NOT NULL,
		[completed_at] DATETIME2(7) NULL,
		CONSTRAINT [PK_inventory_programs_by_source_jobs] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_programs_by_source_jobs]  WITH CHECK ADD  CONSTRAINT [FK_inventory_programs_by_source_jobs_inventory_source] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[inventory_programs_by_source_jobs] CHECK CONSTRAINT [FK_inventory_programs_by_source_jobs_inventory_source]

END

IF OBJECT_ID('inventory_programs_by_source_job_notes') IS NULL
BEGIN

	CREATE TABLE [dbo].[inventory_programs_by_source_job_notes]
	(
		[id] INT IDENTITY(1,1) NOT NULL,
		[job_id] INT NOT NULL,
		[text] VARCHAR(MAX) NOT NULL,
		[created_at] DATETIME2(7) NOT NULL,
		CONSTRAINT [PK_inventory_programs_by_source_job_notes] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[inventory_programs_by_source_job_notes]  WITH CHECK ADD  CONSTRAINT [FK_inventory_programs_by_source_job_notes_job] FOREIGN KEY([job_id])
	REFERENCES [dbo].[inventory_programs_by_source_jobs] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[inventory_programs_by_source_job_notes] CHECK CONSTRAINT [FK_inventory_programs_by_source_job_notes_job]

END

GO

/*************************************** END PRI-15880 *****************************************************/

/*************************************** START PRI-20794 *****************************************************/
IF OBJECT_ID('nti_to_nsi_conversion_rates') IS NULL
BEGIN
	CREATE TABLE [dbo].[nti_to_nsi_conversion_rates](
		[id] int IDENTITY(1,1) NOT NULL,
		[conversion_rate] float NOT NULL,
		[daypart_default_id] int NOT NULL,
		[media_month_id] int NOT NULL,
	 CONSTRAINT [PK_nti_to_nsi_conversion_rates] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[nti_to_nsi_conversion_rates]  WITH CHECK ADD CONSTRAINT [FK_nti_to_nsi_conversion_rates_daypart_defaults] FOREIGN KEY([daypart_default_id])
	REFERENCES [dbo].[daypart_defaults] ([id])

	ALTER TABLE [dbo].[nti_to_nsi_conversion_rates]  WITH CHECK ADD CONSTRAINT [FK_nti_to_nsi_conversion_rates_media_months] FOREIGN KEY([media_month_id])
	REFERENCES [dbo].[media_months] ([id])

	ALTER TABLE [dbo].[nti_to_nsi_conversion_rates]
	ADD CONSTRAINT [UQ_nti_to_nsi_conversion_rates_daypart_default_id_media_month_id] 
	UNIQUE ([daypart_default_id], [media_month_id])

	EXEC('
		DECLARE @media_month_id int = (SELECT TOP 1 id FROM [dbo].[media_months] WHERE year = 2019 AND month = 2 );

		DECLARE @daypart_default_id int = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''EMN'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.85, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''MDN'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.85, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''AMN'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.85, @daypart_default_id, @media_month_id);
		
		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''EN'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''PMN'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''LN'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''ENLN'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);
		
		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''TDN'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.80, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''EF'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''PA'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''PT'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''LF'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''SYN'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''DAY'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''OVN'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''EM'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.85, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''ROSS'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''SPORTS'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''ROSP'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);

		SET @daypart_default_id = (SELECT TOP 1 daypart_defaults.id FROM daypart_defaults WHERE daypart_defaults.code = ''TDNS'');
		INSERT INTO nti_to_nsi_conversion_rates(conversion_rate, daypart_default_id, media_month_id) values(0.75, @daypart_default_id, @media_month_id);
	')
END

/*************************************** END PRI-20794 *****************************************************/

/*************************************** START PRI-20800 *****************************************************/
IF OBJECT_ID('plan_version_flight_days') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_flight_days](
		[id] int IDENTITY(1,1) NOT NULL,
		[plan_version_id] int NOT NULL,
		[day_id] int NOT NULL,
	 CONSTRAINT [PK_plan_version_flight_days] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_flight_days]  WITH CHECK ADD CONSTRAINT [FK_plan_version_flight_days_plan_versions] FOREIGN KEY([plan_version_id])
	REFERENCES [dbo].[plan_versions] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[plan_version_flight_days]  WITH CHECK ADD CONSTRAINT [FK_plan_version_flight_days_days] FOREIGN KEY([day_id])
	REFERENCES [dbo].[days] ([id])

	ALTER TABLE [dbo].[plan_version_flight_days]
	ADD CONSTRAINT [UQ_plan_version_flight_days_plan_version_id_day_id] 
	UNIQUE ([plan_version_id], [day_id])

	EXEC('
		INSERT INTO [dbo].[plan_version_flight_days] (plan_version_id, day_id)
		SELECT pv.id AS plan_version_id, d.id AS day_id
		FROM plans AS p
		INNER JOIN plan_versions AS pv ON p.id = pv.plan_id
		CROSS JOIN days AS d
		WHERE p.latest_version_id = pv.id
	')
END
/*************************************** END PRI-20800 *****************************************************/

/*************************************** START PRI-20999 *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE  object_id = OBJECT_ID('plan_version_pricing_parameters') AND name = 'margin')
BEGIN	
	ALTER TABLE plan_version_pricing_parameters
	ADD margin float
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE  object_id = OBJECT_ID('plan_version_pricing_executions') AND name = 'margin')
BEGIN	
	ALTER TABLE plan_version_pricing_executions
	ADD margin float
END

/*************************************** END PRI-20999 *****************************************************/

/*************************************** START PRI-22782 *****************************************************/
IF EXISTS(SELECT 1 FROM sys.columns WHERE  object_id = OBJECT_ID('plan_version_pricing_parameters') AND name = 'margin')
BEGIN

	EXEC('UPDATE plan_version_pricing_parameters
	SET margin = 0
	WHERE margin IS NULL')
	
	ALTER TABLE plan_version_pricing_parameters
	ALTER COLUMN margin float NOT NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE  object_id = OBJECT_ID('plan_version_pricing_executions') AND name = 'margin')
BEGIN	
	
	EXEC('UPDATE plan_version_pricing_executions
	SET margin = 0
	WHERE margin IS NULL')

	ALTER TABLE plan_version_pricing_executions
	ALTER COLUMN margin float NOT NULL
END
/*************************************** END PRI-22782 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.03.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.02.1' -- Previous release version
		OR [version] = '20.03.1') -- Current release version
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