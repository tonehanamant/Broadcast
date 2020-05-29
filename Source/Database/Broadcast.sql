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

/*************************************** START BP1-3 *****************************************************/

-- update mappings that were loaded with RedBee genres
update program_name_mappings
set genre_id = (select top 1 maestro_genre_id from genre_mappings where mapped_genre_id = genre_id)
where genre_id in (select id from genres where program_source_id = 2) -- RedBee

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'start_date' AND OBJECT_ID = OBJECT_ID('station_inventory_manifest_daypart_programs'))
BEGIN
	ALTER TABLE station_inventory_manifest_daypart_programs
	ALTER COLUMN [start_date] date NULL
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'end_date' AND OBJECT_ID = OBJECT_ID('station_inventory_manifest_daypart_programs'))
BEGIN
	ALTER TABLE station_inventory_manifest_daypart_programs
	ALTER COLUMN [end_date] date NULL
END

update program_sources set name = 'Mapped' where name = 'Maestro'
update program_sources set name = 'Forecasted' where name = 'RedBee'

/*************************************** END BP1-3 *****************************************************/
/*************************************** START BP1-7 *****************************************************/

GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'created_by' AND OBJECT_ID = OBJECT_ID('program_name_mappings'))
BEGIN
	ALTER TABLE program_name_mappings
		ADD created_by VARCHAR(63) NULL

	ALTER TABLE program_name_mappings
		ADD created_at datetime NULL

	ALTER TABLE program_name_mappings
		ADD modified_by VARCHAR(63) NULL

	ALTER TABLE program_name_mappings
		ADD modified_at DATETIME NULL

	EXEC('UPDATE program_name_mappings SET created_by = ''system_column_add'', created_at = SYSDATETIME()')

	ALTER TABLE program_name_mappings
		ALTER COLUMN created_by VARCHAR(63) NOT NULL

	ALTER TABLE program_name_mappings
		ALTER COLUMN created_at DATETIME NOT NULL
END

GO

/*************************************** END BP1-7 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.07.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.06.1' -- Previous release version
		OR [version] = '20.07.1') -- Current release version
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