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

/*************************************** START - PRI-19871 ****************************************************/
IF NOT EXISTS
	(SELECT 1 
	FROM sys.indexes 
	WHERE name='IX_station_inventory_manifest_audiences_manifest_id_audience_id_is_reference'
	AND object_id = OBJECT_ID('station_inventory_manifest_audiences'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_audiences_manifest_id_audience_id_is_reference] ON [dbo].[station_inventory_manifest_audiences]
	(
		[station_inventory_manifest_id] ASC,
		[audience_id] ASC,
		[is_reference] ASC
	)
	INCLUDE ([impressions]) 
END
GO
/*************************************** END - PRI-19871 ****************************************************/

/*************************************** START PRI-18256 *****************************************************/
IF EXISTS(SELECT 1 FROM sys.tables WHERE  object_id = OBJECT_ID('daypart_codes'))
BEGIN		  
    
	-------------------------- BEGIN RENAME DAYPART_CODES AND DROP COLUMNS ---------------------
	EXEC sp_rename 'daypart_codes', 'daypart_defaults';

	ALTER TABLE daypart_defaults
	DROP COLUMN default_start_time_seconds

	ALTER TABLE daypart_defaults
	DROP COLUMN default_end_time_seconds

	ALTER TABLE daypart_defaults
	DROP COLUMN full_name

    ALTER TABLE daypart_defaults
    DROP COLUMN is_active

	ALTER TABLE daypart_defaults
	ADD daypart_id INT NULL

	ALTER TABLE daypart_defaults
	ADD CONSTRAINT FK_daypart_defaults_dayparts FOREIGN KEY (daypart_id) REFERENCES dayparts (id)

	-------------------------- END RENAME DAYPART_CODES AND DROP COLUMNS ---------------------

	-------------------------- BEGIN INSERT NEW DAYPART DEFAULTS ---------------------

	DECLARE @INSERTED_DAYPART_ID INT
	DECLARE @TIMESPAM_ID INT

	----------------- BEGIN EMN - Early Morning News ----------------------------
	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '4:00') AND end_time = DATEDIFF(second, 0, '10:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '4:00'), DATEDIFF(second, 0, '10:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID, 'EMN', 'Early Morning News', 1, 'M-SU 4AM-10AM', 42)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'EMN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'EMN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 1, 'EMN')
	END

	----------------- END EMN - Early Morning News ----------------------------

	----------------- BEGIN MDN - Midday News ----------------------------
	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '11:00') AND end_time = DATEDIFF(second, 0, '13:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '11:00'), DATEDIFF(second, 0, '13:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'MDN','Midday News', 1,'M-SU 11AM-1PM', 14)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'MDN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'MDN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 1, 'MDN')
	END

	----------------- END MDN - Midday News ----------------------------

	----------------- BEGIN AMN - AM News ----------------------------
	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '4:00') AND end_time = DATEDIFF(second, 0, '13:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '4:00'), DATEDIFF(second, 0, '13:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'AMN','AM News', 1,'M-SU 4AM-1PM', 63)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'AMN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'AMN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 1, 'AMN')
	END

	----------------- END AMN - AM News ----------------------------

	----------------- BEGIN EN - Evening News ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '16:00') AND end_time = DATEDIFF(second, 0, '19:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '16:00'), DATEDIFF(second, 0, '19:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'EN','Evening News', 1,'M-SU 4PM-7PM', 21)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'EN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'EN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 1, 'EN')
	END

	----------------- END EN - Evening News ----------------------------

	----------------- BEGIN PMN - PM News ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '13:00') AND end_time = DATEDIFF(second, 0, '0:05') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '13:00'), DATEDIFF(second, 0, '0:05') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'PMN','PM News', 1,'M-SU 1PM-12:05AM', 77)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'PMN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'PMN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 1, 'PMN')
	END

	----------------- END PMN - PM News ----------------------------

	----------------- BEGIN LN - Late News ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '20:00') AND end_time = DATEDIFF(second, 0, '0:05') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '20:00'), DATEDIFF(second, 0, '0:05') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'LN','Late News', 1,'M-SU 8PM-12:05AM', 28)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'LN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'LN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 1, 'LN')
	END

	----------------- END LN - Late News ----------------------------

	----------------- BEGIN ENLN - Evening News/Late News ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '16:00') AND end_time = DATEDIFF(second, 0, '0:05') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '16:00'), DATEDIFF(second, 0, '0:05') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'ENLN','Evening News/Late News', 1,'M-SU 4PM-12:05AM', 56)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'ENLN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'ENLN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 1, 'ENLN')
	END

	----------------- END ENLN - Evening News/Late News ----------------------------

	----------------- BEGIN TDN - Total Day News ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '16:00') AND end_time = DATEDIFF(second, 0, '0:05') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '16:00'), DATEDIFF(second, 0, '0:05') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'TDN','Total Day News', 1,'M-SU 4AM-12:05AM', 140)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'TDN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'TDN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 1, 'TDN')
	END

	----------------- END TDN - Total Day News ----------------------------

	----------------- BEGIN ROSN - ROS News ----------------------------
	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '4:00') AND end_time = DATEDIFF(second, 0, '0:05') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '4:00'), DATEDIFF(second, 0, '0:05') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'ROSN','ROS News', 1,'M-SU 4AM-12:05AM', 140)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'ROSN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'ROSN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 1, 'ROSN')
	END

	----------------- END ROSN - ROS News ----------------------------

	----------------- BEGIN EF - Early Fringe ----------------------------
	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '15:00') AND end_time = DATEDIFF(second, 0, '18:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '15:00'), DATEDIFF(second, 0, '18:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'EF','Early Fringe', 1,'M-SU 3PM-6PM', 21)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'EF')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'EF'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'EF')
	END

	----------------- END EF - Early Fringe ----------------------------

	----------------- BEGIN PA - Prime Access ----------------------------
	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '18:00') AND end_time = DATEDIFF(second, 0, '20:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '18:00'), DATEDIFF(second, 0, '20:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'PA','Prime Access', 1,'M-SU 6PM-8PM', 14)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'PA')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'PA'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'PA')
	END

	----------------- END PA - Prime Access ----------------------------

	----------------- BEGIN PT - Prime ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '20:00') AND end_time = DATEDIFF(second, 0, '23:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '20:00'), DATEDIFF(second, 0, '23:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'PT','Prime', 1,'M-SU 8PM-11PM', 21)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'PT')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'PT'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'PT')
	END

	----------------- END PT - Prime ----------------------------

	----------------- BEGIN LF - Late Fringe ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '23:00') AND end_time = DATEDIFF(second, 0, '2:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '23:00'), DATEDIFF(second, 0, '2:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'LF','Late Fringe', 1,'M-SU 11PM-2AM', 21)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'LF')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'LF'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'LF')
	END

	----------------- END LF - Late Fringe ----------------------------

	----------------- BEGIN SYN - Total Day Syndication ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '6:00') AND end_time = DATEDIFF(second, 0, '2:05') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '6:00'), DATEDIFF(second, 0, '2:05') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'SYN','Total Day Syndication', 1,'M-SU 6AM-2:05AM', 126)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'SYN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'SYN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'SYN')
	END

	----------------- END SYN - Total Day Syndication ----------------------------

	----------------- BEGIN DAY - Daytime ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '9:00') AND end_time = DATEDIFF(second, 0, '16:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '9:00'), DATEDIFF(second, 0, '16:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'DAY','Daytime', 1,'M-SU 9AM-4PM', 49)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'DAY')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'DAY'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'DAY')
	END

	----------------- END DAY - Daytime ----------------------------

	----------------- BEGIN OVN - Overnights ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '2:00') AND end_time = DATEDIFF(second, 0, '6:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '2:00'), DATEDIFF(second, 0, '6:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'OVN','Overnights', 1,'M-SU 2AM-6AM', 28)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'OVN')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'OVN'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'OVN')
	END

	----------------- END OVN - Overnights ----------------------------

	----------------- BEGIN EM - Early morning ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '6:00') AND end_time = DATEDIFF(second, 0, '9:00') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT timespans
		VALUES (DATEDIFF(second, 0, '6:00'), DATEDIFF(second, 0, '9:00') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'EM','Early morning', 1,'M-SU 6AM-9AM', 21)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'EM')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'EM'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'EM')
	END

	----------------- END EM - Early morning ----------------------------

	----------------- BEGIN ROSS - ROS Syndication ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '6:00') AND end_time = DATEDIFF(second, 0, '2:05') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '6:00'), DATEDIFF(second, 0, '2:05') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'ROSS','ROS Syndication', 1,'M-SU 6AM-2:05AM', 126)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'ROSS')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'ROSS'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'ROSS')
	END

	----------------- END ROSS - ROS Syndication ----------------------------

	----------------- BEGIN SPORTS - ROS Sports ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '6:00') AND end_time = DATEDIFF(second, 0, '2:05') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '6:00'), DATEDIFF(second, 0, '2:05') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'SPORTS','ROS Sports', 1,'M-SU 6AM-2:05AM', 126)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'SPORTS')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'SPORTS'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'SPORTS')
	END

	----------------- END SPORTS - ROS Sports ----------------------------

	----------------- BEGIN ROSP - ROS Programming ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '6:00') AND end_time = DATEDIFF(second, 0, '2:05') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '6:00'), DATEDIFF(second, 0, '2:05') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'ROSP','ROS Programming', 1, 'M-SU 6AM-2:05AM', 126)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'ROSP')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'ROSP'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 2, 'ROSP')
	END

	----------------- END ROSP - ROS Programming ----------------------------

	----------------- BEGIN TDNS - Total Day News and Syndication ----------------------------

	SELECT @TIMESPAM_ID = id FROM timespans
	WHERE start_time = DATEDIFF(second, 0, '4:00') AND end_time = DATEDIFF(second, 0, '2:05') -1
	IF @TIMESPAM_ID IS NULL
	BEGIN 
		INSERT INTO timespans
		VALUES (DATEDIFF(second, 0, '4:00'), DATEDIFF(second, 0, '2:05') -1)

		SELECT @TIMESPAM_ID = SCOPE_IDENTITY()
	END
	INSERT INTO dayparts (timespan_id, code, name, tier, daypart_text, total_hours) VALUES 
	(@TIMESPAM_ID,'TDNS','Total Day News and Syndication', 1,'M-SU 4AM-2:05AM',140)

	SELECT @INSERTED_DAYPART_ID = SCOPE_IDENTITY()

	INSERT INTO daypart_days 
	VALUES (@INSERTED_DAYPART_ID, 1), (@INSERTED_DAYPART_ID, 2), (@INSERTED_DAYPART_ID, 3), (@INSERTED_DAYPART_ID, 4), (@INSERTED_DAYPART_ID, 5), (@INSERTED_DAYPART_ID, 6), (@INSERTED_DAYPART_ID, 7)

	IF EXISTS(SELECT 1 FROM daypart_defaults WHERE code = 'TDNS')
	BEGIN
		UPDATE daypart_defaults
		SET daypart_id = @INSERTED_DAYPART_ID
		WHERE code = 'TDNS'
	END
	ELSE
	BEGIN
		INSERT INTO daypart_defaults (daypart_id, daypart_type, code)
		VALUES (@INSERTED_DAYPART_ID, 3, 'TDNS')
	END

	----------------- END TDNS - Total Day News and Syndication ----------------------------

	-------------------------- END INSERT NEW DAYPART DEFAULTS ---------------------

	-------------------------- BEGIN REMOVE COLUMNS, UNUSED REGISTERS AND RENAMING COLUMNS ---------------------

	ALTER TABLE daypart_defaults
	DROP COLUMN code

	ALTER TABLE daypart_defaults
	ALTER COLUMN daypart_id INT NOT NULL

	EXEC sp_rename 'inventory_file_proprietary_header.daypart_code_id', 'daypart_default_id', 'COLUMN'
	EXEC sp_rename 'inventory_summary_quarter_details.daypart_code_id', 'daypart_default_id', 'COLUMN'
	EXEC sp_rename 'plan_version_dayparts.daypart_code_id', 'daypart_default_id', 'COLUMN'
	EXEC sp_rename 'scx_generation_job_files.daypart_code_id', 'daypart_default_id', 'COLUMN'
	EXEC sp_rename 'scx_generation_jobs.daypart_code_id', 'daypart_default_id', 'COLUMN'
	EXEC sp_rename 'station_inventory_manifest_dayparts.daypart_code_id', 'daypart_default_id', 'COLUMN'


	-------------------------- END REMOVE COLUMNS, UNUSED REGISTERS AND RENAMING COLUMNS ---------------------
