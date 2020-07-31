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

/*************************************** START - BP-90 ****************************************************/
BEGIN TRAN
BEGIN TRY

IF OBJECT_ID('plan_version_pricing_api_result_spot_frequencies') IS NULL
BEGIN 
	CREATE TABLE [plan_version_pricing_api_result_spot_frequencies]
	(
		[id] INT IDENTITY(1,1) NOT NULL,
		[plan_version_pricing_api_result_spot_id] INT NOT NULL,
		[spot_length_id] INT NOT NULL,
		[cost] MONEY NOT NULL,
		[spots] INT NOT NULL

		CONSTRAINT [PK_plan_version_pricing_api_result_spot_frequencies] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)

	ALTER TABLE [dbo].[plan_version_pricing_api_result_spot_frequencies]
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_api_result_spot_frequencies_plan_version_pricing_api_result_spots] 
	FOREIGN KEY([plan_version_pricing_api_result_spot_id])
	REFERENCES [dbo].[plan_version_pricing_api_result_spots] ([id])

	ALTER TABLE [dbo].[plan_version_pricing_api_result_spot_frequencies] 
	CHECK CONSTRAINT [FK_plan_version_pricing_api_result_spot_frequencies_plan_version_pricing_api_result_spots]


	ALTER TABLE [dbo].[plan_version_pricing_api_result_spot_frequencies]
	WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_api_result_spot_frequencies_spot_lengths]
	FOREIGN KEY([spot_length_id])
	REFERENCES [dbo].[spot_lengths] ([id])

	ALTER TABLE [dbo].[plan_version_pricing_api_result_spot_frequencies]
	CHECK CONSTRAINT [FK_plan_version_pricing_api_result_spot_frequencies_spot_lengths]

	EXEC('
		INSERT INTO plan_version_pricing_api_result_spot_frequencies(
			plan_version_pricing_api_result_spot_id, 
			spot_length_id, 
			cost, 
			spots)
		select s.id as plan_version_pricing_api_result_spot_id, 
			   pvcl.spot_length_id,
			   s.cost,
			   s.spots
		from plan_version_pricing_api_result_spots as s
		join plan_version_creative_lengths as pvcl
		on pvcl.id = (select top 1 cl.id
					  from plan_version_pricing_api_result_spots as rs
					  join plan_version_pricing_api_results as r on rs.plan_version_pricing_api_results_id = r.id
					  join plan_version_pricing_job as j on j.id = r.plan_version_pricing_job_id
					  join plan_versions as v on j.plan_version_id = v.id
					  join plan_version_creative_lengths as cl on cl.plan_version_id = v.id
					  where rs.id = s.id)
	')
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_result_spots') AND name = 'cost')
BEGIN
	EXEC('ALTER TABLE plan_version_pricing_api_result_spots DROP COLUMN cost')
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_result_spots') AND name = 'spots')
BEGIN
	EXEC('ALTER TABLE plan_version_pricing_api_result_spots DROP COLUMN spots')
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_result_spots') AND name = 'impressions')
BEGIN
	EXEC sp_rename 'dbo.plan_version_pricing_api_result_spots.impressions', 'impressions30sec', 'COLUMN';  
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_api_results') AND name = 'pricing_version')
BEGIN
	ALTER TABLE plan_version_pricing_api_results ADD pricing_version varchar(10) NULL
	EXEC('update plan_version_pricing_api_results set pricing_version = ''2''')
	ALTER TABLE plan_version_pricing_api_results ALTER COLUMN pricing_version varchar(10) NOT NULL
END

COMMIT TRAN
END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0)
	BEGIN
	  ROLLBACK TRAN;
	END;
	THROW
END CATCH

/*************************************** END - BP-90 ****************************************************/

/*************************************** START BP-836 *****************************************************/
-- DROP Foreign Keys 
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_plan_version_pricing_api_results_plan_versions' 
				AND parent_object_id = OBJECT_ID('plan_version_pricing_api_results'))
BEGIN
	ALTER TABLE [plan_version_pricing_api_results] DROP [FK_plan_version_pricing_api_results_plan_versions]
END
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_plan_version_pricing_bands_plan_versions' 
				AND parent_object_id = OBJECT_ID('plan_version_pricing_bands'))
BEGIN
	ALTER TABLE [plan_version_pricing_bands] DROP [FK_plan_version_pricing_bands_plan_versions]
END
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_plan_version_pricing_markets_plan_version' 
				AND parent_object_id = OBJECT_ID('plan_version_pricing_markets'))
BEGIN
	ALTER TABLE [plan_version_pricing_markets] DROP [FK_plan_version_pricing_markets_plan_version]
END
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_plan_version_pricing_results_plan_versions' 
				AND parent_object_id = OBJECT_ID('plan_version_pricing_results'))
BEGIN
	ALTER TABLE [plan_version_pricing_results] DROP [FK_plan_version_pricing_results_plan_versions]
END
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_plan_versions_plan_version_pricing_stations' 
				AND parent_object_id = OBJECT_ID('plan_version_pricing_stations'))
BEGIN
	ALTER TABLE [plan_version_pricing_stations] DROP [FK_plan_versions_plan_version_pricing_stations]
END

-- DROP plan_version_id column
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id'
				AND object_id = OBJECT_ID('plan_version_pricing_api_results'))
BEGIN
	ALTER TABLE [plan_version_pricing_api_results] DROP COLUMN [plan_version_id]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id'
				AND object_id = OBJECT_ID('plan_version_pricing_bands'))
BEGIN
	ALTER TABLE [plan_version_pricing_bands] DROP COLUMN [plan_version_id]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id'
				AND object_id = OBJECT_ID('plan_version_pricing_markets'))
BEGIN
	ALTER TABLE [plan_version_pricing_markets] DROP COLUMN [plan_version_id]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id'
				AND object_id = OBJECT_ID('plan_version_pricing_results'))
BEGIN
	ALTER TABLE [plan_version_pricing_results] DROP COLUMN [plan_version_id]
END
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_id'
				AND object_id = OBJECT_ID('plan_version_pricing_stations'))
BEGIN
	ALTER TABLE [plan_version_pricing_stations] DROP COLUMN [plan_version_id]
END
/*************************************** END BP-836 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.09.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.08.1' -- Previous release version
		OR [version] = '20.09.1') -- Current release version
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