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