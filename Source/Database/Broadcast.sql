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

/*************************************** START PRI-20829 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'hangfire_job_id' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_job'))
BEGIN
	EXEC('ALTER TABLE [plan_version_pricing_job] ADD [hangfire_job_id] VARCHAR(16) NULL')
END
/*************************************** END PRI-20829 *****************************************************/

/*************************************** START PRI-24475 *****************************************************/
IF EXISTS(SELECT 1 FROM plan_versions WHERE target_audience_id = 31 AND target_universe <> hh_universe)
BEGIN
	UPDATE t1
	SET t1.target_universe = t2.universe
	FROM plan_versions as t1
	INNER JOIN nti_universes as t2 ON t1.target_audience_id = t2.audience_id
END
/*************************************** END PRI-24475 *****************************************************/

/*************************************** START PRI-19661 *****************************************************/
IF EXISTS(SELECT 1 FROM sys.columns WHERE name = 'diagnostic_result' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_job'))
BEGIN
	ALTER TABLE [plan_version_pricing_job] ALTER COLUMN [diagnostic_result] nvarchar(max) NULL
END
/*************************************** END PRI-19661 *****************************************************/

/*************************************** START PRI-25505 *****************************************************/

GO

IF EXISTS(SELECT * FROM inventory_file_proprietary_header
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE inventory_file_proprietary_header
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM inventory_summary_quarter_details
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE inventory_summary_quarter_details
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM plan_version_dayparts
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE plan_version_dayparts
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM scx_generation_job_files
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE scx_generation_job_files
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM scx_generation_jobs
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE scx_generation_jobs
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM station_inventory_manifest_dayparts
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	UPDATE station_inventory_manifest_dayparts
	SET daypart_default_id = (SELECT TOP 1 id FROM daypart_defaults WHERE code = 'TDN')
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT * FROM nti_to_nsi_conversion_rates
		  WHERE daypart_default_id IN (SELECT id FROM daypart_defaults
									   WHERE code = 'ROSN'))
BEGIN
	DELETE FROM nti_to_nsi_conversion_rates
	WHERE daypart_default_id IN (SELECT id FROM daypart_defaults WHERE code = 'ROSN')
END

GO

IF EXISTS(SELECT id FROM daypart_defaults  WHERE code = 'ROSN')
BEGIN
	DELETE FROM daypart_defaults
	WHERE id IN (SELECT id FROM daypart_defaults  WHERE code = 'ROSN')
END

GO

/*************************************** END PRI-25505 *******************************************************/

/*************************************** START PRI-24300 *******************************************************/
IF EXISTS(SELECT 1 FROM plan_version_pricing_parameters_inventory_source_type_percentages
				WHERE inventory_source_type NOT IN (1,2,3,4,5))
BEGIN
	DELETE FROM plan_version_pricing_parameters_inventory_source_type_percentages
	WHERE inventory_source_type NOT IN (1,2,3,4,5)
END
/*************************************** END PRI-24300 *******************************************************/

/*************************************** START PRI-25196 *****************************************************/
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'goal_fulfilled_by_proprietary' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_results'))
BEGIN
	ALTER TABLE [plan_version_pricing_results] ADD [goal_fulfilled_by_proprietary] [BIT] NULL
	
	EXEC('UPDATE [plan_version_pricing_results]
		  SET [goal_fulfilled_by_proprietary] = 0')
		  
    ALTER TABLE [plan_version_pricing_results]
	ALTER COLUMN [goal_fulfilled_by_proprietary] [BIT] NOT NULL
END
/*************************************** END PRI-25196 *****************************************************/

/*************************************** START ADDING Persons 2+ *******************************************************/
IF NOT EXISTS(SELECT 1 FROM audiences WHERE code = 'P2+')
BEGIN
	SET NOCOUNT ON

	BEGIN TRANSACTION
	insert into audiences(category_code, sub_category_code, range_start, range_end, custom, code, name)
	values(0, 'P', 2, 99, 1, 'P2+', 'Persons 2+')

	declare @audienceId int = SCOPE_IDENTITY()

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'C2-5'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'C6-11'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'W12-14'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'M12-14'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'W15-17'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'M15-17'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'W18-20'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'M18-20'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'W21-24'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'M21-24'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'W25-34'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'M25-34'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'W35-49'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'M35-49'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'W50-54'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'M50-54'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'W55-64'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'M55-64'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'W65+'))

	insert into audience_audiences(rating_category_group_id, custom_audience_id, rating_audience_id)
	values(2, @audienceId, (select top 1 id from audiences where code = 'M65+'))

	-- DATA VERIFICATION STEPS
	DECLARE @error BIT = 0

	IF (SELECT COUNT(1) FROM audiences WHERE category_code=0 AND sub_category_code='P' AND range_start=2 AND range_end=99 AND custom=1 AND code='P2+' AND name='Persons 2+')<>1
	BEGIN
		PRINT 'Invalid count of demo Persons 2+';
		SET @error = 1;
	END

	IF (SELECT COUNT(1) FROM audience_audiences WHERE custom_audience_id in (SELECT id FROM audiences WHERE code='P2+'))<>20
	BEGIN
		PRINT 'Invalid count of demo Persons 2+ components';
		SET @error = 1;
	END

	IF @error = 1
		ROLLBACK
	ELSE
		COMMIT
