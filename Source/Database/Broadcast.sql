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