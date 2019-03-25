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

/*************************************** START PRI-5638 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE OBJECT_ID = OBJECT_ID('[dbo].[daypart_codes]'))
BEGIN
	CREATE TABLE [dbo].[daypart_codes](
		[id] [INT] IDENTITY(1,1) NOT NULL,
		[name] [varchar](50) NOT NULL,
		[is_active] [bit] NOT NULL,
	CONSTRAINT [PK_daypart_codes] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

GO

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'EMN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('EMN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'MDN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('MDN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'EN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('EN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'LN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('LN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'ENLN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('ENLN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'EF')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('EF', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'PA')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('PA', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'PT')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('PT', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'LF')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('LF', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'SYN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('SYN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'OVN')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('OVN', 1)
END

IF NOT EXISTS(SELECT * FROM daypart_codes WHERE [name] = 'DAY')
BEGIN
	INSERT INTO daypart_codes([name], is_active) VALUES('DAY', 1)
END
/*************************************** END PRI-5638 *****************************************************/

/*************************************** START PRI-5636 *****************************************************/
--add OAndO sources
if not exists (select * from inventory_sources where [name] = 'ABC O&O')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('ABC O&O', 1, 3)
end

if not exists (select * from inventory_sources where [name] = 'NBC O&O')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('NBC O&O', 1, 3)
end

if not exists (select * from inventory_sources where [name] = 'KATZ')
begin
	insert into inventory_sources([name], is_active, inventory_source_type) values('KATZ', 1, 3)
end

--remove unnecessary 'Assembly' source
if exists (select * from inventory_sources where [name] = 'Assembly')
begin
	delete from inventory_sources where [name] = 'Assembly'
end

--change type of Barter sources
if exists (select * from inventory_sources where [name] = 'TVB')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'TVB'
end

if exists (select * from inventory_sources where [name] = 'TTNW')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'TTNW'
end

if exists (select * from inventory_sources where [name] = 'CNN')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'CNN'
end

if exists (select * from inventory_sources where [name] = 'Sinclair')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'Sinclair'
end

if exists (select * from inventory_sources where [name] = 'LilaMax')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'LilaMax'
end

if exists (select * from inventory_sources where [name] = 'MLB')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'MLB'
end

if exists (select * from inventory_sources where [name] = 'Ference Media')
begin
	update inventory_sources set inventory_source_type = 2 where [name] = 'Ference Media'
end

--make cpm and audience_id nullable
IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('inventory_file_barter_header') AND name = 'cpm')
BEGIN
	ALTER TABLE inventory_file_barter_header ALTER COLUMN cpm money NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE OBJECT_ID = OBJECT_ID('inventory_file_barter_header') AND name = 'audience_id')
BEGIN
	ALTER TABLE inventory_file_barter_header ALTER COLUMN audience_id int NULL
END
/*************************************** END PRI-5636 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.05.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.04.1' -- Previous release version
		OR [version] = '19.05.1') -- Current release version
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