IF object_id('[nsi].[usp_ForecastNsiRatingsMonth]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_ForecastNsiRatingsMonth]
END

GO
CREATE PROCEDURE [nsi].[usp_ForecastNsiRatingsMonth]
       @media_month_id SMALLINT
AS
BEGIN
       SET NOCOUNT ON;
       SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
    SET DATEFIRST 1; -- MON

       DECLARE @sample_type VARCHAR(1) = '1'
       DECLARE @reporting_service VARCHAR(1) = '1'
       DECLARE @distribution_source_type VARCHAR(1) = 'B'
       DECLARE @station_type_code_to_exclude TINYINT = 2
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


	CREATE TABLE #usages_averages (media_month_id SMALLINT NOT NULL, playback_type varchar(1) NOT NULL, audience_id INT NOT NULL, start_time int not null, end_time int not null, market_code smallint not null, week_part INT NOT NULL, usage FLOAT NOT NULL);
	INSERT INTO #usages_averages
              SELECT
                     mh.media_month_id,
                     mh.playback_type,
                     qhea.audience_id,
                     hhi.start_time,
                     hhi.end_time,
                     mh.market_code,
					CASE DATEPART(weekday, qhe.calendar_date)
						WHEN 1 THEN 0 WHEN 2 THEN 0 WHEN 3 THEN 0 WHEN 4 THEN 0 WHEN 5 THEN 0	-- weekday = 0
						WHEN 6 THEN 1 WHEN 7 THEN 1												-- weekend = 1
					END week_part,
					AVG(qhea.usage) usage
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
                     mh.market_code,
		     CASE DATEPART(weekday, qhe.calendar_date)
	      	     	WHEN 1 THEN 0 WHEN 2 THEN 0 WHEN 3 THEN 0 WHEN 4 THEN 0 WHEN 5 THEN 0	-- weekday = 0
   		        WHEN 6 THEN 1 WHEN 7 THEN 1												-- weekend = 1
		     END

		UPDATE 
			u
		SET 
			weekday_usage = usage
		FROM 
			nsi.usages u
		join 
			#usages_averages ua
		ON 
			u.media_month_id = ua.media_month_id
			and u.playback_type = ua.playback_type
			and u.audience_id = ua.audience_id
			and u.start_time = ua.start_time
			and u.end_time = ua.end_time
		WHERE 
			ua.week_part = 0;
		
		update 
			u
		SET 
			weekday_usage = usage
		FROM 
			nsi.usages u
		join 
			#usages_averages ua
		ON 
			u.media_month_id = ua.media_month_id
			and u.playback_type = ua.playback_type
			and u.audience_id = ua.audience_id
			and u.start_time = ua.start_time
			and u.end_time = ua.end_time
		WHERE 
			ua.week_part = 1;

       -- clear previous data if it exists
       SELECT TOP 1 @partition_number = $partition.[MediaMonthSmallIntPFN](media_month_id) FROM nsi.viewers (NOLOCK) WHERE media_month_id=@media_month_id
       IF @partition_number IS NOT NULL
       BEGIN
              ALTER TABLE nsi.viewers SWITCH PARTITION @partition_number TO nsi.viewers_trunc PARTITION @partition_number
              TRUNCATE TABLE nsi.viewers_trunc;
       END

       INSERT INTO nsi.viewers
		(media_month_id,legacy_call_letters,start_time,end_time,market_code)
              SELECT
                     mh.media_month_id,
                     dh.legacy_call_letters,
                     hhi.start_time,
                     hhi.end_time,
                     mh.market_code
              FROM
                     nsi.market_headers mh
                     JOIN nsi.distributor_headers dh ON dh.market_header_id=mh.id
                           AND dh.distribution_source_type=@distribution_source_type
                           AND dh.station_type_code <> @station_type_code_to_exclude
                     JOIN nsi.quarter_hour_distributor_estimates qhde ON qhde.market_header_id=mh.id
                           AND qhde.distributor_code=dh.distributor_code
                           AND qhde.sample_type=@sample_type
                     JOIN @half_hour_increments hhi ON qhde.calendar_time BETWEEN hhi.start_time AND hhi.end_time
              WHERE
                     mh.media_month_id=@media_month_id
                     AND mh.reporting_service=@reporting_service
              GROUP BY
                     mh.media_month_id,
                     dh.legacy_call_letters,
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
                           AND dh.legacy_call_letters=v.legacy_call_letters
                           AND dh.distribution_source_type=@distribution_source_type
                           AND dh.station_type_code <> @station_type_code_to_exclude
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

	CREATE TABLE #viewer_details_averages (media_month_id SMALLINT NOT NULL, viewer_id bigint NOT NULL, audience_id INT NOT NULL, playback_type VARCHAR(1) NOT NULL, week_part INT NOT NULL, viewers FLOAT NOT NULL);
	INSERT INTO #viewer_details_averages
	SELECT
		v.media_month_id,
		v.id,
		qhdea.audience_id,
		mh.playback_type,
		CASE DATEPART(weekday, qhde.calendar_date)
			WHEN 1 THEN 0 WHEN 2 THEN 0 WHEN 3 THEN 0 WHEN 4 THEN 0 WHEN 5 THEN 0	-- weekday = 0
			WHEN 6 THEN 1 WHEN 7 THEN 1												-- weekend = 1
		END week_part,
		AVG(qhdea.usage) viewers
	FROM
		nsi.viewers v
		JOIN nsi.market_headers mh ON mh.media_month_id=v.media_month_id
			AND mh.market_code=v.market_code
			AND mh.reporting_service=@reporting_service
		JOIN nsi.distributor_headers dh ON dh.market_header_id=mh.id
			AND dh.legacy_call_letters=v.legacy_call_letters
			AND dh.distribution_source_type=@distribution_source_type
			AND dh.station_type_code <> @station_type_code_to_exclude
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
		mh.playback_type,
			CASE DATEPART(weekday, qhde.calendar_date)
				WHEN 1 THEN 0 WHEN 2 THEN 0 WHEN 3 THEN 0 WHEN 4 THEN 0 WHEN 5 THEN 0	-- weekday = 0
				WHEN 6 THEN 1 WHEN 7 THEN 1												-- weekend = 1
			END

		update 
			vd
		SET 
			weekday_viewers = viewers
		FROM 
			nsi.viewer_details vd 
		join 
			#viewer_details_averages vda 
		ON 
			vda.media_month_id = vd.media_month_id
			and vda.audience_id = vd.audience_id
			and vda.viewer_id = vd.viewer_id
			and vda.playback_type = vd.playback_type
		WHERE 
			vda.week_part = 0;
		
		
		update 
			vd
		SET 
			weekend_viewers = viewers
		FROM 
			nsi.viewer_details vd 
		join 
			#viewer_details_averages vda 
		ON 
			vda.media_month_id = vd.media_month_id
			and vda.audience_id = vd.audience_id
			and vda.viewer_id = vd.viewer_id
			and vda.playback_type = vd.playback_type
		WHERE 
			vda.week_part = 1;

		DROP TABLE #viewer_details_averages;
		DROP TABLE #usages_averages;

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
GO