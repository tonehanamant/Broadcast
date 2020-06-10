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

/*************************************** START BP-265 *****************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('vpvh_quarters'))
BEGIN
	CREATE TABLE vpvh_quarters (
		id INT NOT NULL IDENTITY,
		audience_id INT NOT NULL,
		year INT NOT NULL,
		quarter INT NOT NULL,
		pm_news FLOAT NOT NULL, 
		am_news FLOAT NOT NULL, 
		syn_all FLOAT NOT NULL, 
		tdn FLOAT NOT NULL, 
		tdns FLOAT NOT NULL, 
		CONSTRAINT PK_vpvh_quarters PRIMARY KEY (id),
		CONSTRAINT FK_vpvh_quarters_audiences FOREIGN KEY (audience_id) REFERENCES audiences (id),
		CONSTRAINT UQ_vpvh_quarters UNIQUE (audience_id, quarter, year)
	)
END

IF NOT EXISTS (SELECT 1 FROM audiences WHERE code = 'A18-24')
BEGIN
	EXEC('INSERT INTO audiences (category_code, sub_category_code, range_start, range_end, custom, code, name)
	VALUES (0, ''A'', 18, 24, 1, ''A18-24'', ''Adults 18-24'')
	
	declare @A1824audienceId int = SCOPE_IDENTITY()
	
	INSERT INTO audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values (2, @A1824audienceId, (select top 1 id from audiences where code = ''M18-20'')),
	(2, @A1824audienceId, (select top 1 id from audiences where code = ''M21-24'')),
	(2, @A1824audienceId, (select top 1 id from audiences where code = ''W18-20'')),
	(2, @A1824audienceId, (select top 1 id from audiences where code = ''W21-24''))
	
	INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) 
	VALUES (@A1824audienceId, ''F18-20''), 
	(@A1824audienceId, ''F21-24''), 
	(@A1824audienceId, ''M18-20''), 
	(@A1824audienceId, ''M21-24'')
	')
END

IF NOT EXISTS (SELECT 1 FROM audiences WHERE code = 'A18-64')
BEGIN
	EXEC('INSERT INTO audiences (category_code, sub_category_code, range_start, range_end, custom, code, name)
	VALUES (0, ''A'', 18, 64, 1, ''A18-64'', ''Adults 18-64'')
	
	declare @A1864audienceId int = SCOPE_IDENTITY()
	
	INSERT INTO audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values (2, @A1864audienceId, (select top 1 id from audiences where code = ''M18-20'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''M21-24'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''M25-34'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''M35-49'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''M50-54'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''M55-64'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''W18-20'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''W21-24'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''W25-34'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''W35-49'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''W50-54'')),
	(2, @A1864audienceId, (select top 1 id from audiences where code = ''W55-64''))
	
	INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) 
	VALUES (@A1864audienceId, ''F18-20''), 
	(@A1864audienceId, ''F21-24''), 
	(@A1864audienceId, ''F25-29''), 
	(@A1864audienceId, ''F30-34''),
	(@A1864audienceId, ''F35-39''),
	(@A1864audienceId, ''F40-44''),
	(@A1864audienceId, ''F45-49''),
	(@A1864audienceId, ''F50-54''),
	(@A1864audienceId, ''F55-64''),
	(@A1864audienceId, ''M18-20''), 
	(@A1864audienceId, ''M21-24''), 
	(@A1864audienceId, ''M25-29''), 
	(@A1864audienceId, ''M30-34''),
	(@A1864audienceId, ''M35-39''),
	(@A1864audienceId, ''M40-44''),
	(@A1864audienceId, ''M45-49''),
	(@A1864audienceId, ''M50-54''),
	(@A1864audienceId, ''M55-64'')
	')
END

IF NOT EXISTS (SELECT 1 FROM audiences WHERE code = 'A50-64')
BEGIN
	EXEC('INSERT INTO audiences (category_code, sub_category_code, range_start, range_end, custom, code, name)
	VALUES (0, ''A'', 50, 64, 1, ''A50-64'', ''Adults 50-64'')
	
	declare @A5064audienceId int = SCOPE_IDENTITY()
	
	INSERT INTO audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values (2, @A5064audienceId, (select top 1 id from audiences where code = ''M50-54'')),
	(2, @A5064audienceId, (select top 1 id from audiences where code = ''M55-64'')),
	(2, @A5064audienceId, (select top 1 id from audiences where code = ''W50-54'')),
	(2, @A5064audienceId, (select top 1 id from audiences where code = ''W55-64''))
	
	INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) 
	VALUES (@A5064audienceId, ''F50-54''),
	(@A5064audienceId, ''F55-64''),
	(@A5064audienceId, ''M50-54''),
	(@A5064audienceId, ''M55-64'')
	')
END

IF NOT EXISTS (SELECT 1 FROM audiences WHERE code = 'M18-24')
BEGIN
	EXEC('INSERT INTO audiences (category_code, sub_category_code, range_start, range_end, custom, code, name)
	VALUES (0, ''M'', 18, 24, 1, ''M18-24'', ''Men 18-24'')
	
	declare @M1824audienceId int = SCOPE_IDENTITY()
	
	INSERT INTO audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values (2, @M1824audienceId, (select top 1 id from audiences where code = ''M18-20'')),
	(2, @M1824audienceId, (select top 1 id from audiences where code = ''M21-24''))
	
	INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) 
	VALUES (@M1824audienceId, ''M18-20''), 
	(@M1824audienceId, ''M21-24'')	
	')
END

IF NOT EXISTS (SELECT 1 FROM audiences WHERE code = 'M18-64')
BEGIN
	EXEC('INSERT INTO audiences (category_code, sub_category_code, range_start, range_end, custom, code, name)
	VALUES (0, ''M'', 18, 64, 1, ''M18-64'', ''Men 18-64'')
	
	declare @M1864audienceId int = SCOPE_IDENTITY()
	
	INSERT INTO audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values (2, @M1864audienceId, (select top 1 id from audiences where code = ''M18-20'')),
	(2, @M1864audienceId, (select top 1 id from audiences where code = ''M21-24'')),
	(2, @M1864audienceId, (select top 1 id from audiences where code = ''M25-34'')),
	(2, @M1864audienceId, (select top 1 id from audiences where code = ''M35-49'')),
	(2, @M1864audienceId, (select top 1 id from audiences where code = ''M50-54'')),
	(2, @M1864audienceId, (select top 1 id from audiences where code = ''M55-64''))
	
	INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) 
	VALUES (@M1864audienceId, ''M18-20''), 
	(@M1864audienceId, ''M21-24''), 
	(@M1864audienceId, ''M25-29''), 
	(@M1864audienceId, ''M30-34''),
	(@M1864audienceId, ''M35-39''),
	(@M1864audienceId, ''M40-44''),
	(@M1864audienceId, ''M45-49''),
	(@M1864audienceId, ''M50-54''),
	(@M1864audienceId, ''M55-64'')')
END

IF NOT EXISTS (SELECT 1 FROM audiences WHERE code = 'M50-64')
BEGIN
	EXEC('INSERT INTO audiences (category_code, sub_category_code, range_start, range_end, custom, code, name)
	VALUES (0, ''M'', 50, 64, 1, ''M50-64'', ''Men 50-64'')
	
	declare @M5064audienceId int = SCOPE_IDENTITY()
	
	INSERT INTO audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values (2, @M5064audienceId, (select top 1 id from audiences where code = ''M50-54'')),
	(2, @M5064audienceId, (select top 1 id from audiences where code = ''M55-64''))
	
	INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) 
	VALUES (@M5064audienceId, ''M50-54''),
	(@M5064audienceId, ''M55-64'')')
END

IF NOT EXISTS (SELECT 1 FROM audiences WHERE code = 'W18-24')
BEGIN
	EXEC('INSERT INTO audiences (category_code, sub_category_code, range_start, range_end, custom, code, name)
	VALUES (0, ''W'', 18, 24, 1, ''W18-24'', ''Women 18-24'')
	
	declare @W1824audienceId int = SCOPE_IDENTITY()
	
	INSERT INTO audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values (2, @W1824audienceId, (select top 1 id from audiences where code = ''W18-20'')),
	(2, @W1824audienceId, (select top 1 id from audiences where code = ''W21-24''))
	
	INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) 
	VALUES (@W1824audienceId, ''F18-20''), 
	(@W1824audienceId, ''F21-24'')')
END

IF NOT EXISTS (SELECT 1 FROM audiences WHERE code = 'W18-64')
BEGIN
	EXEC('INSERT INTO audiences (category_code, sub_category_code, range_start, range_end, custom, code, name)
	VALUES (0, ''W'', 18, 64, 1, ''W18-64'', ''Women 18-64'')
	
	declare @W1864audienceId int = SCOPE_IDENTITY()
	
	INSERT INTO audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values (2, @W1864audienceId, (select top 1 id from audiences where code = ''W18-20'')),
	(2, @W1864audienceId, (select top 1 id from audiences where code = ''W21-24'')),
	(2, @W1864audienceId, (select top 1 id from audiences where code = ''W25-34'')),
	(2, @W1864audienceId, (select top 1 id from audiences where code = ''W35-49'')),
	(2, @W1864audienceId, (select top 1 id from audiences where code = ''W50-54'')),
	(2, @W1864audienceId, (select top 1 id from audiences where code = ''W55-64''))
	
	INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) 
	VALUES (@W1864audienceId, ''F18-20''), 
	(@W1864audienceId, ''F21-24''), 
	(@W1864audienceId, ''F25-29''), 
	(@W1864audienceId, ''F30-34''),
	(@W1864audienceId, ''F35-39''),
	(@W1864audienceId, ''F40-44''),
	(@W1864audienceId, ''F45-49''),
	(@W1864audienceId, ''F50-54''),
	(@W1864audienceId, ''F55-64'')')
END

IF NOT EXISTS (SELECT 1 FROM audiences WHERE code = 'W50-64')
BEGIN
	EXEC('INSERT INTO audiences (category_code, sub_category_code, range_start, range_end, custom, code, name)
	VALUES (0, ''W'', 50, 64 , 1, ''W50-64'', ''Women 50-64'')
	
	declare @W5064audienceId int = SCOPE_IDENTITY()
	
	INSERT INTO audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values (2, @W5064audienceId, (select top 1 id from audiences where code = ''W50-54'')),
	(2, @W5064audienceId, (select top 1 id from audiences where code = ''W55-64''))
	
	INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) 
	VALUES (@W5064audienceId, ''F50-54''),
	(@W5064audienceId, ''F55-64'')')
END

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('vpvh_audience_mappings'))
BEGIN 
	CREATE TABLE vpvh_audience_mappings (
		id INT NOT NULL IDENTITY,
		audience_id INT NOT NULL,
		compose_audience_id INT NOT NULL,
		operation INT NOT NULL,
		CONSTRAINT PK_vpvh_audience_mapping_audiences PRIMARY KEY (id),
		CONSTRAINT FK_vpvh_audience_mappings_audiences FOREIGN KEY (audience_id) REFERENCES audiences (id),
		CONSTRAINT FK_vpvh_audience_mapping_audiences_compose_audiences FOREIGN KEY (compose_audience_id) REFERENCES audiences (id)
	)

	EXEC('INSERT INTO vpvh_audience_mappings (audience_id, operation, compose_audience_id)
	VALUES
	((SELECT id FROM audiences WHERE code = ''A18+''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A18+''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),
	((SELECT id FROM audiences WHERE code = ''A18-34''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''A18-34''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),
	((SELECT id FROM audiences WHERE code = ''A18-49''), 1, (SELECT id FROM audiences WHERE code = ''W18-49'')),((SELECT id FROM audiences WHERE code = ''A18-49''), 1, (SELECT id FROM audiences WHERE code = ''M18-49'')),
	((SELECT id FROM audiences WHERE code = ''A25-54''), 1, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''A25-54''), 1, (SELECT id FROM audiences WHERE code = ''M25-54'')),
	((SELECT id FROM audiences WHERE code = ''A35-64''), 1, (SELECT id FROM audiences WHERE code = ''W35-64'')),((SELECT id FROM audiences WHERE code = ''A35-64''), 1, (SELECT id FROM audiences WHERE code = ''M35-64'')),
	((SELECT id FROM audiences WHERE code = ''A55+''), 1, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''A55+''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''A35-49''), 1, (SELECT id FROM audiences WHERE code = ''W18-49'')),((SELECT id FROM audiences WHERE code = ''A35-49''), 2, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''A35-49''), 1, (SELECT id FROM audiences WHERE code = ''M18-49'')),((SELECT id FROM audiences WHERE code = ''A35-49''), 2, (SELECT id FROM audiences WHERE code = ''M18-34'')),
	((SELECT id FROM audiences WHERE code = ''A25+''), 1, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''A25+''), 1 , (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''A25+''), 1, (SELECT id FROM audiences WHERE code = ''M25-54'')),((SELECT id FROM audiences WHERE code = ''A25+''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''A35+''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A35+''), 2, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''A35+''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A35+''), 2, (SELECT id FROM audiences WHERE code = ''M18-34'')),
	((SELECT id FROM audiences WHERE code = ''A50+''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A50+''), 2, (SELECT id FROM audiences WHERE code = ''W18-49'')),((SELECT id FROM audiences WHERE code = ''A50+''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A50+''), 2, (SELECT id FROM audiences WHERE code = ''M18-49'')),
	((SELECT id FROM audiences WHERE code = ''A65+''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A65+''), 2, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''A65+''), 2, (SELECT id FROM audiences WHERE code = ''W35-64'')),((SELECT id FROM audiences WHERE code = ''A65+''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A65+''), 2, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''A65+''), 2, (SELECT id FROM audiences WHERE code = ''M35-64'')),
	((SELECT id FROM audiences WHERE code = ''A50-64''), 1, (SELECT id FROM audiences WHERE code = ''W35-64'')),((SELECT id FROM audiences WHERE code = ''A50-64''), 2, (SELECT id FROM audiences WHERE code = ''W18-49'')),((SELECT id FROM audiences WHERE code = ''A50-64''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''A50-64''), 1, (SELECT id FROM audiences WHERE code = ''M35-64'')),((SELECT id FROM audiences WHERE code = ''A50-64''), 2, (SELECT id FROM audiences WHERE code = ''M18-49'')),((SELECT id FROM audiences WHERE code = ''A50-64''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),
	((SELECT id FROM audiences WHERE code = ''A18-64''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''A18-64''), 1, (SELECT id FROM audiences WHERE code = ''W35-64'')),((SELECT id FROM audiences WHERE code = ''A18-64''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''A18-64''), 1, (SELECT id FROM audiences WHERE code = ''M35-64'')),
	((SELECT id FROM audiences WHERE code = ''A25-64''), 1, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''A25-64''), 1, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''A25-64''), 2, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A25-64''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''A25-64''), 1, (SELECT id FROM audiences WHERE code = ''W35-64'')),((SELECT id FROM audiences WHERE code = ''A25-64''), 1, (SELECT id FROM audiences WHERE code = ''M25-54'')),((SELECT id FROM audiences WHERE code = ''A25-64''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),((SELECT id FROM audiences WHERE code = ''A25-64''), 2, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A25-64''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''A25-64''), 1, (SELECT id FROM audiences WHERE code = ''M35-64'')),
	((SELECT id FROM audiences WHERE code = ''A18-24''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A18-24''), 2, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''A18-24''), 2, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''A18-24''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A18-24''), 2, (SELECT id FROM audiences WHERE code = ''M25-54'')),((SELECT id FROM audiences WHERE code = ''A18-24''), 2, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''A25-34''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''A25-34''), 2, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A25-34''), 1, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''A25-34''), 1, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''A25-34''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''A25-34''), 2, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A25-34''), 1, (SELECT id FROM audiences WHERE code = ''M25-54'')),((SELECT id FROM audiences WHERE code = ''A25-34''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''A50-54''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A50-54''), 2, (SELECT id FROM audiences WHERE code = ''W18-49'')),((SELECT id FROM audiences WHERE code = ''A50-54''), 2, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''A50-54''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A50-54''), 2, (SELECT id FROM audiences WHERE code = ''M18-49'')),((SELECT id FROM audiences WHERE code = ''A50-54''), 2, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''A55-64''), 1, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''A55-64''), 2, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A55-64''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''A55-64''), 1, (SELECT id FROM audiences WHERE code = ''W35-64'')),((SELECT id FROM audiences WHERE code = ''A55-64''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),((SELECT id FROM audiences WHERE code = ''A55-64''), 2, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A55-64''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''A55-64''), 1, (SELECT id FROM audiences WHERE code = ''M35-64'')),
	((SELECT id FROM audiences WHERE code = ''A25-49''), 1, (SELECT id FROM audiences WHERE code = ''W18-49'')),((SELECT id FROM audiences WHERE code = ''A25-49''), 2, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A25-49''), 1, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''A25-49''), 1, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''A25-49''), 1, (SELECT id FROM audiences WHERE code = ''M18-49'')),((SELECT id FROM audiences WHERE code = ''A25-49''), 2, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A25-49''), 1, (SELECT id FROM audiences WHERE code = ''M25-54'')),((SELECT id FROM audiences WHERE code = ''A25-49''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''A35-54''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A35-54''), 2, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''A35-54''), 2, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''A35-54''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A35-54''), 2, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''A35-54''), 2, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''A18-54''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''A18-54''), 2, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''A18-54''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''A18-54''), 2, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''W35-49''), 1, (SELECT id FROM audiences WHERE code = ''W18-49'')),((SELECT id FROM audiences WHERE code = ''W35-49''), 2, (SELECT id FROM audiences WHERE code = ''W18-34'')),
	((SELECT id FROM audiences WHERE code = ''W25+''), 1, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''W25+''), 1, (SELECT id FROM audiences WHERE code = ''W55+'')),
	((SELECT id FROM audiences WHERE code = ''W35+''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W35+''), 2, (SELECT id FROM audiences WHERE code = ''W18-34'')),
	((SELECT id FROM audiences WHERE code = ''W50+''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W50+''), 2, (SELECT id FROM audiences WHERE code = ''W18-49'')),
	((SELECT id FROM audiences WHERE code = ''W65+''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W65+''), 2, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''W65+''), 2, (SELECT id FROM audiences WHERE code = ''W35-64'')),
	((SELECT id FROM audiences WHERE code = ''W50-64''), 1, (SELECT id FROM audiences WHERE code = ''W35-64'')),((SELECT id FROM audiences WHERE code = ''W50-64''), 2, (SELECT id FROM audiences WHERE code = ''W18-49'')),((SELECT id FROM audiences WHERE code = ''W50-64''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),
	((SELECT id FROM audiences WHERE code = ''W18-64''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''W18-64''), 1, (SELECT id FROM audiences WHERE code = ''W35-64'')),
	((SELECT id FROM audiences WHERE code = ''W25-64''), 1, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''W25-64''), 1, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''W25-64''), 2, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W25-64''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''W25-64''), 1, (SELECT id FROM audiences WHERE code = ''W35-64'')),
	((SELECT id FROM audiences WHERE code = ''W18-24''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W18-24''), 2, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''W18-24''), 2, (SELECT id FROM audiences WHERE code = ''W55+'')),
	((SELECT id FROM audiences WHERE code = ''W25-34''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''W25-34''), 2, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W25-34''), 1, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''W25-34''), 1, (SELECT id FROM audiences WHERE code = ''W55+'')),
	((SELECT id FROM audiences WHERE code = ''W50-54''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W50-54''), 2, (SELECT id FROM audiences WHERE code = ''W18-49'')),((SELECT id FROM audiences WHERE code = ''W50-54''), 2, (SELECT id FROM audiences WHERE code = ''W55+'')),
	((SELECT id FROM audiences WHERE code = ''W55-64''), 1, (SELECT id FROM audiences WHERE code = ''W55+'')),((SELECT id FROM audiences WHERE code = ''W55-64''), 2, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W55-64''), 1, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''W55-64''), 1, (SELECT id FROM audiences WHERE code = ''W35-64'')),
	((SELECT id FROM audiences WHERE code = ''W25-49''), 1, (SELECT id FROM audiences WHERE code = ''W18-49'')),((SELECT id FROM audiences WHERE code = ''W25-49''), 2, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W25-49''), 1, (SELECT id FROM audiences WHERE code = ''W25-54'')),((SELECT id FROM audiences WHERE code = ''W25-49''), 1, (SELECT id FROM audiences WHERE code = ''W55+'')),
	((SELECT id FROM audiences WHERE code = ''W35-54''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W35-54''), 2, (SELECT id FROM audiences WHERE code = ''W18-34'')),((SELECT id FROM audiences WHERE code = ''W35-54''), 2, (SELECT id FROM audiences WHERE code = ''W55+'')),
	((SELECT id FROM audiences WHERE code = ''W18-54''), 1, (SELECT id FROM audiences WHERE code = ''W18+'')),((SELECT id FROM audiences WHERE code = ''W18-54''), 2, (SELECT id FROM audiences WHERE code = ''W55+'')),
	((SELECT id FROM audiences WHERE code = ''M35-49''), 1, (SELECT id FROM audiences WHERE code = ''M18-49'')),((SELECT id FROM audiences WHERE code = ''M35-49''), 2, (SELECT id FROM audiences WHERE code = ''M18-34'')),
	((SELECT id FROM audiences WHERE code = ''M25+''), 1, (SELECT id FROM audiences WHERE code = ''M25-54'')),((SELECT id FROM audiences WHERE code = ''M25+''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''M35+''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M35+''), 2, (SELECT id FROM audiences WHERE code = ''M18-34'')),
	((SELECT id FROM audiences WHERE code = ''M50+''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M50+''), 2, (SELECT id FROM audiences WHERE code = ''M18-49'')),
	((SELECT id FROM audiences WHERE code = ''M65+''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M65+''), 2, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''M65+''), 2, (SELECT id FROM audiences WHERE code = ''M35-64'')),
	((SELECT id FROM audiences WHERE code = ''M50-64''), 1, (SELECT id FROM audiences WHERE code = ''M35-64'')),((SELECT id FROM audiences WHERE code = ''M50-64''), 2, (SELECT id FROM audiences WHERE code = ''M18-49'')),((SELECT id FROM audiences WHERE code = ''M50-64''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),
	((SELECT id FROM audiences WHERE code = ''M18-64''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''M18-64''), 1, (SELECT id FROM audiences WHERE code = ''M35-64'')),
	((SELECT id FROM audiences WHERE code = ''M25-64''), 1, (SELECT id FROM audiences WHERE code = ''M25-54'')),((SELECT id FROM audiences WHERE code = ''M25-64''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),((SELECT id FROM audiences WHERE code = ''M25-64''), 2, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M25-64''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''M25-64''), 1, (SELECT id FROM audiences WHERE code = ''M35-64'')),
	((SELECT id FROM audiences WHERE code = ''M18-24''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M18-24''), 2, (SELECT id FROM audiences WHERE code = ''M25-54'')),((SELECT id FROM audiences WHERE code = ''M18-24''), 2, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''M25-34''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''M25-34''), 2, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M25-34''), 1, (SELECT id FROM audiences WHERE code = ''M25-54'')),((SELECT id FROM audiences WHERE code = ''M25-34''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''M50-54''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M50-54''), 2, (SELECT id FROM audiences WHERE code = ''M18-49'')),((SELECT id FROM audiences WHERE code = ''M50-54''), 2, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''M55-64''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),((SELECT id FROM audiences WHERE code = ''M55-64''), 2, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M55-64''), 1, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''M55-64''), 1, (SELECT id FROM audiences WHERE code = ''M35-64'')),
	((SELECT id FROM audiences WHERE code = ''M25-49''), 1, (SELECT id FROM audiences WHERE code = ''M18-49'')),((SELECT id FROM audiences WHERE code = ''M25-49''), 2, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M25-49''), 1, (SELECT id FROM audiences WHERE code = ''M25-54'')),((SELECT id FROM audiences WHERE code = ''M25-49''), 1, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''M35-54''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M35-54''), 2, (SELECT id FROM audiences WHERE code = ''M18-34'')),((SELECT id FROM audiences WHERE code = ''M35-54''), 2, (SELECT id FROM audiences WHERE code = ''M55+'')),
	((SELECT id FROM audiences WHERE code = ''M18-54''), 1, (SELECT id FROM audiences WHERE code = ''M18+'')),((SELECT id FROM audiences WHERE code = ''M18-54''), 2, (SELECT id FROM audiences WHERE code = ''M55+''))')

END
/*************************************** END BP-265 *****************************************************/

