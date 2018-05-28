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

/*************************************** START BCOP-3022 ***************************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.tables where object_id = OBJECT_ID('affidavit_blacklist'))
BEGIN
	CREATE TABLE [affidavit_blacklist](
		id INT IDENTITY(1,1) NOT NULL,
		ISCI VARCHAR(63) NOT NULL,
		created_date DATETIME NULL,
		created_by VARCHAR(255) NOT NULL
		CONSTRAINT [PK_affidavit_blacklist] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]		
	)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'archived' AND object_id = OBJECT_ID('affidavit_file_details'))
BEGIN
	ALTER TABLE affidavit_file_details ADD [archived] BIT NULL;
	UPDATE affidavit_file_details SET [archived] = 0;
	ALTER TABLE affidavit_file_details ALTER COLUMN [archived] BIT NOT  NULL;
END
GO

/*************************************** END BCOP-3022 ***************************************************************/

/*************************************** START BCOP-2910 ***************************************************************/
IF NOT EXISTS(
	SELECT *
	FROM sys.columns 
	WHERE 
		name      = 'single_projection_book_id' AND 
		object_id = OBJECT_ID('proposal_version_details'))
BEGIN
	EXEC sp_RENAME 'proposal_version_details.single_posting_book_id', 'single_projection_book_id', 'COLUMN'
END
GO

IF NOT EXISTS(
	SELECT *
	FROM sys.columns 
	WHERE 
		name      = 'hut_projection_book_id' AND 
		object_id = OBJECT_ID('proposal_version_details'))
BEGIN
	EXEC sp_RENAME 'proposal_version_details.hut_posting_book_id', 'hut_projection_book_id', 'COLUMN'
END
GO

IF NOT EXISTS(
	SELECT *
	FROM sys.columns 
	WHERE 
		name      = 'share_projection_book_id' AND 
		object_id = OBJECT_ID('proposal_version_details'))
BEGIN
	EXEC sp_RENAME 'proposal_version_details.share_posting_book_id', 'share_projection_book_id', 'COLUMN'
END
GO

IF NOT EXISTS(
	SELECT *
	FROM sys.columns 
	WHERE 
		name      = 'posting_book_id' AND 
		object_id = OBJECT_ID('proposal_version_details'))
BEGIN
	ALTER TABLE proposal_version_details
	ADD posting_book_id INT NULL

	ALTER TABLE [dbo].[proposal_version_details]  WITH CHECK ADD  CONSTRAINT [FK_proposal_version_details_posting_media_months] FOREIGN KEY([posting_book_id])
	REFERENCES [dbo].[media_months] ([id])

	ALTER TABLE [dbo].[proposal_version_details] CHECK CONSTRAINT [FK_proposal_version_details_posting_media_months]

END
GO


IF OBJECT_ID('affidavit_client_scrub_audiences', 'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[affidavit_client_scrub_audiences](
		[affidavit_client_scrub_id] [INT] NOT NULL,
		[audience_id] [INT] NOT NULL,
		[impressions] [FLOAT] NOT NULL,
	 CONSTRAINT [PK_affidavit_client_scrub_audiences] PRIMARY KEY CLUSTERED 
	(
		[affidavit_client_scrub_id] ASC,
		[audience_id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
ALTER TABLE [dbo].[affidavit_client_scrub_audiences]  WITH CHECK ADD  CONSTRAINT [FK_affidavit_client_scrub_audiences_affidavit_client_scrubs] FOREIGN KEY([affidavit_client_scrub_id])
REFERENCES [dbo].[affidavit_client_scrubs] ([id])
ON DELETE CASCADE

ALTER TABLE [dbo].[affidavit_client_scrub_audiences] CHECK CONSTRAINT [FK_affidavit_client_scrub_audiences_affidavit_client_scrubs]

ALTER TABLE [dbo].[affidavit_client_scrub_audiences]  WITH CHECK ADD  CONSTRAINT [FK_affidavit_client_scrub_audiences_audiences] FOREIGN KEY([audience_id])
REFERENCES [dbo].[audiences] ([id])

ALTER TABLE [dbo].[affidavit_client_scrub_audiences] CHECK CONSTRAINT [FK_affidavit_client_scrub_audiences_audiences]

INSERT INTO affidavit_client_scrub_audiences 
SELECT acs.id, afda.audience_id, afda.impressions FROM affidavit_file_detail_audiences afda
INNER JOIN affidavit_client_scrubs acs ON afda.affidavit_file_detail_id = acs.affidavit_file_detail_id
END
GO

/*************************************** END BCOP-2910 ***************************************************************/

/*************************************** START BCOP-2376 ***************************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.Columns where name = 'status_override' and object_id = OBJECT_ID('affidavit_client_scrubs'))
BEGIN
	ALTER TABLE [dbo].[affidavit_client_scrubs] 
		ADD status_override bit NOT NULL
		CONSTRAINT DF_affidavit_client_scrubs_status_override DEFAULT 0
END

if exists(SELECT [status] FROM affidavit_client_scrubs WHERE [status] = 0)
BEGIN
	-- get rid of 0 based status value
	UPDATE affidavit_client_scrubs SET [status] =  2 where [status] = 1
	UPDATE affidavit_client_scrubs SET [status] =  1 where [status] = 0
END

/*************************************** END BCOP-2376 ***************************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '18.06.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '18.05.1' -- Previous release version
		OR [version] = '18.06.1') -- Current release version
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