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

/*************************************** START PRI-5664 *****************************************************/
IF OBJECT_ID('inventory_source_logos', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.inventory_source_logos
	(  
		[id] int IDENTITY(1,1) NOT NULL,
		[inventory_source_id] int NOT NULL,
		[created_by] varchar(63) NOT NULL,
		[created_date] datetime NOT NULL,
		[file_name] varchar(127) NOT NULL,
		[file_content] varbinary(MAX) NOT NULL

		CONSTRAINT [PK_inventory_source_logos] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)
	)

	ALTER TABLE [dbo].[inventory_source_logos]  WITH CHECK ADD  CONSTRAINT [FK_inventory_source_logos_inventory_sources] FOREIGN KEY([inventory_source_id])
	REFERENCES [dbo].[inventory_sources] ([id])
	ALTER TABLE [dbo].[inventory_source_logos] CHECK CONSTRAINT [FK_inventory_source_logos_inventory_sources]

	CREATE NONCLUSTERED INDEX [IX_inventory_source_logos_inventory_source_id] ON [dbo].[inventory_source_logos] ([inventory_source_id])
END
/*************************************** END PRI-5664 *****************************************************/

/*************************************** START PRI-9110 Required indexes  *****************************************************/
IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_audience_ID' AND OBJECT_ID = object_id('schedule_detail_audiences'))
	DROP INDEX [IX_audience_ID] ON [schedule_detail_audiences]
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_schedule_detail_audiences_audience_id' AND OBJECT_ID = object_id('schedule_detail_audiences'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_schedule_detail_audiences_audience_id] ON [dbo].[schedule_detail_audiences]
	(
		[audience_id] ASC
	)
	 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_demo' AND OBJECT_ID = object_id('post_file_detail_impressions'))
	DROP INDEX [IX_demo] ON [post_file_detail_impressions]
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_post_file_detail_impressions_demo' AND OBJECT_ID = object_id('post_file_detail_impressions'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_post_file_detail_impressions_demo] ON [dbo].[post_file_detail_impressions]
	(
		[demo] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_audience_ID' AND OBJECT_ID = object_id('bvs_post_details'))
	DROP INDEX [IX_audience_ID] ON [bvs_post_details]
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_bvs_post_details_audience_id' AND OBJECT_ID = object_id('bvs_post_details'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_bvs_post_details_audience_id] ON [dbo].[bvs_post_details]
	(
		[audience_id] ASC
	)
	 WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_audience_ID' AND OBJECT_ID = object_id('affidavit_client_scrub_audiences'))
	DROP INDEX [IX_audience_ID] ON [affidavit_client_scrub_audiences]
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_affidavit_client_scrub_audiences_audience_id' AND OBJECT_ID = object_id('affidavit_client_scrub_audiences'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_affidavit_client_scrub_audiences_audience_id] ON [dbo].[affidavit_client_scrub_audiences]
	(
		[audience_id] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

/*************************************** END PRI-9110 Required indexes  *****************************************************/

/*************************************** START PRI-9110 Demo Cleanup Script *****************************************************/
--if there are cable specific demo configurations run the cleanup script
IF EXISTS(SELECT TOP 1 1 FROM audience_audiences WHERE rating_category_group_id <> 2)
BEGIN
	-- remove cable specific demo configurations since they're not needed/used in broadcast
	DELETE FROM audience_audiences WHERE rating_category_group_id<>2
	DELETE FROM audiences WHERE id IN (309,316,321,326,328,330)
	-- turn custom flag off for all NSI component demos
	UPDATE audiences SET [custom]=0 WHERE id IN (4,5,6,7,348,347,13,14,15,31,339,46,19,20,21,22,284,290,28,29,30,346)
	-- remap KIDS to CHILDREN
	UPDATE audiences SET sub_category_code='C', code='C2-5', name='Children 2-5' WHERE id=339 -- K2-5 to C2-5
	UPDATE audiences SET sub_category_code='C', code='C6-11', name='Children 6-11' WHERE id=46 -- K6-11 to C6-11
	-- remap FEMALE to WOMEN
	UPDATE audiences SET sub_category_code='W', code='W12-14', name='Women 12-14' WHERE id=4   -- F12-14 to W12-14
	UPDATE audiences SET sub_category_code='W', code='W15-17', name='Women 15-17' WHERE id=5   -- F15-17 to W15-17
	UPDATE audiences SET sub_category_code='W', code='W18-20', name='Women 18-20' WHERE id=6   -- F18-20 to W18-20
	UPDATE audiences SET sub_category_code='W', code='W21-24', name='Women 21-24' WHERE id=7   -- F21-24 to W21-24
	UPDATE audiences SET sub_category_code='W', code='W25-34', name='Women 25-34' WHERE id=348 -- F25-34 to W25-34
	UPDATE audiences SET sub_category_code='W', code='W35-49', name='Women 35-49' WHERE id=347 -- F35-49 to W35-49
	UPDATE audiences SET sub_category_code='W', code='W50-54', name='Women 50-54' WHERE id=13  -- F50-54 to W50-54
	UPDATE audiences SET sub_category_code='W', code='W55-64', name='Women 55-64' WHERE id=14  -- F55-64 to W55-64
	UPDATE audiences SET sub_category_code='W', code='W65+', name='Women 65+' WHERE id=15	   -- F65+ to W65+
	-- remap MALES to MEN
	UPDATE audiences SET name='Men 12-14' WHERE id=19  -- Males 12-14 to Men 12-14
	UPDATE audiences SET name='Men 15-17' WHERE id=20  -- Males 15-17 to Men 15-17
	UPDATE audiences SET name='Men 18-20' WHERE id=21  -- Males 18-20 to Men 18-20
	UPDATE audiences SET name='Men 21-24' WHERE id=22  -- Males 21-24 to Men 21-24
	UPDATE audiences SET name='Men 25-34' WHERE id=284 -- Males 25-34 to Men 25-34
	UPDATE audiences SET name='Men 35-49' WHERE id=290 -- Males 35-49 to Men 35-49
	UPDATE audiences SET name='Men 50-54' WHERE id=28  -- Males 50-54 to Men 50-54
	UPDATE audiences SET name='Men 55-64' WHERE id=29  -- Males 55-64 to Men 55-64
	UPDATE audiences SET name='Men 65+' WHERE id=30    -- Males 65+ to Men 65+

	UPDATE audiences SET name='Men 18+' WHERE id=277   -- Males 18+ to Men 18+
	UPDATE audiences SET name='Men 18-34' WHERE id=48  -- Males 18-34 to Men 18-34
	UPDATE audiences SET name='Men 18-49' WHERE id=49  -- Males 18-49 to Men 18-49
	UPDATE audiences SET name='Men 18-54' WHERE id=50  -- Males 18-54 to Men 18-54
	UPDATE audiences SET name='Men 21-49' WHERE id=280 -- Males 21-49 to Men 21-49
	UPDATE audiences SET name='Men 25+' WHERE id=288   -- Males 25+ to Men 25+
	UPDATE audiences SET name='Men 25-49' WHERE id=286 -- Males 25-49 to Men 25-49
	UPDATE audiences SET name='Men 25-54' WHERE id=51  -- Males 25-54 to Men 25-54
	UPDATE audiences SET name='Men 25-64' WHERE id=287 -- Males 25-64 to Men 25-64
	UPDATE audiences SET name='Men 35+' WHERE id=54    -- Males 35+ to Men 35+
	UPDATE audiences SET name='Men 35-54' WHERE id=52  -- Males 35-54 to Men 35-54
	UPDATE audiences SET name='Men 35-64' WHERE id=53  -- Males 35-64 to Men 35-64
	UPDATE audiences SET name='Men 50+' WHERE id=295    -- Males 50+ to Men 50+
	UPDATE audiences SET name='Men 55+' WHERE id=296    -- Males 55+ to Men 55+

	-- remap and delete
	-- F25-54 to W25-54
	-- remove F25-54 from [dbo].[post_file_detail_impressions] where both F25-54 and W25-54 were present
	DELETE FROM 
		[dbo].[post_file_detail_impressions] 
	WHERE 
		post_file_detail_id IN (
			SELECT post_file_detail_id FROM dbo.post_file_detail_impressions WHERE demo IN (415,58) GROUP BY post_file_detail_id HAVING COUNT(1)>1
		)
		AND demo=415;
	-- remove F25-54 from [dbo].[post_file_demos] where both F25-54 and W25-54 were present
	DELETE FROM 
		[dbo].[post_file_demos] 
	WHERE 
		post_file_id IN (
			SELECT post_file_id FROM post_file_demos WHERE demo IN (415,58) GROUP BY post_file_id HAVING COUNT(1)>1
		)
		AND demo=415;
	UPDATE [dbo].[affidavit_client_scrub_audiences] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[affidavit_file_detail_demographics] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[bvs_post_details] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[inventory_file_proprietary_header] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[nsi_component_audiences] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[nti_transmittals_audiences] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[post_file_detail_impressions] SET demo=58 WHERE demo=415
	UPDATE [dbo].[postlog_client_scrub_audiences] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[post_file_demos] SET demo=58 WHERE demo=415
	UPDATE [dbo].[postlog_file_detail_demographics] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[proposal_buy_file_detail_audiences] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[proposal_version_audiences] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[proposal_versions] SET guaranteed_audience_id=58 WHERE guaranteed_audience_id=415
	UPDATE [dbo].[schedule_audiences] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[schedule_detail_audiences] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[station_inventory_manifest_audiences] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[station_inventory_manifest_staging] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[station_inventory_spot_audiences] SET audience_id=58 WHERE audience_id=415
	UPDATE [dbo].[station_inventory_spot_snapshots] SET audience_id=58 WHERE audience_id=415
	DELETE FROM audience_audiences WHERE custom_audience_id=415
	DELETE FROM audiences WHERE id=415

	-- W18+
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=308 AND rating_audience_id=6)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,308,6)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=308 AND rating_audience_id=7)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,308,7)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=308 AND rating_audience_id=348)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,308,348)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=308 AND rating_audience_id=347)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,308,347)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=308 AND rating_audience_id=13)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,308,13)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=308 AND rating_audience_id=14)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,308,14)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=308 AND rating_audience_id=15)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,308,15)

	-- W18-54
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=306 AND rating_audience_id=6)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,306,6)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=306 AND rating_audience_id=7)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,306,7)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=306 AND rating_audience_id=348)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,306,348)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=306 AND rating_audience_id=347)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,306,347)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=306 AND rating_audience_id=13)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,306,13)

	-- W21-49
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=312 AND rating_audience_id=7)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,312,7)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=312 AND rating_audience_id=348)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,312,348)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=312 AND rating_audience_id=347)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,312,347)

	-- W25+
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=319 AND rating_audience_id=348)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,319,348)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=319 AND rating_audience_id=347)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,319,347)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=319 AND rating_audience_id=13)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,319,13)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=319 AND rating_audience_id=14)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,319,14)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=319 AND rating_audience_id=15)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,319,15)

	-- W25-64
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=318 AND rating_audience_id=348)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,318,348)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=318 AND rating_audience_id=347)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,318,347)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=318 AND rating_audience_id=13)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,318,13)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=318 AND rating_audience_id=14)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,318,14)

	-- M18-34
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=48 AND rating_audience_id=21)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,48,21)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=48 AND rating_audience_id=22)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,48,22)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=48 AND rating_audience_id=284)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,48,284)

	-- M18-49
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=49 AND rating_audience_id=21)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,49,21)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=49 AND rating_audience_id=22)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,49,22)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=49 AND rating_audience_id=284)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,49,284)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=49 AND rating_audience_id=290)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,49,290)

	-- M18-54
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=50 AND rating_audience_id=21)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,50,21)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=50 AND rating_audience_id=22)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,50,22)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=50 AND rating_audience_id=284)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,50,284)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=50 AND rating_audience_id=290)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,50,290)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=50 AND rating_audience_id=28)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,50,28)

	-- M21-49
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=280 AND rating_audience_id=22)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,280,22)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=280 AND rating_audience_id=284)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,280,284)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=280 AND rating_audience_id=290)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,280,290)

	-- M25+
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=288 AND rating_audience_id=284)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,288,284)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=288 AND rating_audience_id=290)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,288,290)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=288 AND rating_audience_id=28)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,288,28)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=288 AND rating_audience_id=29)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,288,29)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=288 AND rating_audience_id=30)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,288,30)

	-- M25-49
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=286 AND rating_audience_id=284)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,286,284)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=286 AND rating_audience_id=290)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,286,290)

	-- M25-54
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=51 AND rating_audience_id IN (284,290,28))<>3
	BEGIN
		DELETE FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=51;
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,51,284)
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,51,290)
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,51,28)
	END

	-- M25-64
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=287 AND rating_audience_id=284)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,287,284)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=287 AND rating_audience_id=290)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,287,290)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=287 AND rating_audience_id=28)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,287,28)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=287 AND rating_audience_id=29)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,287,29)

	-- M35-54
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=52 AND rating_audience_id IN (290,28))<>3
	BEGIN
		DELETE FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=52;
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,52,290)
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,52,28)
	END

	-- M35+
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=54 AND rating_audience_id=290)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,54,290)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=54 AND rating_audience_id=28)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,54,28)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=54 AND rating_audience_id=29)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,54,29)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=54 AND rating_audience_id=30)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,54,30)

	-- M50+
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=295 AND rating_audience_id=28)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,295,28)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=295 AND rating_audience_id=29)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,295,29)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=295 AND rating_audience_id=30)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,295,30)

	-- M55+
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=296 AND rating_audience_id=29)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,296,29)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=296 AND rating_audience_id=30)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,296,30)


	-- P12-14
	IF(SELECT COUNT(1) FROM audiences WHERE code='P12-14') = 0
	BEGIN
		DECLARE @p12_14_id INT
		INSERT INTO audiences ([category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) VALUES (0,'P',12,14,1,'P12-14','Persons 12-14')
		SET @p12_14_id = SCOPE_IDENTITY()
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,@p12_14_id,4)
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,@p12_14_id,19)
	END

	-- P15-17
	IF(SELECT COUNT(1) FROM audiences WHERE code='P15-17') = 0
	BEGIN
		DECLARE @p15_17_id INT
		INSERT INTO audiences ([category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) VALUES (0,'P',15,17,1,'P15-17','Persons 15-17')
		SET @p15_17_id = SCOPE_IDENTITY()
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,@p15_17_id,5)
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,@p15_17_id,20)
	END

	-- A18-20
	IF(SELECT COUNT(1) FROM audiences WHERE code='A18-20') = 0
	BEGIN
		DECLARE @a18_20_id INT
		INSERT INTO audiences ([category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) VALUES (0,'A',18,20,1,'A18-20','Adults 18-20')
		SET @a18_20_id = SCOPE_IDENTITY()
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,@a18_20_id,6)
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,@a18_20_id,21)
	END

	-- A21-24
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=249 AND rating_audience_id=7)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,249,7)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=249 AND rating_audience_id=22)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,249,22)

	-- A21-49
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=252 AND rating_audience_id=22)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,252,22)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=252 AND rating_audience_id=284)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,252,284)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=252 AND rating_audience_id=290)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,252,290)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=252 AND rating_audience_id=7)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,252,7)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=252 AND rating_audience_id=348)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,252,348)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=252 AND rating_audience_id=347)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,252,347)
		
	-- A25-34
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=35 AND rating_audience_id=348)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,35,348)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=35 AND rating_audience_id=284)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,35,284)

	-- A35-49
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=38 AND rating_audience_id=347)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,38,347)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=38 AND rating_audience_id=290)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,38,290)

	-- A50-54
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=264 AND rating_audience_id=13)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,264,13)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=264 AND rating_audience_id=28)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,264,28)

	-- A55-64
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=266 AND rating_audience_id=14)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,266,14)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=266 AND rating_audience_id=29)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,266,29)

	-- A65+
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=44 AND rating_audience_id=15)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,44,15)
	IF (SELECT COUNT(1) FROM audience_audiences WHERE rating_category_group_id=2 AND custom_audience_id=44 AND rating_audience_id=30)=0
		INSERT INTO audience_audiences (rating_category_group_id,custom_audience_id,rating_audience_id) VALUES (2,44,30)


	-- delete unused/invalid/out of scope for broadcast demos
	DECLARE @to_delete TABLE (audience_id INT NOT NULL)
	INSERT INTO @to_delete
		SELECT a.id FROM audiences a WHERE a.code NOT IN ('A18+','A18-20','A18-34','A18-49','A18-54','A21-24','A21-49','A25+','A25-34','A25-49','A25-54','A25-64','A35+','A35-49','A35-54','A35-64','A50+','A50-54','A55+','A55-64','A65+','C2-5','C6-11','HH','M12-14','M15-17','M18+','M18-20','M18-34','M18-49','M18-54','M21-24','M21-49','M25+','M25-34','M25-49','M25-54','M25-64','M35+','M35-49','M35-54','M35-64','M50+','M50-54','M55+','M55-64','M65+','P12-14','P15-17','W12-14','W15-17','W18+','W18-20','W18-34','W18-49','W18-54','W21-24','W21-49','W25+','W25-34','W25-49','W25-54','W25-64','W35+','W35-49','W35-54','W35-64','W50+','W50-54','W55+','W55-64','W65+','O0+')
	DELETE FROM audience_audiences WHERE custom_audience_id IN (SELECT audience_id FROM @to_delete)
	DELETE FROM audiences WHERE id IN (SELECT audience_id FROM @to_delete)

	-- DATA VERIFICATION STEPS
	DECLARE @error BIT = 0

	-- WOMEN
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=4 ANd a.category_code=0 AND a.sub_category_code='W' AND a.range_start=12 AND a.range_end=14 AND a.custom=0 AND a.code='W12-14' AND a.name='Women 12-14')=0
	BEGIN
		PRINT 'Invalid Component Demo W12-14';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=5 ANd a.category_code=0 AND a.sub_category_code='W' AND a.range_start=15 AND a.range_end=17 AND a.custom=0 AND a.code='W15-17' AND a.name='Women 15-17')=0
	BEGIN
		PRINT 'Invalid Component Demo W15-17';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=6 ANd a.category_code=0 AND a.sub_category_code='W' AND a.range_start=18 AND a.range_end=20 AND a.custom=0 AND a.code='W18-20' AND a.name='Women 18-20')=0
	BEGIN
		PRINT 'Invalid Component Demo W18-20';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=7 ANd a.category_code=0 AND a.sub_category_code='W' AND a.range_start=21 AND a.range_end=24 AND a.custom=0 AND a.code='W21-24' AND a.name='Women 21-24')=0
	BEGIN
		PRINT 'Invalid Component Demo W21-24';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=348 ANd a.category_code=0 AND a.sub_category_code='W' AND a.range_start=25 AND a.range_end=34 AND a.custom=0 AND a.code='W25-34' AND a.name='Women 25-34')=0
	BEGIN
		PRINT 'Invalid Component Demo W25-34';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=347 ANd a.category_code=0 AND a.sub_category_code='W' AND a.range_start=35 AND a.range_end=49 AND a.custom=0 AND a.code='W35-49' AND a.name='Women 35-49')=0
	BEGIN
		PRINT 'Invalid Component Demo W35-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=13 ANd a.category_code=0 AND a.sub_category_code='W' AND a.range_start=50 AND a.range_end=54 AND a.custom=0 AND a.code='W50-54' AND a.name='Women 50-54')=0
	BEGIN
		PRINT 'Invalid Component Demo W50-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=14 ANd a.category_code=0 AND a.sub_category_code='W' AND a.range_start=55 AND a.range_end=64 AND a.custom=0 AND a.code='W55-64' AND a.name='Women 55-64')=0
	BEGIN
		PRINT 'Invalid Component Demo W55-64';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=15 ANd a.category_code=0 AND a.sub_category_code='W' AND a.range_start=65 AND a.range_end=99 AND a.custom=0 AND a.code='W65+' AND a.name='Women 65+')=0
	BEGIN
		PRINT 'Invalid Component Demo W65+';
		SET @error = 1;
	END

	-- MEN
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=19 ANd a.category_code=0 AND a.sub_category_code='M' AND a.range_start=12 AND a.range_end=14 AND a.custom=0 AND a.code='M12-14' AND a.name='Men 12-14')=0
	BEGIN
		PRINT 'Invalid Component Demo M12-14';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=20 ANd a.category_code=0 AND a.sub_category_code='M' AND a.range_start=15 AND a.range_end=17 AND a.custom=0 AND a.code='M15-17' AND a.name='Men 15-17')=0
	BEGIN
		PRINT 'Invalid Component Demo M15-17';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=21 ANd a.category_code=0 AND a.sub_category_code='M' AND a.range_start=18 AND a.range_end=20 AND a.custom=0 AND a.code='M18-20' AND a.name='Men 18-20')=0
	BEGIN
		PRINT 'Invalid Component Demo M18-20';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=22 ANd a.category_code=0 AND a.sub_category_code='M' AND a.range_start=21 AND a.range_end=24 AND a.custom=0 AND a.code='M21-24' AND a.name='Men 21-24')=0
	BEGIN
		PRINT 'Invalid Component Demo M21-24';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=284 ANd a.category_code=0 AND a.sub_category_code='M' AND a.range_start=25 AND a.range_end=34 AND a.custom=0 AND a.code='M25-34' AND a.name='Men 25-34')=0
	BEGIN
		PRINT 'Invalid Component Demo M25-34';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=290 ANd a.category_code=0 AND a.sub_category_code='M' AND a.range_start=35 AND a.range_end=49 AND a.custom=0 AND a.code='M35-49' AND a.name='Men 35-49')=0
	BEGIN
		PRINT 'Invalid Component Demo M35-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=28 ANd a.category_code=0 AND a.sub_category_code='M' AND a.range_start=50 AND a.range_end=54 AND a.custom=0 AND a.code='M50-54' AND a.name='Men 50-54')=0
	BEGIN
		PRINT 'Invalid Component Demo M50-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=29 ANd a.category_code=0 AND a.sub_category_code='M' AND a.range_start=55 AND a.range_end=64 AND a.custom=0 AND a.code='M55-64' AND a.name='Men 55-64')=0
	BEGIN
		PRINT 'Invalid Component Demo M55-64';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=30 ANd a.category_code=0 AND a.sub_category_code='M' AND a.range_start=65 AND a.range_end=99 AND a.custom=0 AND a.code='M65+' AND a.name='Men 65+')=0
	BEGIN
		PRINT 'Invalid Component Demo M65+';
		SET @error = 1;
	END

	-- KIDS
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=339 ANd a.category_code=0 AND a.sub_category_code='C' AND a.range_start=2 AND a.range_end=5 AND a.custom=0 AND a.code='C2-5' AND a.name='Children 2-5')=0
	BEGIN
		PRINT 'Invalid Component Demo M12-14';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=46 ANd a.category_code=0 AND a.sub_category_code='C' AND a.range_start=6 AND a.range_end=11 AND a.custom=0 AND a.code='C6-11' AND a.name='Children 6-11')=0
	BEGIN
		PRINT 'Invalid Component Demo M15-17';
		SET @error = 1;
	END

	-- HH
	IF(SELECT COUNT(1) FROM audiences a WHERE a.id=31 ANd a.category_code=0 AND a.sub_category_code='H' AND a.range_start=0 AND a.range_end=99 AND a.custom=0 AND a.code='HH' AND a.name='House Holds')=0
	BEGIN
		PRINT 'Invalid Component Demo HH';
		SET @error = 1;
	END

	-- HIGH LEVEL CHECKS
	DECLARE @expected_number_of_available_demos INT = 72;
	IF (SELECT COUNT(1) FROM audiences a WHERE a.code IN ('A18+','A18-20','A18-34','A18-49','A18-54','A21-24','A21-49','A25+','A25-34','A25-49','A25-54','A25-64','A35+','A35-49','A35-54','A35-64','A50+','A50-54','A55+','A55-64','A65+','C2-5','C6-11','HH','M12-14','M15-17','M18+','M18-20','M18-34','M18-49','M18-54','M21-24','M21-49','M25+','M25-34','M25-49','M25-54','M25-64','M35+','M35-49','M35-54','M35-64','M50+','M50-54','M55+','M55-64','M65+','P12-14','P15-17','W12-14','W15-17','W18+','W18-20','W18-34','W18-49','W18-54','W21-24','W21-49','W25+','W25-34','W25-49','W25-54','W25-64','W35+','W35-49','W35-54','W35-64','W50+','W50-54','W55+','W55-64','W65+')) <> @expected_number_of_available_demos
	BEGIN
		PRINT 'Number of expected audiences in dbo.audiences is invalid.';
		SET @error = 1;
	END
	IF (SELECT COUNT(1) FROM audiences a JOIN audience_audiences aa ON aa.custom_audience_id=a.id WHERE a.code IN ('A18+','A18-20','A18-34','A18-49','A18-54','A21-24','A21-49','A25+','A25-34','A25-49','A25-54','A25-64','A35+','A35-49','A35-54','A35-64','A50+','A50-54','A55+','A55-64','A65+','C2-5','C6-11','HH','M12-14','M15-17','M18+','M18-20','M18-34','M18-54','M21-24','M21-49','M25+','M25-34','M25-49','M25-54','M25-64','M35+','M35-49','M35-54','M35-64','M50+','M50-54','M55+','M55-64','M65+','P12-14','P15-17','W12-14','W15-17','W18+','W18-20','W18-34','W18-49','W18-54','W21-24','W21-49','W25+','W25-34','W25-49','W25-54','W25-64','W35+','W35-49','W35-54','W35-64','W50+','W50-54','W55+','W55-64','W65+')) = @expected_number_of_available_demos
	BEGIN
		PRINT 'Number of expected audiences in dbo.audience_audiences is invalid.';
		SET @error = 1;
	END
	IF (SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND aa.rating_audience_id NOT IN (4,5,6,7,348,347,13,14,15,31,339,46,19,20,21,22,284,290,28,29,30)) > 0
	BEGIN
		PRINT 'A custom demo was defined wih a non-NSI component demo.';
		SET @error = 1;
	END


	-- DEMO SPECIFIC CHECKS
	-- A18-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=18 AND a.range_end=99 AND a.custom=1 AND a.code='A18+' AND a.name='Adults 18+')=0
	BEGIN
		PRINT 'Invalid Custom Demo A18+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A18+' AND ar.code IN ('W18-20','W21-24','W25-34','W35-49','W50-54','W55-64','W65+','M18-20','M21-24','M25-34','M35-49','M50-54','M55-64','M65+'))<>14
	BEGIN
		PRINT 'Invalid Custom Demo Components for A18+, Expected 14';
		SET @error = 1;
	END
	-- A18-20
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=18 AND a.range_end=20 AND a.custom=1 AND a.code='A18-20' AND a.name='Adults 18-20')=0
	BEGIN
		PRINT 'Invalid Custom Demo A18-20';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A18-20' AND ar.code IN ('W18-20','M18-20'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for A18-20, Expected 2';
		SET @error = 1;
	END
	-- A18-34
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=18 AND a.range_end=34 AND a.custom=1 AND a.code='A18-34' AND a.name='Adults 18-34')=0
	BEGIN
		PRINT 'Invalid Custom Demo A18-34';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A18-34' AND ar.code IN ('W18-20','W21-24','W25-34','M18-20','M21-24','M25-34'))<>6
	BEGIN
		PRINT 'Invalid Custom Demo Components for A18-34, Expected 6';
		SET @error = 1;
	END
	-- A18-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=18 AND a.range_end=49 AND a.custom=1 AND a.code='A18-49' AND a.name='Adults 18-49')=0
	BEGIN
		PRINT 'Invalid Custom Demo A18-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A18-49' AND ar.code IN ('W18-20','W21-24','W25-34','W35-49','M18-20','M21-24','M25-34','M35-49'))<>8
	BEGIN
		PRINT 'Invalid Custom Demo Components for A18-49, Expected 8';
		SET @error = 1;
	END
	-- A18-54
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=18 AND a.range_end=54 AND a.custom=1 AND a.code='A18-54' AND a.name='Adults 18-54')=0
	BEGIN
		PRINT 'Invalid Custom Demo A18-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A18-54' AND ar.code IN ('W18-20','W21-24','W25-34','W35-49','W50-54','M18-20','M21-24','M25-34','M35-49','M50-54'))<>10
	BEGIN
		PRINT 'Invalid Custom Demo Components for A18-54, Expected 10';
		SET @error = 1;
	END
	-- A21-24
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=21 AND a.range_end=24 AND a.custom=1 AND a.code='A21-24' AND a.name='Adults 21-24')=0
	BEGIN
		PRINT 'Invalid Custom Demo A21-24';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A21-24' AND ar.code IN ('W21-24','M21-24'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for A21-24, Expected 2';
		SET @error = 1;
	END

	-- A21-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=21 AND a.range_end=49 AND a.custom=1 AND a.code='A21-49' AND a.name='Adults 21-49')=0
	BEGIN
		PRINT 'Invalid Custom Demo A21-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A21-49' AND ar.code IN ('W21-24','W25-34','W35-49','M21-24','M25-34','M35-49'))<>6
	BEGIN
		PRINT 'Invalid Custom Demo Components for A21-49, Expected 6';
		SET @error = 1;
	END
	-- A25+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=25 AND a.range_end=99 AND a.custom=1 AND a.code='A25+' AND a.name='Adults 25+')=0
	BEGIN
		PRINT 'Invalid Custom Demo A25+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A25+' AND ar.code IN ('W25-34','W35-49','W50-54','W55-64','W65+','M25-34','M35-49','M50-54','M55-64','M65+'))<>10
	BEGIN
		PRINT 'Invalid Custom Demo Components for A25+, Expected 10';
		SET @error = 1;
	END
	-- A25-34
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=25 AND a.range_end=34 AND a.custom=1 AND a.code='A25-34' AND a.name='Adults 25-34')=0
	BEGIN
		PRINT 'Invalid Custom Demo A25-34';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A25-34' AND ar.code IN ('W25-34','M25-34'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for A25-34, Expected 2';
		SET @error = 1;
	END
	-- A25-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=25 AND a.range_end=49 AND a.custom=1 AND a.code='A25-49' AND a.name='Adults 25-49')=0
	BEGIN
		PRINT 'Invalid Custom Demo A25-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A25-49' AND ar.code IN ('W25-34','W35-49','M25-34','M35-49'))<>4
	BEGIN
		PRINT 'Invalid Custom Demo Components for A25-49, Expected 4';
		SET @error = 1;
	END
	-- A25-54
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=25 AND a.range_end=54 AND a.custom=1 AND a.code='A25-54' AND a.name='Adults 25-54')=0
	BEGIN
		PRINT 'Invalid Custom Demo A25-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A25-54' AND ar.code IN ('W25-34','W35-49','W50-54','M25-34','M35-49','M50-54'))<>6
	BEGIN
		PRINT 'Invalid Custom Demo Components for A25-54, Expected 6';
		SET @error = 1;
	END
	-- A25-64
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=25 AND a.range_end=64 AND a.custom=1 AND a.code='A25-64' AND a.name='Adults 25-64')=0
	BEGIN
		PRINT 'Invalid Custom Demo A25-64';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A25-64' AND ar.code IN ('W25-34','W35-49','W50-54','W55-64','M25-34','M35-49','M50-54','M55-64'))<>8
	BEGIN
		PRINT 'Invalid Custom Demo Components for A25-64, Expected 8';
		SET @error = 1;
	END
	-- A35+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=35 AND a.range_end=99 AND a.custom=1 AND a.code='A35+' AND a.name='Adults 35+')=0
	BEGIN
		PRINT 'Invalid Custom Demo A35+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A35+' AND ar.code IN ('W35-49','W50-54','W55-64','W65+','M35-49','M50-54','M55-64','M65+'))<>8
	BEGIN
		PRINT 'Invalid Custom Demo Components for A35+, Expected 8';
		SET @error = 1;
	END
	-- A35-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=35 AND a.range_end=49 AND a.custom=1 AND a.code='A35-49' AND a.name='Adults 35-49')=0
	BEGIN
		PRINT 'Invalid Custom Demo A35-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A35-49' AND ar.code IN ('W35-49','M35-49'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for A35-49, Expected 2';
		SET @error = 1;
	END
	-- A35-54
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=35 AND a.range_end=54 AND a.custom=1 AND a.code='A35-54' AND a.name='Adults 35-54')=0
	BEGIN
		PRINT 'Invalid Custom Demo A35-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A35-54' AND ar.code IN ('W35-49','W50-54','M35-49','M50-54'))<>4
	BEGIN
		PRINT 'Invalid Custom Demo Components for A35-54, Expected 4';
		SET @error = 1;
	END
	-- A35-64
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=35 AND a.range_end=64 AND a.custom=1 AND a.code='A35-64' AND a.name='Adults 35-64')=0
	BEGIN
		PRINT 'Invalid Custom Demo A35-64';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A35-64' AND ar.code IN ('W35-49','W50-54','W55-64','M35-49','M50-54','M55-64'))<>6
	BEGIN
		PRINT 'Invalid Custom Demo Components for A35-64, Expected 6';
		SET @error = 1;
	END
	-- A50+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=50 AND a.range_end=99 AND a.custom=1 AND a.code='A50+' AND a.name='Adults 50+')=0
	BEGIN
		PRINT 'Invalid Custom Demo A50+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A50+' AND ar.code IN ('W50-54','W55-64','W65+','M50-54','M55-64','M65+'))<>6
	BEGIN
		PRINT 'Invalid Custom Demo Components for A50+, Expected 6';
		SET @error = 1;
	END
	-- A50-54
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=50 AND a.range_end=54 AND a.custom=1 AND a.code='A50-54' AND a.name='Adults 50-54')=0
	BEGIN
		PRINT 'Invalid Custom Demo A50-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A50-54' AND ar.code IN ('W50-54','M50-54'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for A50-54, Expected 2';
		SET @error = 1;
	END
	-- A55+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=55 AND a.range_end=99 AND a.custom=1 AND a.code='A55+' AND a.name='Adults 55+')=0
	BEGIN
		PRINT 'Invalid Custom Demo A55+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A55+' AND ar.code IN ('W55-64','W65+','M55-64','M65+'))<>4
	BEGIN
		PRINT 'Invalid Custom Demo Components for A55+, Expected 4';
		SET @error = 1;
	END
	-- A55-64
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=55 AND a.range_end=64 AND a.custom=1 AND a.code='A55-64' AND a.name='Adults 55-64')=0
	BEGIN
		PRINT 'Invalid Custom Demo A55-64';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A55-64' AND ar.code IN ('W55-64','M55-64'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for A55-64, Expected 2';
		SET @error = 1;
	END
	-- A65+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='A' AND a.range_start=65 AND a.range_end=99 AND a.custom=1 AND a.code='A65+' AND a.name='Adults 65+')=0
	BEGIN
		PRINT 'Invalid Custom Demo A65+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='A65+' AND ar.code IN ('W65+','M65+'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for A65+, Expected 2';
		SET @error = 1;
	END
	-- M18+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=18 AND a.range_end=99 AND a.custom=1 AND a.code='M18+' AND a.name='Men 18+')=0
	BEGIN
		PRINT 'Invalid Custom Demo M18+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M18+' AND ar.code IN ('M18-20','M21-24','M25-34','M35-49','M50-54','M55-64','M65+'))<>7
	BEGIN
		PRINT 'Invalid Custom Demo Components for M18+, Expected 7';
		SET @error = 1;
	END
	-- M18-34
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=18 AND a.range_end=34 AND a.custom=1 AND a.code='M18-34' AND a.name='Men 18-34')=0
	BEGIN
		PRINT 'Invalid Custom Demo M18-34';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M18-34' AND ar.code IN ('M18-20','M21-24','M25-34'))<>3
	BEGIN
		PRINT 'Invalid Custom Demo Components for M18-34, Expected 3';
		SET @error = 1;
	END
	-- M18-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=18 AND a.range_end=49 AND a.custom=1 AND a.code='M18-49' AND a.name='Men 18-49')=0
	BEGIN
		PRINT 'Invalid Custom Demo M18-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M18-49' AND ar.code IN ('M18-20','M21-24','M25-34','M35-49'))<>4
	BEGIN
		PRINT 'Invalid Custom Demo Components for M18-49, Expected 4';
		SET @error = 1;
	END
	-- M18-54
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=18 AND a.range_end=54 AND a.custom=1 AND a.code='M18-54' AND a.name='Men 18-54')=0
	BEGIN
		PRINT 'Invalid Custom Demo M18-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M18-54' AND ar.code IN ('M18-20','M21-24','M25-34','M35-49','M50-54'))<>5
	BEGIN
		PRINT 'Invalid Custom Demo Components for M18-54, Expected 5';
		SET @error = 1;
	END
	-- M21-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=21 AND a.range_end=49 AND a.custom=1 AND a.code='M21-49' AND a.name='Men 21-49')=0
	BEGIN
		PRINT 'Invalid Custom Demo M21-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M21-49' AND ar.code IN ('M21-24','M25-34','M35-49'))<>3
	BEGIN
		PRINT 'Invalid Custom Demo Components for M21-49, Expected 7';
		SET @error = 1;
	END
	-- M25+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=25 AND a.range_end=99 AND a.custom=1 AND a.code='M25+' AND a.name='Men 25+')=0
	BEGIN
		PRINT 'Invalid Custom Demo M25+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M25+' AND ar.code IN ('M25-34','M35-49','M50-54','M55-64','M65+'))<>5
	BEGIN
		PRINT 'Invalid Custom Demo Components for M25+, Expected 5';
		SET @error = 1;
	END
	-- M25-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=25 AND a.range_end=49 AND a.custom=1 AND a.code='M25-49' AND a.name='Men 25-49')=0
	BEGIN
		PRINT 'Invalid Custom Demo M25-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M25-49' AND ar.code IN ('M25-34','M35-49'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for M25-49, Expected 2';
		SET @error = 1;
	END
	-- M25-54
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=25 AND a.range_end=54 AND a.custom=1 AND a.code='M25-54' AND a.name='Men 25-54')=0
	BEGIN
		PRINT 'Invalid Custom Demo M25-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M25-54' AND ar.code IN ('M25-34','M35-49','M50-54'))<>3
	BEGIN
		PRINT 'Invalid Custom Demo Components for M25-54, Expected 3';
		SET @error = 1;
	END
	-- M25-64
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=25 AND a.range_end=64 AND a.custom=1 AND a.code='M25-64' AND a.name='Men 25-64')=0
	BEGIN
		PRINT 'Invalid Custom Demo M25-64';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M25-64' AND ar.code IN ('M25-34','M35-49','M50-54','M55-64'))<>4
	BEGIN
		PRINT 'Invalid Custom Demo Components for M25-64, Expected 4';
		SET @error = 1;
	END
	-- M35+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=35 AND a.range_end=99 AND a.custom=1 AND a.code='M35+' AND a.name='Men 35+')=0
	BEGIN
		PRINT 'Invalid Custom Demo M35+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M35+' AND ar.code IN ('M35-49','M50-54','M55-64','M65+'))<>4
	BEGIN
		PRINT 'Invalid Custom Demo Components for M35+, Expected 4';
		SET @error = 1;
	END
	-- M35-54
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=35 AND a.range_end=54 AND a.custom=1 AND a.code='M35-54' AND a.name='Men 35-54')=0
	BEGIN
		PRINT 'Invalid Custom Demo M35-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M35-54' AND ar.code IN ('M35-49','M50-54'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for M35-54, Expected 2';
		SET @error = 1;
	END
	-- M35-64
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=35 AND a.range_end=64 AND a.custom=1 AND a.code='M35-64' AND a.name='Men 35-64')=0
	BEGIN
		PRINT 'Invalid Custom Demo M35-64';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M35-64' AND ar.code IN ('M35-49','M50-54','M55-64'))<>3
	BEGIN
		PRINT 'Invalid Custom Demo Components for M35-64, Expected 3';
		SET @error = 1;
	END
	-- M50+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=50 AND a.range_end=99 AND a.custom=1 AND a.code='M50+' AND a.name='Men 50+')=0
	BEGIN
		PRINT 'Invalid Custom Demo M50+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M50+' AND ar.code IN ('M50-54','M55-64','M65+'))<>3
	BEGIN
		PRINT 'Invalid Custom Demo Components for M50, Expected 3';
		SET @error = 1;
	END
	-- M55+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='M' AND a.range_start=55 AND a.range_end=99 AND a.custom=1 AND a.code='M55+' AND a.name='Men 55+')=0
	BEGIN
		PRINT 'Invalid Custom Demo M55+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='M55+' AND ar.code IN ('M55-64','M65+'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for M55, Expected 2';
		SET @error = 1;
	END
	-- P12-14
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='P' AND a.range_start=12 AND a.range_end=14 AND a.custom=1 AND a.code='P12-14' AND a.name='Persons 12-14')=0
	BEGIN
		PRINT 'Invalid Custom Demo P12-14';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='P12-14' AND ar.code IN ('W12-14','M12-14'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for P12-14, Expected 2';
		SET @error = 1;
	END
	-- P15-17
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='P' AND a.range_start=15 AND a.range_end=17 AND a.custom=1 AND a.code='P15-17' AND a.name='Persons 15-17')=0
	BEGIN
		PRINT 'Invalid Custom Demo P15-17';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='P15-17' AND ar.code IN ('W15-17','M15-17'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for P15-17, Expected 2';
		SET @error = 1;
	END
	-- W18+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=18 AND a.range_end=99 AND a.custom=1 AND a.code='W18+' AND a.name='Women 18+')=0
	BEGIN
		PRINT 'Invalid Custom Demo W18+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W18+' AND ar.code IN ('W18-20','W21-24','W25-34','W35-49','W50-54','W55-64','W65+'))<>7
	BEGIN
		PRINT 'Invalid Custom Demo Components for W18+, Expected 7';
		SET @error = 1;
	END
	-- W18-34
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=18 AND a.range_end=34 AND a.custom=1 AND a.code='W18-34' AND a.name='Women 18-34')=0
	BEGIN
		PRINT 'Invalid Custom Demo W18-34';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W18-34' AND ar.code IN ('W18-20','W21-24','W25-34'))<>3
	BEGIN
		PRINT 'Invalid Custom Demo Components for W18-34, Expected 3';
		SET @error = 1;
	END
	-- W18-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=18 AND a.range_end=49 AND a.custom=1 AND a.code='W18-49' AND a.name='Women 18-49')=0
	BEGIN
		PRINT 'Invalid Custom Demo W18-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W18-49' AND ar.code IN ('W18-20','W21-24','W25-34','W35-49'))<>4
	BEGIN
		PRINT 'Invalid Custom Demo Components for W18-49, Expected 4';
		SET @error = 1;
	END
	-- W18-54
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=18 AND a.range_end=54 AND a.custom=1 AND a.code='W18-54' AND a.name='Women 18-54')=0
	BEGIN
		PRINT 'Invalid Custom Demo W18-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W18-54' AND ar.code IN ('W18-20','W21-24','W25-34','W35-49','W50-54'))<>5
	BEGIN
		PRINT 'Invalid Custom Demo Components for W18-54, Expected 5';
		SET @error = 1;
	END
	-- W21-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=21 AND a.range_end=49 AND a.custom=1 AND a.code='W21-49' AND a.name='Women 21-49')=0
	BEGIN
		PRINT 'Invalid Custom Demo W21-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W21-49' AND ar.code IN ('W21-24','W25-34','W35-49'))<>3
	BEGIN
		PRINT 'Invalid Custom Demo Components for W21-49, Expected 7';
		SET @error = 1;
	END
	-- W25+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=25 AND a.range_end=99 AND a.custom=1 AND a.code='W25+' AND a.name='Women 25+')=0
	BEGIN
		PRINT 'Invalid Custom Demo W25+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W25+' AND ar.code IN ('W25-34','W35-49','W50-54','W55-64','W65+'))<>5
	BEGIN
		PRINT 'Invalid Custom Demo Components for W25+, Expected 5';
		SET @error = 1;
	END
	-- W25-49
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=25 AND a.range_end=49 AND a.custom=1 AND a.code='W25-49' AND a.name='Women 25-49')=0
	BEGIN
		PRINT 'Invalid Custom Demo W25-49';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W25-49' AND ar.code IN ('W25-34','W35-49'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for W25-49, Expected 2';
		SET @error = 1;
	END
	-- W25-54
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=25 AND a.range_end=54 AND a.custom=1 AND a.code='W25-54' AND a.name='Women 25-54')=0
	BEGIN
		PRINT 'Invalid Custom Demo W25-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W25-54' AND ar.code IN ('W25-34','W35-49','W50-54'))<>3
	BEGIN
		PRINT 'Invalid Custom Demo Components for W25-54, Expected 3';
		SET @error = 1;
	END
	-- W25-64
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=25 AND a.range_end=64 AND a.custom=1 AND a.code='W25-64' AND a.name='Women 25-64')=0
	BEGIN
		PRINT 'Invalid Custom Demo W25-64';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W25-64' AND ar.code IN ('W25-34','W35-49','W50-54','W55-64'))<>4
	BEGIN
		PRINT 'Invalid Custom Demo Components for W25-64, Expected 4';
		SET @error = 1;
	END
	-- W35+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=35 AND a.range_end=99 AND a.custom=1 AND a.code='W35+' AND a.name='Women 35+')=0
	BEGIN
		PRINT 'Invalid Custom Demo W35+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W35+' AND ar.code IN ('W35-49','W50-54','W55-64','W65+'))<>4
	BEGIN
		PRINT 'Invalid Custom Demo Components for W35+, Expected 4';
		SET @error = 1;
	END
	-- W35-54
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=35 AND a.range_end=54 AND a.custom=1 AND a.code='W35-54' AND a.name='Women 35-54')=0
	BEGIN
		PRINT 'Invalid Custom Demo W35-54';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W35-54' AND ar.code IN ('W35-49','W50-54'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for W35-54, Expected 2';
		SET @error = 1;
	END
	-- W35-64
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=35 AND a.range_end=64 AND a.custom=1 AND a.code='W35-64' AND a.name='Women 35-64')=0
	BEGIN
		PRINT 'Invalid Custom Demo W35-64';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W35-64' AND ar.code IN ('W35-49','W50-54','W55-64'))<>3
	BEGIN
		PRINT 'Invalid Custom Demo Components for W35-64, Expected 3';
		SET @error = 1;
	END
	-- W50+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=50 AND a.range_end=99 AND a.custom=1 AND a.code='W50+' AND a.name='Women 50+')=0
	BEGIN
		PRINT 'Invalid Custom Demo W50+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W50+' AND ar.code IN ('W50-54','W55-64','W65+'))<>3
	BEGIN
		PRINT 'Invalid Custom Demo Components for M50, Expected 3';
		SET @error = 1;
	END
	-- W55+
	IF(SELECT COUNT(1) FROM audiences a WHERE a.category_code=0 AND a.sub_category_code='W' AND a.range_start=55 AND a.range_end=99 AND a.custom=1 AND a.code='W55+' AND a.name='Women 55+')=0
	BEGIN
		PRINT 'Invalid Custom Demo W55+';
		SET @error = 1;
	END
	IF(SELECT COUNT(1) FROM audience_audiences aa JOIN audiences a ON a.id=aa.custom_audience_id JOIN audiences ar ON ar.id=aa.rating_audience_id WHERE aa.rating_category_group_id=2 AND a.code='W55+' AND ar.code IN ('W55-64','W65+'))<>2
	BEGIN
		PRINT 'Invalid Custom Demo Components for W55, Expected 2';
		SET @error = 1;
	END

	IF @error = 1
	BEGIN
		-- cause an error!
		-- this is a fake statement intended to cause an exception in order to rollback the whole job
		INSERT INTO audiences ([id], [category_code], [sub_category_code], [range_start], [range_end], [custom], [code], [name]) VALUES (4,0,'W',12,14,0,'W12-14','Women 12-14')
	END
