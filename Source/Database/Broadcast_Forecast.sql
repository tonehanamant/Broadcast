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


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('nsi.usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages', 'P') IS NOT NULL
BEGIN
	DROP PROC nsi.usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages
END

GO
/*
	DECLARE
		@hut_media_month_id SMALLINT=434,
		@share_media_month_id SMALLINT=442,
		@demo VARCHAR(MAX)='6,7,347,348,13,14,15,21,22,28,29,30,284,290', --A18+
		@ratings_request RatingsInputWithId,
		@min_playback_type VARCHAR(1)='3'

	INSERT INTO @ratings_request SELECT 1,'WPIX',1,1,1,1,1,0,0,39600,43200-1--'M-F 11a-12n JERRY SPRINGER - 93',
	INSERT INTO @ratings_request SELECT 2,'WPIX',0,0,0,0,0,0,1,28800,32400-1--'Su 8a-9a PAID - 5',
	INSERT INTO @ratings_request SELECT 3,'WPHL',0,0,0,1,0,0,0,10800,14400-1--'Th 3a-4a CRME WATCH DLY - 14',
	INSERT INTO @ratings_request SELECT 4,'WPHL',1,1,1,1,1,0,0,68400,70200-1--'M-F 7p-7:30p big bang - 65',
	INSERT INTO @ratings_request SELECT 5,'KRON',0,0,0,0,0,0,1,75600,79200-1--'Su 9p-10p KRON 4-NGHT LV - 21',
	INSERT INTO @ratings_request SELECT 6,'KTLA',0,0,0,0,0,1,0,61200,63000-1--'Sa 5p-5:30p DOG WHISPER-CW - 18',
	INSERT INTO @ratings_request SELECT 7,'KTXL',1,1,1,1,0,0,0,84600,86400-1--'M-Th 11:30p-12m SEINFELD B< - 6',
	INSERT INTO @ratings_request SELECT 8,'WJW',1,1,1,0,0,0,0,82800,84600-1--'M-W 11p-11:30p BG BNG THRY B< - 55',
	INSERT INTO @ratings_request SELECT 9,'KASW',1,1,1,1,1,1,1,64800,68400-1--'M-Su 6p-7p VARIOUS	-21',
	INSERT INTO @ratings_request SELECT 10,'WKCF',0,0,0,0,0,1,1,68400,70200-1--'Sa-Su 7p-7:30p 2BRK GIRLS WK - 11',
	INSERT INTO @ratings_request SELECT 12,'KHOU',0,0,0,0,0,0,1,1800,5400-1
	INSERT INTO @ratings_request SELECT 13,'KHOU',0,0,0,0,0,0,1,5400,9000-1
	INSERT INTO @ratings_request SELECT 18,'WBNX',0,0,0,0,0,1,0,70200,72000-1
	INSERT INTO @ratings_request SELECT 19,'WBNX',0,0,0,0,0,1,0,82800,86400-1
	INSERT INTO @ratings_request SELECT 20,'KHOU',1,1,1,1,1,0,0,45000,46800-1
	INSERT INTO @ratings_request SELECT 21,'KHOU',1,1,1,1,1,0,0,66480,67200-1
	INSERT INTO @ratings_request SELECT 22,'KHOU',1,1,1,1,1,0,0,39600-2*60*60,43200-1
	INSERT INTO @ratings_request SELECT 23,'KHOU',1,1,1,1,1,0,0,39600,43200-1
	INSERT INTO @ratings_request SELECT 24,'WKCF',0,0,0,0,0,0,1,3600,7200-1
	INSERT INTO @ratings_request SELECT 25,'WBNX',1,0,0,0,0,0,0,3600,10800-1

	EXEC [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages] @hut_media_month_id, @share_media_month_id, @demo, @ratings_request, @min_playback_type
*/
CREATE PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]
	@hut_media_month_id SMALLINT,
	@share_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInputWithId READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN
	DECLARE @hut_media_month_id_sniff SMALLINT=@hut_media_month_id;
	DECLARE @share_media_month_id_sniff SMALLINT=@share_media_month_id;
	DECLARE @demo_sniff VARCHAR(MAX)=@demo;
	DECLARE @min_playback_type_sniff VARCHAR(1)=@min_playback_type;

	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	CREATE TABLE #rating_requests (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL,
		PRIMARY KEY CLUSTERED (id, legacy_call_letters, mon, tue, wed, thu, fri, sat, sun, start_time, end_time));
	INSERT into #rating_requests
		SELECT * FROM @ratings_request;

	CREATE TABLE #audience_ids (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id));
	INSERT INTO #audience_ids
		SELECT DISTINCT id FROM dbo.SplitIntegers(@demo_sniff);

	CREATE TABLE #hut_market_codes (id INT NOT NULL, market_code SMALLINT NOT NULL, playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(id, market_code, playback_type));
	INSERT INTO #hut_market_codes
		SELECT DISTINCT
			rr.id,
			v.market_code,
			mpt.available_playback_type
		FROM
			#rating_requests rr
			JOIN nsi.viewers v ON v.media_month_id=@hut_media_month_id_sniff
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN (
				SELECT * FROM nsi.udf_GetMinPlaybackTypes(@hut_media_month_id_sniff,@min_playback_type_sniff)
			) mpt ON mpt.market_code=v.market_code;

	CREATE TABLE #share_market_codes (id INT NOT NULL, market_code SMALLINT NOT NULL, playback_type VARCHAR(1) NOT NULL, 
		PRIMARY KEY CLUSTERED(id, market_code, playback_type));
	INSERT INTO #share_market_codes
		SELECT DISTINCT
			rr.id,
			v.market_code,
			mpt.available_playback_type
		FROM
			#rating_requests rr
			JOIN nsi.viewers v ON v.media_month_id=@share_media_month_id_sniff
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			JOIN (
				SELECT * FROM nsi.udf_GetMinPlaybackTypes(@share_media_month_id_sniff,@min_playback_type_sniff)
			) mpt on mpt.market_code=v.market_code;

	CREATE TABLE #hut_usage_days (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, playback_type VARCHAR(1) NOT NULL, weekly_avg FLOAT NOT NULL --,weekday_usage FLOAT NOT NULL ,weekend_usage FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time, market_code, audience_id));
	INSERT INTO #hut_usage_days
		SELECT
			rr.id,
			rr.legacy_call_letters,
			u.start_time,
			u.end_time,
			hmc.market_code,
			a.audience_id,
			hmc.playback_type,
			-- calc average FROM weekday values
			(
				CASE rr.mon WHEN 1 THEN u.mon_usage ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN u.tue_usage ELSE 0  END +
				CASE rr.wed WHEN 1 THEN u.wed_usage ELSE 0 END +
				CASE rr.thu WHEN 1 THEN u.thu_usage ELSE 0 END +
				CASE rr.fri WHEN 1 THEN u.fri_usage ELSE 0 END +
				CASE rr.sat WHEN 1 THEN u.sat_usage ELSE 0 END +
				CASE rr.sun WHEN 1 THEN u.sun_usage ELSE 0 END 
			) 
			/ 
			(
				CASE rr.mon WHEN 1 THEN 1 ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.wed WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.thu WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.fri WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sat WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sun WHEN 1 THEN 1 ELSE 0 END 
			) weekly_avg
		FROM
			#rating_requests rr
			JOIN #hut_market_codes hmc ON hmc.id=rr.id
			CROSS APPLY #audience_ids a
			JOIN nsi.usages u ON u.media_month_id=@hut_media_month_id_sniff
				AND u.playback_type=hmc.playback_type
				AND u.audience_id=a.audience_id
				AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
				AND u.market_code=hmc.market_code;
                
	CREATE TABLE #hut_univ (market_code SMALLINT NOT NULL, audience_id INT NOT NULL, playback_type VARCHAR(1) NOT NULL, universe FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(market_code,audience_id,playback_type));
	INSERT INTO #hut_univ
		SELECT DISTINCT
			u.market_code,
			u.audience_id,
			u.playback_type,
			u.universe as universe
		FROM 
			nsi.universes u
			JOIN #hut_market_codes hmc ON hmc.market_code=u.market_code
				AND u.playback_type=hmc.playback_type
			JOIN #audience_ids a on a.audience_id=u.audience_id 
		WHERE 
			u.media_month_id=@hut_media_month_id_sniff
		GROUP BY 
			u.market_code,u.audience_id,u.playback_type,u.universe;

	CREATE TABLE #hut (id INT NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, HUT FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(id,market_code,audience_id));
	INSERT INTO #hut
		SELECT 
			USAGE.id,
			univ.market_code, 
			univ.audience_id, 
			weekly_avg/universe HUT
		FROM (
			SELECT 
				d.id,
				d.legacy_call_letters, 
				d.market_code, 
				d.audience_id,
				AVG(weekly_avg) weekly_avg
			FROM 
				#hut_usage_days d
			GROUP BY 
				d.id, d.legacy_call_letters, d.market_code, d.audience_id
		) USAGE
		JOIN #hut_univ UNIV ON UNIV.audience_id=USAGE.audience_id 
			AND UNIV.market_code=USAGE.market_code;

	CREATE TABLE #share_usage_days (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, playback_type VARCHAR(1) NOT NULL, weekly_avg FLOAT NOT NULL --,weekday_usage FLOAT NOT NULL ,weekend_usage FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time, market_code, audience_id));
	INSERT INTO #share_usage_days
		SELECT
			rr.id,
			rr.legacy_call_letters,
			u.start_time,
			u.end_time,
			smc.market_code,
			a.audience_id,
			smc.playback_type,
			-- calc average FROM weekday values
			(
				CASE rr.mon WHEN 1 THEN u.mon_usage ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN u.tue_usage ELSE 0 END +
				CASE rr.wed WHEN 1 THEN u.wed_usage ELSE 0 END +
				CASE rr.thu WHEN 1 THEN u.thu_usage ELSE 0 END +
				CASE rr.fri WHEN 1 THEN u.fri_usage ELSE 0 END +
				CASE rr.sat WHEN 1 THEN u.sat_usage ELSE 0 END +
				CASE rr.sun WHEN 1 THEN u.sun_usage ELSE 0 END 
			) 
			/ 
			(
				CASE rr.mon WHEN 1 THEN 1 ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.wed WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.thu WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.fri WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sat WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sun WHEN 1 THEN 1 ELSE 0 END 
			) weekly_avg
		FROM
			#rating_requests rr
			JOIN #share_market_codes smc ON smc.id=rr.id
			CROSS APPLY #audience_ids a
			JOIN nsi.usages u ON u.media_month_id=@share_media_month_id_sniff
				AND u.playback_type=smc.playback_type
				AND u.audience_id=a.audience_id
				AND (u.start_time<=rr.end_time AND u.end_time>=rr.start_time)
				AND u.market_code=smc.market_code;
               
	CREATE TABLE #share_viewer_days (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, weekly_avg FLOAT NOT NULL --,weekday_usage FLOAT NOT NULL ,weekend_usage FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time, market_code, audience_id));
	INSERT INTO #share_viewer_days
		SELECT
			rr.id,
			rr.legacy_call_letters,
			v.start_time,
			v.end_time,
			mc.market_code,
			a.audience_id,
			-- calc average FROM weekday values
			(
				CASE rr.mon WHEN 1 THEN vd.mon_viewers ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN vd.tue_viewers ELSE 0 END +
				CASE rr.wed WHEN 1 THEN vd.wed_viewers ELSE 0 END +
				CASE rr.thu WHEN 1 THEN vd.thu_viewers ELSE 0 END +
				CASE rr.fri WHEN 1 THEN vd.fri_viewers ELSE 0 END +
				CASE rr.sat WHEN 1 THEN vd.sat_viewers ELSE 0 END +
				CASE rr.sun WHEN 1 THEN vd.sun_viewers ELSE 0 END 
			) 
			/ 
			(
				CASE rr.mon WHEN 1 THEN 1 ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.wed WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.thu WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.fri WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sat WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sun WHEN 1 THEN 1 ELSE 0 END 
			) weekly_avg
		FROM
		#rating_requests rr
		JOIN #share_market_codes mc ON mc.id=rr.id
		JOIN nsi.viewers v ON v.media_month_id=@share_media_month_id_sniff
			AND v.legacy_call_letters=rr.legacy_call_letters
			AND (v.start_time<=rr.end_time AND v.end_time>=rr.start_time)
			AND v.market_code=mc.market_code
		CROSS APPLY #audience_ids a
		JOIN nsi.viewer_details vd ON vd.media_month_id=v.media_month_id
			AND vd.viewer_id=v.id
			AND vd.audience_id=a.audience_id
			AND vd.playback_type=mc.playback_type;

	CREATE TABLE #share (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, SHARE FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(id,legacy_call_letters,market_code, audience_id));
	INSERT INTO #share
		SELECT 
			v.id,
			v.legacy_call_letters, 
			v.market_code,
			v.audience_id,
			case when AVG(u.weekly_avg) = 0 then 0 else AVG(v.weekly_avg) / AVG(u.weekly_avg) END as SHARE
			--AVG(v.weekly_avg) / AVG(u.weekly_avg) SHARE
		FROM 
			#share_viewer_days v
			JOIN #share_usage_days u ON v.id = u.id
				AND v.audience_id=u.audience_id
				AND v.start_time=u.start_time
				AND v.end_time=u.end_time
				AND v.market_code=u.market_code
				AND v.legacy_call_letters=u.legacy_call_letters
		GROUP BY 
			v.id,v.legacy_call_letters, v.market_code,v.audience_id;

	CREATE TABLE #pre_output (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, impressions_per_market FLOAT NOT NULL, universe FLOAT NOT NULL,
		PRIMARY KEY CLUSTERED(id,legacy_call_letters,market_code,audience_id));
	INSERT INTO #pre_output
		SELECT 
			s.id,
			s.legacy_call_letters,
			s.market_code,
			s.audience_id,
			SUM(h.hut) * SUM(s.share) * SUM(u.universe) 'impressions_per_market',
			SUM(u.universe) 'universes'
		FROM 
			#share s
			JOIN #hut h ON s.audience_id=h.audience_id
				AND s.id=h.id
				AND s.market_code=h.market_code
			JOIN #hut_univ u ON u.audience_id=h.audience_id 
				AND u.market_code=h.market_code 
		GROUP BY 
			s.id,s.legacy_call_letters,s.market_code,s.audience_id;

	CREATE TABLE #primary_market_universes (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, audience_id INT NOT NULL, universe FLOAT NOT NULL,
		PRIMARY KEY CLUSTERED(id,legacy_call_letters,audience_id));
	INSERT INTO #primary_market_universes
		SELECT
			po.id,
			po.legacy_call_letters,
			po.audience_id,
			MAX(po.universe)
		FROM 
			#pre_output po
		GROUP BY 
			po.id,po.legacy_call_letters,po.audience_id;

	-- zero out universe of bleed market      
	UPDATE
		#pre_output
	SET
		universe=0
	FROM
		#pre_output po
		JOIN #primary_market_universes pmu ON pmu.id=po.id
			AND pmu.legacy_call_letters=po.legacy_call_letters
			AND pmu.audience_id=po.audience_id
	WHERE
		po.universe<>pmu.universe; 

	SELECT
		po.id,
		po.legacy_call_letters,
		SUM(po.impressions_per_market) 'impressions',
		CASE WHEN SUM(po.universe)>0 THEN 
			SUM(po.impressions_per_market) / SUM(po.universe)
		ELSE 
			NULL 
		END 'rating'
	FROM
		#pre_output po
	GROUP BY 
		po.id,
		po.legacy_call_letters
	ORDER BY 
		po.id;

	IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;

	IF OBJECT_ID('tempdb..#hut_univ') IS NOT NULL DROP TABLE #hut_univ;
                
	IF OBJECT_ID('tempdb..#hut_market_codes') IS NOT NULL DROP TABLE #hut_market_codes;
	IF OBJECT_ID('tempdb..#share_market_codes') IS NOT NULL DROP TABLE #share_market_codes;
                
	IF OBJECT_ID('tempdb..#hut_usage_days') IS NOT NULL DROP TABLE #hut_usage_days;
	IF OBJECT_ID('tempdb..#share_usage_days') IS NOT NULL DROP TABLE #share_usage_days;
                
	IF OBJECT_ID('tempdb..#share_viewer_days') IS NOT NULL DROP TABLE #share_viewer_days;

	IF OBJECT_ID('tempdb..#hut') IS NOT NULL DROP TABLE #hut;
	IF OBJECT_ID('tempdb..#share') IS NOT NULL DROP TABLE #share;

	IF OBJECT_ID('tempdb..#pre_output') IS NOT NULL DROP TABLE #pre_output;
	IF OBJECT_ID('tempdb..#primary_market_universes') IS NOT NULL DROP TABLE #primary_market_universes;
