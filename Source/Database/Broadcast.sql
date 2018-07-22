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

/*************************************** START BCOP-3297 ***************************************************************/
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'effective_genre' AND object_id = OBJECT_ID('affidavit_client_scrubs'))
BEGIN
	ALTER TABLE dbo.affidavit_client_scrubs ALTER COLUMN effective_genre VARCHAR(255) NULL
END
GO
/*************************************** END BCOP-3297 ***************************************************************/

/*************************************** START BCOP-2800 ***************************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'nti_conversion_factor' AND object_id = OBJECT_ID('proposal_version_details'))
BEGIN
	ALTER TABLE dbo.proposal_version_details ADD [nti_conversion_factor] FLOAT CONSTRAINT DF_proposal_version_details_nti_conversion_factor DEFAULT(0.2) NOT NULL;
	ALTER TABLE dbo.proposal_version_details DROP CONSTRAINT DF_proposal_version_details_nti_conversion_factor
END
GO
/*************************************** END BCOP-2800 ***************************************************************/




IF EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_affidavit_files_media_month_id')   
BEGIN
	DROP INDEX IX_affidavit_files_media_month_id 	ON affidavit_files  
END
GO  



IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'media_month_id'
          AND Object_ID = Object_ID(N'affidavit_files'))
BEGIN
    ALTER TABLE affidavit_files DROP COLUMN media_month_id;  
END


--affidavit_files
--    Should create FK/Index on media_month_id
--IF NOT EXISTS (SELECT * FROM sys.foreign_keys
--            WHERE name = N'FK_affidavit_files_media_month_id')   
--BEGIN
--	ALTER TABLE affidavit_files
--	ADD CONSTRAINT FK_affidavit_files_media_month_id FOREIGN KEY (media_month_id)     
--		REFERENCES media_months (id)     
--		ON DELETE CASCADE ;    
--END
--GO



/************************* START BCOP-3228 **********************************************/

--affidavit_file_detail_demographics
--    audience_id
--    affidavit_file_detail_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_affidavit_file_detail_demographics_audience_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_affidavit_file_detail_demographics_audience_id
		ON affidavit_file_detail_demographics  (audience_id);   
END
GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_affidavit_file_detail_demographics_affidavit_file_detail_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_affidavit_file_detail_demographics_affidavit_file_detail_id
		ON affidavit_file_detail_demographics  (affidavit_file_detail_id);   
END
GO  


--affidavit_file_detail_problems
--    affidavit_file_detail_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_affidavit_file_detail_problems_affidavit_file_detail_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_affidavit_file_detail_problems_affidavit_file_detail_id
		ON affidavit_file_detail_problems  (affidavit_file_detail_id);   
END
GO  





--affidavit_file_problems
--    affidavit_file_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_affidavit_file_problems_affidavit_file_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_affidavit_file_problems_affidavit_file_id 
		ON affidavit_file_problems  (affidavit_file_id);   
END
GO  

--affidavit_blacklist
--    ISCI (text)
--    ISCI is used in searches, might be easy index.
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_affidavit_blacklist_ISCI')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_affidavit_blacklist_ISCI
		ON affidavit_blacklist  (ISCI);   
END
GO  


--affidavit_outbound_file_problems
--    affidavit_outbound_file_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_affidavit_outbound_file_problems_affidavit_outbound_file_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_affidavit_outbound_file_problems_affidavit_outbound_file_id 
		ON affidavit_outbound_file_problems  (affidavit_outbound_file_id);   
END
GO  


--station_contacts
--    station_code
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_contacts_station_code')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_contacts_station_code 
		ON station_contacts  (station_code);   
END
GO  



--station_inventory_group
--    inventory_source_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_group_inventory_source_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_group_inventory_source_id 
		ON station_inventory_group  (inventory_source_id);   
END
GO  


--station_inventory_manifest
--    station_code
--    spot_length_id
--    inventory_source_id
--    station_inventory_group_id
--    file_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_station_code')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_station_code
		ON station_inventory_manifest  (station_code);   
END
GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_spot_length_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_spot_length_id
		ON station_inventory_manifest  (spot_length_id);   
END
GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_inventory_source_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_inventory_source_id
		ON station_inventory_manifest  (inventory_source_id);   
END
GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_station_inventory_group_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_station_inventory_group_id
		ON station_inventory_manifest  (station_inventory_group_id);   
END
GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_file_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_file_id 
		ON station_inventory_manifest  (file_id);   
END
GO  

--station_inventory_manifest_audiences
--    audience_id
--    station_inventory_manifest_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_audiences_audience_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_audiences_audience_id
		ON station_inventory_manifest_audiences  (audience_id);   
END
GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_audiences_station_inventory_manifest_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_audiences_station_inventory_manifest_id
		ON station_inventory_manifest_audiences  (station_inventory_manifest_id);   
END
GO  


--station_inventory_manifest_dayparts
--    daypart_id
--    station_inventory_manifest_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_dayparts_daypart_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_dayparts_daypart_id 
		ON station_inventory_manifest_dayparts  (daypart_id);   
END
GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_dayparts_station_inventory_manifest_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_dayparts_station_inventory_manifest_id 
		ON station_inventory_manifest_dayparts  (station_inventory_manifest_id);   
END
GO  



--station_inventory_manifest_generation
--    station_inventory_manifest_id
--    media_week_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_generation_station_inventory_manifest_id')   
BEGIN
	CREATE  NONCLUSTERED INDEX IX_station_inventory_manifest_generation_station_inventory_manifest_id
		ON station_inventory_manifest_generation  (station_inventory_manifest_id);   
END
GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_generation_media_week_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_generation_media_week_id
		ON station_inventory_manifest_generation  (media_week_id);   
END
GO  

