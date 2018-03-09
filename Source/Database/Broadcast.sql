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


/*************************************** START UPDATE SCRIPT *****************************************************/

/*************************************** START BCOP-2512 *********************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'monday' OR name = 'tuesday' OR name = 'wednesday' OR name = 'thursday' OR name = 'friday' OR name = 'saturday' OR name = 'sunday'
              AND OBJECT_ID = OBJECT_ID('[proposal_version_detail_quarter_week_iscis]'))
BEGIN
    ALTER TABLE [proposal_version_detail_quarter_week_iscis] 
	ADD monday bit, 
		tuesday bit, 
		wednesday bit, 
		thursday bit, 
		friday bit, 
		saturday bit,
		sunday bit 
END

/*************************************** END BCOP-2450 ***********************************************************/

/*************************************** START BCOP-2450 *********************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'market'
              AND OBJECT_ID = OBJECT_ID('affidavit_file_details'))
BEGIN
    ALTER TABLE affidavit_file_details
	ADD market VARCHAR(63) NULL
END

/*************************************** END BCOP-2450 ***********************************************************/

/*************************************** START BCOP-2452 *****************************************************/

IF OBJECT_ID('affidavit_file_detail_problems', 'U') IS NULL
BEGIN
	CREATE TABLE affidavit_file_detail_problems
	(
		id INT IDENTITY(1,1) NOT NULL,
		affidavit_file_detail_id BIGINT NOT NULL,
		problem_type INT NOT NULL,
		problem_description VARCHAR(255) NULL
		CONSTRAINT [PK_affidavit_file_detail_problems] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)

	ALTER TABLE [dbo].[affidavit_file_detail_problems] 
	WITH CHECK ADD CONSTRAINT [FK_affidavit_file_detail_problems_affidavit_file_details] 
	FOREIGN KEY(affidavit_file_detail_id)
	REFERENCES [dbo].[affidavit_file_details] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].affidavit_file_detail_problems CHECK CONSTRAINT [FK_affidavit_file_detail_problems_affidavit_file_details]

END

/*************************************** END BCOP-2452 *****************************************************/

