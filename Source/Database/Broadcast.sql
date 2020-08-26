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
/*************************************** START BP-894 *******************************************************/

--add total market coverage percentage 
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_results') 
				AND name = 'total_market_coverage_percent')
BEGIN
	ALTER TABLE [plan_version_buying_results] ADD [total_market_coverage_percent] FLOAT NULL

	EXEC('UPDATE t1
	SET t1.total_market_coverage_percent = t2.total_coverage_percent
	FROM [plan_version_buying_results] AS t1
	INNER JOIN [plan_version_buying_markets] AS t2 ON t1.plan_version_buying_job_id = t2.plan_version_buying_job_id')

	ALTER TABLE [plan_version_buying_results] ALTER COLUMN [total_market_coverage_percent] FLOAT NOT NULL
END

--add plan_version_buying_result_id column and all related changes
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_market_details') 
		AND name = 'plan_version_buying_result_id')
BEGIN
	ALTER TABLE [plan_version_buying_market_details] ADD [plan_version_buying_result_id] INT NULL

	EXEC('UPDATE t1 
			SET  t1.plan_version_buying_result_id = t3.id
			FROM plan_version_buying_market_details AS t1
			INNER JOIN plan_version_buying_markets AS t2 ON t1.plan_version_buying_market_id = t2.id
			INNER JOIN [plan_version_buying_results] AS t3 ON t2.plan_version_buying_job_id = t3.plan_version_buying_job_id')

	ALTER TABLE [plan_version_buying_market_details] ALTER COLUMN [plan_version_buying_result_id] INT NOT NULL

	--drop FK constraint on old column [plan_version_buying_market_id]
	IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_buying_market_details')
					AND name = 'FK_plan_version_buying_market_details_buying_market')
	BEGIN
		ALTER TABLE [plan_version_buying_market_details] DROP [FK_plan_version_buying_market_details_buying_market]
	END

	--drop old column plan_version_buying_market_id
	IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_market_details') 
		AND name = 'plan_version_buying_market_id')
	BEGIN
		ALTER TABLE [plan_version_buying_market_details] DROP COLUMN [plan_version_buying_market_id]
	END

	--add FK constraint on the new column plan_version_buying_result_id
	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID('plan_version_buying_market_details')
					AND name = 'FK_plan_version_buying_market_details_plan_version_buying_results')
	BEGIN
		ALTER TABLE [dbo].[plan_version_buying_market_details]
		ADD CONSTRAINT [FK_plan_version_buying_market_details_plan_version_buying_results] 
		FOREIGN KEY([plan_version_buying_result_id])
		REFERENCES [dbo].[plan_version_buying_results] ([id])
	END

	--drop table plan_version_buying_markets
	IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_markets'))
	BEGIN
		DROP TABLE [dbo].[plan_version_buying_markets]
	END
END

--add plan_version_buying_result_id column and all related changes
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_station_details') 
		AND name = 'plan_version_buying_result_id')
BEGIN
	ALTER TABLE [plan_version_buying_station_details] ADD [plan_version_buying_result_id] INT NULL

	EXEC('UPDATE t1 
			SET  t1.plan_version_buying_result_id = t3.id
			FROM plan_version_buying_station_details AS t1
			INNER JOIN plan_version_buying_stations AS t2 ON t1.plan_version_buying_station_id = t2.id
			INNER JOIN [plan_version_buying_results] AS t3 ON t2.plan_version_buying_job_id = t3.plan_version_buying_job_id')

	ALTER TABLE [plan_version_buying_station_details] ALTER COLUMN [plan_version_buying_result_id] INT NOT NULL

	--drop FK constraint on old column [plan_version_buying_station_id]
	IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_buying_station_details')
					AND name = 'FK_plan_version_buying_stations_plan_version_buying_station_details')
	BEGIN
		ALTER TABLE [plan_version_buying_station_details] DROP [FK_plan_version_buying_stations_plan_version_buying_station_details]
	END

	--drop old column plan_version_buying_station_id
	IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_station_details') 
		AND name = 'plan_version_buying_station_id')
	BEGIN
		ALTER TABLE [plan_version_buying_station_details] DROP COLUMN [plan_version_buying_station_id]
	END

	--add FK constraint on the new column plan_version_buying_result_id
	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID('plan_version_buying_station_details')
					AND name = 'FK_plan_version_buying_station_details_plan_version_buying_results')
	BEGIN
		ALTER TABLE [dbo].[plan_version_buying_station_details]
		ADD CONSTRAINT [FK_plan_version_buying_station_details_plan_version_buying_results] 
		FOREIGN KEY([plan_version_buying_result_id])
		REFERENCES [dbo].[plan_version_buying_results] ([id])
	END

	--drop table plan_version_buying_stations
	IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_stations'))
	BEGIN
		DROP TABLE [dbo].[plan_version_buying_stations]
	END
