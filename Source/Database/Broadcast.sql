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

/*************************************** START BCOP-2665 *********************************************************/
IF EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'order' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
    EXEC sp_rename 'proposal_version_details.order', 'sequence', 'COLUMN';
END
IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE name = 'sequence' AND OBJECT_ID = OBJECT_ID('[dbo].[proposal_version_details]'))
BEGIN
	ALTER TABLE [proposal_version_details] 
	ADD [sequence] INT
END
/*************************************** END BCOP-2665 ***********************************************************/

/*************************************** START BCOP-2449 *********************************************************/
IF OBJECT_ID('affidavit_file_detail_demographics', 'U') IS NULL
BEGIN
	CREATE TABLE [affidavit_file_detail_demographics](
		id INT IDENTITY(1,1) NOT NULL,
		audience_id INT,
		affidavit_file_detail_id BIGINT,
		overnight_rating FLOAT,
		overnight_impressions INT		
		CONSTRAINT [PK_affidavit_file_detail_demographics] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]		
	)
	ALTER TABLE [dbo].[affidavit_file_detail_demographics] 
	WITH CHECK ADD CONSTRAINT [FK_affidavit_file_detail_demographics_audiences] 
	FOREIGN KEY(audience_id)
	REFERENCES [dbo].[audiences] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[affidavit_file_detail_demographics] CHECK CONSTRAINT [FK_affidavit_file_detail_demographics_audiences]

	ALTER TABLE [dbo].[affidavit_file_detail_demographics] 
	WITH CHECK ADD CONSTRAINT [FK_affidavit_file_detail_demographics_affidavit_file_details] 
	FOREIGN KEY([affidavit_file_detail_id])
	REFERENCES [dbo].[affidavit_file_details] ([id])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[affidavit_file_detail_demographics] CHECK CONSTRAINT [FK_affidavit_file_detail_demographics_affidavit_file_details]
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE (name = 'estimate_id' OR name = 'inventory_source' OR name = 'spot_cost' OR name = 'affiliate')
              AND OBJECT_ID = OBJECT_ID('[dbo].[affidavit_file_details]'))
BEGIN
    ALTER TABLE [affidavit_file_details] 
	ADD estimate_id INT, 
		inventory_source INT, 
		spot_cost FLOAT, 
		affiliate VARCHAR(15)
END

/*************************************** END BCOP-2517 ***********************************************************/

/*************************************** START BCOP-2517 *********************************************************/
IF OBJECT_ID('affidavit_outbound_files', 'U') IS NULL
BEGIN
    CREATE TABLE affidavit_outbound_files(
		id INT IDENTITY(1,1) NOT NULL,
		file_name VARCHAR(255) NOT NULL,
		file_hash VARCHAR(63) NOT NULL,
		source_id INT NOT NULL,
		status INT NOT NULL,
		created_date DATETIME NOT NULL,
		created_by VARCHAR(63) NOT NULL,
		CONSTRAINT [PK_affidavit_outbound_files] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	)
END

IF OBJECT_ID('affidavit_outbound_file_problems', 'U') IS NULL
BEGIN
	CREATE TABLE affidavit_outbound_file_problems(
		id INT IDENTITY(1,1) NOT NULL,
		affidavit_outbound_file_id  INT,
		problem_description VARCHAR(255) NOT NULL,		
		CONSTRAINT [PK_affidavit_outbound_file_problems] PRIMARY KEY CLUSTERED
		(
			id ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]		
	)
	ALTER TABLE [dbo].[affidavit_outbound_file_problems] 
	WITH CHECK ADD CONSTRAINT [FK_affidavit_outbound_file_problems_affidavit_outbound_files] 
	FOREIGN KEY(affidavit_outbound_file_id)
	REFERENCES [dbo].[affidavit_outbound_files] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[affidavit_outbound_file_problems] CHECK CONSTRAINT [FK_affidavit_outbound_file_problems_affidavit_outbound_files]
END

/*************************************** END BCOP-2517 ***********************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '18.05.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '18.04.1' -- Previous release version
		OR [version] = '18.05.1') -- Current release version
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