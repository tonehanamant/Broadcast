IF object_id('[nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]
END

GO



-- =============================================
-- Author:		
-- Create date: 8/21/2017
-- Description:	
-- =============================================
/*
	DECLARE
		@hut_media_month_id SMALLINT = 416,
		@share_media_month_id SMALLINT = 422,
		@demo VARCHAR(MAX) = '6,7,21,22,284,348',
		@ratings_request RatingsInputWithId,
		@min_playback_type VARCHAR(1) = '3'

	INSERT INTO @ratings_request SELECT 1,'legacy_call_letters',1,1,1,1,1,0,0,0,79199
	INSERT INTO @ratings_request SELECT 2,'legacy_call_letters',1,1,1,1,1,0,0,0,32399
    INSERT INTO @ratings_request SELECT 4,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 5,'legacy_call_letters',1,1,1,1,1,1,1,12345,79199
    INSERT INTO @ratings_request SELECT 6,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 7,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 8,'legacy_call_letters',1,1,1,1,1,1,1,34634,79199
    INSERT INTO @ratings_request SELECT 9,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 10,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 11,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 12,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 13,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 14,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 15,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 16,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 17,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 18,'legacy_call_letters',1,1,1,1,1,1,1,0,79199
    INSERT INTO @ratings_request SELECT 19,'legacy_call_letters',1,1,1,1,1,1,1,0,79199

	EXEC [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages] @hut_media_month_id, @share_media_month_id, @demo, @ratings_request, @min_playback_type
*/
 --=============================================
CREATE PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]
	@hut_media_month_id SMALLINT,
	@share_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInputWithId READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	CREATE TABLE #rating_requests ([id] [int] NOT NULL,	[legacy_call_letters] varchar(15) NOT NULL,	[mon] [bit] NOT NULL,	[tue] [bit] NOT NULL,	[wed] [bit] NOT NULL,	[thu] [bit] NOT NULL,	[fri] [bit] NOT NULL,	[sat] [bit] NOT NULL,	[sun] [bit] NOT NULL,	[start_time] [int] NOT NULL,	[end_time] [int] NOT NULL,
		PRIMARY KEY CLUSTERED (	[id],	[legacy_call_letters],	[mon],	[tue],	[wed],	[thu],	[fri],	[sat],	[sun],	[start_time],	[end_time]))
	INSERT into #rating_requests 
		select * from @ratings_request 

	CREATE TABLE #audience_ids (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id));
	INSERT INTO #audience_ids
		SELECT DISTINCT id FROM dbo.SplitIntegers(@demo);

	CREATE TABLE #hut_market_codes (id INT NOT NULL, market_code SMALLINT NOT NULL, min_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(id, market_code, min_playback_type))
	INSERT INTO #hut_market_codes
		SELECT DISTINCT
			rr.id,
			v.market_code,
			mpt.available_playback_type
		FROM
			#rating_requests rr
			JOIN nsi.viewers v (NOLOCK) ON @hut_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN (
			SELECT * FROM nsi.udf_GetMinPlaybackTypes(@hut_media_month_id,@min_playback_type)
			) mpt ON mpt.market_code=v.market_code

	CREATE TABLE #usage_days (id INT NOT NULL, legacy_call_letters varchar(15) NOT NULL, start_time int NOT NULL, end_time int NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, usage FLOAT NOT NULL, 
		PRIMARY KEY(id, legacy_call_letters, start_time, end_time, market_code, audience_id))
	INSERT INTO #usage_days
		SELECT
			rr.id,rr.legacy_call_letters,u.start_time,u.end_time,hmc.market_code,a.audience_id,
			CASE rr.mon WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.tue WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.wed WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.thu WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.fri WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.sat WHEN 1 THEN u.weekend_usage ELSE 0 END+
			CASE rr.sun WHEN 1 THEN u.weekend_usage ELSE 0 END
		FROM
			#rating_requests rr
			JOIN #hut_market_codes hmc ON hmc.id=rr.id
			CROSS APPLY #audience_ids a
			JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@hut_media_month_id
				AND u.playback_type=hmc.min_playback_type
				AND u.audience_id=a.audience_id
				AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
				AND u.market_code=hmc.market_code

	CREATE TABLE #usage (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, usage FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #usage
		SELECT
			id,legacy_call_letters,u.start_time,u.end_time,
			SUM(u.usage) 'usage'
		FROM
			#usage_days u
		GROUP BY
			id,legacy_call_letters,start_time,end_time

	CREATE TABLE #share_market_codes(id INT NOT NULL, market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1) NOT NULL, 
		PRIMARY KEY CLUSTERED(id, market_code, available_playback_type))
	INSERT INTO #share_market_codes
		SELECT DISTINCT
			rr.id,
			v.market_code,
			mpt.available_playback_type
		FROM
			#rating_requests rr
			JOIN nsi.viewers v (NOLOCK) ON @share_media_month_id = v.media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN (
			SELECT * FROM nsi.udf_GetMinPlaybackTypes(@share_media_month_id,@min_playback_type)
			) mpt on mpt.market_code = v.market_code;

	CREATE TABLE #share_usage_days (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time int NOT NULL, end_time int NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, usage FLOAT NOT NULL, 
		PRIMARY KEY(id, legacy_call_letters, start_time, end_time, market_code, audience_id))
	INSERT INTO #share_usage_days
		SELECT
			rr.id,rr.legacy_call_letters,u.start_time,u.end_time,mc.market_code,a.audience_id,
			CASE rr.mon WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.tue WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.wed WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.thu WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.fri WHEN 1 THEN u.weekday_usage ELSE 0 END+
			CASE rr.sat WHEN 1 THEN u.weekend_usage ELSE 0 END+
			CASE rr.sun WHEN 1 THEN u.weekend_usage ELSE 0 END
		FROM
			#rating_requests rr
			JOIN #share_market_codes mc ON mc.id=rr.id
			CROSS APPLY #audience_ids a
			JOIN nsi.usages u (NOLOCK) ON u.media_month_id=@share_media_month_id
				AND u.playback_type=mc.available_playback_type
				AND u.audience_id=a.audience_id
				AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
				AND u.market_code=mc.market_code

	CREATE TABLE #share_usages (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, usage FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #share_usages
		SELECT
			id,legacy_call_letters,u.start_time,u.end_time,
			SUM(usage) 'usage'
		FROM
			#share_usage_days u
		GROUP BY
			id,legacy_call_letters,start_time,end_time

	CREATE TABLE #share_viewer_days (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time int NOT NULL, end_time int NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, viewers FLOAT NOT NULL, 
		PRIMARY KEY(id, legacy_call_letters, start_time, end_time, market_code, audience_id))
	INSERT INTO #share_viewer_days
		SELECT
			rr.id,rr.legacy_call_letters,v.start_time,v.end_time,mc.market_code,a.audience_id,
			CASE rr.mon WHEN 1 THEN vd.weekday_viewers ELSE 0 END+
			CASE rr.tue WHEN 1 THEN vd.weekday_viewers ELSE 0 END+
			CASE rr.wed WHEN 1 THEN vd.weekday_viewers ELSE 0 END+
			CASE rr.thu WHEN 1 THEN vd.weekday_viewers ELSE 0 END+
			CASE rr.fri WHEN 1 THEN vd.weekday_viewers ELSE 0 END+
			CASE rr.sat WHEN 1 THEN vd.weekend_viewers ELSE 0 END+
			CASE rr.sun WHEN 1 THEN vd.weekend_viewers ELSE 0 END
		FROM
			#rating_requests rr
			JOIN #share_market_codes mc ON mc.id=rr.id
			JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@share_media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
				AND v.market_code = mc.market_code
			CROSS APPLY #audience_ids a
			JOIN nsi.viewer_details vd (NOLOCK) ON 
				vd.media_month_id=v.media_month_id
				AND vd.viewer_id=v.id
				AND vd.audience_id=a.audience_id
				AND vd.playback_type=mc.available_playback_type

	CREATE TABLE #share_viewers (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, viewers FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #share_viewers
		SELECT
			id,legacy_call_letters,start_time,end_time,
			SUM(viewers)'viewers'
		FROM
			#share_viewer_days
		GROUP BY
			id,legacy_call_letters,start_time,end_time
	
	CREATE TABLE #share(id INT NOT NULL,[legacy_call_letters] varchar(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, share FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time));
	INSERT INTO #share
		SELECT 
			v.id,
			v.legacy_call_letters,
			v.start_time,
			v.end_time,
			CASE u.usage WHEN 0 THEN 0 ELSE v.viewers / u.usage END 'share'
		FROM 
			#share_viewers v
			JOIN #share_usages u ON 
				u.id = v.id
				AND u.legacy_call_letters = v.legacy_call_letters
				AND u.start_time = v.start_time
				AND u.end_time = v.end_time;

	SELECT
		h.id,h.legacy_call_letters,
		AVG(h.usage*s.share) 'impressions'
	FROM
		#usage h
		JOIN #share s ON 
			s.id = h.id
			AND s.legacy_call_letters=h.legacy_call_letters
			AND s.start_time=h.start_time
			AND s.end_time=h.end_time
		JOIN #rating_requests rr ON 
			h.id = rr.id 
			AND h.legacy_call_letters=rr.legacy_call_letters
	GROUP BY
		h.id, h.legacy_call_letters

	IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	IF OBJECT_ID('tempdb..#hut_market_codes') IS NOT NULL DROP TABLE #hut_market_codes;
	IF OBJECT_ID('tempdb..#usage_days') IS NOT NULL DROP TABLE #usage_days;
	IF OBJECT_ID('tempdb..#usage') IS NOT NULL DROP TABLE #usage;
	IF OBJECT_ID('tempdb..#share_market_codes') IS NOT NULL DROP TABLE #share_market_codes;
	IF OBJECT_ID('tempdb..#share_usage_days') IS NOT NULL DROP TABLE #share_usage_days;
	IF OBJECT_ID('tempdb..#share_usages') IS NOT NULL DROP TABLE #share_usages;
	IF OBJECT_ID('tempdb..#share_viewer_days') IS NOT NULL DROP TABLE #share_viewer_days;
	IF OBJECT_ID('tempdb..#share_viewers') IS NOT NULL DROP TABLE #share_viewers;
	IF OBJECT_ID('tempdb..#share') IS NOT NULL DROP TABLE #share;
END

GO


