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
/*************************************** END BCOP-2910 ***************************************************************/

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