END

GO


/*************************************** START PRI-5085 *****************************************************/

IF NOT (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'nsi' 
                 AND  TABLE_NAME = 'quarter_hours'))
BEGIN 

	CREATE TABLE [nsi].[quarter_hours](
		[start_time] [int] NOT NULL,
		[end_time] [int] NOT NULL,
	 CONSTRAINT [PK_nsi_quarter_hours] PRIMARY KEY CLUSTERED 
	(
		[start_time] ASC,
		[end_time] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


END

GO
if NOT EXISTS(select * from nsi.quarter_hours where start_time = 0 and end_time = 899)
BEGIN
	INSERT INTO nsi.quarter_hours SELECT 0,899
	INSERT INTO nsi.quarter_hours SELECT 900,1799
	INSERT INTO nsi.quarter_hours SELECT 1800,2699
	INSERT INTO nsi.quarter_hours SELECT 2700,3599
	INSERT INTO nsi.quarter_hours SELECT 3600,4499
	INSERT INTO nsi.quarter_hours SELECT 4500,5399
	INSERT INTO nsi.quarter_hours SELECT 5400,6299
	INSERT INTO nsi.quarter_hours SELECT 6300,7199
	INSERT INTO nsi.quarter_hours SELECT 7200,8099
	INSERT INTO nsi.quarter_hours SELECT 8100,8999
	INSERT INTO nsi.quarter_hours SELECT 9000,9899
	INSERT INTO nsi.quarter_hours SELECT 9900,10799
	INSERT INTO nsi.quarter_hours SELECT 10800,11699
	INSERT INTO nsi.quarter_hours SELECT 11700,12599
	INSERT INTO nsi.quarter_hours SELECT 12600,13499
	INSERT INTO nsi.quarter_hours SELECT 13500,14399
	INSERT INTO nsi.quarter_hours SELECT 14400,15299
	INSERT INTO nsi.quarter_hours SELECT 15300,16199
	INSERT INTO nsi.quarter_hours SELECT 16200,17099
	INSERT INTO nsi.quarter_hours SELECT 17100,17999
	INSERT INTO nsi.quarter_hours SELECT 18000,18899
	INSERT INTO nsi.quarter_hours SELECT 18900,19799
	INSERT INTO nsi.quarter_hours SELECT 19800,20699
	INSERT INTO nsi.quarter_hours SELECT 20700,21599
	INSERT INTO nsi.quarter_hours SELECT 21600,22499
	INSERT INTO nsi.quarter_hours SELECT 22500,23399
	INSERT INTO nsi.quarter_hours SELECT 23400,24299
	INSERT INTO nsi.quarter_hours SELECT 24300,25199
	INSERT INTO nsi.quarter_hours SELECT 25200,26099
	INSERT INTO nsi.quarter_hours SELECT 26100,26999
	INSERT INTO nsi.quarter_hours SELECT 27000,27899
	INSERT INTO nsi.quarter_hours SELECT 27900,28799
	INSERT INTO nsi.quarter_hours SELECT 28800,29699
	INSERT INTO nsi.quarter_hours SELECT 29700,30599
	INSERT INTO nsi.quarter_hours SELECT 30600,31499
	INSERT INTO nsi.quarter_hours SELECT 31500,32399
	INSERT INTO nsi.quarter_hours SELECT 32400,33299
	INSERT INTO nsi.quarter_hours SELECT 33300,34199
	INSERT INTO nsi.quarter_hours SELECT 34200,35099
	INSERT INTO nsi.quarter_hours SELECT 35100,35999
	INSERT INTO nsi.quarter_hours SELECT 36000,36899
	INSERT INTO nsi.quarter_hours SELECT 36900,37799
	INSERT INTO nsi.quarter_hours SELECT 37800,38699
	INSERT INTO nsi.quarter_hours SELECT 38700,39599
	INSERT INTO nsi.quarter_hours SELECT 39600,40499
	INSERT INTO nsi.quarter_hours SELECT 40500,41399
	INSERT INTO nsi.quarter_hours SELECT 41400,42299
	INSERT INTO nsi.quarter_hours SELECT 42300,43199
	INSERT INTO nsi.quarter_hours SELECT 43200,44099
	INSERT INTO nsi.quarter_hours SELECT 44100,44999
	INSERT INTO nsi.quarter_hours SELECT 45000,45899
	INSERT INTO nsi.quarter_hours SELECT 45900,46799
	INSERT INTO nsi.quarter_hours SELECT 46800,47699
	INSERT INTO nsi.quarter_hours SELECT 47700,48599
	INSERT INTO nsi.quarter_hours SELECT 48600,49499
	INSERT INTO nsi.quarter_hours SELECT 49500,50399
	INSERT INTO nsi.quarter_hours SELECT 50400,51299
	INSERT INTO nsi.quarter_hours SELECT 51300,52199
	INSERT INTO nsi.quarter_hours SELECT 52200,53099
	INSERT INTO nsi.quarter_hours SELECT 53100,53999
	INSERT INTO nsi.quarter_hours SELECT 54000,54899
	INSERT INTO nsi.quarter_hours SELECT 54900,55799
	INSERT INTO nsi.quarter_hours SELECT 55800,56699
	INSERT INTO nsi.quarter_hours SELECT 56700,57599
	INSERT INTO nsi.quarter_hours SELECT 57600,58499
	INSERT INTO nsi.quarter_hours SELECT 58500,59399
	INSERT INTO nsi.quarter_hours SELECT 59400,60299
	INSERT INTO nsi.quarter_hours SELECT 60300,61199
	INSERT INTO nsi.quarter_hours SELECT 61200,62099
	INSERT INTO nsi.quarter_hours SELECT 62100,62999
	INSERT INTO nsi.quarter_hours SELECT 63000,63899
	INSERT INTO nsi.quarter_hours SELECT 63900,64799
	INSERT INTO nsi.quarter_hours SELECT 64800,65699
	INSERT INTO nsi.quarter_hours SELECT 65700,66599
	INSERT INTO nsi.quarter_hours SELECT 66600,67499
	INSERT INTO nsi.quarter_hours SELECT 67500,68399
	INSERT INTO nsi.quarter_hours SELECT 68400,69299
	INSERT INTO nsi.quarter_hours SELECT 69300,70199
	INSERT INTO nsi.quarter_hours SELECT 70200,71099
	INSERT INTO nsi.quarter_hours SELECT 71100,71999
	INSERT INTO nsi.quarter_hours SELECT 72000,72899
	INSERT INTO nsi.quarter_hours SELECT 72900,73799
	INSERT INTO nsi.quarter_hours SELECT 73800,74699
	INSERT INTO nsi.quarter_hours SELECT 74700,75599
	INSERT INTO nsi.quarter_hours SELECT 75600,76499
	INSERT INTO nsi.quarter_hours SELECT 76500,77399
	INSERT INTO nsi.quarter_hours SELECT 77400,78299
	INSERT INTO nsi.quarter_hours SELECT 78300,79199
	INSERT INTO nsi.quarter_hours SELECT 79200,80099
	INSERT INTO nsi.quarter_hours SELECT 80100,80999
	INSERT INTO nsi.quarter_hours SELECT 81000,81899
	INSERT INTO nsi.quarter_hours SELECT 81900,82799
	INSERT INTO nsi.quarter_hours SELECT 82800,83699
	INSERT INTO nsi.quarter_hours SELECT 83700,84599
	INSERT INTO nsi.quarter_hours SELECT 84600,85499
	INSERT INTO nsi.quarter_hours SELECT 85500,86399
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
ALTER PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart]
	@posting_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInputWithId READONLY,
	@min_playback_type VARCHAR(1)
WITH RECOMPILE
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

	CREATE TABLE #times (id INT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, 
		PRIMARY KEY CLUSTERED (id,start_time,end_time));
	INSERT INTO #times
		SELECT
			rr.id,
			qh.start_time,
			qh.end_time
		FROM
			#rating_requests rr
			JOIN nsi.quarter_hours qh ON (qh.start_time<=rr.end_time AND qh.end_time>=rr.start_time);

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
			JOIN #times t ON t.id=rr.id
			JOIN nsi.viewers v (NOLOCK) ON v.media_month_id=@posting_media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND v.start_time=t.start_time
				AND v.end_time=t.end_time
			JOIN #min_playback_types mpt ON mpt.market_code=v.market_code
			CROSS APPLY #audience_ids a
			JOIN nsi.viewer_details vd (NOLOCK) ON vd.media_month_id=v.media_month_id
				AND vd.viewer_id=v.id
				AND vd.audience_id=a.audience_id
				AND vd.playback_type=mpt.available_playback_type;

	SELECT
		id,legacy_call_letters,audience_id,
		SUM(mon_viewers + tue_viewers + wed_viewers + thu_viewers + fri_viewers + sat_viewers + sun_viewers) 'impressions'
	FROM #viewers
	GROUP BY
		id,legacy_call_letters,audience_id;

	--IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	--IF OBJECT_ID('tempdb..#min_playback_types') IS NOT NULL DROP TABLE #min_playback_types;
	--IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	--IF OBJECT_ID('tempdb..#viewers') IS NOT NULL DROP TABLE #viewers;
	--IF OBJECT_ID('tempdb..#times') IS NOT NULL DROP TABLE #times;
