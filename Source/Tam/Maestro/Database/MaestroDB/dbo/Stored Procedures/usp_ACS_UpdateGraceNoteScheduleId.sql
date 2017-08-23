-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/23/2017
-- Modified:	4/17/2017 - Fixed bug using time zone of zone rather than time zone of zone/network feed.
-- Description:	Tries to set affidavits.gracenote_schedule_id of all affidavits in a media month WHERE gracenote_schedule_id is still NULL.
--				Loops in batches of 100K, uses TEMPDB as a staging area for performance purposes.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_UpdateGraceNoteScheduleId]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @days_in_month TABLE (date_of DATE NOT NULL);
	DECLARE @current_date DATE;
	DECLARE @start_date DATE;
	DECLARE @end_date DATE;
	DECLARE @textTimestamp VARCHAR(63);
	DECLARE @textRows VARCHAR(63);
	DECLARE @rows INT;
	DECLARE @numRemaining INT;

	SELECT @current_date=mm.start_date, @start_date=mm.start_date,@end_date=mm.end_Date FROM media_months mm WHERE mm.id=@media_month_id;

	WHILE @current_date <= @end_date
	BEGIN
		INSERT INTO @days_in_month (date_of) VALUES (@current_date);
		SET @current_date = DATEADD(day,1,@current_date);
	END

	DECLARE @gracenote_network_maps TABLE (date_of DATE NOT NULL, network_id INT NOT NULL, tz_time_zone_name VARCHAR(45) NOT NULL, tf_station_num INT NOT NULL, PRIMARY KEY CLUSTERED(date_of,network_id,tz_time_zone_name,tf_station_num));
	INSERT INTO @gracenote_network_maps
		SELECT
			dim.date_of,
			gnm.network_id,
			gnm.tz_time_zone_name,
			gnm.tf_station_num
		FROM
			gracenote_network_maps gnm
			CROSS APPLY @days_in_month dim
		WHERE
			(gnm.start_date<=dim.date_of AND (gnm.end_date>=dim.date_of OR gnm.end_date IS NULL));

	CREATE TABLE #schedules (tf_station_num INT NOT NULL, start_date DATETIME NOT NULL, end_date DATETIME NOT NULL, id INT NOT NULL, PRIMARY KEY CLUSTERED(tf_station_num,start_date,end_date))
	INSERT INTO #schedules
		SELECT
			s.tf_station_num,
			s.start_date,
			s.end_date,
			s.id
		FROM
			tri.schedules s (NOLOCK)
		WHERE
			s.start_date>=DATEADD(day,-1,@start_date) AND s.end_date<=DATEADD(day,1,@end_date)
		
	CREATE TABLE #staging (id BIGINT NOT NULL, gmt_air_datetime DATETIME NOT NULL, tf_station_num INT NOT NULL, gracenote_schedule_id INT, PRIMARY KEY CLUSTERED(id))
	INSERT INTO #staging
		SELECT
			a.id,
			a.gmt_air_datetime,
			gnm.tf_station_num,
			NULL
		FROM
			affidavits a
			JOIN uvw_zonenetwork_universe zn ON zn.zone_id=a.zone_id
				AND zn.network_id=a.network_id
				AND (zn.start_date<=a.air_date AND (zn.end_date>=a.air_date OR zn.end_date IS NULL))
			JOIN time_zones tz ON tz.id=
				CASE zn.feed_type
					WHEN 1 THEN 1 -- Eastern
					WHEN 2 THEN 3 -- Mountain
					WHEN 3 THEN 4 -- Pacific
				END
			JOIN @gracenote_network_maps gnm ON gnm.date_of=a.air_date 
				AND gnm.network_id=a.network_id
				AND gnm.tz_time_zone_name=
					CASE tz.name  -- translates maestro.dbo.time_zones to programs.tri.time_zones
						WHEN 'Alaskan' THEN 'Alaskan D.S.' 
						WHEN 'Central' THEN 'Central D.S.' 
						WHEN 'Eastern' THEN 'Eastern D.S.' 
						WHEN 'Hawaiian' THEN 'Hawaiian' 
						WHEN 'Mountain' THEN 'Mountain D.S.' 
						WHEN 'Pacific' THEN 'Pacific D.S.' 
					END
		WHERE
			a.media_month_id=@media_month_id
			AND a.gmt_air_datetime IS NOT NULL
			AND a.gracenote_schedule_id IS NULL;

	CREATE TABLE #batch (id BIGINT NOT NULL, gmt_air_datetime DATETIME NOT NULL, tf_station_num INT NOT NULL, gracenote_schedule_id INT, PRIMARY KEY CLUSTERED(id))
	WHILE 1 = 1
	BEGIN
		-- get the batch from the queue
		INSERT INTO #batch
			SELECT
				TOP(100000) id,gmt_air_datetime,tf_station_num,NULL
			FROM
				#staging

		--SET @rows = @@ROWCOUNT
		--SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		--SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		--RAISERROR('%s - %s Updating Batch...', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;

		-- update the batch
		UPDATE
			#batch
		SET
			gracenote_schedule_id = s.id
		FROM
			#batch b
			JOIN #schedules s ON s.tf_station_num=b.tf_station_num
				AND b.gmt_air_datetime BETWEEN s.start_date AND s.end_date

		--SET @rows = @@ROWCOUNT
		--SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@rows AS MONEY), 1))-3);
		--SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		--RAISERROR('%s - %s Updating Physical...', 0, 1, @textTimestamp, @textRows) WITH NOWAIT;

		-- update physical table from batch
		UPDATE
			affidavits
		SET
			gracenote_schedule_id = b.gracenote_schedule_id
		FROM
			affidavits 
			JOIN #batch b ON b.id=affidavits.id 
				AND affidavits.media_month_id=@media_month_id

		SET @rows = @@ROWCOUNT
		SELECT @numRemaining=COUNT(1)-@rows FROM #staging
		SET @textRows = LEFT(CONVERT(VARCHAR, CAST(@numRemaining AS MONEY), 1), LEN(CONVERT(VARCHAR, CAST(@numRemaining AS MONEY), 1))-3);
		SET @textTimestamp = CONVERT(VARCHAR, GETDATE(), 121);
		RAISERROR('%s - Processed %d, %s Remaining...', 0, 1, @textTimestamp, @rows, @textRows) WITH NOWAIT;

		-- remove batch from queue
		DELETE
			#staging
		FROM
			#staging s
			JOIN #batch b ON b.id=s.id

		TRUNCATE TABLE #batch;

		IF @numRemaining <= 0 BREAK
	END

	DROP TABLE #batch;
	DROP TABLE #staging;
	DROP TABLE #schedules;
END