END

/*************************************** END PRI-9110 Demo Cleanup Script *****************************************************/

/*************************************** START PRI-9110 *****************************************************/
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'audience_maps' AND type = 'U')
BEGIN	
	CREATE TABLE [dbo].[audience_maps](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[audience_id] [INT] NOT NULL,
		[map_value] [nvarchar](16) NOT NULL
	 CONSTRAINT [PK_audience_maps] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[audience_maps] WITH CHECK ADD CONSTRAINT [FK_audience_maps_audiences] FOREIGN KEY([audience_id])
	REFERENCES [dbo].[audiences] ([id])
	ALTER TABLE [dbo].[audience_maps] CHECK CONSTRAINT [FK_audience_maps_audiences]
	CREATE NONCLUSTERED INDEX [IX_audience_maps_audience_id]  ON [dbo].[audience_maps] ([audience_id] ASC)
	INCLUDE ([id], [map_value]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END

IF (SELECT COUNT(1) FROM audience_maps) = 0
BEGIN
	--map adult audiences
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A18+'), 'P18+')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A18-20'), 'P18-20')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A18-34'), 'P18-34')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A18-49'), 'P18-49')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A18-54'), 'P18-54')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A21-24'), 'P21-24')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A25+'), 'P25+')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A25-34'), 'P25-34')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A25-49'), 'P25-49')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A25-54'), 'P25-54')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A25-64'), 'P25-64')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A35+'), 'P35+')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A35-49'), 'P35-49')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A35-54'), 'P35-54')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A35-64'), 'P35-64')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A50+'), 'P50+')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A50-54'), 'P50-54')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A55+'), 'P55+')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A55-64'), 'P55-64')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A65+'), 'P65+')

	--map female audiences
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W12-14'), 'F12-14')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W15-17'), 'F15-17')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W18+'), 'F18+')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W18-20'), 'F18-20')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W18-34'), 'F18-34')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W18-49'), 'F18-49')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W18-54'), 'F18-54')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W21-24'), 'F21-24')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W25+'), 'F25+')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W25-34'), 'F25-34')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W25-49'), 'F25-49')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W25-54'), 'F25-54')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W25-64'), 'F25-64')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W35+'), 'F35+')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W35-49'), 'F35-49')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W35-54'), 'F35-54')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W35-64'), 'F35-64')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W50+'), 'F50+')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W50-54'), 'F50-54')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W55+'), 'F55+')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W55-64'), 'F55-64')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'W50+'), 'F65+')

	--map kid audiences
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'C2-5'), 'K2-5')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'C6-11'), 'K6-11')
	
	--map audience names 
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'HH'), 'Homes0-99')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A18-49'), 'Adults18-49')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A25-54'), 'Adults25-54')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A35-54'), 'Adults35-54')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A35-64'), 'Adults35-64')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A50+'), 'Adults50-99')
	
	--map keeping trac audiences
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A35-64'), 'AD35-64')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A18-49'), 'AD18-49')
	INSERT INTO audience_maps(audience_id, map_value) VALUES ((SELECT id FROM audiences WHERE code = 'A25-54'), 'AD25-54')
END
/*************************************** END PRI-9110 *****************************************************/

/*************************************** END UPDATE SCRIPT *****************************************************/
-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.08.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.07.1' -- Previous release version
		OR [version] = '19.08.1') -- Current release version
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