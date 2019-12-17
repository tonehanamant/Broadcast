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

/*************************************** START PRI-17245 *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'cpp' AND  object_id = OBJECT_ID('plan_version_pricing_parameters'))
BEGIN	
	ALTER TABLE plan_version_pricing_parameters ADD cpp MONEY NULL
	
	EXEC('UPDATE plan_version_pricing_parameters
		  SET cpp = 0')
		  
    ALTER TABLE plan_version_pricing_parameters ALTER COLUMN cpp MONEY NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'currency' AND  object_id = OBJECT_ID('plan_version_pricing_parameters'))
BEGIN
	ALTER TABLE plan_version_pricing_parameters ADD currency INT NULL
	
	EXEC('UPDATE plan_version_pricing_parameters
		  SET currency = 1')
		  
    ALTER TABLE plan_version_pricing_parameters ALTER COLUMN currency INT NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'rating_points' AND  object_id = OBJECT_ID('plan_version_pricing_parameters'))
BEGIN
	ALTER TABLE plan_version_pricing_parameters ADD rating_points FLOAT NULL
	
	EXEC('UPDATE plan_version_pricing_parameters
		  SET rating_points = 1000')
		  
    ALTER TABLE plan_version_pricing_parameters ALTER COLUMN rating_points FLOAT NOT NULL
END

IF OBJECT_ID('plan_version_pricing_parameters_inventory_source_percentages') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_pricing_parameters_inventory_source_percentages](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_pricing_parameter_id] [int] NOT NULL,
		[inventory_source_id] [int] NOT NULL,
		[percentage] [int] NOT NULL,
	 CONSTRAINT [PK_plan_version_pricing_parameters_inventory_source_percentages] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_pricing_parameters_inventory_source_percentages]  WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_parameters_inventory_source_percentages_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])

	ALTER TABLE [dbo].[plan_version_pricing_parameters_inventory_source_percentages] CHECK CONSTRAINT [FK_plan_version_pricing_parameters_inventory_source_percentages_inventory_sources]
	
	ALTER TABLE [dbo].[plan_version_pricing_parameters_inventory_source_percentages]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_pricing_parameters_inventory_source_percentages_plan_version_pricing_parameters] FOREIGN KEY([plan_version_pricing_parameter_id])
	REFERENCES [dbo].[plan_version_pricing_parameters] ([id])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[plan_version_pricing_parameters_inventory_source_percentages] CHECK CONSTRAINT [FK_plan_version_pricing_parameters_inventory_source_percentages_plan_version_pricing_parameters]
END

/*************************************** END PRI-17245 *****************************************************/

/*************************************** START PRI-18180 *****************************************************/
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'audience_name' AND  object_id = OBJECT_ID('plan_version_summaries'))
BEGIN		  
    ALTER TABLE plan_version_summaries DROP COLUMN audience_name
END
/*************************************** END PRI-18180 *****************************************************/

/*************************************** START - PRI-16186 ****************************************************/
UPDATE [plan_versions]
SET [version_number] = (SELECT versioned.vn
						FROM (SELECT ROW_NUMBER() OVER(ORDER BY id ASC) AS vn, pv.id, pv.plan_id FROM plan_versions AS pv WHERE pv.plan_id = pv1.plan_id AND 0 = is_draft) AS versioned
						WHERE versioned.id = pv1.id)
FROM [plan_versions] AS pv1
WHERE pv1.plan_id IN (SELECT plan_id FROM plan_versions AS pv 
						GROUP BY plan_id
						HAVING MAX(pv.version_number) != COUNT(*))
/*************************************** END - PRI-16186 ****************************************************/

/*************************************** START - PRI-18985 ****************************************************/
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'delivery_rating_points' AND  object_id = OBJECT_ID('plan_version_secondary_audiences'))
BEGIN		  
    EXEC sp_rename 'dbo.plan_version_secondary_audiences.delivery_rating_points', 'rating_points', 'COLUMN';
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'delivery_impressions' AND  object_id = OBJECT_ID('plan_version_secondary_audiences'))
BEGIN		  
    EXEC sp_rename 'dbo.plan_version_secondary_audiences.delivery_impressions', 'impressions', 'COLUMN';
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'household_cpm' AND  object_id = OBJECT_ID('campaign_summaries'))
BEGIN		  
    EXEC sp_rename 'dbo.campaign_summaries.household_cpm', 'hh_cpm', 'COLUMN';
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'household_delivery_impressions' AND  object_id = OBJECT_ID('campaign_summaries'))
BEGIN		  
    EXEC sp_rename 'dbo.campaign_summaries.household_delivery_impressions', 'hh_impressions', 'COLUMN';
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'household_rating_points' AND  object_id = OBJECT_ID('campaign_summaries'))
BEGIN		  
    EXEC sp_rename 'dbo.campaign_summaries.household_rating_points', 'hh_rating_points', 'COLUMN';
