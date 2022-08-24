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

/*************************************** START BP-3842 ***************************************/

IF EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'Comments'
        AND OBJECT_ID = OBJECT_ID('spot_exceptions_out_of_specs'))
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs
		DROP COLUMN Comments
END


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND COLUMN_NAME= 'comment')
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs
			ADD comment nvarchar(1024) NULL
END

GO

/*************************************** END BP-3842 ***************************************/

/*************************************** START BP-4494 ***************************************/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plan_iscis' AND COLUMN_NAME= 'start_time')
BEGIN
	ALTER TABLE plan_iscis
		ADD start_time INT NULL

	ALTER TABLE plan_iscis
		ADD end_time INT NULL			
END

GO

/*************************************** END BP-4494 ***************************************/

/*************************************** START BP-4689 ***************************************/

IF (OBJECT_ID('FK_spot_exceptions_out_of_specs_dayparts') IS NOT NULL)
BEGIN
    ALTER TABLE [dbo].[spot_exceptions_out_of_specs]
    DROP CONSTRAINT FK_spot_exceptions_out_of_specs_dayparts
END
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND COLUMN_NAME= 'daypart_id')
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs
	DROP COLUMN daypart_id
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND COLUMN_NAME= 'genre_id')
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs
	DROP COLUMN genre_id
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND COLUMN_NAME= 'daypart_code')
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs
	ADD daypart_code nvarchar(20) null
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND COLUMN_NAME= 'genre_name')
BEGIN
	ALTER TABLE spot_exceptions_out_of_specs
	ADD  genre_name  nvarchar(40) null
END
GO


/*************************************** END BP-4689 ***************************************/

/*************************************** START BP-4692 ********************************************************************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_spec_decisions' AND COLUMN_NAME= 'program_name')
BEGIN
	ALTER TABLE spot_exceptions_out_of_spec_decisions
		ADD [program_name] nvarchar(500) NULL
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_spec_decisions' AND COLUMN_NAME= 'genre_name')
BEGIN
	ALTER TABLE spot_exceptions_out_of_spec_decisions
		ADD genre_name nvarchar(40) NULL
END

GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_spec_decisions' AND COLUMN_NAME= 'daypart_code')
BEGIN
	ALTER TABLE spot_exceptions_out_of_spec_decisions
		ADD daypart_code nvarchar(20) NULL
END

GO

/*************************************** END BP-4692 **********************************************************************************************/

/*************************************** START BP-4774 **********************************************************************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_recommended_plan_decision' AND (COLUMN_NAME= 'accepted_as_in_spec'))
BEGIN
ALTER TABLE spot_exceptions_recommended_plan_decision
ADD accepted_as_in_spec bit NOT NULL

END

GO

/*************************************** END BP-4774 **********************************************************************************************/


/*************************************** START BP-4864 **********************************************************************************************/

DECLARE @created_by VARCHAR(100) = 'BP-4864_program_mappings_2022_06'

IF NOT EXISTS (SELECT 1 FROM genres WHERE name = 'GAME SHOW/ENTERTAINMENT')
BEGIN 
	INSERT INTO genres (name, program_source_id, created_by, created_date, modified_by, modified_date) VALUES	
		('GAME SHOW/ENTERTAINMENT', 1, @created_by, SYSDATETIME(), @created_by, SYSDATETIME())
		, ('GAME SHOW/REALITY', 1, @created_by, SYSDATETIME(), @created_by, SYSDATETIME())
END

