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

-- Only run this script when the schema is in the correct pervious version
INSERT INTO #previous_version
		SELECT parameter_value 
		FROM system_component_parameters 
		WHERE parameter_key = 'SchemaVersion' 


/*************************************** START UPDATE SCRIPT *****************************************************/

/*************************************** MISSED MOP #15027 FROM JULY 20th - START *****************************************************/

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [nsi].[usp_ForecastNsiRatingsMonth]
	@media_month_id SMALLINT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
    SET DATEFIRST 1; -- MON

	DECLARE @sample_type VARCHAR(1) = '1'
	DECLARE @reporting_service VARCHAR(1) = '1'
	DECLARE @distribution_source_type VARCHAR(1) = 'B'
	DECLARE @partition_number INT
	DECLARE @start_date DATE,
			@end_date DATE

	SELECT
		@start_date = MIN(start_date_of_survey), 
		@end_date = MAX(end_date_of_survey) 
	FROM 
		nsi.market_headers 
	WHERE
		media_month_id = @media_month_id

	DECLARE @days_in_month TABLE (calendar_date DATE, date_part TINYINT)
	WHILE @start_date <= @end_date
	BEGIN
		INSERT INTO @days_in_month SELECT @start_date, DATEPART(weekday, @start_date)
		SET @start_date = DATEADD(day,1,@start_date);
	END

	DECLARE @half_hour_increments TABLE (start_time INT NOT NULL, end_time INT NOT NULL)
	DECLARE @current_half_hour INT = 0
	WHILE @current_half_hour <> 86400
	BEGIN
		INSERT INTO @half_hour_increments SELECT @current_half_hour,@current_half_hour+899
		SET @current_half_hour = @current_half_hour + 900
	END

	-- clear previous data if it exists
	SELECT TOP 1 @partition_number = $partition.[MediaMonthSmallIntPFN](media_month_id) FROM nsi.usages (NOLOCK) WHERE media_month_id=@media_month_id
	IF @partition_number IS NOT NULL
	BEGIN
		ALTER TABLE nsi.usages SWITCH PARTITION @partition_number TO nsi.usages_trunc PARTITION @partition_number
		TRUNCATE TABLE nsi.usages_trunc;
	END

	INSERT INTO nsi.usages
		SELECT
			mh.media_month_id,
			mh.playback_type,
			qhea.audience_id,
			hhi.start_time,
			hhi.end_time,
			mh.market_code,
			ISNULL(AVG(CASE d.date_part WHEN 1 THEN qhea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 2 THEN qhea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 3 THEN qhea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 4 THEN qhea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 5 THEN qhea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 6 THEN qhea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 7 THEN qhea.usage ELSE NULL END), 0),
			0,
			0
		FROM
			nsi.market_headers mh
			JOIN nsi.quarter_hour_estimates qhe ON qhe.market_header_id=mh.id
				AND qhe.sample_type=@sample_type
			JOIN nsi.quarter_hour_estimate_audiences qhea ON qhea.quarter_hour_estimate_id=qhe.id
			JOIN @days_in_month d ON d.calendar_date=qhe.calendar_date
			JOIN @half_hour_increments hhi ON qhe.calendar_time BETWEEN hhi.start_time AND hhi.end_time
		WHERE
			mh.media_month_id=@media_month_id
			AND mh.reporting_service=@reporting_service
		GROUP BY
			mh.media_month_id,
			mh.playback_type,
			qhea.audience_id,
			hhi.start_time,
			hhi.end_time,
			mh.market_code

	UPDATE 
		nsi.usages 
	SET
		weekday_usage = (mon_usage+tue_usage+wed_usage+thu_usage+fri_usage)/5,
		weekend_usage = (sat_usage + sun_usage)/2
	WHERE
		media_month_id = @media_month_id;

	-- clear previous data if it exists
	SELECT TOP 1 @partition_number = $partition.[MediaMonthSmallIntPFN](media_month_id) FROM nsi.viewers (NOLOCK) WHERE media_month_id=@media_month_id
	IF @partition_number IS NOT NULL
	BEGIN
		ALTER TABLE nsi.viewers SWITCH PARTITION @partition_number TO nsi.viewers_trunc PARTITION @partition_number
		TRUNCATE TABLE nsi.viewers_trunc;
	END

	INSERT INTO nsi.viewers
		SELECT
			mh.media_month_id,
			dh.distributor_code,
			hhi.start_time,
			hhi.end_time,
			mh.market_code
		FROM
			nsi.market_headers mh
			JOIN nsi.distributor_headers dh ON dh.market_header_id=mh.id
				AND dh.distribution_source_type=@distribution_source_type
			JOIN nsi.quarter_hour_distributor_estimates qhde ON qhde.market_header_id=mh.id
				AND qhde.distributor_code=dh.distributor_code
				AND qhde.sample_type=@sample_type
			JOIN @half_hour_increments hhi ON qhde.calendar_time BETWEEN hhi.start_time AND hhi.end_time
		WHERE
			mh.media_month_id=@media_month_id
			AND mh.reporting_service=@reporting_service
		GROUP BY
			mh.media_month_id,
			dh.distributor_code,
			hhi.start_time,
			hhi.end_time,
			mh.market_code

	-- clear previous data if it exists
	SELECT TOP 1 @partition_number = $partition.[MediaMonthSmallIntPFN](media_month_id) FROM nsi.viewer_details (NOLOCK) WHERE media_month_id=@media_month_id
	IF @partition_number IS NOT NULL
	BEGIN
		ALTER TABLE nsi.viewer_details SWITCH PARTITION @partition_number TO nsi.viewer_details_trunc PARTITION @partition_number
		TRUNCATE TABLE nsi.viewer_details_trunc;
	END

	INSERT INTO nsi.viewer_details
		SELECT
			v.media_month_id,
			v.id,
			qhdea.audience_id,
			mh.playback_type,
			ISNULL(AVG(CASE d.date_part WHEN 1 THEN qhdea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 2 THEN qhdea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 3 THEN qhdea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 4 THEN qhdea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 5 THEN qhdea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 6 THEN qhdea.usage ELSE NULL END), 0),
			ISNULL(AVG(CASE d.date_part WHEN 7 THEN qhdea.usage ELSE NULL END), 0),
			0,
			0
		FROM
			nsi.viewers v
			JOIN nsi.market_headers mh ON mh.media_month_id=v.media_month_id
				AND mh.market_code=v.market_code
				AND mh.reporting_service=@reporting_service
			JOIN nsi.distributor_headers dh ON dh.market_header_id=mh.id
				AND dh.distributor_code=v.station_code
				AND dh.distribution_source_type=@distribution_source_type
			JOIN nsi.quarter_hour_distributor_estimates qhde ON qhde.market_header_id=mh.id
				AND qhde.distributor_code=dh.distributor_code
				AND qhde.sample_type=@sample_type
				AND qhde.calendar_time BETWEEN v.start_time AND v.end_time
			JOIN @days_in_month d ON d.calendar_date=qhde.calendar_date
			JOIN nsi.quarter_hour_distributor_estimate_audiences qhdea ON qhdea.market_header_id=mh.id
				AND qhde.id=qhdea.quarter_hour_distributor_estimate_id
		WHERE
			v.media_month_id=@media_month_id
		GROUP BY
			v.media_month_id,
			v.id,
			qhdea.audience_id,
			mh.playback_type

	UPDATE 
		nsi.viewer_details
	SET
		weekday_viewers = (mon_viewers+tue_viewers+wed_viewers+thu_viewers+fri_viewers) / 5,
		weekend_viewers = (sat_viewers + sun_viewers) / 2
	WHERE
		media_month_id = @media_month_id;

	-- clear previous data if it exists
	DELETE FROM nsi.universes WHERE media_month_id=@media_month_id;

	INSERT INTO nsi.universes
		SELECT
			mh.media_month_id,
			mh.playback_type,
			mh.market_code,
			mueha.audience_id,
			mueha.universe
		FROM
			nsi.market_headers mh
			JOIN nsi.market_universe_estimate_headers mueh ON mueh.market_header_id=mh.id
			JOIN nsi.market_universe_estimate_header_audiences mueha ON mueha.market_universe_estimate_header_id=mueh.id
		WHERE
			mh.media_month_id=@media_month_id
			AND mueh.sample_type=@sample_type
			AND mh.reporting_service=@reporting_service
END


/*************************************** MISSED MOP #15027 FROM JULY 20 - END *****************************************************/

/*************************************** END UPDATE SCRIPT *******************************************************/
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '5.8.10' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '5.8.9' -- Previous release version
		OR [version] = '5.8.10') -- Current release version
	BEGIN
		PRINT 'Database Successfully Updated'
		COMMIT TRANSACTION
		DROP TABLE #previous_version
	END
	ELSE
	BEGIN
		PRINT 'Incorrect Previous Database Version'
		ROLLBACK TRANSACTION
	END

END
GO

IF(XACT_STATE() = -1)
BEGIN
	ROLLBACK TRANSACTION
	PRINT 'Database Update Failed. Transaction rolled back.'
END
GO