END
/*************************************** END - PRI-18985 ****************************************************/
/*************************************** START - PRI-16134 ****************************************************/

IF OBJECT_ID('plan_version_pricing_api_results') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_pricing_api_results](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_id] [int] NOT NULL,
		[optimal_cpm] [money] NOT NULL
	 CONSTRAINT [PK_plan_version_pricing_api_results] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
		
	ALTER TABLE [dbo].[plan_version_pricing_api_results]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_pricing_api_results_plan_versions] FOREIGN KEY([plan_version_id])
	REFERENCES [dbo].[plan_versions] ([id])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[plan_version_pricing_api_results] CHECK CONSTRAINT [FK_plan_version_pricing_api_results_plan_versions]
END

IF OBJECT_ID('plan_version_pricing_api_result_spots') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_pricing_api_result_spots](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_pricing_api_results_id] [int] NOT NULL,
		[station_inventory_manifest_id] [int] NOT NULL,
		[media_week_id] [int] NOT NULL,
		[impressions] [float] NOT NULL,
		[cost] [money] NOT NULL,
		[spots] [int] NOT NULL,
	 CONSTRAINT [PK_plan_version_pricing_api_result_spots] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]
		
	ALTER TABLE [dbo].[plan_version_pricing_api_result_spots]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_pricing_api_result_spots_plan_version_pricing_api_results] FOREIGN KEY([plan_version_pricing_api_results_id])
	REFERENCES [dbo].[plan_version_pricing_api_results] ([id])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[plan_version_pricing_api_result_spots] CHECK CONSTRAINT [FK_plan_version_pricing_api_result_spots_plan_version_pricing_api_results]
	
	ALTER TABLE [dbo].[plan_version_pricing_api_result_spots]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_pricing_api_result_spots_station_inventory_manifest] FOREIGN KEY([station_inventory_manifest_id])
	REFERENCES [dbo].[station_inventory_manifest] ([id])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[plan_version_pricing_api_result_spots] CHECK CONSTRAINT [FK_plan_version_pricing_api_result_spots_station_inventory_manifest]
	
	ALTER TABLE [dbo].[plan_version_pricing_api_result_spots]  WITH CHECK ADD  CONSTRAINT [FK_plan_version_pricing_api_result_spots_media_weeks] FOREIGN KEY([media_week_id])
	REFERENCES [dbo].[media_weeks] ([id])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[plan_version_pricing_api_result_spots] CHECK CONSTRAINT [FK_plan_version_pricing_api_result_spots_media_weeks]
END

/*************************************** START - PRI-16134 ****************************************************/