END
GO



GO
/*
	DECLARE
		@posting_media_month_id SMALLINT=437,
		@demo VARCHAR(MAX)='6,7,347,348,13,14,15,21,22,28,29,30,284,290', --A18+
		@ratings_request RatingsInputWithId,
		@min_playback_type VARCHAR(1)='3'

	INSERT INTO @ratings_request SELECT 5,'WMAR',1,1,1,1,1,0,0,32400,35999
	INSERT INTO @ratings_request SELECT 6,'WMAR',1,1,1,1,1,0,0,36000,39599
	INSERT INTO @ratings_request SELECT 7,'WMAR',1,1,1,1,1,0,0,39600,43199
	INSERT INTO @ratings_request SELECT 71,'WESH',1,1,1,1,1,0,0,32400,35999
	INSERT INTO @ratings_request SELECT 74,'WESH',1,1,1,1,1,0,0,36000,39599
	INSERT INTO @ratings_request SELECT 77,'WESH',1,1,1,1,1,0,0,39600,43199
	INSERT INTO @ratings_request SELECT 89,'EESH',1,1,1,1,1,0,0,32400,35999
	INSERT INTO @ratings_request SELECT 90,'EESH',1,1,1,1,1,0,0,36000,39599
	INSERT INTO @ratings_request SELECT 91,'EESH',1,1,1,1,1,0,0,39600,43199
		
	EXEC [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages_Projections] @posting_media_month_id, @demo, @ratings_request, @min_playback_type
*/
ALTER PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages_Projections]
	@posting_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInputWithId READONLY,
	@min_playback_type VARCHAR(1)