/*************************************** START BP1-481 *****************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE OBJECT_ID = OBJECT_ID('program_name_exceptions'))
BEGIN
	CREATE TABLE [program_name_exceptions] (
		[id] INT IDENTITY(1,1) NOT NULL,
		[custom_program_name] NVARCHAR(500) NOT NULL,
		[genre_id] INT NOT NULL,
		[show_type_id] INT NOT NULL,
		[created_by] VARCHAR(63) NOT NULL,
		[created_at] DATETIME2 NOT NULL
		CONSTRAINT [PK_program_name_exceptions] PRIMARY KEY CLUSTERED
	(
        [id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
    ) ON [PRIMARY]

	ALTER TABLE [dbo].[program_name_exceptions]  WITH CHECK ADD  CONSTRAINT [FK_program_name_exceptions_genres] FOREIGN KEY([genre_id])
	REFERENCES [dbo].[genres] ([id])
	
	ALTER TABLE [dbo].[program_name_exceptions]  WITH CHECK ADD  CONSTRAINT [FK_program_name_exceptions_show_types] FOREIGN KEY([show_type_id])
	REFERENCES [dbo].[show_types] ([id])

	INSERT INTO [program_name_exceptions]([custom_program_name], [genre_id], [show_type_id], [created_by], [created_at])
	VALUES('Golf', (SELECT id FROM genres WHERE name = 'Sports/Sports Talk')
				 , (SELECT id FROM show_types WHERE name = 'Sports')
				 , 'System', getdate())
	INSERT INTO [program_name_exceptions]([custom_program_name], [genre_id], [show_type_id], [created_by], [created_at])
	VALUES('NBA', (SELECT id FROM genres WHERE name = 'Sports/Sports Talk')
				 , (SELECT id FROM show_types WHERE name = 'Sports')
				 , 'System', getdate())
	INSERT INTO [program_name_exceptions]([custom_program_name], [genre_id], [show_type_id], [created_by], [created_at])
	VALUES('NHL', (SELECT id FROM genres WHERE name = 'Sports/Sports Talk')
				 , (SELECT id FROM show_types WHERE name = 'Sports')
				 , 'System', getdate())
	INSERT INTO [program_name_exceptions]([custom_program_name], [genre_id], [show_type_id], [created_by], [created_at])
	VALUES('NFL', (SELECT id FROM genres WHERE name = 'Sports/Sports Talk')
				 , (SELECT id FROM show_types WHERE name = 'Sports')
				 , 'System', getdate())
END
/*************************************** END BP1-481 *****************************************************/

