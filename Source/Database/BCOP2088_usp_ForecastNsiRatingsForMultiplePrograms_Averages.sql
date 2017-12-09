IF object_id('[nsi].[usp_ForecastNsiRatingsForMultiplePrograms_Averages]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_ForecastNsiRatingsForMultiplePrograms_Averages]
END

GO

/*
-- example exec
DECLARE
	@hut_media_month_id SMALLINT = 418,
	@share_media_month_id SMALLINT = 418,
	@demo VARCHAR(MAX) = '31',
	@ratings_request RatingsInput,
	@min_playback_type VARCHAR(1) = '1'

	
INSERT INTO @ratings_request SELECT 'WUNC',1,1,1,1,1,1,1,20000,66599


EXEC [nsi].[usp_ForecastNsiRatingsForMultiplePrograms_Averages]
		@hut_media_month_id,@share_media_month_id, @demo, @ratings_request, @min_playback_type
*/
CREATE PROCEDURE [nsi].[usp_ForecastNsiRatingsForMultiplePrograms_Averages]
	@hut_media_month_id SMALLINT,
	@share_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInput READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN

	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @audience_ids TABLE (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id ASC));
	INSERT INTO @audience_ids
		SELECT id FROM dbo.SplitIntegers(@demo);
	
	DECLARE @hut_min_playback_types TABLE (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(market_code ASC))
	INSERT INTO @hut_min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@hut_media_month_id,@min_playback_type);

	DECLARE @market_codes TABLE (legacy_call_letters varchar(15) NOT NULL, market_code INT NOT NULL, 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, market_code ASC))
	INSERT INTO @market_codes
		SELECT
			rr.legacy_call_letters,
			v.market_code
		FROM
			@ratings_request rr
			JOIN nsi.viewers v (NOLOCK) ON @hut_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
		GROUP BY
			rr.legacy_call_letters,
			v.market_code;

	DECLARE @share_market_codes TABLE (legacy_call_letters varchar(15) NOT NULL, market_code INT NOT NULL, 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, market_code ASC))
	INSERT INTO @share_market_codes
		SELECT
			rr.legacy_call_letters,
			v.market_code
		FROM
			@ratings_request rr
			JOIN nsi.viewers v (NOLOCK) ON @share_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
		GROUP BY
			rr.legacy_call_letters,
			v.market_code;

	DECLARE @hut TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, hut FLOAT NOT NULL 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @hut
		SELECT
			legacy_call_letters,mon,tue,wed,thu,fri,sat,sun,start_time,end_time,
			SUM(usage) / SUM(universe) 'hut'
		FROM (
			SELECT
				rr.legacy_call_letters,rr.mon,rr.tue,rr.wed,rr.thu,rr.fri,rr.sat,rr.sun,u.start_time,u.end_time,
				uv.universe * (
					CAST(rr.mon AS INT) +
					CAST(rr.tue AS INT) +
					CAST(rr.wed AS INT) +
					CAST(rr.thu AS INT) +
					CAST(rr.fri AS INT) +
					CAST(rr.sat AS INT) +
					CAST(rr.sun AS INT)) 'universe',
				CASE rr.mon WHEN 1 THEN u.weekday_usage ELSE 0 END +
				CASE rr.tue WHEN 1 THEN u.weekday_usage ELSE 0 END +
				CASE rr.wed WHEN 1 THEN u.weekday_usage ELSE 0 END +
				CASE rr.thu WHEN 1 THEN u.weekday_usage ELSE 0 END +
				CASE rr.fri WHEN 1 THEN u.weekday_usage ELSE 0 END +
				CASE rr.sat WHEN 1 THEN u.weekend_usage ELSE 0 END +
				CASE rr.sun WHEN 1 THEN u.weekend_usage ELSE 0 END 'usage'
			FROM
				@ratings_request rr
				JOIN @market_codes hdi ON hdi.legacy_call_letters=rr.legacy_call_letters
				JOIN @hut_min_playback_types mpt ON mpt.market_code=hdi.market_code
				CROSS APPLY @audience_ids a
				JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@hut_media_month_id
					AND u.playback_type=mpt.available_playback_type
					AND u.audience_id=a.audience_id
					AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
					AND u.market_code=hdi.market_code
				JOIN nsi.universes uv (NOLOCK) ON uv.media_month_id=u.media_month_id
					AND uv.playback_type=mpt.available_playback_type
					AND uv.market_code=u.market_code
					AND uv.audience_id=a.audience_id
		) tmp
		GROUP BY
			legacy_call_letters,
			mon,tue,wed,thu,fri,sat,sun,
			start_time,end_time

	DECLARE @share_min_playback_types TABLE (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(market_code ASC))
	INSERT INTO @share_min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@share_media_month_id,@min_playback_type);

	DECLARE @share_usages TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, usage FLOAT NOT NULL 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @share_usages
			SELECT
				rr.legacy_call_letters,rr.mon,rr.tue,rr.wed,rr.thu,rr.fri,rr.sat,rr.sun,u.start_time,u.end_time,
				SUM(CASE rr.mon WHEN 1 THEN u.weekday_usage ELSE 0 END +
				    CASE rr.tue WHEN 1 THEN u.weekday_usage ELSE 0 END +
				    CASE rr.wed WHEN 1 THEN u.weekday_usage ELSE 0 END +
				    CASE rr.thu WHEN 1 THEN u.weekday_usage ELSE 0 END +
				    CASE rr.fri WHEN 1 THEN u.weekday_usage ELSE 0 END +
				    CASE rr.sat WHEN 1 THEN u.weekend_usage ELSE 0 END +
				    CASE rr.sun WHEN 1 THEN u.weekend_usage ELSE 0 END) 'usage'
			FROM
				@ratings_request rr
				JOIN @share_market_codes hdi ON hdi.legacy_call_letters=rr.legacy_call_letters
				JOIN @share_min_playback_types mpt ON mpt.market_code=hdi.market_code
				CROSS APPLY @audience_ids a
				JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@share_media_month_id
					AND u.playback_type=mpt.available_playback_type
					AND u.audience_id=a.audience_id
					AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
					AND u.market_code=hdi.market_code
				JOIN nsi.universes uv (NOLOCK) ON uv.media_month_id=u.media_month_id
					AND uv.playback_type=mpt.available_playback_type
					AND uv.market_code=u.market_code
					AND uv.audience_id=a.audience_id
		GROUP BY
			rr.legacy_call_letters,
			mon,tue,wed,thu,fri,sat,sun,
			u.start_time,u.end_time

	DECLARE @share_viewers TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, viewers FLOAT NOT NULL 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @share_viewers
			SELECT
				rr.legacy_call_letters,rr.mon,rr.tue,rr.wed,rr.thu,rr.fri,rr.sat,rr.sun,v.start_time,v.end_time,
				SUM(CASE rr.mon WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
				    CASE rr.tue WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
				    CASE rr.wed WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
				    CASE rr.thu WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
				    CASE rr.fri WHEN 1 THEN vd.weekday_viewers ELSE 0 END +
				    CASE rr.sat WHEN 1 THEN vd.weekend_viewers ELSE 0 END +
				    CASE rr.sun WHEN 1 THEN vd.weekend_viewers ELSE 0 END) 'viewers'
			FROM
				@ratings_request rr
				JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@share_media_month_id
					AND v.legacy_call_letters=rr.legacy_call_letters
					AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
				JOIN @share_min_playback_types mpt ON mpt.market_code=v.market_code
				CROSS APPLY @audience_ids a
				JOIN nsi.viewer_details vd (NOLOCK) ON vd.media_month_id=v.media_month_id
					AND vd.viewer_id=v.id
					AND vd.audience_id=a.audience_id
					AND vd.playback_type=mpt.available_playback_type
		GROUP BY
			rr.legacy_call_letters,mon,tue,wed,thu,fri,sat,sun,v.start_time,v.end_time

	DECLARE @share TABLE (legacy_call_letters varchar(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, share FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(legacy_call_letters ASC, mon ASC, tue ASC, wed ASC, thu ASC, fri ASC, sat ASC, sun ASC, start_time ASC, end_time ASC));
	INSERT INTO @share
		SELECT 
			v.legacy_call_letters,
			v.mon,
			v.tue,
			v.wed,
			v.thu,
			v.fri,
			v.sat,
			v.sun,
			v.start_time,
			v.end_time,
			CASE u.usage WHEN 0 THEN 0 ELSE v.viewers / u.usage END 'share'
		FROM 
			@share_viewers v
			JOIN @share_usages u ON u.legacy_call_letters = v.legacy_call_letters
				AND u.mon = v.mon
				AND u.tue = v.tue
				AND u.wed = v.wed
				AND u.thu = v.thu
				AND u.fri = v.fri
				AND u.sat = v.sat
				AND u.sun = v.sun
				AND u.start_time = v.start_time
				AND u.end_time = v.end_time;

	SELECT
		h.legacy_call_letters,h.mon,h.tue,h.wed,h.thu,h.fri,h.sat,h.sun,rr.start_time,rr.end_time,
		AVG(h.hut*s.share) 'rating'
	FROM
		@hut h
		JOIN @share s ON s.legacy_call_letters=h.legacy_call_letters
			AND s.mon=h.mon
			AND s.tue=h.tue
			AND s.wed=h.wed
			AND s.thu=h.thu
			AND s.fri=h.fri
			AND s.sat=h.sat
			AND s.sun=h.sun
			AND s.start_time=h.start_time
			AND s.end_time=h.end_time
		JOIN @ratings_request rr ON h.legacy_call_letters=rr.legacy_call_letters
			AND h.mon=rr.mon
			AND h.tue=rr.tue
			AND h.wed=rr.wed
			AND h.thu=rr.thu
			AND h.fri=rr.fri
			AND h.sat=rr.sat
			AND h.sun=rr.sun
			AND (h.start_time<=rr.end_time AND h.end_time>=rr.start_time)			
	GROUP BY
		h.legacy_call_letters,h.mon,h.tue,h.wed,h.thu,h.fri,h.sat,h.sun,rr.start_time,rr.end_time
END


GO


