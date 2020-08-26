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


/*************************************** START - BP-51 ****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_result_spot_frequencies') AND name = 'impressions')
BEGIN
	ALTER TABLE plan_version_pricing_api_result_spot_frequencies
	ADD impressions float NULL

	EXEC('UPDATE f
	      SET f.impressions = s.impressions30sec
	      FROM plan_version_pricing_api_result_spot_frequencies f
	      JOIN plan_version_pricing_api_result_spots as s on f.plan_version_pricing_api_result_spot_id = s.id
	      JOIN plan_version_pricing_api_results as r on s.plan_version_pricing_api_results_id = r.id
	      WHERE r.pricing_version = ''2''')

	EXEC('UPDATE f
	      SET f.impressions = (s.impressions30sec * 
		case pv.equivalized
			when 0 then 1
			when 1 then sl.delivery_multiplier
		end)
	      FROM plan_version_pricing_api_result_spot_frequencies as f
		  JOIN plan_version_pricing_api_result_spots as s on f.plan_version_pricing_api_result_spot_id = s.id
		  JOIN plan_version_pricing_api_results as r on s.plan_version_pricing_api_results_id = r.id
		  JOIN plan_version_pricing_job as j on r.plan_version_pricing_job_id = j.id
		  JOIN plan_versions as pv on j.plan_version_id = pv.id
		  JOIN spot_lengths as sl on f.spot_length_id = sl.id
		  WHERE r.pricing_version = ''3''')

	ALTER TABLE plan_version_pricing_api_result_spot_frequencies
	ALTER COLUMN impressions float NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_api_result_spot_frequencies') AND name = 'impressions')
BEGIN
	ALTER TABLE plan_version_buying_api_result_spot_frequencies
	ADD impressions float NULL

	EXEC('UPDATE f
		  SET f.impressions = s.impressions30sec
		  FROM plan_version_buying_api_result_spot_frequencies f
		  JOIN plan_version_buying_api_result_spots as s on f.plan_version_buying_api_result_spot_id = s.id
		  JOIN plan_version_buying_api_results as r on s.plan_version_buying_api_results_id = r.id
		  WHERE r.buying_version = ''2''')

	EXEC('UPDATE f
	      SET f.impressions = (s.impressions30sec * 
		case pv.equivalized
			when 0 then 1
			when 1 then sl.delivery_multiplier
		end)
	      FROM plan_version_buying_api_result_spot_frequencies as f
		  JOIN plan_version_buying_api_result_spots as s on f.plan_version_buying_api_result_spot_id = s.id
		  JOIN plan_version_buying_api_results as r on s.plan_version_buying_api_results_id = r.id
		  JOIN plan_version_buying_job as j on r.plan_version_buying_job_id = j.id
		  JOIN plan_versions as pv on j.plan_version_id = pv.id
		  JOIN spot_lengths as sl on f.spot_length_id = sl.id
		  WHERE r.buying_version = ''3''')

	ALTER TABLE plan_version_buying_api_result_spot_frequencies
	ALTER COLUMN impressions float NOT NULL
END
/*************************************** END - BP-51 ****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.10.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.09.1' -- Previous release version
		OR [version] = '20.10.1') -- Current release version
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