WITH RECOMPILE
AS
BEGIN
	DECLARE @posting_media_month_id_sniff SMALLINT=@posting_media_month_id;
	DECLARE @min_playback_type_sniff VARCHAR(1)=@min_playback_type;
	DECLARE @demo_sniff VARCHAR(MAX)=@demo;

	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	CREATE TABLE #min_playback_types (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(market_code));
	INSERT INTO #min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@posting_media_month_id_sniff,@min_playback_type_sniff);

	CREATE TABLE #rating_requests (id int NOT NULL,	legacy_call_letters varchar(15) NOT NULL, mon bit NOT NULL, tue bit NOT NULL, wed bit NOT NULL, thu bit NOT NULL, fri bit NOT NULL, sat bit NOT NULL, sun bit NOT NULL, start_time int NOT NULL, end_time int NOT NULL, market_code SMALLINT NOT NULL, available_playback_type VARCHAR(3),
		PRIMARY KEY CLUSTERED (id,legacy_call_letters,mon,tue,wed,thu,fri,sat,sun,start_time,end_time,market_code,available_playback_type));
	INSERT into #rating_requests
		SELECT 
			rr.*,
			mccl.market_code,
			mpt.available_playback_type
		FROM 
			@ratings_request rr
			JOIN nsi.uvw_market_codes_call_letters mccl ON mccl.media_month_id=@posting_media_month_id_sniff
				AND mccl.legacy_call_letters=rr.legacy_call_letters
			JOIN #min_playback_types mpt ON mpt.market_code=mccl.market_code;

	CREATE TABLE #audience_ids (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id));
	INSERT INTO #audience_ids
		SELECT id FROM dbo.SplitIntegers(@demo_sniff);

	CREATE TABLE #times (id INT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, 
		PRIMARY KEY CLUSTERED (id,start_time,end_time));
	INSERT INTO #times
		SELECT
			rr.id,
			qh.start_time,
			qh.end_time
		FROM
			#rating_requests rr
			JOIN nsi.quarter_hours qh ON (qh.start_time<=rr.end_time AND qh.end_time>=rr.start_time)
		GROUP BY
			rr.id,
			qh.start_time,
			qh.end_time;

	CREATE TABLE #viewers (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, audience_id INT NOT NULL, market_code SMALLINT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, viewers FLOAT, universe FLOAT,  
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, audience_id, market_code, start_time, end_time));
	INSERT INTO #viewers
		SELECT
			rr.id, 
			rr.legacy_call_letters,
			a.audience_id,
			v.market_code,
			v.start_time, 
			v.end_time,
			(
				CASE rr.mon WHEN 1 THEN vd.mon_viewers ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN vd.tue_viewers ELSE 0 END +
				CASE rr.wed WHEN 1 THEN vd.wed_viewers ELSE 0 END +
				CASE rr.thu WHEN 1 THEN vd.thu_viewers ELSE 0 END +
				CASE rr.fri WHEN 1 THEN vd.fri_viewers ELSE 0 END +
				CASE rr.sat WHEN 1 THEN vd.sat_viewers ELSE 0 END +
				CASE rr.sun WHEN 1 THEN vd.sun_viewers ELSE 0 END 
			) 
			/ 
			(
				CASE rr.mon WHEN 1 THEN 1 ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.wed WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.thu WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.fri WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sat WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sun WHEN 1 THEN 1 ELSE 0 END 
			) 'viewers',
			u.universe
		FROM
			#rating_requests rr
			JOIN #times t ON t.id=rr.id
			JOIN nsi.viewers v ON v.media_month_id=@posting_media_month_id_sniff
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND v.market_code=rr.market_code
				AND v.start_time=t.start_time
				AND v.end_time=t.end_time
			CROSS APPLY #audience_ids a
			JOIN nsi.viewer_details vd ON vd.media_month_id=v.media_month_id
				AND vd.viewer_id=v.id
				AND vd.audience_id=a.audience_id
				AND vd.playback_type=rr.available_playback_type
			JOIN nsi.universes u ON u.media_month_id=v.media_month_id
				AND u.playback_type=rr.available_playback_type
				AND u.market_code=v.market_code
				AND u.audience_id=a.audience_id;

	CREATE TABLE #pre_output (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, impressions_per_market FLOAT NOT NULL, universe FLOAT NOT NULL,
		PRIMARY KEY CLUSTERED(id,legacy_call_letters,market_code,audience_id));
	INSERT INTO #pre_output
		-- get averages of impressions for each market (aggregating out start_time and end_time dimensions)
		SELECT 
			id,
			legacy_call_letters,
			market_code, 
			audience_id, 
			AVG(viewers) market_impression_avg,
			AVG(universe) market_universe_avg
		FROM 
			#viewers
		GROUP BY 
			id,legacy_call_letters,market_code,audience_id;

	CREATE TABLE #primary_market_universes (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, audience_id INT NOT NULL, universe FLOAT NOT NULL,
		PRIMARY KEY CLUSTERED(id,legacy_call_letters,audience_id));
	INSERT INTO #primary_market_universes
		SELECT
			po.id,
			po.legacy_call_letters,
			po.audience_id,
			MAX(po.universe)
		FROM 
			#pre_output po
		GROUP BY 
			po.id,po.legacy_call_letters,po.audience_id;

	-- zero out universe of bleed market      
	UPDATE
		#pre_output
	SET
		universe=0
	FROM
		#pre_output po
		JOIN #primary_market_universes pmu ON pmu.id=po.id
			AND pmu.legacy_call_letters=po.legacy_call_letters
			AND pmu.audience_id=po.audience_id
	WHERE
		po.universe<>pmu.universe; 
	
	SELECT
		po.id,
		po.legacy_call_letters,
		audience_id,
		SUM(po.impressions_per_market) 'impressions',
		CASE WHEN SUM(po.universe)>0 THEN 
			SUM(po.impressions_per_market) / SUM(po.universe)
		ELSE 
			NULL 
		END 'rating'
	FROM
		#pre_output po
	GROUP BY 
		po.id,
		po.legacy_call_letters,
		audience_id
	ORDER BY 
		po.id;

	--IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	--IF OBJECT_ID('tempdb..#min_playback_types') IS NOT NULL DROP TABLE #min_playback_types;
	--IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	--IF OBJECT_ID('tempdb..#viewers') IS NOT NULL DROP TABLE #viewers;
	--IF OBJECT_ID('tempdb..#pre_output') IS NOT NULL DROP TABLE #pre_output;
	--IF OBJECT_ID('tempdb..#primary_market_universes') IS NOT NULL DROP TABLE #primary_market_universes;
	--IF OBJECT_ID('tempdb..#times') IS NOT NULL DROP TABLE #times;