/*************************************** START BP1-539 *****************************************************/

GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id =OBJECT_ID('FK_station_inventory_manifest_dayparts_station_inventory_manifest_daypart_programs'))
BEGIN
	UPDATE d SET
		primary_program_id = NULL
	FROM station_inventory_manifest_dayparts d
	LEFT JOIN station_inventory_manifest_daypart_programs p
		ON p.id = d.primary_program_id
	WHERE p.id IS NULL

	ALTER TABLE station_inventory_manifest_dayparts
		ADD CONSTRAINT FK_station_inventory_manifest_dayparts_station_inventory_manifest_daypart_programs 
		FOREIGN KEY (primary_program_id) 
		REFERENCES station_inventory_manifest_daypart_programs (id)
END

GO
/*************************************** END BP1-539 *****************************************************/

/*************************************** START BP1-538 *****************************************************/

IF (0 = COLUMNPROPERTY(OBJECT_ID('plan_version_pricing_parameters', 'U'), 'margin', 'AllowsNull'))
BEGIN 
	ALTER TABLE plan_version_pricing_parameters ALTER COLUMN margin float NULL;
END

GO

/*************************************** END BP1-538 *****************************************************/

/*************************************** START - BP1-171 ****************************************************/

IF OBJECT_ID('plan_version_audience_daypart_vpvh') IS NULL
BEGIN
	CREATE TABLE [plan_version_audience_daypart_vpvh]
	(
		[id] INT IDENTITY(1,1) NOT NULL,
		[plan_version_id] INT NOT NULL,
		[audience_id] INT NOT NULL,
		[daypart_default_id] INT NOT NULL,
		[vpvh_type] INT NOT NULL,
		[vpvh_value] FLOAT NOT NULL,
		[starting_point] DATETIME2 NOT NULL

		CONSTRAINT [PK_plan_version_audience_daypart_vpvh] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)

	ALTER TABLE [dbo].[plan_version_audience_daypart_vpvh] WITH CHECK ADD CONSTRAINT [FK_plan_version_audience_daypart_vpvh_plan_versions] FOREIGN KEY([plan_version_id])
	REFERENCES [dbo].[plan_versions] ([id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[plan_version_audience_daypart_vpvh] CHECK CONSTRAINT [FK_plan_version_audience_daypart_vpvh_plan_versions]

	CREATE NONCLUSTERED INDEX [IX_plan_version_audience_daypart_vpvh_plan_version_id] ON [dbo].[plan_version_audience_daypart_vpvh] ([plan_version_id])

	ALTER TABLE [dbo].[plan_version_audience_daypart_vpvh] WITH CHECK ADD CONSTRAINT [FK_plan_version_audience_daypart_vpvh_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])
	ALTER TABLE [dbo].[plan_version_audience_daypart_vpvh] CHECK CONSTRAINT [FK_plan_version_audience_daypart_vpvh_audiences]

	ALTER TABLE [dbo].[plan_version_audience_daypart_vpvh] WITH CHECK ADD CONSTRAINT [FK_plan_version_audience_daypart_vpvh_daypart_defaults] FOREIGN KEY([daypart_default_id])
	REFERENCES [dbo].[daypart_defaults] ([id])
	ALTER TABLE [dbo].[plan_version_audience_daypart_vpvh] CHECK CONSTRAINT [FK_plan_version_audience_daypart_vpvh_daypart_defaults]
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'vpvh_calculation_source_type' AND OBJECT_ID = OBJECT_ID('daypart_defaults'))
BEGIN
	ALTER TABLE daypart_defaults
	ADD vpvh_calculation_source_type int NULL

	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 1 where code = ''EMN''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 1 where code = ''MDN''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 2 where code = ''EN''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 2 where code = ''LN''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 2 where code = ''ENLN''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''EF''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''PA''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''PT''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''LF''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''SYN''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''OVN''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''DAY''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''EM''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 1 where code = ''AMN''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 2 where code = ''PMN''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 4 where code = ''TDN''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''ROSS''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''SPORTS''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 3 where code = ''ROSP''')
	EXEC('UPDATE daypart_defaults SET vpvh_calculation_source_type = 5 where code = ''TDNS''')

	ALTER TABLE daypart_defaults
	ALTER COLUMN vpvh_calculation_source_type int NOT NULL 
END

/*************************************** END - BP1-171 ****************************************************/

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