END
/*************************************** END ADDING Persons 2+ *******************************************************/

/*************************************** START PRI-20832 *****************************************************/

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'total_impressions' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_results'))
BEGIN
	ALTER TABLE plan_version_pricing_results
	ADD total_impressions FLOAT NULL

	EXEC('UPDATE plan_version_pricing_results
	SET total_impressions = 0
	WHERE total_impressions IS NULL')

	ALTER TABLE plan_version_pricing_results
	ALTER COLUMN total_impressions FLOAT NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'total_budget' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_results'))
BEGIN
	ALTER TABLE plan_version_pricing_results
	ADD total_budget MONEY NULL

	EXEC('UPDATE plan_version_pricing_results
	SET total_budget = 0
	WHERE total_budget IS NULL')

	ALTER TABLE plan_version_pricing_results
	ALTER COLUMN total_budget MONEY NOT NULL
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_pricing_job_id' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_parameters'))
BEGIN
	ALTER TABLE plan_version_pricing_parameters
	ADD plan_version_pricing_job_id INT NULL
	
	ALTER TABLE plan_version_pricing_parameters
	ADD CONSTRAINT FK_plan_version_pricing_parameters_plan_version_pricing_job FOREIGN KEY (plan_version_pricing_job_id) REFERENCES plan_version_pricing_job (id)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_pricing_job_id' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_executions'))
BEGIN
	ALTER TABLE plan_version_pricing_executions
	ADD plan_version_pricing_job_id INT NULL
	
	ALTER TABLE plan_version_pricing_executions
	ADD CONSTRAINT FK_plan_version_pricing_executions_plan_version_pricing_job FOREIGN KEY (plan_version_pricing_job_id) REFERENCES plan_version_pricing_job (id)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_pricing_job_id' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_api_results'))
BEGIN
	ALTER TABLE plan_version_pricing_api_results
	ADD plan_version_pricing_job_id INT NULL
	
	ALTER TABLE plan_version_pricing_api_results
	ADD CONSTRAINT FK_plan_version_pricing_api_results_plan_version_pricing_job FOREIGN KEY (plan_version_pricing_job_id) REFERENCES plan_version_pricing_job (id)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE name = 'plan_version_pricing_job_id' AND OBJECT_ID = OBJECT_ID('plan_version_pricing_results'))
BEGIN
	ALTER TABLE plan_version_pricing_results
	ADD plan_version_pricing_job_id INT NULL
	
	ALTER TABLE plan_version_pricing_results
	ADD CONSTRAINT FK_plan_version_pricing_results_plan_version_pricing_job FOREIGN KEY (plan_version_pricing_job_id) REFERENCES plan_version_pricing_job (id)

END
/*************************************** END PRI-20832 *****************************************************/

/*************************************** START PRI-25866 *****************************************************/
------------------------------ START P2+ ------------------------------
EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F2-5'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F6-8'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F9-11'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F12-14'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F15-17'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''F65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M2-5'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M6-8'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M9-11'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M12-14'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M15-17'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M18-20'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M21-24'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M25-29'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M30-34'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M35-39'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M40-44'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M45-49'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M50-54'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M55-64'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')

EXEC('
  declare @audience_id int = (SELECT TOP 1 id FROM audiences WHERE code = ''P2+'');
  declare @nti_audience_code varchar(15) = ''M65+'';

  IF NOT EXISTS(SELECT 1 FROM nti_universe_audience_mappings WHERE audience_id = @audience_id AND nti_audience_code = @nti_audience_code)
  BEGIN
      INSERT INTO nti_universe_audience_mappings(audience_id, nti_audience_code) values(@audience_id, @nti_audience_code)
  END')
------------------------------ END P2+ ------------------------------
/*************************************** END PRI-25866 *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '20.05.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.04.1' -- Previous release version
		OR [version] = '20.05.1') -- Current release version
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