END

/*************************************** END PRI-18256 *****************************************************/


/*************************************** START - PRI-19669 ****************************************************/
IF OBJECT_ID('plan_version_pricing_parameters_inventory_source_type_percentages') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_pricing_parameters_inventory_source_type_percentages](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_pricing_parameter_id] [int] NOT NULL,
		[inventory_source_type] [tinyint] NOT NULL,
		[percentage] [int] NOT NULL
	 CONSTRAINT [PK_plan_version_pricing_parameters_inventory_source_type_percentages] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
		
	ALTER TABLE [dbo].[plan_version_pricing_parameters_inventory_source_type_percentages] 
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_parameters_inventory_source_type_percentages_plan_version_pricing_parameters] FOREIGN KEY([plan_version_pricing_parameter_id])
	REFERENCES [dbo].[plan_version_pricing_parameters] ([id])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[plan_version_pricing_parameters_inventory_source_type_percentages]
	CHECK CONSTRAINT [FK_plan_version_pricing_parameters_inventory_source_type_percentages_plan_version_pricing_parameters]

	ALTER TABLE [dbo].[plan_version_pricing_parameters_inventory_source_type_percentages]
	ADD CONSTRAINT [UQ_plan_version_pricing_parameters_inventory_source_type_percentages_plan_version_pricing_parameter_id_inventory_source_type] 
	UNIQUE ([plan_version_pricing_parameter_id], [inventory_source_type])