END
GO


GO
/*
	DECLARE
		@hut_media_month_id SMALLINT=434,
		@share_media_month_id SMALLINT=442,
		@demo VARCHAR(MAX)='6,7,347,348,13,14,15,21,22,28,29,30,284,290', --A18+
		@ratings_request RatingsInputWithId,
		@min_playback_type VARCHAR(1)='3'

	INSERT INTO @ratings_request SELECT 1,'WPIX',1,1,1,1,1,0,0,39600,43200-1--'M-F 11a-12n JERRY SPRINGER - 93',
	INSERT INTO @ratings_request SELECT 2,'WPIX',0,0,0,0,0,0,1,28800,32400-1--'Su 8a-9a PAID - 5',
	INSERT INTO @ratings_request SELECT 3,'WPHL',0,0,0,1,0,0,0,10800,14400-1--'Th 3a-4a CRME WATCH DLY - 14',
	INSERT INTO @ratings_request SELECT 4,'WPHL',1,1,1,1,1,0,0,68400,70200-1--'M-F 7p-7:30p big bang - 65',
	INSERT INTO @ratings_request SELECT 5,'KRON',0,0,0,0,0,0,1,75600,79200-1--'Su 9p-10p KRON 4-NGHT LV - 21',
	INSERT INTO @ratings_request SELECT 6,'KTLA',0,0,0,0,0,1,0,61200,63000-1--'Sa 5p-5:30p DOG WHISPER-CW - 18',
	INSERT INTO @ratings_request SELECT 7,'KTXL',1,1,1,1,0,0,0,84600,86400-1--'M-Th 11:30p-12m SEINFELD B< - 6',
	INSERT INTO @ratings_request SELECT 8,'WJW',1,1,1,0,0,0,0,82800,84600-1--'M-W 11p-11:30p BG BNG THRY B< - 55',
	INSERT INTO @ratings_request SELECT 9,'KASW',1,1,1,1,1,1,1,64800,68400-1--'M-Su 6p-7p VARIOUS	-21',
	INSERT INTO @ratings_request SELECT 10,'WKCF',0,0,0,0,0,1,1,68400,70200-1--'Sa-Su 7p-7:30p 2BRK GIRLS WK - 11',
	INSERT INTO @ratings_request SELECT 12,'KHOU',0,0,0,0,0,0,1,1800,5400-1
	INSERT INTO @ratings_request SELECT 13,'KHOU',0,0,0,0,0,0,1,5400,9000-1
	INSERT INTO @ratings_request SELECT 18,'WBNX',0,0,0,0,0,1,0,70200,72000-1
	INSERT INTO @ratings_request SELECT 19,'WBNX',0,0,0,0,0,1,0,82800,86400-1
	INSERT INTO @ratings_request SELECT 20,'KHOU',1,1,1,1,1,0,0,45000,46800-1
	INSERT INTO @ratings_request SELECT 21,'KHOU',1,1,1,1,1,0,0,66480,67200-1
	INSERT INTO @ratings_request SELECT 22,'KHOU',1,1,1,1,1,0,0,39600-2*60*60,43200-1
	INSERT INTO @ratings_request SELECT 23,'KHOU',1,1,1,1,1,0,0,39600,43200-1
	INSERT INTO @ratings_request SELECT 24,'WKCF',0,0,0,0,0,0,1,3600,7200-1
	INSERT INTO @ratings_request SELECT 25,'WBNX',1,0,0,0,0,0,0,3600,10800-1

	EXEC [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages] @hut_media_month_id, @share_media_month_id, @demo, @ratings_request, @min_playback_type
*/
ALTER PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_TwoBooks_Averages]
	@hut_media_month_id SMALLINT,
	@share_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInputWithId READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN
	DECLARE @hut_media_month_id_sniff SMALLINT=@hut_media_month_id;
	DECLARE @share_media_month_id_sniff SMALLINT=@share_media_month_id;
	DECLARE @demo_sniff VARCHAR(MAX)=@demo;
	DECLARE @min_playback_type_sniff VARCHAR(1)=@min_playback_type;

	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	CREATE TABLE #rating_requests (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, mon BIT NOT NULL, tue BIT NOT NULL, wed BIT NOT NULL, thu BIT NOT NULL, fri BIT NOT NULL, sat BIT NOT NULL, sun BIT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL,
		PRIMARY KEY CLUSTERED (id, legacy_call_letters, mon, tue, wed, thu, fri, sat, sun, start_time, end_time));
	INSERT into #rating_requests
		SELECT * FROM @ratings_request;

	CREATE TABLE #audience_ids (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id));
	INSERT INTO #audience_ids
		SELECT DISTINCT id FROM dbo.SplitIntegers(@demo_sniff);

	CREATE TABLE #times (id INT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, 
		PRIMARY KEY CLUSTERED (id,start_time,end_time));

	INSERT INTO #times
		SELECT
			rr.id,
			qh.start_time,
			qh.end_time
		FROM
			#rating_requests rr
			JOIN nsi.quarter_hours qh ON (qh.start_time<=rr.end_time AND qh.end_time>=rr.start_time);

	CREATE TABLE #hut_market_codes (id INT NOT NULL, market_code SMALLINT NOT NULL, playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(id, market_code, playback_type));
	INSERT INTO #hut_market_codes
		SELECT DISTINCT
			rr.id,
			v.market_code,
			mpt.available_playback_type
		FROM
			#rating_requests rr
			JOIN #times t ON t.id=rr.id
			JOIN nsi.viewers v ON v.media_month_id=@hut_media_month_id_sniff
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND v.start_time=t.start_time
				AND v.end_time=t.end_time
			JOIN (
				SELECT * FROM nsi.udf_GetMinPlaybackTypes(@hut_media_month_id_sniff,@min_playback_type_sniff)
			) mpt ON mpt.market_code=v.market_code;

	CREATE TABLE #share_market_codes (id INT NOT NULL, market_code SMALLINT NOT NULL, playback_type VARCHAR(1) NOT NULL, 
		PRIMARY KEY CLUSTERED(id, market_code, playback_type));
	INSERT INTO #share_market_codes
		SELECT DISTINCT
			rr.id,
			v.market_code,
			mpt.available_playback_type
		FROM
			#rating_requests rr
			JOIN #times t ON t.id=rr.id
			JOIN nsi.viewers v ON v.media_month_id=@share_media_month_id_sniff
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND v.start_time=t.start_time
				AND v.end_time=t.end_time
			JOIN (
				SELECT * FROM nsi.udf_GetMinPlaybackTypes(@share_media_month_id_sniff,@min_playback_type_sniff)
			) mpt on mpt.market_code=v.market_code;

	CREATE TABLE #hut_usage_days (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, playback_type VARCHAR(1) NOT NULL, weekly_avg FLOAT NOT NULL --,weekday_usage FLOAT NOT NULL ,weekend_usage FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time, market_code, audience_id));
	INSERT INTO #hut_usage_days
		SELECT
			rr.id,
			rr.legacy_call_letters,
			u.start_time,
			u.end_time,
			hmc.market_code,
			a.audience_id,
			hmc.playback_type,
			-- calc average FROM weekday values
			(
				CASE rr.mon WHEN 1 THEN u.mon_usage ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN u.tue_usage ELSE 0  END +
				CASE rr.wed WHEN 1 THEN u.wed_usage ELSE 0 END +
				CASE rr.thu WHEN 1 THEN u.thu_usage ELSE 0 END +
				CASE rr.fri WHEN 1 THEN u.fri_usage ELSE 0 END +
				CASE rr.sat WHEN 1 THEN u.sat_usage ELSE 0 END +
				CASE rr.sun WHEN 1 THEN u.sun_usage ELSE 0 END 
			) 
			/ 
			(
				CASE rr.mon WHEN 1 THEN 1 ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.wed WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.thu WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.fri WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sat WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sun WHEN 1 THEN 1 ELSE 0 END 
			) weekly_avg
		FROM
			#rating_requests rr
			JOIN #times t ON t.id=rr.id
			JOIN #hut_market_codes hmc ON hmc.id=rr.id
			CROSS APPLY #audience_ids a
			JOIN nsi.usages u ON u.media_month_id=@hut_media_month_id_sniff
				AND u.playback_type=hmc.playback_type
				AND u.audience_id=a.audience_id
				AND u.start_time=t.start_time
				AND u.end_time=t.end_time
				AND u.market_code=hmc.market_code 
                
	CREATE TABLE #hut_univ (market_code SMALLINT NOT NULL, audience_id INT NOT NULL, playback_type VARCHAR(1) NOT NULL, universe FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(market_code,audience_id,playback_type));
	INSERT INTO #hut_univ
		SELECT DISTINCT
			u.market_code,
			u.audience_id,
			u.playback_type,
			u.universe as universe
		FROM 
			nsi.universes u
			JOIN #hut_market_codes hmc ON hmc.market_code=u.market_code
				AND u.playback_type=hmc.playback_type
			JOIN #audience_ids a on a.audience_id=u.audience_id 
		WHERE 
			u.media_month_id=@hut_media_month_id_sniff
		GROUP BY 
			u.market_code,u.audience_id,u.playback_type,u.universe;

	CREATE TABLE #hut (id INT NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, HUT FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(id,market_code,audience_id));
	INSERT INTO #hut
		SELECT 
			USAGE.id,
			univ.market_code, 
			univ.audience_id, 
			weekly_avg/universe HUT
		FROM (
			SELECT 
				d.id,
				d.legacy_call_letters, 
				d.market_code, 
				d.audience_id,
				AVG(weekly_avg) weekly_avg
			FROM 
				#hut_usage_days d
			GROUP BY 
				d.id, d.legacy_call_letters, d.market_code, d.audience_id
		) USAGE
		JOIN #hut_univ UNIV ON UNIV.audience_id=USAGE.audience_id 
			AND UNIV.market_code=USAGE.market_code;

	CREATE TABLE #share_usage_days (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, playback_type VARCHAR(1) NOT NULL, weekly_avg FLOAT NOT NULL --,weekday_usage FLOAT NOT NULL ,weekend_usage FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time, market_code, audience_id));
	INSERT INTO #share_usage_days
		SELECT
			rr.id,
			rr.legacy_call_letters,
			u.start_time,
			u.end_time,
			smc.market_code,
			a.audience_id,
			smc.playback_type,
			-- calc average FROM weekday values
			(
				CASE rr.mon WHEN 1 THEN u.mon_usage ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN u.tue_usage ELSE 0 END +
				CASE rr.wed WHEN 1 THEN u.wed_usage ELSE 0 END +
				CASE rr.thu WHEN 1 THEN u.thu_usage ELSE 0 END +
				CASE rr.fri WHEN 1 THEN u.fri_usage ELSE 0 END +
				CASE rr.sat WHEN 1 THEN u.sat_usage ELSE 0 END +
				CASE rr.sun WHEN 1 THEN u.sun_usage ELSE 0 END 
			) 
			/ 
			(
				CASE rr.mon WHEN 1 THEN 1 ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.wed WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.thu WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.fri WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sat WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sun WHEN 1 THEN 1 ELSE 0 END 
			) weekly_avg
		FROM
			#rating_requests rr
			JOIN #times t ON t.id=rr.id
			JOIN #share_market_codes smc ON smc.id=rr.id
			CROSS APPLY #audience_ids a
			JOIN nsi.usages u ON u.media_month_id=@share_media_month_id_sniff
				AND u.playback_type=smc.playback_type
				AND u.audience_id=a.audience_id
				AND u.start_time=t.start_time
				AND u.end_time=t.end_time
				AND u.market_code=smc.market_code ;
               
	CREATE TABLE #share_viewer_days (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, weekly_avg FLOAT NOT NULL --,weekday_usage FLOAT NOT NULL ,weekend_usage FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, start_time, end_time, market_code, audience_id));
	INSERT INTO #share_viewer_days
		SELECT
			rr.id,
			rr.legacy_call_letters,
			v.start_time,
			v.end_time,
			mc.market_code,
			a.audience_id,
			-- calc average FROM weekday values
			(
				CASE rr.mon WHEN 1 THEN vd.mon_viewers ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN vd.tue_viewers ELSE 0 END +
				CASE rr.wed WHEN 1 THEN vd.wed_viewers ELSE 0 END +
				CASE rr.thu WHEN 1 THEN vd.thu_viewers ELSE 0 END +
				CASE rr.fri WHEN 1 THEN vd.fri_viewers ELSE 0 END +
				CASE rr.sat WHEN 1 THEN vd.sat_viewers ELSE 0 END +
				CASE rr.sun WHEN 1 THEN vd.sun_viewers ELSE 0 END 
			) 
			/ 
			(
				CASE rr.mon WHEN 1 THEN 1 ELSE 0 END + 
				CASE rr.tue WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.wed WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.thu WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.fri WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sat WHEN 1 THEN 1 ELSE 0 END +
				CASE rr.sun WHEN 1 THEN 1 ELSE 0 END 
			) weekly_avg
		FROM
		#rating_requests rr
		JOIN #times t ON t.id=rr.id
		JOIN #share_market_codes mc ON mc.id=rr.id
		JOIN nsi.viewers v ON v.media_month_id=@share_media_month_id_sniff
			AND v.legacy_call_letters=rr.legacy_call_letters
			AND v.market_code=mc.market_code
			AND v.start_time=t.start_time
			AND v.end_time=t.end_time
		CROSS APPLY #audience_ids a
		JOIN nsi.viewer_details vd ON vd.media_month_id=v.media_month_id
			AND vd.viewer_id=v.id
			AND vd.audience_id=a.audience_id
			AND vd.playback_type=mc.playback_type;

	CREATE TABLE #share (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, SHARE FLOAT NOT NULL
		PRIMARY KEY CLUSTERED(id,legacy_call_letters,market_code, audience_id));
	INSERT INTO #share
		SELECT 
			v.id,
			v.legacy_call_letters, 
			v.market_code,
			v.audience_id,
			case when AVG(u.weekly_avg) = 0 then 0 else AVG(v.weekly_avg) / AVG(u.weekly_avg) END as SHARE
			--AVG(v.weekly_avg) / AVG(u.weekly_avg) SHARE
		FROM 
			#share_viewer_days v
			JOIN #share_usage_days u ON v.id = u.id
				AND v.audience_id=u.audience_id
				AND v.start_time=u.start_time
				AND v.end_time=u.end_time
				AND v.market_code=u.market_code
				AND v.legacy_call_letters=u.legacy_call_letters
		GROUP BY 
			v.id,v.legacy_call_letters, v.market_code,v.audience_id;

	CREATE TABLE #pre_output (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, market_code SMALLINT NOT NULL, audience_id INT NOT NULL, impressions_per_market FLOAT NOT NULL, universe FLOAT NOT NULL,
		PRIMARY KEY CLUSTERED(id,legacy_call_letters,market_code,audience_id));
	INSERT INTO #pre_output
		SELECT 
			s.id,
			s.legacy_call_letters,
			s.market_code,
			s.audience_id,
			SUM(h.hut) * SUM(s.share) * SUM(u.universe) 'impressions_per_market',
			SUM(u.universe) 'universes'
		FROM 
			#share s
			JOIN #hut h ON s.audience_id=h.audience_id
				AND s.id=h.id
				AND s.market_code=h.market_code
			JOIN #hut_univ u ON u.audience_id=h.audience_id 
				AND u.market_code=h.market_code 
		GROUP BY 
			s.id,s.legacy_call_letters,s.market_code,s.audience_id;

	CREATE TABLE #primary_market_universes (id INT NOT NULL, legacy_call_letters VARCHAR(15) NOT NULL, audience_id INT NOT NULL, universe FLOAT NOT NULL,
		PRIMARY KEY CLUSTERED(id,legacy_call_letters,audience_id));
	INSERT INTO #primary_market_universes
		SELECT
			po.id,
			po.legacy_call_letters,
			po.audience_id,
			MAX(po.universe)
		FROM 
			#pre_output po
		GROUP BY 
			po.id,po.legacy_call_letters,po.audience_id;

	-- zero out universe of bleed market      
	UPDATE
		#pre_output
	SET
		universe=0
	FROM
		#pre_output po
		JOIN #primary_market_universes pmu ON pmu.id=po.id
			AND pmu.legacy_call_letters=po.legacy_call_letters
			AND pmu.audience_id=po.audience_id
	WHERE
		po.universe<>pmu.universe; 

	SELECT
		po.id,
		po.legacy_call_letters,
		SUM(po.impressions_per_market) 'impressions',
		CASE WHEN SUM(po.universe)>0 THEN 
			SUM(po.impressions_per_market) / SUM(po.universe)
		ELSE 
			NULL 
		END 'rating'
	FROM
		#pre_output po
	GROUP BY 
		po.id,
		po.legacy_call_letters
	ORDER BY 
		po.id;

	--IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	--IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	--IF OBJECT_ID('tempdb..#hut_univ') IS NOT NULL DROP TABLE #hut_univ;                
	--IF OBJECT_ID('tempdb..#hut_market_codes') IS NOT NULL DROP TABLE #hut_market_codes;
	--IF OBJECT_ID('tempdb..#share_market_codes') IS NOT NULL DROP TABLE #share_market_codes;                
	--IF OBJECT_ID('tempdb..#hut_usage_days') IS NOT NULL DROP TABLE #hut_usage_days;
	--IF OBJECT_ID('tempdb..#share_usage_days') IS NOT NULL DROP TABLE #share_usage_days;                
	--IF OBJECT_ID('tempdb..#share_viewer_days') IS NOT NULL DROP TABLE #share_viewer_days;
	--IF OBJECT_ID('tempdb..#hut') IS NOT NULL DROP TABLE #hut;
	--IF OBJECT_ID('tempdb..#share') IS NOT NULL DROP TABLE #share;
	--IF OBJECT_ID('tempdb..#pre_output') IS NOT NULL DROP TABLE #pre_output;
	--IF OBJECT_ID('tempdb..#primary_market_universes') IS NOT NULL DROP TABLE #primary_market_universes;
	--IF OBJECT_ID('tempdb..#times') IS NOT NULL DROP TABLE #times;