/*************************************** START BCOP-2409 *********************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'program_name_id'
              AND OBJECT_ID = OBJECT_ID('proposal_version_detail_criteria_programs'))
BEGIN
    ALTER TABLE proposal_version_detail_criteria_programs
	ADD program_name_id INT NULL
END
GO

UPDATE proposal_Version_detail_criteria_programs
SET program_name_id = 0
GO

ALTER TABLE proposal_version_detail_criteria_programs
ALTER COLUMN program_name_id INT NOT NULL
GO

IF OBJECT_ID('program_names', 'U') IS NULL
BEGIN
	CREATE TABLE program_names
	(
		id INT IDENTITY(1,1) NOT NULL,
		program_name NVARCHAR(100) NOT NULL
		CONSTRAINT [PK_program_names] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)
END

if not exists (select 1 from program_names)
begin
insert into program_names (program_name) values ('18 Again!')
insert into program_names (program_name) values ('ABBA: The Movie')
insert into program_names (program_name) values ('Above Us the Waves')
insert into program_names (program_name) values ('Above the Law')
insert into program_names (program_name) values ('Above the Rim')
insert into program_names (program_name) values ('Abraham')
insert into program_names (program_name) values ('Absolute Power')
insert into program_names (program_name) values ('Addams Family, The')
insert into program_names (program_name) values ('Addicted to Love')
insert into program_names (program_name) values ('Adventures of Milo and Otis, The')
insert into program_names (program_name) values ('America Undercover')
insert into program_names (program_name) values ('Annie O')
insert into program_names (program_name) values ('Attic: The Hiding of Anne Frank, The')
insert into program_names (program_name) values ('Austin Powers: The Spy Who Shagged Me')
insert into program_names (program_name) values ('Autobiography of Miss Jane Pittman, The')
insert into program_names (program_name) values ('Babe: Pig in the City')
insert into program_names (program_name) values ('Belly')
insert into program_names (program_name) values ('Beyond the Call')
insert into program_names (program_name) values ('Black Fox: The Price of Peace')
insert into program_names (program_name) values ('Blind Faith')
insert into program_names (program_name) values ('Brotherhood of Justice')
insert into program_names (program_name) values ('Case of Deadly Force, A')
insert into program_names (program_name) values ('Chaplin')
insert into program_names (program_name) values ('Chasing Amy')
insert into program_names (program_name) values ('Cheers')
insert into program_names (program_name) values ('Clerks')
insert into program_names (program_name) values ('Consenting Adults')
insert into program_names (program_name) values ('Courtyard, The')
insert into program_names (program_name) values ('Daily Show with Jon Stewart, The')
insert into program_names (program_name) values ('Dances with Wolves')
insert into program_names (program_name) values ('Death Wish II')
insert into program_names (program_name) values ('Elizabeth')
insert into program_names (program_name) values ('Fabulous Baker Boys, The')
insert into program_names (program_name) values ('Faculty, The')
insert into program_names (program_name) values ('Fixer, The')
insert into program_names (program_name) values ('For Richer, for Poorer')
insert into program_names (program_name) values ('Four Feathers, The')
insert into program_names (program_name) values ('Glass House, The')
insert into program_names (program_name) values ('Gleaming the Cube')
insert into program_names (program_name) values ('Gods and Monsters')
insert into program_names (program_name) values ('Gone with the Wind')
insert into program_names (program_name) values ('Hands of a Murderer')
insert into program_names (program_name) values ('Harvest of Fire')
insert into program_names (program_name) values ('Harvey')
insert into program_names (program_name) values ('Holy Man')
insert into program_names (program_name) values ('Hotline')
insert into program_names (program_name) values ('I Know What You Did Last Summer')
insert into program_names (program_name) values ('I Still Know What You Did Last Summer')
insert into program_names (program_name) values ('In Dreams')
insert into program_names (program_name) values ('Lantern Hill')
insert into program_names (program_name) values ('Larry King Live')
insert into program_names (program_name) values ('Late Show with David Letterman')
insert into program_names (program_name) values ('Looking for Miracles')
insert into program_names (program_name) values ('Love Potion No. 9')
insert into program_names (program_name) values ('M')
insert into program_names (program_name) values ('Mac and Me')
insert into program_names (program_name) values ('Max Is Missing')
insert into program_names (program_name) values ('Miles To Go')
insert into program_names (program_name) values ('Mind Games')
insert into program_names (program_name) values ('Movie')
insert into program_names (program_name) values ('Mr. and Mrs. Loving')
insert into program_names (program_name) values ('My Chauffeur')
insert into program_names (program_name) values ('NFL Football')
insert into program_names (program_name) values ('Night at the Roxbury, A')
insert into program_names (program_name) values ('Off Air')
insert into program_names (program_name) values ('Over the Hill')
insert into program_names (program_name) values ('Paid Program')
insert into program_names (program_name) values ('Pals')
insert into program_names (program_name) values ('Patch Adams')
insert into program_names (program_name) values ('Phar Lap')
insert into program_names (program_name) values ('Psycho')
insert into program_names (program_name) values ('Rehearsal for Murder')
insert into program_names (program_name) values ('Return of the Native, The')
insert into program_names (program_name) values ('Sabrina the Teenage Witch')
insert into program_names (program_name) values ('Saving Private Ryan')
insert into program_names (program_name) values ('Shadow of a Doubt')
insert into program_names (program_name) values ('ShadowZone: The Undead Express')
insert into program_names (program_name) values ('Siege, The')
insert into program_names (program_name) values ('Simon Birch')
insert into program_names (program_name) values ('Simple Plan, A')
insert into program_names (program_name) values ('Six Weeks')
insert into program_names (program_name) values ('Square Dance')
insert into program_names (program_name) values ('Star Trek: Insurrection')
insert into program_names (program_name) values ('Star Wars: Episode IV - A New Hope')
insert into program_names (program_name) values ('Stepmom')
insert into program_names (program_name) values ('Take Your Best Shot')
insert into program_names (program_name) values ('Terror in the Shadows')
insert into program_names (program_name) values ('Thin Red Line, The')
insert into program_names (program_name) values ('To Be Announced')
insert into program_names (program_name) values ('Top Gun')
insert into program_names (program_name) values ('Valley Girl')
insert into program_names (program_name) values ('Varsity Blues')
insert into program_names (program_name) values ('Waking Ned Devine')
insert into program_names (program_name) values ('Wall, The')
insert into program_names (program_name) values ('Welcome Home')
insert into program_names (program_name) values ('What''s Up, Tiger Lily?')
insert into program_names (program_name) values ('Where the Heart Is')
insert into program_names (program_name) values ('Wild Flower')
insert into program_names (program_name) values ('Yearling, The')
end


/*************************************** END BCOP-2409 ***********************************************************/

/*************************************** BCOP-2513 ***************************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'match_isci_days'
              AND OBJECT_ID = OBJECT_ID('affidavit_client_scrubs'))
BEGIN
    ALTER TABLE affidavit_client_scrubs
	ADD match_isci_days BIT NULL
	
	EXEC('UPDATE affidavit_client_scrubs
	      SET match_isci_days = 1')
		  
	ALTER TABLE affidavit_client_scrubs
	ALTER COLUMN match_isci_days BIT NOT NULL
END

/*************************************** END BCOP-2513 ***********************************************************/

/*************************************** BCOP-2411/2561 ***********************************************************/

if not exists(select 1 from genres where name = 'Instructional')
insert into genres (name, created_by, created_date, modified_by, modified_date)
values ('Instructional', 'System', current_timestamp, 'System', current_timestamp)
go

/*************************************** END BCOP-2411/2561 ***********************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '18.03.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '18.02.1' -- Previous release version
		OR [version] = '18.03.1') -- Current release version
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