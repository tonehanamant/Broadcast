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

/*************************************** START - BP-1516 **************************************************/
--Stations
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_stations') 
				AND name = 'posting_type')
BEGIN

	ALTER TABLE [plan_version_pricing_stations] ADD [posting_type] INT NULL

	EXEC ('
		UPDATE
			pvps
		SET
			pvps.posting_type = COALESCE(pv.posting_type, 1)
		FROM 
			[plan_version_pricing_stations] pvps
			  LEFT JOIN [plan_version_pricing_job] pvpj
				ON	pvpj.id = pvps.plan_version_pricing_job_id
			  LEFT JOIN [plan_versions] pv
				ON pv.id = pvpj.plan_version_id')

	ALTER TABLE [plan_version_pricing_stations] ALTER COLUMN [posting_type] INT NOT NULL
END

--Markets
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_markets') 
				AND name = 'posting_type')
BEGIN
	ALTER TABLE [plan_version_pricing_markets] ADD [posting_type] INT NULL

	EXEC ('
		UPDATE
			pvpm
		SET
			pvpm.posting_type = COALESCE(pv.posting_type, 1)
		FROM 
			[plan_version_pricing_markets] pvpm
				LEFT JOIN [plan_version_pricing_job] pvpj
				ON	pvpj.id = pvpm.plan_version_pricing_job_id
				LEFT JOIN [plan_versions] pv
				ON pv.id = pvpj.plan_version_id')

	ALTER TABLE [plan_version_pricing_markets] ALTER COLUMN [posting_type] INT NOT NULL
END

--Programs
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('plan_version_pricing_results') 
				AND name = 'posting_type')
BEGIN
	DELETE FROM [plan_version_pricing_results] WHERE plan_Version_pricing_job_id IS NULL

	ALTER TABLE [plan_version_pricing_results] ADD [posting_type] INT NULL
	EXEC ('
		 UPDATE
			pvpr
		 SET
			pvpr.posting_type = COALESCE(pv.posting_type, 1)
		 FROM 
			[plan_version_pricing_results] pvpr
			  LEFT JOIN [plan_version_pricing_job] pvpj
				ON	pvpj.id = pvpr.plan_version_pricing_job_id
			  LEFT JOIN [plan_versions] pv
				ON pv.id = pvpj.plan_version_id')

	ALTER TABLE [plan_version_pricing_results] ALTER COLUMN [posting_type] INT NOT NULL
END

/*************************************** END - BP-1516 ****************************************************/

GO
/*************************************** START BP-1623 : Add Spot Lengths ****************************************************/

IF NOT EXISTS (SELECT 1 FROM spot_lengths WHERE [length] = 75)
BEGIN 
	/***  Add the new records ***/
	DECLARE @NewSpotLengths TABLE 
	(
		[length] INT,
		delivery_multiplier FLOAT,
		cost_multiplier FLOAT,	
		is_default BIT,
		spot_length_id INT,
		order_by INT,
		multiplier_id INT
	)

	DECLARE @ExistingOrderBy INT,
		@IsDefault BIT = 0

	SELECT @ExistingOrderBy = MAX(order_by) FROM spot_lengths

	INSERT INTO @NewSpotLengths ([length], delivery_multiplier, cost_multiplier, is_default) VALUES 
		(75, 2.5, 2.5, @IsDefault), -- NEW
		(5, 1.6666666666 , 1.6666666666 ,@IsDefault), -- NEW
		(10, 0.333333,  0.333333, @IsDefault) -- UPDATE

	UPDATE n SET
		spot_length_id = s.id,
		order_by = s.order_by
	FROM @NewSpotLengths n
	JOIN spot_lengths s
		ON n.[length] = s.[length]

	UPDATE @NewSpotLengths SET
		order_by = @ExistingOrderBy + 1
	WHERE order_by IS NULL
	AND [length] = 75

	SET @ExistingOrderBy = @ExistingOrderBy + 1

	UPDATE @NewSpotLengths SET
		order_by = @ExistingOrderBy + 1
	WHERE order_by IS NULL
	AND [length] = 5

	UPDATE n SET
		multiplier_id = m.id
	FROM @NewSpotLengths n
	JOIN spot_length_cost_multipliers m
		ON n.spot_length_id = m.spot_length_id

	UPDATE s SET
		delivery_multiplier = n.delivery_multiplier	
	FROM spot_lengths s
	JOIN @NewSpotLengths n
		ON n.spot_length_id = s.id

	UPDATE m SET
		cost_multiplier = n.cost_multiplier
	FROM spot_length_cost_multipliers m
	JOIN @NewSpotLengths n
		ON n.multiplier_id = m.id

	INSERT INTO spot_lengths ([length], delivery_multiplier, order_by, is_default) 
		SELECT [length], delivery_multiplier, order_by, @IsDefault
		FROM @NewSpotLengths
		WHERE spot_length_id IS NULL

	UPDATE n SET
		spot_length_id = s.id
	FROM @NewSpotLengths n
	JOIN spot_lengths s
		ON n.[length] = s.[length]
	WHERE n.spot_length_id IS NULL

	INSERT INTO spot_length_cost_multipliers (spot_length_id, cost_multiplier)
		SELECT spot_length_id, cost_multiplier
		FROM @NewSpotLengths
		WHERE multiplier_id IS NULL

	/*** Update the Open Market Inventory ***/
	-- this part took 1m20s in CD a few days after a Prod refresh
	-- delete what we're about to redo
	DELETE FROM station_inventory_manifest_rates WHERE id IN 
	(
		SELECT r.ID
		FROM station_inventory_manifest i
		JOIN station_inventory_manifest_rates r
			ON i.id = r.station_inventory_manifest_id
		WHERE i.inventory_source_id = 1 -- open market
		AND r.spot_length_id IN (SELECT id FROM spot_lengths WHERE [length] IN (10, 75, 5))
	) 

	INSERT INTO station_inventory_manifest_rates (station_inventory_manifest_id, spot_length_id, spot_cost)	
		SELECT b.inventory_id, s.spot_length_id
			, NewCost = ThirtySecondCost * cost_multiplier
		FROM 
		(
			SELECT i.id AS inventory_id, r.spot_cost AS ThirtySecondCost
			FROM station_inventory_manifest i
			JOIN station_inventory_manifest_rates r
				ON i.id = r.station_inventory_manifest_id
			WHERE i.inventory_source_id = 1 -- open market
			AND r.spot_length_id IN (SELECT id FROM spot_lengths WHERE [length] = 30)
		) b,
		(
			SELECT l.id AS spot_length_id, l.[length], m.cost_multiplier
			FROM spot_lengths l
			JOIN spot_length_cost_multipliers m
				ON l.id = m.spot_length_id
			WHERE [length] IN (10, 75, 5)
		) s

END 

GO
/*************************************** END - BP-1623 : Add Spot Lengths ****************************************************/


/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
	SET parameter_value = '20.11.1' -- Current release version
	, last_modified_time = SYSDATETIME()
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '20.10.1' -- Previous release version
		OR [version] = '20.11.1') -- Current release version
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