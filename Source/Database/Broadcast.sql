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
/*************************************** START BCOP-3883 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'supplied_program_name' AND OBJECT_ID = OBJECT_ID('[dbo].[affidavit_file_details]'))
BEGIN
	ALTER TABLE [affidavit_file_details] ADD [supplied_program_name] VARCHAR(255) NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'program_show_type' AND OBJECT_ID = OBJECT_ID('[dbo].[affidavit_file_details]'))
BEGIN
	ALTER TABLE [affidavit_file_details] ALTER COLUMN [program_show_type] VARCHAR(255) NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'leadin_show_type' AND OBJECT_ID = OBJECT_ID('[dbo].[affidavit_file_details]'))
BEGIN
	ALTER TABLE [affidavit_file_details] ALTER COLUMN [leadin_show_type] VARCHAR(255) NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'leadout_show_type' AND OBJECT_ID = OBJECT_ID('[dbo].[affidavit_file_details]'))
BEGIN
	ALTER TABLE [affidavit_file_details] ALTER COLUMN [leadout_show_type] VARCHAR(255) NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'leadin_end_time' AND OBJECT_ID = OBJECT_ID('[dbo].[affidavit_file_details]'))
BEGIN
	ALTER TABLE [affidavit_file_details] ALTER COLUMN [leadin_end_time] INT NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'leadout_start_time' AND OBJECT_ID = OBJECT_ID('[dbo].[affidavit_file_details]'))
BEGIN
	ALTER TABLE [affidavit_file_details] ALTER COLUMN [leadout_start_time] INT NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'effective_program_name' AND OBJECT_ID = OBJECT_ID('[dbo].[affidavit_client_scrubs]'))
BEGIN
	ALTER TABLE [affidavit_client_scrubs] ALTER COLUMN [effective_program_name] VARCHAR(255) NULL
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'effective_show_type' AND OBJECT_ID = OBJECT_ID('[dbo].[affidavit_client_scrubs]'))
BEGIN
	ALTER TABLE [affidavit_client_scrubs] ALTER COLUMN [effective_show_type] VARCHAR(255) NULL
END
/*************************************** END BCOP-3883 *****************************************************/

/*************************************** START BCOP-3884 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'supplied_program_name' AND OBJECT_ID = OBJECT_ID('[dbo].[postlog_file_details]'))
BEGIN
	ALTER TABLE [postlog_file_details] ADD [supplied_program_name] VARCHAR(255) NULL
END
/*************************************** END BCOP-3884 *****************************************************/
 



/******************** START BCOP-3899 *********************************************************************/
IF EXISTS ( SELECT  * FROM    sys.objects WHERE   object_id = OBJECT_ID(N'usp_GetPostedProposals') AND type IN ( N'P', N'PC' ) ) 
BEGIN
	DROP PROCEDURE usp_GetPostedProposals
END 

GO 
CREATE PROCEDURE usp_GetPostedProposals
AS
BEGIN

	SELECT
		v.proposal_id as ContractId ,
		v.equivalized as Equivalized,
		p.Name as ContractName,
		(select Max(af.created_date) 
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				join proposal_version_detail_quarter_weeks w on w.proposal_version_quarter_id = q.id 
				join affidavit_client_scrubs s on s.proposal_version_detail_quarter_week_id = w.id
				join affidavit_file_details afd on s.affidavit_file_detail_id = afd.id
				join affidavit_files af on af.id = afd.affidavit_file_id
				where d.proposal_version_id = v.id) as UploadDate ,
		(select count(s.id) 
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				join proposal_version_detail_quarter_weeks w on w.proposal_version_quarter_id = q.id 
				join affidavit_client_scrubs s on s.proposal_version_detail_quarter_week_id = w.id
				where d.proposal_version_id = v.id and s.status = 2) as SpotsInSpec ,
		(select count(s.id) 
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				join proposal_version_detail_quarter_weeks w on w.proposal_version_quarter_id = q.id 
				join affidavit_client_scrubs s on s.proposal_version_detail_quarter_week_id = w.id
				where d.proposal_version_id = v.id and s.status = 1)as SpotsOutOfSpec ,
		p.advertiser_id as AdvertiserId,
		v.guaranteed_audience_id as GuaranteedAudienceId,
		v.post_type as PostType,
     		(select sum(q.impressions_goal) 
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				where d.proposal_version_id = v.id) as PrimaryAudienceBookedImpressions 
	from proposal_versions v
		join proposals p on p.id = v.proposal_id
	where v.status = 3


END

GO
/******************** END BCOP-3899 *********************************************************************/




/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '18.12.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '18.11.1' -- Previous release version
		OR [version] = '18.12.1') -- Current release version
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