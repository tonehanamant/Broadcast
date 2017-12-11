
IF object_id('[nsi].[usp_GetImpressionsForMultiplePrograms_Daypart]') IS NOT NULL
BEGIN
	DROP PROC [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart]
END

GO


---- =============================================
---- Author:		
---- Create date: 05/18/2017
---- Description:	
---- =============================================
/*
	DECLARE
		@posting_media_month_id SMALLINT = 410,
		@demo VARCHAR(MAX) = '13,14,15,347,348',
		@ratings_request RatingsInputWithId,
		@min_playback_type VARCHAR(1) = '3'

	INSERT INTO @ratings_request SELECT 1 ,'legacy_call_letters',1,1,1,1,1,0,0,77400,79199
	INSERT INTO @ratings_request SELECT 2 ,'legacy_call_letters',1,1,1,1,1,0,0,25200,32399
    INSERT INTO @ratings_request SELECT 4 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 5 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 6 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 7 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 8 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 9 ,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 10,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 11,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 12,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 13,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 14,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 15,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 16,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 17,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 18,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
    INSERT INTO @ratings_request SELECT 19,'legacy_call_letters',1,1,1,1,1,1,1,25200,79199
	
	EXEC [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart] @posting_media_month_id, @demo, @ratings_request, @min_playback_type
*/
--=============================================
CREATE PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart]
	@posting_media_month_id SMALLINT,
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
		SELECT id FROM dbo.SplitIntegers(@demo);

	CREATE TABLE #min_playback_types (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(market_code))
	INSERT INTO #min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@posting_media_month_id,@min_playback_type);

	CREATE TABLE #viewers (id INT NOT NULL, [legacy_call_letters] varchar(15) NOT NULL, audience_id INT NOT NULL, market_code SMALLINT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, mon_viewers FLOAT NOT NULL, tue_viewers FLOAT NOT NULL, wed_viewers FLOAT NOT NULL, thu_viewers FLOAT NOT NULL, fri_viewers FLOAT NOT NULL, sat_viewers FLOAT NOT NULL, sun_viewers FLOAT NOT NULL, 
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, audience_id, market_code, start_time, end_time))
	INSERT INTO #viewers
		SELECT
			rr.id, rr.legacy_call_letters,a.audience_id,v.market_code,v.start_time, v.end_time,
			CASE rr.mon WHEN 1 THEN vd.mon_viewers ELSE 0 END 'mon_viewers',
			CASE rr.tue WHEN 1 THEN vd.tue_viewers ELSE 0 END 'tue_viewers',
			CASE rr.wed WHEN 1 THEN vd.wed_viewers ELSE 0 END 'wed_viewers',
			CASE rr.thu WHEN 1 THEN vd.thu_viewers ELSE 0 END 'thu_viewers',
			CASE rr.fri WHEN 1 THEN vd.fri_viewers ELSE 0 END 'fri_viewers',
			CASE rr.sat WHEN 1 THEN vd.sat_viewers ELSE 0 END 'sat_viewers',
			CASE rr.sun WHEN 1 THEN vd.sun_viewers ELSE 0 END 'sun_viewers'
		FROM
			#rating_requests rr
			JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@posting_media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN #min_playback_types mpt ON mpt.market_code=v.market_code
			CROSS APPLY #audience_ids a
			JOIN nsi.viewer_details vd (NOLOCK) ON vd.media_month_id=v.media_month_id
				AND vd.viewer_id=v.id
				AND vd.audience_id=a.audience_id
				AND vd.playback_type=mpt.available_playback_type

	SELECT
		id,legacy_call_letters,audience_id,
		SUM(mon_viewers + tue_viewers + wed_viewers + thu_viewers + fri_viewers + sat_viewers + sun_viewers) 'impressions'
	FROM #viewers
	GROUP BY
		id,legacy_call_letters,audience_id

	IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	IF OBJECT_ID('tempdb..#min_playback_types') IS NOT NULL DROP TABLE #min_playback_types;
	IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	IF OBJECT_ID('tempdb..#viewers') IS NOT NULL DROP TABLE #viewers;
END

GO