--station_inventory_manifest_rates
--    station_inventory_manifest_id
--    spot_length_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_rates_station_inventory_manifest_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_rates_station_inventory_manifest_id
		ON station_inventory_manifest_rates  (station_inventory_manifest_id);   
END
GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_manifest_rates_spot_length_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_manifest_rates_spot_length_id 
		ON station_inventory_manifest_rates  (spot_length_id);   
END
GO  



--station_inventory_spots
--    proposal_version_detail_quarter_week_id
--    station_inventory_manifest_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_spots_proposal_version_detail_quarter_week_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_spots_proposal_version_detail_quarter_week_id
		ON station_inventory_spots  (proposal_version_detail_quarter_week_id);   
END
GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_station_inventory_spots_station_inventory_manifest_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_station_inventory_spots_station_inventory_manifest_id 
		ON station_inventory_spots  (station_inventory_manifest_id);   
END
GO  


--inventory_files
--    sweep_book_id
--    inventory_source_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_inventory_files_sweep_book_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_inventory_files_sweep_book_id
		ON inventory_files  (sweep_book_id);   
END
GO  

IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_inventory_files_inventory_source_id')
BEGIN
	CREATE NONCLUSTERED INDEX IX_inventory_files_inventory_source_id 
		ON inventory_files  (inventory_source_id);   
END
GO


--media_weeks
--    media_month_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_media_weeks_media_month_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_media_weeks_media_month_id 
		ON media_weeks  (media_month_id);   
END
GO  


--post_file_details
--    post_file_id
--    spot_length_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_post_file_details_post_file_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_post_file_details_post_file_id 
		ON post_file_details  (post_file_id);   

END

GO  
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_post_file_details_spot_length_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_post_file_details_spot_length_id 
		ON post_file_details  (spot_length_id);   

END

GO  


--post_files
--    posting_book_id
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_post_files_posting_book_id')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_post_files_posting_book_id 
		ON post_files  (posting_book_id);   

END

GO  


--program_name
--  program_names
IF NOT EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'IX_program_names_program_name')   
BEGIN
	CREATE NONCLUSTERED INDEX IX_program_names_program_name 
		ON program_names  ([program_name]);   

END

GO  

/************************* END BCOP-3228 **********************************************/


/*************************************** START BCOP-3280 & BCOP-3336 ***************************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'adjustment_margin' AND object_id = OBJECT_ID('proposal_version_details'))
BEGIN
	ALTER TABLE dbo.proposal_version_details ADD [adjustment_margin] FLOAT 
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'adjustment_rate' AND object_id = OBJECT_ID('proposal_version_details'))
BEGIN
	ALTER TABLE dbo.proposal_version_details ADD [adjustment_rate] FLOAT 
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'adjustment_inflation' AND object_id = OBJECT_ID('proposal_version_details'))
BEGIN
	ALTER TABLE dbo.proposal_version_details ADD [adjustment_inflation] FLOAT 
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'goal_impression' AND object_id = OBJECT_ID('proposal_version_details'))
BEGIN
	ALTER TABLE dbo.proposal_version_details ADD [goal_impression] FLOAT 
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'goal_budget' AND object_id = OBJECT_ID('proposal_version_details'))
BEGIN
	ALTER TABLE dbo.proposal_version_details ADD [goal_budget] money 
END
GO

/*************************************** END BCOP-3280 & BCOP-3336 ***************************************************************/


/*************************************** START BCOP-3324 ***************************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'market_coverage' AND object_id = OBJECT_ID('proposal_versions'))
BEGIN
	ALTER TABLE dbo.proposal_versions ADD [market_coverage] FLOAT CONSTRAINT DF_proposal_versions_market_coverage DEFAULT(0.8) NULL;
	ALTER TABLE dbo.proposal_version_details DROP CONSTRAINT DF_proposal_versions_market_coverage
END
GO
ALTER TABLE dbo.proposal_versions ALTER COLUMN markets TINYINT NULL
GO
IF EXISTS(SELECT * FROM proposal_versions WHERE WHERE blackout_markets = 0)
BEGIN
	UPDATE proposal_versions
	SET blackout_markets = 1
	WHERE blackout_markets = 0
END
GO
IF EXISTS(SELECT * FROM proposal_versions WHERE blackout_markets = 100)
BEGIN
	UPDATE proposal_versions
	SET blackout_markets = 1
	WHERE blackout_markets = 100
END
GO
IF EXISTS(SELECT * FROM proposal_versions WHERE blackout_markets = 50)
BEGIN
	UPDATE proposal_versions
	SET blackout_markets = 1
	WHERE blackout_markets = 50
END
GO
IF EXISTS(SELECT * FROM proposal_versions WHERE blackout_markets = 255)
BEGIN
	UPDATE proposal_versions
	SET blackout_markets = null
	WHERE blackout_markets = 255
END
GO
IF EXISTS(SELECT * FROM proposal_versions WHERE markets = 0)
BEGIN
	UPDATE proposal_versions
	SET markets = 1
	WHERE markets = 0
END
GO
IF EXISTS(SELECT * FROM proposal_versions WHERE markets = 100)
BEGIN
	UPDATE proposal_versions
	SET markets = 1
	WHERE markets = 100
END
GO
IF EXISTS(SELECT * FROM proposal_versions WHERE markets = 50)
BEGIN
	UPDATE proposal_versions
	SET markets = 1
	WHERE markets = 50
END
GO
IF EXISTS(SELECT * FROM proposal_versions WHERE markets = 255)
BEGIN
	UPDATE proposal_versions
	SET markets = null
	WHERE markets = 255
END
/*************************************** END BCOP-3324 ***************************************************************/



/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '18.08.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '18.06.1' -- Previous release version
		OR [version] = '18.08.1') -- Current release version
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