END
GO


GO
ALTER PROCEDURE [nsi].[usp_GetImpressionsForMultiplePrograms_Daypart_Averages]
	@posting_media_month_id SMALLINT,
	@demo VARCHAR(MAX),
	@ratings_request RatingsInputWithId READONLY,
	@min_playback_type VARCHAR(1)
AS
BEGIN
	DECLARE @posting_media_month_id_sniff SMALLINT=@posting_media_month_id;
	DECLARE @min_playback_type_sniff VARCHAR(1)=@min_playback_type;
	DECLARE @demo_sniff VARCHAR(MAX)=@demo;

	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	CREATE TABLE #rating_requests (id int NOT NULL,	legacy_call_letters varchar(15) NOT NULL, mon bit NOT NULL, tue bit NOT NULL, wed bit NOT NULL, thu bit NOT NULL, fri bit NOT NULL, sat bit NOT NULL, sun bit NOT NULL, start_time int NOT NULL, end_time int NOT NULL,
		PRIMARY KEY CLUSTERED (id,legacy_call_letters,mon,tue,wed,thu,fri,sat,sun,start_time,end_time));
	INSERT into #rating_requests
		SELECT * FROM @ratings_request;

	CREATE TABLE #audience_ids (audience_id INT NOT NULL, 
		PRIMARY KEY CLUSTERED(audience_id));
	INSERT INTO #audience_ids
		SELECT id FROM dbo.SplitIntegers(@demo_sniff);

	CREATE TABLE #min_playback_types (market_code SMALLINT NOT NULL, available_playback_type VARCHAR(1), 
		PRIMARY KEY CLUSTERED(market_code));
	INSERT INTO #min_playback_types
		SELECT * FROM nsi.udf_GetMinPlaybackTypes(@posting_media_month_id_sniff,@min_playback_type_sniff);

	CREATE TABLE #times (id INT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, 
		PRIMARY KEY CLUSTERED (id,start_time,end_time));
	INSERT INTO #times
		SELECT
			rr.id,
			qh.start_time,
			qh.end_time
		FROM
			#rating_requests rr
			JOIN nsi.quarter_hours qh ON (qh.start_time<=rr.end_time AND qh.end_time>=rr.start_time)

	CREATE TABLE #viewers (id INT NOT NULL, legacy_call_letters varchar(15) NOT NULL, audience_id INT NOT NULL, market_code SMALLINT NOT NULL, start_time INT NOT NULL, end_time INT NOT NULL, viewers FLOAT  
		PRIMARY KEY CLUSTERED(id, legacy_call_letters, audience_id, market_code, start_time, end_time));
	INSERT INTO #viewers
		SELECT
			rr.id, 
			rr.legacy_call_letters,
			a.audience_id,
			v.market_code,
			v.start_time, 
			v.end_time,
			-- basically if weekday and weekend then avg the two, otherwise return one or other
			CASE WHEN rr.mon=1 OR rr.tue=1 OR rr.wed=1 OR rr.thu=1 OR rr.fri=1 THEN 
				CASE WHEN rr.sat=1 OR rr.sun=1 THEN (vd.weekend_viewers  + vd.weekday_viewers) / 2 
					ELSE vd.weekday_viewers  
				END  			
			ELSE 
				CASE WHEN rr.sat=1 OR rr.sun=1 THEN vd.weekend_viewers ELSE 0 END 
			END 
		FROM
			#rating_requests rr
			JOIN nsi.uvw_market_codes_call_letters mccl ON mccl.media_month_id=@posting_media_month_id
				AND mccl.legacy_call_letters=rr.legacy_call_letters
			JOIN #times t ON t.id=rr.id
			JOIN nsi.viewers v ON v.media_month_id=@posting_media_month_id
				AND v.legacy_call_letters=rr.legacy_call_letters
				AND v.market_code=mccl.market_code
				AND v.start_time=t.start_time
				AND v.end_time=t.end_time
			JOIN #min_playback_types mpt ON mpt.market_code=v.market_code
			CROSS APPLY #audience_ids a
			JOIN nsi.viewer_details vd ON vd.media_month_id=v.media_month_id
				AND vd.viewer_id=v.id
				AND vd.audience_id=a.audience_id
				AND vd.playback_type=mpt.available_playback_type OPTION ( OPTIMIZE FOR (@posting_media_month_id UNKNOWN) );

	SELECT 
		market_averages.id,
		legacy_call_letters,
		audience_id, 
		SUM(market_impression_avg) 'impressions' -- sum of market averages by call letter, etc (aggregating out market_code dimension)
	FROM 
	(
		-- get averages of impressions for each market (aggregating out start_time and end_time dimensions)
		SELECT 
			id,
			legacy_call_letters,
			market_code, 
			audience_id, 
			AVG(viewers) 'market_impression_avg' 
		FROM 
			#viewers
		GROUP BY 
			id,legacy_call_letters,market_code,audience_id
	) AS market_averages
	GROUP BY 
		id,legacy_call_letters,audience_id;

	--IF OBJECT_ID('tempdb..#rating_requests') IS NOT NULL DROP TABLE #rating_requests;
	--IF OBJECT_ID('tempdb..#min_playback_types') IS NOT NULL DROP TABLE #min_playback_types;
	--IF OBJECT_ID('tempdb..#audience_ids') IS NOT NULL DROP TABLE #audience_ids;
	--IF OBJECT_ID('tempdb..#viewers') IS NOT NULL DROP TABLE #viewers;
	--IF OBJECT_ID('tempdb..#times') IS NOT NULL DROP TABLE #times;
END
GO

/*************************************** END PRI-5085 *****************************************************/





/*************************************** END UPDATE SCRIPT *******************************************************/

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '19.04.1' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN 
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '19.03.1' -- Previous release version
		OR [version] = '19.04.1') -- Current release version
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