IF NOT EXISTS (SELECT 1 FROM program_name_exceptions WHERE created_by = @created_by)
BEGIN 

	IF OBJECT_ID('tempdb..#new_exceptions') IS NOT NULL
	BEGIN
		DROP TABLE #new_exceptions
	END

	CREATE TABLE #new_exceptions (mapping_name NVARCHAR(500), genre_name NVARCHAR(100), show_type_name NVARCHAR(100),  genre_id INT, show_type_id INT)

	INSERT INTO  #new_exceptions(mapping_name, genre_name, show_type_name, genre_id, show_type_id) VALUES
	('12 News Live @ 530P', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('12 News Live @ 5P', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('12 News Live @ 6P', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('2021 BILLBOARD MUSIC AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('2021 ROSE PARADE''S NEW YORK CELEBRATION', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('21st Annual BET Awards', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('3 News Now Live at 10p', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('3 News Now Live at 10PM', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('3 News Now Live at 6PM', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('3TV NEWS @ 10PM', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('3TV NEWS AT 10PM', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('3TV NEWS AT 9PM', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('48 HOURS SUSPICION', 'CRIME', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CRIME'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('55TH ANNUAL ACM AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('56th Annual ACM Awards', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('63rd ANNUAL GRAMMY AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('70th Primetime Emmy Awards', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('A Saturday Night Live Mother’s Day', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('A SATURDAY NIGHT LIVE', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('A Very Pentatonix Christmas Special', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ABC 21 News Weekend', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ABC NEWS WORLD NEWS', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ABC PROGRAMMING', 'INFORMATIONAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ACADEMY AWARDS 2021', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ACADEMY AWARDS COUNTDOWN LIVE', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ACADEMY AWARDS RED CARPET LIVE', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ACADIANA NEWS 10P M-SU', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('After the CMAs', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ALABAMA/LSU', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ALL AMERICAN STORIES', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('AMAZING RACE', 'REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('AMERICA UNITED', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ATLANTA V. DENVER', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('B POSITIVE', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BALTIMORE @ WASHINGTON', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Best of the Bluegrass', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BLACKLIST', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BOSTON NEWS AT 4AM', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BUCCANEERS/FALCONS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BUGS BUNNY AND  FRIENDS', 'CHILDREN', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CHILDREN'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CALL ME KAT', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Cape Fear CW Primetime News', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Capital One Pregame Show', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CAPITAL ONE TOURNAMENT CENTRAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CARPOOL KARAOKE', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CBS PRIME ROTATION', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CHAMPIONSHIP CENTRAL PREGAME', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Channel 8 Eyewitness News At Ten LT', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CHARLOTTE SPORTS LIVE', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CHIEFS/BILLS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CHURCH BROADCAST', 'RELIGIOUS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'RELIGIOUS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CINCINNATI @ MIAMI', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CINCINNATI @ WASHINGTON', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CLARICE', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CLEVELAND 19 NEWS @ 11', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CLEVELAND 19 NEWS @ 5A', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CLEVELAND 19 NEWS @ 6A', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CLEVELAND 19 NEWS @ 7A', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CLEVELAND 19 NEWS @11P', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CMA Music Festival', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COLLEGE BASKETBALL GAME 1', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COLLEGE BASKETBALL GAME 2', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COLLEGE BASKETBALL GAME 3', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COLLEGE FOOTBALL PRIME', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CONNECTING...', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Countdown To 2021', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Countdown to Oscars Live', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COWBOYS @ RAVENS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COWBOYS @ VIKINGS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COWBOYS VS EAGLES', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COWBOYS/BENGALS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COWBOYS/RAVENS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COWBOYS/VIKINGS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Crazy in Texas', 'INFORMATIONAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Critics Choice Super Awards', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CRITICS''S CHOICE AWARD', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DAY ROTATION', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Deal or No Deal Holiday Special', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DICK CLARK''S NEW YEAR''S EVE', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DOLPHINS/49ERS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Dr. Seuss''s How the Grinch Stole Christmas', 'Movie', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Movie'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('EARLY EVENING', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('EMMY AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('EUROPEAN RUBGY FINALS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Eyewitness News Early Today', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FINAL FOUR WITH MICHIGAN', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Fox 5 News 5pm-7pm Rotator', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FOX 5 NEWS ROTATOR 11pm-12am', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FOX NEWS M-SU', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FOX TNF', 'Informational', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Informational'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FOX VA NEWS', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GAME OF TALENTS', 'GAME SHOW/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Game Of Talents', 'GAME SHOW/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GAME OF TALENTS', 'GAME SHOW/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GAME TIME WITH BOOMER ESAISON', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GD MRN TX-OK @ 8A SAT', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GD MRN TX-OK @ 8A SUN', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GD MRN TX-OK @630A SAT', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GD MRN TX-OK @630A SUN', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GMET AT 430AM', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GMET AT 530AM', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GMET AT 5AM', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GMET AT 6AM', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GMJ SUNRISE', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Good Day Virginia', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GOOD MORNING MISSOURI', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Gordon Ramsay''s American Road Trip', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GRAMMY AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Grammy Awards', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GREATEST AT HOME VIDEOS', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('HELLO IOWA', 'TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('HOLEY MOLEY 2 THE SEQUEL', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('HOLEY MOLEY 2', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('HOME ECONOMICS', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Inside the Washington', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('JERRY JUDY', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('KANSAS CITY @ BUFFALO', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('KANSAS CITY @ DENVER', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('KANSAS CITY @ TAMPA BAY', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Kansas City @ Tennessee', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Kansas City/Saints', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Kansas City/Tampa Bay', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('KENAN', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('KENTUCKY DERBY', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('KING OF QUEENS', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('KMVT NEWS AT 10 WEEKEND', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('KREM 2 NEWS - 5P (SUN)', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('KREM 2 NEWS - 6P (SUN)', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LA''s Finest', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LAS VEGAS @ DENVER', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LAS VEGAS BOWL 12/19', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LAS VEGAS@DENVER', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LATE FRINGE SITCOMS', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LATE FRINGE SUPER BOWL', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LAW & ORDER: Organized Crime', 'CRIME', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CRIME'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LAW &CRIME DAILY', 'CRIME', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CRIME'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LIFE OF CARDS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Local Paid Programming', 'PAID PROGRAM', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'PAID PROGRAM'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LOCAL RELIGIOUS PROGRAMING', 'RELIGIOUS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'RELIGIOUS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Louisville Basketball', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LOUISVILLE VS NOTRE DAME', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Mad Dog and Merrill', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Manhunters', 'REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MARCH MADNESS ELITE 8 GAME 1', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MARCH MADNESS ELITE 8 GAME 2', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MARCH MADNESS NATIONAL CHAMP', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('March Madness Selection Sunday', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MEMORIAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MeTV', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Michael Buble', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Morning Mix', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MORNING NEWS ROTATION', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MORNING SHOW', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MORNING UPDATE 5-6A', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MY DESTINATION', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Nashville to Kansas', 'INFORMATIONAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NATIONAL CHAMPIONSHIP', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NBC 33 News Morning', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NBC SPECIAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NBC40 Weather', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NEBRASKA COACHES SHOW', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NEIGHBORHOOD ALL-STARS', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NEW ENGLAND @ LOS ANGELES', 'CRIME', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CRIME'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NEW ENGLAND @ LOS ANGELES', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NICK CANNON', 'TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('North Carolina News', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NOTRE DAME V ARKANSAS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NOTRE DAME V SYRACUSE', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NOTRE DAME VS STANFORD', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NY Giants @ LA Rams', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NY GIANTS/LA RAMS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('On The Red Carpet Post Show', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ON THE RED CARPET: KABC PRE SHOW!', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('OSCARS COUNTDOWN, LIVE!', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('OVERWATCH LEAGUE PLAYOFFS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PATRIOTS @ RAMS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PATRIOTS WILD CARD', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PATRIOTS/Jets', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PATRIOTS/Miami', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Penn & Teller: Try This at Home Too', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PEOPLE PRESENT', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PEOPLE''S  COURT', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PHILADELPHIA @ GREEN BAY', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PHOENIX RACEWAY', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PITTSBURGH @ DALLAS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('POOCH PERFECT', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('POPSTAR’S BEST OF 2020', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('POPSTAR''S BEST OF 2020', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PREMIER LG SOCCER', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PRESIDENTAL DEBATE', 'INFORMATIONAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PRIMETIME EMMY AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PRIMETIME EMMYS', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('RECIPE.TV', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Rock & Review', 'INFORMATIONAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SAINTS/BEARS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SAT PRIME ACC ROTATION', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SATURDAY AFT MOVIES', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Saturday Afternoon Programming', 'INFORMATIONAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SATURDAY COLLEGE FOOTBALL', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SATURDAY MATINEE', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SATURDAY NGHT LIVE', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SATURDAY PRIME', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Saturday Sunrise', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SATURDAY SUPER BOWL LOCAL SPECIAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SATURDAY THIS MORNING', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Sherrifs El Dorado County', 'Crime', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Crime'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SNL CHRISTMAS SPECIAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Soul of a Nation', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SOUNDWAVES', 'INFORMATIONAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SOUTH CAROLINA AT LSU', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SPORTS DRIVE', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Sports/Entertainment Rotator', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('STAR TREK: THE ORIGINAL', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Studio 701', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUN MORNING MOVIE', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Sunday Sunrise', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUPER BOWL GREATEST COMMERCIAL 2021', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUPER BOWL', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUPERBOWL 55', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUPERBOWL LIII', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUPERCROSS/MOTORCROSS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUPERMAN & LOIS', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Superman & Lois', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUPERMAN & LOIS', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUPERMAN AND LOIS', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Supernatural: The Long Road Home', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Tampa Bay/Chargers', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TEEN CHICE AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TENNESSEE AT ARKANSAS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THANKSGIVING LIONS/TEXANS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THANKSGIVING PREGAME', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THANKSGIVING TEXANS @ LIONS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE 26TH ANNUAL CRITICS CHOICE AWARD', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE 54RD ANNUAL CMA AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE ACADEMY AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Afc Kansas At Tampa Bay', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Bachelor : Listen to Your Heart', 'REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE BACHELOR: GREATEST SEASONS - EVER!', 'REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Bachelor: Unforgettable', 'REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE BELMONT STAKES', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE BIG BAD B-MOVIE SHOW', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE BILLBOARD MUSIC AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Bucs 8P Internl - 12/26-In', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Cfb Lsu Tex', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Cfb: Wisconson V. Notre Dame', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Coll.Fb Non Wisc. Prime', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE ESPYS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE GOLDEN GLOBES AWARDS', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE GREAT NORTH', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Greatest At Home Videos', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The How Grinch Xmas', 'Movie', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Movie'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The J Kimmel Game Night', 'Entertainment', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Entertainment'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The J. Kimmel Game Night', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE LATE NIGHT SHOW WITH STEPHEN COLBERT', 'TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE LITTLE LATE WITH LILLY SINGH', 'TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Lt Night Colbert - S.B. Night', 'TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE MASKED DANCER', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE MASTERS TOURNAMENT', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE MISS AMERICA COMPETITION', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE NATIONAL DESK', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Ncaa: Big 10 Semis', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Nfc Champ Packers', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Nfc Divisonal Sun Gameseahawks-Packers', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE OUTBACK BOWL', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Pnc Father-Son Challenge', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Rbc Heritage (Final Round', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE REPUBLIC OF SARAH', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Snf Eagles V 49Ers', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Snf Ravens At Patriots', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Snf Steelers V Bills', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Snf Vikings V Seahawks', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Snf: San Francisco @ Dallas', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Snfb Steelers @ Bills', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Snfb Vikings @ Seahawks', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Thurs Night Lights: Medina Valley @ Veterans Memorial', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Tnf-Chargers At Raiders', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE UNITED STATES OF AL', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Us Ski-Snowboard-Mammoth', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE VERY BEST OF THE ED SULLIVAN SHOW', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE VOICE SPECIAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE YEAR IN MEMORIAM 2020', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Yng Shel Enc/B Pos Enc', 'Sports', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Sports'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THURSDAY PRIME ROTATION', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TITANS @ RAVENS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TMZ  LIVE', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Tournament of Roses', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Trickster', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TRICKSTER', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TV7 NEWS @ 10P SAT', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TV7 NEWS @ 10P SUN', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TV7 NEWS @ 10P', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TV7 NEWS @ 4P', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TV7 NEWS @ 530P SUN', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TV7 NEWS @ 5P', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TV7 NEWS @ 6P SAT', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TV7 NEWS @ 6P', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('UNICORN THE', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('VARIOUS SPORT SPECIAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('VP DEBATE', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WAKE UP IDAHO', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WAVE 3 NEWS NIGHTCAST', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WEEKEND DAYBREAK-SUN', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WEEKEND ROTATION', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WELLS FARGO THIRD ROUND', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WHNT News 19 @ 10P - Sat', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WHNT News 19 @ 10P - Sun', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WHNT News 19 @ 530P - Sun', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WHO WANTS TO BE A MILLIONARE', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WILD CARD GAME', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WILD CARD PREGAME', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WINTER SPORTS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WINTERFEST 2020', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WORLD SERIES BREWERS CUBS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WORLD SERIES GAME 5 IF NEC', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WORLD SERIES GAME 6 IF NEC', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WORLD SERIES GAME 7 IF NEC', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WSNN Headline News @ 5a', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WSNN Headline News @ 8p', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WSNN Headline News', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('YOUNG KENAN', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('YOUNG ROCK', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('1st Look', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('20/20', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('6 On Your Side Live', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('60 Minutes', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('60 MINUTES', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('9-1-1: Lone Star', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('A TOAST TO 2020', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('ADVENTURES OF SUPERMAN', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('American Idol', 'ENTERTAINMENT/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('AMERICAN IDOL', 'GAME SHOW/ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('American Idol: Where The Stars Are Born', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('AMERICAN NINJA WARRIOR', 'GAME SHOW/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('American Son', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('AMERICA''S COURT WITH JUDGE ROSS', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('America''s Got Talent', 'ENTERTAINMENT/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('America''s Heartland', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Batwoman', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Beverly Hills Cop', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Bewitched', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BIG BROTHER', 'GAME SHOW/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Big News', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BIG SKY', 'CRIME', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CRIME'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Big Sky', 'CRIME', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CRIME'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Black & Gold', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BLACK AND BLUE', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Black Lightning', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Black Sheep Squadron', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Bless the Harts', 'ANIMATION', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ANIMATION'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Bob''s Burgers', 'ANIMATION', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ANIMATION'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BOB''S BURGERS', 'ANIMATION', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ANIMATION'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Buccaneers', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Buck Rogers in the 25th Century', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Bulletproof', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BULLETPROOF', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Bulletproof', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('BULLETPROOF', 'Movie', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Movie'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Capitol Connection', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CBS Thanksgiving Day Parade', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CHRIS WATTS: CONFESSIONS OF A KILLER', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CHRISTMAS STORY', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CIRCLE OPRY SPECIAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CITRUS BOWL SPECIAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Coast Live', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Combat!', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Concert for Hometown Heroes', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('CORONA VIRUS: WHAT YOU NEED TO KNOW', 'INFORMATIONAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Cosmos', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('COUPLES COURT WITH THE CUTLERS', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Dancing with Stars', 'GAME SHOW/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Dancing with the Stars: Juniors', 'GAME SHOW/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Date Movie', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Dateline NBC', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Dateline Saturday Night Mystery', 'INFORMATIONAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DATELINE', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Dateline', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DATELINE', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Dateline', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DATELINE', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Dateline', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DATELINE', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DC''s Legends of Tomorrow', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Debris', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Disney Movie', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Divorce Court', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DIVORCE COURT', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Divorce Court', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DIVORCE COURT', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DOLLY PARTON', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Dolly Parton''s Coat of Many Colors', 'Movie', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Movie'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DOLLY PARTON''S COAT OF MANY COLORS', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('DRIVE-IN', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Dry Rot', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Duplex', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('E.T.', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('EM3', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Emergency Call', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FAMILY GUY', 'ANIMATION', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ANIMATION'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FBI: Most Wanted', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Filthy Rich', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FILTHY RICH', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('First Round-Up', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FOX 11 NEWS SPECIAL: THE ISSUE IS', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FROZEN', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Full Measure with Sharyl Attkisson', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('FULL MEASURE WITH SHARYL ATTKISSON', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GMA3: WHAT YOU NEED TO KNOW', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Golden Globe Arrivals Special', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GOOD DAY', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GOOD MORNING AMERICA', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Gray''s Anatomy', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('GREAT DAY', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Greatest Super Bowl Commercials', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Gwen Stefani''s ''Wind Up''', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Hell''s Kitchen', 'ENTERTAINMENT/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('HEROES & HISTORY', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Highway Patrolman', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Hiring America', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('How to Train Your Dragon', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Howie Mandel All-Star Comedy Gala', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Hurricane Special', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('I See You', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Ice Age: A Mammoth Christmas', 'Movie', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Movie'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Idaho at 150', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('iHeartRadio Music Festival', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('IN DEPTH', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Intelligence', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('INTERVENTION', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('It''s a Wonderful Life', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('IT''S A WONDERFUL LIFE', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('It''s a Wonderful Life', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('IT''S A WONDERFUL LIFE', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Jack Hanna Special', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Jerry Springer', 'TALK/TRASH TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK/TRASH TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Jesus Christ Superstar', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('JOHN DEERE SPECIAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('JOHN LEGEND SPECIAL', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('JUDGE JERRY', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Judge Jerry', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Judge Judy', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Judge Mathis', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Justice for All', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Justice Matters', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Justice with Judge Mablean', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('JUSTICE', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('KELLY CLARKSON''S BACK!', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Kickin'' It with Byron Allen', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Land of the Giants', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LAW & CRIME DAILY', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Law & Order: Special Victims Unit', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Let''s Go Navy!', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Little Big Shots', 'ENTERTAINMENT/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('LITTLE HOUSES', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Making It', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MAKING IT', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MARCH MADNESS', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Marvel Special', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Marvel''s Agents of S.H.I.E.L.D.', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Mary Poppins Returns: Behind the Magic - A Special Edition of 20/20', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MASTER CHEF', 'ENTERTAINMENT/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MASTERCHEF', 'GAME SHOW/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MATTER OF FACT', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Maury', 'TALK/TRASH TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK/TRASH TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('MAURY', 'TALK/TRASH TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK/TRASH TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Mr. Mayor', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('My Last Days', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('National Dog Show', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('National Dog Show', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NBC Sports Special', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('NCIS: The Cases They Can’t Forget', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('New Year''s at the Needle', 'Movie', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'Movie'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Nightwatch', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('On the Red Carpet, The Winners', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Open House NYC', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Outdoor America', 'NATURE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NATURE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('OUTPOST', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Pandora', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Pawn Stars', 'ENTERTAINMENT/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Penn & Teller: Fool Us', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PENN & TELLER: FOOL US', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Penn & Teller: Fool Us', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PENN & TELLER: FOOL US', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PEOPLE', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Personal Injury Court', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PERSONAL INJURY COURT', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Personal Injury Court', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('POPSTAR’S BEST OF 2018', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PROTECTION COURT', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Protection Court', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('PROTECTION COURT', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('REAL STORIES', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Rudolph the Red Nosed Reindeer', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Sabrina the Teenage Witch', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Same Time Next Christmas', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SANTA CLAUS IS COMING TO TOWN', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Sesame Street 40th Anniversary', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Sheriffs of El Dorado County', 'CRIME', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CRIME'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SIMPSONS', 'ANIMATION', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ANIMATION'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Songland', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Sports Zone', 'SPORTS TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Star Trek', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('STAR TREK: THE NEXT GENERATION', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Steamboat Heroine', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUCCESSFUL FARMING', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Sunday Morning', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Superbowl', 'SPORTS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPORTS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SUPREME JUSTICE WITH JUDGE KAREN MILLS', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Survivor: Reunion', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Swamp Thing', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('SWAMP THING', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Swamp Thing', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TELL ME A STORY', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TEXAS AGGIE BAND SHOW', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE 13 SCARIEST MOVIES OF ALL TIME', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Carbonaro Effect', 'ENTERTAINMENT/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE CHAT ROOM', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE CHRISTMAS CAROLER CHALLENGE', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Con', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE FINAL WISH', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The First Look', 'NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Flintstones', 'CHILDREN', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CHILDREN'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE GREAT CHRISTMAS LIGHT FIGHT', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Honeymooners', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Hustler', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Immortals', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE IMMORTALS', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE JETSONS', 'CHILDREN', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CHILDREN'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE LATE LATE SHOW WITH JAMES CORDEN', 'TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE LISTENER', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE LOST WEEKEND', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE MASKED SINGER', 'GAME SHOW', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE MASKED SINGER', 'GAME SHOW/ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE NEIGHBORHOOD', 'LIFESTYLE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'LIFESTYLE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Rookie', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE SIMPSONS', 'ANIMATION', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ANIMATION'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE SINGER', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE SONG', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE SONG', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE SOUND OF MUSIC', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE STEVE WILKOS SHOW', 'TALK/TRASH TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK/TRASH TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Titan Games', 'GAME SHOW/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The Verdict with Judge Hatchett', 'COURT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COURT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE VOICE', 'ENTERTAINMENT/REALITY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT/REALITY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THE VOICE', 'GAME SHOW/ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'GAME SHOW/ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('The World''s Best Metro', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THIS WEEK IN AGRIBUSINESS', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('THRILLER', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('To the Rescue', 'CHILDREN', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'CHILDREN'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TODAY AND TOMORROW', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TODAY SHOW', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TODAY', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('TOY STORY', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Trends on 3', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Two Sentence Horror Stories', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('U.S. FARM REPORT', 'INFORMATIONAL/NEWS', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'INFORMATIONAL/NEWS'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Various Programs', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Venga la alegría', 'SPANISH', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPANISH'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Walker, Texas Ranger', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Weekend Movies', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Wendy Williams', 'TALK', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'TALK'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Westminster Kennel Club Highlight Show', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('Wonder Woman', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WONDERFUL WORLD', 'MOVIE', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'MOVIE'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('World of Dance', 'ENTERTAINMENT', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'ENTERTAINMENT'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
	,('WORLD''S FUNNIEST ANIMALS', 'COMEDY', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'COMEDY'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))

	INSERT INTO program_name_exceptions (custom_program_name, genre_id, show_type_id, created_by, created_at)
		SELECT mapping_name, genre_id, show_type_id, @created_by, SYSDATETIME()
		FROM #new_exceptions
	
END

GO

/*************************************** END BP-4864 **********************************************************************************************/

/*************************************** START BP-4864 - Take 2 **********************************************************************************************/

DECLARE @created_by VARCHAR(100) = 'BP-4864_program_mappings_2022_06-Take2'

IF NOT EXISTS (SELECT 1 FROM program_name_exceptions WHERE created_by = @created_by)
BEGIN 
	IF OBJECT_ID('tempdb..#new_exceptions') IS NOT NULL
	BEGIN
		DROP TABLE #new_exceptions
	END

	CREATE TABLE #new_exceptions (mapping_name NVARCHAR(500), genre_name NVARCHAR(100), show_type_name NVARCHAR(100),  genre_id INT, show_type_id INT)
	INSERT INTO  #new_exceptions(mapping_name, genre_name, show_type_name, genre_id, show_type_id) VALUES
		('A Saturday Night Live Mother''s Day', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
		,('Venga la alegria', 'SPANISH', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPANISH'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
		,('POPSTAR''S BEST OF 2018', 'SPECIAL', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'SPECIAL'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))
		,('NCIS: The Cases They Can''t Forget', 'DRAMA', 'Miscellaneous', (SELECT id FROM genres WHERE program_source_id = 1 AND name = 'DRAMA'),(SELECT id FROM show_types WHERE program_source_id = 1 AND name = 'Miscellaneous'))

	INSERT INTO program_name_exceptions (custom_program_name, genre_id, show_type_id, created_by, created_at)
		SELECT mapping_name, genre_id, show_type_id, @created_by, SYSDATETIME()
		FROM #new_exceptions
END 

GO

/*************************************** END BP-4864 - Take 2 **********************************************************************************************/

/*************************************** START BP-4306 **********************************************************************************************/
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND (COLUMN_NAME= 'inventory_source_name'))
BEGIN		
	DELETE FROM spot_exceptions_out_of_spec_decisions
	DELETE FROM spot_exceptions_out_of_specs

	ALTER TABLE spot_exceptions_out_of_specs
		ADD inventory_source_name VARCHAR(100) NOT NULL
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND (COLUMN_NAME= 'inventory_source_id'))
BEGIN	
	ALTER TABLE spot_exceptions_out_of_specs
		DROP CONSTRAINT FK_inventory_sources_spot_exceptions_out_of_specs

	ALTER TABLE spot_exceptions_out_of_specs
		DROP COLUMN inventory_source_id
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_out_of_specs' AND (COLUMN_NAME= 'program_genre_id'))
BEGIN	
	ALTER TABLE spot_exceptions_out_of_specs
		DROP CONSTRAINT FK_spot_exceptions_out_of_specs_program_genre

	ALTER TABLE spot_exceptions_out_of_specs
		DROP COLUMN program_genre_id
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_recommended_plans' AND (COLUMN_NAME= 'inventory_source_name'))
BEGIN	

	DELETE FROM spot_exceptions_recommended_plan_decision
	DELETE FROM spot_exceptions_recommended_plan_details
	DELETE FROM spot_exceptions_recommended_plans

	ALTER TABLE spot_exceptions_recommended_plans
		ADD inventory_source_name VARCHAR(100) NULL
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_recommended_plans' AND (COLUMN_NAME= 'inventory_source_id'))
BEGIN	
	ALTER TABLE spot_exceptions_recommended_plans
		DROP CONSTRAINT FK_inventory_sources_spot_exceptions_recommended_plans

	ALTER TABLE spot_exceptions_recommended_plans
		DROP COLUMN inventory_source_id
END

GO
/*************************************** END BP-4306 **********************************************************************************************/

/*************************************** START BP-4306 **********************************************************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns 
        WHERE Name = 'execution_id_external'
        AND OBJECT_ID = OBJECT_ID('staged_recommended_plans'))
BEGIN
	DROP TABLE staged_recommended_plan_details
	DROP TABLE staged_recommended_plans
END

IF OBJECT_ID('staged_recommended_plans') IS NULL
BEGIN
	CREATE TABLE staged_recommended_plans
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_unique_hash_external VARCHAR(255) NOT NULL,
		ambiguity_code INT NOT NULL,
		execution_id_external VARCHAR(100) NOT NULL,
		estimate_id INT NOT NULL,
		inventory_source VARCHAR(100) NOT NULL,
		house_isci VARCHAR(100) NOT NULL,
		client_isci VARCHAR(100) NOT NULL,
		client_spot_length INT NOT NULL,
		broadcast_aired_date DATETIME2 NOT NULL,
		aired_time INT NOT NULL,
		station_legacy_call_letters VARCHAR(30) NULL,
		affiliate VARCHAR(30) NULL,
		market_code INT NULL,
		market_rank INT NULL,		
		[program_name] VARCHAR(500) NOT NULL,
		program_genre VARCHAR(127) NOT NULL,
		ingested_by VARCHAR(100) NOT NULL,
		ingested_at DATETIME NOT NULL
	)

	CREATE TABLE staged_recommended_plan_details
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		staged_recommended_plan_id INT NOT NULL, -- FK
		recommended_plan_id INT NOT NULL,
		execution_trace_id BIGINT NOT NULL,
		rate MONEY NULL,
		audience_name VARCHAR(127) NULL,
		impressions FLOAT NULL,
		is_recommended_plan BIT NOT NULL,
		plan_clearance_percentage FLOAT NULL,	
		daypart_code VARCHAR(10) NULL,
		start_time INT NULL,
		end_time INT NULL,
		monday INT NULL,
		tuesday INT NULL,
		wednesday INT NULL,
		thursday INT NULL,
		friday INT NULL,
		saturday INT NULL,
		sunday INT NULL,
	)

	ALTER TABLE staged_recommended_plan_details
		ADD CONSTRAINT FK_staged_recommended_plan_details_staged_recommended_plans
		FOREIGN KEY (staged_recommended_plan_id) REFERENCES staged_recommended_plans(id)
END

GO
/*************************************** END BP-4306 **********************************************************************************************/

/*************************************** START BP-4306 **********************************************************************************************/
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'staged_recommended_plans' 
	AND COLUMN_NAME= 'program_genre' 
	AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN
	ALTER TABLE staged_recommended_plans
		ALTER COLUMN program_genre VARCHAR(127) NULL
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_recommended_plans' AND (COLUMN_NAME= 'spot_unique_hash_external'))
BEGIN	

	DELETE FROM spot_exceptions_recommended_plan_decision
	DELETE FROM spot_exceptions_recommended_plan_details
	DELETE FROM spot_exceptions_recommended_plans

	ALTER TABLE spot_exceptions_recommended_plans
		ADD spot_unique_hash_external VARCHAR(255) NOT NULL
END

GO
/*************************************** END BP-4306 **********************************************************************************************/

/*************************************** Start BP-3101 **********************************************************************************************/

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'schedules' AND COLUMN_NAME= 'advertiser_master_id')
BEGIN
	ALTER TABLE schedules
	ADD advertiser_master_id uniqueidentifier null
END
GO

/*************************************** END BP-3101 **********************************************************************************************/

/*************************************** START BP-4306 **********************************************************************************************/
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'spot_exceptions_recommended_plans' 
AND (COLUMN_NAME= 'created_by'))
BEGIN	

	DELETE FROM spot_exceptions_recommended_plan_decision
	DELETE FROM spot_exceptions_recommended_plan_details
	DELETE FROM spot_exceptions_recommended_plans

	ALTER TABLE spot_exceptions_recommended_plans
		ADD created_by VARCHAR(100) NOT NULL

	ALTER TABLE spot_exceptions_recommended_plans
		ADD created_at datetime NOT NULL

	ALTER TABLE spot_exceptions_recommended_plans
		ADD modified_by VARCHAR(100) NOT NULL

	ALTER TABLE spot_exceptions_recommended_plans
		ADD modified_at datetime NOT NULL
END

GO
/*************************************** END BP-4306 **********************************************************************************************/

/*************************************** START BP-4975 **********************************************************************************************/
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'campaigns' AND COLUMN_NAME= 'unified_id')
BEGIN

	ALTER TABLE campaigns
	ADD unified_id VARCHAR(50) NULL

	ALTER TABLE campaigns
	ADD max_fluidity_percent INT NULL

	ALTER TABLE campaigns
	ADD unified_campaign_last_sent_at DATETIME2 NULL

	ALTER TABLE campaigns
	ADD unified_campaign_last_received_at DATETIME2 NULL

END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'plans' AND COLUMN_NAME= 'unified_tactic_line_id')
BEGIN

	ALTER TABLE plans
	ADD unified_tactic_line_id VARCHAR(50) NULL
	
	ALTER TABLE plans
	ADD unified_campaign_last_sent_at DATETIME2 NULL

	ALTER TABLE plans
	ADD unified_campaign_last_received_at DATETIME2 NULL

END
GO

/*************************************** START BP-3162 **********************************************************************************************/

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'shared_folder_files' AND COLUMN_NAME= 'attachment_id' AND IS_NULLABLE = 'NO')
BEGIN
	ALTER TABLE shared_folder_files 
		DROP COLUMN attachment_id
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'shared_folder_files' AND COLUMN_NAME= 'attachment_id')
BEGIN
	ALTER TABLE shared_folder_files ADD attachment_id UNIQUEIDENTIFIER NULL
END
GO

/*************************************** END BP-3162 **********************************************************************************************/

/*************************************** START BP-4306 **********************************************************************************************/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'spot_exceptions_recommended_plan_details' 
	AND (COLUMN_NAME= 'contracted_impressions'))

BEGIN

	DROP TABLE spot_exceptions_recommended_plan_decision;
	DROP TABLE spot_exceptions_recommended_plan_details;
	DROP TABLE spot_exceptions_recommended_plans;

	CREATE TABLE spot_exceptions_recommended_plans
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_unique_hash_external varchar(255) NOT NULL,
		ambiguity_code int NOT NULL,
		execution_id_external varchar(100) NOT NULL,
		estimate_id int NOT NULL,
		inventory_source varchar(100) NOT NULL,
		house_isci varchar(100) NOT NULL,
		client_isci varchar(100) NOT NULL,
		spot_length_id int NULL,
		program_air_time datetime NOT NULL,
		station_legacy_call_letters varchar(30) NULL,
		affiliate varchar(30) NULL,
		market_code int NULL,
		market_rank int NULL,
		program_name varchar(500) NOT NULL,
		program_genre varchar(127) NULL,
		ingested_by varchar(100) NOT NULL,
		ingested_at datetime NOT NULL,
		created_by VARCHAR(100) NOT NULL,
		created_at datetime NOT NULL,
		modified_by VARCHAR(100) NOT NULL,
		modified_at datetime NOT NULL
	)

	ALTER TABLE spot_exceptions_recommended_plans WITH CHECK
		ADD  CONSTRAINT FK_spot_exceptions_recommended_plans_spot_lengths
		FOREIGN KEY(spot_length_id) REFERENCES spot_lengths(id)

	CREATE TABLE spot_exceptions_recommended_plan_details
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_exceptions_recommended_plan_id int NOT NULL,
		recommended_plan_id int NOT NULL,
		execution_trace_id bigint NOT NULL,
		rate money NULL,
		audience_name varchar(127) NULL,
		contracted_impressions float NULL,
		delivered_impressions float NULL,
		is_recommended_plan bit NOT NULL,
		plan_clearance_percentage float NULL,
		daypart_code varchar(10) NULL,
		start_time int NULL,
		end_time int NULL,
		monday int NULL,
		tuesday int NULL,
		wednesday int NULL,
		thursday int NULL,
		friday int NULL,
		saturday int NULL,
		sunday int NULL,
	)

	ALTER TABLE spot_exceptions_recommended_plan_details
		ADD  CONSTRAINT FK_spot_exceptions_recommended_plan_details_plans
		FOREIGN KEY(recommended_plan_id) REFERENCES plans(id)

	ALTER TABLE spot_exceptions_recommended_plan_details
		ADD  CONSTRAINT FK_spot_exceptions_recommended_plan_details_spot_exceptions_recommended_plans
		FOREIGN KEY(spot_exceptions_recommended_plan_id) REFERENCES spot_exceptions_recommended_plans(id)

	CREATE TABLE spot_exceptions_recommended_plan_decision
	(
		id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
		spot_exceptions_recommended_plan_detail_id int NOT NULL,
		username varchar(63) NOT NULL,
		created_at datetime NOT NULL,
		synced_by varchar(100) NULL,
		synced_at datetime2(7) NULL,
		accepted_as_in_spec bit NOT NULL
	)

	ALTER TABLE spot_exceptions_recommended_plan_decision
		ADD  CONSTRAINT FK_spot_exceptions_recommended_plan_decision_spot_exceptions_recommended_plans_details
		FOREIGN KEY(spot_exceptions_recommended_plan_detail_id) REFERENCES spot_exceptions_recommended_plan_details(id)
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'staged_recommended_plan_details' 
	AND (COLUMN_NAME = 'impressions'))
BEGIN

	ALTER TABLE staged_recommended_plan_details
		DROP COLUMN impressions

	ALTER TABLE staged_recommended_plan_details
		ADD contracted_impressions float NULL

	ALTER TABLE staged_recommended_plan_details
		ADD delivered_impressions float NULL
END

GO

/*************************************** END BP-4306 **********************************************************************************************/


/*************************************** START BP-5086 ********************************************************/

UPDATE plan_versions SET
	impressions_per_unit = 1,
	modified_by = 'BP-5086-set impressions_per_unit to 1',
	modified_date = SYSDATETIME()
WHERE COALESCE(impressions_per_unit, 0) = 0

INSERT INTO plan_version_audience_daypart_vpvh (plan_version_id, audience_id, standard_daypart_id, vpvh_type, vpvh_value, starting_point, daypart_customization_id)
	SELECT 
		s.plan_version_id, 
		s.audience_id,
		d.standard_daypart_id,
		1 AS VPVH_Type, -- Custom
		CASE 
			WHEN s.audience_id = 31 THEN 1 -- HH
			WHEN s.audience_id = 37 THEN 0.392 -- grepped from existing
		ELSE NULL 
		END AS vpvh_value,
		vv.created_date AS starting_point,
		null AS daypart_cusomization_id	
	from plan_version_secondary_audiences s
	left outer join plan_version_audience_daypart_vpvh v
		ON s.plan_version_id = v.plan_version_id
		AND s.audience_id = v.audience_id
	join plan_version_dayparts d
		on s.plan_Version_id = d.plan_version_id
	join plan_versions vv
		on vv.id = d.plan_version_id
	WHERE v.id is null

GO

/*************************************** END BP-5086 ********************************************************/

/*************************************** START BP-4947 ***************************************/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'staged_recommended_plan_details' 
	AND (COLUMN_NAME = 'plan_spot_unique_hash_external'))
BEGIN

	DELETE FROM staged_recommended_plan_details
	DELETE FROM staged_recommended_plans
	DELETE FROM spot_exceptions_recommended_plan_decision
	DELETE FROM spot_exceptions_recommended_plan_details
	DELETE FROM spot_exceptions_recommended_plans

	ALTER TABLE staged_recommended_plan_details
		ADD plan_spot_unique_hash_external varchar(255) NOT NULL

	ALTER TABLE staged_recommended_plan_details
		ADD plan_execution_id_external varchar(100) NOT NULL

	ALTER TABLE spot_exceptions_recommended_plan_details
		ADD plan_spot_unique_hash_external varchar(255) NOT NULL

	ALTER TABLE spot_exceptions_recommended_plan_details
		ADD plan_execution_id_external varchar(100) NOT NULL
END

IF (OBJECT_ID('FK_spot_exceptions_recommended_plan_details_spot_exceptions_recommended_plans') IS NOT NULL)
BEGIN
    ALTER TABLE spot_exceptions_recommended_plan_details
    DROP CONSTRAINT FK_spot_exceptions_recommended_plan_details_spot_exceptions_recommended_plans
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_recommended_plans' AND (COLUMN_NAME= 'inventory_source_name'))
BEGIN	

	DELETE FROM spot_exceptions_recommended_plan_decision
	DELETE FROM spot_exceptions_recommended_plan_details
	DELETE FROM spot_exceptions_recommended_plans

	ALTER TABLE spot_exceptions_recommended_plans
		DROP COLUMN inventory_source_name
END

GO

/*************************************** END BP-4947 ***************************************/

/*************************************** START BP-5246 ***************************************/
IF OBJECT_ID('program_name_genres') IS NOT NULL
BEGIN
	DROP TABLE program_name_genres
END
IF OBJECT_ID('program_names') IS NOT NULL
BEGIN
	DROP TABLE program_names
END
IF OBJECT_ID('programs') IS NULL
BEGIN
 CREATE TABLE dbo.programs
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		name nvarchar(500) NOT NULL,
		show_type_id int NOT NULL,
		genre_id int NOT NULL
	)

 ALTER TABLE programs ADD CONSTRAINT FK_programs_show_types FOREIGN KEY(show_type_id) REFERENCES show_types(id)
    ALTER TABLE programs ADD CONSTRAINT FK_programs_genres FOREIGN KEY(genre_id) REFERENCES genres(id)
END
GO
/*************************************** END BP-5246 ***************************************/
/*************************************** START BP-5276 ***************************************/

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'spot_exceptions_recommended_plan_details' AND COLUMN_NAME = 'spot_delivered_impression')
BEGIN
	
	DELETE FROM spot_exceptions_recommended_plan_decision
	DELETE FROM spot_exceptions_recommended_plan_details
	DELETE FROM spot_exceptions_recommended_plans

	ALTER TABLE spot_exceptions_recommended_plan_details
		ADD spot_delivered_impression float NOT NULL
END
GO

/*************************************** END BP-5276 ***************************************/

/*************************************** START BP-5361 ***************************************/
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'spot_exceptions_recommended_plan_details' AND COLUMN_NAME= 'plan_total_contracted_impressions')
BEGIN
	ALTER TABLE spot_exceptions_recommended_plan_details
		ADD plan_total_contracted_impressions float NULL,
		    plan_total_delivered_impressions  float NULL
END
GO
/*************************************** END BP-5361 ***************************************/

/*************************************** END BP-5360 ***************************************/
IF OBJECT_ID('campaign_plan_secondary_audiences') IS NULL
BEGIN
 CREATE TABLE dbo.campaign_plan_secondary_audiences
	(
		id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,		
		campaign_id  int NOT NULL,
		audience_id  int NOT NULL
	)
ALTER TABLE campaign_plan_secondary_audiences ADD CONSTRAINT FK_campaign_plan_secondary_audiences_audiences FOREIGN KEY(audience_id) REFERENCES audiences(id)
ALTER TABLE campaign_plan_secondary_audiences ADD CONSTRAINT FK_campaign_plan_secondary_audiences_campaigns FOREIGN KEY(campaign_id) REFERENCES campaigns(id)
END
GO
/*************************************** END BP-5360 ***************************************/

/*************************************** START BP-5173 ***************************************/
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'staged_recommended_plan_details' AND COLUMN_NAME = 'spot_delivered_impression')
BEGIN

	ALTER TABLE staged_recommended_plan_details
		ADD spot_delivered_impression float NULL,
			plan_total_contracted_impressions float NULL,
		    plan_total_delivered_impressions  float NULL
END
GO
/*************************************** END BP-5173 ***************************************/

/*************************************** START BP-5366 ***************************************/
DELETE FROM program_name_exceptions WHERE custom_program_name = 'TMZ  LIVE'
GO
/*************************************** END BP-5366 ***************************************/

/*************************************** START BP-5341 ***************************************/
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'campaigns' AND COLUMN_NAME= 'view_details_url')
BEGIN
	ALTER TABLE campaigns
		ADD view_details_url varchar(250) null
END
GO
/*************************************** END BP-5341 ***************************************/

/*************************************** START BP-5343 ***************************************/
IF EXISTS (SELECT UPPER(IS_NULLABLE) FROM INFORMATION_SCHEMA.COLUMNS 
	WHERE TABLE_NAME = 'spot_exceptions_recommended_plan_details' 
	AND COLUMN_NAME= 'spot_delivered_impression' 
	AND UPPER(IS_NULLABLE) = UPPER('NO'))
BEGIN

	DELETE FROM spot_exceptions_recommended_plan_decision
	DELETE FROM spot_exceptions_recommended_plan_details
	DELETE FROM spot_exceptions_recommended_plans

	ALTER TABLE spot_exceptions_recommended_plan_details
		ALTER COLUMN spot_delivered_impression float NULL
END
GO
/*************************************** END BP-5343 ***************************************/


/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '22.06.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '22.02.2' -- Previous release version
		OR [version] = '22.06.1') -- Current release version
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