/*************************************** START - PRI-19007 ****************************************************/
IF OBJECT_ID('affiliates') IS NULL
BEGIN
	CREATE TABLE [dbo].[affiliates](
		
		[id] [int] IDENTITY(1,1) NOT NULL,
		[name] [varchar](127) NOT NULL,
		[created_by] [varchar](63) NOT NULL,
		[created_date] [datetime] NOT NULL,
		[modified_by] [varchar](63) NOT NULL,
		[modified_date] [datetime] NOT NULL,
	 CONSTRAINT [PK_affiliates] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	INSERT INTO [dbo].[affiliates](
		[name],
		[created_by],
		[created_date],
		[modified_by],
		[modified_date]
	)
	SELECT DISTINCT affiliation, 'System' as created_by, SYSDATETIME() as created_date, 'System' as modified_by, SYSDATETIME() as modified_date
	FROM stations 
	WHERE affiliation IS NOT NULL
END
/*************************************** END - PRI-19007 ****************************************************/

/*************************************** Start - PRI-19008 ****************************************************/

GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'ratings' AND  object_id = OBJECT_ID('plan_version_weeks'))
BEGIN
	ALTER TABLE plan_version_weeks ADD ratings FLOAT NULL
	
	EXEC('UPDATe w SET
			Ratings = v.target_rating_points * (w.weekly_impressions_percentage / 100.0)
		FROM plan_version_weeks w
		INNER JOIN plan_versions v
			ON w.plan_version_id = v.id
		WHERE COALESCE(w.ratings, 0) = 0')
		  
    ALTER TABLE plan_version_weeks ALTER COLUMN ratings FLOAT NOT NULL
END

GO

/*************************************** END - PRI-19008 ******************************************************/


/*************************************** START - PRI-19058 ****************************************************/
IF OBJECT_ID('plan_version_daypart_affiliate_restrictions') IS NULL
BEGIN
	CREATE TABLE [dbo].[plan_version_daypart_affiliate_restrictions](		
		[id] [int] IDENTITY(1,1) NOT NULL,
		[plan_version_daypart_id] [int] NOT NULL,
		[affiliate_id] [int] NOT NULL,
	 CONSTRAINT [PK_plan_version_daypart_affiliate_restrictions] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[plan_version_daypart_affiliate_restrictions] WITH CHECK ADD CONSTRAINT [FK_plan_version_daypart_affiliate_restrictions_plan_version_dayparts] FOREIGN KEY([plan_version_daypart_id])
	REFERENCES [dbo].[plan_version_dayparts] ([id])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[plan_version_daypart_affiliate_restrictions] CHECK CONSTRAINT [FK_plan_version_daypart_affiliate_restrictions_plan_version_dayparts]

	ALTER TABLE [dbo].[plan_version_daypart_affiliate_restrictions] WITH CHECK ADD CONSTRAINT [FK_plan_version_daypart_affiliate_restrictions_affiliates] FOREIGN KEY([affiliate_id])
	REFERENCES [dbo].[affiliates] ([id])

	ALTER TABLE [dbo].[plan_version_daypart_affiliate_restrictions] CHECK CONSTRAINT [FK_plan_version_daypart_affiliate_restrictions_affiliates]
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_dayparts') AND name = 'affiliate_restrictions_contain_type')
BEGIN
	ALTER TABLE [plan_version_dayparts] ADD [affiliate_restrictions_contain_type] INT NULL
END
/*************************************** END - PRI-19058 ****************************************************/

/*************************************** START - PRI-18261 ****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'daypart_id' AND  object_id = OBJECT_ID('plan_version_pricing_api_result_spots'))
BEGIN
	ALTER TABLE plan_version_pricing_api_result_spots ADD daypart_id INT NULL
	
	EXEC('UPDATE plan_version_pricing_api_result_spots
		  SET daypart_id = (SELECT TOP 1 daypart_id FROM station_inventory_manifest_dayparts
							WHERE station_inventory_manifest_id = plan_version_pricing_api_result_spots.station_inventory_manifest_id)')
		  
    ALTER TABLE plan_version_pricing_api_result_spots ALTER COLUMN daypart_id INT NOT NULL
	
	ALTER TABLE [dbo].[plan_version_pricing_api_result_spots]  WITH CHECK ADD CONSTRAINT [FK_plan_version_pricing_api_result_spots_dayparts] FOREIGN KEY([daypart_id])
	REFERENCES [dbo].[dayparts] ([id])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[plan_version_pricing_api_result_spots] CHECK CONSTRAINT [FK_plan_version_pricing_api_result_spots_dayparts]
END

/*************************************** END - PRI-18261 ****************************************************/

/*************************************** END - PRI-19871 ****************************************************/
IF NOT EXISTS
	(SELECT 1 
	FROM sys.indexes 
	WHERE name='IX_station_inventory_manifest_audiences_manifest_id_audience_id_is_reference'
	AND object_id = OBJECT_ID('station_inventory_manifest_audiences'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_station_inventory_manifest_audiences_manifest_id_audience_id_is_reference] ON [dbo].[station_inventory_manifest_audiences]
	(
		[station_inventory_manifest_id] ASC,
		[audience_id] ASC,
		[is_reference] ASC
	)
	INCLUDE ([impressions]) 
END
GO
/*************************************** END - PRI-19871 ****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.01.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.12.1' -- Previous release version
		OR [version] = '20.01.1') -- Current release version
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