END

--add plan_version_buying_result_id column and all related changes
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_band_details') 
		AND name = 'plan_version_buying_result_id')
BEGIN
	ALTER TABLE [plan_version_buying_band_details] ADD [plan_version_buying_result_id] INT NULL

	EXEC('UPDATE t1 
			SET  t1.plan_version_buying_result_id = t3.id
			FROM plan_version_buying_band_details AS t1
			INNER JOIN plan_version_buying_bands AS t2 ON t1.plan_version_buying_band_id = t2.id
			INNER JOIN [plan_version_buying_results] AS t3 ON t2.plan_version_buying_job_id = t3.plan_version_buying_job_id')

	ALTER TABLE [plan_version_buying_band_details] ALTER COLUMN [plan_version_buying_result_id] INT NOT NULL

	--drop FK constraint on old column [plan_version_buying_band_id]
	IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('plan_version_buying_band_details')
					AND name = 'FK_plan_version_buying_band_details_plan_version_buying_bands')
	BEGIN
		ALTER TABLE [plan_version_buying_band_details] DROP [FK_plan_version_buying_band_details_plan_version_buying_bands]
	END

	--drop old column plan_version_buying_band_id
	IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_buying_band_details') 
		AND name = 'plan_version_buying_band_id')
	BEGIN
		ALTER TABLE [plan_version_buying_band_details] DROP COLUMN [plan_version_buying_band_id]
	END

	--add FK constraint on the new column plan_version_buying_result_id
	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID('plan_version_buying_band_details')
					AND name = 'FK_plan_version_buying_band_details_plan_version_buying_results')
	BEGIN
		ALTER TABLE [dbo].[plan_version_buying_band_details]
		ADD CONSTRAINT [FK_plan_version_buying_band_details_plan_version_buying_results] 
		FOREIGN KEY([plan_version_buying_result_id])
		REFERENCES [dbo].[plan_version_buying_results] ([id])
	END

	--drop table plan_version_buying_bands
	IF EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_bands'))
	BEGIN
		DROP TABLE [dbo].[plan_version_buying_bands]
	END
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE object_id = OBJECT_ID('plan_version_buying_ownership_group_details'))
BEGIN
	CREATE TABLE [dbo].[plan_version_buying_ownership_group_details](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_buying_result_id] [int] NOT NULL,	
		[ownership_group_name] [NVARCHAR] (100) NOT NULL,
		[markets] [int] NOT NULL,
		[stations] [int] NOT NULL,
		[spots] [int] NOT NULL,
		[impressions] [float] NOT NULL,
		[cpm] [money] NOT NULL,
		[budget] [money] NOT NULL,
		[impressions_percentage] [float] NOT NULL,
	 CONSTRAINT [PK_plan_version_buying_ownership_group_details] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_buying_ownership_group_details] ADD CONSTRAINT [FK_plan_version_buying_ownership_group_details_plan_version_buying_results] FOREIGN KEY([plan_version_buying_result_id])
	REFERENCES [dbo].[plan_version_buying_results] ([id])
END
/*************************************** END BP-894 *******************************************************/

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