END

IF OBJECT_ID('plan_version_pricing_inventory_source_type_percentages') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_pricing_inventory_source_type_percentages](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_pricing_execution_id] [int] NOT NULL,
		[inventory_source_type] [tinyint] NOT NULL,
		[percentage] [int] NOT NULL
	 CONSTRAINT [PK_plan_version_pricing_inventory_source_type_percentages] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
		
	ALTER TABLE [dbo].[plan_version_pricing_inventory_source_type_percentages] 
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_inventory_source_type_percentages_plan_version_pricing_executions] FOREIGN KEY([plan_version_pricing_execution_id])
	REFERENCES [dbo].[plan_version_pricing_executions] ([id])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[plan_version_pricing_inventory_source_type_percentages]
	CHECK CONSTRAINT [FK_plan_version_pricing_inventory_source_type_percentages_plan_version_pricing_executions]

	ALTER TABLE [dbo].[plan_version_pricing_inventory_source_type_percentages]
	ADD CONSTRAINT [UQ_plan_version_pricing_inventory_source_type_percentages_plan_version_pricing_execution_id_inventory_source_type] 
	UNIQUE ([plan_version_pricing_execution_id], [inventory_source_type])
END
/*************************************** END - PRI-19669 ****************************************************/

/*************************************** START PRI-19211 *****************************************************/
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID('FK_inventory_summary_gaps_inventory_summary') AND parent_object_id = OBJECT_ID('inventory_summary_gaps'))
BEGIN
	-- simplify the PK in inventory_summary so that the table can be pointed easier
	ALTER TABLE inventory_summary DROP CONSTRAINT PK_inventory_summary
	ALTER TABLE inventory_summary ADD CONSTRAINT PK_inventory_summary PRIMARY KEY (id)

	-- drop the old index
	DROP INDEX inventory_summary_gaps.IX_inventory_summary_gaps_inventory_summary_id

	-- drop the old constraint
	ALTER TABLE [inventory_summary_gaps] DROP CONSTRAINT [FK_inventory_summary_gaps_inventory_summary]

	-- add a new column inventory_summary_id1 (1 in the end is needed because there is inventory_summary_id column in the table at the moment and it points to a wrong table)
	ALTER TABLE [inventory_summary_gaps] ADD [inventory_summary_id1] int NULL

	-- set inventory_summary_id1 column from inventory_summary.id based on inventory_summary_quarters. Right now inventory_summary_id points to inventory_summary_quarters
	EXEC('
	UPDATE [inventory_summary_gaps] 
	SET [inventory_summary_id1] = 
		(select top 1 id from inventory_summary where inventory_source_id = 
			(select top 1 inventory_source_id from inventory_summary_quarters where id = inventory_summary_id))')

	-- make inventory_summary_id1 to be not nullable
	ALTER TABLE [inventory_summary_gaps] ALTER COLUMN [inventory_summary_id1] int NOT NULL

	-- now we can drop old inventory_summary_id column
	ALTER TABLE [inventory_summary_gaps] DROP COLUMN [inventory_summary_id]

	-- let`s rename the new inventory_summary_id1 to inventory_summary_id
	EXEC sp_rename 'dbo.inventory_summary_gaps.inventory_summary_id1', 'inventory_summary_id', 'COLUMN';

	-- naming the new constraint [FK_inventory_summary_gaps_inventory_summary1] because name FK_inventory_summary_gaps_inventory_summary is used in the IF EXISTS operator to prevent multiple running of the script
	ALTER TABLE [inventory_summary_gaps]  WITH CHECK ADD  CONSTRAINT [FK_inventory_summary_gaps_inventory_summary1] FOREIGN KEY([inventory_summary_id]) 
	REFERENCES [dbo].[inventory_summary] ([id])
	ON DELETE CASCADE	

	ALTER TABLE [inventory_summary_gaps] CHECK CONSTRAINT [FK_inventory_summary_gaps_inventory_summary1]

	-- add the index back
	EXEC('CREATE NONCLUSTERED INDEX IX_inventory_summary_gaps_inventory_summary_id ON inventory_summary_gaps(inventory_summary_id)')
END
/*************************************** END PRI-19211 *****************************************************/

/*************************************** START PRI-15810 *****************************************************/
IF OBJECT_ID('dbo.bvs_file_details') IS NOT NULL
BEGIN
	-- table name
	EXEC sp_rename 'dbo.bvs_file_details', 'detection_file_details'
	-- column names
	EXEC sp_rename 'dbo.detection_file_details.bvs_file_id', 'detection_file_id', 'COLUMN'
	-- key names
	EXEC sp_rename 'dbo.detection_file_details.PK_bvs_file_details', 'PK_detection_file_details'
	EXEC sp_rename 'dbo.FK_bvs_file_bvs_file_details', 'FK_detection_file_detection_file_details'
	EXEC sp_rename 'dbo.FK_bvs_file_details_schedule_detail_weeks', 'FK_detection_file_details_schedule_detail_weeks'
	EXEC sp_rename 'dbo.FK_bvs_file_details_spot_lengths', 'FK_detection_file_details_spot_lengths'
	-- constraint names
	EXEC sp_rename 'dbo.DF_bvs_file_details_has_lead_in_schedule_matches', 'DF_detection_file_details_has_lead_in_schedule_matches'
	EXEC sp_rename 'dbo.DF_bvs_file_details_linked_to_block', 'DF_detection_file_details_linked_to_block'
	EXEC sp_rename 'dbo.DF_bvs_file_details_linked_to_leadin', 'DF_detection_file_details_linked_to_leadin'
	EXEC sp_rename 'dbo.DF_bvs_file_details_match_airtime', 'DF_detection_file_details_match_airtime'
	EXEC sp_rename 'dbo.DF_bvs_file_details_match_isci', 'DF_detection_file_details_match_isci'
	EXEC sp_rename 'dbo.DF_bvs_file_details_match_program', 'DF_detection_file_details_match_program'
	EXEC sp_rename 'dbo.DF_bvs_file_details_match_spot_length', 'DF_detection_file_details_match_spot_length'
	EXEC sp_rename 'dbo.DF_bvs_file_details_match_station', 'DF_detection_file_details_match_station'
	EXEC sp_rename 'dbo.DF_bvs_file_details_status', 'DF_detection_file_details_status'
END

IF OBJECT_ID('dbo.bvs_files') IS NOT NULL
BEGIN
	-- table name
	EXEC sp_rename 'dbo.bvs_files', 'detection_files'
	-- key names
	EXEC sp_rename 'dbo.detection_files.PK_bvs_files', 'PK_detection_files'
END

IF OBJECT_ID('dbo.bvs_map_types') IS NOT NULL
BEGIN
	-- table name
	EXEC sp_rename 'dbo.bvs_map_types', 'detection_map_types'
	-- key names
	EXEC sp_rename 'dbo.detection_map_types.PK_bvs_map_types', 'PK_detection_map_types'
	-- constraint names
	EXEC sp_rename 'dbo.DF_bvs_map_types_version', 'DF_detection_map_types_version'
END

IF OBJECT_ID('dbo.bvs_maps') IS NOT NULL
BEGIN
	-- table name
	EXEC sp_rename 'dbo.bvs_maps', 'detection_maps'
	-- column names
	EXEC sp_rename 'dbo.detection_maps.bvs_map_type_id', 'detection_map_type_id', 'COLUMN'
	EXEC sp_rename 'dbo.detection_maps.bvs_value', 'detection_value', 'COLUMN'
	-- key names
	EXEC sp_rename 'dbo.detection_maps.PK_bvs_maps', 'PK_detection_maps'
	EXEC sp_rename 'dbo.FK_bvs_maps_bvs_map_types', 'FK_detection_maps_detection_map_types'
END

IF OBJECT_ID('dbo.bvs_post_details') IS NOT NULL
BEGIN
	-- table name
	EXEC sp_rename 'dbo.bvs_post_details', 'detection_post_details'	
	-- column name
	EXEC sp_rename 'dbo.detection_post_details.bvs_file_detail_id', 'detection_file_detail_id', 'COLUMN'
	-- key names
	EXEC sp_rename 'dbo.detection_post_details.PK_bvs_post_details', 'PK_detection_post_details'
	EXEC sp_rename 'dbo.FK_bvs_post_details_audiences', 'FK_detection_post_details_audiences'
	EXEC sp_rename 'dbo.FK_bvs_post_details_bvs_file_details', 'FK_detection_post_details_detection_file_details'
END
/*************************************** END PRI-15810 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.02.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.01.1' -- Previous release version
		OR [version] = '20.02.1') -- Current release version
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